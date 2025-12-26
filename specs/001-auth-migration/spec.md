# Feature Specification: Permission-Based Authorization Migration

**Feature Branch**: `001-auth-migration`  
**Created**: 21/12/2024  
**Status**: Draft  
**Input**: User description provided in prompt.

## Clarifications

### Session 2024-12-21
- Q: Scope of implementation (Registration only vs. Full Enforcement)? → A: Full Implementation: Register permissions/roles AND apply authorization checks to API endpoints.
- Q: Permission/Role Source of Truth? → A: Code-First Seeding: Define in code and seed database on startup/migration.
- Q: User-Role Assignment Mechanism? → A: Assumed/Mock Assignment: Focus on definition/enforcement; assignment via JWT claims from Identity Provider.
- Q: Caching Strategy for Authorization Checks? → A: Distributed Cache (Redis): Use Redis for caching (recommended if scaling horizontally is planned soon).
- Q: Default Endpoint Protection Policy? → A: Opt-in (Granular): Only endpoints explicitly marked with permission requirements are protected.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Permission Registration (Priority: P1)

As a System Administrator, I need all granular permissions to be registered in the system so that I can control access to specific accounting operations.

**Why this priority**: Without registered permissions, no authorization checks can be performed. This is the foundation of the feature.

**Independent Test**: Can be tested by querying the permission registry/database to verify all 18 expected permissions exist with correct codes.

**Acceptance Scenarios**:

1. **Given** the system has started, **When** I query the permission list, **Then** I see all Journal Entry permissions (create, read, update, post, reverse).
2. **Given** the system has started, **When** I query the permission list, **Then** I see all Account operations permissions (create, read, update, close).
3. **Given** the system has started, **When** I query the permission list, **Then** I see all Financial Report permissions (balance-sheet, income-statement, cash-flow, trial-balance, export).
4. **Given** the system has started, **When** I query the permission list, **Then** I see all Period operations permissions (open, close, reopen).

---

### User Story 2 - Role Definition (Priority: P1)

As a System Administrator, I need predefined roles to be available so that I can quickly assign standard access levels to users without manually selecting permissions for every user.

**Why this priority**: Roles provide the necessary abstraction for managing user access efficiently.

**Independent Test**: Can be tested by assigning a role to a test user and verifying they inherit the expected permissions.

**Acceptance Scenarios**:

1. **Given** a user assigned the 'accounting-admin' role, **When** I check their effective permissions, **Then** they have all accounting.* permissions.
2. **Given** a user assigned the 'accounting-manager' role, **When** I check their permissions, **Then** they have all permissions EXCEPT 'periods.close' and 'periods.reopen'.
3. **Given** a user assigned the 'accounting-clerk' role, **When** I check their permissions, **Then** they ONLY have 'journal-entries.create', 'journal-entries.read', 'accounts.read', and all 'reports.*' permissions.
4. **Given** a user assigned the 'accounting-controller' role, **When** I check their permissions, **Then** they have all journal, account, period, and report permissions.
5. **Given** a user assigned the 'accounting-viewer' role, **When** I check their permissions, **Then** they ONLY have read permissions and report permissions.

---

### User Story 3 - Critical Permission Flagging (Priority: P2)

As a Security Auditor, I need critical high-impact permissions to be explicitly marked so that they can be easily identified for stricter monitoring or control.

**Why this priority**: distinguishing critical actions allows for future security enhancements like enhanced logging or 2FA, even if not fully implemented now.

**Independent Test**: Can be tested by inspecting the metadata of the 'post', 'reverse', 'close', and 'reopen' permissions.

**Acceptance Scenarios**:

1. **Given** the permission 'accounting.journal-entries.post', **When** I inspect its properties, **Then** it is marked as 'Critical'.
2. **Given** the permission 'accounting.journal-entries.reverse', **When** I inspect its properties, **Then** it is marked as 'Critical'.
3. **Given** the permission 'accounting.periods.close', **When** I inspect its properties, **Then** it is marked as 'Critical'.
4. **Given** the permission 'accounting.periods.reopen', **When** I inspect its properties, **Then** it is marked as 'Critical'.

### Edge Cases

- What happens if a predefined role name conflicts with an existing role? (Assume system prevents duplicates or updates existing).
- How does the system handle permission checks if the permission registry service is down? (Fail secure/deny access).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST register the following Journal Entry permissions: `accounting.journal-entries.create`, `accounting.journal-entries.read`, `accounting.journal-entries.update`, `accounting.journal-entries.post`, `accounting.journal-entries.reverse`.
- **FR-002**: System MUST register the following Account permissions: `accounting.accounts.create`, `accounting.accounts.read`, `accounting.accounts.update`, `accounting.accounts.close`.
- **FR-003**: System MUST register the following Financial Report permissions: `accounting.reports.balance-sheet`, `accounting.reports.income-statement`, `accounting.reports.cash-flow`, `accounting.reports.trial-balance`, `accounting.reports.export`.
- **FR-004**: System MUST register the following Period permissions: `accounting.periods.open`, `accounting.periods.close`, `accounting.periods.reopen`.
- **FR-005**: System MUST define the `accounting-admin` role containing ALL `accounting.*` permissions.
- **FR-006**: System MUST define the `accounting-manager` role containing all permissions EXCEPT `accounting.periods.close` and `accounting.periods.reopen`.
- **FR-007**: System MUST define the `accounting-clerk` role containing `accounting.journal-entries.create`, `accounting.journal-entries.read`, `accounting.accounts.read`, and all `accounting.reports.*` permissions.
- **FR-008**: System MUST define the `accounting-controller` role containing all `accounting.journal-entries.*`, `accounting.accounts.*`, `accounting.periods.*`, and `accounting.reports.*` permissions.
- **FR-009**: System MUST define the `accounting-viewer` role containing all `*.read` and `accounting.reports.*` permissions.
- **FR-010**: System MUST explicitly mark the following permissions as 'Critical': `accounting.journal-entries.post`, `accounting.journal-entries.reverse`, `accounting.periods.close`, `accounting.periods.reopen`.
- **FR-011**: System MUST enforce authorization checks on API endpoints explicitly decorated with permission requirements.
- **FR-012**: System MUST use a Code-First approach to seed permissions and roles into the database on startup or migration, ensuring consistency with the codebase.
- **FR-013**: System MUST assume roles are provided via JWT claims from an external Identity Provider; local user-role management is out of scope.
- **FR-014**: System MUST implement a distributed cache (Redis) to store permission-to-role mappings, reducing database load during authorization enforcement.
- **FR-015**: System MUST follow an Opt-in (Granular) protection policy, where only endpoints with explicit permission attributes are subject to these new checks.

### Key Entities

- **Permission**: Represents a specific granular action (e.g., `accounting.journal-entries.post`). Attributes: Code, Description, IsCritical.
- **Role**: Represents a named collection of permissions (e.g., `accounting-manager`). Attributes: Name, AssignedPermissions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: System successfully registers exactly 18 unique permissions.
- **SC-002**: System successfully registers exactly 5 predefined roles.
- **SC-003**: The 5 roles correctly map to the specified permission sets (verified by effective permission checks).
- **SC-004**: 4 specific permissions (`post`, `reverse`, `close`, `reopen`) are identifiable as 'Critical' in the system metadata.
- **SC-005**: All protected endpoints reject unauthorized requests with HTTP 403 Forbidden.
- **SC-006**: Authorization enforcement latency is minimized by successfully retrieving permission mappings from Redis cache.
