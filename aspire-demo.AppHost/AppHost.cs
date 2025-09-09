using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var serviceEnvironment = builder.AddParameter("serviceEnvironment");
var serviceProject = builder.AddParameter("serviceProject");
var serviceInstance = builder.AddParameter("serviceInstance");
var serviceRegion = builder.AddParameter("serviceRegion");
var serviceWorkload = builder.AddParameter("serviceWorkload");
var serviceOrganization = builder.AddParameter("serviceOrganization");

var context = new ServiceContext
{
    Region = serviceRegion,
    Organization = serviceOrganization,
    Workload = serviceWorkload,
    Project = serviceProject,
    Instance = serviceInstance,
    Environment = serviceEnvironment,
};

//builder.Services.Configure<AzureProvisioningOptions>(options =>
//{
//    options.ProvisioningBuildOptions.InfrastructureResolvers.Insert(0, new NameInfrastructureResolver(context));
//});

var apiService = builder.AddProject<Projects.aspire_demo_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

#pragma warning disable ASPIRECOSMOSDB001 // This suppresses the warning about using the PREVIEW CosmosDB emulator. The preview emulator is faster and more efficient for development purposes, but it is not recommended for production use.
var cosmosDbAccount = builder.AddShiftAzureCosmosDB("cosmosdb-account", context)
    //.WithParameter("serviceEnvironment", serviceEnv)
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
    .WithReference(cosmosDbAccount, "cosmosdb-account")
    .WaitFor(apiService);

builder.Build().Run();
