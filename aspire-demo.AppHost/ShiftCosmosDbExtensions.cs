using Aspire.Hosting;
using Aspire.Hosting.Azure;
using Azure.Provisioning.CosmosDB;
using Azure.Provisioning.Expressions;
using Microsoft.AspNetCore.Mvc.Filters;

public static class ShiftCosmosDbExtensions
{
    public static IResourceBuilder<AzureCosmosDBResource> AddShiftAzureCosmosDB(this IDistributedApplicationBuilder builder, string name, Context context)
    {
        // You can customize the logic here as needed
        var cosmosDbAccount = builder.AddAzureCosmosDB(name)
        .ConfigureInfrastructure(infra =>
        {
            var cosmosAccount = infra.GetProvisionableResources().OfType<CosmosDBAccount>().Single();
            cosmosAccount.Tags.Add("Workload", context.Workload);

            cosmosAccount.Name = BicepFunction.Interpolate($"{context.NamingConventionPrefix}-cosno-{context.NamingConventionSuffix}");

            cosmosAccount.ConsistencyPolicy = new()
            {
                DefaultConsistencyLevel = DefaultConsistencyLevel.Strong,
            };

            cosmosAccount.IsVirtualNetworkFilterEnabled = true;

            cosmosAccount.IPRules = [];

            cosmosAccount.VirtualNetworkRules = []; 

        });
        
        return cosmosDbAccount;
    }
}