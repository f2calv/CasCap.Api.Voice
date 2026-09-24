# CasCap.Api.Voice

Provider-neutral speech-to-text and text-to-speech components for .NET applications.

## Status

The repository is being bootstrapped from proven application code. The library is intentionally
non-packable while its public contract is extracted and stabilized.

## Public Surface

The first implementation tranche will provide speech provider adapters, bounded audio
normalization, transcription and synthesis policies, typed results, configuration, metrics, and
dependency-injection registration.

## Configuration

No configuration contract is available until the extraction lands.

## Dependencies

Development builds use adjacent source checkouts for shared CasCap libraries. Release builds use
their published package equivalents so continuous integration remains self-contained.

## Development

Build the Debug solution:

```powershell
dotnet build CasCap.Api.Voice.Debug.slnx --configuration Debug
```

Credential-free tests will live under `src/CasCap.Api.Voice.Tests/Tests/Unit`. Integration tests
requiring external speech services will be explicitly categorized and excluded from the default
local test command.

## License

This project is released under the [Unlicense](LICENSE).
