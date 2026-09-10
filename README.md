# MailForge

**MailForge** is a production-ready B2B email finder and verification SaaS platform. It helps users discover, verify, organize, enrich, and export legitimate business contact information from lawful and permitted data sources.

## Architecture

```
src/
 ├── Api/           # ASP.NET Core REST API + Swagger
 ├── Application/   # Business interfaces, DTOs
 ├── Domain/        # Entities, enums
 ├── Infrastructure/# EF Core, providers, services, messaging
 ├── Workers/       # RabbitMQ background workers
 └── Shared/        # Settings, constants
frontend/           # Next.js dashboard
tests/              # Unit + integration tests
```

## Tech Stack

- **Backend:** .NET 8, ASP.NET Core, EF Core, PostgreSQL
- **Frontend:** Next.js 14, TypeScript, Tailwind CSS
- **Infrastructure:** Redis, RabbitMQ, Docker Compose
- **Auth:** JWT + refresh tokens, API keys (hashed)
- **Security:** BCrypt password hashing, rate limiting, audit logs

## Quick Start

### 1. Clone

```bash
git clone <repo-url>
cd mailforge
```

### 2. Configure `.env`

```bash
cp .env.example .env
# Edit values as needed
```

### 3. Start Docker

```bash
docker compose up -d
```

This starts: `postgres`, `redis`, `rabbitmq`, `api`, `worker`, `frontend`

### 4. Run Migrations

Migrations run automatically on API startup via the database seeder.

### 5. Access Services

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| RabbitMQ UI | http://localhost:15672 (guest/guest) |
| Health | http://localhost:5000/health |

### 6. Login

Default admin account (seeded):
- **Email:** `admin@mailforge.local`
- **Password:** `Admin123!`

New users get 1,000 starter credits on registration.

### 7. Test Finder

1. Go to **Finder** in the dashboard
2. Enter: John, Smith, example.com
3. Click **Find Email**
4. Review candidate emails with confidence scores

### 8. Test Verifier

1. Go to **Verifier**
2. Enter an email address
3. Click **Verify**
4. Review status, score, MX/SMTP results

### 9. Test Bulk Upload

1. Create a CSV with columns: `first_name,last_name,domain`
2. Go to **Bulk Search** and upload
3. Monitor progress (Total, Processed, Found, etc.)

## Local Development (without Docker)

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 16
- Redis 7
- RabbitMQ 3

### Backend

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=mailforge;Username=mailforge;Password=mailforge_dev"
cd src/Api
dotnet run
```

### Worker

```bash
cd src/Workers
dotnet run
```

### Frontend

```bash
cd frontend
npm install
NEXT_PUBLIC_API_URL=http://localhost:5000 npm run dev
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `POSTGRES_PASSWORD` | PostgreSQL password | `mailforge_dev` |
| `JWT_SECRET` | JWT signing key (min 32 chars) | dev key |
| `FRONTEND_URL` | CORS allowed origin | `http://localhost:3000` |
| `API_URL` | Public API URL for frontend | `http://localhost:5000` |

See `.env.example` for full list.

## API Documentation

### Authentication

**JWT (Dashboard):**
```http
Authorization: Bearer <access_token>
```

**API Key (Public API):**
```http
X-API-Key: mf_<your_key>
```

### Public Endpoints

```http
POST /api/v1/finder
POST /api/v1/verifier
POST /api/v1/domain-search
POST /api/v1/bulk
GET  /api/v1/jobs/{id}
GET  /api/v1/credits
```

### Example: Find Email

```bash
curl -X POST http://localhost:5000/api/v1/finder \
  -H "X-API-Key: mf_your_key_here" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Smith","domain":"example.com"}'
```

Full interactive docs at `/swagger`.

## Database Schema

Core tables: `Users`, `CreditBalances`, `CreditTransactions`, `CreditRules`, `Plans`, `Subscriptions`, `Contacts`, `FinderResults`, `VerificationResults`, `BulkJobs`, `Exports`, `ApiKeys`, `AuditLogs`, and more.

All public IDs use UUIDs. Credit operations are auditable with transaction locking.

## Credit System

| Operation | Default Cost |
|-----------|-------------|
| Email Finder | 1 credit |
| Email Verification | 1 credit |
| Enrichment | 2 credits |
| Domain Search | 2 credits |
| Export | 1 credit |

Costs are configurable via the `CreditRules` table — never hard-coded in controllers.

## Testing

```bash
# Unit tests
dotnet test tests/MailForge.UnitTests

# Integration tests
dotnet test tests/MailForge.IntegrationTests

# All tests
dotnet test
```

## CI/CD

GitHub Actions workflows run on every PR and on pushes to `main`:

| Workflow | Purpose |
|----------|---------|
| `ci.yml` | .NET build/test, frontend lint/build, Docker build verify |
| `cd.yml` | Publish images to GHCR on `main` / tags; staging deploy hook |
| `release.yml` | GitHub Release + artifacts on `v*.*.*` tags |

See [deploy/README.md](deploy/README.md) for production deployment with GHCR images.

### Quick release

```bash
git tag v1.0.0
git push origin v1.0.0
```

## Production Deployment

1. Set strong `JWT_SECRET` and `POSTGRES_PASSWORD`
2. Configure real SMTP/DNS verification limits
3. Replace `MockDomainSearchProvider` with lawful data providers
4. Set up SSL/TLS termination (nginx/Cloudflare)
5. Configure backup for PostgreSQL
6. Set Redis and RabbitMQ persistence
7. Use secrets manager for environment variables

```bash
docker compose -f docker-compose.yml up -d --build
```

## Known Limitations

- Domain search uses a mock development provider for demo domains (`example.com`, `company.com`)
- SMTP verification depends on target mail server policies and may return `Unknown` for servers that block verification
- Email sending (verification/reset) is logged but not sent in development
- Payment/billing integration is placeholder UI only

## Next Recommended Features

- Stripe billing integration
- Real email provider integrations (configurable via `IEmailFinderProvider`)
- Webhook notifications for bulk job completion
- Team/organization multi-user support
- Advanced enrichment providers
- GDPR data export/deletion UI
- Prometheus metrics endpoint

## License

Original implementation — not affiliated with any existing email finder product.
