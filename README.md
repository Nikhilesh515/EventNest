# EventNest Backend (.NET)

EventNest is a full-stack event-planning application. This repository contains
the backend: four ASP.NET Core microservices, a YARP API gateway, shared gRPC
contracts, and a PostgreSQL server hosting one database per service.

## About the Project

- **Event lifecycle** — create, edit, publish, cancel, and complete events.
  Drafts are private to their organizer and admins.
- **RSVPs** — Going / Maybe / Not Going / Cancelled with capacity enforcement
  and one active RSVP per user.
- **Tags** — a shared tag catalog with colors, attached to events.
- **Accounts and sessions** — registration and login issuing short-lived JWTs
  plus rotating refresh tokens in an HttpOnly cookie.
- **RBAC** — a 15-permission catalog, five built-in roles, custom roles, and
  per-user permission grants.
- **Administration** — user directory with server-side paging/search, role
  assignment, and deactivation.

**Tech stack:** .NET 10 · ASP.NET Core (controllers + gRPC) · EF Core 10 with
Npgsql · PostgreSQL 15 · Redis 7 (optional cache) · RabbitMQ (provisioned, not
wired) · YARP reverse proxy · BCrypt password hashing.

## Architecture

### Services and ports

| Service | REST | gRPC | Database |
|---|---|---|---|
| API Gateway (YARP) | 5000 | — | — |
| Auth Service | 5001 | 51001 | `eventnest_auth` |
| Event Service | 5002 | 51002 | `eventnest_event` |
| Tag Service | 5003 | 51003 | `eventnest_tag` |
| RSVP Service | 5004 | 51004 | `eventnest_rsvp` |

gRPC ports follow the convention `REST + 10000`. Kestrel listens on these ports
explicitly in each `Program.cs`, so the `applicationUrl` values in
`launchSettings.json` do not apply at runtime.

### Inter-service gRPC

| Caller | Callee | Purpose |
|---|---|---|
| Event | Auth | Resolve organizer display names |
| Event | Tag | Resolve tag names/colors; validate tag ids |
| Event | RSVP | Batched going/maybe counts for event listings |
| RSVP | Event | Validate event status/capacity; enrich RSVP details |

gRPC clients degrade gracefully when a dependency is unavailable (for example,
an unknown organizer falls back to the name from the access token).

### API gateway routes

The gateway proxies `/api/**` to four clusters. Route-level authorization is
enforced before forwarding; service controllers enforce the fine-grained
permissions listed in the API reference.

| Route | Path | Policy |
|---|---|---|
| auth-route | `/api/auth/**` | anonymous |
| auth-user-route | `/api/users/**` | authenticated |
| roles-route | `/api/roles/**` | authenticated |
| permissions-route | `/api/permissions/**` | authenticated |
| event-route | `/api/events/**` | anonymous |
| tag-route | `/api/tags/**` | anonymous |
| rsvp-route | `/api/events/{eventId}/rsvps/**` | authenticated |
| rsvp-user-route | `/api/users/{userId}/rsvps/**` | authenticated |
| rsvp-id-route | `/api/rsvps/{id}` | authenticated |

## Engineering Decisions

### Microservices per bounded context
Auth, events, tags, and RSVPs are separate deployables with their own
databases. Services never share tables; cross-domain data is fetched through
gRPC or stored as denormalized snapshots (organizer name, tag name).

### One PostgreSQL server, one database per service
A single PostgreSQL instance hosts `eventnest_auth`, `eventnest_event`,
`eventnest_tag`, and `eventnest_rsvp`. `docker/postgres/init.sql` creates the
databases on first boot; each service owns its schema through EF Core
migrations.

### Synchronous gRPC for service-to-service calls
Internal calls use shared protobuf contracts under
`services/EventNest.Shared/EventNest.Shared.Infrastructure/Proto/`.

### YARP gateway as the single entry point
The UI talks only to the gateway (port 5000). The gateway handles routing, JWT
authentication for protected routes, CORS, per-IP rate limiting, and aggregated
health checks.

### JWT access tokens with HttpOnly refresh cookies
Access tokens are 60-minute HS256 JWTs carrying identity claims only —
permissions are never embedded. Refresh tokens live in an HttpOnly,
SameSite=Lax cookie scoped to `/api/auth`, so browser scripts never see them.

