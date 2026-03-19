@description('App Service name')
param name string

@description('Azure region')
param location string

@description('Application Insights connection string')
param appInsightsConnectionString string

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
      linuxFxVersion: 'NODE|22-lts'
      alwaysOn: true
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
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
