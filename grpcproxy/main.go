package main

import (
    "log"
    "net/http"
    "os"

    "github.com/improbable-eng/grpc-web/go/grpcweb"
    proxy "github.com/mwitkow/grpc-proxy/proxy"
    "context"
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
        if wrapped.IsGrpcWebRequest(r) || wrapped.IsAcceptableGrpcCorsRequest(r) {
            wrapped.ServeHTTP(w, r)
            return
        }
        w.WriteHeader(http.StatusNotFound)
    })

    log.Printf("gRPC-Web proxy listening on %s forwarding to %s", listenAddr, backendAddr)
    if err := http.ListenAndServe(listenAddr, handler); err != nil {
        log.Fatalf("failed to serve: %v", err)
    }
}

