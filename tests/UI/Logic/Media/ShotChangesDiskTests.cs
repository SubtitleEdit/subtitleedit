using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// Shot changes are cached per video as "{hash}_{track}_{name}.shotchanges" when a video has an
/// audio track selected. Reading used to look for "{hash}_{name}" first and otherwise take any
/// "{hash}*" match, so a saved (e.g. moved) list could be shadowed by an older file on reopen, and
/// emptying the list deleted only the track file - the older one brought the shot changes back.
/// </summary>
public class ShotChangesDiskTests : IDisposable
{
    private readonly string _videoFileName;

    public ShotChangesDiskTests()
    {
        // Unique content -> unique movie hash, so the test never touches another video's cache.
        _videoFileName = Path.Combine(Path.GetTempPath(), $"shotchg{Guid.NewGuid():N}.mkv");
        File.WriteAllBytes(_videoFileName, Guid.NewGuid().ToByteArray().Concat(new byte[70_000]).Concat(Guid.NewGuid().ToByteArray()).ToArray());
    }

    public void Dispose()
    {
        ShotChangesHelper.DeleteShotChanges(_videoFileName, -1);
        File.Delete(_videoFileName);
    }

    [Fact]
    public void SaveWithTrack_ThenFromDiskWithTrack_ReadsTheSavedList_EvenWithAnOlderTracklessFile()
    {
        ShotChangesHelper.SaveShotChanges(_videoFileName, new List<double> { 1, 2, 3 }, -1);
        ShotChangesHelper.SaveShotChanges(_videoFileName, new List<double> { 1.04, 2.04 }, 1);

        Assert.Equal(new List<double> { 1.04, 2.04 }, ShotChangesHelper.FromDisk(_videoFileName, 1));
        Assert.Equal(new List<double> { 1.04, 2.04 }, ShotChangesHelper.FromDisk(_videoFileName));
    }

    [Fact]
    public void SaveWithAnotherTrack_ReplacesTheOlderList()
    {
        ShotChangesHelper.SaveShotChanges(_videoFileName, new List<double> { 1, 2, 3 }, 0);
        ShotChangesHelper.SaveShotChanges(_videoFileName, new List<double> { 5 }, 2);

        Assert.Equal(new List<double> { 5 }, ShotChangesHelper.FromDisk(_videoFileName, 0));
        Assert.Equal(new List<double> { 5 }, ShotChangesHelper.FromDisk(_videoFileName, 2));
    }

    [Fact]
    public void Delete_RemovesEveryFileFromDiskCouldRead()
    {
        ShotChangesHelper.SaveShotChanges(_videoFileName, new List<double> { 1, 2, 3 }, -1);
        ShotChangesHelper.SaveShotChanges(_videoFileName, new List<double> { 4 }, 1);

        ShotChangesHelper.DeleteShotChanges(_videoFileName, 1);

        Assert.Empty(ShotChangesHelper.FromDisk(_videoFileName, 1));
        Assert.Empty(ShotChangesHelper.FromDisk(_videoFileName));
    }
}
