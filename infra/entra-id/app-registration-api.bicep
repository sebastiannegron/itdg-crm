extension 'br:mcr.microsoft.com/bicep/extensions/microsoftgraph/v1.0:0.1.9-preview'

@description('Application display name')
param displayName string = 'ITDG CRM API'

@description('Application unique name (must be globally unique)')
param uniqueName string

@description('API backend hostname (e.g., itdg-crm-dev-api.azurewebsites.net)')
param apiHostName string = ''

// Well-known Microsoft Graph app ID
var microsoftGraphAppId = '00000003-0000-0000-c000-000000000000'

// Microsoft Graph permission IDs
var graphPermissions = {
  userRead: 'e1fe6dd8-ba31-4d61-89e7-88639da4683d' // User.Read (delegated)
  mailSend: 'b633e1c5-b582-4048-a93e-9f11b44c7e96' // Mail.Send (application)
}

// Custom app roles for RBAC
var appRoles = [
  {
    allowedMemberTypes: ['User']
    description: 'Administrators can manage all aspects of the CRM'
    displayName: 'Administrator'
    id: '8e4a9a2c-1b3d-4f5e-8a7b-2c9d0e1f3a5b'
    isEnabled: true
    value: 'Administrator'
  }
  {
    allowedMemberTypes: ['User']
    description: 'Associates can manage assigned clients'
    displayName: 'Associate'
    id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    isEnabled: true
    value: 'Associate'
  }
  {
    allowedMemberTypes: ['User']
    description: 'Client portal users can view their own data'
    displayName: 'ClientPortal'
    id: 'f9e8d7c6-b5a4-3210-fedc-ba9876543210'
    isEnabled: true
    value: 'ClientPortal'
  }
]

// OAuth2 scopes exposed by the API
var oauth2Scopes = [
  {
    adminConsentDescription: 'Allows the app to access the CRM API on behalf of the signed-in user'
    adminConsentDisplayName: 'Access ITDG CRM API'
    id: 'd1c2b3a4-5e6f-7890-1234-567890abcdef'
    isEnabled: true
    type: 'User'
    userConsentDescription: 'Allows the app to access the CRM API on your behalf'
    userConsentDisplayName: 'Access ITDG CRM API'
    value: 'access_as_user'
  }
]

resource apiApp 'Microsoft.Graph/applications@v1.0' = {
  uniqueName: uniqueName
  displayName: displayName
  signInAudience: 'AzureADMyOrg'
  tags: ['webApi']
  appRoles: appRoles
  api: {
    requestedAccessTokenVersion: 2
    oauth2PermissionScopes: oauth2Scopes
  }
  requiredResourceAccess: [
    {
      resourceAppId: microsoftGraphAppId
      resourceAccess: [
        { id: graphPermissions.userRead, type: 'Scope' } // Delegated: User.Read
        { id: graphPermissions.mailSend, type: 'Role' } // Application: Mail.Send
      ]
    }
  ]
  web: {
    implicitGrantSettings: {
      enableAccessTokenIssuance: false
      enableIdTokenIssuance: false
    }
  }
}

// Service principal for the API app
resource apiSp 'Microsoft.Graph/servicePrincipals@v1.0' = {
  appId: apiApp.appId
}

// Set the identifier URI after creation (requires appId)
resource apiAppUri 'Microsoft.Graph/applications@v1.0' = {
  uniqueName: apiApp.uniqueName
  displayName: displayName
  signInAudience: 'AzureADMyOrg'
  identifierUris: ['api://${apiApp.appId}']
}

output apiAppId string = apiApp.appId
output apiAppObjectId string = apiApp.id
output apiServicePrincipalId string = apiSp.id
