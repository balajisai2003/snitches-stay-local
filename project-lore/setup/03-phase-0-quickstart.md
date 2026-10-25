# Setup: Phase 0 Bootstrapping Quickstart

This step-by-step guide walks you through setting up the repository from scratch and completing **Phase 0**.

---

## 1. Git Initialization & Layout

From the root project directory (`/Users/balajinadakuditi/Repos/Personal Document Organizer`):

```bash
# 1. Initialize Git repository
git init

# 2. Create the target directory structure
mkdir -p src/App
mkdir -p src/Modules/{Identity,Documents,Search,Ask,Activity,Shared}
mkdir -p src/Processing
mkdir -p contracts
mkdir -p frontend
mkdir -p infra
mkdir -p tests/{App.Tests,Processing.Tests,Architecture.Tests,E2E}
```

---

## 2. Scaffold .NET 10 Solution & Projects

```bash
# 1. Create root solution
dotnet new sln -n Organizer

# 2. Create Main App (Web API) and Processing (Worker)
dotnet new webapi -n Organizer.App -o src/App
dotnet new worker -n Organizer.Processing -o src/Processing

# 3. Create Module Class Libraries
dotnet new classlib -n Organizer.Modules.Identity -o src/Modules/Identity
dotnet new classlib -n Organizer.Modules.Documents -o src/Modules/Documents
dotnet new classlib -n Organizer.Modules.Search -o src/Modules/Search
dotnet new classlib -n Organizer.Modules.Ask -o src/Modules/Ask
dotnet new classlib -n Organizer.Modules.Activity -o src/Modules/Activity
dotnet new classlib -n Organizer.Modules.Shared -o src/Modules/Shared

# 4. Create Test Projects
dotnet new xunit -n Organizer.Architecture.Tests -o tests/Architecture.Tests
dotnet new xunit -n Organizer.App.Tests -o tests/App.Tests
dotnet new xunit -n Organizer.Processing.Tests -o tests/Processing.Tests

# 5. Add all projects to the solution
dotnet sln add src/App/Organizer.App.csproj
dotnet sln add src/Processing/Organizer.Processing.csproj
dotnet sln add src/Modules/*/*.csproj
dotnet sln add tests/*/*.csproj
```

---

## 3. Configure Module References

Enforce modular dependencies:
* `Organizer.App` references all modules (`Identity`, `Documents`, `Search`, `Ask`, `Activity`, `Shared`).
* Modules reference only `Organizer.Modules.Shared`.
* `Architecture.Tests` references all projects to enforce boundary rules using **NetArchTest.Rules**.

```bash
dotnet add tests/Architecture.Tests/Organizer.Architecture.Tests.csproj package NetArchTest.Rules
```

---

## 4. Spin Up Infrastructure via Docker Compose

```bash
cd infra
docker compose up -d sqlserver rabbitmq jaeger
```

Verify services are running:
* **RabbitMQ UI:** Open [http://localhost:15672](http://localhost:15672) (User: `organizer`, Pass: `organizer_secret`)
* **Jaeger UI:** Open [http://localhost:16686](http://localhost:16686)
* **PostgreSQL:** Connect via DBeaver/psql on `localhost:5432` (User: `postgres`, Pass: `postgres`, DB: `organizer`)

---

## 5. Implement and Verify `Hello.v1` Message

1. In `src/Modules/Shared`, create the `Hello.v1` contract:
   ```csharp
   public record HelloMessage(
       string MessageId, 
       string CorrelationId, 
       int SchemaVersion, 
       DateTimeOffset OccurredAt, 
       string Greeting);
   ```
2. In `src/App`, publish `Hello.v1` on startup or via a test endpoint `POST /api/test/hello`.
3. In `src/Processing`, consume `Hello.v1`, verify idempotency against processed IDs, and log receipt.
4. Open [http://localhost:16686](http://localhost:16686) in Jaeger to verify the unified trace connecting both services.

---

## 6. Run Automated Tests

```bash
dotnet test
```

When all architecture and unit tests pass, you have successfully crossed the **Phase 0 Gate**!

