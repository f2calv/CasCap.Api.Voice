# CasCap.Api.Voice.Testing

Canned speech-to-text and text-to-speech clients for testing code built on `CasCap.Api.Voice`
without a speech backend.

## Installation

```powershell
dotnet package add CasCap.Api.Voice.Testing
```

Reference it from test projects only.

## Purpose

The Voice pipelines depend on the Microsoft.Extensions.AI `ISpeechToTextClient` and
`ITextToSpeechClient` contracts. These fakes stand in for a real provider, so a consumer can
exercise transcription, synthesis and its own orchestration deterministically. There is no
network, audio tooling or credential involved.

**Target framework:** `net10.0`

## Public Surface

All types live in the `CasCap.Fakes` namespace.

| Type | Behaviour |
| --- | --- |
| `FakeSpeechToTextClient` | Returns `Transcript`. It counts `Requests`, fails with `Failure` when set, and waits on `Gate` when set, so a test can observe work that is still in flight. |
| `FakeTextToSpeechClient` | Returns `Audio` with `MediaType` (Ogg Opus by default). It counts `Requests`, records `LastText`, and fails with `Failure` when set. |

Both contracts are published as experimental (`MEAI001`). Consumers that reference the fakes by
their interface type need the same suppression that production code uses.

## Usage

Replace the provider client before the Voice pipeline is resolved:

```csharp
var stt = new FakeSpeechToTextClient { Transcript = "what trades are open" };
var tts = new FakeTextToSpeechClient();

services.AddSpeechToText();
services.AddTextToSpeech();
services.AddSingleton<ISpeechToTextClient>(stt);
services.AddSingleton<ITextToSpeechClient>(tts);
```

`AddSpeechToText` and `AddTextToSpeech` register their provider clients with `TryAdd`. A fake
registered before them therefore takes precedence, and so does a later plain `AddSingleton`.
