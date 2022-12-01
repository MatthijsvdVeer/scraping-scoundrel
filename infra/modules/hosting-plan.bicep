param applicationName string
param location string = resourceGroup().location

resource hostingPlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: 'plan-${applicationName}-${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

output planName string = hostingPlan.name
