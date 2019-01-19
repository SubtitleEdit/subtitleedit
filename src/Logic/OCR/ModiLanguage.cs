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
                var list = new List<ModiLanguage>
                {
                    new ModiLanguage { Id = DefaultLanguageId, Text = "Default" },
                    new ModiLanguage { Id = 2052, Text = "Chinese simplified" },
                    new ModiLanguage { Id = 1028, Text = "Chinese traditional" },
                    new ModiLanguage { Id = 5, Text = "Chech" },
                    new ModiLanguage { Id = 6, Text = "Danish" },
                    new ModiLanguage { Id = 19, Text = "Dutch" },
                    new ModiLanguage { Id = 9, Text = "English" },
                    new ModiLanguage { Id = 11, Text = "Finnish" },
                    new ModiLanguage { Id = 12, Text = "French" },
                    new ModiLanguage { Id = 7, Text = "German" },
                    new ModiLanguage { Id = 8, Text = "Greek" },
                    new ModiLanguage { Id = 14, Text = "Hungarian" },
                    new ModiLanguage { Id = 16, Text = "Italian" },
                    new ModiLanguage { Id = 17, Text = "Japanese" },
                    new ModiLanguage { Id = 18, Text = "Korean" },
                    new ModiLanguage { Id = 20, Text = "Norweigian" },
                    new ModiLanguage { Id = 21, Text = "Polish" },
                    new ModiLanguage { Id = 22, Text = "Portuguese" },
                    new ModiLanguage { Id = 25, Text = "Russian" },
                    new ModiLanguage { Id = 10, Text = "Spanish" },
                    new ModiLanguage { Id = 29, Text = "Swedish" },
                    new ModiLanguage { Id = 31, Text = "Turkish" }
                };
                return list;
            }
        }
    }
}
