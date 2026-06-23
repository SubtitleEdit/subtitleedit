using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageSpellCheck
{
    public string SpellCheck { get; set; }
    public string GetDictionariesTitle { get; set; }
    public string GetDictionaryInstructions { get; set; }
    public string AddNameToUserDictionary { get; set; }
    public string AddNameToNamesList { get; set; }
    public string AddNamesToNamesList { get; set; }
    public string ImportXNames { get; set; }
    public string SomeNamesAreNotProperCase { get; set; }
    public string EnterOneNamePerLine { get; set; }
    public string NoNamesAdded { get; set; }
    public string XOfYNamesImported { get; set; }
    public string NoDictionariesFound { get; set; }
    public string WordNotFound { get; set; }
    public string LineXofY { get; set; }
    public string ChangeWordFromXToY { get; set; }
    public string ChangeAllWordsFromXToY { get; set; }
    public string IgnoreWordXOnce { get; set; }
    public string IgnoreWordXAlways { get; set; }
    public string WordXAddedToNamesList { get; set; }
    public string WordXAddedToUserDictionary { get; set; }
    public string UseSuggestionX { get; set; }
    public string UseSuggestionXAlways { get; set; }
    public string AddXToUserDictionary { get; set; }
    public string IgnoreAllX { get; set; }
    public string PickSpellCheckDictionaryDotDotDot { get; set; }
    public string ChooseSpellCheckDictionary { get; set; }
    public string PickLiveSpellCheckDictionary { get; set; }
    public string UndoX { get; set; }
    public string EditWholeText { get; set; }

    public LanguageSpellCheck()
    {
        SpellCheck = "Spell check";
        GetDictionariesTitle = "Spell check - get dictionaries";
        GetDictionaryInstructions = "Choose your language and click download";
        AddNameToUserDictionary = "Add name to user dictionary";
        AddNameToNamesList = "Add name to names list";
        AddNamesToNamesList = "Add names to names list";
        ImportXNames = "Import {0} names?";
        SomeNamesAreNotProperCase = "Some names do not start with an uppercase letter or are all uppercase. Import anyway?";
        EnterOneNamePerLine = "Enter one name per line";
        NoNamesAdded = "No names were added (perhaps they already exist).";
        XOfYNamesImported = "{0} of {1} names imported (the rest may already exist).";
        NoDictionariesFound = "No dictionaries found";
        WordNotFound = "Word not found";
        LineXofY = "Spell checker - line {0} of {1}";
        ChangeWordFromXToY = "Change word from '{0}' to '{1}'";
        ChangeAllWordsFromXToY = "Change all words from '{0}' to '{1}'";
        IgnoreWordXOnce = "Ignore word '{0}' once";
        IgnoreWordXAlways = "Ignore word '{0}' always";
        WordXAddedToNamesList = "Word '{0}' added to names list";
        WordXAddedToUserDictionary = "Word '{0}' added to user dictionary";
        UseSuggestionX = "Use suggestion '{0}'";
        UseSuggestionXAlways = "Use suggestion '{0}' always";
        AddXToUserDictionary = "Add '{0}' to user dictionary";
        IgnoreAllX = "Ignore all '{0}'";
        PickSpellCheckDictionaryDotDotDot = "Choose spell check dictionary...";
        ChooseSpellCheckDictionary = "Choose spell check dictionary";
        PickLiveSpellCheckDictionary = "Pick live spell check dictionary";
        UndoX = "Undo: {0}";
        EditWholeText = "Edit whole text";
    }
}