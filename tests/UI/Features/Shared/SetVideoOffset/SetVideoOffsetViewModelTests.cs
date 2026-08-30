using Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace UITests.Features.Shared.SetVideoOffset;

/// <summary>
/// The dialog stays open across an Apply, and remembers the offsets it applied so the same few
/// values can be picked again instead of retyped (SE 4 parity).
/// </summary>
public class SetVideoOffsetViewModelTests
{
    private static readonly TimeSpan TenHours = TimeSpan.FromHours(10);
    private static readonly TimeSpan NineFiftyNineForty = new(0, 9, 59, 40, 0);

    private sealed class Recorder
    {
        internal List<(TimeSpan Offset, bool Relative, bool KeepTimeCodes)> Applied { get; } = new();
        internal int ResetCount { get; private set; }

        internal SetVideoOffsetViewModel NewViewModel()
        {
            var vm = new SetVideoOffsetViewModel();
            vm.Initialize(
                (offset, relative, keepTimeCodes) => Applied.Add((offset, relative, keepTimeCodes)),
                () => ResetCount++);
            return vm;
        }
    }

    [Fact]
    public void Apply_AppliesWithoutClosingAndCanBeRepeatedWithNewValues()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long>();
        var recorder = new Recorder();
        var vm = recorder.NewViewModel();

        vm.TimeOffset = NineFiftyNineForty;
        vm.KeepTimeCodes = true;
        vm.ApplyCommand.Execute(null);

        vm.TimeOffset = TenHours;
        vm.KeepTimeCodes = false;
        vm.ApplyCommand.Execute(null);

        Assert.Equal(
            new[]
            {
                (NineFiftyNineForty, false, true),
                (TenHours, false, false),
            },
            recorder.Applied);
    }

    [Fact]
    public void Ok_AppliesOnce()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long>();
        var recorder = new Recorder();
        var vm = recorder.NewViewModel();

        vm.TimeOffset = NineFiftyNineForty;
        vm.OkCommand.Execute(null);

        Assert.Single(recorder.Applied);
        Assert.Equal(NineFiftyNineForty, recorder.Applied[0].Offset);
    }

    [Fact]
    public void Reset_ClearsTheOffsetFieldAndCallsBack()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long>();
        var recorder = new Recorder();
        var vm = recorder.NewViewModel();

        vm.TimeOffset = TenHours;
        vm.ResetCommand.Execute(null);

        Assert.Equal(1, recorder.ResetCount);
        Assert.Equal(TimeSpan.Zero, vm.TimeOffset);
        Assert.Empty(recorder.Applied);
    }

    [Fact]
    public void AppliedOffsets_AreRememberedMostRecentFirstAndPersisted()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long>();
        var recorder = new Recorder();
        var vm = recorder.NewViewModel();

        vm.TimeOffset = NineFiftyNineForty;
        vm.ApplyCommand.Execute(null);
        vm.TimeOffset = TenHours;
        vm.ApplyCommand.Execute(null);

        Assert.Equal(
            new[] { (long)TenHours.TotalMilliseconds, (long)NineFiftyNineForty.TotalMilliseconds },
            vm.OffsetHistory.Take(2).Select(p => p.TotalMilliseconds));
        Assert.Equal(
            vm.OffsetHistory.Select(p => p.TotalMilliseconds),
            Se.Settings.General.VideoOffsetHistoryInMs);

        // A new dialog offers what the previous one applied.
        var next = recorder.NewViewModel();
        Assert.Equal((long)TenHours.TotalMilliseconds, next.OffsetHistory[0].TotalMilliseconds);
    }

    [Fact]
    public void ReusingAnOffset_MovesItToTheTopWithoutDuplicating()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long>();
        var recorder = new Recorder();
        var vm = recorder.NewViewModel();

        vm.TimeOffset = NineFiftyNineForty;
        vm.ApplyCommand.Execute(null);
        vm.TimeOffset = TenHours;
        vm.ApplyCommand.Execute(null);
        vm.TimeOffset = NineFiftyNineForty;
        vm.ApplyCommand.Execute(null);

        Assert.Equal(
            new[] { (long)NineFiftyNineForty.TotalMilliseconds, (long)TenHours.TotalMilliseconds },
            vm.OffsetHistory.Take(2).Select(p => p.TotalMilliseconds));
        Assert.Equal(
            1,
            vm.OffsetHistory.Count(p => p.TotalMilliseconds == (long)NineFiftyNineForty.TotalMilliseconds));
    }

    [Fact]
    public void History_KeepsTheTenMostRecentOffsets()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long>();
        var recorder = new Recorder();
        var vm = recorder.NewViewModel();

        for (var minutes = 1; minutes <= 12; minutes++)
        {
            vm.TimeOffset = TimeSpan.FromMinutes(minutes);
            vm.ApplyCommand.Execute(null);
        }

        Assert.Equal(10, vm.OffsetHistory.Count);
        Assert.Equal((long)TimeSpan.FromMinutes(12).TotalMilliseconds, vm.OffsetHistory[0].TotalMilliseconds);
        Assert.Equal((long)TimeSpan.FromMinutes(3).TotalMilliseconds, vm.OffsetHistory[9].TotalMilliseconds);
    }

    [Fact]
    public void ZeroOffset_IsNotRemembered()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long>();
        var recorder = new Recorder();
        var vm = recorder.NewViewModel();

        vm.TimeOffset = TimeSpan.Zero;
        vm.ApplyCommand.Execute(null);

        Assert.Single(recorder.Applied);
        Assert.DoesNotContain(vm.OffsetHistory, p => p.TotalMilliseconds == 0);
    }

    [Fact]
    public void EmptyHistory_OffersOneAndTenHours()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long>();
        var vm = new Recorder().NewViewModel();

        Assert.Equal(
            new[] { 60L * 60 * 1000, 10L * 60 * 60 * 1000 },
            vm.OffsetHistory.Select(p => p.TotalMilliseconds));
    }

    [Fact]
    public void PickingAHistoryItem_FillsTheOffsetField()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long> { (long)NineFiftyNineForty.TotalMilliseconds };
        var vm = new Recorder().NewViewModel();

        vm.SelectedOffsetHistoryItem = vm.OffsetHistory[0];

        Assert.Equal(NineFiftyNineForty, vm.TimeOffset);
    }

    [Fact]
    public void TypingOverAPickedOffset_ClearsTheSelectionSoItCanBePickedAgain()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long> { (long)NineFiftyNineForty.TotalMilliseconds };
        var vm = new Recorder().NewViewModel();

        vm.SelectedOffsetHistoryItem = vm.OffsetHistory[0];
        vm.TimeOffset = TimeSpan.FromSeconds(42);
        Assert.Null(vm.SelectedOffsetHistoryItem);

        vm.SelectedOffsetHistoryItem = vm.OffsetHistory[0];
        Assert.Equal(NineFiftyNineForty, vm.TimeOffset);
    }

    [Fact]
    public void HistoryItemDisplayText_IsATimeCode()
    {
        using var _ = new SettingsScope("General.UseFrameMode");
        Se.Settings.General.UseFrameMode = false;

        Assert.Equal("00:59:40,000", new VideoOffsetHistoryItem(59 * 60 * 1000 + 40 * 1000).DisplayText);
    }
}
