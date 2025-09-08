using Azure.Provisioning;
using Azure.Provisioning.Primitives;
using Azure.Provisioning.CosmosDB;
using Azure.Provisioning.Expressions;

internal sealed class NameInfrastructureResolver(ServiceContext context) : InfrastructureResolver
{
    private readonly ServiceContext _context = context;

    public override void ResolveProperties(ProvisionableConstruct construct, ProvisioningBuildOptions options)
    {
        // Generate names based on resource type
        switch (construct)
        {
            case CosmosDBAccount account:
                account.Name = GenerateResourceName("cosno"); // cosmos-nosql
                break;
            // Add more resource types as needed
            // case StorageAccount storage:
            //     storage.Name = GenerateResourceName("st");
            //     break;
            // case KeyVault keyVault:
            //     keyVault.Name = GenerateResourceName("kv");
            //     break;
        }

        base.ResolveProperties(construct, options);
    }

    private BicepValue<string> GenerateResourceName(string resourceTypeAbbreviation)
    {
        // Create a proper ARM template expression for resource naming
        // This uses string interpolation with parameter references
        return BicepFunction.Interpolate(
            $"{_context.NamingConventionPrefix}-{resourceTypeAbbreviation}-{_context.NamingConventionSuffix}"
        );
    }
}
