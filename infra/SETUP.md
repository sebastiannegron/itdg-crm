# ITDG CRM — Infrastructure Setup Guide

Step-by-step guide for provisioning all external services and Azure resources required by the CRM platform. Follow the sections in order.

---

## Prerequisites

Before starting, ensure you have:

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (2.60+)
- [Bicep CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install) (bundled with Azure CLI)
- An Azure subscription with Owner or Contributor access
- A Google Cloud project with billing enabled
- A Google Workspace account (for Gmail and Drive integration)

Log in to Azure CLI:

```bash
az login
az account set --subscription "<your-subscription-id>"
```

---

## Step 1: Create the Azure Resource Group

```bash
# Dev environment
az group create --name rg-itdg-crm-dev --location eastus2

# Staging (when ready)
# az group create --name rg-itdg-crm-staging --location eastus2

# Production (when ready)
# az group create --name rg-itdg-crm-prod --location eastus2
```

---

## Step 2: Deploy Entra ID App Registrations

Entra ID (Azure AD) provides authentication for both the API and the frontend. This creates two app registrations:

- **ITDG CRM API** — backend with RBAC roles (Administrator, Associate, ClientPortal) and Mail.Send permission
- **ITDG CRM Web** — frontend SPA with redirect URIs

### Deploy

```bash
cd infra/entra-id
chmod +x deploy.sh
./deploy.sh dev rg-itdg-crm-dev
```

### Post-deployment

1. **Grant admin consent** for the Mail.Send application permission:

   ```bash
   az ad app permission admin-consent --id <API_APP_ID>
   ```

   Replace `<API_APP_ID>` with the value printed by the deploy script.

2. **Save the App IDs** — you will need them for the infrastructure deployment:
   - `API_APP_ID` — goes into `entraIdApiClientId` parameter
   - `WEB_APP_ID` — goes into `entraIdWebClientId` parameter

3. **Update the parameters file** (`infra/parameters/dev.bicepparam`):

   ```bicep
   param entraIdApiClientId = '<API_APP_ID>'
   param entraIdWebClientId = '<WEB_APP_ID>'
   ```

4. **Update appsettings** (`src/api/Itdg.Crm.Api/appsettings.json`):

   ```json
   "AzureAd": {
     "Instance": "https://login.microsoftonline.com/",
     "TenantId": "<your-tenant-id>",
     "ClientId": "<API_APP_ID>",
     "Audience": "api://<API_APP_ID>/.default"
   }
   ```

### Assign users to roles

After app registration, assign users to the RBAC roles in the Azure Portal:

1. Go to **Entra ID > Enterprise Applications > ITDG CRM API**
2. Click **Users and groups > Add user/group**
3. Assign each user to their role: `Administrator`, `Associate`, or `ClientPortal`

---

## Step 3: Set Up Google Cloud Project (Gmail + Drive)

The CRM integrates with Google Workspace for email mirroring (Gmail API) and document storage (Google Drive API).

### 3.1 Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click **Select a project > New Project**
3. Name: `ITDG CRM` (or your preferred name)
4. Click **Create**

### 3.2 Enable APIs

Navigate to **APIs & Services > Library** and enable:

- **Gmail API**
- **Google Drive API**
- **Google Calendar API**

### 3.3 Configure OAuth Consent Screen

1. Go to **APIs & Services > OAuth consent screen**
2. Select **Internal** (if using Google Workspace) or **External** (if testing with personal accounts)
3. Fill in:
   - **App name:** `ITDG CRM`
   - **User support email:** your admin email
   - **Developer contact:** your admin email
4. Add scopes:
   - `https://www.googleapis.com/auth/gmail.readonly`
   - `https://www.googleapis.com/auth/gmail.send`
   - `https://www.googleapis.com/auth/gmail.modify`
   - `https://www.googleapis.com/auth/drive.file`
   - `https://www.googleapis.com/auth/calendar.events`
5. Save and continue

### 3.4 Create OAuth 2.0 Credentials

1. Go to **APIs & Services > Credentials**
2. Click **Create Credentials > OAuth client ID**
3. Application type: **Web application**
4. Name: `ITDG CRM Backend`
5. Authorized redirect URIs:
   - `https://localhost:5001/api/v1/auth/google/callback` (development)
   - `https://itdg-crm-dev-api.azurewebsites.net/api/v1/auth/google/callback` (dev deployment)
6. Click **Create**
7. **Save the Client ID and Client Secret** — you will need these for the next step

### 3.5 Configure the Application

Update `appsettings.json` (or store in Key Vault for production):

```json
"Gmail": {
  "ClientId": "<GOOGLE_CLIENT_ID>",
  "ClientSecret": "<GOOGLE_CLIENT_SECRET>",
  "ApplicationName": "ITDG-CRM"
},
"GoogleDrive": {
  "ClientId": "<GOOGLE_CLIENT_ID>",
  "ClientSecret": "<GOOGLE_CLIENT_SECRET>",
  "ApplicationName": "ITDG-CRM"
}
```

> **Note:** Gmail and Google Drive share the same OAuth credentials since they belong to the same Google Cloud project. The Client ID and Client Secret are identical for both sections.

> **Production:** Never commit real credentials. Store them in Azure Key Vault and reference via the app configuration. See Step 5.

---

## Step 4: Deploy Azure Infrastructure

### 4.1 Set the SQL admin password

Choose a strong password for the SQL Server admin account. You will pass it as a deployment parameter (never commit it).

### 4.2 Deploy

