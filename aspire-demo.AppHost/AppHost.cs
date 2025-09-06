using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Provisioning.CosmosDB;
using Azure.Provisioning.Expressions;


var builder = DistributedApplication.CreateBuilder(args);

var azureRegion = builder.AddParameter("Region");
var azureOrg = builder.AddParameter("Organization");
var azureWorkload = builder.AddParameter("Workload");
var azureProject = builder.AddParameter("Project");
var azureInstance = builder.AddParameter("Instance");
var azureEnv = builder.AddParameter("Environment");
var source = new CancellationTokenSource();
var token = source.Token;
var prj = builder.Configuration.GetValue<string>("Parameters:Project") ?? "ffff";
var context = new Context
{
    Region = await azureRegion.Resource.GetValueAsync(token),
    Organization = await azureOrg.Resource.GetValueAsync(token),
    Workload = await azureWorkload.Resource.GetValueAsync(token),
    Project = prj,
    Instance = await azureInstance.Resource.GetValueAsync(token),
    Environment = await azureEnv.Resource.GetValueAsync(token),
};

// builder.Services.Configure<AzureProvisioningOptions>(options =>
// {
//     options.ProvisioningBuildOptions.InfrastructureResolvers.Insert(0, new NameInfrastructureResolver(context));
// });

var apiService = builder.AddProject<Projects.aspire_demo_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

#pragma warning disable ASPIRECOSMOSDB001 // This suppresses the warning about using the PREVIEW CosmosDB emulator. The preview emulator is faster and more efficient for development purposes, but it is not recommended for production use.
var cosmosDbAccount = builder.AddAzureCosmosDB("cosmosdb-account")
    .ConfigureInfrastructure(infra =>
    {
        var cosmosAccount = infra.GetProvisionableResources().OfType<CosmosDBAccount>().Single();
        cosmosAccount.Name = BicepFunction.Interpolate($"{context.NamingConventionPrefix}-cosno-{context.NamingConventionSuffix}");
    })
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
