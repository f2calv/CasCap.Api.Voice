namespace CasCap.Tests.Unit;

#pragma warning disable MEAI001

/// <summary>Verifies the optional spoken-reply policy.</summary>
[Trait("Category", "TextToSpeech")]
public sealed class VoiceReplySynthesisServiceTests
{
    private const string Reply = "The hallway door is closed.";

    [Theory]
    [InlineData(VoiceReplyMode.Disabled, false, false)]
    [InlineData(VoiceReplyMode.MatchInbound, false, false)]
    [InlineData(VoiceReplyMode.MatchInbound, true, true)]
    [InlineData(VoiceReplyMode.Always, false, true)]
    public async Task TrySynthesizeAsync_AppliesMode(VoiceReplyMode mode, bool inboundWasVoice,
        bool expectedAttachment)
    {
        var client = new FakeTextToSpeechClient();
        var service = CreateService(client, mode);
        var result = await service.TrySynthesizeAsync(Reply, inboundWasVoice,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedAttachment, result is not null);
        Assert.Equal(expectedAttachment ? 1 : 0, client.Requests);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TrySynthesizeAsync_EmptyReplyIsIgnored(string? text)
    {
        var client = new FakeTextToSpeechClient();
        var result = await CreateService(client, VoiceReplyMode.Always).TrySynthesizeAsync(text, true,
            TestContext.Current.CancellationToken);

        Assert.Null(result);
        Assert.Equal(0, client.Requests);
    }

    [Fact]
    public async Task TrySynthesizeAsync_BackendFailureIsContained()
    {
        var client = new FakeTextToSpeechClient { Failure = new HttpRequestException("backend down") };
        var result = await CreateService(client, VoiceReplyMode.Always).TrySynthesizeAsync(Reply, true,
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    private static VoiceReplySynthesisService CreateService(FakeTextToSpeechClient client, VoiceReplyMode mode) =>
        new(NullLogger<VoiceReplySynthesisService>.Instance, client,
            Options.Create(new TextToSpeechConfig { Mode = mode }));
}

