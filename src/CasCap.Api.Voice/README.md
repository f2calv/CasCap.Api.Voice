# CasCap.Api.Voice

Reusable speech-to-text and text-to-speech provider adapters and processing policies.

## Purpose

This project isolates voice processing from messaging transports and application orchestration.

## Public Surface

Applications depend on the stable `IVoiceTranscriptionService` and `IVoiceSynthesisService`
contracts under `CasCap.Abstractions`. These contracts return transport-neutral
`VoiceTranscriptionResult` and `VoiceSynthesisResult` values.

Provider adapters share the Microsoft.Extensions.AI `ISpeechToTextClient` and
`ITextToSpeechClient` contracts. Configuration selects the default adapter, while dependency
injection can replace either provider client or either complete Voice pipeline.

The library exposes `SpeechToTextConfig`, `TextToSpeechConfig`, `VoiceMetricsConfig`, provider enums,
and result records under `CasCap.Models`. Register the default pipelines with `AddSpeechToText` and
`AddTextToSpeech` from `VoiceServiceCollectionExtensions`.

## Configuration

Options bind from `CasCap:SpeechToTextConfig`, `CasCap:TextToSpeechConfig`, and
`CasCap:VoiceMetricsConfig`. `VoiceMetricsConfig:MetricNamePrefix` defaults to `voice` and should be
set by a host when its telemetry naming convention requires another prefix.

## Dependencies

The project uses Microsoft.Extensions.AI abstractions, shared CasCap configuration/extensions/services,
and optional CasCap Azure authentication and cognitive-services libraries. It has no messaging or
application-orchestration dependency. Release builds consume exact published package versions for
all cross-repository dependencies.
