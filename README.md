# library-dot-net

A .NET library management system built as a university project. Implements borrowing rules, domain hierarchy, reader constraints and staff-specific overrides with full service layer validation and unit testing.

## Requirements

### Books

- A book must belong to at least one domain.
- Domains can be subdivided hierarchically (e.g. Science &rarr; {Mathematics, Physics, Chemistry, Computer Science}; Computer Science &rarr; {Algorithms, Programming, Databases, ...}).
- If a book is assigned to a subdomain, it is automatically considered part of all ancestor domains (this does not need to be declared explicitly).
- A book **cannot** be explicitly assigned to two domains that are in an ancestor-descendant relationship (e.g. assigning a book to both *Algorithms* and *Science* is an error).
- A book cannot belong to more than **DOMENII** domains (configurable, no recompilation required).
- A book can have multiple authors and multiple editions. Each edition stores edition-specific data (page count, book type, publication date).
- Some or all copies of a book may be marked as **lecture-room only**.
- A book is available for borrowing only if:
  - Not all copies are marked as lecture-room only,
  - The number of remaining borrowable copies is at least **10%** of the initial stock.

### Readers

- Personal data must be stored in a consistent format (first name, last name, address, etc.).
- At least one contact method must be provided: phone number or email address.
- A reader can borrow at most **NMC** books within a period **PER**.
- A single borrowing request can include at most **C** books. If the request includes 3 or more books, they must span at least 2 distinct domains.
- A reader cannot borrow more than **D** books from the same domain (leaf or ancestor) in the last **L** months.
- Books are borrowed for a fixed period. Extensions are allowed, but the total extensions granted in the last 3 months cannot exceed the limit **LIM**.
- A reader cannot borrow the same book more than once within a **DELTA** interval (measured from the last borrow of that book).
- A reader can borrow at most **NCZ** books per day.

### Library Staff

- If staff are also registered as readers on the same account, their thresholds are adjusted: **NMC**, **C**, **D** and **LIM** are doubled; **DELTA** and **PER** are halved.
- Staff cannot process more than **PERSIMP** book loans per day. The **NCZ** limit does not apply to them.

Librarian adjusted values (doubles/halves) are computed at read time by passing a `forLibrarian` flag to the relevant getter methods (no separate records are stored for staff variants).

### Technical Constraints
 
- **Unit testing** - minimum 200 test methods; property values, business rules, workflow and result correctness must all be covered. Tests must validate service layer coherence (invalid inputs should produce exceptions or failure results).
- **Logging** - mandatory in application layers.
- **Validation** - mandatory object state validation.
- **Layered architecture** - Domain Layer and Data Layer; no UI required.
- **Relational database** - SQL Server, MySQL, Oracle or PostgreSQL.
- **Domain Layer** implemented as a Domain Model and **Data Layer** implemented as a Data Mapper.
- **Code coverage** - minimum 90% for Domain Model and minimum 90% for Service Layer.
- **Mocking** - required for isolating service layer tests.
- **XML documentation comments** - minimum coverage for methods, constructors, properties, indexers, destructors and nested types.
- **Configurable thresholds** - all uppercase constants (NCZ, C, NMC, etc.) must be changeable without recompiling.

## Architecture

The project follows a layered structure loosely inspired by Clean Architecture, with the following projects:

- **DomainModel** - entities, domain logic, repository contracts, validation annotations
- **ServiceLayer** - business rules, FluentValidation validators, service contracts
- **Infrastructure** - Entity Framework DbContext, repositories, Unit of Work, DI registration
- **TestDomainModel** - unit tests covering domain model
- **TestServiceLayer** - unit tests covering service layer
- **LibraryAppConsole** - console entry point and the place where all DI registrations and wiring happen

No UI layer was required or implemented.

Domain and Service layers are kept separate per project requirements (rather than merging into a single Core project).

**Unit of Work and Repositories**

- `UnitOfWork` coordinates all repositories and shares a single `LibraryContext` across them.
- In practice it is used primarily as a convenience abstraction with each service injecting one `IUnitOfWork` instead of multiple repositories.
- Transaction methods (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`) are implemented and available, but the current services call `SaveChangesAsync()` directly rather than wrapping operations in explicit transactions.


## Tech Stack

| Concern | Technology |
|---|---|
| Framework | .NET Framework 4.8 |
| ORM | Entity Framework |
| Database | SQL Server |
| Logging | Serilog (console + SQL Server sinks) |
| Validation | FluentValidation |
| Dependency Injection | Microsoft.Extensions DI |
| Unit Testing | MSTest |
| Test Utilities | FluentAssertions, AutoFixture |
| Mocking | Moq |

## Configuration
 
The app expects a `ConnectionStrings.config` file in the console project root (not committed to source control):
 
```xml
<?xml version="1.0" encoding="utf-8" ?>
<connectionStrings>
  <add name="LibraryDBConnectionString"
       connectionString="server=YOUR_SERVER; database=LibraryDB; user id=YOUR_USER; password=YOUR_PASSWORD; TrustServerCertificate=true"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Configurable Thresholds

All uppercase thresholds (NCZ, C, NMC, D, L, LIM, DELTA, PER, PERSIMP, DOMENII) can be changed without recompiling the application. They are stored as `ConfigurationSetting` records in the database, each with a key, value (stored as string), data type, category and description. The `ConfigurationSettingService` retrieves values via the repository and caches them in memory for 30 minutes (cache-aside pattern). Hard-coded defaults from `ConfigurationConstants` serve as fallbacks when a key is not found in the database.

## Testing

- ~**500 test methods** covering domain entities, service layer constraints, validation and workflow scenarios.
- Tests assert property values, business rule enforcement, combined constraint scenarios and object state consistency.
- Code coverage target: **&ge; 90%** for both Domain Model and Service Layer.
- Mocking is used to isolate service layer tests from infrastructure dependencies.

## Error Handling & Logging

Services extend a `BaseService` class that wraps all operations in a standardized `ExecuteServiceOperationAsync` method. This handles the following exception types uniformly:

- `AggregateValidationException` - validation failures
- `NotFoundException` - missing resources
- `BusinessRuleException` - violated business rules
- Unhandled `Exception` - generic fallback

Serilog is configured with two sinks:
- **Console** - Information level and above
- **SQL Server** (`ApplicationLogs` table) - Warning level and above
