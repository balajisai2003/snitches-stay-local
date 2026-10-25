# Infrastructure & Deployment

This directory contains container configurations and orchestration definitions for local development and integration testing.

---

## Service Inventory

| Service | Container Name | Host Port | Internal Port | Purpose |
| --- | --- | --- | --- | --- |
| **`app`** | `organizer-app` | `5000` | `8080` | Main ASP.NET Core Web API |
| **`processing`** | `organizer-processing` | `5001` | `8080` | Processing Worker service |
| **`sqlserver`** | `organizer-sql` | `1433` | `1433` | SQL Server 2025 Developer Edition |
| **`rabbitmq`** | `organizer-rabbitmq` | `5672`, `15672` | `5672`, `15672` | RabbitMQ broker & web management UI |
| **`jaeger`** | `organizer-jaeger` | `16686`, `4317` | `16686`, `4317` | OpenTelemetry distributed tracing & UI |

---

## Shared Volumes

* **`organizer-doc-storage`** (Mounted to both `app` and `processing` at `/storage`):
  * `/storage/originals/`: Read-write for `app`, read-only for `processing`.
  * `/storage/results/`: Write-only for `processing`, read-only for `app`.
* **`organizer-sql-data`**: Durable data directory for SQL Server.
* **`organizer-rabbitmq-data`**: Durable state for RabbitMQ queues and exchanges.

---

## Quickstart

### 1. Configure Environment (Optional)
```bash
cp infra/.env.example infra/.env
```

### 2. Start Services
From the repository root:
```bash
docker compose -f infra/docker-compose.yml up -d
```

### 3. Verify Health & UIs
* **RabbitMQ UI:** [http://localhost:15672](http://localhost:15672) (User: `organizer`, Pass: `organizer_secret`)
* **Jaeger Tracing:** [http://localhost:16686](http://localhost:16686)
* **App Health:** [http://localhost:5000/health](http://localhost:5000/health)
* **Processing Health:** [http://localhost:5001/health](http://localhost:5001/health)

### 4. Stop Services
```bash
docker compose -f infra/docker-compose.yml down
```
*(To wipe persistent volumes, add `-v`)*
