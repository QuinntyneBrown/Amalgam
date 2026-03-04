@description('Name of the Container App')
param name string

@description('Azure region for the resource')
param location string

@description('Resource ID of the Container Apps Environment')
param environmentId string

@description('Container image to deploy')
param containerImage string

@description('Container registry login server')
param registryServer string

@description('Container registry username')
param registryUsername string

@secure()
@description('Container registry password')
param registryPassword string

@description('FQDN of the API Container App for proxying /api/ requests')
param apiFqdn string

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: name
  location: location
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 80
      }
      registries: [
        {
          server: registryServer
          username: registryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: registryPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'web'
          image: containerImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'API_FQDN'
              value: apiFqdn
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

@description('The FQDN of the Container App')
output fqdn string = containerApp.properties.configuration.ingress.fqdn

@description('The name of the Container App')
output name string = containerApp.name
