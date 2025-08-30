# poc-microservice

This repository contains a proof-of-concept microservice system. The solution file is `poc-micro.sln` and the source code follows the structured layout under `src`, `tests`, `deploy`, `scripts`, `config`, and versioned protobuf contracts in `contracts`.


## Frontend

A minimal Vue 3 client lives under `frontend`. It uses gRPC-Web to submit orders to the dispatcher through a proxy.

### Run with Docker Compose

The provided `docker-compose.yml` builds the frontend and a gRPC-Web proxy. After running:

```bash
docker-compose up --build
```

the application is available at <http://localhost:8081> and the gRPC-Web proxy listens on port `8080`.

### Run locally

```bash
cd frontend
npm install
npm run dev
```

When running locally, ensure a gRPC-Web proxy (e.g. `grpcwebproxy`) forwards requests to the dispatcher on port `8080`.

## gRPC-Web proxy

The `grpcproxy` service bridges browser gRPC-Web requests to the native gRPC
dispatcher. It listens on `0.0.0.0:8080`, accepts gRPC-Web traffic and
translates it into standard gRPC calls forwarded to `dispatcher:8080`.
It uses [`github.com/improbable-eng/grpc-web/go/grpcweb`](https://github.com/improbable-eng/grpc-web)
to handle the gRPC-Web protocol and
[`github.com/mwitkow/grpc-proxy/proxy`](https://github.com/mwitkow/grpc-proxy)
to relay any method without pre-registering them. Before forwarding, the proxy
removes low-level headers such as `user-agent` and `connection`. The process is
transparent for clients: no `.proto` regeneration is required—simply point the
browser to the proxy endpoint.

