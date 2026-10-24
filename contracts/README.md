# Contracts

This directory contains message schemas (JSON Schema) and OpenAPI / API specifications that define the formal agreements between services and modules.

## Target Contracts for Phase 0 & Phase 3

- `Hello.v1.json`: The handshake test message for Phase 0.
- `ProcessDocument.v1.json`: Extraction command sent from Main App to Processing Service.
- `DocumentProcessed.v1.json`: Completion event sent from Processing Service to Main App.
- `DocumentProcessingFailed.v1.json`: Failure event sent from Processing Service to Main App.

