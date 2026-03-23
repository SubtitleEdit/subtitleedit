namespace Nikse.SubtitleEdit.Core.Grammar
{
    /// <summary>
    /// http://wiki.languagetool.org/development-overview
    /// https://raw.githubusercontent.com/languagetool-org/languagetool/master/languagetool-language-modules/en/src/main/resources/org/languagetool/rules/en/grammar.xml
    /// </summary>
    public class LanguageToolsRule 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }
        public string[] Pattern { get; set; }
        public string Url { get; set; }
        public string Message { get; set; }
        public string ExampleCorrect { get; set; }
    }
}