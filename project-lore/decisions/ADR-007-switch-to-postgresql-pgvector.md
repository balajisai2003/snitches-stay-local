# ADR-007: Migration from SQL Server to PostgreSQL with pgvector

- **Status:** Accepted
- **Date:** 2026-10-25
- **Deciders:** User & Antigravity (Pair Programming)
- **Supersedes:** ADR-002 (Data Persistence section) & ADR-006 (Sections 2 & 3: storage engine)

---

## Context

The initial architectural choice (ADR-002) selected Microsoft SQL Server 2025 Developer Edition in Docker due to its unified relational and preview vector capabilities. However, during local Phase 0 / Sprint 1 execution on macOS developer workstations, several operational and architectural limitations emerged:

1. **High Memory Overhead:** The SQL Server on Linux container requires a minimum of 2.0 to 3.5 GB of dedicated RAM to function reliably. On local developer workstations running Ollama (which consumes significant RAM for LLMs), RabbitMQ, Jaeger, and .NET processes simultaneously, this created severe memory pressure.
2. **Preview Vector Constraints:** Vector storage and exact distance calculation in SQL Server 2025 are relatively new, and vector index structures (ANN / HNSW) remain preview features.
3. **Architecture / Platform Friction:** Running SQL Server on macOS Apple Silicon (arm64) relies on emulation or preview compatibility, whereas PostgreSQL provides native `linux/arm64` container images.
4. **Open Source & Self-Hosting Alignment:** *Snitches-Stay-Local* is a private-first personal document organizer. A pure open-source database engine (PostgreSQL License) aligns significantly better with personal self-hosting and zero-telemetry expectations than proprietary Microsoft developer licenses.

---

## Decision

We migrate our single primary relational and vector database from Microsoft SQL Server to **PostgreSQL 16 with the `pgvector` extension** using the official `pgvector/pgvector:pg16` Docker image.

Specifically:
1. **Container Image:** Use `pgvector/pgvector:pg16` in `infra/docker-compose.yml` exposed on standard host port `5432`.
2. **Entity Framework Core Provider:** Standardize on `Npgsql.EntityFrameworkCore.PostgreSQL` and `Pgvector.EntityFrameworkCore`.
3. **Vector Embeddings (Ask Module / RAG):** Store chunk embeddings in the `ask` schema using PostgreSQL's native `vector(dim)` type. Utilize `pgvector`'s generally available, battle-tested HNSW and IVFFlat index types for ultra-fast approximate nearest neighbor retrieval.
4. **Full-Text Search (Search Module):** Leverage PostgreSQL's native `tsvector`, `tsquery`, and GIN indexing for document full-text search, eliminating the need for an external search cluster.
5. **Dependency Health Checks:** Use `AspNetCore.HealthChecks.NpgSql` to probe PostgreSQL availability in `/health`.
6. **Integration Testing:** Utilize `Testcontainers.PostgreSql` in integration test suites.

---

## Rationale

- **Dramatically Lower Resource Footprint:** PostgreSQL typically consumes ~80–150 MB of RAM (a ~95% reduction compared to SQL Server's 2–3.5 GB), freeing vital memory for local Ollama LLMs and embedding models.
- **Battle-Tested RAG Standard:** `pgvector` with HNSW indexing is the recognized industry standard for relational RAG and semantic vector search.
- **Native Apple Silicon Speed:** Runs 100% native on Apple Silicon (arm64) with zero emulation overhead.
- **Top-Tier .NET Support:** `Npgsql` is widely regarded as one of the fastest and most feature-complete EF Core database providers in the .NET ecosystem.
- **Zero Cost & True Open Source:** Eliminates licensing ambiguities for self-hosted users.

---

## Consequences

- **Positive:**
  - Saves 2+ GB RAM on developer and self-hosted environments.
  - Native arm64 execution on macOS Apple Silicon.
  - Battle-tested HNSW vector index support out of the box.
  - Native Full-Text Search via GIN indexes and `to_tsvector`.
- **Negative / Trade-offs:**
  - Team members familiar primarily with T-SQL syntax must adapt to standard PostgreSQL SQL dialect and `tsvector` syntax.
