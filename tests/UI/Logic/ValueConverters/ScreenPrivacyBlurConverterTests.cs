using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Globalization;

namespace UITests.Logic.ValueConverters;

public class ScreenPrivacyBlurConverterTests
{
    private static object? Convert(object? item, object? selectedItem) =>
        ScreenPrivacyBlurConverter.Instance.Convert([item, selectedItem], typeof(IEffect), null, CultureInfo.InvariantCulture);

    [Fact]
    public void UnselectedRow_IsBlurred()
    {
        Assert.IsAssignableFrom<IBlurEffect>(Convert(new object(), new object()));
    }

    [Fact]
    public void NoSelection_IsBlurred()
    {
        Assert.IsAssignableFrom<IBlurEffect>(Convert(new object(), null));
    }

    [Fact]
    public void SelectedRow_IsSharp()
    {
        var item = new object();

        Assert.Null(Convert(item, item));
    }

    [Fact]
    public void RowWithoutItem_IsSharp()
    {
        Assert.Null(Convert(null, new object()));
    }
}
