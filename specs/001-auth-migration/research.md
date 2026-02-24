# Phase 0: Research & Technical Approach

**Feature**: Permission-Based Authorization Migration
**Branch**: `001-auth-migration`

## Technical Decisions

### 1. Data Model for Permissions & Roles

**Decision**: Implement `Permission`, `Role`, and `RolePermission` entities in `Maliev.AccountingService.Data`.

**Rationale**:
- **Structured Storage**: Allows querying and management of permissions/roles.
- **EF Core Integration**: leverages existing ORM for persistence.
- **Join Table**: `RolePermission` enables many-to-many relationship.

**Schema**:
- `Permissions`: `Code` (PK, string), `Description` (string), `IsCritical` (bool).
- `Roles`: `Name` (PK, string), `Description` (string).
- `RolePermissions`: `RoleName` (FK), `PermissionCode` (FK).

### 2. Seeding Strategy

**Decision**: Use EF Core `HasData` in `OnModelCreating` (Migration-based seeding) for initial population.

**Rationale**:
- **Code-First Consistency**: Ensures permissions defined in code are reflected in DB migrations.
- **Version Control**: Migrations track changes to permissions/roles.
- **Automatic Deployment**: Applied via `dotnet ef database update` or migration bundles during deployment.

**Alternative Considered**:
- **Startup Service**: Seeding on application startup. Rejected because it can cause race conditions in scaled environments and requires distributed locking. Migrations are safer for schema/static data changes.

### 3. Authorization Enforcement Mechanism

**Decision**: Implement a custom `IAuthorizationPolicyProvider` and `AuthorizationHandler`.

**Rationale**:
- **Dynamic Policies**: Allows using `[Authorize(Policy = "PermissionCode")]` without pre-registering every single policy in `Program.cs`.
- **Granular Control**: Matches Spec `FR-015` (Opt-in/Granular).
- **Separation of Concerns**: Policy provider handles policy creation; Handler handles evaluation logic.

**Flow**:
1. Controller action decorated with `[Authorize(Policy = "accounting.journal-entries.create")]`.
2. `PermissionPolicyProvider` dynamically creates a policy requiring a "Permission" requirement with that name.
3. `PermissionAuthorizationHandler` executes:
   - Extracts User Roles from JWT Claims.
   - Checks **Redis Cache** for `Permission:accounting.journal-entries.create` -> `List<Role>`.
   - If cache miss, queries DB and updates Redis.
   - Validates if User has one of the allowed Roles.

### 4. Caching Strategy

**Decision**: Use `IDistributedCache` (StackExchange.Redis) with a `PermissionService`.

**Rationale**:
- **Performance**: Reduces DB hits on every request (Spec `FR-014`).
- **Standard Interface**: `IDistributedCache` is already registered.
- **Expiration**: Set reasonable TTL (e.g., 10-30 minutes) with cache invalidation on role updates (though role updates are Code-First/Migration based, so runtime invalidation is less critical, but good practice).

### 5. API Endpoints

**Decision**: Add `GET /permissions` endpoint.

**Rationale**:
- **User Story 1**: "When I query the permission list".
- **Visibility**: Admins need to see available permissions and roles.
- **Contract**: Return list of Permission DTOs (Code, Description, IsCritical).

### 6. Aspire Integration (Hybrid Dependency)

**Decision**: Retain `Maliev.Aspire.ServiceDefaults` as a **Project Reference** for local development.

**Rationale**:
- **Local Development**: Enables seamless debugging and iteration across the service defaults library.
- **CI Compliance**: CI workflows are responsible for dynamically swapping the Project Reference to a NuGet Package Reference to ensure build isolation and compliance with Section XIII during release builds.

## Unknowns Resolved

- **Permission Source**: Confirmed Code-First via EF Core Migrations.
- **Assignment**: Confirmed Mock/JWT Claims.
- **Caching**: Confirmed Redis via `IDistributedCache`.
