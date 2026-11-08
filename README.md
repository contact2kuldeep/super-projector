# Super Projector

A small learning project that projects a superannuation balance at age 67.
A member enters their age, current balance, salary and contribution rate; a .NET API
calculates the projected balance and a React web app displays it.

## Architecture

| Part | Folder | Technology | Hosting |
| --- | --- | --- | --- |
| API | `api/` | .NET 10 C# minimal API | Azure App Service |
| Web app | `web/` | React + TypeScript (Vite) | Azure Static Web Apps |
| Infrastructure | `infra/` | Bicep | Azure |
| CI/CD | `.github/workflows/` | GitHub Actions, logging in to Azure with OIDC (no stored secrets) | GitHub |
| Mobile | `mobile/` | Reserved for a future mobile client | — |

## Prerequisites

- [Homebrew](https://brew.sh/)
- [Git](https://git-scm.com/)
- [Node.js](https://nodejs.org/) (LTS)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (version pinned in `global.json`)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- [GitHub CLI](https://cli.github.com/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Claude Code](https://claude.com/claude-code)

## Getting started

_To be written once the API and web app have been scaffolded._

## Web app

The React app in `web/` calls the API, so start both, each in its own terminal:

```sh
# Terminal 1: the API, on http://localhost:5080
cd api
dotnet run --project src/SuperProjector.Api

# Terminal 2: the web app, on http://localhost:5173
cd web
npm install   # first time only
npm run dev
```

Then open http://localhost:5173.

Other commands, run from `web/`: `npm run test` (Vitest), `npm run lint`
(oxlint) and `npm run build` (type-check and build into `web/dist/`).

### `VITE_API_BASE_URL`

The base URL of the API, without a trailing slash. Locally it is set to
`http://localhost:5080` in `web/.env.development`, which is committed because it
holds no secrets. To use a different value on your machine, copy
`web/.env.example` to `web/.env.local` (git-ignored) and change it there.

Vite builds this value into the JavaScript bundle when you run `npm run build`,
so it must be set to the deployed API's URL at build time, and it is visible
to anyone using the site. Never put secrets in a `VITE_` variable.

The API only accepts browser requests from the origins listed in its
`Cors:AllowedOrigins` setting, so the web app's URL must be listed there too.

## Infrastructure

All Azure resources are defined in Bicep in `infra/`; never change them by
hand in the portal.

- `infra/main.bicep`: the template. It creates a Log Analytics workspace,
  workspace-based Application Insights, a Free-tier Static Web App (web), a
  Linux App Service Plan and a Web App running .NET 10 (API). Every resource is
  tagged `project=super-projector`, `env=<environment>`, `managedBy=bicep`.
- `infra/main.bicepparam`: parameter values for the `dev` environment.

Resource names end in a short suffix generated from the resource group ID, so
they are unique across Azure and stay the same on every deployment.

The resource group `rg-superprojector-dev` (australiaeast) must already exist.
From the repo root, check and preview the template:

```sh
az bicep build --file infra/main.bicep        # compile and check for errors
az deployment group what-if \
  --resource-group rg-superprojector-dev \
  --parameters infra/main.bicepparam          # preview changes; deploys nothing
```

Useful parameters (override with `--parameters name=value`):

| Parameter | Default | Notes |
| --- | --- | --- |
| `location` | resource group's region | Region for the API and monitoring. |
| `appServiceSkuName` | `F1` (free) | Use `B1` if F1 is unavailable. |
| `staticWebAppLocation` | `eastasia` | Static Web Apps only run in a few regions; content is still served globally. |

### Known constraints

- **API and monitoring are in australiasoutheast (Melbourne), not
  australiaeast.** Our new subscription has 0 F1 App Service quota in
  australiaeast, so deployments fail there with `SubscriptionIsOverQuotaForSku`.
  `main.bicepparam` sets `location = 'australiasoutheast'` for dev, which still
  keeps data onshore in Australia. The resource group itself stays in
  australiaeast; that only affects where its metadata is stored.
- **The Static Web App is in eastasia.** Static Web Apps supports only a few
  regions, none of them in Australia. Its content is still served from a
  global CDN, and it hosts static files only (HTML, JavaScript, CSS) with no
  member data. Member inputs go straight from the browser to the API in
  Australia.

## CI/CD

Pipelines live in `.github/workflows/` and run on GitHub Actions. They log in to
Azure with **OIDC**: GitHub issues a short-lived token that Azure trusts through
a federated credential on an Entra ID app registration. No Azure passwords or
keys are stored in GitHub.

### Infrastructure pipeline (`infra.yml`)

Runs when changes to `infra/**` or the workflow file are pushed to `main`, or
manually from the Actions tab (**Run workflow**, on `main`).

1. **validate**: compiles `infra/main.bicep` and runs a what-if preview,
   which also runs Azure's preflight checks such as quota.
2. **deploy** (only if validate passes): runs `az deployment group create`
   against `rg-superprojector-dev`, then lists the API and web URLs and app
   names in the run's summary.

Only one infra run happens at a time (concurrency group `infra-dev`); a new run
waits rather than cancelling one in progress.

**Repository secrets required** (these are IDs, not passwords):

| Secret | Value |
| --- | --- |
| `AZURE_CLIENT_ID` | Application (client) ID of the Entra ID app registration |
| `AZURE_TENANT_ID` | Directory (tenant) ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

The app's federated credential trusts the subject
`repo:<owner>/super-projector:ref:refs/heads/main`, so the jobs deliberately
have no GitHub `environment:`. Adding one changes the token subject and breaks
login, unless a matching federated credential is added first. The identity
also needs a role on the resource group that can create resources (e.g.
Contributor).
