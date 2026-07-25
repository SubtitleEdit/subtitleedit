using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.GrammarCheck;

public class GrammarCheckSettingsWindow : Window
{
    public GrammarCheckSettingsWindow(GrammarCheckSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.GrammarCheck.SettingsTitle;
        Width = 560;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Tools.GrammarCheck;

        var labelInfo = UiUtil.MakeTextBlock(l.SettingsInfo);
        labelInfo.TextWrapping = TextWrapping.Wrap;
        labelInfo.Opacity = 0.75;

        var textBoxUsername = UiUtil.MakeTextBox(300, vm, nameof(vm.Username))
            .WithAccessibleName(l.Username);
        var textBoxApiKey = UiUtil.MakeTextBox(300, vm, nameof(vm.ApiKey))
            .WithAccessibleName(Se.Language.General.ApiKey);
        textBoxApiKey.PasswordChar = '●';
        var textBoxDisabledRules = UiUtil.MakeTextBox(300, vm, nameof(vm.DisabledRules))
            .WithAccessibleName(l.DisabledRules);
        ToolTip.SetTip(textBoxDisabledRules, l.DisabledRulesHint);
        var numericMaxLines = UiUtil.MakeNumericUpDownInt(1, 500, 25, 100, vm, nameof(vm.MaxLinesPerBatch))
            .WithAccessibleName(l.MaxLinesPerBatch);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto"),
            ColumnSpacing = 10,
            RowSpacing = 8,
        };
        grid.Add(UiUtil.MakeTextBlock(l.Username), 0, 0);
        grid.Add(textBoxUsername, 0, 1);
        grid.Add(UiUtil.MakeTextBlock(Se.Language.General.ApiKey), 1, 0);
        grid.Add(textBoxApiKey, 1, 1);
        grid.Add(UiUtil.MakeTextBlock(l.DisabledRules), 2, 0);
        grid.Add(textBoxDisabledRules, 2, 1);
        grid.Add(UiUtil.MakeTextBlock(l.MaxLinesPerBatch), 3, 0);
        grid.Add(numericMaxLines, 3, 1);

        var panel = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 12,
            Children =
            {
                labelInfo,
                grid,
                UiUtil.MakeButtonBar(UiUtil.MakeButtonOk(vm.OkCommand), UiUtil.MakeButtonCancel(vm.CancelCommand)),
            },
        };

        Content = panel;

        Loaded += delegate
        {
            textBoxUsername.Focus();
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
