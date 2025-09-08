using Azure.Provisioning;
using Azure.Provisioning.Expressions;

public struct ServiceContext
{
    public IResourceBuilder<ParameterResource> Region { get; set; }
    public IResourceBuilder<ParameterResource> Organization { get; set; }
    public IResourceBuilder<ParameterResource> Workload { get; set; }
    public IResourceBuilder<ParameterResource> Project { get; set; }
    public IResourceBuilder<ParameterResource> Environment { get; set; }
    public IResourceBuilder<ParameterResource> Instance { get; set; }

    public BicepValue<string> NamingConventionPrefix =>
        BicepFunction.Interpolate($"{new IdentifierExpression("serviceOrganization")}-{new IdentifierExpression("serviceRegion")}-{new IdentifierExpression("serviceEnvironment")}-{new IdentifierExpression("serviceWorkload")}");
    public BicepValue<string> NamingConventionSuffix =>
        BicepFunction.Interpolate($"{new IdentifierExpression("serviceProject")}-{new IdentifierExpression("serviceInstance")}");
}