### Refresh rotation with reuse detection
Every refresh rotates the token and stores only a SHA-256 hash at rest. A 30 s
grace window allows multi-tab refreshes; reuse of an already-rotated token
revokes all sessions for that user.

### Database-driven RBAC with a Redis-backed cache
Roles and permissions live in `role_permissions` and can change at runtime.
Effective permissions are cached per user (`user:{id}:permissions`, 5-minute
TTL) and re-warmed on role/permission changes to avoid lock-out windows.

### Migrations and seeds on startup
Each API calls `MigrateAndSeed()` before serving traffic. Seeds are idempotent:
five roles, the default admin, five tags, and three demo events.

### Uniform response envelope
Every service returns `{ code, success, message, result, errors }`. Controllers
throw domain exceptions that shared middleware maps to HTTP status codes
(400/401/403/404/409).

### Rate limiting at the gateway
Per-IP sliding windows: 100 req/min anonymous and 300 req/min authenticated,
with `X-RateLimit-*` headers and `Retry-After` on 429.

## API Reference

Base URL: the gateway, `http://localhost:5000` (in the Docker bundle it is also
reachable same-origin at `/api` through the UI's nginx proxy).

Responses use the envelope above; `204` responses have no body. Gateway-level
failures (rate limit, no healthy destination) use
`{ "error": { "code": ..., "message": ... } }` instead.

Status codes: `200/201/204` success · `400` validation · `401` unauthenticated ·
`403` permission denied · `404` not found · `409` conflict · `429` rate limited ·
`500` unhandled.

### Auth

| Method | Path | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | anonymous | Create an account; returns tokens and sets the refresh cookie |
| POST | `/api/auth/login` | anonymous | Authenticate with email and password |
| POST | `/api/auth/refresh` | refresh cookie | Rotate the session; a disallowed `Origin` is rejected with 403 |
| POST | `/api/auth/logout` | refresh cookie | Revoke the refresh token and clear the cookie (idempotent) |

### Users

| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/api/users/me` | authenticated | Current user profile (database-backed) |
| GET | `/api/users` | `Users.View` | Paged directory: `page`, `pageSize` (1–100), `search`, `role` (role id); active users only |
| GET | `/api/users/{id}` | `Users.View` | User by id |
| POST | `/api/users` | `Users.Manage` | Create a user and assign a role |
| PUT | `/api/users/{id}` | `Users.Manage` | Update display name |
| DELETE | `/api/users/{id}` | `Users.Manage` | Deactivate a user (one-way) |
| PUT | `/api/users/{id}/role` | `Users.Manage` | Assign a role; re-warms the permission cache |

### Roles

| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/api/roles` | `Users.View` | List roles with user counts and permission names |
| GET | `/api/roles/{id}` | `Users.View` | Role by id |
| POST | `/api/roles` | `Users.Manage` | Create a custom role with permissions |
| PUT | `/api/roles/{id}` | `Users.Manage` | Update a role (name is immutable) and replace its permissions |
| DELETE | `/api/roles/{id}` | `Users.Manage` | Delete a custom role; built-in roles and roles in use return 409 |

### Permissions

| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/api/permissions` | authenticated | Full permission catalog |
| GET | `/api/permissions/user/{userId}` | `Users.View` | Effective permissions (`source`: role-default / direct-grant) |
| POST | `/api/permissions/grant` | `Users.Manage` | Grant a permission (optional expiry) |
| POST | `/api/permissions/revoke` | `Users.Manage` | Revoke a direct grant |
| GET | `/api/permissions/check` | `Users.View` | Check whether a user holds a permission |

### Events

| Method | Path | Access | Description |
|---|---|---|---|
| GET | `/api/events` | anonymous | Paged list: `search`, `tagId[]`, `visibility`, `status`, `timeframe`, `sort`. Published events only unless the caller may manage events |
| GET | `/api/events/{id}` | anonymous | Event detail. Non-published events are visible only to their organizer, Admin, or SuperAdmin — others receive 404 |
| GET | `/api/events/my` | authenticated | The caller's events, including drafts |
| POST | `/api/events` | `Events.Create` | Create an event (draft by default); tags resolved via Tag gRPC; duplicate titles return 409 |
| PUT | `/api/events/{id}` | `Events.Edit` + owner | Update an event |
| DELETE | `/api/events/{id}` | `Events.Delete` + owner | Delete an event |
| PUT | `/api/events/{id}/publish` | `Events.Edit` + owner | Draft → Published |
| PUT | `/api/events/{id}/cancel` | `Events.Edit` + owner | Cancel an event |
| PUT | `/api/events/{id}/complete` | `Events.Edit` + owner | Mark an event completed |

### Tags

| Method | Path | Permission | Description |
|---|---|---|---|
| GET | `/api/tags` | anonymous | List all tags |
| GET | `/api/tags/{id}` | anonymous | Tag by id |
| POST | `/api/tags` | `Tags.Create` | Create a tag (unique name, hex color) |
| PUT | `/api/tags/{id}` | `Tags.Edit` | Update a tag |
| DELETE | `/api/tags/{id}` | `Tags.Delete` | Delete a tag |

### RSVPs

| Method | Path | Permission | Description |
|---|---|---|---|
| POST | `/api/events/{eventId}/rsvps` | `RSVPs.Create` | RSVP to a published event (`guestCount`, optional `notes`) |
| DELETE | `/api/events/{eventId}/rsvps` | `RSVPs.Cancel` | Cancel the caller's RSVP |
| GET | `/api/events/{eventId}/rsvps` | `RSVPs.Manage` | List an event's RSVPs (organizer) |
| GET | `/api/rsvps/{id}` | `RSVPs.View` | RSVP detail (ownership violations return 401 by contract) |
| PUT | `/api/rsvps/{id}` | `RSVPs.Edit` | Update status, guest count, or notes (owner only) |
| GET | `/api/users/{userId}/rsvps` | `RSVPs.View` | List a user's RSVPs |

### Health and docs

- `GET /health` — every service; the gateway aggregates all four checks.
- `/swagger` — each service, Development environment only.

### Permissions catalog and built-in roles

15 permissions across four groups:

| Group | Permissions |
|---|---|
| Events | `Events.View`, `Events.Create`, `Events.Edit`, `Events.Delete` |
| Tags | `Tags.View`, `Tags.Create`, `Tags.Edit`, `Tags.Delete` |
| RSVPs | `RSVPs.View`, `RSVPs.Create`, `RSVPs.Edit`, `RSVPs.Manage`, `RSVPs.Cancel` |
| Users | `Users.View`, `Users.Manage` |

Built-in roles seed these permission sets:

| Role | Permissions |
|---|---|
| User | Events.View, Tags.View, RSVPs.View/Create/Edit/Cancel |
| Organizer | User + Events.Create/Edit, Tags.Create, RSVPs.Manage |
| Moderator | Organizer + Users.View |
| Admin | All 15 |
| SuperAdmin | All 15 |

Permission changes are re-warmed immediately; other clients may observe them
within the 5-minute cache TTL.

## Data and Seed Data

Four databases, one EF Core `DbContext` per service:

| Database | Key tables |
|---|---|
| `eventnest_auth` | `users`, `roles`, `role_permissions`, `permission_grants`, `refresh_tokens` |
| `eventnest_event` | `events`, `event_tags` |
| `eventnest_tag` | `tags` |
| `eventnest_rsvp` | `rsvps` |

Seeded on first startup (idempotent):

- Admin `admin@eventnest.io` / `Admin@123` with deterministic id
  `5eed0000-0000-4000-8000-000000000001`
- Five roles with the permission sets above
- Five tags: Technology, Music, Food & Drink, Sports, Networking
- Three demo events owned by the admin: Tech Meetup 2026 (draft), Food Festival
  (published), Music Concert (draft)

## Configuration

Settings are overridden with environment variables (ASP.NET Core maps `__` to
`:`, so `Jwt__SecretKey` sets `Jwt:SecretKey`).

| Variable | Config key | Default |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | — | `Production` (compose) / `Development` (launch profiles) |
| `JWT_SECRET_KEY` | `Jwt:SecretKey` | Placeholder in appsettings; must be at least 32 characters and not the placeholder outside Development |
| `JWT_ISSUER` / `JWT_AUDIENCE` | `Jwt:Issuer` / `Jwt:Audience` | `EventNest.AuthService` / `EventNest` |
| `JWT_ACCESS_EXPIRY` / `JWT_REFRESH_EXPIRY` | `Jwt:AccessTokenExpiryMinutes` / `Jwt:RefreshTokenExpiryDays` | `60` / `30` |
| — | `Jwt:RefreshTokenCleanupIntervalHours` | `12` |
| — | `Auth:RefreshRotationGraceSeconds` | `30` |
| `COOKIE_SECURE` | `Cookie:Secure` | `true` in code; the Docker bundle defaults it to `false` |
| `POSTGRES_USER` / `POSTGRES_PASSWORD` | — | `postgres` / `postgres` (compose only) |
| `ConnectionStrings__AuthDb` and siblings | `ConnectionStrings:*Db` | `Host=postgres;...;Database=eventnest_{auth,event,tag,rsvp}` in Docker |
| `ConnectionStrings__Redis` | `ConnectionStrings:Redis` | `redis:6379` in Docker; empty selects the in-memory cache |
| `Services__Auth__GrpcUrl` | `Services:Auth:GrpcUrl` | `http://auth-service:51001` in Docker |
| `Services__Tag__GrpcUrl` / `Services__Rsvp__GrpcUrl` / `Services__Event__GrpcUrl` | `Services:*:GrpcUrl` | `http://{tag,rsvp,event}-service:5100{3,4,2}` in Docker |
| `RABBITMQ_USER` / `RABBITMQ_PASS` | `RabbitMQ:Username` / `RabbitMQ:Password` | `guest` / `guest` |
| `RabbitMQ__Host` / `RabbitMQ__Port` | `RabbitMQ:Host` / `RabbitMQ:Port` | `rabbitmq` / `5672` in Docker |
| `CORS_ALLOWED_ORIGIN_0/1` | `Cors:AllowedOrigins` | `http://localhost:5173`, `https://localhost:5173` |
| `HealthChecks__*Service` | `HealthChecks:*` | Gateway check URLs; `http://{service}:500x/health` in Docker |
| `ReverseProxy__Clusters__*__Destinations__*__Address` | `ReverseProxy:Clusters:...` | Gateway destinations; Docker service names |

## Local Development

Prerequisites: .NET 10 SDK, PostgreSQL 15 on `localhost:5432`, optionally
Redis 7 (without it, services use an in-memory cache).

1. Create the four databases (or run them from `docker/postgres/init.sql`):
   `eventnest_auth`, `eventnest_event`, `eventnest_tag`, `eventnest_rsvp`.
2. Adjust the connection strings in each service's
   `appsettings.Development.json` to match your local PostgreSQL credentials.
3. Run the services in separate terminals (order is not critical; the gateway
   last is convenient):

```powershell
dotnet run --project services/EventNest.AuthService/EventNest.AuthService.API
dotnet run --project services/EventNest.TagService/EventNest.TagService.API
dotnet run --project services/EventNest.EventService/EventNest.EventService.API
dotnet run --project services/EventNest.RSVPService/EventNest.RSVPService.API
dotnet run --project services/EventNest.Gateway
```

4. Verify:

```powershell
curl.exe http://localhost:5000/health
curl.exe http://localhost:5000/api/events
```

Each service applies migrations and seeds on startup. Swagger is available at
`http://localhost:{5001..5004}/swagger/index.html` in Development.

## Docker

The recommended path is the compose bundle in the `EventNest-DotNetRec`
repository — it starts all services, infrastructure, and the UI with one
command (`docker compose up --build -d`). See that repository's README for
ports, credentials, and troubleshooting.

This repository also contains a backend-only `docker-compose.yml` that
publishes every service port (5000–5004, 51001–51004, 5432, 6379, 5672/15672).
Don't run both stacks at once — they both bind port 5000.

## Testing

There is no committed automated test harness yet (the xUnit harness milestone
is deferred). Verification currently uses:

- per-milestone test plans and the PowerShell E2E suite maintained in the
  project documentation (`0005-milestone-e2e-testing/run-e2e-tests.ps1`)
- health endpoints and Swagger for manual checks

## Business Assumptions

- Events are free: no payments, ticketing, or seat selection.
- One active RSVP per user per event; cancelling and RSVPing again reactivates
  the same record. Capacity counts every non-cancelled RSVP.
- Anonymous visitors can browse published events; creating events and RSVPing
  require an account.
- The permission catalog is fixed at 15 permissions; roles and direct grants
  are managed through the API.
- No email notifications, file uploads, or realtime updates. Organizer and tag
  names are denormalized snapshots and may lag behind profile or tag changes.
