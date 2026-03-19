targetScope = 'resourceGroup'

@description('Environment name (dev, staging, prod)')
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

var resourcePrefix = '${baseName}-${environment}'

module appInsights 'modules/app-insights.bicep' = {
  name: 'appInsights'
  params: {
    name: '${resourcePrefix}-ai'
    location: location
  }
}

module keyVault 'modules/key-vault.bicep' = {
  name: 'keyVault'
  params: {
    name: '${resourcePrefix}-kv'
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

module apiAppService 'modules/app-service-api.bicep' = {
  name: 'apiAppService'
  params: {
    name: '${resourcePrefix}-api'
    location: location
    appInsightsConnectionString: appInsights.outputs.connectionString
  }
}

module webAppService 'modules/app-service-web.bicep' = {
  name: 'webAppService'
  params: {
    name: '${resourcePrefix}-web'
    location: location
    appInsightsConnectionString: appInsights.outputs.connectionString
  }
}

output apiAppServiceName string = apiAppService.outputs.appServiceName
output webAppServiceName string = webAppService.outputs.appServiceName
output sqlServerFqdn string = sqlDatabase.outputs.serverFqdn
output keyVaultName string = keyVault.outputs.keyVaultName
output appInsightsName string = appInsights.outputs.appInsightsName
