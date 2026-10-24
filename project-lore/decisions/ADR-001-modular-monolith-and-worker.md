# ADR-001: Modular Monolith with Separate Worker Service

## Context

The Personal Document Organizer requires user management, document upload and cataloging, full-text search, local vector RAG, and heavy file text extraction (PDF/DOCX/TXT). 

A common design fork for modern applications is choosing between:
1. A single monolithic application.
2. A microservices architecture (with separate services for Auth, Documents, Search, AI, and Ingestion).
3. A modular monolith paired with a dedicated worker service.

## Decision

We adopt a **modular monolith** for the main application paired with **one separate Processing Worker service**:
- The **Main App** is a single ASP.NET Core process containing strictly partitioned domain modules (`Identity`, `Documents`, `Search`, `Ask`, `Activity`, `Shared`).
- The **Processing Service** is an independent .NET Worker service handling compute-heavy file extraction.
- The two communicate asynchronously over **RabbitMQ** and a **shared Docker storage volume**.

## Rationale

1. **Focus on Learning Without Operational Paralysis:** Microservices require API gateways, service discovery, distributed consensus, and multiple deployment pipelines. A modular monolith provides the structural hygiene of domain separation without the distributed ops tax.
2. **One Real Boundary:** Introducing exactly one distributed boundary (Main App $\leftrightarrow$ Processing) teaches core distributed systems skills: message serialisation, async queues, consumer idempotency, retries, dead-lettering, and distributed tracing.
3. **Compute and Failure Isolation:** PDF parsing and document conversion are CPU-intensive and prone to segmentation faults or memory spikes from corrupt files. Isolating extraction in a separate worker process guarantees that a crash in file parsing never takes down the web app or user API.
4. **Independent Evolution:** If needed in the future, the Search or Ask modules can be extracted into separate standalone services with minimal friction because module boundary rules are strictly enforced by architecture tests.

## Consequences

- **Positive:** Single database to manage (with schema isolation), rapid development feedback loop, robust failure isolation for file ingestion.
- **Negative / Trade-off:** Requires maintaining message contracts and running RabbitMQ; requires discipline to prevent modules from bypassing public facades.

