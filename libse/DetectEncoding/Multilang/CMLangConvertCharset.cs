using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
    [ComImport, Guid("D66D6F98-CDAA-11D0-B822-00C04FC9B31F"), CoClass(typeof(CMLangConvertCharsetClass))]
    public interface ICMLangConvertCharset : IMLangConvertCharset
    {
    }
}
