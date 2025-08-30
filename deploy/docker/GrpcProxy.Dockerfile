# Build stage
FROM golang:1.22 AS builder
WORKDIR /src
COPY grpcproxy/go.mod grpcproxy/go.sum ./
RUN go mod download
COPY grpcproxy/ ./
RUN CGO_ENABLED=0 go build -o /grpcproxy

# Runtime stage
FROM gcr.io/distroless/base-debian11
COPY --from=builder /grpcproxy /grpcproxy
ENTRYPOINT ["/grpcproxy"]
