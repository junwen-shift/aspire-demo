using Azure.Provisioning;
using Azure.Provisioning.Primitives;
using Azure.Provisioning.CosmosDB;
using Azure.Provisioning.Expressions;

internal sealed class NameInfrastructureResolver(Context context) : InfrastructureResolver
{
    private readonly Context _context = context;

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
        return BicepFunction.Interpolate($"{_context.Organization}-{_context.Region}-{_context.Environment}-{_context.Workload}-{resourceTypeAbbreviation}-{_context.Project}-{_context.Instance}");
    }
}

public struct Context
{
    public string Region { get; set; }
    public string Organization { get; set; }
    public string Workload { get; set; }
    public string Project { get; set; }
    public string Environment { get; set; }
    public string Instance { get; set; }

    public string NamingConventionPrefix =>
        $"{Organization}-{Region}-{Environment}-{Workload}";
    public string NamingConventionSuffix =>
        $"{Project}-{Instance}";
}