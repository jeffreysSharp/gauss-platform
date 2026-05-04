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
