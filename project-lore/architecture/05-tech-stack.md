# Architecture: Technology Stack and Integrations

## Core Technology Selection

| Component | Selected Technology | Version / Target | Architecture Rationale |
| --- | --- | --- | --- |
| **Main App** | ASP.NET Core Web API | .NET 10 LTS (C#) | High throughput, mature dependency injection, native health checks. |
| **Processing Worker** | .NET Worker Service | .NET 10 LTS (C#) | Headless background daemon with minimal footprint. |
| **Frontend** | React + TypeScript | Vite | Fast HMR, clean component model, single-page application. |
| **Database** | Microsoft SQL Server | SQL Server 2025 (Docker) | Full-text search and vector columns in a single transactional database. |
| **Data Access** | Entity Framework Core | 10.x | Code-first migrations, schema management, optimistic concurrency. |
| **Message Broker** | RabbitMQ | 3.13+ (Docker) | Industrial message queuing; official .NET client behind `IMessageBus`. |
| **File Storage** | Shared Docker Volume | Local Disk via `IFileStore` | Zero cloud storage cost, simple volume mapping between containers. |
| **Local AI Engine** | Ollama | Latest (Docker or Host) | Local execution of LLM and embedding models; zero external API calls. |
| **Telemetry** | OpenTelemetry + Jaeger | OTLP Standard | Unified distributed tracing crossing App $\leftrightarrow$ Worker boundaries. |
| **Testing** | xUnit + Testcontainers | Latest | Fast unit tests + true containerized integration tests without mocks. |

---

## SQL Server 2025 Vector Search Architecture

SQL Server 2025 introduced native vector capabilities:
* **Generally Available (GA) Features:** 
  * The native `vector(dim)` column type.
  * Exact vector distance functions: `VECTOR_DISTANCE('cosine', @v1, @v2)`.
* **Preview Features (Do Not Depend On for v1):**
  * Approximate Nearest Neighbor (ANN) index structures and `VECTOR_SEARCH` syntax are still preview features.
* **Scale Consideration:**
  * At v1 scale (~2,000 documents $\approx$ 40,000 page chunks), exact cosine distance calculation via `VECTOR_DISTANCE` executes in **milliseconds**.
  * By restricting queries to GA features and pre-filtering by `spaceId`, vector similarity search is ultra-fast, predictable, and robust.

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

