using System.Text.Json;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes.Profile;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.BeautifyTimeCodes;

/// <summary>
/// Named Beautify time codes profiles saved by the user (#11541).
/// </summary>
public class BeautifyTimeCodesCustomProfileTests
{
    [Fact]
    public void CustomProfiles_RoundTripThroughSettingsJson_AndActiveProfileKeysAreUnchanged()
    {
        var se = new Se();
        se.BeautifyTimeCodes.Gap = 3;
        var custom = new SeBeautifyTimeCodesCustomProfile { Name = "Broadcast", Gap = 4, InCuesLeftGreenZone = 12, ChainingGeneralShotChangeBehavior = 2 };
        se.BeautifyTimeCodes.CustomProfiles.Add(custom);

        var json = JsonSerializer.Serialize(se, SeJsonContext.Default.Se);
        var beautify = JsonDocument.Parse(json).RootElement.GetProperty("BeautifyTimeCodes");
        Assert.Equal(3, beautify.GetProperty("Gap").GetInt32()); // the active profile's keys did not move

        var loaded = JsonSerializer.Deserialize(json, SeJsonContext.Default.Se)!;
        var loadedCustom = Assert.Single(loaded.BeautifyTimeCodes.CustomProfiles);
        Assert.Equal("Broadcast", loadedCustom.Name);
        Assert.Equal(4, loadedCustom.Gap);
        Assert.Equal(12, loadedCustom.InCuesLeftGreenZone);
        Assert.Equal(2, loadedCustom.ChainingGeneralShotChangeBehavior);
    }

    [Fact]
    public void OldSettingsJsonWithoutCustomProfiles_LoadsWithAnEmptyList()
    {
        var se = JsonSerializer.Deserialize("""{"BeautifyTimeCodes":{"Saved":true,"Gap":3}}""", SeJsonContext.Default.Se)!;

        Assert.Equal(3, se.BeautifyTimeCodes.Gap);
        Assert.NotNull(se.BeautifyTimeCodes.CustomProfiles);
        Assert.Empty(se.BeautifyTimeCodes.CustomProfiles);
    }

    [Fact]
    public void LoadCustomProfile_FillsTheEditor()
    {
        var netflix = new BeautifyTimeCodesSettings.BeautifyTimeCodesProfile(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.Netflix);
        var custom = new SeBeautifyTimeCodesCustomProfile { Name = "Netflix copy" };
        custom.CopyProfileFrom(netflix);
        custom.Gap = netflix.Gap + 1;

        var vm = new BeautifyTimeCodesProfileViewModel(new StubWindowService());
        vm.LoadCustomProfile(custom);

        Assert.Equal(netflix.Gap + 1, vm.Gap);
        Assert.Equal(netflix.InCuesLeftGreenZone, vm.InCuesLeftGreenZone);
        Assert.Equal(netflix.OutCuesRightRedZone, vm.OutCuesRightRedZone);
        Assert.Equal(netflix.ConnectedSubtitlesTreatConnected, vm.ConnectedTreatConnectedMs);
        Assert.Equal((int)netflix.ChainingGeneralShotChangeBehavior, vm.SelectedChainingShotChangeBehaviorIndex);
    }
}
