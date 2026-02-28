using Nikse.SubtitleEdit.Logic.Ocr;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrAddHistoryManager
{
    public List<NOcrAddHistoryItem> Items { get; set; }

    public NOcrAddHistoryManager()
    {
        Items = new List<NOcrAddHistoryItem>();
    }

    public void Add(NOcrChar nOcrChar, NikseBitmap2? letterBitmap, int lineIndex)
    {
        if (nOcrChar == null)
        {
            return;
        }

        var item = new NOcrAddHistoryItem(nOcrChar, letterBitmap, lineIndex);
        Items.Add(item);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public void ClearNotInOcrDb(NOcrDb nOcrDb)
    {
        if (nOcrDb == null)
        {
            return;
        }

        Items.RemoveAll(item => !nOcrDb.OcrCharactersCombined.Contains(item.NOcrChar));
    }


    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            return;
        }
        Items.RemoveAt(index);
    }

    public void Remove(NOcrChar nOcrChar)
    {
        if (nOcrChar == null)
        {
            return;
        }

        var item = Items.Find(i => i.NOcrChar.Equals(nOcrChar));
        if (item != null)
        {
            Items.Remove(item);
        }
    }
}
