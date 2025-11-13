# KopiAku — POS System (Dotnet + GraphQL)

A point-of-sale backend for KopiAku coffee shop implemented with .NET and GraphQL. Designed for inventory, orders, customers, and payments with a schema-first GraphQL API suitable for web/mobile POS clients.

## Key features
- GraphQL API for queries, mutations and subscriptions
- Order management (create, update, cancel)
- Inventory & product catalog
- Customer profiles & loyalty points
- Payment integration hooks
- Role-based authentication (staff, admin)
- Docker-ready for deployment

## Tech stack
- .NET (9+) — API and business logic
- GraphQL server (e.g., HotChocolate or GraphQL .NET)
- JWT authentication
- Docker for containerization
- Optional: Redis for caching, RabbitMQ for async jobs

## Getting started

Prerequisites
- .NET SDK 7.0+ installed
- Database (Postgres / SQL Server) or use SQLite for local dev
- (Optional) Docker & Docker Compose

Local run (example)
```bash
# restore & build
dotnet restore
dotnet build

# apply migrations (EF Core)
dotnet ef database update --project src/KopiAku.Data

# run API
dotnet run --project src/KopiAku.Api
# default GraphQL endpoint: http://localhost:5000/graphql
# interactive UI (HotChocolate Banana Cake Pop): http://localhost:5000/graphql/ui
```

Docker
```bash
docker build -t kopiaku:latest .
docker run -e ConnectionStrings__DefaultConnection="..." -p 5000:80 kopiaku:latest
```

Environment variables
- ConnectionStrings__DefaultConnection — DB connection string
- Jwt__Issuer, Jwt__Audience, Jwt__Key — JWT settings
- ASPNETCORE_ENVIRONMENT — Development | Production

## Authentication & Authorization
- JWT-based authentication for staff and admin roles.
- Protect GraphQL fields with role-based policies.
- Secure mutation endpoints and payment operations.