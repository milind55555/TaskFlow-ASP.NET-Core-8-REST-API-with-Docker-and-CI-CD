# TaskFlow

A task management REST API built with **ASP.NET Core 8**, **EF Core + PostgreSQL** and **JWT authentication**, packaged with **Docker** and a **GitHub Actions CI/CD pipeline**.

![CI](https://github.com/<your-username>/taskflow/actions/workflows/ci.yml/badge.svg)

## Features
- Register / login with JWT bearer tokens (BCrypt password hashing)
- Projects and tasks (status, priority, due date); filter tasks by status
- Per-user data isolation: users can only access their own projects and tasks
- Swagger UI, `/health` endpoint (checks the database)
- xUnit tests for the service layer (EF Core InMemory)

## Tech stack
| Area | Tools |
|------|-------|
| API | ASP.NET Core 8, EF Core, Npgsql, Swashbuckle |
| Auth | JWT, BCrypt |
| Database | PostgreSQL 16 |
| Tests | xUnit, EF Core InMemory |
| Containers | Multi-stage Dockerfile (non-root runtime), Docker Compose |
| CI/CD | GitHub Actions (build, test, Trivy scan, push image to GHCR) |
| IaC (optional) | Terraform, Azure App Service (Linux container) |

## Architecture
```
Client --> Controllers --> Services --> EF Core (AppDbContext) --> PostgreSQL
              |                ^
            JWT auth      ownership checks (userId on every query)
```

## Run it with Docker (recommended)
```bash
docker compose up --build
```
- Swagger UI: http://localhost:8080/swagger
- Health check: http://localhost:8080/health

## Run it locally
```bash
# start only the database
docker compose up -d db

dotnet run --project src/TaskFlow.Api
```

## Run the tests
```bash
dotnet test
```

## API overview
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/register` | Create an account, returns a JWT |
| POST | `/api/auth/login` | Log in, returns a JWT |
| GET/POST | `/api/projects` | List / create projects |
| GET/PUT/DELETE | `/api/projects/{id}` | Read / update / delete a project |
| GET/POST | `/api/projects/{projectId}/tasks` | List (optional `?status=Done`) / create tasks |
| GET/PUT/DELETE | `/api/projects/{projectId}/tasks/{id}` | Read / update / delete a task |

Quick try with curl:
```bash
TOKEN=$(curl -s -X POST localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"me@example.com","password":"password123"}' | jq -r .token)

curl -s -X POST localhost:8080/api/projects \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"My first project"}'
```

## CI/CD
`.github/workflows/ci.yml`:
1. **build-test**: restore, build, run unit tests, upload test results
2. **docker**: on pull requests builds the image; on `main` scans with Trivy, logs in to GHCR and pushes `latest` and `<commit-sha>` tags

## Deploy to Azure (optional)
```bash
cd infra/terraform
terraform init
terraform apply -var="github_owner=<your-username>" \
  -var="db_connection_string=<postgres-connection-string>" \
  -var="jwt_key=<long-random-secret>"
```
Creates a resource group, a Linux App Service plan and a Web App that runs the GHCR image, with `/health` as the health probe. The database (for example Azure Database for PostgreSQL) is not created by this file.

## Configuration
| Setting | Purpose |
|---------|---------|
| `ConnectionStrings__Default` | PostgreSQL connection string |
| `Jwt__Key` | Signing key, minimum 32 characters. Use a secret in real deployments |
| `Jwt__Issuer` / `Jwt__Audience` | Token issuer and audience |

## Possible next steps
- Replace `EnsureCreated()` with EF Core migrations
- Add integration tests with Testcontainers
- Add pagination and refresh tokens
- Add Prometheus metrics and a Grafana dashboard

## License
MIT
