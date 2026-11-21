using System.Net.WebSockets;
using System.Text.Json;

namespace CasCap.Tests;

/// <summary>Verifies an Azure OpenAI Realtime deployment accepts speed updates between audio replies.</summary>
[Trait("Category", "Integration")]
public sealed class AzureOpenAiRealtimeSpeedTests(ITestOutputHelper output) : TestBase(output)
{
    private const string Phrase = "Heute trinken wir Tee.";
    private static readonly string[] AudioModalities = ["audio"];

    [Fact]
    public async Task SessionUpdate_ChangesSpeedBetweenAudioReplies()
    {
        var settings = Configuration.GetSection("AzureRealtimeProbe");
        Assert.SkipUnless(settings.GetValue<bool>("Enabled"), "The Azure Realtime probe is not enabled.");
        var endpoint = settings["Endpoint"];
        var deployment = settings["Deployment"];
        var apiKey = settings["ApiKey"];
        Assert.SkipWhen(string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(deployment) ||
            string.IsNullOrWhiteSpace(apiKey), "The Azure Realtime probe connection settings are incomplete.");
        Assert.True(Uri.TryCreate(endpoint, UriKind.Absolute, out var resource) &&
            resource.Scheme == Uri.UriSchemeHttps && resource.PathAndQuery == "/" &&
            resource.UserInfo.Length == 0 && resource.Fragment.Length == 0,
            "Configure the Azure Realtime probe with the HTTPS resource root.");

        var address = new UriBuilder(resource!)
        {
            Scheme = "wss",
            Path = "/openai/v1/realtime",
            Query = $"model={Uri.EscapeDataString(deployment!)}",
        };
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(60));
        using var socket = new ClientWebSocket();
        socket.Options.SetRequestHeader("api-key", apiKey);
        try
        {
            await socket.ConnectAsync(address.Uri, deadline.Token);
        }
        catch (WebSocketException)
        {
            Assert.Fail("Azure rejected the Realtime WebSocket connection or could not be reached.");
        }

        await WaitForEventAsync(socket, "session.created", deadline.Token);
        var slowBytes = await MeasureReplyAsync(socket, 0.8m, deadline.Token);
        var fastBytes = await MeasureReplyAsync(socket, 1.2m, deadline.Token);
        Assert.True(fastBytes < slowBytes,
            $"Expected the same phrase to take less audio at 1.2x than at 0.8x ({fastBytes} vs {slowBytes} bytes).");
    }

    private static async Task<long> MeasureReplyAsync(ClientWebSocket socket, decimal speed, CancellationToken ct)
    {
        await SendAsync(socket, new
        {
            type = "session.update",
            session = new
            {
                type = "realtime",
                output_modalities = AudioModalities,
                audio = new { output = new { format = new { type = "audio/pcm", rate = 24000 }, speed } },
            },
        }, ct);
        var updated = await WaitForEventAsync(socket, "session.updated", ct);
        var output = updated.GetProperty("session").GetProperty("audio").GetProperty("output");
        Assert.True(output.TryGetProperty("speed", out var applied) &&
            applied.TryGetDecimal(out var actual) && actual == speed,
            $"Azure did not acknowledge the requested audio output speed ({speed}).");

        await SendAsync(socket, new
        {
            type = "response.create",
            response = new
            {
                conversation = "none",
                input = Array.Empty<object>(),
                instructions = $"Say only this German sentence: {Phrase}",
            },
        }, ct);
        var audioBytes = 0L;
        var transcriptMatched = false;
        while (true)
        {
            var message = await ReadEventAsync(socket, ct);
            switch (message.GetProperty("type").GetString())
            {
                case "response.output_audio.delta":
                    audioBytes += message.GetProperty("delta").GetBytesFromBase64().Length;
                    break;
                case "response.output_audio_transcript.done":
                    transcriptMatched = string.Equals(
                        LettersOnly(Phrase), LettersOnly(message.GetProperty("transcript").GetString() ?? ""),
                        StringComparison.OrdinalIgnoreCase);
                    break;
                case "response.done":
                    Assert.Equal("completed", message.GetProperty("response").GetProperty("status").GetString());
                    Assert.True(audioBytes > 0, "Azure did not return audio at the acknowledged speed.");
                    Assert.True(transcriptMatched, "The generated phrase differed; audio durations are not comparable.");
                    return audioBytes;
            }
        }
    }

    private static string LettersOnly(string value) => string.Concat(value.Where(char.IsLetter));

    private static Task SendAsync(ClientWebSocket socket, object message, CancellationToken ct)
        => socket.SendAsync(new ArraySegment<byte>(JsonSerializer.SerializeToUtf8Bytes(message)),
            WebSocketMessageType.Text, true, ct);

    private static async Task<JsonElement> WaitForEventAsync(ClientWebSocket socket, string type, CancellationToken ct)
    {
        while (true)
        {
            var message = await ReadEventAsync(socket, ct);
            if (message.GetProperty("type").GetString() == type)
                return message;
        }
    }

    private static async Task<JsonElement> ReadEventAsync(ClientWebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[8192];
        using var payload = new MemoryStream();
        ValueWebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(buffer.AsMemory(), ct);
            Assert.Equal(WebSocketMessageType.Text, result.MessageType);
            Assert.True(payload.Length + result.Count <= 1024 * 1024, "The Realtime event exceeded the probe limit.");
            payload.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        payload.Position = 0;
        using var document = await JsonDocument.ParseAsync(payload, cancellationToken: ct);
        var message = document.RootElement;
        Assert.False(message.GetProperty("type").GetString() == "error", "Azure returned a Realtime protocol error.");
        return message.Clone();
    }
}
