@description('App Service name')
param name string

@description('Azure region')
param location string

@description('Application Insights connection string')
param appInsightsConnectionString string

@description('Key Vault URI for secret references')
param keyVaultUri string = ''

@description('Entra ID client ID for JWT validation')
param entraIdClientId string = ''

@description('Entra ID tenant ID')
param entraIdTenantId string = subscription().tenantId

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${name}-plan'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: true
  }
  kind: 'linux'
}

resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'AzureAd__ClientId'
          value: entraIdClientId
        }
        {
          name: 'AzureAd__TenantId'
          value: entraIdTenantId
        }
        {
          name: 'KeyVault__Uri'
          value: keyVaultUri
        }
      ]
    }
    httpsOnly: true
  }
  identity: {
    type: 'SystemAssigned'
  }
}

output appServiceName string = appService.name
output principalId string = appService.identity.principalId
output defaultHostName string = appService.properties.defaultHostName
