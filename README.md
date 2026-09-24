# CasCap.Api.Voice

Provider-neutral speech-to-text and text-to-speech components for .NET applications.

## Status

The library is published as the `CasCap.Api.Voice` NuGet package. Its provider-neutral contract is
shared by applications that need speech processing without taking a messaging dependency.

## Installation

```powershell
dotnet package add CasCap.Api.Voice
```

## Public Surface

The library provides:

- speech-to-text adapters for openai-whisper-asr-webservice, whisper.cpp, and Azure AI Speech;
- text-to-speech adapters for Azure AI Speech, Azure OpenAI, and Piper over Wyoming;
- bounded media validation and normalization to 16 kHz mono PCM WAV;
- transport-neutral transcription and synthesis results;
- stable application-facing transcription and synthesis interfaces;
- OpenTelemetry-compatible voice metrics; and
- dependency-injection registration through `AddSpeechToText` and `AddTextToSpeech`.

## Configuration

Configuration binds from `CasCap:SpeechToTextConfig`, `CasCap:TextToSpeechConfig`, and
`CasCap:VoiceMetricsConfig`. Provider endpoints and credentials belong in environment variables,
.NET User Secrets, or another secret-backed provider rather than tracked configuration.

## Dependencies

Development builds use adjacent source checkouts for shared CasCap libraries. Release builds and the
published package use their exact NuGet package equivalents.

### Runtime and Container Dependencies

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

For Debian or Ubuntu-based application images, install ffmpeg in the final runtime stage and remove
the package-manager cache in the same layer:

```dockerfile
RUN apt-get update \
&& apt-get install -y --no-install-recommends ffmpeg \
&& ffmpeg -version \
&& ffmpeg -hide_banner -encoders 2>/dev/null | grep -q libopus \
&& rm -rf /var/lib/apt/lists/*
```

The `ffmpeg -version` check catches a missing executable during the image build. The encoder check is
required for Piper because Piper returns raw PCM and the adapter produces Ogg Opus. A minimal or
custom ffmpeg build may omit `libopus` even when the executable itself is present.

Set `SpeechToTextConfig.FfmpegPath` and `TextToSpeechConfig.FfmpegPath` when the executable is not
available as `ffmpeg` on `PATH`. The library pipes media through standard input and output and does
not require writable temporary-file storage.

## Development

Open the repository in its Dev Container for .NET 10, PowerShell, pre-commit, and an ffmpeg build
that includes the `libopus` encoder. Container creation restores the standalone Release solution but
does not install Git hooks automatically.

Build the Debug solution:

```powershell
dotnet build CasCap.Api.Voice.Debug.slnx --configuration Debug
```

Run the credential-free unit tests:

```powershell
dotnet test --project src/CasCap.Api.Voice.Tests/CasCap.Api.Voice.Tests.csproj --framework net10.0 -- --filter-not-trait Category=Integration
```

Integration tests requiring external speech services are explicitly categorized and excluded from
that command.

Run all repository lint hooks manually:

```bash
pre-commit run --all-files
```

## License

This project is released under the [Unlicense](LICENSE).
