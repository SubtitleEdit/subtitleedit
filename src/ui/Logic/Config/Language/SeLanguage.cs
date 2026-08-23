using System.Text.Encodings.Web;
using System.Text.Json;
using Nikse.SubtitleEdit.Logic.Config.Language.Assa;
using Nikse.SubtitleEdit.Logic.Config.Language.Edit;
using Nikse.SubtitleEdit.Logic.Config.Language.File;
using Nikse.SubtitleEdit.Logic.Config.Language.Main;
using Nikse.SubtitleEdit.Logic.Config.Language.Options;
using Nikse.SubtitleEdit.Logic.Config.Language.Sync;
using Nikse.SubtitleEdit.Logic.Config.Language.Tools;
using Nikse.SubtitleEdit.Logic.Config.Language.Translate;
using Nikse.SubtitleEdit.Logic.Config.Language.Waveform;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class SeLanguage
{
    public string Title { get; set; } = "Subtitle Edit";
    public string Version { get; set; } = Se.Version;
    public string TranslatedBy { get; set; } = string.Empty;
    public string CultureName { get; set; } = "en-US";

    public LanguageGeneral General { get; set; } = new();
    public LanguageMain Main { get; set; } = new();
    public LanguageFile File { get; set; } = new();
    public LanguageEdit Edit { get; set; } = new();
    public LanguageSourceView SourceView { get; set; } = new();
    public LanguageTools Tools { get; set; } = new();
    public LanguageSpellCheck SpellCheck { get; set; } = new();
    public LanguageVideo Video { get; set; } = new();
    public LanguageWaveform Waveform { get; set; } = new();
    public LanguageSync Sync { get; set; } = new();
    public LanguageTranslate Translate { get; set; } = new();
    public LanguageOptions Options { get; set; } = new();
    public LanguagePlugins Plugins { get; set; } = new();
    public LanguageHelp Help { get; set; } = new();
    public LanguageOcr Ocr { get; set; } = new();
    public LanguageAssa Assa { get; set; } = new();
    public LanguageAbout About { get; set; } = new();
    public LanguageErrorList ErrorList { get; set; } = new();

    /// <summary>
    /// Serializes a language to the exact text used for <c>English.json</c> - the base file every
    /// translation is generated from. Used both by the "Save language file" shortcut in the main
    /// window and by the test that checks the checked-in English.json is still in sync with the code.
    /// </summary>
    public static string ToJson(SeLanguage language)
    {
        var json = JsonSerializer.Serialize(language, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

            // English.json must not depend on who saved it: WriteIndented defaults to
            // Environment.NewLine, which gives CRLF on Windows and LF elsewhere, turning a re-save
            // into a whole-file diff with no real change in it.
            NewLine = "\n",
        });

        // Same reason, for the line breaks *inside* the strings: several language classes build their
        // text with Environment.NewLine, so the same class yields "\r\n" on Windows and "\n" elsewhere
        // and the file flip-flopped with whoever regenerated it last. Normalize to "\n" here rather
        // than chase every class - this is the one place the file is written. Only escaped CRLF in
        // string values matches; a literal backslash-r in the text is escaped as "\\r" and is left be.
        return json.Replace("\\r\\n", "\\n");
    }
}