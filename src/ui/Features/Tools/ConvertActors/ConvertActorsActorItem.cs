namespace Nikse.SubtitleEdit.Features.Tools.ConvertActors;

/// <summary>
/// One speaker found in the preview, with the number of preview rows that carry it - the items
/// of the actor picker that ticks or unticks all of that speaker's lines.
/// </summary>
public class ConvertActorsActorItem
{
    public string Name { get; }
    public int Count { get; }

    public ConvertActorsActorItem(string name, int count)
    {
        Name = name;
        Count = count;
    }

    public override string ToString() => $"{Name} ({Count})";
}
