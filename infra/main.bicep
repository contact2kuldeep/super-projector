// Super Projector: Azure infrastructure for one environment.
//
// Deployed into an existing resource group, e.g.:
//   az deployment group create --resource-group rg-superprojector-dev \
//     --parameters infra/main.bicepparam
//
// All Azure changes go through this file; never make manual changes in the portal.

targetScope = 'resourceGroup'

// ---- Parameters -------------------------------------------------------------

@description('Azure region for the API and monitoring resources. Defaults to the resource group\'s region.')
param location string = resourceGroup().location

@description('Short environment name, used in tags (e.g. dev, test, prod).')
@maxLength(10)
param environmentName string

@description('App Service Plan SKU. F1 is free; switch to B1 if F1 is unavailable in your subscription or region.')
@allowed([
  'F1'
  'B1'
])
param appServiceSkuName string = 'F1'

@description('Region for the Static Web App. Only a few regions support it; content is still served globally.')
@allowed([
  'centralus'
  'eastus2'
  'eastasia'
  'westeurope'
  'westus2'
])
param staticWebAppLocation string = 'eastasia'

// ---- Variables --------------------------------------------------------------

// uniqueString() returns the same value every time for the same resource group,
// so redeploying updates the same resources instead of creating new ones.
// Some names (e.g. the web app) must be unique across all of Azure.
var suffix = take(uniqueString(resourceGroup().id), 6)

var tags = {
  project: 'super-projector'
  env: environmentName
  managedBy: 'bicep'
}

// ---- Resources --------------------------------------------------------------

// Log Analytics workspace: the central store for logs and telemetry.
// Application Insights writes its data here.
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-superprojector-${suffix}'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018' // pay per GB ingested; a small app stays in the free allowance
    }
    retentionInDays: 30
  }
}

// Application Insights: monitors the API (requests, failures, response times).
// "Workspace-based" means its data lives in the Log Analytics workspace above.
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-superprojector-${suffix}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// Static Web App: hosts the built React app (HTML, JS, CSS) on a global CDN.
// The Free tier is enough for a learning project.
resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: 'stapp-superprojector-web-${suffix}'
  location: staticWebAppLocation
  tags: tags
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// App Service Plan: the virtual machine (compute) the API runs on.
// 'reserved: true' is how ARM says "Linux".
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-superprojector-${suffix}'
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: appServiceSkuName
  }
  properties: {
    reserved: true
  }
}

// Web App: runs the .NET API on the plan above.
resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-superprojector-api-${suffix}'
  location: location
  tags: tags
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true // redirect any http:// request to https://
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0' // from `az webapp list-runtimes --os linux`
      alwaysOn: false // not supported on F1; the app sleeps when idle, so the first request is slower
      minTlsVersion: '1.2'
      ftpsState: 'Disabled' // we deploy from GitHub Actions, never by FTP
      healthCheckPath: '/health' // App Service pings this to check the app is up
      appSettings: [
        {
          // Lets the API send telemetry to Application Insights.
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production' // also turns off the /scalar and /openapi endpoints
        }
        {
          // Double underscore maps to ':' in .NET config, i.e. Cors:AllowedOrigins:0.
          // Allows the browser to call the API from the Static Web App.
          name: 'Cors__AllowedOrigins__0'
          value: 'https://${staticWebApp.properties.defaultHostname}'
        }
      ]
    }
  }
}

// ---- Outputs ----------------------------------------------------------------
// Printed after deployment, and used later by the GitHub Actions workflows.

output apiAppName string = apiApp.name
output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output staticWebAppName string = staticWebApp.name
output webUrl string = 'https://${staticWebApp.properties.defaultHostname}'
output appInsightsName string = appInsights.name
