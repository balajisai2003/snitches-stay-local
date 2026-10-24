# ADR-005: File Storage Strategy and Immutability

## Context

Uploaded files (PDF, DOCX, TXT, JPG, PNG) must be stored durably, accessed by both the Main App and the Processing Service, and protected against accidental overwriting or corruption.

Storing binary files directly inside SQL Server `varbinary(max)` columns degrades database backup/restore performance and inflates transaction logs. Storing files on S3/Azure Blob adds unnecessary cloud service dependencies and credential management to a local-first learning project.

## Decision

1. **Shared Docker Volume:** Store physical files on a local disk directory exposed to both the `App` and `Processing` containers as a shared Docker volume.
2. **`IFileStore` Abstraction:** Wrap all file operations behind an `IFileStore` interface so local disk storage can be swapped for object storage in the future without changing business logic.
3. **Immutable Originals (Rule #1):**
   - Files are stored using system-generated GUID storage keys: `originals/{yyyy}/{mm}/{versionGuid}.{ext}`.
   - Original files are **never overwritten**.
   - If a user uploads an edited version of an existing document, the system creates a new `DocumentVersion` record with a distinct `StorageKey`. Version 1 remains byte-identical.
4. **Reference Passing via Messages:** Messages passing between Main App and Processing carry string storage keys (`storageKey`, `resultKey`), never binary payloads.

## Rationale

- **Performance:** Local disk volume I/O is blazing fast and does not burden SQL Server transaction logs.
- **Integrity:** Generated GUID keys prevent path traversal attacks (`../../etc/passwd`) and collision bugs from arbitrary user filenames.
- **Testability:** Downloaded files can be validated for byte-level integrity against the original upload SHA-256 hash.

## Consequences

- **Positive:** Zero cloud cost; high throughput; simple container volume mounting; immutable audit trail.
- **Negative / Trade-off:** Requires both containers to be deployed on the same physical host (or shared network mount) in v1.

