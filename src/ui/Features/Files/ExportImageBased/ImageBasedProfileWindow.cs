using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

/// <summary>
/// The export-to-images profiles, in the editor look (see <see cref="EditorChrome"/>): the
/// profile list with add/delete on the left, the selected profile's name on the right.
/// </summary>
public class ImageBasedProfileWindow : Window
{
    public ImageBasedProfileWindow(ImageBasedProfileViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.ExportImagesProfiles;
        Width = 640;
        Height = 420;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        // Left column: Profile list
        var listBox = new ListBox
        {
            DataContext = vm,
            ItemsSource = vm.Profiles,
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedProfile)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<ProfileDisplayItem>((profile, _) =>
            {
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    MinHeight = 26,
                };

                // Name text (left-aligned)
                var nameText = new TextBlock
                {
                    FontSize = EditorChrome.FieldFontSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(ProfileDisplayItem.Name))
                };
                Grid.SetColumn(nameText, 0);
                grid.Children.Add(nameText);

                // Delete button (right-aligned)
                var deleteButton = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Delete, vm.DeleteCommand, 26);
                deleteButton.CommandParameter = profile;
                deleteButton.DataContext = vm;
                deleteButton.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsProfileDeleteEnabled)) { Mode = BindingMode.OneWay });
                Grid.SetColumn(deleteButton, 1);
                grid.Children.Add(deleteButton);

                return grid;
            })
        };
        AutomationProperties.SetName(listBox, Se.Language.General.Profiles);

        var buttonNew = EditorChrome.MakeTextToolButton("fa-solid fa-plus", Se.Language.General.NewProfile, vm.NewProfileCommand);
        var profileList = MakeHeaderPanel(Se.Language.General.Profiles, EditorChrome.MakeListFrame(listBox, clearListBackground: true), buttonNew);
        profileList.Margin = new Thickness(14, 12, 14, 14);

        // Right column: Profile editor
        var nameBox = new TextBox
        {
            PlaceholderText = Se.Language.General.EnterProfileName,
        };
        nameBox.Classes.Add("text");
        nameBox.Bind(TextBox.TextProperty, new Binding($"{nameof(vm.SelectedProfile)}.{nameof(ProfileDisplayItem.Name)}"));
        nameBox.KeyDown += vm.ProfileNameTextBoxOnKeyDown;
        AutomationProperties.SetName(nameBox, Se.Language.General.ProfileName);
        vm.FocusProfileName = () => Avalonia.Threading.Dispatcher.UIThread.Post(() => nameBox.Focus());

        var editor = EditorChrome.MakeEditorSection(Se.Language.General.ProfileName, nameBox, first: true);
        editor.VerticalAlignment = VerticalAlignment.Top;

        // Hide if no profile is selected
        editor.Bind(IsVisibleProperty, new Binding(nameof(vm.IsProfileSelected)));

        var sections = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        sections.Add(profileList, 0, 0);
        sections.Add(new Border
        {
            Child = editor,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonBar = EditorChrome.MakeDockedButtonBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), buttonOk);
        Content = EditorChrome.MakeRoot(sections, buttonBar);

        UiUtil.FocusOnFirstActivation(this, () => { Focus(); }); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// A section header (with actions on the right) above a list that takes the rest of the height.
    /// </summary>
    private static DockPanel MakeHeaderPanel(string title, Control content, Control headerRight)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8), MinHeight = 28 };
        DockPanel.SetDock(headerRight, Dock.Right);
        header.Children.Add(headerRight);
        header.Children.Add(EditorChrome.MakeSectionHeader(title));
        var panel = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        panel.Children.Add(header);
        panel.Children.Add(content);
        return panel;
    }
}
