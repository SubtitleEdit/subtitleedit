using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Assa;

public class AssaAttachmentItem
{
    public string FileName { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public byte[] Bytes { get; set; }

    public string Size { get; set; }

    public AssaAttachmentItem()
    {
        FileName = string.Empty;
        Category = string.Empty;
        Content = string.Empty;
        Bytes = Array.Empty<byte>();
        Size = string.Empty;
    }

    public AssaAttachmentItem(string fileName)
    {
        FileName = fileName;
        if (fileName.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))
        {
            Category = Se.Language.General.Fonts;
        }
        else
        {
            Category = Se.Language.Assa.Graphics;
        }

        Bytes = FileUtil.ReadAllBytesShared(fileName);
        Content = UUEncoding.UUEncode(Bytes); 
        Size = Utilities.FormatBytesToDisplayFileSize(Bytes.Length);
    }
}
