using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Microsoft.Extensions.DependencyInjection;


var builder = DistributedApplication.CreateBuilder(args);

var azureRegion = Environment.GetEnvironmentVariable("AZURE_REGION");
var azureOrg = Environment.GetEnvironmentVariable("AZURE_ORG");
var azureWorkload = Environment.GetEnvironmentVariable("AZURE_WORKLOAD");
var azureProject = Environment.GetEnvironmentVariable("AZURE_PROJECT");
var azureInstance = Environment.GetEnvironmentVariable("AZURE_INSTANCE");
var azureEnv = Environment.GetEnvironmentVariable("AZURE_ENV");

var context = new Context
{
    Region = string.IsNullOrWhiteSpace(azureRegion) ? "frc1" : azureRegion,
    Organization = string.IsNullOrWhiteSpace(azureOrg) ? "sh" : azureOrg,
    Workload = string.IsNullOrWhiteSpace(azureWorkload) ? "asd" : azureWorkload,
    Project = string.IsNullOrWhiteSpace(azureProject) ? "shrd" : azureProject,
    Instance = string.IsNullOrWhiteSpace(azureInstance) ? "001" : azureInstance,
    Environment = string.IsNullOrWhiteSpace(azureEnv) ? "dev" : azureEnv,
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
