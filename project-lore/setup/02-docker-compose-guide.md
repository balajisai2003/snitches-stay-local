# Setup: Docker Compose Architecture and Configuration

## Container Topology

The entire local development and production environment is managed via a single `docker-compose.yml` located in the `infra/` directory.

```mermaid
graph TD
    subgraph DockerBridge["Bridge Network: organizer-net"]
        App["organizer-app (Port 5050)<br/>Web API Host"]
        Processing["organizer-processing (Port 5051)<br/>Worker Daemon"]
        Postgres["organizer-postgres (Port 5432)<br/>PostgreSQL 16 + pgvector"]
        Rabbit["organizer-rabbitmq (Ports 5672, 15672)<br/>AMQP Broker & UI"]
        Jaeger["organizer-jaeger (Port 16686)<br/>Distributed Tracing"]
    end

    SharedVol[("Shared Volume: doc-storage<br/>/storage/originals<br/>/storage/results")]
    PostgresVol[("Postgres Data Volume: postgres-data")]

    App -->|Mounts| SharedVol
    Processing -->|Mounts| SharedVol
    Postgres -->|Mounts| PostgresVol
```

---

## Service Port Assignments

| Service Name | Container Name | Internal Port | Host Port | Purpose |
| --- | --- | --- | --- | --- |
| **`app`** | `organizer-app` | `8080` | `5050` | Main Web API & frontend assets |
| **`processing`** | `organizer-processing` | `8080` | `5051` | Worker service health check & jobs endpoint |
| **`postgres`** | `organizer-postgres` | `5432` | `5432` | PostgreSQL 16 with pgvector extension |
| **`rabbitmq`** | `organizer-rabbitmq` | `5672` | `5672` | AMQP messaging protocol |
| **`rabbitmq-ui`**| *(Same container)* | `15672` | `15672` | RabbitMQ web management console |
| **`jaeger`** | `organizer-jaeger` | `16686` | `16686` | OpenTelemetry distributed trace viewer |

---

## Sample Compose Template (`infra/docker-compose.yml`)

```yaml
version: '3.8'

networks:
  organizer-net:
    driver: bridge

volumes:
  doc-storage:
  postgres-data:

services:
  postgres:
    image: pgvector/pgvector:pg16
    container_name: organizer-postgres
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
      - POSTGRES_DB=organizer
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - organizer-net
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d organizer"]
      interval: 5s
      timeout: 5s
      retries: 5
      start_period: 5s

  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: organizer-rabbitmq
    environment:
      - RABBITMQ_DEFAULT_USER=organizer
      - RABBITMQ_DEFAULT_PASS=organizer_secret
    ports:
      - "5672:5672"
      - "15672:15672"
    networks:
      - organizer-net
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: organizer-jaeger
    ports:
      - "16686:16686"
      - "4317:4317" # OTLP gRPC
      - "4318:4318" # OTLP HTTP
    networks:
      - organizer-net

  app:
    build:
      context: ..
      dockerfile: infra/Dockerfile.app
    container_name: organizer-app
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    ports:
      - "5050:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=organizer;Username=postgres;Password=postgres;
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=organizer
      - RabbitMQ__Password=organizer_secret
      - Storage__RootPath=/storage
      - OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317
    volumes:
      - doc-storage:/storage
    networks:
      - organizer-net

  processing:
    build:
      context: ..
      dockerfile: infra/Dockerfile.processing
    container_name: organizer-processing
    depends_on:
      rabbitmq:
        condition: service_healthy
    ports:
      - "5051:8080"
    environment:
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=organizer
      - RabbitMQ__Password=organizer_secret
      - Storage__RootPath=/storage
      - OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317
    volumes:
      - doc-storage:/storage
    networks:
      - organizer-net
```

