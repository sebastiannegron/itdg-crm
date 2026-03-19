using '../main.bicep'

param environment = 'dev'
param location = 'eastus2'
param sqlAdminLogin = 'sqladmin'
param sqlAdminPassword = '' // Set via deployment parameter or Key Vault
