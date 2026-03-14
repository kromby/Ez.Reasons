# Ez.Reasons — Technical Specification

## Stack

- **Frontend**: Next.js + TypeScript + Tailwind CSS, static export (`output: 'export'`)
- **Backend**: C# Azure Functions (isolated worker, .NET 8)
- **Storage**: Azure Table Storage
- **Auth**: JWT (24h expiry, signed with `JWT_SECRET` env var, min 256 bits)
- **Hosting**: Azure Static Web Apps

---

## Project Structure

Three C# projects with strict dependency boundaries:

- **Ez.Reasons.Core** — Domain models, repository interfaces, pure business logic services. Zero infrastructure dependencies (no Azure SDK). Services depend only on repository interfaces.
- **Ez.Reasons.Infrastructure** — Implements repository interfaces against Azure Table Storage. References Core. Contains table entities, mappers between domain models and storage entities.
- **Ez.Reasons.Api** — Azure Functions project. Thin HTTP layer delegating to Core services. References Core + Infrastructure. Contains function definitions, DI registration (`Program.cs`), and JWT middleware.
- **Ez.Reasons.Core.Tests** — xUnit + Moq. References Core only. Mocks repository interfaces.

```
Ez.Reasons/
├── staticwebapp.config.json
├── swa-cli.config.json
├── src/
│   ├── frontend/
│   │   ├── package.json
│   │   ├── next.config.ts
│   │   ├── tailwind.config.ts
│   │   ├── app/
│   │   │   ├── layout.tsx
│   │   │   ├── globals.css
│   │   │   ├── page.tsx
│   │   │   ├── about/page.tsx
│   │   │   ├── submit/page.tsx
│   │   │   ├── login/page.tsx
│   │   │   └── dashboard/page.tsx
│   │   └── components/
│   │       ├── Navbar.tsx
│   │       ├── Footer.tsx
│   │       ├── LetterCard.tsx
│   │       ├── SubmitForm.tsx
│   │       ├── LoginForm.tsx
│   │       ├── FeedbackPrompt.tsx
│   │       └── AuthProvider.tsx
│   ├── Ez.Reasons.Core/
│   │   ├── Ez.Reasons.Core.csproj
│   │   ├── Models/
│   │   │   ├── Letter.cs
│   │   │   ├── User.cs
│   │   │   └── Dtos.cs
│   │   ├── Repositories/
│   │   │   ├── ILetterRepository.cs
│   │   │   └── IUserRepository.cs
│   │   └── Services/
│   │       ├── ILetterService.cs
│   │       ├── LetterService.cs
│   │       ├── IAuthService.cs
│   │       └── AuthService.cs
│   ├── Ez.Reasons.Infrastructure/
│   │   ├── Ez.Reasons.Infrastructure.csproj
│   │   ├── Entities/
│   │   │   ├── LetterEntity.cs
│   │   │   └── UserEntity.cs
│   │   ├── Mappers/
│   │   │   └── EntityMappers.cs
│   │   └── Repositories/
│   │       ├── TableLetterRepository.cs
│   │       └── TableUserRepository.cs
│   └── api/
│       ├── Ez.Reasons.Api.csproj
│       ├── Program.cs
│       ├── host.json
│       ├── local.settings.json
│       ├── Functions/
│       │   ├── LetterFunctions.cs
│       │   ├── ModerationFunctions.cs
│       │   └── AuthFunctions.cs
│       └── Middleware/
│           └── JwtMiddleware.cs
├── tests/
│   └── Ez.Reasons.Core.Tests/
│       ├── Ez.Reasons.Core.Tests.csproj
│       ├── LetterServiceTests.cs
│       └── AuthServiceTests.cs
└── .github/workflows/azure-swa.yml
```

---

## Data Model

### Letters table

| Field | Type | Constraint |
|---|---|---|
| PartitionKey | string | Letter status: `pending`, `approved`, `rejected` |
| RowKey | string | GUID |
| Title | string | Required, max 200 characters |
| Body | string | Required, max 5000 characters |
| Email | string | Optional, valid email format if provided, never exposed via public API |
| SubmittedAt | DateTimeOffset | Set on creation |
| ReviewedAt | DateTimeOffset | Null until moderated |
| ReviewedBy | string | Null until moderated; moderator username |
| ViewCount | int | Incremented each time the letter is displayed. Default 0. |
| LikeCount | int | Incremented when a visitor likes the letter. Default 0. |
| DislikeCount | int | Incremented when a visitor dislikes the letter. Default 0. |

Approving or rejecting a letter changes its PartitionKey. Table Storage does not support updating partition keys in place. The operation requires: (1) insert into new partition, (2) delete from old partition. Insert-first ordering prevents data loss on partial failure.

### Users table

