using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class ModiLanguage
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public const int DefaultLanguageId = 2048;

        public static IEnumerable<ModiLanguage> AllLanguages
        {
            get
            {
                var list = new List<ModiLanguage>();
                list.Add(new ModiLanguage { Id = DefaultLanguageId, Text = "Default" });
                list.Add(new ModiLanguage { Id = 2052, Text = "Chinese simplified" });
                list.Add(new ModiLanguage { Id = 1028, Text = "Chinese traditional" });
                list.Add(new ModiLanguage { Id = 5, Text = "Chech" });
                list.Add(new ModiLanguage { Id = 6, Text = "Danish" });
                list.Add(new ModiLanguage { Id = 19, Text = "Dutch" });
                list.Add(new ModiLanguage { Id = 9, Text = "English" });
                list.Add(new ModiLanguage { Id = 11, Text = "Finnish" });
                list.Add(new ModiLanguage { Id = 12, Text = "French" });
                list.Add(new ModiLanguage { Id = 7, Text = "German" });
                list.Add(new ModiLanguage { Id = 8, Text = "Greek" });
                list.Add(new ModiLanguage { Id = 14, Text = "Hungarian" });
                list.Add(new ModiLanguage { Id = 16, Text = "Italian" });
                list.Add(new ModiLanguage { Id = 17, Text = "Japanese" });
                list.Add(new ModiLanguage { Id = 18, Text = "Korean" });
                list.Add(new ModiLanguage { Id = 20, Text = "Norweigian" });
                list.Add(new ModiLanguage { Id = 21, Text = "Polish" });
                list.Add(new ModiLanguage { Id = 22, Text = "Portuguese" });
                list.Add(new ModiLanguage { Id = 25, Text = "Russian" });
                list.Add(new ModiLanguage { Id = 10, Text = "Spanish" });
                list.Add(new ModiLanguage { Id = 29, Text = "Swedish" });
                list.Add(new ModiLanguage { Id = 31, Text = "Turkish" });
                return list;
            }
        }
    }
}
