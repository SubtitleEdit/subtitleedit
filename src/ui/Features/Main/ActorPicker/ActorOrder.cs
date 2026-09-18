using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.ActorPicker;

/// <summary>
/// Keeps the actors of the current subtitle in a session-stable order, so the "Set actor 1-10"
/// shortcuts keep pointing at the same actor while new actors are added (#15018). The first
/// actors seen are sorted alphabetically; every later actor is appended at the end instead of
/// being sorted in, which used to shift all the shortcuts after it by one.
/// </summary>
public class ActorOrder
{
    private readonly List<string> _order = new();

    public void Reset()
    {
        _order.Clear();
    }

    /// <summary>
    /// Returns the actors in <paramref name="actorsInSubtitle"/> in stable order. Actors no
    /// longer present are skipped but keep their position, so an undo brings them back where
    /// they were.
    /// </summary>
    public List<string> GetOrdered(IEnumerable<string?> actorsInSubtitle)
    {
        var present = new HashSet<string>(StringComparer.Ordinal);
        foreach (var actor in actorsInSubtitle)
        {
            if (!string.IsNullOrEmpty(actor))
            {
                present.Add(actor);
            }
        }

        var known = new HashSet<string>(_order, StringComparer.Ordinal);
        _order.AddRange(present.Where(p => !known.Contains(p)).OrderBy(p => p));

        return _order.Where(present.Contains).ToList();
    }

    /// <summary>
    /// Applies a user-made order: the actors in <paramref name="actors"/> swap places among the
    /// positions they already hold, so actors not in the list stay where they are.
    /// </summary>
    public void SetOrder(IReadOnlyList<string> actors)
    {
        var wanted = actors.Where(_order.Contains).Distinct().ToList();
        var slots = wanted.Select(p => _order.IndexOf(p)).OrderBy(p => p).ToList();
        for (var i = 0; i < wanted.Count; i++)
        {
            _order[slots[i]] = wanted[i];
        }
    }

    /// <summary>
    /// Lets a renamed actor keep its position. When the new name already exists (a merge of two
    /// actors) the old entry is simply dropped.
    /// </summary>
    public void Rename(string oldName, string newName)
    {
        var index = _order.IndexOf(oldName);
        if (index < 0 || oldName == newName)
        {
            return;
        }

        if (_order.Contains(newName))
        {
            _order.RemoveAt(index);
        }
        else
        {
            _order[index] = newName;
        }
    }
}
