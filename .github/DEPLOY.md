# Azure CI/CD Pipeline

The workflow **`.github/workflows/azure-deploy.yml`** builds and deploys ShopNet to Azure Web App on every push to `main` (and on manual run).

## What it does

1. **Build**
   - Installs Node.js and builds the Angular client (production) into `API/wwwroot`.
   - Restores and builds the .NET API, then publishes it (including `wwwroot`) to `./publish`.
   - Uploads the publish folder as an artifact.

2. **Deploy**
   - Downloads the artifact and deploys it to the configured Azure Web App using the Azure login action.

## Configuration

- **App name**: Set in the workflow as `AZURE_WEBAPP_NAME` (default: `shopnet2k6`). Change it in the `env` section of `azure-deploy.yml` if your Web App has another name.
- **Secrets**: The workflow uses the secrets created when you connect the repo to Azure (e.g. via Azure Portal → App Service → Deployment Center → GitHub). Required secrets:
  - `AZUREAPPSERVICE_CLIENTID_...`
  - `AZUREAPPSERVICE_TENANTID_...`
  - `AZUREAPPSERVICE_SUBSCRIPTIONID_...`

If you use a different connection, add the same secret names in **Settings → Secrets and variables → Actions** with your service principal’s Client ID, Tenant ID, and Subscription ID.

## Running manually

In GitHub: **Actions** → select **Azure CI/CD - ShopNet** → **Run workflow**.
