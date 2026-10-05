using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveUnicodeControlCharacters
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.BatchConvert.RemoveUnicodeControlCharactersTitle,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelInfo = new TextBlock
        {
            Text = Se.Language.Tools.BatchConvert.RemoveUnicodeControlCharactersInfo,
            Opacity = 0.7,
            FontStyle = FontStyle.Italic,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 500,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Avalonia.Thickness(10),
            Spacing = 10,
            Children =
            {
                labelHeader,
                labelInfo,
            }
        };
    }
}
