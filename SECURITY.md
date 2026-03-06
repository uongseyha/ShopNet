# Security practices

## Secrets and configuration

- **Never commit** real passwords, connection strings, API keys, or tokens.
- **Ignore list**: The repo `.gitignore` excludes:
  - `API/appsettings.json`, `API/appsettings.Development.json`, `API/appsettings.Production.json`
  - `.env` and `.env.*` (except `.env.example`)
- **Local development**:
  - Copy `API/appsettings.Development.example.json` to `API/appsettings.Development.json` and fill in real values only on your machine.
  - Copy `.env.example` to `.env` for Docker (e.g. `MSSQL_SA_PASSWORD`) and keep `.env` out of version control.
- **Production**: Use Azure App Service Application Settings / Key Vault (or env vars) for connection strings and secrets. Do not store them in appsettings in the repo.

## If you already committed secrets

1. Rotate the exposed secrets (passwords, keys, connection strings) immediately.
2. Remove the file from history (e.g. `git filter-branch` or BFG Repo-Cleaner) or create a new repo and force-push, then rotate secrets again.

## CI/CD

- GitHub Actions use **secrets** (Settings → Secrets and variables → Actions) for Azure credentials. Never put production credentials in workflow YAML.
- Use the least-privision service principal for deployment (e.g. only needed for the target App Service).

## Dependency and supply chain

- Run `npm audit` and `dotnet list package --vulnerable` periodically; fix or accept known issues.
- Prefer `npm ci` in CI (with a committed `package-lock.json`) for reproducible installs.
