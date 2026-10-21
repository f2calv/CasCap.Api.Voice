# CasCap.Api.Voice.Testing

Canned speech-to-text and text-to-speech clients for testing code built on `CasCap.Api.Voice`
without a speech backend.

## Installation

```powershell
dotnet package add CasCap.Api.Voice.Testing
```

Reference it from test projects only.

## Test Clients

The Voice pipelines depend on the Microsoft.Extensions.AI
[`ISpeechToTextClient`](https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.AI.Abstractions/SpeechToText/ISpeechToTextClient.cs)
and [`ITextToSpeechClient`](https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.AI.Abstractions/TextToSpeech/ITextToSpeechClient.cs)
contracts. These fakes stand in for a real provider, so a consumer can
exercise transcription, synthesis and its own orchestration deterministically. There is no
network, audio tooling or credential involved.

**Target framework:** `net10.0`

## Public Surface

All types live in the `CasCap.Fakes` namespace.

| Type | Behaviour |
| --- | --- |
| `FakeSpeechToTextClient` | Returns `Transcript`. It counts `Requests`, fails with `Failure` when set, and waits on `Gate` when set, so a test can observe work that is still in flight. |
| `FakeTextToSpeechClient` | Returns `Audio` with `MediaType` (Ogg Opus by default). It counts `Requests`, records `LastText`, and fails with `Failure` when set. |

The default synthesis bytes are only the four-byte `OggS` signature, not playable audio. Supply
appropriate synthetic content when the consumer validates or decodes its result.

## Usage

Replace the provider client before the Voice pipeline is resolved:

```csharp
using CasCap.Fakes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable MEAI001
var stt = new FakeSpeechToTextClient { Transcript = "synthetic transcript" };
var tts = new FakeTextToSpeechClient();

services.AddSingleton<ISpeechToTextClient>(stt);
services.AddSingleton<ITextToSpeechClient>(tts);
services.AddSpeechToText();
services.AddTextToSpeech();
#pragma warning restore MEAI001
```

`services` is the test host's `IServiceCollection`. The provider interfaces are experimental upstream,
so the example scopes that compiler diagnostic to the registration code.

`AddSpeechToText` and `AddTextToSpeech` register their provider clients with `TryAdd`. A fake
registered before them therefore takes precedence, and so does a later plain `AddSingleton`.
Replacing a provider does not bypass media validation or synthesis policy: supply valid audio and
enable a reply mode when testing the full pipelines.

See the [core package usage](https://github.com/f2calv/CasCap.Api.Voice/blob/main/src/CasCap.Api.Voice/README.md#usage)
for the application-facing contracts.
