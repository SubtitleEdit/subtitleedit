using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ImageBasedProfileWindow : Window
{
    public ImageBasedProfileWindow(ImageBasedProfileViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.ExportImagesProfiles;
        Width = 800;
        Height = 440;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = UiUtil.MakeWindowMargin(),
        };

        // Left column: Profile list
        var profileListPanel = new StackPanel
        {
            Margin = new Thickness(10),
            Spacing = 10,
            Children =
            {
                new Button
                {
                    Content = Se.Language.General.NewProfile,
                    Command = vm.NewProfileCommand
                },
                new ListBox
                {
                    DataContext = vm,
                    ItemsSource = vm.Profiles,
                    [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedProfile)) { Mode = BindingMode.TwoWay },
                    ItemTemplate = new FuncDataTemplate<ProfileDisplayItem>((profile, _) =>
                    {
                       var grid = new Grid
                        {
                            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                            Margin = new Thickness(2)
                        };

                        // Name text (left-aligned)
                        var nameText = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
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
                        };
                        deleteButton.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsProfileDeleteEnabled)) { Mode = BindingMode.TwoWay });
                        Attached.SetIcon(deleteButton, "fa-solid fa-trash");
                        Grid.SetColumn(deleteButton, 1);
                        grid.Children.Add(deleteButton);

                        return grid;
                    })
                }
            }
        };
        grid.Children.Add(profileListPanel);
        Grid.SetRow(profileListPanel, 1);
        Grid.SetColumn(profileListPanel, 0);

        // Right column: Profile editor
        var editorGrid = new Grid
        {
            Margin = new Thickness(10),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,*,Auto"),
        };

        // "Edit Profile" heading
        var heading = new TextBlock
        {
            Text = Se.Language.General.ProfileName,
            FontSize = 20,
            Padding = new Thickness(0, 25, 0, 0),
        };
        editorGrid.Children.Add(heading);
        Grid.SetRow(heading, 0);

        // Name textbox
        var nameBox = new TextBox
        {
            Watermark = Se.Language.General.EnterProfileName,
        };
        nameBox.Bind(TextBox.TextProperty, new Binding($"{nameof(vm.SelectedProfile)}.{nameof(ProfileDisplayItem.Name)}"));
        nameBox.KeyDown += vm.ProfileNameTextBoxOnKeyDown;
        editorGrid.Children.Add(nameBox);
        Grid.SetRow(nameBox, 1);
        vm.ProfileNameTextBox = nameBox;

        var buttonRow = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );
        Grid.SetRow(buttonRow, 4);
        editorGrid.Children.Add(buttonRow);

        // Hide if no profile is selected
        editorGrid.Bind(IsVisibleProperty, new Binding(nameof(vm.IsProfileSelected)));

        Grid.SetRow(editorGrid, 1);
        Grid.SetColumn(editorGrid, 1);
        grid.Children.Add(editorGrid);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}