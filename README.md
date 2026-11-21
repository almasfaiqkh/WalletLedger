# WalletLedger API

Core Payment & Digital Wallet REST API built with **.NET 8**, **Clean Architecture**, and **PostgreSQL**.

> **Status:** Active Development — Core domain, PostgreSQL migrations, and atomic ledger transactions are implemented. Authentication, REST endpoints, unit tests, and Docker configuration are currently in progress.

## Tech Stack
- **Framework:** C# / ASP.NET Core 8 Web API
- **Architecture:** Clean Architecture (`Domain`, `Application`, `Infrastructure`, `API`)
- **Database & ORM:** PostgreSQL, Entity Framework Core (Code-First)
- **Key Patterns:** ACID Database Transactions, Idempotency Keys, Optimistic Concurrency (`xmin`), Repository & Unit of Work

## Solution Structure
- `src/WalletLedger.Domain` — Core entities (`User`, `Wallet`, `LedgerTransaction`), enums, and domain exceptions
- `src/WalletLedger.Application` — Business logic (`WalletService`), DTOs, and repository contracts
- `src/WalletLedger.Infrastructure` — EF Core `DbContext`, database migrations, and persistence implementation
- `src/WalletLedger.API` — HTTP entry point and dependency injection setup
