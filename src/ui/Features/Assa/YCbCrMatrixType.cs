namespace Nikse.SubtitleEdit.Features.Assa;

public enum YCbCrMatrixType
{
    NotSet = 0,
    None = 1,
    Tv601 = 2,
    Tv709 = 3,
    Pc601 = 4,
    Pc709 = 5,
    TvFcc = 6,
    PcFcc = 7,
    Tv240M = 8,
    Pc240M = 9,

    /// <summary>A value from the file that libass does not recognize, kept as-is.</summary>
    Custom = 10,
}
