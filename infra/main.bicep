param applicationName string = 'scrapez'
param location string = resourceGroup().location
param functionsName string = 'ctwFunction'
param principalId string
param tenantId string

module keyVault 'modules/key-vault.bicep' = {
  name: 'keyVault'
  params: {
    tenantId: tenantId
    location: location
  }
}

var secretsOfficerPrincipalIds = [
  principalId
]

module keyVaultSecretsOfficers 'modules/key-vault-secrets-officer.bicep' = {
  name: 'key-vault-secrets-officer'
  params: {
    keyVaultName: keyVault.outputs.name
    principalIds: secretsOfficerPrincipalIds
  }
}

module storageAccount 'modules/storage-account.bicep' = {
  name: 'storageAccount'
  params: {
    applicationName: applicationName
    location: location
  }
}

module mappingTable 'modules/table-storage.bicep' = {
  name: 'items-table'
  params: {
    storageAccountName: storageAccount.name
    tableName: 'items'
  }
}

module appInsights 'modules/application-insights.bicep' = {
  name: 'application-insights'
  params: {
    functionFullName: functionsName
    applicationName: applicationName
    location: location
  }
}

module hostingPlan 'modules/hosting-plan.bicep' = {
  name: 'hosting-plan'
  params: {
    applicationName: applicationName
    location: location
  }
}

module functions 'modules/function.bicep' = {
  name: 'functions'
  params: {
    hostingPlanName: hostingPlan.outputs.planName
    functionFullName: functionsName
    storageAccountName: storageAccount.name
    applicationInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    location: location
  }
}

var tableDataContributorPrincipalIds = [
  functions.outputs.principalId
  principalId
]

module tableDataContributors 'modules/table-storage-data-contributor.bicep' = {
  name: 'table-storage-data-contributor'
  params: {
    principalIds: tableDataContributorPrincipalIds
    storageAccountName: storageAccount.name
  }
}
