using Azure.Provisioning;
using Azure.Provisioning.Primitives;
using Azure.Provisioning.CosmosDB;

internal sealed class NameInfrastructureResolver(Context context) : InfrastructureResolver
{
    private readonly Context _context = context;

    public override void ResolveProperties(ProvisionableConstruct construct, ProvisioningBuildOptions options)
    {
        if (construct is CosmosDBAccount account)
        {
            account.Name = $"{_context.NamingConventionPrefix}-cosno-{_context.NamingConventionSuffix}";
        }

        base.ResolveProperties(construct, options);
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