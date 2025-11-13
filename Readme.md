# KopiAku — POS System (Dotnet + GraphQL)

A point-of-sale backend for KopiAku coffee shop implemented with .NET and GraphQL. Designed for inventory, orders, customers, and payments with a schema-first GraphQL API suitable for web/mobile POS clients.

## Key features
- GraphQL API for queries, mutations and subscriptions
- Order management (create, update, cancel)
- Inventory & product catalog
- Customer profiles & loyalty points
- Payment integration hooks
- Role-based authentication (staff, admin)
- EF Core migrations and seeding
- Docker-ready for deployment

## Tech stack
- .NET (7+) — API and business logic
- GraphQL server (e.g., HotChocolate or GraphQL .NET)
- EF Core for data access (SQL Server / PostgreSQL / SQLite)
- JWT authentication
- Docker for containerization
- Optional: Redis for caching, RabbitMQ for async jobs

## Repository layout (suggested)
- src/KopiAku.Api — GraphQL server + middleware
- src/KopiAku.Domain — domain models & services
- src/KopiAku.Data — EF Core DbContext, migrations, seeders
- tests/KopiAku.Tests — unit & integration tests
- docker/ — Dockerfile and compose templates

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

## GraphQL usage examples

Sample types (conceptual)
```graphql
type Product {
    id: ID!
    name: String!
    price: Float!
    stock: Int!
    category: String
}

type Order {
    id: ID!
    items: [OrderItem!]!
    total: Float!
    status: String!
    placedAt: DateTime!
}
```

Queries
```graphql
query GetProducts($page:Int,$size:Int){
    products(page:$page,size:$size){
        items { id name price stock }
        totalCount
    }
}
```

Mutations
```graphql
mutation CreateOrder($input: CreateOrderInput!){
    createOrder(input: $input) {
        id
        status
        total
    }
}
```

Subscriptions (real-time order updates)
```graphql
subscription OnOrderUpdated {
    orderUpdated {
        id status total
    }
}
```

## Database & Migrations
- Use EF Core migrations:
    - Add: dotnet ef migrations add InitialCreate --project src/KopiAku.Data
    - Apply: dotnet ef database update --project src/KopiAku.Data
- Seed sample data on startup for local development.

## Authentication & Authorization
- JWT-based authentication for staff and admin roles.
- Protect GraphQL fields with role-based policies.
- Secure mutation endpoints and payment operations.

## Testing
- Unit tests: dotnet test tests/KopiAku.Tests
- Add integration tests that run against an in-memory or dockerized DB.
- Include schema validation tests for GraphQL.

## CI / CD
- Build and test in CI (GitHub Actions / Azure Pipelines)
- Run migrations and deploy Docker image to your registry
- Use environment-specific configuration for secrets

## Contributing
- Fork -> feature branch -> PR with description and tests
- Follow code style (EditorConfig) and add unit tests for logic
- Keep PRs small and focused

## License
MIT — see LICENSE file.

## Maintainers / Contact
- Project maintainers: add team contact or repo link

Notes: adapt GraphQL server implementation (HotChocolate recommended) and database provider to project requirements. Keep schema and resolvers separated from infrastructure for testability.