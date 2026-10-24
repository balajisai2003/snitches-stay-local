# Requirements: Vision and Scope

## Vision

The **Personal Document Organizer** is a private-first document storage and intelligence system. It lets users store PDFs, Word documents, text files, and images, search them using full-text indexing, and ask questions answered with strict, verifiable citations. 

All AI computation and data processing runs on the user's local hardware—no cloud services or external API dependencies are required.

Beyond product utility, the project serves as a structured platform for learning real-world software engineering: modular monolithic architecture, distributed messaging, reliable outbox patterns, authorization matrices, and automated verification.

---

## Target Audience

- **Primary Persona:** An amateur or learning developer building software from scratch and mastering system architecture, testing, and security.
- **End Users:** Individuals and small groups (e.g., family members or a small team up to ~10 people) collaborating across shared spaces governed by role-based access control.
- **Not Intended For:** Enterprise corporate organizations. There is no support for SSO/SAML, enterprise tenant admin portals, or complex compliance retention policies.

---

## Scale & Operational Environment

- **Document Volume:** Up to ~2,000 documents per installation.
- **File Size:** Each document under 25 MB.
- **Infrastructure:** Runs locally using Docker Compose; zero cloud network dependencies.
- **Hardware Profile:** Standard developer laptop/workstation capable of running Docker and Ollama with a small quantized LLM and embedding model.

---

## Document Formats

| Format | Handling Strategy | Text Extraction |
| --- | --- | --- |
| **PDF** (`.pdf`) | Full processing: text extracted page-by-page, chunked, indexed, searchable, and AI-queryable. | Yes |
| **DOCX** (`.docx`) | Full processing: structured text extracted, indexed, and searchable. | Yes |
| **TXT** (`.txt`) | Full processing: plain text extracted, indexed, and searchable. | Yes |
| **JPG / PNG** (`.jpg`, `.png`) | View-only: stored securely, titled and tagged manually; opens in viewer. | No (view-only in v1) |

---

## Out of Scope for v1

To keep the project manageable and focused on architectural rigor:
- **OCR (Optical Character Recognition):** Scanned receipts, IDs, and image text extraction are deferred to post-v1.
- **Email parsing and threading:** No `.eml` or `.msg` processing.
- **Legal review features:** No Bates numbering, redaction tools, privilege logs, or formal document review workflows.
- **Cloud deployment:** No Kubernetes, cloud storage (S3/Azure Blob), or hosted managed services.
- **Mobile applications:** Responsive web UI only.
- **Real-time collaboration:** No WebSockets or live co-editing of notes.
- **Analytics & Graphs:** No spending timelines, communication graphs, or visualization charts in v1.

---

## Working Principles

1. **Finish a thin vertical slice before adding breadth:** Always get database, backend, messaging, and UI working for one feature before expanding.
2. **Agree on contracts first:** Define DTOs, message schemas, and error codes before implementing either side of a boundary.
3. **Demo & test at every milestone:** Every phase must conclude with an automated test suite passing and a concrete user demo.
4. **Search and organize must work fully without AI:** AI (the Ask module) is an enhancement layer; core filing and search are completely functional even if Ollama is disabled or offline.

