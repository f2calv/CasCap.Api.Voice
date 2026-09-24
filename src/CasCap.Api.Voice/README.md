# CasCap.Api.Voice

Reusable speech-to-text and text-to-speech provider adapters and processing policies.

## Purpose

This project isolates voice processing from messaging transports and application orchestration.

## Public Surface

The library exposes `SpeechToTextConfig`, `TextToSpeechConfig`, `VoiceMetricsConfig`, the voice
provider enums and result records under `CasCap.Models`; processing services and provider adapters
remain under `CasCap.Services`. Register them with `AddSpeechToText` and `AddTextToSpeech` from
`VoiceServiceCollectionExtensions`.

## Configuration

Options bind from `CasCap:SpeechToTextConfig`, `CasCap:TextToSpeechConfig`, and
`CasCap:VoiceMetricsConfig`. `VoiceMetricsConfig:MetricNamePrefix` defaults to `voice` and should be
set by a host when its telemetry naming convention requires another prefix.

## Dependencies

The project uses Microsoft.Extensions.AI abstractions, shared CasCap configuration/extensions/services,
and optional CasCap Azure authentication and cognitive-services libraries. It has no messaging or
application-orchestration dependency and is currently non-packable while the extracted API settles.
