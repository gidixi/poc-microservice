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

