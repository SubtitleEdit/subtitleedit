using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// No two default shortcuts may clash (#15326: the voice manager default reused Ctrl+Shift+V from
/// "fill selected lines with clipboard text"). Same rule as the Shortcuts dialog's duplicate check:
/// equal keys clash when both are in the same category or either is "General". A default without
/// a category counts as General.
/// </summary>
public class ShortcutsDefaultDuplicatesTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DefaultShortcutsHaveNoDuplicates(bool isMacOS)
    {
        // GetDefaultShortcuts only uses the vm parameter for nameof() - never dereferenced.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!, isMacOS)
            .Where(s => s.Keys.Count > 0)
            .Select(s => new
            {
                s.ActionName,
                Keys = string.Join("+", ShortcutManager.OrderKeys(s.Keys.Select(ShortcutManager.NormalizeKeyToken))),
                Category = Enum.TryParse<ShortcutCategory>(s.ControlName, out var category) ? category : ShortcutCategory.General,
            })
            .ToList();

        var duplicates = new List<string>();
        foreach (var group in defaults.GroupBy(s => s.Keys).Where(g => g.Count() > 1))
        {
            var items = group.ToList();
            for (var i = 0; i < items.Count; i++)
            {
                for (var j = i + 1; j < items.Count; j++)
                {
                    if (items[i].Category == items[j].Category ||
                        items[i].Category == ShortcutCategory.General ||
                        items[j].Category == ShortcutCategory.General)
                    {
                        duplicates.Add($"{group.Key}: {items[i].ActionName} ({items[i].Category}), {items[j].ActionName} ({items[j].Category})");
                    }
                }
            }
        }

        Assert.True(duplicates.Count == 0, "Duplicate default shortcuts:" + Environment.NewLine + string.Join(Environment.NewLine, duplicates));
    }
}
