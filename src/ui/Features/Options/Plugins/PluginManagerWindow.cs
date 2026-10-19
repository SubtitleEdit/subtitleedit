using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

/// <summary>
/// The installed plugins in the editor look (see <see cref="EditorChrome"/>): the list with the
/// "apply to" choice below it, the actions in a column to the right, Close in the button bar.
/// </summary>
public class PluginManagerWindow : Window
{
    private readonly PluginManagerViewModel _vm;

    public PluginManagerWindow(PluginManagerViewModel vm)
    {
        _vm = vm;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Plugins.Title.Replace("_", string.Empty); // the language string doubles as a menu header, where "_" marks the access key
        CanResize = true;
        Width = 850;
        Height = 480;
        MinWidth = 600;
        MinHeight = 320;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox();
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(vm.Plugins)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(vm.SelectedPlugin)) { Mode = BindingMode.TwoWay });
        ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);
        AutomationProperties.SetName(listBox, Se.Language.General.Installed);
        listBox.ItemTemplate = new FuncDataTemplate<PluginDisplayItem>((_, _) =>
        {
            var checkBox = new CheckBox { VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 0, 4, 0) };
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(PluginDisplayItem.IsEnabled)) { Mode = BindingMode.TwoWay });
            checkBox.Bind(IsEnabledProperty, new Binding(nameof(PluginDisplayItem.CanRun)));
            // The plugin name is in a separate text block - name the check box too, or a screen
            // reader announces a bare "check box" (#12087).
            checkBox.Bind(AutomationProperties.NameProperty, new Binding(nameof(PluginDisplayItem.Name)));

            var name = new TextBlock { FontWeight = FontWeight.SemiBold };
            name.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.Name)));

            var version = new TextBlock { FontSize = 11.5, Foreground = EditorChrome.FaintTextBrush, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Bottom };
            version.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.Version)));

            var author = new TextBlock { FontSize = 11.5, Foreground = EditorChrome.FaintTextBrush, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Bottom };
            author.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.Author)));

            var titleRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { name, version, author },
            };

            var description = new TextBlock { FontSize = 12.5, Foreground = EditorChrome.DimTextBrush, TextWrapping = TextWrapping.Wrap };
            description.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.Description)));

            var status = new TextBlock { FontSize = 11, Foreground = EditorChrome.FaintTextBrush };
            status.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.StatusText)));

            var textPanel = new StackPanel
            {
                Spacing = 2,
                Margin = new Thickness(4, 0, 0, 0),
                Children = { titleRow, description, status },
            };

            // A grid rather than a horizontal stack panel, so the description wraps at the
            // list's width instead of running off its right edge.
            var row = new Grid
            {
                Margin = new Thickness(0, 4),
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
            };
            row.Add(checkBox, 0, 0);
            row.Add(textPanel, 0, 1);
            return row;
        }, true);

        var buttonGetPlugins = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.Plugins.GetPluginsOnline, vm.GetPluginsOnlineCommand)
            .WithIconLeft(IconNames.CloudDownload));
        buttonGetPlugins.Classes.Add("primary");
        var buttonOpenFolder = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenPluginsFolderCommand)
            .WithIconLeft(IconNames.FolderOpen));
        var buttonRemove = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.General.Remove, vm.RemovePluginCommand)
            .WithIconLeft(IconNames.Trash));
        var buttonUpdateAll = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(string.Empty, vm.UpdateAllCommand)
            .WithIconLeft(IconNames.Refresh));
        // Replace the empty placeholder text with a bound TextBlock inside the icon+text panel.
        if (buttonUpdateAll.Content is StackPanel stack && stack.Children.OfType<TextBlock>().FirstOrDefault() is { } labelTb)
        {
            labelTb.Bind(TextBlock.TextProperty, new Binding(nameof(vm.UpdateAllButtonText)));
        }
        buttonUpdateAll.Bind(AutomationProperties.NameProperty, new Binding(nameof(vm.UpdateAllButtonText)));
        buttonUpdateAll.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.HasUpdates)));
        buttonUpdateAll.Bind(IsEnabledProperty, new Binding(nameof(vm.CanUpdateAll)));

        var actions = EditorChrome.MakeActionColumn(new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Width = 200,
            Children = { buttonGetPlugins, buttonUpdateAll, buttonOpenFolder, buttonRemove },
        });

        // Where plugins run when only some lines are selected - the "Do not ask again" choice
        // from the apply-plugin prompt lands here so it can be changed back (#14844).
        var comboBoxApplyTo = UiUtil.MakeComboBox(vm.ApplyToLinesOptions, vm, nameof(vm.SelectedApplyToLines))
            .WithMinWidth(220);
        comboBoxApplyTo.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboBoxApplyTo, Se.Language.Plugins.ApplyPluginsTo);
        var panelApplyTo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { EditorChrome.MakeDimLabel(Se.Language.Plugins.ApplyPluginsTo, new Thickness(0)), comboBoxApplyTo },
        };

        var countBadge = EditorChrome.MakeCountBadge(new Binding(nameof(vm.Plugins) + ".Count") { Mode = BindingMode.OneWay }, EditorChrome.StrongTextBrush, EditorChrome.AccentSoftBrush);

        var listSection = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };
        listSection.Add(EditorChrome.MakeHeaderRow(EditorChrome.MakeSectionHeader(Se.Language.General.Installed), countBadge), 0);
        listSection.Add(EditorChrome.MakeCompactListFrame(listBox, compact: false), 1);
        listSection.Add(panelApplyTo, 2);

        var buttonClose = UiUtil.MakeButton(Se.Language.General.Close, vm.CloseCommand);
        buttonClose.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(EditorChrome.MakeSidePanelRoot(listSection, actions), EditorChrome.MakeDockedButtonBar(null, buttonClose));

        UiUtil.FocusOnFirstActivation(this, buttonClose);
        KeyDown += (_, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing();
    }
}
