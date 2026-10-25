# Architecture: Technology Stack and Integrations

## Core Technology Selection

| Component | Selected Technology | Version / Target | Architecture Rationale |
| --- | --- | --- | --- |
| **Main App** | ASP.NET Core Web API | .NET 10 LTS (C#) | High throughput, mature dependency injection, native health checks. |
| **Processing Worker** | .NET Worker Service | .NET 10 LTS (C#) | Headless background daemon with minimal footprint. |
| **Frontend** | React + TypeScript | Vite | Fast HMR, clean component model, single-page application. |
| **Database** | PostgreSQL + pgvector | PostgreSQL 16 (Docker) | Relational storage, native `tsvector` full-text search, and `pgvector` HNSW vector embeddings in one lightweight engine. |
| **Data Access** | Entity Framework Core | 10.x (`Npgsql`) | Code-first migrations, schema management, optimistic concurrency, first-class pgvector support. |
| **Message Broker** | RabbitMQ | 3.13+ (Docker) | Industrial message queuing; official .NET client behind `IMessageBus`. |
| **File Storage** | Shared Docker Volume | Local Disk via `IFileStore` | Zero cloud storage cost, simple volume mapping between containers. |
| **Local AI Engine** | Ollama | Latest (Docker or Host) | Local execution of LLM and embedding models; zero external API calls. |
| **Telemetry** | OpenTelemetry + Jaeger | OTLP Standard | Unified distributed tracing crossing App $\leftrightarrow$ Worker boundaries. |
| **Testing** | xUnit + Testcontainers | Latest | Fast unit tests + true containerized integration tests (PostgreSql + RabbitMQ) without mocks. |

---

## PostgreSQL 16 & pgvector Vector Search Architecture

PostgreSQL with the `pgvector` extension provides industry-standard vector capabilities:
* **Storage:** Native `vector(dim)` column type inside the `ask` schema.
* **Vector Indexing:** Battle-tested **HNSW (Hierarchical Navigable Small World)** and **IVFFlat** indexes for millisecond approximate nearest neighbor searches.
* **Distance Operators:** Native cosine distance (`<=>`), L2 Euclidean distance (`<->`), and inner product (`<#>`).
* **Scale Consideration:**
  * At v1 scale (~2,000 documents $\approx$ 40,000 page chunks), queries pre-filtered by `spaceId` with HNSW indexes execute in sub-millisecond to low millisecond speeds.
  * Native Apple Silicon (arm64) support ensures top performance on macOS developer machines with minimal RAM footprint (~80–150 MB).

---

## Local AI Integration (Ollama)

All artificial intelligence capabilities are concentrated in the **Ask Module**:
* **Embedding Model:** A small, fast embedding model (such as `all-minilm` or `bge-small-en`, dimension 384).
* **Chat / Generation Model:** A small, instruction-tuned quantized model (such as `llama3.2:3b` or `phi3:mini`) running locally.
* **Network Isolation:** In production Docker Compose, containers operate without outbound internet access, guaranteeing private documents never leave the machine.

---

## File Storage Abstraction (`IFileStore`)

```csharp
public interface IFileStore
{
    Task<string> SaveAsync(Stream stream, string extension, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}
```

* **Storage Paths:**
  * Originals: `storage/originals/{yyyy}/{mm}/{versionGuid}.{ext}`
  * Results: `storage/results/{yyyy}/{mm}/{jobGuid}.json`
* Files travel between programs exclusively as **opaque string storage keys**, never as byte streams inside message payloads.

