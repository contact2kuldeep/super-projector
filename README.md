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
