using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>Standalone host for the shared form; profile management embeds the same control.</summary>
public sealed class TranslationProfileEditorWindow : Window
{
    public TranslationProfile? Result { get; private set; }
    public TranslationProfileEditorWindow(TranslationProfile profile)
    {
        Title = Se.Language.General.Profile;
        Width = 650; Height = 700; MinWidth = 500; MinHeight = 420;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        var editor = new TranslationProfileEditorControl(profile);
        Closed += (_, _) => editor.Dispose();
        var save = new Button { Content = Se.Language.General.Ok, IsDefault = true };
        save.Click += (_, _) =>
        {
            if (!TranslationProfileEditorControl.TryValidate(editor.CaptureDraft(), Se.Settings.AutoTranslate.Profiles, out var result, out var error))
            { editor.ShowError(error); return; }
            Result = result;
            Close();
        };
        var cancel = new Button { Content = Se.Language.General.Cancel, IsCancel = true };
        cancel.Click += (_, _) => Close();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8, Margin = new Thickness(16), Children = { cancel, save } };
        var layout = new DockPanel();
        DockPanel.SetDock(buttons, Dock.Bottom);
        layout.Children.Add(buttons); layout.Children.Add(editor);
        Content = layout;
    }
}
