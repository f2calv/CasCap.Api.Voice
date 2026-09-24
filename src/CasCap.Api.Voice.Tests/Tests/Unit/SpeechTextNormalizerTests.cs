namespace CasCap.Tests.Unit;

/// <summary>Verifies conversion of assistant markdown into speakable text.</summary>
[Trait("Category", "TextToSpeech")]
public sealed class SpeechTextNormalizerTests
{
    [Theory]
    [InlineData("**bold**", "bold.")]
    [InlineData("## Status", "Status.")]
    [InlineData("- one\n- two", "one. two.")]
    [InlineData("[the dashboard](https://example.com)", "the dashboard.")]
    public void ToSpeakable_RemovesMarkup(string input, string expected) =>
        Assert.Equal(expected, SpeechTextNormalizer.ToSpeakable(input));

    [Fact]
    public void ToSpeakable_PreservesTextSymbols()
    {
        Assert.Equal("It is 21°C and costs £5.",
            SpeechTextNormalizer.ToSpeakable("It is 21°C and costs £5"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToSpeakable_EmptyInputReturnsEmpty(string? input) =>
        Assert.Equal(string.Empty, SpeechTextNormalizer.ToSpeakable(input));
}