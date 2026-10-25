# Setup: Prerequisites and Tooling

## Required Developer Tooling

Before building the system, ensure the following tools are installed on your machine:

| Tool | Minimum Version | Verified Local Status | Installation Link |
| --- | --- | --- | --- |
| **.NET SDK** | .NET 10 LTS | Installed (`10.0.401`) | [Download .NET 10](https://dotnet.microsoft.com/download) |
| **Node.js** | Current LTS (20+) | Installed (`v24.21.0`) | [Download Node.js](https://nodejs.org/) |
| **Docker Desktop** | Engine 24+ / Compose v2 | Installed (`29.8.2`) | [Download Docker Desktop](https://www.docker.com/products/docker-desktop/) |
| **Git** | 2.40+ | Installed (`2.56.0`) | [Download Git](https://git-scm.com/) |
| **Ollama** | Latest | Optional until Phase 6 | [Download Ollama](https://ollama.com/) |

---

## Recommended Editors & Extensions

* **IDE / Editor:**
  * **JetBrains Rider** (Recommended for C# / .NET development).
  * **Visual Studio Code** with:
    * C# Dev Kit (`ms-dotnettools.csdevkit`)
    * Docker extension (`ms-azuretools.vscode-docker`)
    * PostgreSQL extension (`cweijan.vscode-postgresql-client2` or `ckolkman.vscode-postgres`)
* **Database Management Tools:**
  * DBeaver, pgAdmin, TablePlus, or VS Code Database Client.

---

## Verifying Your Environment

Run this quick command in your terminal to verify your tool versions:

```bash
dotnet --version
docker --version
node --version
git --version
```

If all commands output valid version numbers, your workstation is ready for **Phase 0**.

