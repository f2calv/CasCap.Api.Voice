namespace CasCap.Tests;

/// <summary>Provides environment-only configuration and xUnit logging for optional provider tests.</summary>
public abstract class TestBase(ITestOutputHelper output)
{
    protected ITestOutputHelper Output { get; } = output;

    protected IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();
}