| Field | Type | Constraint |
|---|---|---|
| PartitionKey | string | Fixed value: `moderator` |
| RowKey | string | Username, lowercase |
| PasswordHash | string | bcrypt, work factor 12 |
| CreatedAt | DateTimeOffset | Set on creation |

First moderator account is seeded manually or via a seed script.

---

## Letter Selection and Scoring

### Quality Score

Each approved letter has a quality score: `LikeCount - DislikeCount`. Letters with higher scores are more likely to be selected.

### Seen Tracking

- The frontend stores IDs of previously seen letters in localStorage.
- When requesting a letter, the frontend sends the seen IDs to the API.
- The API excludes seen IDs from the candidate pool.
- If all approved letters have been seen, the seen list is ignored and any approved letter may be returned.

### Selection Algorithm

1. Query all approved letters.
2. Exclude IDs present in the client's seen list.
3. If no candidates remain, use all approved letters (ignore seen list).
4. Weight candidates by quality score (higher score = higher probability). Letters with score <= 0 still have a minimum weight so they are not permanently excluded.
5. Select one letter using weighted random selection.
6. Increment the selected letter's `ViewCount`.

### View Count Updates

Each time a letter is returned by the API, its `ViewCount` is incremented. This is a single field update on the entity (no partition key change).

---

## API Endpoints

All endpoints prefixed with `/api` (Azure SWA convention).

### Public

| Method | Route | Request | Response |
|---|---|---|---|
| POST | `/api/letters/next` | `{ seenIds: ["id1", "id2", ...] }` | `{ id, title, body, submittedAt }` or 404 |
| POST | `/api/letters` | `{ title, body, email? }` | 201 Created |
| POST | `/api/letters/{id}/feedback` | `{ type: "like" \| "dislike" }` | 200 OK |

Note: The letter selection endpoint is POST (not GET) because the client sends a list of seen IDs in the request body.

### Auth

| Method | Route | Request | Response |
|---|---|---|---|
| POST | `/api/auth/login` | `{ username, password }` | `{ token }` (JWT, 24h expiry) |

### Protected (require `Authorization: Bearer <token>`)

| Method | Route | Request | Response |
|---|---|---|---|
| GET | `/api/moderation/pending` | — | `[{ id, title, body, email, submittedAt }]` |
| POST | `/api/moderation/{id}/approve` | — | 200 OK |
| POST | `/api/moderation/{id}/reject` | — | 200 OK |

### Validation

- `title`: required, max 200 characters
- `body`: required, max 5000 characters
- `email`: optional, valid email format if provided
- `feedback type`: must be `"like"` or `"dislike"`
- `seenIds`: array of strings, may be empty
- All error responses: `{ error: "message" }`

---

## Authentication

- Moderators authenticate with username and password
- API verifies bcrypt hash, returns JWT with claims: `sub` (username), `role` ("moderator"), `exp` (24h)
- Signing key: `JWT_SECRET` environment variable (min 256 bits)
- Frontend stores token in localStorage
- Protected endpoints guarded by `JwtMiddleware` validating signature and expiry
- No refresh tokens; moderators re-authenticate after 24 hours
- On 401 response, frontend clears token and redirects to `/login`

---

## Frontend

All data fetching happens client-side at runtime (not at build time). The frontend is a static export — no server-side rendering.

### Shared Components

- **Navbar**: Links to Home, About, Submit. Conditionally shows Dashboard and Logout when authenticated.
- **Footer**: Site info.
- **LetterCard**: Displays a single letter (title, body, date).
- **FeedbackPrompt**: Shown when visitor clicks "next letter". Three options: like, dislike, skip. Calls `/api/letters/{id}/feedback` on like/dislike, then loads the next letter.
- **AuthProvider**: React context for JWT state (token in localStorage, `isAuthenticated`, `login()`, `logout()`).

### Seen Letter Tracking (Frontend)

- On page load, read seen IDs from `localStorage` key `ez-reasons-seen`.
- Send seen IDs with each letter request (`POST /api/letters/next`).
- After receiving a letter, add its ID to the seen list and persist to `localStorage`.
- If `localStorage` is unavailable, send an empty seen list. Repeats are acceptable.

### Icelandic Labels

- "Naesta bref" — next letter button
- "Titill" — title field
- "Bref" — body field
- "Netfang" — email field
- "Samthykkja" — approve button
- "Hafna" — reject button

---

## Testability

- `LetterService` depends on `ILetterRepository`. `AuthService` depends on `IUserRepository`. No storage SDK in scope.
- Unit tests mock repository interfaces with Moq.
- `Core.Tests` references only Core + test packages. No Azure SDK, no Infrastructure, no Functions host.
- Repository interface methods:
  - `ILetterRepository`: `GetApprovedAsync()`, `CreateAsync(Letter)`, `GetPendingAsync()`, `MoveToStatusAsync(id, newStatus, reviewedBy)`, `GetByIdAsync(id)`, `IncrementViewCountAsync(id)`, `IncrementLikeCountAsync(id)`, `IncrementDislikeCountAsync(id)`
  - `IUserRepository`: `GetByUsernameAsync(username)`
