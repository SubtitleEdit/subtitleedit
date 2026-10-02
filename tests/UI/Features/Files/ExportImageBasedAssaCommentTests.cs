using System;
using System.Collections.ObjectModel;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Files;

/// <summary>
/// ASSA renderers never draw a {comment} block, so an image export of an ASSA/SSA subtitle must
/// not draw it either - but in other formats a brace is real text and stays (#15584).
/// </summary>
public class ExportImageBasedAssaCommentTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static ExportImageBasedViewModel BuildViewModel(bool removeAssaCommentBlocks)
    {
        var vm = new ExportImageBasedViewModel(
            new FileHelper(),
            new FolderHelper(),
            new WindowService(new NullServiceProvider()));

        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new SubtitleLineViewModel
            {
                Number = 1,
                Text = "{tl note}{\\i1}Hello{\\i0} {laughs}",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        vm.Initialize(new ExportHandlerBluRaySup(), subtitles, null, null, removeAssaCommentBlocks: removeAssaCommentBlocks);
        return vm;
    }

    [AvaloniaFact]
    public void Assa_CommentBlocksAreNotDrawn()
    {
        Assert.Equal("<i>Hello</i>", BuildViewModel(true).GetImageParameter(0).Text.Trim());
    }

    [AvaloniaFact]
    public void OtherFormats_BracesStayText()
    {
        Assert.Contains("{laughs}", BuildViewModel(false).GetImageParameter(0).Text);
    }
}
