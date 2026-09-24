namespace CasCap.Models;

/// <summary>Typed audio produced by the voice synthesis policy.</summary>
/// <param name="Audio">The encoded audio bytes.</param>
/// <param name="MediaType">The audio MIME type.</param>
/// <param name="FileName">The safe filename offered to a transport adapter.</param>
public sealed record VoiceSynthesisResult(ReadOnlyMemory<byte> Audio, string MediaType, string FileName);
