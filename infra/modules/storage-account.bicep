param applicationName string
param location string = resourceGroup().location

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: 'st${toLower(take(applicationName,8))}${uniqueString(resourceGroup().id)}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
  resource tableService 'tableServices@2021-09-01' = {
    name: 'default'
  }
}
