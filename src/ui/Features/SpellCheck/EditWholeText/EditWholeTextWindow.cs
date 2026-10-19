using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.SpellCheck.EditWholeText;

/// <summary>
/// Edit the whole text of the line with the word not found, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class EditWholeTextWindow : Window
{
    public EditWholeTextWindow(EditWholeTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.EditWholeTextTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        // "Line x of y" as the section header
        var labelLineInfo = new TextBlock
        {
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            LetterSpacing = 0.6,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LineInfo)),
        };

        var textBoxWholeText = new TextBox
        {
            AcceptsReturn = true,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            [!TextBox.TextProperty] = new Binding(nameof(vm.WholeText)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            Width = 420,
            Height = 100,
        }.WithAccessibleName(Se.Language.SpellCheck.EditWholeTextTitle);
        textBoxWholeText.Classes.Add("text");
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textBoxWholeText.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(labelLineInfo);
        panel.Children.Add(textBoxWholeText);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        buttonOk.Margin = new Thickness(0);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        buttonCancel.Margin = new Thickness(0);

        Content = EditorChrome.MakeRoot(new Border { Child = panel, Padding = new Thickness(14, 12, 14, 14) }, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        // The text box gets the focus, so a bare Space types a space (#15028)
        UiUtil.FocusOnFirstActivation(this, textBoxWholeText); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
