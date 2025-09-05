using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


var builder = DistributedApplication.CreateBuilder(args);

var azureRegion = builder.AddParameter("region");
var azureOrg = builder.AddParameter("organization");
var azureWorkload = builder.AddParameter("workload");
var azureProject = builder.AddParameter("project");
var azureInstance = builder.AddParameter("instance");
var azureEnv = builder.AddParameter("environment");

var context = new Context
{
    Region = string.IsNullOrWhiteSpace(azureRegion.Resource.ValueExpression) ? "frc1" : azureRegion.Resource.ValueExpression,
    Organization = string.IsNullOrWhiteSpace(azureOrg.Resource.ValueExpression) ? "sh" : azureOrg.Resource.ValueExpression,
    Workload = string.IsNullOrWhiteSpace(azureWorkload.Resource.ValueExpression) ? "asd" : azureWorkload.Resource.ValueExpression,
    Project = string.IsNullOrWhiteSpace(azureProject.Resource.ValueExpression) ? "shrd" : azureProject.Resource.ValueExpression,
    Instance = string.IsNullOrWhiteSpace(azureInstance.Resource.ValueExpression) ? "001" : azureInstance.Resource.ValueExpression,
    Environment = string.IsNullOrWhiteSpace(azureEnv.Resource.ValueExpression) ? "dev" : azureEnv.Resource.ValueExpression,
};

builder.Services.Configure<AzureProvisioningOptions>(options =>
{
    options.ProvisioningBuildOptions.InfrastructureResolvers.Insert(0, new NameInfrastructureResolver(context));
});

var apiService = builder.AddProject<Projects.aspire_demo_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

#pragma warning disable ASPIRECOSMOSDB001 // This suppresses the warning about using the PREVIEW CosmosDB emulator. The preview emulator is faster and more efficient for development purposes, but it is not recommended for production use.
var cosmosDbAccount = builder.AddAzureCosmosDB("cosmosdb-account")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithLifetime(ContainerLifetime.Persistent); // Use persistent lifetime for the emulator, this allows the emulator to retain data across application restarts.
        emulator.WithDataExplorer(); // Enable Data Explorer for the emulator, this allows you to view and manage the data in the emulator.
    });

var cosmosDbDatabase = cosmosDbAccount.AddCosmosDatabase("cosmosDbDatabase", "sampleDatabase");
cosmosDbDatabase.AddContainer("items", "/id");

builder.AddProject<Projects.aspire_demo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
