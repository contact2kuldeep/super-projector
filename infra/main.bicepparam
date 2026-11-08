// Parameter values for the dev environment.
// Deploy into: rg-superprojector-dev (australiaeast).
using 'main.bicep'

param environmentName = 'dev'

// Resources go in australiasoutheast (Melbourne), not the resource group's
// region (australiaeast, Sydney). Our new subscription has 0 F1 App Service
// quota in australiaeast, so the deployment fails there. Melbourne still keeps
// data onshore in Australia. (A resource group's region only stores its
// metadata; its resources can live in any region.)
param location = 'australiasoutheast'
