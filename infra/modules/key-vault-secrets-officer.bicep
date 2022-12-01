param principalIds array
param keyVaultName string

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: keyVaultName
}

var keyVaultSecretsOfficer = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
@description('Key Vault Secrets Officer')
resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: keyVaultSecretsOfficer
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [for principalId in principalIds: {
  scope: keyVault
  name: guid(keyVault.id, principalId, keyVaultSecretsOfficer)
  properties: {
    roleDefinitionId: contributorRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
