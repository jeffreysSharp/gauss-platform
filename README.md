# GAUSS Platform

[![Build Status](https://dev.azure.com/gauss-platform/gauss/_apis/build/status/gauss-platform?branchName=main)](https://dev.azure.com/gauss-platform/gauss/_build)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-proprietary-red)](LICENSE)

---

## About

**GAUSS Platform** is a modular financial platform under active development, designed to support banks, fintechs, credit institutions, finance companies, direct credit companies (SCD), and other financial market participants.

The platform is being built as a **multi-tenant SaaS** with a strong focus on security, tenant isolation, auditability, regulatory readiness, and modern cloud-native engineering practices.

GAUSS Platform aims to modernize legacy credit and banking management systems by combining modular architecture, automated credit workflows, configurable business rules, identity management, observability, and compliance-oriented foundations.

---

## Key Differentiators

| Feature | Description |
|---------|-------------|
| **Multi-tenant SaaS** | Designed for strong tenant isolation across data, identity, configuration, audit, and operational boundaries. |
| **Modular Architecture** | Business capabilities are organized as independent modules and bounded contexts, allowing gradual evolution and selective adoption. |
| **Security by Design** | Authentication, authorization, password hashing, token-based access, validation pipelines, and auditability are treated as core platform concerns. |
| **Regulatory Readiness** | Designed with Brazilian financial market requirements in mind, including LGPD, BACEN-oriented controls, audit trails, and future support for regulatory reporting. |
| **Credit Platform Foundation** | Planned support for credit product configuration, client registration, simulation, approval workflows, negotiation pipelines, and digital formalization. |
| **White-label Ready** | Planned tenant-level branding, custom domains, onboarding configuration, and institution-specific settings. |

---

## Current Status

GAUSS Platform is currently in the initial foundation phase.

The current development focus includes:

- Modular monolith solution structure
- Application building blocks
- Identity domain model
- User registration
- Login with access token
- API validation pipeline
- SQL Server persistence
- FluentMigrator database migrations
- Unit, integration, and API tests
- Azure Pipelines CI for build and automated tests

---

## Architecture Principles

The platform follows engineering practices commonly used in enterprise and financial systems:

- Domain-Driven Design
- Clean Architecture
- Modular Monolith
- CQRS-oriented application use cases
- Explicit application contracts
- Result-based error handling
- Minimal APIs
- Validation pipeline
- Automated tests
- CI pipeline with build, unit tests, integration tests, and API tests

---

## Technology Stack

| Area | Technology |
|------|------------|
| Backend | .NET 10, ASP.NET Core |
| Architecture | DDD, Clean Architecture, Modular Monolith |
| API | Minimal APIs, OpenAPI/Scalar |
| Persistence | SQL Server |
| Migrations | FluentMigrator |
| Testing | xUnit, AwesomeAssertions, integration tests, API tests |
| CI | Azure Pipelines |
| Observability | Correlation ID, health checks, structured foundations |

---

## Repository Structure

```text
src/
  building-blocks/
  database/
  services/
    audit/
    identity/

tests/
  UnitTests/
  IntegrationTests/

docs/
````

---

## Local Development

### Prerequisites

* .NET SDK 10
* SQL Server Express or SQL Server container
* Docker, recommended for infrastructure dependencies
* Visual Studio, Rider, or VS Code

### Restore

```bash
dotnet restore Gauss.slnx
```

### Build

```bash
dotnet build Gauss.slnx --configuration Release --no-restore
```

### Run tests

```bash
dotnet test Gauss.slnx --configuration Release
```

---

## Secrets and Local Configuration

Secrets are never committed to source control. The repository contains safe
placeholders in `appsettings.Development.json` and `.env.example`. Real values
must be supplied through one of the mechanisms below before running the API or
tests locally.

### Identity API — dotnet user-secrets

The Identity API reads `Identity:Persistence:ConnectionString` and
`Identity:AccessToken:SecretKey` from `dotnet user-secrets` when running in
the `Development` environment. The application will fail at startup with a
clear validation error if either value is missing.

```bash
# Initialise the user-secrets store for the Identity API project (run once)
dotnet user-secrets init --project src/services/identity/Gauss.Identity.Api

# Set the SQL Server connection string (adjust password to match your .env)
dotnet user-secrets set "Identity:Persistence:ConnectionString" \
  "Server=.\SQLEXPRESS;Database=GAUSS;User ID=sa;Password=<YOUR_LOCAL_SA_PASSWORD>;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=True;" \
  --project src/services/identity/Gauss.Identity.Api

# Set the JWT signing key (generate a strong random value, minimum 32 characters)
dotnet user-secrets set "Identity:AccessToken:SecretKey" \
  "<GENERATE_A_STRONG_RANDOM_KEY_HERE>" \
  --project src/services/identity/Gauss.Identity.Api

# Verify
dotnet user-secrets list --project src/services/identity/Gauss.Identity.Api
```

### Docker Compose and integration tests — .env file

The `.env` file at the repository root is used by **both** Docker Compose and
the test infrastructure:

* `docker-compose.yml` reads `MSSQL_SA_PASSWORD` to configure the SQL Server
  container. The compose file will refuse to start if the variable is absent.
* Integration and API tests read `GAUSS_TEST_SQLSERVER_*` and
  `GAUSS_TEST_REDIS_CONNECTION_STRING` from `.env` automatically, so you do
  not need to export them as shell environment variables for local runs.

```bash
# Copy the example file and fill in your chosen local SA password
cp .env.example .env
# Edit .env — set MSSQL_SA_PASSWORD and GAUSS_TEST_SQLSERVER_PASSWORD to the
# same value, and adjust other variables to match your local setup.
```

The `.env` file is gitignored and must never be committed. `.env.example`
contains placeholders only and is safe to commit.

The test infrastructure resolves values in this order (first wins):

1. Process environment variable
2. `.env` file at the repository root

| Variable | Description | Default if absent |
|---|---|---|
| `GAUSS_TEST_SQLSERVER_HOST` | SQL Server host and port | `.\SQLEXPRESS` |
| `GAUSS_TEST_SQLSERVER_USER` | SQL Server login | `sa` |
| `GAUSS_TEST_SQLSERVER_PASSWORD` | SQL Server SA password | *(required — fails fast)* |
| `GAUSS_TEST_REDIS_CONNECTION_STRING` | Redis connection string | `localhost:6379,abortConnect=false` |

In CI these variables are supplied by the Azure DevOps variable group
`gauss-integration-tests-secrets` as process environment variables, which
take precedence over any `.env` file.

When using the Docker Compose stack for local tests:

```bash
docker compose up -d
dotnet test Gauss.slnx --configuration Release
```

### CI pipeline — Azure DevOps secret variable group

The CI pipeline resolves `$(sqlServerPassword)` from the Azure DevOps Library
variable group `gauss-integration-tests-secrets`. This group must be created
manually in the Azure DevOps project Library before the Integration Tests stage
can run.

Required secret variable in the group:

| Variable name | Type | Description |
|---|---|---|
| `sqlServerPassword` | **Secret** | SA password for the pipeline SQL Server container |

---

## CI Pipeline

The official CI pipeline is defined in:

```text
azure-pipelines.yml
```

The pipeline currently runs:

1. Build
2. Unit Tests
3. Integration and API Tests

It also publishes:

* test results
* code coverage results

---

## License

This project is proprietary and confidential.

Copyright (c) 2026 GAUSS Platform. All rights reserved.
