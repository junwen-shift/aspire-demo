using Azure.Provisioning.CosmosDB;
using Azure.Provisioning.Expressions;

public static class ShiftCosmosDbExtensions
{
    public static IResourceBuilder<AzureCosmosDBResource> AddShiftAzureCosmosDB(this IDistributedApplicationBuilder builder, string name, ServiceContext context)
    {
        // You can customize the logic here as needed
        var cosmosDbAccount = builder.AddAzureCosmosDB(name)
            .WithParameter("serviceEnvironment", context.Environment)
            .WithParameter("serviceOrganization", context.Organization)
            .WithParameter("serviceRegion", context.Region)
            .WithParameter("serviceWorkload", context.Workload)
            .WithParameter("serviceProject", context.Project)
            .WithParameter("serviceInstance", context.Instance)
            .ConfigureInfrastructure(infra =>
            {
                var cosmosAccount = infra.GetProvisionableResources().OfType<CosmosDBAccount>().Single();
                var nameExpression = BicepFunction.Concat(context.NamingConventionPrefix, "-cosno-", context.NamingConventionSuffix);

                cosmosAccount.Tags.Add("Workload", new IdentifierExpression("serviceWorkload"));
                cosmosAccount.Tags.Add("Project", new IdentifierExpression("serviceProject"));
                cosmosAccount.Tags.Add("Environment", new IdentifierExpression("serviceEnvironment"));

                cosmosAccount.Kind = CosmosDBAccountKind.GlobalDocumentDB;

                cosmosAccount.Name = nameExpression;

                //cosmosAccount.Capabilities = [];


                cosmosAccount.IsVirtualNetworkFilterEnabled = true;

                cosmosAccount.IPRules = [];

                cosmosAccount.VirtualNetworkRules = []; 

            });
        
        return cosmosDbAccount;
    }
}