using System.Text.RegularExpressions;

namespace CasCap.Tests.Unit;

#pragma warning disable MEAI001

/// <summary>Verifies the whisper-asr multipart request and response contract.</summary>
[Trait("Category", "SpeechToText")]
public sealed class WhisperAsrSpeechToTextClientTests
{
    private const string Endpoint = "http://speech.example.com:9000";

    [Theory]
    [InlineData("audio/wav", "false", "audio.wav")]
    [InlineData("audio/aac", "true", "audio.aac")]
    [InlineData("audio/ogg", "true", "audio.ogg")]
    public async Task GetTextAsync_SendsExpectedMultipartRequest(string mediaType, string encode,
        string fileName)
    {
        using var handler = new RecordingHandler(new(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"text\":\"ok\"}", Encoding.UTF8, "application/json"),
        });
        using var client = CreateClient(handler);
        using var audio = new MemoryStream("payload"u8.ToArray());

        await client.GetTextAsync(audio, OptionsFor(mediaType), TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal($"{Endpoint}/asr", handler.RequestUri!.GetLeftPart(UriPartial.Path));
        Assert.Contains($"encode={encode}", handler.RequestUri.Query);
        Assert.Matches($"name=\"?{WhisperAsrSpeechToTextClient.AudioFilePartName}\"?", handler.Body);
        Assert.Matches($"filename=\"?{Regex.Escape(fileName)}\"?", handler.Body);
        Assert.Contains("payload", handler.Body);
    }

    [Theory]
    [InlineData("{\"text\":\"hello\"}", "hello")]
    [InlineData("{}", "")]
    public async Task GetTextAsync_ParsesResponse(string json, string expected)
    {
        using var handler = new RecordingHandler(new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
        using var client = CreateClient(handler);
        using var audio = new MemoryStream("payload"u8.ToArray());

        var result = await client.GetTextAsync(audio, OptionsFor("audio/wav"), TestContext.Current.CancellationToken);

        Assert.Equal(expected, result.Text);
    }

    [Fact]
    public async Task GetTextAsync_TrailingSlashDoesNotDoublePrefix()
    {
        using var handler = new RecordingHandler(new(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"text\":\"ok\"}", Encoding.UTF8, "application/json"),
        });
        using var client = new WhisperAsrSpeechToTextClient(NullLogger<WhisperAsrSpeechToTextClient>.Instance,
            Options.Create(new SpeechToTextConfig { WhisperAsrEndpoint = $"{Endpoint}/" }),
            new StubHttpClientFactory(handler));
        using var audio = new MemoryStream("payload"u8.ToArray());

        await client.GetTextAsync(audio, OptionsFor("audio/wav"), TestContext.Current.CancellationToken);

        Assert.Equal("/asr", handler.RequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.RequestTimeout, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    public void IsTransientStatusCode(HttpStatusCode statusCode, bool expected) =>
        Assert.Equal(expected, WhisperAsrSpeechToTextClient.IsTransientStatusCode(statusCode));

    private static WhisperAsrSpeechToTextClient CreateClient(HttpMessageHandler handler) =>
        new(NullLogger<WhisperAsrSpeechToTextClient>.Instance,
            Options.Create(new SpeechToTextConfig { WhisperAsrEndpoint = Endpoint }),
            new StubHttpClientFactory(handler));

    private static SpeechToTextOptions OptionsFor(string mediaType) => new()
    {
        AdditionalProperties = new AdditionalPropertiesDictionary
        {
            [WhisperAsrSpeechToTextClient.MediaTypePropertyKey] = mediaType,
        },
    };

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class RecordingHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string Body { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            Method = request.Method;
            Body = Encoding.Latin1.GetString(await request.Content!.ReadAsByteArrayAsync(cancellationToken));
            return response;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                response.Dispose();
            base.Dispose(disposing);
        }
    }
}
