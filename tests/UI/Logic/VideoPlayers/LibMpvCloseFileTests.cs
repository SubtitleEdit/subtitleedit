using System.Reflection;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic.VideoPlayers;

/// <summary>
/// The mpv handle exists from CanLoad on, but the core only starts with the first render pass or
/// native window - and a native window stays uncreated behind the theme art. Closing such a player
/// used to send "stop" to the core and log mpv's "core not initialized" on start-up (#16513).
/// The native call is replaced with a recorder here.
/// </summary>
public class LibMpvCloseFileTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

    private sealed class FakeMpv
    {
        public int Commands;

        public int Command(IntPtr handle, IntPtr utf8Strings)
        {
            Commands++;
            return 0;
        }
    }

    private static (LibMpvDynamicPlayer Player, FakeMpv Fake) MakePlayer(bool coreInitialized)
    {
        var player = new LibMpvDynamicPlayer();
        var fake = new FakeMpv();
        var type = typeof(LibMpvDynamicPlayer);
        type.GetField("_mpv", Flags)!.SetValue(player, new IntPtr(1));
        type.GetField("_coreInitialized", Flags)!.SetValue(player, coreInitialized);
        var commandField = type.GetField("_mpvCommand", Flags)!;
        commandField.SetValue(player, Delegate.CreateDelegate(commandField.FieldType, fake, nameof(FakeMpv.Command)));
        return (player, fake);
    }

    [Fact]
    public void CloseFile_BeforeTheCoreIsUp_SendsNothingToMpv()
    {
        var (player, fake) = MakePlayer(coreInitialized: false);

        player.CloseFile();

        Assert.Equal(0, fake.Commands);
    }

    [Fact]
    public void CloseFile_BeforeTheCoreIsUp_StillDiscardsAParkedLoad()
    {
        var (player, _) = MakePlayer(coreInitialized: false);
        var pendingField = typeof(LibMpvDynamicPlayer).GetField("_pendingLoadFileName", Flags)!;
        pendingField.SetValue(player, "parked.mkv");

        player.CloseFile();

        Assert.Null(pendingField.GetValue(player));
    }

    [Fact]
    public void CloseFile_WithTheCoreUp_StopsPlayback()
    {
        var (player, fake) = MakePlayer(coreInitialized: true);

        player.CloseFile();

        Assert.Equal(1, fake.Commands);
    }
}
