using System.Collections.ObjectModel;
using System.Globalization;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The toolbar frame rate combo box used to be hard-coded to the first preset (23.976) at start-up
/// no matter what the live frame rate was, and a rate outside the preset list - a video with an
/// unusual rate, or a value restored from settings - selected nothing at all and left the combo box
/// blank. <see cref="FrameRateHelper.SelectInList"/> owns both cases now.
/// </summary>
public class FrameRateToolbarComboTests
{
    private static ObservableCollection<string> Presets() => new()
    {
        "23.976",
        "24",
        "25",
        "29.97",
        "30",
        "50",
        "59.94",
        "60",
        "120",
    };

    [Theory]
    [InlineData(23.976, "23.976")]
    [InlineData(25.0, "25")]
    [InlineData(29.97, "29.97")]
    [InlineData(120.0, "120")]
    public void SelectInList_SelectsPresetWithoutAddingIt(double frameRate, string expected)
    {
        var frameRates = Presets();

        var selected = FrameRateHelper.SelectInList(frameRates, frameRate);

        Assert.Equal(expected, selected);
        Assert.Equal(9, frameRates.Count);
    }

    [Fact]
    public void SelectInList_AddsRateThatIsNotAPreset()
    {
        var frameRates = Presets();

        var selected = FrameRateHelper.SelectInList(frameRates, 23.98);

        Assert.Equal("23.98", selected);
        Assert.Equal("23.98", frameRates[0]);
        Assert.Equal(10, frameRates.Count);
    }

    [Fact]
    public void SelectInList_AddsTheSameRateOnlyOnce()
    {
        var frameRates = Presets();

        FrameRateHelper.SelectInList(frameRates, 23.98);
        var selected = FrameRateHelper.SelectInList(frameRates, 23.98);

        Assert.Equal("23.98", selected);
        Assert.Equal(10, frameRates.Count);
    }

    // An empty or corrupt setting must not select an empty combo box.
    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void SelectInList_FallsBackToFirstPresetForNonPositiveRates(double frameRate)
    {
        var frameRates = Presets();

        var selected = FrameRateHelper.SelectInList(frameRates, frameRate);

        Assert.Equal("23.976", selected);
        Assert.Equal(9, frameRates.Count);
    }

    // The combo box parses the selected item back with the invariant culture, so a Danish (comma
    // decimal separator) machine must not end up with an unparsable "23,98" item.
    [Fact]
    public void SelectInList_UsesInvariantCultureRegardlessOfCurrentCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("da-DK");
            var frameRates = Presets();

            var selected = FrameRateHelper.SelectInList(frameRates, 23.98);

            Assert.Equal("23.98", selected);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
