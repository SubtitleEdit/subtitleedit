using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class ShortcutManagerTests
{
    [Theory]
    [InlineData(Key.Home, PhysicalKey.NumPad7, Key.NumPad7)]
    [InlineData(Key.Left, PhysicalKey.NumPad4, Key.NumPad4)]
    [InlineData(Key.Right, PhysicalKey.NumPad6, Key.NumPad6)]
    [InlineData(Key.End, PhysicalKey.NumPad1, Key.NumPad1)]
    [InlineData(Key.Home, PhysicalKey.Home, Key.Home)]
    public void GetShortcutKeyUsesPhysicalKeyForNumpad(Key key, PhysicalKey physicalKey, Key expected)
    {
        var result = ShortcutManager.GetShortcutKey(key, physicalKey);

        Assert.Equal(expected, result);
    }
}