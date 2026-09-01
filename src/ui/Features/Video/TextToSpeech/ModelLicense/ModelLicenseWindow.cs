using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;

/// <summary>
/// Shown once before an engine's first model download when the weights carry their own
/// licence. The "accept" button stays disabled until the checkbox is ticked, so acceptance is
/// a deliberate act rather than a reflex click on a default-focused OK. The content comes from
/// a <see cref="ModelLicenseDefinition"/>, which the caller hands to the view model's
/// Initialize before this constructor runs.
/// </summary>
public class ModelLicenseWindow : Window
{
    public ModelLicenseWindow(ModelLicenseViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Definition.DialogTitle;
        // Explicit width rather than SizeToContent.WidthAndHeight: on macOS the latter makes
        // text-heavy windows far too wide.
        Width = 640;
        SizeToContent = SizeToContent.Height;
        CanResize = false;

        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);
    }

    private static Border BuildContent(ModelLicenseViewModel vm)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                BuildHeader(vm),
                BuildSummary(vm),
                BuildLinks(vm),
                BuildAcceptRow(vm),
                BuildActions(vm),
            },
        };

        var outerGrid = new Grid { Margin = UiUtil.MakeWindowMargin() };
        outerGrid.Children.Add(stack);

        return new Border
        {
            Child = outerGrid,
            Padding = new Thickness(4),
        };
    }

    private static StackPanel BuildHeader(ModelLicenseViewModel vm)
    {
        var title = new TextBlock
        {
            Text = vm.Definition.Header,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        };

        var subtitle = new TextBlock
        {
            Text = vm.Definition.Intro,
            FontSize = 12,
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0),
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children = { title, subtitle },
        };
    }

    private static Border BuildSummary(ModelLicenseViewModel vm)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
        };

        foreach (var point in vm.Definition.SummaryPoints)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "•", Opacity = 0.7, VerticalAlignment = VerticalAlignment.Top },
                    new TextBlock { Text = point, TextWrapping = TextWrapping.Wrap, MaxWidth = 560 },
                },
            };
            stack.Children.Add(row);
        }

        return new Border
        {
            Child = stack,
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 128, 128, 128)),
        };
    }

    private static StackPanel BuildLinks(ModelLicenseViewModel vm)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children =
            {
                UiUtil.MakeLink(Se.Language.Video.IndexTts25LicenseReadFullText, vm.OpenLicenseCommand),
                UiUtil.MakeLink(Se.Language.Video.IndexTts25LicenseModelPage, vm.OpenModelPageCommand),
            },
        };
    }

    private static CheckBox BuildAcceptRow(ModelLicenseViewModel vm)
    {
        var checkBox = UiUtil.MakeCheckBox(
            vm.Definition.AcceptCheckBoxText,
            vm,
            nameof(vm.IsAccepted));
        return checkBox;
    }

    private static Control BuildActions(ModelLicenseViewModel vm)
    {
        var accept = UiUtil.MakeButton(Se.Language.Video.IndexTts25LicenseAcceptAndDownload, vm.AcceptCommand);
        // Bound to the checkbox so the download cannot start on an unread licence.
        accept.Bind(
            Button.IsEnabledProperty,
            new Avalonia.Data.Binding(nameof(vm.IsAccepted)));

        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return UiUtil.MakeButtonBar(accept, cancel);
    }
}
