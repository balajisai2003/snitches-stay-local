# Requirements: Roadmap and Phase Milestones

## Roadmap at a Glance

The project spans 8 distinct phases, totaling approximately 16 weeks full-time (~640 hours). Every phase concludes with a demonstrable vertical slice and verifiable quality gates.

| Phase | Title | Est. Full-Time | Gate to Pass |
| --- | --- | --- | --- |
| **0** | **Foundation** | 1 week | Two programs exchange a message via RabbitMQ under Docker Compose |
| **1** | **Identity and spaces** | 2 weeks | Secure cookie sign-in; strict cross-space authorization matrix |
| **2** | **Documents and upload** | 2 weeks | Stream upload, versioning, SHA-256 deduplication |
| **3** | **Processing service** | 3 weeks | Standalone text extraction worker for PDF, DOCX, TXT |
| **4** | **Organize and viewer** | 2 weeks | Tagging, status triage, notes with row-versioning, page viewer |
| **5** | **Search module** | 2 weeks | 10 gold search queries pass with highlights and page numbers |
| **6** | **Ask module** | 3 weeks | RAG answering with $\ge 80\%$ citation accuracy over local Ollama |
| **7** | **Harden and release** | 1 week | Backup/restore drill, failure resilience, fresh clone demo |

---

## Detailed Phase Breakdown

### Phase 0: Foundation (1 Week)
* **Goal:** A working workshop with the repository layout, Docker containers, and one message traveling between the two programs.
* **Learn First:** Docker images/volumes/Compose, RabbitMQ publisher/queue/consumer concepts, .NET multi-project solution structure.
* **Build Steps:**
  1. Initialize repo layout (`src/App`, `src/Processing`, `src/Modules/*`, `tests/*`, `infra/`).
  2. Create `src/App` (Web API) and `src/Processing` (Worker Service) with `/health` endpoints.
  3. Author `infra/docker-compose.yml` (SQL Server, RabbitMQ, shared volume, Jaeger).
  4. Define `IMessageBus` and `Hello.v1` message. Publish from App, consume in Processing.
  5. Implement consumer idempotency (track handled `messageId`s).
  6. Add OpenTelemetry tracing connecting App and Processing in one trace.
  7. Scaffold the 5 modules and create an initial architecture test with NetArchTest.
  8. Configure GitHub Actions CI pipeline.
* **Try It Yourself:** Stop RabbitMQ while App is running, publish a message, restart RabbitMQ, and verify successful handling.
* **Outcomes Checklist:**
  - [ ] `docker compose up` starts all services on a clean machine.
  - [ ] `Hello.v1` is handled; duplicate deliveries have only a single effect.
  - [ ] Messages queued while Processing is offline process upon restart.
  - [ ] Distributed trace connects App and Processing in Jaeger.
  - [ ] Architecture tests pass and fail correctly on illegal references.
  - [ ] CI pipeline is green.

---

### Phase 1: Identity and Spaces (2 Weeks)
* **Goal:** Users can register, sign in, manage spaces, and hold roles, with every path authorized.
* **Learn First:** AuthN vs. AuthZ, cookie sign-in mechanics, password hashing, EF Core migrations, React routing.
* **Build Steps:**
  1. Identity module with ASP.NET Core Identity in `identity` schema.
  2. Cookie-based sign-in, registration, sign-out endpoints.
  3. `Space` and `SpaceMember` tables; auto-create Personal space on registration; reject second member on Personal space.
  4. Roles (`Owner`, `Editor`, `Viewer`) and shared `ISpaceAccess` check.
  5. Public `IIdentityApi` with `GetAllowedSpaces(userId)`.
  6. React pages: sign-in, register, space switcher, protected routes.
  7. Authorization test suite: 2 users, 3 spaces, matrix of allowed/forbidden endpoints.
* **Try It Yourself:** Sign in as User A and alter the space ID in the URL to User B's space; verify zero data leakage (returns 404).
* **Outcomes Checklist:**
  - [ ] Registration creates a Personal space; adding a second member is rejected.
  - [ ] Unauthenticated requests return 401; non-members get 404 for inaccessible spaces.
  - [ ] Authorization test suite passes across all endpoints.
  - [ ] Automated test proves plain passwords never appear in DB or logs.
  - [ ] Architecture tests enforce that other modules access Identity only via `IIdentityApi`.

---

