using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.DCinemaSmpteProperties;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files;

public class DCinemaSmptePropertiesMxfTests
{
    private static DCinemaSmptePropertiesViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<DCinemaSmptePropertiesViewModel>();
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 2000));
        return subtitle;
    }

    /// <summary>
    /// The MXF track file embeds the font under the LoadFont urn:uuid, so OK stores the chosen font
    /// file and turns a font URI that is not a urn:uuid (an Interop-style file name) into one.
    /// </summary>
    [AvaloniaFact]
    public void MxfExport_Ok_StoresFontFileAndMakesFontUriAUuid()
    {
        Se.Settings.File.DCinemaSmpte = new SeDCinemaSmpte { CurrentDCinemaFontUri = "Arial.ttf" };
        var fontFile = Path.GetTempFileName();
        var vm = MakeViewModel();
        vm.InitializeMxfExport(MakeSubtitle(), TimeSpan.FromSeconds(12));
        var window = new DCinemaSmptePropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.True(vm.IsMxfExport);
            Assert.Contains("MXF", vm.WindowTitle);
            Assert.Equal("00:00:12:00", vm.ReelDuration); // the video's length at the default 24 fps

            vm.MxfFontFile = fontFile;
            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            var ss = Se.Settings.File.DCinemaSmpte;
            Assert.Equal(fontFile, ss.MxfFontFile);
            Assert.StartsWith("urn:uuid:", ss.CurrentDCinemaFontUri);
            Assert.True(Guid.TryParse(ss.CurrentDCinemaFontUri.Substring(9), out _));
        }
        finally
        {
            if (!vm.OkPressed)
            {
                window.Close();
            }

            File.Delete(fontFile);
        }
    }

    [AvaloniaFact]
    public void Properties_Ok_DoesNotNeedFontFile()
    {
        Se.Settings.File.DCinemaSmpte = new SeDCinemaSmpte();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), new DCinemaSmpte2014());
        var window = new DCinemaSmptePropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.False(vm.IsMxfExport);

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.Equal(string.Empty, Se.Settings.File.DCinemaSmpte.MxfFontFile);
        }
        finally
        {
            if (!vm.OkPressed)
            {
                window.Close();
            }
        }
    }
    /// <summary>
    /// #16865: Edit rate is picked from the D-Cinema rates. A non-standard value from old settings
    /// is shown as it is (not silently replaced) and OK refuses it.
    /// </summary>
    [AvaloniaFact]
    public void Properties_NonStandardEditRate_IsListedAndOkRefusesIt()
    {
        Se.Settings.File.DCinemaSmpte = new SeDCinemaSmpte { CurrentDCinemaEditRate = "24000 1001", CurrentDCinemaTimeCodeRate = "23" };
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), new DCinemaSmpte2014());
        var window = new DCinemaSmptePropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("24000 1001", vm.EditRate);
            Assert.Contains("24000 1001", vm.EditRates);
            Assert.Equal("23", vm.SelectedTimeCodeRate);
            Assert.Contains("50 1", vm.EditRates);
            Assert.Contains("60", vm.TimeCodeRates);

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.False(vm.OkPressed);
            Assert.Equal("24000 1001", Se.Settings.File.DCinemaSmpte.CurrentDCinemaEditRate);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Properties_StandardEditRate_OkStoresIt()
    {
        Se.Settings.File.DCinemaSmpte = new SeDCinemaSmpte { CurrentDCinemaEditRate = "25 1", CurrentDCinemaTimeCodeRate = "25" };
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), new DCinemaSmpte2014());
        var window = new DCinemaSmptePropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(6, vm.EditRates.Count);

            vm.EditRate = "48 1";
            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.Equal("48 1", Se.Settings.File.DCinemaSmpte.CurrentDCinemaEditRate);
        }
        finally
        {
            if (!vm.OkPressed)
            {
                window.Close();
            }
        }
    }
}
