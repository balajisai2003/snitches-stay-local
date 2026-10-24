# Requirements: Features by Tier & Scope Management

## Feature Matrix

The system features are partitioned across three tiers:
- **v1 (Build Now):** A fully functional, secure, private document organizer with search and citations.
- **v2 (Next):** Enhancements that make the app indispensable for weekly personal operations.
- **Later:** Long-term advanced features.

| Functional Area | v1 (Build Now) | v2 (Next) | Later |
| --- | --- | --- | --- |
| **Spaces & Sharing** | Private personal space, shared group spaces, roles (`Owner`, `Editor`, `Viewer`), per-space AI switch (`AiEnabled`). | Invites via email or link, move documents between spaces. | Copy documents between spaces. |
| **Adding Documents** | Upload PDF, DOCX, TXT; JPG and PNG stored view-only; immutable versions; live upload status with retry. | Drag-and-drop whole folders. | Email inbox import, folder sync watch, mobile phone scan, OCR. |
| **Organizing** | Space-scoped tags, status triage (`New`, `Needs action`, `Done`, `Archived`), document notes with optimistic locking, exact-duplicate detection (SHA-256). | Near-duplicate detection, AI-suggested tags and titles, favorites, bulk batch actions. | Custom metadata schemas. |
| **Finding / Search** | Full-text search with operators (`AND`, `OR`, `NOT`, `"phrases"`, wildcards `*`); highlights & page numbers; filter by tag, status, type; cross-space search for authorized spaces. | Proximity search, saved searches, hybrid semantic search. | Natural language search filters. |
| **Ask (Local AI)** | Grounded Q&A over a single space; strict page citations; fallback *"I couldn't find that"*; click citation to jump to viewer page. | Single-document executive summary. | Ask across multiple spaces, side-by-side document comparison. |
| **Dates & Reminders** | *(None in v1)* | Automatic extraction of due dates and dollar amounts, "Coming Up" agenda view, calendar reminders. | Spending summaries, visual timeline graphs. |
| **Viewing & Export** | In-app document viewer with page navigation, download original file, zip bundle export. | *(None)* | Expiring read-only guest links. |
| **Trust & Audit** | Role checks on every endpoint, append-only activity audit log, stable error codes, encrypted local storage. | Two-factor authentication (2FA), soft-delete trash with restore, full backup/restore CLI. | Immutable audit tamper seals. |

---

## Best v2 Bet: Dates and Reminders

The highest-leverage post-v1 feature is **Dates and Reminders**. 
Answering *"What is expiring soon?"* (car insurance, utility bills, contract renewals, warranties) is the primary trigger that turns a static archive into a weekly habit.

---

## Scope Management: Cut Order

If project time is constrained or you feel overwhelmed, **do not compromise on architectural rules or automated testing**. Instead, cut features strictly in this prioritized order:

1. **Ask Module (Phase 6):** Move the AI layer to a stretch goal. Phases 0–5 deliver a complete, highly capable document management and search platform without any LLM.
2. **Activity Screen:** Keep the underlying `ActivityEvent` database table recording operations, but defer building the dedicated audit UI screen.
3. **Shared Space Management UI:** Retain space roles and permissions in Identity backend APIs, but defer complex UI screens for managing member lists.
4. **JPG & PNG Storage:** Focus exclusively on text documents (PDF, DOCX, TXT).
5. **Near-Duplicate Grouping & Proximity Search:** Stick strictly to exact SHA-256 duplicate detection and standard full-text search operators.