### Phase 2: Documents and Upload (2 Weeks)
* **Goal:** Store documents safely and manage immutable versions without reading document contents in the main app.
* **Learn First:** ASP.NET Core streaming uploads, SHA-256 integrity, transactional outbox pattern.
* **Build Steps:**
  1. Documents module with `Document` and `DocumentVersion` in `documents` schema.
  2. `IFileStore` abstraction backed by local shared volume using generated storage keys.
  3. Upload endpoint: stream directly to disk, compute SHA-256, validate magic bytes, create version record.
  4. Re-upload creates version 2 while keeping version 1 immutable.
  5. Exact-duplicate detection within a space via SHA-256.
  6. Transactional outbox: atomicity between version creation and `ProcessDocument.v1` message dispatch.
  7. State transitions: `Uploaded` $\rightarrow$ `Queued` (Images go straight to `Ready`).
  8. React upload form with progress bar and document list with status chips.
* **Try It Yourself:** Rename a `.exe` to `.pdf` and attempt upload; verify rejection by file signature inspection.
* **Outcomes Checklist:**
  - [ ] 25 MB file upload returns document ID and `Queued` status in under 1 second.
  - [ ] Downloaded original file is byte-identical to upload; version 2 does not mutate version 1.
  - [ ] Uploading an identical file flags "also found in...".
  - [ ] Uploads succeed and stay `Queued` when Processing service is stopped.
  - [ ] Images immediately transition to `Ready`.
  - [ ] Outbox guarantees message delivery even during process crashes.

---

### Phase 3: Processing Service (3 Weeks)
* **Goal:** Standalone worker service that extracts text and page metadata from files without any knowledge of users or spaces.
* **Learn First:** .NET Worker Services, PdfPig, Open XML SDK, retry policies with backoff, dead-letter queues.
* **Build Steps:**
  1. Define message contracts in `contracts/` and OpenAPI spec for `POST /jobs` / `GET /jobs/{id}`.
  2. Dedicated `ProcessingJob` database schema for the worker.
  3. `ITextExtractor` implementations for TXT, DOCX, and PDF returning structured pages.
  4. Safeguards: 25 MB limit, execution timeout per file, sanitization of exception messages.
  5. Save result JSON to shared results volume; publish `DocumentProcessed.v1` or `DocumentProcessingFailed.v1`.
  6. Retry with exponential backoff; route exhausted failures to dead-letter queue.
  7. Main App handles replies: transition version state to `Ready` or `Failed` and emit `DocumentReady` in-process event.
  8. Standalone test script `scripts/process-file.sh` for testing the worker in isolation.
* **Try It Yourself:** Terminate the Processing container halfway through extracting a large document; verify recovery upon container restart.
* **Outcomes Checklist:**
  - [ ] Processing runs completely standalone with only RabbitMQ, its own DB, and the storage volume.
  - [ ] TXT, DOCX, and PDF return correct text and page numbers.
  - [ ] Result message passes result file path reference, never raw text in message payloads.
  - [ ] Corrupt or password-protected files fail cleanly with `EXTRACTION_FAILED` without crashing the worker.
  - [ ] Handler idempotency proven: duplicate message processing results in a single output.
  - [ ] Test proves Processing codebase has zero references to users, spaces, or tags.

---

### Phase 4: Organizing and Viewing (2 Weeks)
* **Goal:** Enable triage, tagging, metadata notes, and in-app document viewing.
* **Learn First:** Optimistic concurrency via SQL row-versioning, REST filter/pagination conventions, React keyboard event listeners.
* **Build Steps:**
  1. Tags (space-scoped), status (`New`, `Needs action`, `Done`, `Archived`), notes with `RowVersion` concurrency checks.
  2. List endpoint with filter parameters (`tag`, `status`, `mimeType`).
  3. Delete document workflow: delete files, raise `DocumentDeleted` in-process event.
  4. Document viewing API: verify space access and serve page-by-page extracted text or raw original.
  5. React document viewer with page controls, zoom, and text/original toggle.
  6. Keyboard triage UI: rapidly navigate and update metadata using hotkeys.
* **Try It Yourself:** Open the same note in two browser tabs; modify and save in both to verify `EDIT_CONFLICT` (409) handling.
* **Outcomes Checklist:**
  - [ ] Add/remove tags and modify status in 2 clicks; filter across tag, status, and file type.
  - [ ] Concurrent edits trigger `EDIT_CONFLICT` with user-friendly conflict resolution prompt.
  - [ ] Tags are strictly space-isolated (e.g. "Tax" in Personal does not leak into Shared).
  - [ ] Viewer opens any `Ready` document page in under 2 seconds.
  - [ ] Deleting a document raises `DocumentDeleted` and purges all derived data.

---

