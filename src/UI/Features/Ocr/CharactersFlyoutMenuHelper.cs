using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Ocr;

public static class CharactersFlyoutMenuHelper
{
    public static void MakeFlyoutLetters(MenuFlyout menuFlyout, IRelayCommand<string> clickCommand)
    {
        menuFlyout.Items.Clear();
        MakeFlyoutLetterItem(menuFlyout, "Catalan", "àÀéÉèÈíÍïÏóÓòÒúÚüÜçÇ", clickCommand);
        MakeFlyoutLetterItem(menuFlyout, "French", "àÀâÂæÆçÇéèÉÈêÊËëîÎïÏôÔœŒùÙûÛüÜÿŸ", clickCommand);
        MakeFlyoutLetterItem(menuFlyout, "German", "äÄöÖüÜß", clickCommand);
        MakeFlyoutLetterItem(menuFlyout, "Italian", "àÀèÈéÉìÌòÒùÙ", clickCommand);
        MakeFlyoutLetterItem(menuFlyout, "Nordic", "æÆøØåÅäÄöÖ", clickCommand);
        MakeFlyoutLetterItem(menuFlyout, "Polish", "ąĄćĆęĘłŁńŃóÓśŚźŹżŻ", clickCommand);
        MakeFlyoutLetterItem(menuFlyout, "Portuguese", "ãÃõÕáÁéÉíÍóÓúÚâÂêÊôÔàÀçÇ", clickCommand);
        MakeFlyoutLetterItem(menuFlyout, "Spanish", "áÁéÉíÍóÓúÚüÜñÑ¿¡", clickCommand);
        MakeFlyoutLetterItem(menuFlyout, string.Empty, "♪♫", clickCommand);
    }

    private static void MakeFlyoutLetterItem(MenuFlyout menuFlyout, string text, string letters, IRelayCommand<string> clickCommand)
    {
        if (string.IsNullOrEmpty(text))
        {
            foreach (var letter in letters)
            {
                var menuItem = new MenuItem
                {
                    Header = letter.ToString(),
                    Command = clickCommand,
                    CommandParameter = letter.ToString()
                };
                menuFlyout.Items.Add(menuItem);
            }

            return;
        }

        var parentMenuItem = new MenuItem
        {
            Header = text
        };
        menuFlyout.Items.Add(parentMenuItem);

        foreach (var letter in letters)
        {
            var childMenuItem = new MenuItem
            {
                Header = letter.ToString(),
                Command = clickCommand,
                CommandParameter = letter.ToString()
            };
            parentMenuItem.Items.Add(childMenuItem);
        }
    }
}
