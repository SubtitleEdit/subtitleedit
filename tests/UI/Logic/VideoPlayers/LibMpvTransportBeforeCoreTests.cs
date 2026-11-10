using System.Reflection;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic.VideoPlayers;

/// <summary>
/// Same start-up gap as <see cref="LibMpvCloseFileTests"/>: the mpv handle exists before the core,
/// and a player that never rendered (undocked, hidden behind the theme art) has no core. Pause after
/// a deferred open, or Space before the first frame, used to send the command anyway and log mpv's
/// "core not initialized" (#16734). The native call is replaced with a recorder here.
/// </summary>
public class LibMpvTransportBeforeCoreTests
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

    public static TheoryData<string> TransportCommands => new()
    {
        nameof(LibMpvDynamicPlayer.Pause),
        nameof(LibMpvDynamicPlayer.PlayOrPause),
        nameof(LibMpvDynamicPlayer.Play),
        nameof(LibMpvDynamicPlayer.Stop),
        nameof(LibMpvDynamicPlayer.StepOneFrameForward),
        nameof(LibMpvDynamicPlayer.StepOneFrameBack),
        nameof(LibMpvDynamicPlayer.SetAudioTrack),
        nameof(LibMpvDynamicPlayer.SetAudioDelay),
    };

    private static void Invoke(LibMpvDynamicPlayer player, string command)
    {
        switch (command)
        {
            case nameof(LibMpvDynamicPlayer.Pause): player.Pause(); break;
            case nameof(LibMpvDynamicPlayer.PlayOrPause): player.PlayOrPause(); break;
            case nameof(LibMpvDynamicPlayer.Play): player.Play(); break;
            case nameof(LibMpvDynamicPlayer.Stop): player.Stop(); break;
            case nameof(LibMpvDynamicPlayer.StepOneFrameForward): player.StepOneFrameForward(); break;
            case nameof(LibMpvDynamicPlayer.StepOneFrameBack): player.StepOneFrameBack(); break;
            case nameof(LibMpvDynamicPlayer.SetAudioTrack): player.SetAudioTrack(2); break;
            case nameof(LibMpvDynamicPlayer.SetAudioDelay): player.SetAudioDelay(0.5); break;
            default: throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
    }

    [Theory]
    [MemberData(nameof(TransportCommands))]
    public void BeforeTheCoreIsUp_SendsNothingToMpv(string command)
    {
        var (player, fake) = MakePlayer(coreInitialized: false);

        Invoke(player, command);

        Assert.Equal(0, fake.Commands);
    }

    [Theory]
    [MemberData(nameof(TransportCommands))]
    public void WithTheCoreUp_SendsTheCommand(string command)
    {
        var (player, fake) = MakePlayer(coreInitialized: true);

        Invoke(player, command);

        Assert.True(fake.Commands > 0);
    }

    [Fact]
    public void ToggleAudioTrack_BeforeTheCoreIsUp_ReturnsNullWithoutCommands()
    {
        var (player, fake) = MakePlayer(coreInitialized: false);

        Assert.Null(player.ToggleAudioTrack());
        Assert.Equal(0, fake.Commands);
    }
}
