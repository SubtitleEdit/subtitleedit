using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

/// <summary>
/// Track properties (name, language, forced, default), in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class EditEmbeddedTrackWindow : Window
{
    private const double LabelWidth = 84;

    public EditEmbeddedTrackWindow(EditEmbeddedTrackViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Settings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        VideoSubDialogChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var textBoxName = VideoSubDialogChrome.MakeTextBox(nameof(vm.Name), Se.Language.General.Name, 300);
        var textBoxTitleOrLanguage = VideoSubDialogChrome.MakeTextBox(nameof(vm.TitleOrlanguage), Se.Language.General.Language, 300);

        var switchForced = VideoSubDialogChrome.MakeSwitch(nameof(vm.IsForced), Se.Language.General.Forced);
        var switchDefault = VideoSubDialogChrome.MakeSwitch(nameof(vm.IsDefault), Se.Language.General.Default);

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(VideoSubDialogChrome.MakeRow(Se.Language.General.Name, textBoxName, LabelWidth));
        rows.Children.Add(VideoSubDialogChrome.MakeRow(Se.Language.General.Language, textBoxTitleOrLanguage, LabelWidth));
        rows.Children.Add(VideoSubDialogChrome.MakeRow(string.Empty, VideoSubDialogChrome.MakeSwitchRow(switchForced, Se.Language.General.Forced), LabelWidth));
        rows.Children.Add(VideoSubDialogChrome.MakeRow(string.Empty, VideoSubDialogChrome.MakeSwitchRow(switchDefault, Se.Language.General.Default), LabelWidth));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonPanel = VideoSubDialogChrome.MakeButtonBar(
            null,
            UiUtil.MakeButtonCancel(vm.CancelCommand),
            buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(VideoSubDialogChrome.MakePanel(EditorChrome.MakeEditorSection(Se.Language.General.Settings, rows, first: true)), 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        Loaded += (_, e) => vm.EditEmbeddedTrackWindowLoaded(e);
        KeyDown += (_, args) => vm.OnKeyDown(args);
    }
}
