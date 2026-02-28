using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public static class ObservableCollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    public static SubtitleLineViewModel? GetOrNull(this ObservableCollection<SubtitleLineViewModel> collection, int index)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        return (index >= 0 && index < collection.Count) ? collection[index] : null;
    }

    public static SubtitleLineViewModel? GetOrNull(this List<SubtitleLineViewModel> collection, int index)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        return (index >= 0 && index < collection.Count) ? collection[index] : null;
    }

    public static void Renumber(this ObservableCollection<SubtitleLineViewModel> collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        for (int i = 0; i < collection.Count; i++)
        {
            var item = collection[i];
            if (item != null)
            {
                item.Number = i + 1;
            }
        }
    }

    public static string? AutoDetectGoogleLanguage(this ObservableCollection<SubtitleLineViewModel> collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        var subtitle = new Subtitle();
        foreach (var line in collection)
        {
            subtitle.Paragraphs.Add(new Paragraph(line.Text, line.StartTime.TotalMilliseconds, line.EndTime.TotalMilliseconds));
        }

        return LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
    }

}