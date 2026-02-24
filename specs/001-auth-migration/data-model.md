# Data Model: Permission-Based Authorization

## Entities

### Permission
Represents a granular action within the system.

| Field | Type | Required | Key | Description |
|-------|------|----------|-----|-------------|
| Code | string (100) | Yes | PK | Unique identifier (e.g., `accounting.journal-entries.post`) |
| Description | string (255) | Yes | | Human-readable description |
| IsCritical | bool | Yes | | Flag for high-impact permissions |

### Role
Represents a predefined set of permissions.

| Field | Type | Required | Key | Description |
|-------|------|----------|-----|-------------|
| Name | string (50) | Yes | PK | Unique identifier (e.g., `accounting-manager`) |
| Description | string (255) | Yes | | Human-readable description |

### RolePermission
Join table for Many-to-Many relationship between Role and Permission.

| Field | Type | Required | Key | Description |
|-------|------|----------|-----|-------------|
| RoleName | string (50) | Yes | PK, FK | Reference to `Role.Name` |
| PermissionCode | string (100) | Yes | PK, FK | Reference to `Permission.Code` |

## EF Core Configuration

- **Composite Key** for `RolePermission`: `HasKey(rp => new { rp.RoleName, rp.PermissionCode })`
- **Seeding**: Use `HasData` in `IEntityTypeConfiguration` classes for each entity.

## Caching Model (Redis)

- **Key**: `auth:perm:{PermissionCode}`
- **Value**: `List<string>` (List of Role Names authorized for this permission)
- **TTL**: 30 minutes
