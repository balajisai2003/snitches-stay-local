# ADR-004: Transactional Outbox and Idempotent Messaging

## Context

When a document is uploaded, the Main App must store the version metadata in SQL Server and instruct the Processing Service to extract text via RabbitMQ. 

If the database write succeeds but the network connection to RabbitMQ fails (the dual-write problem), the document will remain stuck in `Queued` state forever. Conversely, RabbitMQ guarantees at-least-once delivery, meaning network glitches can cause duplicate message delivery to the worker.

## Decision

1. **Transactional Outbox:** The Main App saves outgoing RabbitMQ messages (e.g. `ProcessDocument.v1`) to an `OutboxMessage` table inside the exact same SQL Server transaction that persists the `DocumentVersion`. A background worker periodically polls the outbox and dispatches pending messages.
2. **Idempotent Handlers:** All consumers on both sides of the boundary maintain a `HandledMessage` table tracking unique `messageId`s. Handlers check this table before executing domain logic. Duplicate messages are immediately acknowledged (`BasicAck`) and skipped.
3. **Dead-Letter Exchange:** Messages failing repeatedly due to unhandled exceptions after exponential retry backoff are forwarded to a dead-letter queue (`dlq.processing`).

## Rationale

- **Guaranteed At-Least-Once Delivery:** Decoupling message dispatch from the HTTP request thread ensures that temporary broker downtime never causes data loss.
- **Resilience:** Idempotent consumers allow safe retries after worker crashes or network timeouts without producing duplicate text pages or duplicate chunk embeddings.

## Consequences

- **Positive:** System guarantees zero lost jobs; handles broker restarts gracefully; satisfies the failure drills required in the roadmap.
- **Negative / Trade-off:** Requires a background outbox polling loop and table maintenance for processed `messageId` records.

