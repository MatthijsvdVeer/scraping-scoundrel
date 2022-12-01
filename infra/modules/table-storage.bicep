param tableName string
param storageAccountName string

resource tableServices 'Microsoft.Storage/storageAccounts/tableServices@2021-09-01' existing = {
  name: '${storageAccountName}/default'
}

resource table 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-09-01' = {
  name: tableName
  parent: tableServices
}
