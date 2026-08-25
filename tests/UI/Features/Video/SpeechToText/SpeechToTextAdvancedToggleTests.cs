using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Xunit;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// The buttons in the advanced speech-to-text window are toggles that read their own state
/// back out of the parameter text. What each one reads has to be exactly the switches it
/// sets: a parameter that merely looks similar - a maximum length from the "Standard" preset,
/// a fuller JSON file, word timestamps - must leave the button off, or the next press removes
/// something instead of switching the button's own setting on.
/// </summary>
public class SpeechToTextAdvancedToggleTests
{
    private static SpeechToTextAdvancedViewModel MakeViewModel(string parameters)
    {
        var vm = new SpeechToTextAdvancedViewModel { Parameters = parameters };
        return vm;
    }

    [Fact]
    public void CrispAsrStandardLeavesHighlightWordOff()
    {
        var vm = MakeViewModel(string.Empty);

        vm.StandardCrispAsrCommand.Execute(null);

        Assert.Equal("--max-len 50 --split-on-punct", vm.Parameters);
        Assert.False(vm.IsHighlightWordsCrispAsrActive);
    }

    /// <summary>One press after "Standard" switches highlighting on - it used to take two.</summary>
    [Fact]
    public void CrispAsrHighlightWordFollowsStandardInOnePress()
    {
        var vm = MakeViewModel(string.Empty);
        vm.StandardCrispAsrCommand.Execute(null);

        vm.EnableHighlightWordsCrispAsrCommand.Execute(null);

        Assert.Equal("-ml 1 -sow", vm.Parameters);
        Assert.True(vm.IsHighlightWordsCrispAsrActive);
    }

    [Fact]
    public void CrispAsrHighlightWordComesOffAgain()
    {
        var vm = MakeViewModel("--vad -ml 1 -sow");

        Assert.True(vm.IsHighlightWordsCrispAsrActive);
        vm.EnableHighlightWordsCrispAsrCommand.Execute(null);

        Assert.Equal("--vad", vm.Parameters);
        Assert.False(vm.IsHighlightWordsCrispAsrActive);
    }

    /// <summary>A maximum length that is not one word is a length, not highlighting.</summary>
    [Theory]
    [InlineData("--max-len 50")]
    [InlineData("-ml 50 -sow")]
    [InlineData("-ml 1")]
    [InlineData("-sow")]
    public void CrispAsrHighlightWordNeedsBothSwitches(string parameters)
    {
        Assert.False(MakeViewModel(parameters).IsHighlightWordsCrispAsrActive);
    }

    [Fact]
    public void WhisperCppWordLevelNeedsTheWordOutput()
    {
        Assert.False(MakeViewModel("-ojf").IsWordLevelCppActive);
        Assert.True(MakeViewModel("-owts -ojf").IsWordLevelCppActive);
    }

    [Fact]
    public void WhisperCppWordLevelKeepsWhatIsAlreadyThere()
    {
        var vm = MakeViewModel("-t 8");

        vm.EnableWordLevelCppCommand.Execute(null);
        Assert.Equal("-t 8 -owts -ojf", vm.Parameters);
        Assert.True(vm.IsWordLevelCppActive);

        vm.EnableWordLevelCppCommand.Execute(null);
        Assert.Equal("-t 8", vm.Parameters);
        Assert.False(vm.IsWordLevelCppActive);
    }

    [Theory]
    [InlineData("--word_timestamps True")]
    [InlineData("--highlight_words False")]
    public void CTranslate2HighlightWordNeedsHighlightingItself(string parameters)
    {
        Assert.False(MakeViewModel(parameters).IsHighlightWordsCTranslate2Active);
    }

    /// <summary>A switch set to False is the switch off, and a model path alone does nothing.</summary>
    [Theory]
    [InlineData("--vad_filter False")]
    [InlineData("--vad_filter 0")]
    public void CTranslate2VadOffValuesLeaveTheButtonOff(string parameters)
    {
        Assert.False(MakeViewModel(parameters).IsVadCTranslate2Active);
    }

    [Fact]
    public void VadNeedsTheVadSwitchNotJustAModelPath()
    {
        Assert.False(MakeViewModel("--vad-model \"x.bin\"").IsVadCppActive);
        Assert.False(MakeViewModel("-vm x.bin").IsVadCppActive);
        Assert.True(MakeViewModel("--vad").IsVadCppActive);
        Assert.True(MakeViewModel("--vad --vad-model \"x.bin\"").IsVadCrispAsrActive);
    }

    /// <summary>Switching VAD off also clears the short-form model path both backends accept.</summary>
    [Fact]
    public void CrispAsrVadOffClearsShortFormModelPath()
    {
        var vm = MakeViewModel("-vm x.bin --vad -t 8");

        Assert.True(vm.IsVadCrispAsrActive);
        vm.EnableVadCrispAsrCommand.Execute(null);

        Assert.Equal("-t 8", vm.Parameters);
        Assert.False(vm.IsVadCrispAsrActive);
    }

    /// <summary>The highlight press turns a switched-off VAD filter into a switched-on one, once.</summary>
    [Fact]
    public void CTranslate2HighlightWordReplacesAnOffVadFilter()
    {
        var vm = MakeViewModel("--vad_filter False");

        vm.WhisperCTranslate2HighLightWordCommand.Execute(null);

        Assert.Equal("--vad_filter True --highlight_words True --word_timestamps True", vm.Parameters);
        Assert.True(vm.IsVadCTranslate2Active);
    }

    [Fact]
    public void CTranslate2HighlightWordLeavesTheVadFilterOnWhenSwitchedOff()
    {
        var vm = MakeViewModel(string.Empty);

        vm.WhisperCTranslate2HighLightWordCommand.Execute(null);
        Assert.Equal("--vad_filter True --highlight_words True --word_timestamps True", vm.Parameters);
        Assert.True(vm.IsHighlightWordsCTranslate2Active);

        vm.WhisperCTranslate2HighLightWordCommand.Execute(null);
        Assert.Equal("--vad_filter True", vm.Parameters);
        Assert.False(vm.IsHighlightWordsCTranslate2Active);
        Assert.True(vm.IsVadCTranslate2Active);
    }
}
