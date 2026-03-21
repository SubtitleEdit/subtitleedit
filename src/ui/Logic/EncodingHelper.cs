using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

public static class EncodingHelper
{
    public static List<TextEncoding> GetEncodings()
    {
        var encodingList = new List<TextEncoding>();
        encodingList.Insert(TextEncoding.Utf8WithBomIndex, new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom));
        encodingList.Insert(TextEncoding.Utf8WithoutBomIndex, new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom));
        foreach (var encodingInfo in Encoding.GetEncodings())
        {
            var encoding = encodingInfo.GetEncoding();
            if (encoding.CodePage >= 874 && !encoding.IsEbcdic() && !encoding.CodePage.Equals(Encoding.UTF8.CodePage))
            {
                var item = new TextEncoding(encoding, null);
                encodingList.Add(item);
            }
        }

        return encodingList;
    }
}
