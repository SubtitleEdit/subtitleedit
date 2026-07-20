using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// SE4-style context menu for regex-enabled find/replace text boxes,
/// inserting common regex snippets at the caret.
/// </summary>
public static class RegexContextFlyout
{
    public static void Attach(TextBox textBox, INotifyPropertyChanged vm, Func<bool> isRegexMode, bool isReplaceBox = false)
    {
        var flyout = isReplaceBox ? MakeReplaceFlyout(textBox) : MakeFindFlyout(textBox);

        void Update()
        {
            if (isRegexMode())
            {
                textBox.ContextFlyout = flyout;
            }
            else
            {
                // restore the theme's default cut/copy/paste menu
                textBox.ClearValue(Control.ContextFlyoutProperty);
            }
        }

        vm.PropertyChanged += (_, _) => Update();
        Update();

        UiUtil.AttachMacContextFlyoutHandler(textBox);
    }

    private static MenuFlyout MakeFindFlyout(TextBox textBox)
    {
        var l = Se.Language.Edit.RegularExpressionContextMenu;
        var flyout = new MenuFlyout();
        AddItem(flyout, textBox, l.WordBoundary, "\\b");
        AddItem(flyout, textBox, l.NonWordBoundary, "\\B");
        AddItem(flyout, textBox, l.NewLine, "\\r\\n");
        AddItem(flyout, textBox, l.AnyDigit, "\\d");
        AddItem(flyout, textBox, l.NonDigit, "\\D");
        AddItem(flyout, textBox, l.AnyCharacter, ".");
        AddItem(flyout, textBox, l.AnyWhitespace, "\\s");
        AddItem(flyout, textBox, l.NonSpaceCharacter, "\\S");
        AddItem(flyout, textBox, l.ZeroOrMore, "*");
        AddItem(flyout, textBox, l.OneOrMore, "+");
        AddItem(flyout, textBox, l.InCharacterGroup, "[test]");
        AddItem(flyout, textBox, l.NotInCharacterGroup, "[^test]");
        return flyout;
    }

    private static MenuFlyout MakeReplaceFlyout(TextBox textBox)
    {
        var l = Se.Language.Edit.RegularExpressionContextMenu;
        var flyout = new MenuFlyout();
        AddItem(flyout, textBox, l.NewLineShort, "\\n");
        return flyout;
    }

    private static void AddItem(MenuFlyout flyout, TextBox textBox, string header, string insertText)
    {
        var item = new MenuItem
        {
            // TextBlock header: a plain string header would eat '_' as an access-key marker.
            Header = new TextBlock { Text = header },
        };

        item.Click += (_, _) =>
        {
            textBox.SelectedText = insertText;
            textBox.Focus();
        };

        flyout.Items.Add(item);
    }
}
