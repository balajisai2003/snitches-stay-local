# Developer and Operational Scripts

This directory contains shell scripts and CLI utilities to support development, testing, and operational drills across the project phases.

---

## Script Inventory

| Script | Phase | Purpose |
| --- | --- | --- |
| **`verify-phase0.sh`** | Phase 0 | Verifies local developer tooling (.NET 10, Docker, Node, Git) and tests connectivity to Docker services. |
| **`process-file.sh`** | Phase 3 | Submits a test document directly to the standalone Processing Service via HTTP `POST /jobs` and polls `GET /jobs/{id}` for the result. |

---

## Usage Instructions

Make scripts executable before running:

```bash
chmod +x scripts/*.sh
```

### Running Phase 0 Verification

```bash
./scripts/verify-phase0.sh
```

### Running Standalone Processing Worker Test

```bash
./scripts/process-file.sh sample.pdf
```

