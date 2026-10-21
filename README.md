# CasCap.Api.Voice

Speech-to-text and text-to-speech libraries for .NET 10 applications, with interchangeable Whisper,
Azure AI Speech, Azure OpenAI and Piper adapters. Validate and normalize audio, transcribe it or
synthesize spoken replies without coupling the application to a messaging transport.

## Packages

| Package | Purpose | NuGet |
| --- | --- | --- |
| [CasCap.Api.Voice](src/CasCap.Api.Voice/README.md) | Provider adapters, bounded transcription and spoken-reply policy | [![NuGet version](https://img.shields.io/nuget/v/CasCap.Api.Voice)](https://www.nuget.org/packages/CasCap.Api.Voice) |
| [CasCap.Api.Voice.Testing](src/CasCap.Api.Voice.Testing/README.md) | Canned provider clients for consumer tests | [![NuGet version](https://img.shields.io/nuget/v/CasCap.Api.Voice.Testing)](https://www.nuget.org/packages/CasCap.Api.Voice.Testing) |

## Quick Start

In a .NET 10 Generic Host application:

The application-facing contracts are
[`IVoiceTranscriptionService`](src/CasCap.Api.Voice/Abstractions/IVoiceTranscriptionService.cs) and
[`IVoiceSynthesisService`](src/CasCap.Api.Voice/Abstractions/IVoiceSynthesisService.cs).

```powershell
dotnet package add CasCap.Api.Voice
```

The default transcription adapter expects an openai-whisper-asr-webservice backend at
`http://localhost:9000`. With that backend available and a local 16 kHz mono signed 16-bit PCM WAV file:

```csharp
using CasCap.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSpeechToText();

using var host = builder.Build();
var voice = host.Services.GetRequiredService<IVoiceTranscriptionService>();
var result = await voice.Transcribe(
    await File.ReadAllBytesAsync("input.wav"), "audio/wav");

Console.WriteLine(result.Outcome);
```

Read `result.Text` only when `result.TranscriptAvailable` is true. Audio is sent to the configured
backend; keep recordings and transcripts out of source control and routine logs.

## Configuration

Options bind from `CasCap:SpeechToTextConfig`, `CasCap:TextToSpeechConfig` and
`CasCap:VoiceMetricsConfig`. Register synthesis separately with `AddTextToSpeech()`;
spoken replies are disabled by default.

See the [core package README](src/CasCap.Api.Voice/README.md#configuration-examples) for provider
settings, processing limits, synthesis usage and the distinction between host-controlled voice modes
and direct transcription calls. Supply real endpoints and credentials through secret-backed
configuration, not tracked examples.

## Runtime Requirements

The library does not require Docker and does not publish a container image. A consuming application
is responsible for installing native tools in its runtime image and providing network access to the
selected speech backend.

| Capability | Runtime dependency | Required when |
| --- | --- | --- |
| Audio validation and already-normalized WAV transcription | None | Input is 16 kHz, mono, signed 16-bit PCM WAV |
| Other audio containers or WAV formats | `ffmpeg` executable | `VoiceMessageTranscriptionService` must normalize the input |
| Piper synthesis | Piper Wyoming server and `ffmpeg` with the `libopus` encoder | `TextToSpeechProvider.Piper` is selected |
| Whisper ASR | Reachable openai-whisper-asr-webservice endpoint | `SpeechToTextProvider.WhisperAsr` is selected |
| whisper.cpp | Reachable `whisper-server` endpoint | `SpeechToTextProvider.WhisperCpp` is selected |
| Azure AI Speech | Azure endpoint and token credential | An Azure Speech provider is selected |
| Azure OpenAI speech | Azure OpenAI endpoint, deployment, API version, and token credential | `TextToSpeechProvider.AzureOpenAi` is selected |

For Debian or Ubuntu application images, install ffmpeg in the final runtime stage:

```dockerfile
RUN apt-get update \
&& apt-get install -y --no-install-recommends ffmpeg \
&& ffmpeg -version \
&& ffmpeg -hide_banner -encoders 2>/dev/null | grep -q libopus \
&& rm -rf /var/lib/apt/lists/*
```

Piper returns raw PCM and the adapter produces Ogg Opus. The encoder check catches ffmpeg builds that
omit `libopus`, even when the executable is present.

Set `SpeechToTextConfig.FfmpegPath` and `TextToSpeechConfig.FfmpegPath` when the executable is not
available as `ffmpeg` on `PATH`. The library pipes media through standard input and output and does
not require writable temporary-file storage.

## Development

Use the .NET 10 SDK. The Release solution consumes published cross-repository dependencies and can be
built from this checkout alone:

```powershell
dotnet build CasCap.Api.Voice.Release.slnx --configuration Release
```

The Debug solution substitutes adjacent source checkouts for the shared CasCap libraries:

```powershell
dotnet build CasCap.Api.Voice.Debug.slnx --configuration Debug
```

Run the credential-free unit tests:

```powershell
dotnet test --project src/CasCap.Api.Voice.Tests/CasCap.Api.Voice.Tests.csproj --framework net10.0 --filter-not-trait Category=Integration
```

The [test README](src/CasCap.Api.Voice.Tests/README.md) lists source-declared cases, prerequisites and
skip behavior. The four integration-designated cases are explicitly disabled even when endpoints
are configured; they do not currently exercise live providers.

## License

This project is released under the [MIT License](LICENSE).
