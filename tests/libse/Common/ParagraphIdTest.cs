using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

/// <summary>
/// Paragraph ids come from a counter instead of Guid.NewGuid - they must still be unique, and must
/// never collide with the random (version 4) Guids the grid rows share the id space with.
/// </summary>
public class ParagraphIdTest
{
    [Fact]
    public void Ids_AreUnique()
    {
        var ids = new HashSet<Guid>();
        for (var i = 0; i < 100_000; i++)
        {
            Assert.True(ids.Add(new Paragraph().Id!.Value));
        }
    }

    [Fact]
    public void Ids_AreUnique_AcrossThreads()
    {
        var bags = new Guid[8][];
        Parallel.For(0, bags.Length, t =>
        {
            var local = new Guid[20_000];
            for (var i = 0; i < local.Length; i++)
            {
                local[i] = new Paragraph("x", 0, 1000).Id!.Value;
            }

            bags[t] = local;
        });

        var all = bags.SelectMany(b => b).ToList();
        Assert.Equal(all.Count, all.Distinct().Count());
    }

    [Fact]
    public void Ids_AreVersion8_SoTheyNeverEqualARandomGuid()
    {
        var id = new Paragraph().Id!.Value;
        Assert.Equal(8, id.Version);
        Assert.Equal(4, Guid.NewGuid().Version);

        // RFC 9562 variant: the top two bits of byte 8 are 10.
        Assert.Equal(0x80, id.ToByteArray()[8] & 0xC0);
    }

    [Fact]
    public void CopyConstructor_KeepsOrRenewsTheId()
    {
        var p = new Paragraph("x", 0, 1000);
        Assert.Equal(p.Id, new Paragraph(p, false).Id);
        Assert.NotEqual(p.Id, new Paragraph(p).Id);
    }
}
