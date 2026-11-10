using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The light alternating row color default went from #F5F5F5 (too faint on white) to
/// #E6E6E6 (#16779). Saved old defaults move along once; a user's own color stays.
/// </summary>
public class GridAlternatingRowColorMigrationTests
{
    [Fact]
    public void New_Default_Is_Visible_On_White()
    {
        Assert.Equal("#FFE6E6E6", new SeAppearance().GridAlternatingRowColor);
    }

    [Fact]
    public void Saved_Old_Default_Is_Migrated()
    {
        var appearance = new SeAppearance
        {
            GridAlternatingRowColor = Color.FromRgb(245, 245, 245).FromColorToHex(),
            GridAlternatingRowColorMigrationVersion = null,
        };

        Se.MigrateGridAlternatingRowColor(appearance);

        Assert.Equal("#FFE6E6E6", appearance.GridAlternatingRowColor);
        Assert.Equal(SeAppearance.CurrentGridAlternatingRowColorMigrationVersion, appearance.GridAlternatingRowColorMigrationVersion);
    }

    [Fact]
    public void Custom_Color_Is_Kept()
    {
        var appearance = new SeAppearance
        {
            GridAlternatingRowColor = "#FFDDEEFF",
            GridAlternatingRowColorMigrationVersion = null,
        };

        Se.MigrateGridAlternatingRowColor(appearance);

        Assert.Equal("#FFDDEEFF", appearance.GridAlternatingRowColor);
    }

    [Fact]
    public void Old_Default_Picked_After_Migration_Is_Kept()
    {
        var appearance = new SeAppearance
        {
            GridAlternatingRowColor = "#FFF5F5F5",
            GridAlternatingRowColorMigrationVersion = SeAppearance.CurrentGridAlternatingRowColorMigrationVersion,
        };

        Se.MigrateGridAlternatingRowColor(appearance);

        Assert.Equal("#FFF5F5F5", appearance.GridAlternatingRowColor);
    }
}
