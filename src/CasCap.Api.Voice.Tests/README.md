# CasCap.Api.Voice.Tests

Credential-free tests for the voice adapters, media policies, registration and metrics, plus four
integration-designated cases that check configuration and then explicitly skip live execution.
The project targets .NET 10.

## Running the Tests

From the repository root, with the .NET 10 SDK:

```powershell
dotnet test --project src/CasCap.Api.Voice.Tests/CasCap.Api.Voice.Tests.csproj --framework net10.0 --filter-not-trait Category=Integration
```

The unit subset needs no service, ffmpeg or credentials. Transcription cases use generated normalized
WAV data and fake provider clients rather than a speech backend.

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
| `AzureOpenAiTextToSpeechClientTests` | 1 | 1 |
| `AzureSpeechTextToSpeechClientTests` | 1 | 1 |
| `AzureSpeechToTextClientTests` | 1 | 1 |
| `PiperTextToSpeechClientTests` | 1 | 1 |
| **Total** | **31** | **51** |

Counts are source-declared before skips, not observed execution or provider-coverage results.

## Trait Categories

`SpeechToText`, `TextToSpeech`, and `Integration` are used. Integration cases also carry their
provider-area trait.

## Skipped Tests

| Reason | Cases |
| --- | ---: |
| Azure OpenAI endpoint absent; otherwise live Azure OpenAI execution is explicitly disabled | 1 |
| Azure Speech synthesis endpoint absent; otherwise live Azure Speech execution is explicitly disabled | 1 |
| Azure Speech transcription endpoint absent; otherwise live Azure Speech execution is explicitly disabled | 1 |
| Piper endpoint absent; otherwise live Piper execution is explicitly disabled | 1 |
| **Total** | **4** |

All four integration-designated cases read environment variables only and skip on either branch.
Providing credentials does not turn them into live tests. No unit cases declare skips.

## Layout

```text
Tests/
├── Integration/
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
