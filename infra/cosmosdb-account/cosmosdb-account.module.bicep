@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource cosmosdb_account 'Microsoft.DocumentDB/databaseAccounts@2024-08-15' = {
  name: take('cosmosdbaccount-${uniqueString(resourceGroup().id)}', 44)
  location: location
  properties: {
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    databaseAccountOfferType: 'Standard'
    disableLocalAuth: true
  }
  kind: 'GlobalDocumentDB'
  tags: {
    'aspire-resource-name': 'cosmosdb-account'
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-08-15' = {
  name: 'sampleDatabase'
  location: location
  properties: {
    resource: {
      id: 'sampleDatabase'
    }
  }
  parent: cosmosdb_account
}

resource items 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-08-15' = {
  name: 'items'
  location: location
  properties: {
    resource: {
      id: 'items'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
    }
  }
  parent: cosmosDbDatabase
}

output connectionString string = cosmosdb_account.properties.documentEndpoint

output name string = cosmosdb_account.name