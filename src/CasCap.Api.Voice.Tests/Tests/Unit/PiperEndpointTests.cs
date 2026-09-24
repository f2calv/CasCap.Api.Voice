namespace CasCap.Tests.Unit;

/// <summary>Verifies Piper endpoint parsing.</summary>
[Trait("Category", "TextToSpeech")]
public sealed class PiperEndpointTests
{
    [Theory]
    [InlineData("piper.example", "piper.example", 10200)]
    [InlineData("piper.example:11000", "piper.example", 11000)]
    [InlineData("tcp://piper.example:10200", "piper.example", 10200)]
    [InlineData("https://piper.example:10200/", "piper.example", 10200)]
    public void ParseEndpoint_SplitsHostAndPort(string endpoint, string expectedHost, int expectedPort)
    {
        var (host, port) = PiperTextToSpeechClient.ParseEndpoint(endpoint);
        Assert.Equal(expectedHost, host);
        Assert.Equal(expectedPort, port);
    }

    [Fact]
    public void ParseEndpoint_NonNumericPortUsesDefault()
    {
        var (host, port) = PiperTextToSpeechClient.ParseEndpoint("piper.example:notaport");
        Assert.Equal("piper.example:notaport", host);
        Assert.Equal(PiperTextToSpeechClient.DefaultPort, port);
    }
}