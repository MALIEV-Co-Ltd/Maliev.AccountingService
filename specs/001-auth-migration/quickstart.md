# Quickstart: Permission-Based Authorization

## Prerequisites

- Redis running (local or Docker)
- PostgreSQL running

## 1. Adding a New Permission

1.  Open `Maliev.AccountingService.Data/Data/AccountingPermissions.cs` (or similar seed file).
2.  Add a new `Permission` object to the seed list.
3.  Add the permission to a `Role` in `AccountingPredefinedRoles.cs`.
4.  Run `dotnet ef migrations add AddNewPermission`.
5.  Run `dotnet ef database update`.

## 2. Protecting an Endpoint

Decorate your controller or action with the `Authorize` attribute using the permission code as the policy name:

```csharp
[Authorize(Policy = "accounting.journal-entries.post")]
[HttpPost]
public async Task<IActionResult> CreateJournalEntry(...)
{
    // ...
}
```

## 3. Testing

### Unit/Integration Tests
Run the test suite:
```bash
dotnet test
```

### Manual Testing
1. Obtain a JWT with a specific `role` claim (e.g., `role: accounting-viewer`).
2. Call a protected endpoint (e.g., `POST /journal-entries`).
3. Verify you receive `403 Forbidden`.
4. Obtain a JWT with `role: accounting-manager`.
5. Call the same endpoint.
6. Verify you receive `200 OK` (or `201 Created`).

## 4. Troubleshooting

- **403 Forbidden when expected 200**: Check Redis or DB to ensure the Role has the Permission. Check JWT to ensure User has the Role.
- **500 Internal Server Error**: Check Redis connectivity.
