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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var textBoxName = EditorChrome.MakeTextField(nameof(vm.Name), Se.Language.General.Name, 300);
        var textBoxTitleOrLanguage = EditorChrome.MakeTextField(nameof(vm.TitleOrlanguage), Se.Language.General.Language, 300);

        var switchForced = EditorChrome.MakeSwitch(nameof(vm.IsForced), Se.Language.General.Forced);
        var switchDefault = EditorChrome.MakeSwitch(nameof(vm.IsDefault), Se.Language.General.Default);

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.General.Name, textBoxName, LabelWidth, spacing: 6));
        rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.General.Language, textBoxTitleOrLanguage, LabelWidth, spacing: 6));
        rows.Children.Add(EditorChrome.MakeLabeledRow(string.Empty, EditorChrome.MakeSwitchRow(switchForced, Se.Language.General.Forced), LabelWidth, spacing: 6));
        rows.Children.Add(EditorChrome.MakeLabeledRow(string.Empty, EditorChrome.MakeSwitchRow(switchDefault, Se.Language.General.Default), LabelWidth, spacing: 6));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonPanel = EditorChrome.MakeButtonBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(EditorChrome.MakePanel(EditorChrome.MakeEditorSection(Se.Language.General.Settings, rows, first: true)), 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        Loaded += (_, e) => vm.EditEmbeddedTrackWindowLoaded(e);
        KeyDown += (_, args) => vm.OnKeyDown(args);
    }
}
