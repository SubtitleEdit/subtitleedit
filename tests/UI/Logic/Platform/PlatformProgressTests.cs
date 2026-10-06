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

    [Fact]
    public void CompletingCurrentSourceRestoresOtherActiveSource()
    {
        var progress = new FakeProgress();
        var group = new PlatformProgressGroup(() => progress);
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
        Assert.True(progress.IsDisposed);
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

    [Fact]
    public void NewOperationCreatesNewBackendAfterCleanup()
    {
        var instances = new List<FakeProgress>();
        var group = new PlatformProgressGroup(() =>
        {
            var progress = new FakeProgress();
            instances.Add(progress);
            return progress;
        });
        var source = new object();

        group.Update(source, null, false);
        Assert.Empty(instances);
        group.Update(source, 10, false);
        group.Update(source, null, false);
        group.Update(source, 20, false);

        Assert.Equal(2, instances.Count);
        Assert.True(instances[0].IsDisposed);
        Assert.Equal(((double?)20, false), instances[1].Updates[^1]);
        group.Update(source, null, false);
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
