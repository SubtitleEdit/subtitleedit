using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

/// <summary>
/// Pick a video resolution from a list, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class BurnInResolutionPickerWindow : Window
{
    private readonly BurnInResolutionPickerViewModel _vm;

    public BurnInResolutionPickerWindow(BurnInResolutionPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.PickResolution;
        SizeToContent = SizeToContent.WidthAndHeight;
        MaxHeight = 900;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var listBoxResolutions = new ListBox
        {
            ItemsSource = vm.Resolutions,
            SelectedItem = vm.SelectedResolution,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = Avalonia.Media.Brushes.Transparent,
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedResolution)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            // Recycling-safe template: everything binds and nothing dereferences the build
            // parameter - a recycled container gets a different item as its DataContext, so a
            // structural branch on the first item (separator vs row) and a captured `item` in
            // the click handler would both act on the wrong resolution after scrolling.
            ItemTemplate = new FuncDataTemplate<ResolutionItem>((_, _) =>
            {
                var separator = new Separator
                {
                    Margin = new Thickness(0, 2, 0, 2),
                    IsHitTestVisible = false,
                };
                separator.Bind(Visual.IsVisibleProperty, new Binding(nameof(ResolutionItem.IsSeparator)));

                var border = new Border();
                border.Bind(Border.BackgroundProperty, new Binding(nameof(ResolutionItem.BackgroundColor)));
                border.Bind(Visual.IsVisibleProperty, new Binding(nameof(ResolutionItem.IsSeparator))
                {
                    Converter = BoolConverters.Not,
                });

                var textBlock = new TextBlock
                {
                    FontSize = EditorChrome.FieldFontSize,
                    Margin = new Thickness(10, 0),
                    // Use the theme-appropriate text color (white in dark mode, black in
                    // light mode). Previously the foreground was bound to
                    // ResolutionItem.TextColor, which was never assigned, so it fell back to
                    // a black/transparent color that was unreadable in dark mode.
                    Foreground = UiUtil.GetTextColor(),
                };
                textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(ResolutionItem.DisplayName)));

                border.Child = textBlock;

                border.PointerPressed += (s, e) =>
                {
                    if ((s as Border)?.DataContext is ResolutionItem current)
                    {
                        vm.ResolutionItemClicked(current);
                    }
                };

                return new Panel { Children = { separator, border } };
            }, true),
        };

        var style = new Style(x => x.OfType<ListBoxItem>());
        style.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(0, 4, 0, 4)));
        style.Setters.Add(new Setter(ListBoxItem.MarginProperty, new Thickness(0)));
        listBoxResolutions.Styles.Add(style);

        listBoxResolutions.SelectionChanged += (s, e) =>
        {
            if (listBoxResolutions.SelectedItem is ResolutionItem selected && selected.IsSeparator)
            {
                listBoxResolutions.SelectedItem = null;
            }
        };

        listBoxResolutions.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                if (listBoxResolutions.SelectedItem is ResolutionItem selected && !selected.IsSeparator)
                {
                    vm.ResolutionItemClicked(selected);
                    e.Handled = true; 
                }
            }
        };

        Avalonia.Automation.AutomationProperties.SetName(listBoxResolutions, Se.Language.General.PickResolution);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(new Border
        {
            Child = EditorChrome.MakeListFrame(listBoxResolutions),
            Background = EditorChrome.PanelBrush,
            Padding = new Thickness(14),
        }, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, listBoxResolutions); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}