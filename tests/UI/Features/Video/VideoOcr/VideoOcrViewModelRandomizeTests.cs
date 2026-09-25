using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using System;
using Xunit;

namespace UITests.Features;

public class VideoOcrViewModelRandomizeTests
{
    [Fact]
    public void IsRandomized_ShufflesAndRestoresListOrder()
    {
        // Arrange: Create the view model (passing null for dependencies we don't need)
        var vm = new VideoOcrViewModel(null!, null!, null!);
        vm.Lines.Add(new VideoOcrLineItem { Number = 1, Text = "First", StartTime = TimeSpan.FromSeconds(1) });
        vm.Lines.Add(new VideoOcrLineItem { Number = 2, Text = "Second", StartTime = TimeSpan.FromSeconds(2) });
        vm.Lines.Add(new VideoOcrLineItem { Number = 3, Text = "Third", StartTime = TimeSpan.FromSeconds(3) });

        // Act: Trigger the shuffle
        vm.IsRandomized = true;

        // Assert: Ensure no data was lost during the shuffle
        Assert.Equal(3, vm.Lines.Count);

        // Act: Toggle the checkbox back off
        vm.IsRandomized = false;

        // Assert: Ensure they are put back in perfect chronological order and renumbered
        Assert.Equal("First", vm.Lines[0].Text);
        Assert.Equal(1, vm.Lines[0].Number);

        Assert.Equal("Second", vm.Lines[1].Text);
        Assert.Equal(2, vm.Lines[1].Number);

        Assert.Equal("Third", vm.Lines[2].Text);
        Assert.Equal(3, vm.Lines[2].Number);
    }
}