### Phase 5: Search Module (2 Weeks)
* **Goal:** Full-text keyword search across extracted document text without any AI dependencies.
* **Learn First:** SQL Server Full-Text indexing, `CONTAINSTABLE`, recursive-descent query parsers, parameterized SQL generation.
* **Build Steps:**
  1. `IndexedPage` table in `search` schema with SQL Server Full-Text index migration.
  2. Handle `DocumentReady` event: stream result JSON from storage and insert page records.
  3. Query parser: tokenize input, build AST supporting `AND`, `OR`, `NOT`, `"phrases"`, wildcards `*`, and generate safe full-text expressions.
  4. Search endpoint: accept query + allowed space IDs from `IIdentityApi`; return ranked documents, snippets, and highlights.
  5. Combine full-text search results with Documents module filters (tags, status).
  6. React search UI: search bar, term highlighting, page badges, jump-to-viewer.
  7. Gold search suite: 10 automated test queries verifying expected document rankings.
  8. Full index replay CLI command to reconstruct search tables from raw storage files.
* **Try It Yourself:** Input malformed queries (e.g., `a OR`, `"unclosed quotes`); verify parser returns `QUERY_INVALID` instead of crashing.
* **Outcomes Checklist:**
  - [ ] 10 automated gold search queries return exact expected rankings.
  - [ ] Malformed queries return `QUERY_INVALID` with safe syntax advice.
  - [ ] Multi-user search strictly isolates results to authorized space IDs.
  - [ ] Query response time $< 1$ second across 2,000 documents.
  - [ ] Index replay successfully rebuilds identical search tables.

---

### Phase 6: Ask Module (3 Weeks)
* **Goal:** Question-answering over a single space with strict page-level citations and grounded AI responses.
* **Learn First:** Vector embeddings, chunking strategies, RAG grounding, prompt injection defenses, Ollama HTTP API.
* **Build Steps:**
  1. Configure local Ollama container with a quantized chat model and embedding model.
  2. Implement `IEmbeddingService` and `IAnswerService` talking to Ollama.
  3. Handle `DocumentReady`: if space `AiEnabled == true`, chunk text (~300 words with 10% overlap), generate embeddings, store in `DocumentChunk`.
  4. Retrieval pipeline: filter chunks by `spaceId`, calculate cosine similarity via SQL Server 2025 GA `VECTOR_DISTANCE`.
  5. Grounded prompt engineering: instruct LLM to use only provided passages, mandate bracketed citations, and fall back to *"I couldn't find that"*.
  6. Post-generation citation validator: reject answers missing valid citations.
  7. Automated evaluation set: 20–30 test questions with expected citations and negative tests.
  8. React Ask UI: conversational chat pane, message history, interactive citation links.
* **Try It Yourself:** Ask identical questions with 3 chunks vs. 10 chunks; observe answer fidelity, latency, and context window limits.
* **Outcomes Checklist:**
  - [ ] $\ge 80\%$ of answerable test questions answer correctly with accurate document and page citations.
  - [ ] All unanswerable questions return *"I couldn't find that"* without hallucinating.
  - [ ] Prompt injection payloads inside uploaded documents fail to breach boundary instructions.
  - [ ] Spaces with `AiEnabled == false` skip chunking and return `AI_DISABLED`.
  - [ ] If Ollama is stopped, Ask returns `AI_UNAVAILABLE` while the rest of the application remains fully functional.

---

### Phase 7: Harden and Release (1 Week)
* **Goal:** Ensure the system is trustworthy, recoverable, resilient to outages, and ready for real-world usage.
* **Learn First:** Secret scrubbing, database backup and file volume snapshot restoration, OWASP Top 10 web security.
* **Build Steps:**
  1. Activity module: handle `ActionRecorded` events into an append-only audit table.
  2. Zip export feature: download raw originals for selected tags or spaces.
  3. Automated backup and restore scripts for SQL Server and storage volumes.
  4. Security audit pass: sanitize logs, enforce CORS/CSRF headers, review package dependencies.
  5. Resilience drills: simulate sudden crashes of RabbitMQ, Ollama, and Processing services.
  6. 2,000-document synthetic load test.
  7. Polish developer documentation, seed scripts, and create a 15-minute quickstart demo script.
* **Try It Yourself:** Hand the clean repository and setup guide to a peer on another machine; observe and document any friction points.
* **Outcomes Checklist:**
  - [ ] Every user action (sign-in, upload, edit, delete, export) is recorded in Activity audit logs without leaking document text.
  - [ ] Zip export produces byte-identical files.
  - [ ] Database & volume restore on a clean machine recovers 100% of documents and search indexes.
  - [ ] 2,000-document load test completes with zero failed jobs and search response $< 1$s.
  - [ ] A clean machine clone can run `docker compose up` and run the demo within 15 minutes.

