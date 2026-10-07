using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Logic.Platform.Progress;

namespace UITests.Logic.Platform;

public class PlatformProgressTests
{
    [AvaloniaFact]
    public void OwnedDialogsResolveToMainTaskbarWindow()
    {
        var main = new Window();
        var dialog = new Window();
        var nestedDialog = new Window();
        try
        {
            main.Show();
            dialog.Show(main);
            nestedDialog.Show(dialog);

            Assert.Same(main, PlatformProgress.GetOwner(nestedDialog));
        }
        finally
        {
            nestedDialog.Close();
            dialog.Close();
            main.Close();
        }
    }

    private sealed class FakeProgress : IPlatformProgress
    {
        public List<(double? Percentage, bool Indeterminate)> Updates { get; } = [];
        public bool IsDisposed { get; private set; }

        public void Update(double? percentage, bool indeterminate) => Updates.Add((percentage, indeterminate));
        public void Dispose() => IsDisposed = true;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletingCurrentSourceRestoresOtherActiveSource(bool keepBackendWhenIdle)
    {
        var progress = new FakeProgress();
        var group = new PlatformProgressGroup(() => progress, keepBackendWhenIdle);
        var first = new object();
        var second = new object();

        group.Update(first, 70, false);
        group.Update(second, 20, false);
        group.Update(first, 80, false);
        Assert.Equal(((double?)20, false), progress.Updates[^1]);

        group.Update(second, null, false);
        Assert.Equal(((double?)80, false), progress.Updates[^1]);
        Assert.False(progress.IsDisposed);

        group.Update(first, null, false);
        Assert.Equal(((double?)null, false), progress.Updates[^1]);
        Assert.Equal(!keepBackendWhenIdle, progress.IsDisposed);
    }

    [Fact]
    public void CompletingBackgroundSourceKeepsCurrentSource()
    {
        var progress = new FakeProgress();
        var group = new PlatformProgressGroup(() => progress);
        var first = new object();
        var second = new object();

        group.Update(first, 70, false);
        group.Update(second, 20, false);
        group.Update(first, null, false);

        Assert.Equal(2, progress.Updates.Count);
        Assert.Equal(((double?)20, false), progress.Updates[^1]);
        Assert.False(progress.IsDisposed);
        group.Update(second, null, false);
    }

