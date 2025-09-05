targetScope = 'subscription'

param resourceGroupName string

param location string

param principalId string

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
}

module cosmosdb_account 'cosmosdb-account/cosmosdb-account.bicep' = {
  name: 'cosmosdb-account'
  scope: rg
  params: {
    location: location
  }
}

module cosmosdb_account_roles 'cosmosdb-account-roles/cosmosdb-account-roles.bicep' = {
  name: 'cosmosdb-account-roles'
  scope: rg
  params: {
    location: location
    cosmosdb_account_outputs_name: cosmosdb_account.outputs.name
    principalId: ''
  }
}