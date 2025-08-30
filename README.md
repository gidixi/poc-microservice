# poc-microservice

This repository contains a proof-of-concept microservice system. The solution file is `poc-micro.sln` and the source code follows the structured layout under `src`, `tests`, `deploy`, `scripts`, `config`, and versioned protobuf contracts in `contracts`.

## Docker Compose

The provided `docker-compose.yml` includes a [gRPC-Web proxy](https://github.com/grpc/grpc-web)
based on the `quay.io/grpcweb/grpcwebproxy:v0.15.0` image. The proxy forwards gRPC-Web
requests to the `dispatcher` service and is configured with:

```
command:
  - --backend_addr=dispatcher:8080
  - --run_tls_server=false
  - --allow_all_origins
```

This replaces the previous `improbable/grpc-web` image and environment variables.
