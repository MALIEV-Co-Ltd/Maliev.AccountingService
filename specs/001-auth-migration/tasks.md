# Tasks: Permission-Based Authorization Migration

**Feature Branch**: `001-auth-migration`
**Spec**: [spec.md](spec.md)
**Plan**: [plan.md](plan.md)

## Implementation Strategy

- **Incremental Delivery**: Start with the foundational data layer (Permissions/Roles), then the Authorization Infrastructure, and finally apply protection to endpoints per User Story.
- **Code-First Seeding**: All permissions and roles are defined in C# code and applied via EF Core Migrations.
- **Opt-in Protection**: Endpoints will be protected one by one, ensuring no breakage of existing functionality until explicitly switched over.
- **Test-First**: Integration tests will be written to verify the authorization logic before it's applied to critical paths.

## Phase 1: Setup

*Goal: Initialize project structure and dependencies for authorization.*

- [x] T001 Install `Microsoft.AspNetCore.Authentication.JwtBearer` in `Maliev.AccountingService.Api/Maliev.AccountingService.Api.csproj`
- [x] T002 Install `StackExchange.Redis` in `Maliev.AccountingService.Api/Maliev.AccountingService.Api.csproj` (if not present)
- [x] T003 [P] Create `contracts/permissions-api.yaml` with OpenAPI definition for `/permissions` and `/roles`

## Phase 2: Foundational (Blocking Prerequisites)

*Goal: Establish the data model, seeding mechanism, and core authorization services.*

### Data Layer
- [x] T004 Create `Permission` entity in `Maliev.AccountingService.Data/Models/Permission.cs`
- [x] T005 Create `Role` entity in `Maliev.AccountingService.Data/Models/Role.cs`
- [x] T006 Create `RolePermission` entity in `Maliev.AccountingService.Data/Models/RolePermission.cs`
- [x] T007 [P] Configure EF Core mappings (keys, relationships) in `Maliev.AccountingService.Data/Data/AccountingDbContext.cs`
- [x] T008 Create seed data class `AccountingPermissions.cs` in `Maliev.AccountingService.Data/Data/` with all 18 permissions
- [x] T009 Create seed data class `AccountingPredefinedRoles.cs` in `Maliev.AccountingService.Data/Data/` with 5 roles and mappings
- [x] T010 Apply seed data in `OnModelCreating` within `AccountingDbContext.cs`
- [x] T011 Generate EF Core Migration `AddPermissionsAndRoles` in `Maliev.AccountingService.Data/Migrations/`

### Authorization Infrastructure
- [x] T012 Implement `IPermissionService` and `PermissionService` in `Maliev.AccountingService.Api/Services/PermissionService.cs` (Redis caching logic)
- [x] T013 Implement `PermissionRequirement : IAuthorizationRequirement` in `Maliev.AccountingService.Api/Services/Auth/PermissionRequirement.cs`
- [x] T014 Implement `PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>` in `Maliev.AccountingService.Api/Services/Auth/PermissionAuthorizationHandler.cs`
- [x] T015 Implement `PermissionPolicyProvider : IAuthorizationPolicyProvider` in `Maliev.AccountingService.Api/Services/Auth/PermissionPolicyProvider.cs`
- [x] T016 Register Auth services (`AddSingleton<IAuthorizationPolicyProvider, ...>`, `AddScoped<IAuthorizationHandler, ...>`) in `Maliev.AccountingService.Api/Program.cs`

## Phase 3: User Story 1 - Permission Registration & Visibility

*Goal: Ensure permissions/roles are visible via API (Admins).*
*Story: [US1] Permission Registration & [US2] Role Definition*

- [x] T017 [US1] Create `PermissionsController` in `Maliev.AccountingService.Api/Controllers/PermissionsController.cs`
- [x] T018 [US1] Implement `GET /permissions` endpoint in `PermissionsController.cs` (Use `IPermissionService` or DbContext)
- [x] T019 [US2] Implement `GET /roles` endpoint in `PermissionsController.cs`
- [x] T020 [P] [US1] Write Integration Test `GetPermissions_ReturnsAll18Permissions` in `Maliev.AccountingService.Tests/Integration/PermissionTests.cs` (Merged into AuthZTests.cs)
- [x] T021 [P] [US2] Write Integration Test `GetRoles_ReturnsAll5Roles` in `Maliev.AccountingService.Tests/Integration/RoleTests.cs` (Merged into AuthZTests.cs)

## Phase 4: User Story 3 - Critical Permission Flagging

*Goal: Verify critical permissions are identifiable.*
*Story: [US3] Critical Permission Flagging*

- [x] T022 [US3] Verify `IsCritical` property is correctly populated in `GET /permissions` output (Update `PermissionDto` if needed)
- [x] T023 [P] [US3] Write Integration Test `GetPermissions_CriticalFlagsAreCorrect` in `Maliev.AccountingService.Tests/Integration/PermissionTests.cs` (Merged into AuthZTests.cs)

## Phase 5: Authorization Enforcement (Apply to Endpoints)

*Goal: Protect actual resources based on permission requirements.*
*Stories: [US1], [US2] (Enforcement aspect)*

### Journal Entries
- [x] T024 [US1] Protect `JournalEntriesController` Create action with `[Authorize(Policy = "accounting.journal-entries.create")]`
- [x] T025 [US1] Protect `JournalEntriesController` Read actions with `[Authorize(Policy = "accounting.journal-entries.read")]`
- [x] T026 [US1] Protect `JournalEntriesController` Post action with `[Authorize(Policy = "accounting.journal-entries.post")]`
- [x] T027 [US1] Protect `JournalEntriesController` Reverse action with `[Authorize(Policy = "accounting.journal-entries.reverse")]`

### Accounts
- [x] T028 [US2] Protect `AccountsController` endpoints with respective `accounting.accounts.*` policies

### Reports
- [x] T029 [US2] Protect `ReportsController` endpoints with respective `accounting.reports.*` policies

### Periods
- [x] T030 [US2] Protect `PeriodsController` endpoints with respective `accounting.periods.*` policies

### Verification
- [x] T031 [US1] Write Integration Test `CallProtectedEndpoint_WithCorrectPermission_Succeeds` in `Maliev.AccountingService.Tests/Integration/AuthZTests.cs`
- [x] T032 [US1] Write Integration Test `CallProtectedEndpoint_WithoutPermission_ReturnsForbidden` in `Maliev.AccountingService.Tests/Integration/AuthZTests.cs`

## Phase 6: Polish & Cross-Cutting

*Goal: Final cleanup, documentation, and ensuring non-functional requirements.*

- [x] T033 Update `quickstart.md` with final testing instructions
- [x] T034 Verify Redis cache hit/miss logging in `PermissionService.cs`
- [x] T035 Ensure all new code has XML comments for Swagger documentation
- [x] T036 Run full test suite and ensure 100% pass rate

## Dependencies

1. **Setup (Phase 1)** -> **Foundational (Phase 2)**
2. **Foundational (Phase 2)** -> **Phase 3 (Registration)** & **Phase 5 (Enforcement)** (Can start Phase 3 and 5 in parallel once Phase 2 is done, but Phase 5 depends on Controller existence)
3. **Phase 3** -> **Phase 4 (Critical Flags)**

## Parallel Execution Opportunities

- **T017, T018, T019 (Controllers)** can be built while **T020, T021 (Tests)** are being written.
- **T024-T030 (Endpoint Protection)** can be distributed among developers once the policies (Phase 2) are available.
- **T003 (Contract)** can be done independently at any time before Phase 3.