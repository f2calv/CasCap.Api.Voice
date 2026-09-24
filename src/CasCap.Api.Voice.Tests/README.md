# CasCap.Api.Voice.Tests

Unit and integration coverage for the voice provider adapters and processing policies. Provider
integration cases are retained as credential-optional buildable coverage and are excluded from the
default local run.

## Tests

| Area | Methods | Cases |
| --- | ---: | ---: |
| Unit | 21 | 41 |
| Integration | 4 | 4 |

## Trait Categories

`SpeechToText`, `TextToSpeech`, and `Integration` are used. Integration cases also carry their
provider-area trait.

## Skipped Tests

The four integration cases skip when their provider endpoint is not configured, and otherwise
remain explicitly skipped in the credential-free suite. No unit tests are skipped.

## Layout

```text
Tests/
├── Integration/
│   ├── AzureOpenAiTextToSpeechClientTests.cs
│   ├── AzureSpeechTextToSpeechClientTests.cs
│   ├── AzureSpeechToTextClientTests.cs
│   └── PiperTextToSpeechClientTests.cs
└── Unit/
    ├── FakeSpeechToTextClient.cs
    ├── FakeTextToSpeechClient.cs
    ├── PiperEndpointTests.cs
    ├── SpeechTextNormalizerTests.cs
    ├── TestMetrics.cs
    ├── VoiceMessageTranscriptionServiceTests.cs
    ├── VoiceReplySynthesisServiceTests.cs
    ├── VoiceTranscriptionMetricsTests.cs
    └── WhisperAsrSpeechToTextClientTests.cs
```
