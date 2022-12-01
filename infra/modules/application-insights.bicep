param applicationName string
param location string = resourceGroup().location
param functionFullName string // Needed for the tag (circular reference https://markheath.net/post/azure-functions-bicep)

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: 'appi-${applicationName}-${uniqueString(resourceGroup().id)}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
  tags: {
    'hidden-link:/subscriptions/${subscription().id}/resourceGroups/${resourceGroup().name}/providers/Microsoft.Web/sites/${functionFullName}': 'Resource'
  }
}

output instrumentationKey string = appInsights.properties.InstrumentationKey
output connectionString string = appInsights.properties.ConnectionString
