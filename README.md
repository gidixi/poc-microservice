# poc-microservice

This repository contains a proof-of-concept microservice system. The solution file is `poc-micro.sln` and the source code follows the structured layout under `src`, `tests`, `deploy`, `scripts`, `config`, and versioned protobuf contracts in `contracts`.

## Frontend

A minimal Vue 3 client lives under `frontend`. It uses gRPC-Web to submit orders to the dispatcher via a proxy on `localhost:8080` and relies on `protobufjs` to load the protobuf contracts at runtime.

### Run the frontend

```bash
cd frontend
npm install
npm run dev
```

Ensure the microservices are running via `docker-compose` and that a gRPC-Web proxy (e.g. `grpcwebproxy`) is forwarding requests to the dispatcher on port 8080.
