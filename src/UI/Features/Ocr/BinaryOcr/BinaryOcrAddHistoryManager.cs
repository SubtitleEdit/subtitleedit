using Nikse.SubtitleEdit.Logic.Ocr;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public class BinaryOcrAddHistoryManager
{
    public List<BinaryOcrAddHistoryItem> Items { get; set; }

    public BinaryOcrAddHistoryManager()
    {
        Items = new List<BinaryOcrAddHistoryItem>();
    }

    public void Add(BinaryOcrBitmap binaryOcrBitmap, NikseBitmap2? letterBitmap, int lineIndex)
    {
        if (binaryOcrBitmap == null)
        {
            return;
        }

        var item = new BinaryOcrAddHistoryItem(binaryOcrBitmap, letterBitmap, lineIndex);
        Items.Add(item);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public void ClearNotInOcrDb(BinaryOcrDb binaryOcrDb)
    {
        if (binaryOcrDb == null)
        {
            return;
        }

        Items.RemoveAll(item => binaryOcrDb.FindExactMatch(item.BinaryOcrBitmap) == -1 && 
                                binaryOcrDb.FindExactMatchExpanded(item.BinaryOcrBitmap) == -1);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            return;
        }
        Items.RemoveAt(index);
    }

    public void Remove(BinaryOcrBitmap binaryOcrBitmap)
    {
        if (binaryOcrBitmap == null)
        {
            return;
        }

        var item = Items.Find(i => i.BinaryOcrBitmap == binaryOcrBitmap);
        if (item != null)
        {
            Items.Remove(item);
        }
    }
}
