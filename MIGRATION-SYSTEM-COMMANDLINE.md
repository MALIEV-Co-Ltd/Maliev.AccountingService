# Migration from System.CommandLine to REST API

## Summary

Successfully migrated the bulk import functionality from a CLI-based approach using `System.CommandLine` to a proper REST API, making the service fully HTTP-based and suitable for Linux/Kubernetes deployment.

## Changes Made

### 1. **Removed CLI Dependencies**
- ❌ Removed `System.CommandLine` package from `Maliev.AccountingService.Api.csproj`
- ❌ Deleted `Commands/BulkImportCommand.cs` (was never hooked up in Program.cs anyway)
- ❌ Deleted `Tests/Unit/BulkImportCommandTests.cs`

### 2. **Created Service Layer**
- ✅ Created `Services/IBulkImportService.cs` - Service interface
- ✅ Created `Services/BulkImportService.cs` - Service implementation with all parsing/validation logic
- ✅ Created `Models/BulkImportModels.cs` - DTOs for requests/responses

**Key Features:**
- Pure C# CSV parsing using `CsvHelper`
- Pure C# JSON parsing using `System.Text.Json`
- Comprehensive validation logic
- Dry-run mode support
- Detailed error reporting

### 3. **Created REST API Endpoints**
- ✅ Created `Controllers/BulkImportController.cs`
- ✅ Registered service in `Program.cs`

**Endpoints:**
- `POST /accounting/v1/bulk-import/chart-of-accounts` - Import chart of accounts
- `POST /accounting/v1/bulk-import/opening-balances` - Import opening balances

**Features:**
- File upload via `multipart/form-data`
- Supports both CSV and JSON formats
- Query parameter `?dryRun=true` for validation-only mode
- Requires authentication (JWT)
- Returns detailed import results with statistics and errors

## API Usage Examples

### Import Chart of Accounts (CSV)

```bash
curl -X POST "https://api.maliev.com/accounting/v1/bulk-import/chart-of-accounts" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@accounts.csv"
```

### Dry Run (Validation Only)

```bash
curl -X POST "https://api.maliev.com/accounting/v1/bulk-import/chart-of-accounts?dryRun=true" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@accounts.csv"
```

### Import Opening Balances (JSON)

```bash
curl -X POST "https://api.maliev.com/accounting/v1/bulk-import/opening-balances" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@balances.json"
```

## Response Format

```json
{
  "success": true,
  "totalRecords": 150,
  "importedRecords": 145,
  "skippedRecords": 5,
  "errors": [],
  "warnings": [
    "Skipping 1000 (already exists)"
  ],
  "summary": "Imported 145 accounts, skipped 5 existing accounts"
}
```

## File Format Specifications

### CSV Format (Chart of Accounts)
```csv
AccountNumber,Name,Description,Type,Category,IsActive
1000,Cash,"Cash on hand",Asset,Current,true
1100,Accounts Receivable,"Customer balances",Asset,Current,true
```

### JSON Format (Chart of Accounts)
```json
[
  {
    "accountNumber": "1000",
    "name": "Cash",
    "description": "Cash on hand",
    "type": "Asset",
    "category": "Current",
    "isActive": true
  }
]
```

### CSV Format (Opening Balances)
```csv
AccountNumber,DebitAmount,CreditAmount,Description
1000,50000.00,0.00,Opening cash balance
2000,0.00,50000.00,Opening capital
```

## Why This Change?

1. **Linux Compatibility**: REST APIs work perfectly in containerized Linux environments
2. **Microservice Architecture**: HTTP endpoints integrate seamlessly with other services
3. **Better Tooling**: Can be tested with Postman, curl, or any HTTP client
4. **Security**: Proper authentication/authorization using JWT tokens
5. **Observability**: Automatic logging, tracing, and metrics via ServiceDefaults
6. **No Dead Code**: The old CLI command was never hooked up in Program.cs

## Migration Notes

- All parsing/validation logic remains **pure C#** code
- No external command-line tools are used
- CSV parsing: `CsvHelper` library
- JSON parsing: `System.Text.Json` library
- File uploads handled via ASP.NET Core's `IFormFile`
- Streams are used for efficient memory usage with large files

## Testing

Build successful:
```bash
cd Maliev.AccountingService
dotnet build Maliev.AccountingService.Api
# Build succeeded. 0 Warning(s) 0 Error(s)
```

All references to `System.CommandLine` have been removed from source code.
