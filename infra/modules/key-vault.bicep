param location string = resourceGroup().location
param tenantId string

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: 'kv-${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    enabledForTemplateDeployment: true
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: tenantId
    softDeleteRetentionInDays: 90
    enableSoftDelete: true
    enableRbacAuthorization: true
  }
}

output name string = keyVault.name
