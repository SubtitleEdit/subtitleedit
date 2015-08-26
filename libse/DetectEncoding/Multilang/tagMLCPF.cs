using System;

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
    [Flags]
    public enum MLCPF
    {
        // Not currently supported.
        MLDETECTF_MAILNEWS = 0x0001,

        // Not currently supported.
        MLDETECTF_BROWSER = 0x0002,

        // Detection result must be valid for conversion and text rendering.
        MLDETECTF_VALID = 0x0004,

        // Detection result must be valid for conversion.
        MLDETECTF_VALID_NLS = 0x0008,

        //Preserve preferred code page order.
        //This is meaningful only if you have set the puiPreferredCodePages parameter in IMultiLanguage3::DetectOutboundCodePage or IMultiLanguage3::DetectOutboundCodePageInIStream.
        MLDETECTF_PRESERVE_ORDER = 0x0010,

        // Only return one of the preferred code pages as the detection result.
        // This is meaningful only if you have set the puiPreferredCodePages parameter in IMultiLanguage3::DetectOutboundCodePage or IMultiLanguage3::DetectOutboundCodePageInIStream.
        MLDETECTF_PREFERRED_ONLY = 0x0020,

        // Filter out graphical symbols and punctuation.
        MLDETECTF_FILTER_SPECIALCHAR = 0x0040,

        // Return only Unicode codepages if the euro character is detected.
        MLDETECTF_EURO_UTF8 = 0x0080
    }

}
