@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param cosmosdb_account_outputs_name string

param principalId string

resource cosmosdb_account 'Microsoft.DocumentDB/databaseAccounts@2024-08-15' existing = {
  name: 'sh-frc1-stg-demo-cosno-shrd-001'
}

resource cosmosdb_account_roleDefinition 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2024-08-15' existing = {
  name: '00000000-0000-0000-0000-000000000002'
  parent: cosmosdb_account
}

resource cosmosdb_account_roleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-08-15' = {
  name: guid(principalId, cosmosdb_account_roleDefinition.id, cosmosdb_account.id)
  properties: {
    principalId: principalId
    roleDefinitionId: cosmosdb_account_roleDefinition.id
    scope: cosmosdb_account.id
  }
  parent: cosmosdb_account
}