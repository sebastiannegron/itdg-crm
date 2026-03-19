#!/bin/bash
# Deploy Entra ID app registrations using Azure CLI
# The microsoftGraph extension requires az CLI deployment (not az bicep)
#
# Usage: ./deploy.sh <environment> <resource-group>
# Example: ./deploy.sh dev rg-itdg-crm-dev

set -euo pipefail

ENVIRONMENT="${1:?Usage: ./deploy.sh <environment> <resource-group>}"
RESOURCE_GROUP="${2:?Usage: ./deploy.sh <environment> <resource-group>}"
BASE_NAME="itdg-crm"

echo "=== Deploying API app registration ==="
API_OUTPUT=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file app-registration-api.bicep \
  --parameters uniqueName="${BASE_NAME}-${ENVIRONMENT}-api" \
  --query 'properties.outputs' \
  --output json)

API_APP_ID=$(echo "$API_OUTPUT" | jq -r '.apiAppId.value')
echo "API App ID: $API_APP_ID"

echo "=== Deploying Web app registration ==="
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file app-registration-web.bicep \
  --parameters \
    uniqueName="${BASE_NAME}-${ENVIRONMENT}-web" \
    apiAppId="$API_APP_ID" \
    redirectUris="[\"http://localhost:3000/en-pr\", \"http://localhost:3000/es-pr\", \"https://${BASE_NAME}-${ENVIRONMENT}-web.azurewebsites.net/en-pr\"]"

echo ""
echo "=== Post-deployment steps ==="
echo "1. Grant admin consent for Mail.Send (application permission):"
echo "   az ad app permission admin-consent --id $API_APP_ID"
echo ""
echo "2. Add the API App ID to your API appsettings:"
echo "   AzureAd__ClientId=$API_APP_ID"
