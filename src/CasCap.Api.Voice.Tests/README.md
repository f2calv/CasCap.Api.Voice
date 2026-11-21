# CasCap.Api.Voice.Tests

Credential-free tests for the voice adapters, media policies, registration and metrics, plus an
opt-in Azure OpenAI Realtime speech-speed probe. The project targets .NET 10 and .NET 11.

## Running the Tests

From the repository root, with a stable .NET 11 SDK:

```powershell
dotnet test --project src/CasCap.Api.Voice.Tests/CasCap.Api.Voice.Tests.csproj --configuration Release --framework net11.0 --filter-not-trait Category=Integration
```

The unit subset needs no service, ffmpeg or credentials. Transcription cases use generated normalized
WAV data and fake provider clients rather than a speech backend.

## Azure Realtime Speech-Speed Probe

Use an approved Azure OpenAI Realtime test deployment. Supply `AzureRealtimeProbe__Endpoint`
(the HTTPS resource root), `AzureRealtimeProbe__Deployment`, and `AzureRealtimeProbe__ApiKey`
through the local environment or CI secret store. Set `AzureRealtimeProbe__Enabled` to `true`
only for the targeted run:

```powershell
dotnet test --project src/CasCap.Api.Voice.Tests/CasCap.Api.Voice.Tests.csproj --framework net10.0 --filter-class CasCap.Tests.AzureOpenAiRealtimeSpeedTests
```

The test requests two synthetic audio replies in a single Azure WebSocket session, at 0.8x and
1.2x. It checks Azure's acknowledgement, verifies that both replies say the expected phrase, and
compares their PCM lengths. It does not measure perceived pace or validate a WebRTC client. The test
uses no microphone, agent tools, or learner data, and does not save audio or transcripts. Live
synthesis may incur charges.

## Tests

| Class | Methods | Expanded Cases |
| --- | ---: | ---: |
| `PiperEndpointTests` | 2 | 5 |
| `SpeechTextNormalizerTests` | 3 | 8 |
| `VoiceMessageTranscriptionServiceTests` | 6 | 8 |
| `VoiceReplySynthesisServiceTests` | 4 | 9 |
| `VoiceServiceCollectionExtensionsTests` | 5 | 5 |
| `VoiceTranscriptionMetricsTests` | 3 | 3 |
| `WhisperAsrSpeechToTextClientTests` | 4 | 9 |
| Unit subtotal | **27** | **47** |
| `AzureOpenAiRealtimeSpeedTests` | 1 | 1 |
| `AzureOpenAiTextToSpeechClientTests` | 1 | 1 |
| `AzureSpeechTextToSpeechClientTests` | 1 | 1 |
| `AzureSpeechToTextClientTests` | 1 | 1 |
| `PiperTextToSpeechClientTests` | 1 | 1 |
| **Total** | **32** | **52** |

Counts are source-declared before skips, not observed execution or provider-coverage results.

## Trait Categories

`SpeechToText`, `TextToSpeech`, and `Integration` are used.

## Skipped Tests

| Reason | Cases |
| --- | ---: |
| Azure Realtime probe not enabled or connection settings incomplete | 1 |
| Azure OpenAI endpoint absent; otherwise live Azure OpenAI execution is explicitly disabled | 1 |
| Azure Speech synthesis endpoint absent; otherwise live Azure Speech execution is explicitly disabled | 1 |
| Azure Speech transcription endpoint absent; otherwise live Azure Speech execution is explicitly disabled | 1 |
| Piper endpoint absent; otherwise live Piper execution is explicitly disabled | 1 |
| **Total without probe configuration** | **5** |

The other four integration-designated cases always skip, even when configured. No unit cases
declare skips.

## Layout

```text
Tests/
├── Integration/
│   ├── AzureOpenAiRealtimeSpeedTests.cs
│   ├── AzureOpenAiTextToSpeechClientTests.cs
│   ├── AzureSpeechTextToSpeechClientTests.cs
│   ├── AzureSpeechToTextClientTests.cs
│   └── PiperTextToSpeechClientTests.cs
└── Unit/
    ├── PiperEndpointTests.cs
    ├── SpeechTextNormalizerTests.cs
    ├── TestMetrics.cs
    ├── VoiceMessageTranscriptionServiceTests.cs
    ├── VoiceReplySynthesisServiceTests.cs
    ├── VoiceServiceCollectionExtensionsTests.cs
    ├── VoiceTranscriptionMetricsTests.cs
    └── WhisperAsrSpeechToTextClientTests.cs
```

The speech-to-text and text-to-speech fakes come from the
[`CasCap.Api.Voice.Testing`](../CasCap.Api.Voice.Testing/README.md) project, which is also
published for downstream consumers.
