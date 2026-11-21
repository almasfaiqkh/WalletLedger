# WalletLedger API

Core Payment & Digital Wallet REST API built with **.NET 8**, **Clean Architecture**, **Entity Framework Core**, and **PostgreSQL**.

## Key Features
- **Clean Architecture:** Strict separation of concerns across `Domain`, `Application`, `Infrastructure`, and `API` layers.
- **ACID Financial Transactions:** Atomic peer-to-peer wallet transfers and top-ups orchestrated through a Unit of Work pattern with automatic rollback on failure.
- **Idempotency Protection:** Prevents duplicate balance deductions or double credits on retried requests via unique `IdempotencyKey` tracking.
- **Optimistic Concurrency Control:** Uses PostgreSQL's system `xmin` column as a concurrency token (`IsRowVersion`) to prevent race conditions during concurrent balance updates.
- **JWT Authentication & Security:** Stateless Bearer token authentication paired with BCrypt password hashing.
- **Input Validation & Standardized Errors:** Request validation via **FluentValidation** and RFC 7807 `ProblemDetails` global exception handling.
- **Automated Testing & CI:** Unit-tested business logic using **xUnit** & **Moq**, automated with **GitHub Actions**.

## Tech Stack
- **Language & Framework:** C# 12, .NET 8, ASP.NET Core Web API
- **Database & ORM:** PostgreSQL 16, Entity Framework Core 8 (Code-First)
- **Security & Validation:** JWT Bearer, BCrypt.Net, FluentValidation
- **Testing & CI/CD:** xUnit, Moq, GitHub Actions

## Solution Structure
```text
WalletLedger/
├── .github/workflows/ci.yml        # GitHub Actions build & test pipeline
├── src/
│   ├── WalletLedger.Domain         # Core entities, enums, and domain exceptions
│   ├── WalletLedger.Application    # Business services, DTOs, validators, and interfaces
│   ├── WalletLedger.Infrastructure # EF Core DbContext, migrations, repositories, JWT & hashing
│   └── WalletLedger.API            # REST controllers, middleware, and Swagger configuration
└── tests/
    └── WalletLedger.UnitTests      # Unit tests for WalletService and AuthService
```

## API Endpoints

| Method | Endpoint | Auth | Description |
| :--- | :--- | :---: | :--- |
| `POST` | `/api/auth/register` | Public | Register a new user and initialize a zero-balance wallet |
| `POST` | `/api/auth/login` | Public | Authenticate user and receive a JWT access token |
| `GET` | `/api/wallets/me` | Bearer | Retrieve authenticated user's wallet balance |
| `POST` | `/api/wallets/top-up` | Bearer | Add funds to wallet (idempotent) |
| `POST` | `/api/wallets/transfer` | Bearer | Transfer funds atomically to another user by email |
| `GET` | `/api/wallets/transactions` | Bearer | List incoming and outgoing ledger history |

## Getting Started (Local Setup)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16](https://www.postgresql.org/download/)

### 1. Clone & Configure Database
```bash
git clone [https://github.com/almasfaiqkh/WalletLedger.git](https://github.com/almasfaiqkh/WalletLedger.git)
cd WalletLedger
```
Ensure PostgreSQL is running locally on port `5432` and create a database named `wallet_ledger_db` (or update `ConnectionStrings:DefaultConnection` in `src/WalletLedger.API/appsettings.json`).

### 2. Apply Database Migrations
```bash
dotnet ef database update --project src/WalletLedger.Infrastructure --startup-project src/WalletLedger.API
```

### 3. Run the API
```bash
dotnet run --project src/WalletLedger.API
```
Open Swagger UI at `http://localhost:5000/swagger` (check terminal output for the exact port).

### 4. Run Unit Tests
```bash
dotnet test
```