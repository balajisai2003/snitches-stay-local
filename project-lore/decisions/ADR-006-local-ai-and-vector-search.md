# ADR-006: Local AI and SQL Server Vector Search

## Context

Users want to ask natural-language questions about their documents and receive grounded answers with citations. However:
- Cloud AI APIs (OpenAI, Anthropic) compromise the "private-first" vision by transmitting sensitive private documents over the internet.
- Running a separate vector database (Qdrant, Milvus, Chroma) adds yet another container and service boundary to maintain.
- SQL Server 2025 has introduced native vector types and vector distance functions.

## Decision

1. **Local AI Engine via Ollama:** Run Ollama locally on the developer machine (or containerized) for both embedding generation (e.g. `all-minilm`) and chat completion (e.g. `llama3.2:3b`). No document text is ever sent outside the local network.
2. **Native Vector Storage in SQL Server 2025:** Store document chunk vectors in SQL Server using the native `vector(dim)` type inside the `ask` schema.
3. **Exact Distance Calculation (GA Features Only):**
   - Use SQL Server 2025 GA functions: `VECTOR_DISTANCE('cosine', @queryEmbedding, Chunk.Embedding)`.
   - Avoid preview features (such as `VECTOR_SEARCH` syntax or preview ANN vector index structures) until they reach General Availability.
   - At v1 scale (~2,000 documents $\approx$ 40,000 chunks), filtering by `spaceId` first ensures exact cosine distance takes only milliseconds.
4. **Per-Space AI Switch (`AiEnabled`):**
   - Each space has an `AiEnabled` flag.
   - If disabled, documents are not chunked or embedded, existing chunks are purged, and the Ask API returns `AI_DISABLED`.

## Rationale

- **Absolute Privacy:** Satisfies Security Rule #7 (*"AI runs locally and respects a per-space switch"*).
- **Reduced Infrastructure Complexity:** Eliminates the need for a separate vector database; relational data, full-text indexes, and vector embeddings reside harmoniously in SQL Server.
- **Production Stability:** Relying exclusively on generally available SQL Server features guarantees long-term schema stability.

## Consequences

- **Positive:** Local privacy, zero API subscription costs, unified backup/restore encompassing relational and vector data.
- **Negative / Trade-off:** Requires a local machine with sufficient RAM and CPU/GPU to run Ollama smoothly.

