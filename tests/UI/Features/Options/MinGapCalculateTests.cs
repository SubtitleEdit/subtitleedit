using System.Globalization;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Options.Settings.MinGapCalculate;
using Xunit;

namespace UITests.Features.Options;

/// <summary>
/// The minimum gap setting is in milliseconds but delivery specs state it in frames, so Subtitle
/// Edit 4 had a small calculator behind a "..." button next to the field. It did not survive the
/// move to SE5 (issue #13906); these cover the arithmetic it does.
/// </summary>
public class MinGapCalculateTests
{
    private static MinGapCalculateViewModel MakeViewModel(double currentFrameRate, int frames)
    {
        Configuration.Settings.General.CurrentFrameRate = currentFrameRate;
        var vm = new MinGapCalculateViewModel();
        vm.Initialize(frames);
        return vm;
    }

    [AvaloniaTheory]
    [InlineData(23.976, 2, 83)]  // the case in the issue screenshot
    [InlineData(25.0, 2, 80)]
    [InlineData(24.0, 1, 42)]
    [InlineData(30.0, 3, 100)]
    [InlineData(59.94, 2, 33)]
    public void CalculatesMillisecondsFromFramesAndFrameRate(double frameRate, int frames, int expectedMs)
    {
        var vm = MakeViewModel(frameRate, frames);

        Assert.Equal(expectedMs, vm.MinGapMs);
    }

    [AvaloniaFact]
    public void StartsFromTheCurrentFrameRateAndOffersIt()
    {
        var vm = MakeViewModel(23.976, 2);

        Assert.Equal(23.976.ToString("0.###", CultureInfo.CurrentCulture), vm.SelectedFrameRate);
        Assert.Contains(vm.SelectedFrameRate, vm.FrameRates);
    }

    [AvaloniaFact]
    public void AnUnusualFrameRateIsAddedToTheListRatherThanDropped()
    {
        var vm = MakeViewModel(48.0, 2);

        Assert.Equal(48.0.ToString("0.###", CultureInfo.CurrentCulture), vm.SelectedFrameRate);
        Assert.Equal(42, vm.MinGapMs);
    }

    [AvaloniaFact]
    public void PickingAnotherFrameRateRecalculates()
    {
        var vm = MakeViewModel(23.976, 2);

        vm.SelectedFrameRate = 25.0.ToString("0.###", CultureInfo.CurrentCulture);

        Assert.Equal(80, vm.MinGapMs);
    }

    [AvaloniaFact]
    public void ChangingTheFrameCountRecalculates()
    {
        var vm = MakeViewModel(25.0, 2);

        vm.Frames = 5;

        Assert.Equal(200, vm.MinGapMs);
    }

    /// <summary>
    /// The frame rate box is editable, so it can hold a half-typed value at any moment - that must
    /// not divide by zero or blow up the binding.
    /// </summary>
    [AvaloniaTheory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    public void UnparsableFrameRateFallsBackToTheCurrentOne(string typed)
    {
        var vm = MakeViewModel(25.0, 2);

        vm.SelectedFrameRate = typed;

        Assert.Equal(80, vm.MinGapMs);
    }

    [AvaloniaFact]
    public void TheTextsQuoteTheNumbersTheUserIsAboutToAccept()
    {
        var vm = MakeViewModel(23.976, 2);

        Assert.Contains("83", vm.CalculationText);
        Assert.Contains("83", vm.UseAsNewGapText);
    }

    [AvaloniaFact]
    public void OkIsWhatTellsTheCallerToAdoptTheValue()
    {
        var vm = MakeViewModel(25.0, 2);
        Assert.False(vm.OkPressed);

        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
    }

    [AvaloniaFact]
    public void CancelLeavesOkPressedUnset()
    {
        var vm = MakeViewModel(25.0, 2);

        vm.CancelCommand.Execute(null);

        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void WindowConstructs()
    {
        var vm = MakeViewModel(23.976, 2);

        var window = new MinGapCalculateWindow(vm);

        Assert.NotNull(window.Content);
        window.Close();
    }
}
