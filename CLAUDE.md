# CLAUDE.md

Conventions for AI agents (and humans) working in this repository.

## Project

Super Projector: a superannuation balance projector. The .NET API calculates the
projected balance at age 67; the React web app displays it. Keep everything small
and simple: this is a learning project.

## Layout

- `api/`: .NET API
- `web/`: React web app
- `infra/`: Bicep templates
- `mobile/`: reserved for later
- `.github/workflows/`: GitHub Actions pipelines

## Technology

- **API:** .NET 10 LTS, C# minimal API. Tests use xUnit.
- **Web:** React + TypeScript, built with Vite.
- **Infrastructure:** Bicep only. All Azure resources are defined as code in
  `infra/`; never make manual changes in the Azure portal.

## Security

- Never commit secrets, keys, connection strings or `.env` files.
- GitHub Actions logs in to Azure with OIDC federated credentials; do not add
  client secrets or publish profiles.

## Code quality

- Every change to business logic (e.g. the projection calculation) must include
  unit tests.
- Keep code small and readable, with comments that explain the *why* for a
  learning audience.

## Writing style

- Use Australian English in docs and comments (e.g. "organisation", "colour",
  "behaviour").
