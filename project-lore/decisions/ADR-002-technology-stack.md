# ADR-002: Core Technology Stack Selection

## Context

The system must run reliably on a local developer workstation using Docker Compose, support full-text search, support vector similarity calculations, provide robust typing, and offer rich documentation and mature community libraries.

## Decision

We select the following core stack:
- **Backend:** C# / .NET 10 LTS (ASP.NET Core Web API + Worker Service).
- **Data Persistence:** Microsoft SQL Server 2025 in Docker, managed via Entity Framework Core 10.
- **Frontend:** React + TypeScript using Vite.
- **Message Broker:** RabbitMQ 3.13+ in Docker.
- **Local AI:** Ollama running locally.
- **Observability:** OpenTelemetry with Jaeger.
- **Test Automation:** xUnit, Testcontainers for .NET, NetArchTest.

## Rationale

1. **C# / .NET 10:** Delivers top-tier raw throughput, rock-solid type safety, built-in dependency injection, native health checks, and cross-platform Docker support.
2. **SQL Server 2025:** Provides relational storage, ACID transactions, native Full-Text Catalog search, and native `vector(dim)` storage with `VECTOR_DISTANCE` functions in a single database engine. This eliminates the operational headache of running PostgreSQL + pgvector + Elasticsearch simultaneously.
3. **React + TypeScript (Vite):** Provides instant hot module replacement (HMR), a rich ecosystem for document viewer controls, and strict type safety aligned with backend DTOs.
4. **RabbitMQ:** The industry reference for AMQP messaging, providing explicit queue declarations, publisher confirms, consumer acknowledgments, and dead-letter routing.
5. **Testcontainers:** Enables running integration tests against genuine SQL Server and RabbitMQ Docker containers rather than fragile in-memory mocks.

## Consequences

- **Positive:** Uniform language on the backend, single database engine for relational/text/vector data, high testing realism.
- **Negative / Trade-off:** SQL Server Linux container has higher memory requirements (~2 GB RAM minimum) compared to SQLite or lightweight engines.