- Selection logic (weighted random, seen-list exclusion) lives in `LetterService` and is fully testable with mocked data.

---

## Deployment

- Azure Static Web Apps
- Frontend: static export from `src/frontend/`, output directory `out/`
- API: Azure Functions from `src/api/`
- `staticwebapp.config.json` at repo root with navigation fallback to `index.html` (excluding `/api/*` and `/_next/*`)
- Environment variables in Azure portal: `TableStorageConnection`, `JWT_SECRET`
- CI/CD: GitHub Actions with `Azure/static-web-apps-deploy@v1`

---

## Dependencies

### NuGet (Core)
- `BCrypt.Net-Next`
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.IdentityModel.Tokens`

### NuGet (Infrastructure)
- `Azure.Data.Tables`

### NuGet (Tests)
- `xunit`
- `Moq`
- `Microsoft.NET.Test.Sdk`
- `xunit.runner.visualstudio`

### Toolchain
- .NET 8 SDK
- Node.js 18+
- Azure Functions Core Tools v4
- SWA CLI
- Azurite (local Table Storage emulator)

---

## Local Development

```bash
# Start storage emulator
azurite --silent --location /tmp/azurite

# Start full stack (frontend dev server + API)
swa start src/frontend --api-location src/api
# Available at http://localhost:4280
```

`src/api/local.settings.json` (gitignored):
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "TableStorageConnection": "UseDevelopmentStorage=true",
    "JWT_SECRET": "local-dev-secret-at-least-32-characters-long!!"
  }
}
```

---

## Implementation Order

1. **Scaffolding**: Create Next.js app, Ez.Reasons.Core, Ez.Reasons.Infrastructure, Ez.Reasons.Api, Ez.Reasons.Core.Tests. Configure static export, add dependencies, update `.gitignore`, create `staticwebapp.config.json`.
2. **Core models + interfaces**: Domain models (`Letter` with ViewCount/LikeCount/DislikeCount, `User`), DTOs, repository interfaces, service interfaces.
3. **Core services**: `LetterService` (including weighted selection and seen-list exclusion) and `AuthService`. Depend only on repository interfaces.
4. **Core unit tests**: Test services with mocked repositories. Cover: selection with scores, seen-list exclusion, fallback when all seen, feedback increment, approval/rejection.
5. **Infrastructure**: Table Storage entities (with counter fields), entity mappers, `TableLetterRepository` (with atomic increment operations), `TableUserRepository`.
6. **API functions**: All endpoints including `/api/letters/next`, `/api/letters/{id}/feedback`. DI wiring in `Program.cs`, `JwtMiddleware`. Functions are thin — parse request, call service, return response.
7. **Frontend public pages**: Root layout, Navbar, Footer, Home (with seen tracking in localStorage), About, Submit.
8. **Frontend feedback + auth + moderation**: FeedbackPrompt, AuthProvider, Login, Dashboard.
9. **Local dev setup**: SWA CLI config, seed script, end-to-end test with Azurite.

---

## Design Decisions

- **Partition key = letter status**: Makes the two most common queries efficient (all approved, all pending). Trade-off: approve/reject requires cross-partition delete+insert.
- **Insert-first on status change**: Insert into new partition before deleting from old. If delete fails, a temporary duplicate exists (recoverable). If insert-first is reversed, a failed delete loses the letter permanently.
- **Weighted random selection**: Quality score (likes - dislikes) determines selection probability. Higher-scored letters are shown more often but lower-scored letters are not excluded entirely. Selection logic lives in Core, not Infrastructure.
- **Seen tracking in localStorage**: Per-browser, not per-user. Resets if cleared or on a new device. Acceptable trade-off given no user accounts. The API does the filtering; the frontend just stores and sends IDs.
- **POST for letter selection**: The next-letter endpoint is POST rather than GET because the client sends a seen-ID list in the body. This avoids URL length limits with many seen IDs.
- **View count on letter entity**: A single `ViewCount` field incremented per display. Simpler than a separate event log. Sufficient for scoring and basic analytics.
- **Counter increments**: `ViewCount`, `LikeCount`, `DislikeCount` are incremented via read-modify-write on the entity. At v1 scale (low concurrency), ETag conflicts are rare. If a conflict occurs, a single retry is sufficient.
- **Three-project backend split**: Core has zero infrastructure dependencies. Services are testable by mocking repository interfaces. Infrastructure is swappable without touching business logic.
