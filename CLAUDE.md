# AI Coding Rules — eProtocol

These rules apply to EVERY change you make in this codebase, regardless of
what the prompt asks. No exceptions. Read them before touching any file.

---

## 1. READ BEFORE YOU WRITE

- Read every file relevant to the task before writing a single line.
- Understand the full call chain: Controller ? Service ? Repository ? DB.
- If you are unsure how something works, read more files. Never assume.
- Never generate code based on what "usually" exists in projects like this.
  Only generate code based on what actually exists here.

---

## 2. PRESERVE THE ARCHITECTURE

- The layered architecture (Controllers, Services, Repositories, DTOs,
  Entities) must never be violated.
- Controllers never talk to repositories directly.
- Repositories never contain business logic.
- Services never directly access HttpContext or request objects.
- Entities never leave the repository layer — always map to DTOs before
  returning from a service.
- If the task cannot be done without violating one of these rules, stop and
  explain why before proceeding.

---

## 3. NEVER DUPLICATE LOGIC

- Before writing any new logic, search the codebase for existing logic that
  does the same thing.
- If it exists, reuse it. Call it, extend it, or refactor it — never copy it.
- If you find existing duplication while working, consolidate it as part of
  your change. Do not leave known duplicates untouched.
- Shared logic belongs in: a service, a base class, an extension method,
  a helper, or a middleware — whichever already exists for that category.

---

## 4. REUSE EVERYTHING THAT ALREADY EXISTS

- Existing DTOs: use them. Do not create a new DTO if an existing one fits
  or can be extended with a nullable property.
- Existing services: call them. Do not reimplement what a service already does.
- Existing middleware: do not add a second exception handler, logger, or
  auth middleware.
- Existing AutoMapper profiles: add new maps to the existing profile file.
  Never create a second profile for the same domain area.
- Existing validators: extend them. Never create a parallel validation path.
- Existing error response format: use it for every new endpoint.
- Only create something new when nothing existing can reasonably serve the
  purpose.

---

## 5. CONSISTENCY IS NON-NEGOTIABLE

- Match the naming convention already in use: if services are named
  DocumentService, name yours XxxService. If repositories are IXxxRepository,
  follow that exactly.
- Match the async pattern: if the codebase uses async/await throughout,
  every new method must be async. No .Result, no .Wait(), no sync-over-async.
- Match the dependency injection pattern: if services are Scoped, register
  yours as Scoped. Never guess the lifetime.
- Match the response pattern: if controllers return ActionResult<T>, yours
  must too. If they use Ok(), NotFound(), BadRequest() — follow that exactly.
- Match the logging pattern: if ILogger<T> is injected and used, do the same.
  Do not use Console.WriteLine or Debug.WriteLine.
- When in doubt about style, find the nearest similar file and match it
  exactly.

---

## 6. NEVER BREAK EXISTING BEHAVIOR

- Never change an existing method signature. Add an overload if needed.
- Never rename an existing DTO property. The frontend depends on it.
- Never change an existing route path or HTTP verb.
- Never change existing DB column names or types without a migration.
- Never remove a registered service, middleware, or configuration entry
  unless explicitly told to.
- After every change, mentally trace all existing callers of what you
  modified and confirm none of them are broken.

---

## 7. CODE QUALITY STANDARDS

- Every new public method needs a single responsibility. If a method does
  more than one thing, split it.
- Maximum method length: 40 lines. If it grows beyond that, extract
  private helper methods.
- No magic numbers or magic strings. Use constants, enums, or configuration
  values.
- No empty catch blocks. Ever. Either handle the exception meaningfully or
  let it propagate.
- No swallowed exceptions. If you catch, you must log or rethrow.
- No commented-out code left behind.
- No TODO comments left in production code paths.
- Guard clauses at the top of methods — fail fast, return early.
  Avoid deep nesting.
