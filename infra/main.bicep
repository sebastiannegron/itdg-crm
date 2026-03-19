targetScope = 'resourceGroup'

@description('Environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Base name for all resources')
param baseName string = 'itdg-crm'

@description('SQL Server administrator login')
@secure()
param sqlAdminLogin string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('Entra ID client ID for the API app registration (from entra-id deployment)')
param entraIdApiClientId string = ''

@description('Entra ID client ID for the Web app registration (from entra-id deployment)')
param entraIdWebClientId string = ''

var resourcePrefix = '${baseName}-${environment}'

// --- Shared infrastructure ---

module appInsights 'modules/app-insights.bicep' = {
  name: 'appInsights'
  params: {
    name: '${resourcePrefix}-ai'
    location: location
  }
}

module sqlDatabase 'modules/sql-database.bicep' = {
  name: 'sqlDatabase'
  params: {
    serverName: '${resourcePrefix}-sql'
    databaseName: '${resourcePrefix}-db'
    location: location
    adminLogin: sqlAdminLogin
    adminPassword: sqlAdminPassword
  }
}

// --- App Services (deployed before Key Vault so we can grant access) ---

module apiAppService 'modules/app-service-api.bicep' = {
  name: 'apiAppService'
  params: {
    name: '${resourcePrefix}-api'
    location: location
    appInsightsConnectionString: appInsights.outputs.connectionString
    entraIdClientId: entraIdApiClientId
    entraIdTenantId: subscription().tenantId
  }
}

module webAppService 'modules/app-service-web.bicep' = {
  name: 'webAppService'
  params: {
    name: '${resourcePrefix}-web'
    location: location
    appInsightsConnectionString: appInsights.outputs.connectionString
    apiBaseUrl: 'https://${apiAppService.outputs.defaultHostName}'
    entraIdClientId: entraIdWebClientId
    entraIdTenantId: subscription().tenantId
  }
}

// --- Key Vault (grants Secrets User role to both App Services) ---

module keyVault 'modules/key-vault.bicep' = {
  name: 'keyVault'
  params: {
    name: '${resourcePrefix}-kv'
    location: location
    secretsUserPrincipalIds: [
      apiAppService.outputs.principalId
      webAppService.outputs.principalId
    ]
  }
}

// --- Outputs ---

output apiAppServiceName string = apiAppService.outputs.appServiceName
output apiDefaultHostName string = apiAppService.outputs.defaultHostName
output webAppServiceName string = webAppService.outputs.appServiceName
output webDefaultHostName string = webAppService.outputs.defaultHostName
output sqlServerFqdn string = sqlDatabase.outputs.serverFqdn
output keyVaultName string = keyVault.outputs.keyVaultName
output keyVaultUri string = keyVault.outputs.keyVaultUri
output appInsightsName string = appInsights.outputs.appInsightsName
