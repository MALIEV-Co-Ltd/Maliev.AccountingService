# Implementation Plan: Permission-Based Authorization Migration

**Branch**: `001-auth-migration` | **Date**: 2024-12-21 | **Spec**: [specs/001-auth-migration/spec.md](spec.md)
**Input**: Feature specification from `/specs/001-auth-migration/spec.md`

## Summary

Implement a granular, permission-based authorization system to replace/augment existing role checks.
**Approach**:
- **Data**: Code-First seeding of `Permissions` and `Roles` using EF Core Migrations.
- **AuthZ**: Custom `IAuthorizationPolicyProvider` and `AuthorizationHandler` to dynamically enforce `[Authorize(Policy = "permission.code")]`.
- **Performance**: Redis caching (`IDistributedCache`) for Permission-to-Role lookups.
- **API**: Opt-in protection of endpoints.

## Technical Context

**Language/Version**: C# (.NET 10)
**Primary Dependencies**: `MassTransit`, `EntityFrameworkCore` (Npgsql), `StackExchange.Redis`, `Microsoft.AspNetCore.Authentication.JwtBearer`
**Storage**: PostgreSQL (Permissions, Roles), Redis (AuthZ Cache)
**Testing**: xUnit, Testcontainers (Postgres, Redis)
**Target Platform**: Linux (Docker)
**Project Type**: Single Microservice API
**Performance Goals**: Low latency authorization checks (<5ms added latency via Redis)
**Constraints**: Must adhere to Maliev Constitution (Testcontainers, No InMemory DB)
**Scale/Scope**: ~18 Permissions, 5 Roles

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Service Autonomy**: New tables owned by this service.
- [x] **Explicit Contracts**: OpenAPI updated for new endpoints.
- [x] **Test-First Development**: Integration tests planned.
- [x] **Real Infrastructure Testing**: Testcontainers for PG/Redis.
- [x] **Auditability**: Standard logging.
- [x] **Security**: JWT used; Permissions granular.
- [x] **Secrets Management**: No hardcoded secrets.
- [x] **Zero Warnings**: Will ensure build is clean.
- [x] **Clean Project Artifacts**: Gitignore utilized.
- [x] **Docker Best Practices**: Standard Dockerfile usage.
- [x] **Simplicity**: No complex IAM service; code-first seeding.
- [x] **Business Metrics**: Will expose auth metrics (success/fail).
- [x] **Aspire Integration**: Local Project Reference accepted; CI handles switch to NuGet (User-defined Override).
- [x] **Code Quality**: No AutoMapper/FluentValidation.
- [x] **Project Structure**: Flat structure.

## Project Structure

### Documentation (this feature)

```text
specs/001-auth-migration/
├── plan.md              # This file
├── research.md          # Technical decisions
├── data-model.md        # DB Schema
├── quickstart.md        # Usage guide
├── contracts/           # API Specs
│   └── permissions-api.yaml
└── tasks.md             # Tasks (to be generated)
```

### Source Code (repository root)

```text
Maliev.AccountingService.Api/
├── Controllers/         # Updated with [Authorize]
├── Services/            # PermissionService, AuthHandlers
└── ...

Maliev.AccountingService.Data/
├── Models/              # Permission, Role, RolePermission
├── Data/                # DbContext updates, Seeds
└── Migrations/          # EF Migrations

Maliev.AccountingService.Tests/
├── Integration/         # AuthZ Tests
└── ...
```

**Structure Decision**: Standard .NET Microservice layout.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Custom Auth Policy Provider | Dynamic permissions without 100s of `options.AddPolicy` lines | Hardcoding policies in `Program.cs` is unmaintainable |
| Many-to-Many Join Table | Roles have multiple permissions | Simple 1:N not sufficient for RBAC |

## Phases

### Phase 1: Data Layer & Seeding
- Define `Permission`, `Role`, `RolePermission` entities.
- Configure EF Core relationships.
- Create Seed Data (`AccountingPermissions.cs`, `AccountingPredefinedRoles.cs`).
- Generate and Apply EF Core Migration.
- Create `IPermissionRepository` (or use DbContext directly if simple).

### Phase 2: Authorization Infrastructure
- Implement `PermissionService` (caches Role->Permissions in Redis).
- Implement `PermissionRequirement`.
- Implement `PermissionAuthorizationHandler` (checks JWT Role against cached Permissions).
- Implement `PermissionPolicyProvider` (creates policies on the fly).
- Register services in `Program.cs`.

### Phase 3: API & Enforcement
- Create `PermissionsController` (GET /permissions).
- Update existing Controllers with `[Authorize(Policy = "...")]`.
- **Critical**: Ensure `[AllowAnonymous]` is used where needed.

### Phase 4: Testing & Verification
- Update `IntegrationTestFixture` to seed permissions/roles in Testcontainers.
- Write Integration Tests:
    - `Given_UserWithPermission_When_CallProtectedEndpoint_Then_Success`
    - `Given_UserWithoutPermission_When_CallProtectedEndpoint_Then_Forbidden`
    - `Given_UserWithRole_When_CallProtectedEndpoint_Then_Success`
- Verify Redis caching behavior (hit/miss).

### Phase 5: Deployment & Documentation
- Verify Docker build.
- Update Swagger/OpenAPI.
- Finalize `quickstart.md`.