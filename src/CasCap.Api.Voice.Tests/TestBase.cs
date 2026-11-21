using Azure.Core;
using Azure.Identity;

namespace CasCap.Tests;

/// <summary>Provides environment-only configuration and xUnit logging for optional provider tests.</summary>
public abstract class TestBase(ITestOutputHelper output)
{
    protected ITestOutputHelper Output { get; } = output;

    protected IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    /// <summary>Gets a Microsoft Entra access token from the active Azure CLI session.</summary>
    protected static async ValueTask<AccessToken> GetAzureCliTokenAsync(string scope, CancellationToken cancellationToken)
    {
        var credential = new AzureCliCredential();
        try
        {
            return await credential.GetTokenAsync(new TokenRequestContext([scope]), cancellationToken);
        }
        catch (AuthenticationFailedException) when (!cancellationToken.IsCancellationRequested)
        {
            throw Xunit.Sdk.FailException.ForFailure(
                "Could not get a Microsoft Entra token from Azure CLI. Run az login in the Azure resource's tenant.");
        }
    }
}
