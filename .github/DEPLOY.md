# Azure CI/CD Pipeline

The workflow **`.github/workflows/main_shopnet2k6.yml`** builds and deploys ShopNet to Azure Web App on every push to `main` (and on manual run). You do **not** need to commit `ng build` output; the pipeline builds the Angular client in CI.

## What it does

1. **Build**
   - Installs Node.js, runs `npm install` and `ng build --configuration production` in `Client/`. Output goes to `API/wwwroot` (per `angular.json`).
   - Restores and builds the .NET API, then publishes it (including `wwwroot`) to the artifact folder.
   - Uploads the publish folder as an artifact.

2. **Deploy**
   - Downloads the artifact and deploys it to the configured Azure Web App using the Azure login action.

## Configuration

- **App name**: Set in the workflow as `app-name: 'shopnet2k6'` in the Deploy step. Change it in `.github/workflows/main_shopnet2k6.yml` if your Web App has another name.
- **Secrets**: The workflow uses the secrets created when you connect the repo to Azure (e.g. via Azure Portal → App Service → Deployment Center → GitHub). Required secrets:
  - `AZUREAPPSERVICE_CLIENTID_...`
  - `AZUREAPPSERVICE_TENANTID_...`
  - `AZUREAPPSERVICE_SUBSCRIPTIONID_...`

If you use a different connection, add the same secret names in **Settings → Secrets and variables → Actions** with your service principal’s Client ID, Tenant ID, and Subscription ID.

## Running manually

In GitHub: **Actions** → select **Build and deploy ASP.Net Core app to Azure Web App - shopnet2k6** → **Run workflow**.

## Docker

The **Dockerfile** builds the Angular client in a `node:20-alpine` stage (output to `API/wwwroot`), then builds and publishes the .NET API. Run from repo root:

```bash
docker build -t shopnet .
docker run -p 8080:80 shopnet
```