```bash
cd infra

# Deploy with parameters file + secure password override
az deployment group create \
  --resource-group rg-itdg-crm-dev \
  --template-file main.bicep \
  --parameters parameters/dev.bicepparam \
  --parameters sqlAdminPassword='<YOUR_STRONG_PASSWORD>'
```

### 4.3 Verify deployment

After deployment completes, note the outputs:

- **API hostname:** `itdg-crm-dev-api.azurewebsites.net`
- **Web hostname:** `itdg-crm-dev-web.azurewebsites.net`
- **SQL Server FQDN:** `itdg-crm-dev-sql.database.windows.net`
- **Key Vault URI:** `https://itdg-crm-dev-kv.vault.azure.net/`

---

## Step 5: Store Secrets in Azure Key Vault

The App Services have managed identities with Key Vault Secrets User role pre-assigned by the Bicep template. Store all sensitive configuration here.

```bash
KV_NAME="itdg-crm-dev-kv"

# Database connection string
az keyvault secret set --vault-name $KV_NAME \
  --name "ConnectionStrings--CrmDb" \
  --value "Server=itdg-crm-dev-sql.database.windows.net;Database=itdg-crm-dev-db;User Id=sqladmin;Password=<YOUR_PASSWORD>;Encrypt=True;TrustServerCertificate=False;"

# Google OAuth credentials
az keyvault secret set --vault-name $KV_NAME \
  --name "Gmail--ClientId" \
  --value "<GOOGLE_CLIENT_ID>"

az keyvault secret set --vault-name $KV_NAME \
  --name "Gmail--ClientSecret" \
  --value "<GOOGLE_CLIENT_SECRET>"

az keyvault secret set --vault-name $KV_NAME \
  --name "GoogleDrive--ClientId" \
  --value "<GOOGLE_CLIENT_ID>"

az keyvault secret set --vault-name $KV_NAME \
  --name "GoogleDrive--ClientSecret" \
  --value "<GOOGLE_CLIENT_SECRET>"
```

### Configure App Services to use Key Vault

Add Key Vault references to the App Service configuration:

```bash
API_APP="itdg-crm-dev-api"

az webapp config appsettings set --name $API_APP \
  --resource-group rg-itdg-crm-dev \
  --settings \
    ConnectionStrings__CrmDb="@Microsoft.KeyVault(VaultName=itdg-crm-dev-kv;SecretName=ConnectionStrings--CrmDb)" \
    Gmail__ClientId="@Microsoft.KeyVault(VaultName=itdg-crm-dev-kv;SecretName=Gmail--ClientId)" \
    Gmail__ClientSecret="@Microsoft.KeyVault(VaultName=itdg-crm-dev-kv;SecretName=Gmail--ClientSecret)" \
    GoogleDrive__ClientId="@Microsoft.KeyVault(VaultName=itdg-crm-dev-kv;SecretName=GoogleDrive--ClientId)" \
    GoogleDrive__ClientSecret="@Microsoft.KeyVault(VaultName=itdg-crm-dev-kv;SecretName=GoogleDrive--ClientSecret)"
```

---

## Step 6: Run Database Migrations

After infrastructure is deployed and the connection string is configured:

```bash
cd src/api

# Apply EF Core migrations
dotnet ef database update --project Itdg.Crm.Api.Infrastructure --startup-project Itdg.Crm.Api
```

For the dev environment with LocalDB:

```bash
# The connection string in appsettings.Development.json points to LocalDB
dotnet ef database update --project Itdg.Crm.Api.Infrastructure --startup-project Itdg.Crm.Api
```

---

## Step 7: Verify the Deployment

### Backend

```bash
# Health check (if endpoint exists)
curl https://itdg-crm-dev-api.azurewebsites.net/health

# Swagger UI
open https://itdg-crm-dev-api.azurewebsites.net/swagger
```

### Frontend

```bash
open https://itdg-crm-dev-web.azurewebsites.net
```

### Key Vault access

Verify the App Services can read secrets:

```bash
# Check App Service logs for Key Vault errors
az webapp log tail --name itdg-crm-dev-api --resource-group rg-itdg-crm-dev
```

---

## Quick Reference

| Resource | Dev Environment |
|----------|----------------|
| Resource Group | `rg-itdg-crm-dev` |
| API App Service | `itdg-crm-dev-api.azurewebsites.net` |
| Web App Service | `itdg-crm-dev-web.azurewebsites.net` |
| SQL Server | `itdg-crm-dev-sql.database.windows.net` |
| SQL Database | `itdg-crm-dev-db` |
| Key Vault | `itdg-crm-dev-kv` |
| App Insights | `itdg-crm-dev-ai` |
| Entra ID API App | `ITDG CRM API` |
| Entra ID Web App | `ITDG CRM Web` |
| Google Cloud Project | `ITDG CRM` |

## Troubleshooting

### "Options validation failed" on startup

The API validates all options on startup. If a required configuration value is missing, the app will fail to start. Check:

- Key Vault secrets are set (Step 5)
- App Service configuration references are correct
- `appsettings.json` has the correct section structure (`Gmail`, `GoogleDrive`, `AzureAd`)

### "AADSTS700016" or authentication errors

- Verify the Entra ID App IDs match between `appsettings.json` and the app registrations
- Ensure admin consent was granted for Mail.Send
- Check that users are assigned to roles in the Enterprise Application

### Google API "access_denied" or "invalid_client"

- Verify the OAuth Client ID and Secret are correct
- Check that the redirect URIs match exactly (including trailing slashes)
- Ensure the required APIs (Gmail, Drive, Calendar) are enabled in the Google Cloud project
- If using Internal consent screen, the user must be in the same Google Workspace organization
