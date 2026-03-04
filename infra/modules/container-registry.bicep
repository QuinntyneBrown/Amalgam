@description('Name of the Azure Container Registry')
param name string

@description('Azure region for the resource')
param location string

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: name
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

@description('The login server URL of the container registry')
output loginServer string = acr.properties.loginServer

@description('The name of the container registry')
output name string = acr.name

@description('The admin username of the container registry')
output username string = acr.listCredentials().username

@description('The admin password of the container registry')
output password string = acr.listCredentials().passwords[0].value
