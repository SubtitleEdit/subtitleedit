using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsProfileWindow : Window
{
    private readonly FixCommonErrorsProfileViewModel _vm;

    public FixCommonErrorsProfileWindow(FixCommonErrorsProfileViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.FixCommonErrors.FixCommonErrorsProfiles;
        Width = 680;
        Height = 420;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // Left: the profile list, "New profile" in its header
        var listBoxProfiles = new ListBox
        {
            DataContext = vm,
            ItemsSource = vm.Profiles,
            Background = Brushes.Transparent,
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedProfile)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<ProfileDisplayItem>((profile, _) =>
            {
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    Margin = new Thickness(0),
                };

                // Name text (left-aligned)
                var nameText = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    FontSize = 12.5,
                    [!TextBlock.TextProperty] = new Binding(nameof(ProfileDisplayItem.Name))
                };
                Grid.SetColumn(nameText, 0);
                grid.Children.Add(nameText);

                // Delete button (right-aligned)
                var deleteButton = new Button
                {
                    Command = vm.DeleteCommand,
                    CommandParameter = profile,
                    DataContext = vm,
                    Width = 24,
                    Height = 24,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };
                deleteButton.Classes.Add("tool");
                deleteButton.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsProfileDeleteEnabled)) { Mode = BindingMode.OneWay });
                Attached.SetIcon(deleteButton, "fa-solid fa-trash");
                AutomationProperties.SetName(deleteButton, Se.Language.General.Delete);
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(deleteButton, Se.Language.General.Delete);
                }

                Grid.SetColumn(deleteButton, 1);
                grid.Children.Add(deleteButton);

                return grid;
            })
        };
        AutomationProperties.SetName(listBoxProfiles, Se.Language.General.Profiles);

        var buttonNewProfile = EditorChrome.MakeTextToolButton("fa-solid fa-plus", Se.Language.General.NewProfile, vm.NewProfileCommand);
        var profilesHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(buttonNewProfile, Dock.Right);
        profilesHeader.Children.Add(buttonNewProfile);
        profilesHeader.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Profiles));

        var profilesPanel = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = new Thickness(14, 8, 14, 14),
        };
        profilesPanel.Add(profilesHeader, 0);
        profilesPanel.Add(EditorChrome.MakeListFrame(listBoxProfiles, clearListBackground: true), 1);

        // Right: the selected profile's name
        var nameBox = new TextBox
        {
            PlaceholderText = Se.Language.General.EnterProfileName,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        nameBox.Classes.Add("text");
        nameBox.Bind(TextBox.TextProperty, new Binding($"{nameof(vm.SelectedProfile)}.{nameof(ProfileDisplayItem.Name)}"));
        nameBox.KeyDown += vm.ProfileNameTextBoxOnKeyDown;
        AutomationProperties.SetName(nameBox, Se.Language.General.ProfileName);
        vm.FocusProfileName = () => Avalonia.Threading.Dispatcher.UIThread.Post(() => nameBox.Focus());

        var editorSection = EditorChrome.MakeEditorSection(Se.Language.General.ProfileName, nameBox, first: true);

        // Hide if no profile is selected
        editorSection.Bind(IsVisibleProperty, new Binding(nameof(vm.IsProfileSelected)));

        var columns = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
        };
        columns.Add(profilesPanel, 0, 0);
        columns.Add(new Border
        {
            Child = editorSection,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonBar = EditorChrome.MakeButtonBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), buttonOk);

        Content = EditorChrome.MakeRoot(columns, buttonBar);

        UiUtil.FocusOnFirstActivation(this, () => { Focus(); }); // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