- Nullable reference types: check before accessing. Never assume a nullable
  is non-null without a prior null check.

---

## 8. DATABASE & QUERIES

- All DB calls must be async (ToListAsync, FirstOrDefaultAsync,
  SaveChangesAsync, etc.). Never use the sync equivalents.
- All filtering (Where), sorting (OrderBy), and pagination (Skip/Take)
  must happen on IQueryable — before the query hits the database.
  Never load a full table and filter in memory.
- All list queries that could return more than ~20 rows must have
  pagination applied.
- All navigation properties accessed in a query must be .Include()'d
  or .ThenInclude()'d. No lazy loading surprises.
- Never use raw SQL strings unless EF Core literally cannot express the
  query — and even then, use parameterized commands only.
- Every new foreign key column needs an index. Add it in EF config and
  create a migration.
- Protocol number generation or any auto-increment business key must be
  done inside a transaction to prevent race conditions.

---

## 9. SECURITY

- Every new endpoint must have [Authorize] or [AllowAnonymous] explicitly
  declared. No endpoint is accidentally open.
- Every endpoint that is role-restricted must have the correct role policy
  or [Authorize(Roles = "...")] attribute.
- File uploads must validate MIME type and file size before any processing.
  Reject invalid files with 400 immediately.
- All stored filenames must be UUID-based. Never write user-supplied
  filenames to disk.
- Passwords must always be hashed. Never store, log, or return plain-text
  passwords.
- No secrets, keys, or connection strings in source code. Configuration
  only.
- Never trust user input for file paths, IDs, or query construction.

---

## 10. ERROR HANDLING

- Use the global exception middleware for unhandled exceptions. Do not add
  try/catch in controllers unless you are handling a specific, known,
  recoverable exception.
- Return 404 when a resource is not found. Never return 200 with a null body.
- Return 400 with clear validation messages for invalid input.
- Return 403 when a user is authenticated but not authorized for the action.
- Never let a stack trace reach the client response.
- All error responses must use the same format as the rest of the project.

---

## 11. MIGRATIONS

- Every DB schema change (new table, new column, new index, constraint
  change, rename) requires a new EF Core migration.
- Name migrations descriptively: AddFileStoreTable, AddProtocolNumberIndex,
  AddUserIsActiveDefault.
- Never edit an existing migration that has already been applied.
  Always create a new one.
- After adding a migration, verify the generated Up() and Down() methods
  are correct and complete before considering the task done.

---

## 12. WHAT YOU MUST NEVER DO

- Never introduce a new architectural pattern not already present.
- Never add a NuGet package without a compelling reason. If you must,
  pick the smallest, most established option and explain why it is needed.
- Never generate placeholder, stub, or "TODO: implement" code. If you
  write a method, it must be fully implemented.
- Never leave dead code (unused methods, unused usings, unreachable
  branches).
- Never use var when the type is not immediately obvious from the
  right-hand side.
- Never write a comment that just restates what the code does. Comments
  explain WHY, not WHAT.
- Never make a change outside the scope of the current task unless it is
  a direct blocker. If you spot something unrelated that needs fixing,
  note it at the end but do not fix it silently.

---

## 13. BEFORE YOU FINISH

Run this checklist mentally before submitting any change:

[ ] I read all relevant files before writing anything.
[ ] I have not duplicated any logic that already existed.
[ ] I have not changed any existing method signature, route, or DTO field.
[ ] Every new endpoint has an [Authorize] or [AllowAnonymous] attribute.
[ ] Every new DB query is fully async and filters on IQueryable.
[ ] Every new service is registered in DI.
[ ] Every new migration has correct Up() and Down() methods.
[ ] No secrets are hardcoded anywhere.
[ ] No stack traces can reach the client.
[ ] All new code is consistent in style, naming, and pattern with the
    existing codebase.
[ ] I have not introduced any new packages or patterns without necessity.
[ ] No dead code, no TODOs, no stubs remain.