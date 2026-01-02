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
- .NET SDK 9.0+ installed
- Database (Postgres / SQL Server) or use SQLite for local dev
- (Optional) Docker & Docker Compose

Local run (example)
```bash
# restore & build
dotnet restore
dotnet build

# run API
dotnet run
# default GraphQL endpoint: http://localhost:5031/graphql
```

Docker
```bash
docker build -t kopiaku:latest .
docker run -e ConnectionStrings__DefaultConnection="..." -p 5000:80 kopiaku:latest
```

Environment variables
- MongoDBSettings__ConnectionString
- MongoDBSettings__DatabaseName

- JWTSettings__SecretKey
- JWTSettings__Issuer
- JWTSettings__Audience
- JWTSettings__ExpiryInMinutes

- B2Settings__BaseUrl
- B2Settings__AccessKey
- B2Settings__SecretKey
- ASPNETCORE_ENVIRONMENT — Development | Production

## Authentication & Authorization
- JWT-based authentication for staff and admin roles.
- Protect GraphQL fields with role-based policies.
- Secure mutation endpoints and payment operations.