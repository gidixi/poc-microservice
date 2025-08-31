package main

import (
	"context"
	"log"
	"net/http"
	"os"
	"time"

	"github.com/improbable-eng/grpc-web/go/grpcweb"
	proxy "github.com/mwitkow/grpc-proxy/proxy"
	"google.golang.org/grpc"
	"google.golang.org/grpc/metadata"
)

func getenv(key, fallback string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return fallback
}

func main() {
	backendAddr := getenv("BACKEND_ADDR", "dispatcher:8080")
	listenAddr := getenv("GRPCWEB_ADDRESS", "0.0.0.0:8080")

	backendConn, err := grpc.Dial(backendAddr, grpc.WithInsecure())
	if err != nil {
		log.Fatalf("failed to dial backend %s: %v", backendAddr, err)
	}

	director := func(ctx context.Context, fullMethodName string) (context.Context, grpc.ClientConnInterface, error) {
		log.Printf("→ proxy: forwarding %s", fullMethodName)

		md, _ := metadata.FromIncomingContext(ctx)
		mdCopy := md.Copy()
		delete(mdCopy, "user-agent")
		delete(mdCopy, "connection")
		outCtx := metadata.NewOutgoingContext(ctx, mdCopy)
		return outCtx, backendConn, nil
	}

	grpcServer := grpc.NewServer(
		grpc.CustomCodec(proxy.Codec()),
		grpc.UnknownServiceHandler(proxy.TransparentHandler(director)),
	)

	wrapped := grpcweb.WrapServer(grpcServer,
		grpcweb.WithCorsForRegisteredEndpointsOnly(false),
		grpcweb.WithOriginFunc(func(origin string) bool { return true }),
	)

	handler := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()

		log.Printf("→ http: %s %s from %s", r.Method, r.URL.Path, r.RemoteAddr)

		if wrapped.IsGrpcWebRequest(r) || wrapped.IsAcceptableGrpcCorsRequest(r) {
			wrapped.ServeHTTP(w, r)
			log.Printf("← http: %s %s finished in %v (grpc-web)", r.Method, r.URL.Path, time.Since(start))
			return
		}

		log.Printf("← http: %s %s 404 Not Found in %v", r.Method, r.URL.Path, time.Since(start))
		w.WriteHeader(http.StatusNotFound)
	})

	log.Printf("gRPC-Web proxy listening on %s forwarding to %s", listenAddr, backendAddr)
	if err := http.ListenAndServe(listenAddr, handler); err != nil {
		log.Fatalf("failed to serve: %v", err)
	}
}