    [Theory]
    [InlineData(-10, 0)]
    [InlineData(150, 100)]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, 0)]
    public void InvalidValuesAreClamped(double value, double expected)
    {
        var progress = new FakeProgress();
        var group = new PlatformProgressGroup(() => progress);
        var source = new object();

        group.Update(source, value, false);

        Assert.Equal(((double?)expected, false), progress.Updates[^1]);
        group.Update(source, null, false);
    }

    [Fact]
    public void IndeterminateSourceRemainsActive()
    {
        var progress = new FakeProgress();
        var group = new PlatformProgressGroup(() => progress);
        var source = new object();

        group.Update(source, null, true);
        Assert.Equal(((double?)null, true), progress.Updates[^1]);
        Assert.False(progress.IsDisposed);

        group.Update(source, 35, false);
        Assert.Equal(((double?)35, false), progress.Updates[^1]);
        group.Update(source, null, false);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RetainedBackendClearsAndIsReusedForNextOperation(bool indeterminate)
    {
        var instances = new List<FakeProgress>();
        var group = new PlatformProgressGroup(() =>
        {
            var progress = new FakeProgress();
            instances.Add(progress);
            return progress;
        }, keepBackendWhenIdle: true);
        var first = new object();
        var second = new object();
        double? percentage = indeterminate ? null : 10;

        group.Update(first, null, false);
        Assert.Empty(instances);

        group.Update(first, percentage, indeterminate);
        var backend = Assert.Single(instances);
        group.Update(first, null, false);

        Assert.Equal(2, backend.Updates.Count);
        Assert.Equal((percentage, indeterminate), backend.Updates[0]);
        Assert.Equal(((double?)null, false), backend.Updates[1]);
        Assert.False(backend.IsDisposed);

        group.Update(second, 20, false);

        Assert.Same(backend, Assert.Single(instances));
        Assert.Equal(((double?)20, false), backend.Updates[^1]);
        group.Update(second, null, false);
        Assert.Equal(4, backend.Updates.Count);
        Assert.Equal(((double?)null, false), backend.Updates[^1]);
        Assert.False(backend.IsDisposed);
    }

    [Fact]
    public void UpdatesWithinSameWholePercentageAreSkipped()
    {
        var progress = new FakeProgress();
        var group = new PlatformProgressGroup(() => progress);
        var source = new object();

        group.Update(source, 10.1, false);
        group.Update(source, 10.99, false);
        group.Update(source, 11, false);
        group.Update(source, 11.9, false);
        group.Update(source, 10.9, false);

        Assert.Equal(new[] { ((double?)10, false), ((double?)11, false), ((double?)10, false) }, progress.Updates);
        group.Update(source, null, false);
        Assert.Equal(((double?)null, false), progress.Updates[^1]);
    }

    [Fact]
    public void IndeterminateTransitionsAreSentButNumericChangesAreSkipped()
    {
        var progress = new FakeProgress();
        var group = new PlatformProgressGroup(() => progress);
        var source = new object();

        group.Update(source, 35.2, false);
        group.Update(source, 35.2, true);
        group.Update(source, 80.9, true);
        group.Update(source, null, true);
        group.Update(source, 35.9, false);

        Assert.Equal(new[] { ((double?)35, false), ((double?)null, true), ((double?)35, false) }, progress.Updates);
        group.Update(source, null, false);
        Assert.Equal(((double?)null, false), progress.Updates[^1]);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void NewSourceAndRestorationAreSentEvenWithSameEffectiveState(bool keepBackendWhenIdle, bool indeterminate)
    {
        var progress = new FakeProgress();
        var group = new PlatformProgressGroup(() => progress, keepBackendWhenIdle);
        var first = new object();
        var second = new object();

        group.Update(first, 20.2, indeterminate);
        group.Update(second, 20.9, indeterminate);
        group.Update(first, 20.7, indeterminate);
        Assert.Equal(2, progress.Updates.Count);

        group.Update(second, null, false);

        double? effectivePercentage = indeterminate ? null : 20;
        Assert.Equal(new[] { (effectivePercentage, indeterminate), (effectivePercentage, indeterminate),
            (effectivePercentage, indeterminate) }, progress.Updates);
        group.Update(first, null, false);
        Assert.Equal(((double?)null, false), progress.Updates[^1]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearAndRestartOfSameSourceAreSentEvenWithSameWholePercentage(bool keepBackendWhenIdle)
    {
        var instances = new List<FakeProgress>();
        var group = new PlatformProgressGroup(() =>
        {
            var progress = new FakeProgress();
            instances.Add(progress);
            return progress;
        }, keepBackendWhenIdle);
        var source = new object();

        group.Update(source, null, false);
        Assert.Empty(instances);

        group.Update(source, 20.2, false);
        group.Update(source, 20.9, false);
        group.Update(source, null, false);

        Assert.Equal(new[] { ((double?)20, false), ((double?)null, false) }, instances[0].Updates);
        Assert.Equal(!keepBackendWhenIdle, instances[0].IsDisposed);

        group.Update(source, 20.7, false);

        Assert.Equal(keepBackendWhenIdle ? 1 : 2, instances.Count);
        Assert.Equal(keepBackendWhenIdle ? 3 : 1, instances[^1].Updates.Count);
        Assert.Equal(((double?)20, false), instances[^1].Updates[^1]);
        group.Update(source, null, false);
        Assert.Equal(((double?)null, false), instances[^1].Updates[^1]);
    }

    [Fact]
    public void UnavailableBackendDoesNotInterruptOperation()
    {
        var group = new PlatformProgressGroup(() => throw new InvalidOperationException());
        var source = new object();

        group.Update(source, 50, false);
        group.Update(source, null, false);
    }
}
