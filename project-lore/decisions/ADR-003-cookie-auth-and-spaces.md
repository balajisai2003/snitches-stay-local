# ADR-003: Cookie Authentication and Space-Based Access Control

## Context

The Personal Document Organizer serves individual users and small teams (~10 users). The authorization model must prevent data leakage between users and spaces across all operations (API, search, viewer, and AI).

Token-based authentication (e.g. Bearer JWTs stored in browser `localStorage`) is vulnerable to Cross-Site Scripting (XSS) token theft and introduces complex refresh token rotation lifecycles.

## Decision

1. **Authentication:** Use ASP.NET Core Identity with secure, HTTP-only, `SameSite=Strict` session cookies.
2. **Access Partitioning via Spaces:** All documents belong to a `Space`. A user cannot interact with a document without membership in the document's parent space.
3. **Roles:** Membership grants one of three roles: `Owner`, `Editor`, or `Viewer`.
4. **The Personal Space Invariant:** Every user registration automatically provisions one `Personal` space where `Type = Personal`. Adding any second member to a `Personal` space is strictly disallowed by business logic and database constraints.
5. **Deny-by-Default Authorization:** Every API endpoint validates membership through a centralized `ISpaceAccess` check. If a user attempts to access a space they do not belong to, the API returns `404 Not Found` (rather than `403 Forbidden`) to avoid leaking the existence of unauthorized spaces.

## Rationale

- **Security:** HTTP-only cookies cannot be read by malicious JavaScript, mitigating XSS token extraction.
- **Simplicity:** Session cookies are seamlessly attached by browsers to AJAX/fetch requests without manual header management.
- **Privacy:** Restricting `Personal` spaces to single ownership prevents accidental sharing of sensitive documents (passports, tax filings, medical records).

## Consequences

- **Positive:** Zero client-side token storage overhead; robust defenses against XSS; clean multi-tenant isolation.
- **Negative / Trade-off:** API requires CSRF protection (anti-forgery tokens or custom header verification) for state-changing requests.

