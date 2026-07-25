## Herafy
ITI Graduation Project

## About
**ITI Project** is a backend solution built with **ASP.NET Core (.NET 8)** that exposes a REST API for a service-request workflow (clients create service requests and providers submit offers), with real-time updates and notifications.

The solution is organized using a layered architecture (API/Core/Repository/Services) and common backend patterns (Unit of Work, repositories, DTO mapping).

## Features
- RESTful API built with ASP.NET Core
- Authentication & authorization with ASP.NET Core Identity + JWT
- Real-time notifications and updates via SignalR
- Database access with Entity Framework Core (SQL Server)
- Centralized error handling middleware
- Structured logging with Serilog
- Payment integration hooks (Stripe)
- Media upload integration hooks (Cloudinary)
- Swagger/OpenAPI documentation

## Deployment

- **API Backend:** [https://iti-final-project.runasp.net](https://iti-final-project.runasp.net)
- **Herafy App (Frontend):** [https://harafy-app.vercel.app/](https://harafy-app.vercel.app/)

## Admin Test Account

For testing and reviewing as an admin, you can use the following credentials:

- **Email:** Admin@gmail.com
- **Password:** Pa$$w0rd

## Architecture

```text
┌─────────────────────┐
│      API Layer      │
│---------------------│
│ Controllers         │
│ Middleware          │
│ SignalR Hubs        │
│ Authentication      │
│ Dependency Injection│
└─────────┬───────────┘
          │
          │ accesses
          ▼
┌────────────────────┐
│    Service Layer   │
│--------------------│
│ Business Logic     │
│ Application Rules  │
│ Result Pattern     │
│ DTO Mapping        │
└─────────┬──────────┘
          │
          │ accesses
          ▼
┌────────────────────┐
│ Repository Layer   │
│--------------------│
│ Repositories       │
│ Unit Of Work       │
│ Specifications     │
│ EF Core Queries    │
└─────────┬──────────┘
          │
          │ accesses
          ▼
┌────────────────────┐
│     Core Layer     │
│--------------------│
│ Entities           │
│ Interfaces         │
│ Enums              │
│ Shared Models      │
│ Errors & Results   │
└─────────┬──────────┘
          │
          ▼
┌────────────────────┐
│     SQL Server     │
│ Entity Framework   │
└────────────────────┘
```
API Layer ─────────► Core Layer
Service Layer ─────► Core Layer
Repository Layer ──► Core Layer

## Tech Stack
- **Language**: C# 12
- **Framework**: ASP.NET Core Web API (.NET 8)
- **Database**: SQL Server (LocalDB for development)
- **ORM**: Entity Framework Core
- **Auth**: ASP.NET Core Identity, JWT Bearer tokens
- **Real-time**: SignalR
- **Logging**: Serilog
- **Payments**: Stripe
- **Media**: Cloudinary
- **API Docs**: Swagger / OpenAPI

## Project Structure
High-level solution layout:

- `ITI_Project.Api/` — ASP.NET Core Web API (controllers, middleware, SignalR hubs, DI setup)
- `ITI_Project.Core/` — domain models, enums, constants, helpers, shared contracts
- `ITI_Project.Repository/` — EF Core DbContexts, migrations, repository & data access implementations
- `ITI_Project.Service/` — application/services layer (business use cases)

## Engineering Features
- Clean layered architecture
- Repository + Unit of Work pattern
- Result pattern for business failures
- Global exception handling middleware
- Real-time notifications with SignalR
- Stripe payment integration with webhooks
- Cloudinary media storage and migration
- Background image migration batching
- Role-based authorization
- Specification pattern for querying

## Media Management
Images and provider documents are stored using Cloudinary.

The project includes:
- image upload abstraction
- cloud storage integration
- background migration from local storage to Cloudinary
- batched migration processing

## Real-Time Features

SignalR is used for:
- notifications
- request updates
- real-time provider live map location

## Getting Started

### Prerequisites
- .NET SDK **8.x**
- SQL Server / SQL Server Express / LocalDB
- (Optional) Visual Studio 2026+ / VS Code

### Run locally
1. Clone the repository.
2. Configure your environment variables (see **Configuration**).
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Run the API:
   ```bash
   dotnet run --project ITI_Project.Api
   ```
5. Open Swagger UI:
   - `https://localhost:<port>/swagger`

> Note: On startup, the API runs EF Core migrations automatically (see `ITI_Project.Api/Program.cs`).

## Configuration
This project reads configuration from environment variables (and a local `.env` file in Development).

### Environment variables
Common keys used by the API (names follow the `__` convention for nested config):

#### Database
- `ConnectionStrings__DefaultConnection` — Application database connection string
- `ConnectionStrings__IdentityConnection` — Identity database connection string

#### JWT
- `JWT__SecretKey`
- `JWT__ValidAudience`
- `JWT__ValidIssuer`
- `JWT__AccessTokenExpirationInMinutes`

#### Stripe
- `Stripe__SecretKey`
- `Stripe__PublishableKey`
- `Stripe__WebhookSecret`

#### Cloudinary
- `Cloudinary__CloudName`
- `Cloudinary__ApiKey`
- `Cloudinary__ApiSecret`

### Local development with `.env`
In Development, the API loads a `.env` file from `ITI_Project.Api/.env` if present.

**Security note**: Do not commit real secrets to source control. Use placeholder values for documentation and keep production secrets in a secure secret store (e.g., environment variables, Azure Key Vault, GitHub Actions secrets).

## How to Contribute
1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature/my-change
   ```
3. Make your changes and ensure the build succeeds:
   ```bash
   dotnet build
   ```
4. Commit with a clear message and push your branch.
5. Open a Pull Request describing the change and rationale.

## License
No license has been specified for this repository yet.

## Author
- GitHub: [MoAlaa31](https://github.com/MoAlaa31)
