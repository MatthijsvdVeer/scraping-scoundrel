param principalIds array
param storageAccountName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' existing = {
  name: storageAccountName
}

var adtDataOwner = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
@description('This is the built-in Storage Table Data Contributor role.')
resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: adtDataOwner
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [for principalId in principalIds: {
  scope: storageAccount
  name: guid(storageAccount.id, principalId, adtDataOwner)
  properties: {
    roleDefinitionId: contributorRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
