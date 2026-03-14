# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ez.Reasons is an Icelandic-language website displaying random anonymous letters of encouragement. Visitors read letters, submit new ones, and moderators approve/reject submissions. All UI text is in Icelandic; all URL paths and code are in English.

## Architecture

**Frontend:** Next.js + TypeScript + Tailwind CSS in `src/frontend/`. Statically exported (`output: 'export'`). All API calls happen client-side at runtime, not at build time.

**Backend:** Three C# projects with strict dependency boundaries:

- **Ez.Reasons.Core** — Domain models, repository interfaces, pure business logic services. Zero infrastructure dependencies (no Azure SDK). Services depend only on repository interfaces (`ILetterRepository`, `IUserRepository`).
- **Ez.Reasons.Infrastructure** — Implements repository interfaces against Azure Table Storage. References Core only. Contains table entities, mappers between domain models and storage entities.
- **Ez.Reasons.Api** — Azure Functions (isolated worker, .NET 8). Thin HTTP layer delegating to Core services. References Core + Infrastructure. Contains functions, DI wiring (`Program.cs`), and JWT middleware.

**Tests:** `Ez.Reasons.Core.Tests` — xUnit + Moq. References Core only. Tests mock repository interfaces; no Azurite or storage dependencies needed.

**Deployment:** Azure Static Web Apps. Frontend serves from `src/frontend/out/`, API from `src/api/`. Config in `staticwebapp.config.json` at repo root.

## Build Commands

```bash
# Frontend
cd src/frontend && npm install && npm run build

# Backend (all C# projects)
dotnet build

# Tests
dotnet test tests/Ez.Reasons.Core.Tests/

# Run single test
dotnet test tests/Ez.Reasons.Core.Tests/ --filter "FullyQualifiedName~TestMethodName"
```

## Local Development

Prerequisites: Node.js 18+, .NET 8 SDK, Azure Functions Core Tools v4, SWA CLI, Azurite.

```bash
# Start storage emulator
azurite --silent --location /tmp/azurite

# Start full stack (frontend + API)
swa start src/frontend --api-location src/api
# Available at http://localhost:4280
```

Environment: `src/api/local.settings.json` (gitignored) needs `TableStorageConnection`, `JWT_SECRET`, `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`.

## Key Design Decisions

- **Table Storage partition key = letter status** (`pending`/`approved`/`rejected`). Approve/reject requires delete-then-insert across partitions (insert first, then delete, to avoid data loss on partial failure).
- **Random letter selection** loads all approved letter keys into memory. Acceptable for v1 scale (low thousands).
- **JWT auth** with 24h expiry, no refresh tokens. `JWT_SECRET` from environment variable (min 256 bits). Token stored in localStorage.
- **Moderator accounts** seeded manually into Users table (no UI for account management in v1).

## Conventions

- API error responses use shape `{ error: "message" }`.
- Validation: title max 200 chars, body max 5000 chars, email required and valid format. Email is never exposed via public API endpoints.
- Frontend pages: `/` (home), `/about`, `/submit`, `/login`, `/dashboard`. Icelandic labels (e.g., "Naesta bref", "Samthykkja", "Hafna").
