﻿namespace Nikse.SubtitleEdit.Forms.Tts
{
    public class TtsLanguage
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public TtsLanguage(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
