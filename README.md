# poc-microservice

A proof-of-concept (POC) microservice platform that demonstrates how to orchestrate an order-style workflow end-to-end using gRPC, background workers and a Vue 3 frontend. The solution file is `poc-micro.sln` and the codebase is organised across `src`, `tests`, `deploy`, `scripts`, `config`, and versioned protobuf contracts in `contracts`.

The system can be repurposed for real commercial, logistical or administrative processes by customising the domain model and integrating external systems. This README walks through the architecture, execution flow and possible adaptations.

## High-level architecture

```
Vue Frontend ── gRPC-Web ──► grpcproxy ──► Dispatcher Service
                                            │
                                            ├── Pricing Service
                                            ├── Inventory Service
                                            ├── Data Service
                                            └── Logger Service
```

* **Frontend** – A Vue 3 single page application that lets users submit orders and monitor their lifecycle in real time.
* **grpcproxy** – A Go-based proxy that translates gRPC-Web calls into vanilla gRPC requests, enabling browser clients without regenerating protobuf stubs.
* **Dispatcher** – The orchestrator. It receives incoming jobs, executes the domain workflow and streams status updates back to the client.
* **Worker services** – Each step of the flow is handled by a microservice: pricing, inventory reservation, data persistence and logging.
* **Contracts** – Shared `.proto` definitions versioned under `contracts` to keep service interfaces consistent across clients.

## Workflow lifecycle

1. **Submit** – The frontend issues a `CreateOrder` request via gRPC-Web to the dispatcher.
2. **Price** – Dispatcher forwards the payload to the Pricing service, which calculates subtotals, taxes and totals.
3. **Reserve stock** – Inventory service validates availability, reserves items or emits backorder status.
4. **Persist** – Data service stores the order, using the `IOrdersAppService` abstraction so you can plug in SQL/NoSQL databases, message queues or downstream APIs.
5. **Audit** – Logger service captures structured events for observability and compliance.
6. **Stream status** – The dispatcher emits state transitions (e.g. pending → priced → reserved → persisted) back to the client so operators can follow progress live.

Each step is isolated, allowing you to change implementations or scaling characteristics without affecting the others.

## Running the stack

### Docker Compose

Use the root-level `docker-compose.yml` to build and run the frontend and gRPC proxy:

```bash
docker-compose up --build
```

After the images are up, visit <http://localhost:8081> to access the Vue application. The gRPC-Web proxy listens on port `8080` and forwards requests to the dispatcher running in your environment.

### Local frontend development

```bash
cd frontend
npm install
npm run dev
```

When developing locally, ensure a gRPC-Web proxy (for example [`grpcwebproxy`](https://github.com/improbable-eng/grpc-web/tree/master/go/grpcwebproxy)) forwards requests to the dispatcher on port `8080`.

## Extending for real-world scenarios

The POC is intentionally modular so you can adapt it beyond the sample order domain.

### Commercial processes

* Model quotes, subscriptions or service requests by adjusting the `Order` schema in the protobuf contracts.
* Connect the Pricing service to ERP price lists, promotion engines or CPQ logic.
* Push confirmations to CRM or billing systems after the Data service persists the payload.

### Logistical processes

* Expand the Inventory service to coordinate multiple warehouses or 3PL providers, exposing events for shipment preparation.
* Use the streaming job states to inform operations dashboards, enabling staff to track reservations, backorders and fulfillment stages in real time.

### Administrative workflows

* Reinterpret the entities as onboarding tasks, support tickets or internal approval requests.
* Integrate the Logger service with ELK/Grafana Loki to maintain an audit trail that satisfies compliance requirements.
* Trigger human-in-the-loop steps by publishing events from the Dispatcher when certain states are reached.

## Customisation checklist

1. **Update protobuf contracts** to reflect your domain entities and regenerate client/server stubs where needed.
2. **Swap service implementations** (e.g. PricingClient, InventoryClient, DataClient) to communicate with existing systems.
3. **Enhance persistence** by implementing `IOrdersAppService` against your storage technology of choice.
4. **Harden observability** by exporting logs and metrics to your monitoring stack.
5. **Secure the perimeter** with authentication/authorisation in the proxy and TLS for service-to-service communication.

## Repository structure

Key directories and files:

| Path | Description |
| --- | --- |
| `src/` | .NET microservices and supporting libraries. |
| `frontend/` | Vue 3 client application using gRPC-Web. |
| `grpcproxy/` | Go-based proxy translating gRPC-Web to native gRPC. |
| `contracts/` | Versioned protobuf definitions shared across services. |
| `tests/` | Automated tests for backend services. |
| `deploy/` | Infrastructure manifests and deployment helpers. |
| `scripts/` | Utility scripts for local development and CI. |
| `INFRASTRUCTURE.md` | Guidance on infrastructure decisions and setup. |

By tailoring these components you can evolve the POC into a production-ready platform that orchestrates complex business workflows while retaining clear service boundaries and modern communication patterns.
