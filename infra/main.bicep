@description('Environment name used for resource naming')
param environmentName string = 'dev'

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Prefix for resource names')
param prefix string = 'amalgam'

@description('Container image for the API app (override during deployment)')
param apiContainerImage string = ''

@description('Container image for the Web app (override during deployment)')
param webContainerImage string = ''

var uniqueSuffix = uniqueString(resourceGroup().id)
var acrName = '${prefix}acr${uniqueSuffix}'
var envName = '${prefix}-env-${environmentName}-${uniqueSuffix}'
var logAnalyticsName = '${prefix}-logs-${environmentName}-${uniqueSuffix}'
var apiAppName = '${prefix}-api-${environmentName}'
var webAppName = '${prefix}-web-${environmentName}'

module acr 'modules/container-registry.bicep' = {
  name: 'container-registry'
  params: {
    name: acrName
    location: location
  }
}

module containerAppsEnv 'modules/container-apps-env.bicep' = {
  name: 'container-apps-env'
  params: {
    name: envName
    location: location
    logAnalyticsName: logAnalyticsName
  }
}

module apiApp 'modules/container-app-api.bicep' = {
  name: 'container-app-api'
  params: {
    name: apiAppName
    location: location
    environmentId: containerAppsEnv.outputs.id
    containerImage: apiContainerImage != '' ? apiContainerImage : '${acr.outputs.loginServer}/amalgam-api:latest'
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.username
    registryPassword: acr.outputs.password
  }
}

module webApp 'modules/container-app-web.bicep' = {
  name: 'container-app-web'
  params: {
    name: webAppName
    location: location
    environmentId: containerAppsEnv.outputs.id
    containerImage: webContainerImage != '' ? webContainerImage : '${acr.outputs.loginServer}/amalgam-web:latest'
    registryServer: acr.outputs.loginServer
    registryUsername: acr.outputs.username
    registryPassword: acr.outputs.password
    apiFqdn: apiApp.outputs.fqdn
  }
}

@description('The URL of the API app')
output apiUrl string = 'https://${apiApp.outputs.fqdn}'

@description('The URL of the Web app')
output webUrl string = 'https://${webApp.outputs.fqdn}'

@description('The ACR login server')
output acrLoginServer string = acr.outputs.loginServer
