using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr;

/// <summary>
/// A Tesseract OCR engine mode, passed to tesseract as the --oem value (0-3).
/// </summary>
public class TesseractEngineModeItem
{
    public string Name { get; set; }

    /// <summary>
    /// The tesseract --oem value: 0 = legacy only, 1 = LSTM only, 2 = legacy + LSTM, 3 = default.
    /// </summary>
    public int Oem { get; set; }

    public TesseractEngineModeItem(string name, int oem)
    {
        Name = name;
        Oem = oem;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<TesseractEngineModeItem> List()
    {
        var language = Se.Language.Ocr;
        return new List<TesseractEngineModeItem>
        {
            new(language.TesseractEngineModeLegacy, 0),
            new(language.TesseractEngineModeNeural, 1),
            new(language.TesseractEngineModeBoth, 2),
            new(language.TesseractEngineModeDefault, 3),
        };
    }
}
