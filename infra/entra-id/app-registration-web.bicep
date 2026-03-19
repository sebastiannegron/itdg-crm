extension 'br:mcr.microsoft.com/bicep/extensions/microsoftgraph/v1.0:0.1.9-preview'

@description('Application display name')
param displayName string = 'ITDG CRM Web'

@description('Application unique name (must be globally unique)')
param uniqueName string

@description('API app registration client ID (from app-registration-api deployment)')
param apiAppId string

@description('API scope ID for access_as_user')
param apiScopeId string = 'd1c2b3a4-5e6f-7890-1234-567890abcdef'

@description('Frontend app redirect URIs')
param redirectUris array = []

// Well-known Microsoft Graph app ID
var microsoftGraphAppId = '00000003-0000-0000-c000-000000000000'

// Microsoft Graph permission IDs
var graphPermissions = {
  userRead: 'e1fe6dd8-ba31-4d61-89e7-88639da4683d' // User.Read (delegated)
  openidProfile: '37f7f235-527c-4136-accd-4a02d197296e' // openid (delegated)
  offlineAccess: '7427e0e9-2fba-42fe-b0c0-848c9e6a8182' // offline_access (delegated)
}

resource webApp 'Microsoft.Graph/applications@v1.0' = {
  uniqueName: uniqueName
  displayName: displayName
  signInAudience: 'AzureADMyOrg'
  tags: ['webApp']
  spa: {
    redirectUris: redirectUris
  }
  requiredResourceAccess: [
    {
      // Microsoft Graph — delegated permissions for sign-in
      resourceAppId: microsoftGraphAppId
      resourceAccess: [
        { id: graphPermissions.userRead, type: 'Scope' }
        { id: graphPermissions.openidProfile, type: 'Scope' }
        { id: graphPermissions.offlineAccess, type: 'Scope' }
      ]
    }
    {
      // ITDG CRM API — delegated access_as_user scope
      resourceAppId: apiAppId
      resourceAccess: [
        { id: apiScopeId, type: 'Scope' }
      ]
    }
  ]
}

// Service principal for the web app
resource webSp 'Microsoft.Graph/servicePrincipals@v1.0' = {
  appId: webApp.appId
}

output webAppId string = webApp.appId
output webAppObjectId string = webApp.id
