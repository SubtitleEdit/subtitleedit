using System.IO;
using System.Text;
using System.Xml;

// !!! THIS FILE IS AUTO-GENERATED!!!
// !!! THIS FILE IS AUTO-GENERATED!!!
// !!! THIS FILE IS AUTO-GENERATED!!!

namespace Nikse.SubtitleEdit.Core
{

    public class LanguageDeserializer // NOTE: This class is AUTO-GENERATED!!!!
    {

        public static Language CustomDeserializeLanguage(string fileName)
        {
            var name = new StringBuilder(100, 1000);
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var language = new Language();

            using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings {
                   IgnoreWhitespace = true, IgnoreProcessingInstructions = true, IgnoreComments = true,
                   DtdProcessing = DtdProcessing.Ignore, CheckCharacters = false, CloseInput = true }))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (!reader.IsEmptyElement && reader.Depth > 0)
                        {
                            name.Append('/').Append(reader.Name);
                        }
                        else if (reader.Depth == 0)
                        {
                            language.Name = reader["Name"];
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (name.Length > 0)
                        {
                            name.Length -= reader.Name.Length + 1;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        SetValue(language, reader, name.ToString(1, name.Length - 1));
                    }
                }
            }
            return language;
        }

        private static void SetValue(Language language, XmlReader reader, string name)
        {
            switch (name)
            {
                case "Name":
                    language.Name = reader.Value;
                    break;
                case "General/Title":
                    language.General.Title = reader.Value;
                    break;
                case "General/Version":
                    language.General.Version = reader.Value;
                    break;
                case "General/TranslatedBy":
                    language.General.TranslatedBy = reader.Value;
                    break;
                case "General/CultureName":
                    language.General.CultureName = reader.Value;
                    break;
                case "General/HelpFile":
                    language.General.HelpFile = reader.Value;
                    break;
                case "General/Ok":
                    language.General.Ok = reader.Value;
                    break;
                case "General/Cancel":
                    language.General.Cancel = reader.Value;
                    break;
                case "General/Apply":
                    language.General.Apply = reader.Value;
                    break;
                case "General/None":
                    language.General.None = reader.Value;
                    break;
                case "General/All":
                    language.General.All = reader.Value;
                    break;
                case "General/Preview":
                    language.General.Preview = reader.Value;
                    break;
                case "General/SubtitleFiles":
                    language.General.SubtitleFiles = reader.Value;
                    break;
                case "General/AllFiles":
                    language.General.AllFiles = reader.Value;
                    break;
                case "General/VideoFiles":
                    language.General.VideoFiles = reader.Value;
                    break;
                case "General/AudioFiles":
                    language.General.AudioFiles = reader.Value;
                    break;
                case "General/OpenSubtitle":
                    language.General.OpenSubtitle = reader.Value;
                    break;
                case "General/OpenVideoFile":
                    language.General.OpenVideoFile = reader.Value;
                    break;
                case "General/OpenVideoFileTitle":
                    language.General.OpenVideoFileTitle = reader.Value;
                    break;
                case "General/NoVideoLoaded":
                    language.General.NoVideoLoaded = reader.Value;
                    break;
                case "General/VideoInformation":
                    language.General.VideoInformation = reader.Value;
                    break;
                case "General/StartTime":
                    language.General.StartTime = reader.Value;
                    break;
                case "General/EndTime":
                    language.General.EndTime = reader.Value;
                    break;
                case "General/Duration":
                    language.General.Duration = reader.Value;
                    break;
                case "General/CharsPerSec":
                    language.General.CharsPerSec = reader.Value;
                    break;
                case "General/WordsPerMin":
                    language.General.WordsPerMin = reader.Value;
                    break;
                case "General/Actor":
                    language.General.Actor = reader.Value;
                    break;
                case "General/Gap":
                    language.General.Gap = reader.Value;
                    break;
                case "General/Region":
                    language.General.Region = reader.Value;
                    break;
                case "General/NumberSymbol":
                    language.General.NumberSymbol = reader.Value;
                    break;
                case "General/Number":
                    language.General.Number = reader.Value;
                    break;
                case "General/Text":
                    language.General.Text = reader.Value;
                    break;
                case "General/HourMinutesSecondsMilliseconds":
                    language.General.HourMinutesSecondsMilliseconds = reader.Value;
                    break;
                case "General/HourMinutesSecondsFrames":
                    language.General.HourMinutesSecondsFrames = reader.Value;
                    break;
                case "General/Bold":
                    language.General.Bold = reader.Value;
                    break;
                case "General/Italic":
                    language.General.Italic = reader.Value;
                    break;
                case "General/Underline":
                    language.General.Underline = reader.Value;
                    break;
                case "General/Visible":
                    language.General.Visible = reader.Value;
                    break;
                case "General/FrameRate":
                    language.General.FrameRate = reader.Value;
                    break;
                case "General/Name":
                    language.General.Name = reader.Value;
                    break;
                case "General/FileNameXAndSize":
                    language.General.FileNameXAndSize = reader.Value;
                    break;
                case "General/ResolutionX":
                    language.General.ResolutionX = reader.Value;
                    break;
                case "General/FrameRateX":
                    language.General.FrameRateX = reader.Value;
                    break;
                case "General/TotalFramesX":
                    language.General.TotalFramesX = reader.Value;
                    break;
                case "General/VideoEncodingX":
                    language.General.VideoEncodingX = reader.Value;
                    break;
                case "General/SingleLineLengths":
                    language.General.SingleLineLengths = reader.Value;
                    break;
                case "General/TotalLengthX":
                    language.General.TotalLengthX = reader.Value;
                    break;
                case "General/TotalLengthXSplitLine":
                    language.General.TotalLengthXSplitLine = reader.Value;
                    break;
                case "General/SplitLine":
                    language.General.SplitLine = reader.Value;
                    break;
                case "General/NotAvailable":
                    language.General.NotAvailable = reader.Value;
                    break;
                case "General/OverlapPreviousLineX":
                    language.General.OverlapPreviousLineX = reader.Value;
                    break;
                case "General/OverlapX":
                    language.General.OverlapX = reader.Value;
                    break;
                case "General/OverlapNextX":
                    language.General.OverlapNextX = reader.Value;
                    break;
                case "General/Negative":
                    language.General.Negative = reader.Value;
                    break;
                case "General/RegularExpressionIsNotValid":
                    language.General.RegularExpressionIsNotValid = reader.Value;
                    break;
                case "General/CurrentSubtitle":
                    language.General.CurrentSubtitle = reader.Value;
                    break;
                case "General/OriginalText":
                    language.General.OriginalText = reader.Value;
                    break;
                case "General/OpenOriginalSubtitleFile":
                    language.General.OpenOriginalSubtitleFile = reader.Value;
                    break;
                case "General/PleaseWait":
                    language.General.PleaseWait = reader.Value;
                    break;
                case "General/SessionKey":
                    language.General.SessionKey = reader.Value;
                    break;
                case "General/UserName":
                    language.General.UserName = reader.Value;
                    break;
                case "General/UserNameAlreadyInUse":
                    language.General.UserNameAlreadyInUse = reader.Value;
                    break;
                case "General/WebServiceUrl":
                    language.General.WebServiceUrl = reader.Value;
                    break;
                case "General/IP":
                    language.General.IP = reader.Value;
                    break;
                case "General/VideoWindowTitle":
                    language.General.VideoWindowTitle = reader.Value;
                    break;
                case "General/AudioWindowTitle":
                    language.General.AudioWindowTitle = reader.Value;
                    break;
                case "General/ControlsWindowTitle":
                    language.General.ControlsWindowTitle = reader.Value;
                    break;
                case "General/Advanced":
                    language.General.Advanced = reader.Value;
                    break;
                case "General/Style":
                    language.General.Style = reader.Value;
                    break;
                case "General/StyleLanguage":
                    language.General.StyleLanguage = reader.Value;
                    break;
                case "General/Character":
                    language.General.Character = reader.Value;
                    break;
                case "General/Class":
                    language.General.Class = reader.Value;
                    break;
                case "General/GeneralText":
                    language.General.GeneralText = reader.Value;
                    break;
                case "General/LineNumber":
                    language.General.LineNumber = reader.Value;
                    break;
                case "General/Before":
                    language.General.Before = reader.Value;
                    break;
                case "General/After":
                    language.General.After = reader.Value;
                    break;
                case "General/Size":
                    language.General.Size = reader.Value;
                    break;
                case "General/Search":
                    language.General.Search = reader.Value;
                    break;
                case "General/DeleteCurrentLine":
                    language.General.DeleteCurrentLine = reader.Value;
                    break;
                case "General/Width":
                    language.General.Width = reader.Value;
                    break;
                case "General/Height":
                    language.General.Height = reader.Value;
                    break;
                case "About/Title":
                    language.About.Title = reader.Value;
                    break;
                case "About/AboutText1":
                    language.About.AboutText1 = reader.Value;
                    break;
                case "AddToNames/Title":
                    language.AddToNames.Title = reader.Value;
                    break;
                case "AddToNames/Description":
                    language.AddToNames.Description = reader.Value;
                    break;
                case "AddToOcrReplaceList/Title":
                    language.AddToOcrReplaceList.Title = reader.Value;
                    break;
                case "AddToOcrReplaceList/Description":
                    language.AddToOcrReplaceList.Description = reader.Value;
                    break;
                case "AddToUserDictionary/Title":
                    language.AddToUserDictionary.Title = reader.Value;
                    break;
                case "AddToUserDictionary/Description":
                    language.AddToUserDictionary.Description = reader.Value;
                    break;
                case "AddWaveform/Title":
                    language.AddWaveform.Title = reader.Value;
                    break;
                case "AddWaveform/SourceVideoFile":
                    language.AddWaveform.SourceVideoFile = reader.Value;
                    break;
                case "AddWaveform/GenerateWaveformData":
                    language.AddWaveform.GenerateWaveformData = reader.Value;
                    break;
                case "AddWaveform/PleaseWait":
                    language.AddWaveform.PleaseWait = reader.Value;
                    break;
                case "AddWaveform/VlcMediaPlayerNotFoundTitle":
                    language.AddWaveform.VlcMediaPlayerNotFoundTitle = reader.Value;
                    break;
                case "AddWaveform/VlcMediaPlayerNotFound":
                    language.AddWaveform.VlcMediaPlayerNotFound = reader.Value;
                    break;
                case "AddWaveform/GoToVlcMediaPlayerHomePage":
                    language.AddWaveform.GoToVlcMediaPlayerHomePage = reader.Value;
                    break;
                case "AddWaveform/GeneratingPeakFile":
                    language.AddWaveform.GeneratingPeakFile = reader.Value;
                    break;
                case "AddWaveform/GeneratingSpectrogram":
                    language.AddWaveform.GeneratingSpectrogram = reader.Value;
                    break;
                case "AddWaveform/ExtractingSeconds":
                    language.AddWaveform.ExtractingSeconds = reader.Value;
                    break;
                case "AddWaveform/ExtractingMinutes":
                    language.AddWaveform.ExtractingMinutes = reader.Value;
                    break;
                case "AddWaveform/WaveFileNotFound":
                    language.AddWaveform.WaveFileNotFound = reader.Value;
                    break;
                case "AddWaveform/WaveFileMalformed":
                    language.AddWaveform.WaveFileMalformed = reader.Value;
                    break;
                case "AddWaveform/LowDiskSpace":
                    language.AddWaveform.LowDiskSpace = reader.Value;
                    break;
                case "AddWaveform/FreeDiskSpace":
                    language.AddWaveform.FreeDiskSpace = reader.Value;
                    break;
                case "AddWaveformBatch/Title":
                    language.AddWaveformBatch.Title = reader.Value;
                    break;
                case "AddWaveformBatch/ExtractingAudio":
                    language.AddWaveformBatch.ExtractingAudio = reader.Value;
                    break;
                case "AddWaveformBatch/Calculating":
                    language.AddWaveformBatch.Calculating = reader.Value;
                    break;
                case "AddWaveformBatch/Done":
                    language.AddWaveformBatch.Done = reader.Value;
                    break;
                case "AddWaveformBatch/Error":
                    language.AddWaveformBatch.Error = reader.Value;
                    break;
                case "AdjustDisplayDuration/Title":
                    language.AdjustDisplayDuration.Title = reader.Value;
                    break;
                case "AdjustDisplayDuration/AdjustVia":
                    language.AdjustDisplayDuration.AdjustVia = reader.Value;
                    break;
                case "AdjustDisplayDuration/Seconds":
                    language.AdjustDisplayDuration.Seconds = reader.Value;
                    break;
                case "AdjustDisplayDuration/Percent":
                    language.AdjustDisplayDuration.Percent = reader.Value;
                    break;
                case "AdjustDisplayDuration/Recalculate":
                    language.AdjustDisplayDuration.Recalculate = reader.Value;
                    break;
                case "AdjustDisplayDuration/AddSeconds":
                    language.AdjustDisplayDuration.AddSeconds = reader.Value;
                    break;
                case "AdjustDisplayDuration/SetAsPercent":
                    language.AdjustDisplayDuration.SetAsPercent = reader.Value;
                    break;
                case "AdjustDisplayDuration/Note":
                    language.AdjustDisplayDuration.Note = reader.Value;
                    break;
                case "AdjustDisplayDuration/Fixed":
                    language.AdjustDisplayDuration.Fixed = reader.Value;
                    break;
                case "AdjustDisplayDuration/Milliseconds":
                    language.AdjustDisplayDuration.Milliseconds = reader.Value;
                    break;
                case "AdjustDisplayDuration/ExtendOnly":
                    language.AdjustDisplayDuration.ExtendOnly = reader.Value;
                    break;
                case "ApplyDurationLimits/Title":
                    language.ApplyDurationLimits.Title = reader.Value;
                    break;
                case "ApplyDurationLimits/FixesAvailable":
                    language.ApplyDurationLimits.FixesAvailable = reader.Value;
                    break;
                case "ApplyDurationLimits/UnableToFix":
                    language.ApplyDurationLimits.UnableToFix = reader.Value;
                    break;
                case "AudioToText/Title":
                    language.AudioToText.Title = reader.Value;
                    break;
                case "AudioToText/ExtractingAudioUsingX":
                    language.AudioToText.ExtractingAudioUsingX = reader.Value;
                    break;
                case "AudioToText/ExtractingTextUsingX":
                    language.AudioToText.ExtractingTextUsingX = reader.Value;
                    break;
                case "AudioToText/ProgessViaXy":
                    language.AudioToText.ProgessViaXy = reader.Value;
                    break;
                case "AudioToText/ShowLess":
                    language.AudioToText.ShowLess = reader.Value;
                    break;
                case "AudioToText/ShowMore":
                    language.AudioToText.ShowMore = reader.Value;
                    break;
                case "AutoBreakUnbreakLines/TitleAutoBreak":
                    language.AutoBreakUnbreakLines.TitleAutoBreak = reader.Value;
                    break;
                case "AutoBreakUnbreakLines/TitleUnbreak":
                    language.AutoBreakUnbreakLines.TitleUnbreak = reader.Value;
                    break;
                case "AutoBreakUnbreakLines/LinesFoundX":
                    language.AutoBreakUnbreakLines.LinesFoundX = reader.Value;
                    break;
                case "AutoBreakUnbreakLines/OnlyBreakLinesLongerThan":
                    language.AutoBreakUnbreakLines.OnlyBreakLinesLongerThan = reader.Value;
                    break;
                case "AutoBreakUnbreakLines/OnlyUnbreakLinesLongerThan":
                    language.AutoBreakUnbreakLines.OnlyUnbreakLinesLongerThan = reader.Value;
                    break;
                case "BatchConvert/Title":
                    language.BatchConvert.Title = reader.Value;
                    break;
                case "BatchConvert/Input":
                    language.BatchConvert.Input = reader.Value;
                    break;
                case "BatchConvert/InputDescription":
                    language.BatchConvert.InputDescription = reader.Value;
                    break;
                case "BatchConvert/Status":
                    language.BatchConvert.Status = reader.Value;
                    break;
                case "BatchConvert/Output":
                    language.BatchConvert.Output = reader.Value;
                    break;
                case "BatchConvert/SaveInSourceFolder":
                    language.BatchConvert.SaveInSourceFolder = reader.Value;
                    break;
                case "BatchConvert/SaveInOutputFolder":
                    language.BatchConvert.SaveInOutputFolder = reader.Value;
                    break;
                case "BatchConvert/OverwriteFiles":
                    language.BatchConvert.OverwriteFiles = reader.Value;
                    break;
                case "BatchConvert/Style":
                    language.BatchConvert.Style = reader.Value;
                    break;
                case "BatchConvert/UseStyleFromSource":
                    language.BatchConvert.UseStyleFromSource = reader.Value;
                    break;
                case "BatchConvert/ConvertOptions":
                    language.BatchConvert.ConvertOptions = reader.Value;
                    break;
                case "BatchConvert/RemoveFormatting":
                    language.BatchConvert.RemoveFormatting = reader.Value;
                    break;
                case "BatchConvert/RemoveTextForHI":
                    language.BatchConvert.RemoveTextForHI = reader.Value;
                    break;
                case "BatchConvert/OverwriteOriginalFiles":
                    language.BatchConvert.OverwriteOriginalFiles = reader.Value;
                    break;
                case "BatchConvert/RedoCasing":
                    language.BatchConvert.RedoCasing = reader.Value;
                    break;
                case "BatchConvert/Convert":
                    language.BatchConvert.Convert = reader.Value;
                    break;
                case "BatchConvert/NothingToConvert":
                    language.BatchConvert.NothingToConvert = reader.Value;
                    break;
                case "BatchConvert/PleaseChooseOutputFolder":
                    language.BatchConvert.PleaseChooseOutputFolder = reader.Value;
                    break;
                case "BatchConvert/NotConverted":
                    language.BatchConvert.NotConverted = reader.Value;
                    break;
                case "BatchConvert/Converted":
                    language.BatchConvert.Converted = reader.Value;
                    break;
                case "BatchConvert/Settings":
                    language.BatchConvert.Settings = reader.Value;
                    break;
                case "BatchConvert/FixRtl":
                    language.BatchConvert.FixRtl = reader.Value;
                    break;
                case "BatchConvert/FixRtlAddUnicode":
                    language.BatchConvert.FixRtlAddUnicode = reader.Value;
                    break;
                case "BatchConvert/FixRtlRemoveUnicode":
                    language.BatchConvert.FixRtlRemoveUnicode = reader.Value;
                    break;
                case "BatchConvert/FixRtlReverseStartEnd":
                    language.BatchConvert.FixRtlReverseStartEnd = reader.Value;
                    break;
                case "BatchConvert/SplitLongLines":
                    language.BatchConvert.SplitLongLines = reader.Value;
                    break;
                case "BatchConvert/AutoBalance":
                    language.BatchConvert.AutoBalance = reader.Value;
                    break;
                case "BatchConvert/ScanFolder":
                    language.BatchConvert.ScanFolder = reader.Value;
                    break;
                case "BatchConvert/Recursive":
                    language.BatchConvert.Recursive = reader.Value;
                    break;
                case "BatchConvert/SetMinMsBetweenSubtitles":
                    language.BatchConvert.SetMinMsBetweenSubtitles = reader.Value;
                    break;
                case "BatchConvert/BridgeGaps":
                    language.BatchConvert.BridgeGaps = reader.Value;
                    break;
                case "BatchConvert/PlainText":
                    language.BatchConvert.PlainText = reader.Value;
                    break;
                case "BatchConvert/Ocr":
                    language.BatchConvert.Ocr = reader.Value;
                    break;
                case "BatchConvert/Filter":
                    language.BatchConvert.Filter = reader.Value;
                    break;
                case "BatchConvert/FilterSkipped":
                    language.BatchConvert.FilterSkipped = reader.Value;
                    break;
                case "BatchConvert/FilterSrtNoUtf8BOM":
                    language.BatchConvert.FilterSrtNoUtf8BOM = reader.Value;
                    break;
                case "BatchConvert/FilterMoreThanTwoLines":
                    language.BatchConvert.FilterMoreThanTwoLines = reader.Value;
                    break;
                case "BatchConvert/FilterContains":
                    language.BatchConvert.FilterContains = reader.Value;
                    break;
                case "BatchConvert/FilterFileNameContains":
                    language.BatchConvert.FilterFileNameContains = reader.Value;
                    break;
                case "BatchConvert/FixCommonErrorsErrorX":
                    language.BatchConvert.FixCommonErrorsErrorX = reader.Value;
                    break;
                case "BatchConvert/MultipleReplaceErrorX":
                    language.BatchConvert.MultipleReplaceErrorX = reader.Value;
                    break;
                case "BatchConvert/AutoBalanceErrorX":
                    language.BatchConvert.AutoBalanceErrorX = reader.Value;
                    break;
                case "BatchConvert/OffsetTimeCodes":
                    language.BatchConvert.OffsetTimeCodes = reader.Value;
                    break;
                case "BatchConvert/TransportStreamSettings":
                    language.BatchConvert.TransportStreamSettings = reader.Value;
                    break;
                case "BatchConvert/TransportStreamOverrideXPosition":
                    language.BatchConvert.TransportStreamOverrideXPosition = reader.Value;
                    break;
                case "BatchConvert/TransportStreamOverrideYPosition":
                    language.BatchConvert.TransportStreamOverrideYPosition = reader.Value;
                    break;
                case "BatchConvert/TransportStreamOverrideVideoSize":
                    language.BatchConvert.TransportStreamOverrideVideoSize = reader.Value;
                    break;
                case "BatchConvert/TransportStreamFileNameEnding":
                    language.BatchConvert.TransportStreamFileNameEnding = reader.Value;
                    break;
                case "BatchConvert/TransportStreamSettingsButton":
                    language.BatchConvert.TransportStreamSettingsButton = reader.Value;
                    break;
                case "BatchConvert/RemoveLineBreaks":
                    language.BatchConvert.RemoveLineBreaks = reader.Value;
                    break;
                case "Beamer/Title":
                    language.Beamer.Title = reader.Value;
                    break;
                case "Bookmarks/EditBookmark":
                    language.Bookmarks.EditBookmark = reader.Value;
                    break;
                case "Bookmarks/AddBookmark":
                    language.Bookmarks.AddBookmark = reader.Value;
                    break;
                case "Bookmarks/GoToBookmark":
                    language.Bookmarks.GoToBookmark = reader.Value;
                    break;
                case "ChangeCasing/Title":
                    language.ChangeCasing.Title = reader.Value;
                    break;
                case "ChangeCasing/ChangeCasingTo":
                    language.ChangeCasing.ChangeCasingTo = reader.Value;
                    break;
                case "ChangeCasing/NormalCasing":
                    language.ChangeCasing.NormalCasing = reader.Value;
                    break;
                case "ChangeCasing/FixNamesCasing":
                    language.ChangeCasing.FixNamesCasing = reader.Value;
                    break;
                case "ChangeCasing/FixOnlyNamesCasing":
                    language.ChangeCasing.FixOnlyNamesCasing = reader.Value;
                    break;
                case "ChangeCasing/OnlyChangeAllUppercaseLines":
                    language.ChangeCasing.OnlyChangeAllUppercaseLines = reader.Value;
                    break;
                case "ChangeCasing/AllUppercase":
                    language.ChangeCasing.AllUppercase = reader.Value;
                    break;
                case "ChangeCasing/AllLowercase":
                    language.ChangeCasing.AllLowercase = reader.Value;
                    break;
                case "ChangeCasingNames/Title":
                    language.ChangeCasingNames.Title = reader.Value;
                    break;
                case "ChangeCasingNames/NamesFoundInSubtitleX":
                    language.ChangeCasingNames.NamesFoundInSubtitleX = reader.Value;
                    break;
                case "ChangeCasingNames/Enabled":
                    language.ChangeCasingNames.Enabled = reader.Value;
                    break;
                case "ChangeCasingNames/Name":
                    language.ChangeCasingNames.Name = reader.Value;
                    break;
                case "ChangeCasingNames/LinesFoundX":
                    language.ChangeCasingNames.LinesFoundX = reader.Value;
                    break;
                case "ChangeCasingNames/ExtraNames":
                    language.ChangeCasingNames.ExtraNames = reader.Value;
                    break;
                case "ChangeFrameRate/Title":
                    language.ChangeFrameRate.Title = reader.Value;
                    break;
                case "ChangeFrameRate/ConvertFrameRateOfSubtitle":
                    language.ChangeFrameRate.ConvertFrameRateOfSubtitle = reader.Value;
                    break;
                case "ChangeFrameRate/FromFrameRate":
                    language.ChangeFrameRate.FromFrameRate = reader.Value;
                    break;
                case "ChangeFrameRate/ToFrameRate":
                    language.ChangeFrameRate.ToFrameRate = reader.Value;
                    break;
                case "ChangeFrameRate/FrameRateNotCorrect":
                    language.ChangeFrameRate.FrameRateNotCorrect = reader.Value;
                    break;
                case "ChangeFrameRate/FrameRateNotChanged":
                    language.ChangeFrameRate.FrameRateNotChanged = reader.Value;
                    break;
                case "ChangeSpeedInPercent/Title":
                    language.ChangeSpeedInPercent.Title = reader.Value;
                    break;
                case "ChangeSpeedInPercent/TitleShort":
                    language.ChangeSpeedInPercent.TitleShort = reader.Value;
                    break;
                case "ChangeSpeedInPercent/Info":
                    language.ChangeSpeedInPercent.Info = reader.Value;
                    break;
                case "ChangeSpeedInPercent/Custom":
                    language.ChangeSpeedInPercent.Custom = reader.Value;
                    break;
                case "ChangeSpeedInPercent/ToDropFrame":
                    language.ChangeSpeedInPercent.ToDropFrame = reader.Value;
                    break;
                case "ChangeSpeedInPercent/FromDropFrame":
                    language.ChangeSpeedInPercent.FromDropFrame = reader.Value;
                    break;
                case "CheckForUpdates/Title":
                    language.CheckForUpdates.Title = reader.Value;
                    break;
                case "CheckForUpdates/CheckingForUpdates":
                    language.CheckForUpdates.CheckingForUpdates = reader.Value;
                    break;
                case "CheckForUpdates/CheckingForUpdatesFailedX":
                    language.CheckForUpdates.CheckingForUpdatesFailedX = reader.Value;
                    break;
                case "CheckForUpdates/CheckingForUpdatesNoneAvailable":
                    language.CheckForUpdates.CheckingForUpdatesNoneAvailable = reader.Value;
                    break;
                case "CheckForUpdates/CheckingForUpdatesNewVersion":
                    language.CheckForUpdates.CheckingForUpdatesNewVersion = reader.Value;
                    break;
                case "CheckForUpdates/InstallUpdate":
                    language.CheckForUpdates.InstallUpdate = reader.Value;
                    break;
                case "CheckForUpdates/NoUpdates":
                    language.CheckForUpdates.NoUpdates = reader.Value;
                    break;
                case "ChooseAudioTrack/Title":
                    language.ChooseAudioTrack.Title = reader.Value;
                    break;
                case "ChooseEncoding/Title":
                    language.ChooseEncoding.Title = reader.Value;
                    break;
                case "ChooseEncoding/CodePage":
                    language.ChooseEncoding.CodePage = reader.Value;
                    break;
                case "ChooseEncoding/DisplayName":
                    language.ChooseEncoding.DisplayName = reader.Value;
                    break;
                case "ChooseEncoding/PleaseSelectAnEncoding":
                    language.ChooseEncoding.PleaseSelectAnEncoding = reader.Value;
                    break;
                case "ChooseLanguage/Title":
                    language.ChooseLanguage.Title = reader.Value;
                    break;
                case "ChooseLanguage/Language":
                    language.ChooseLanguage.Language = reader.Value;
                    break;
                case "ColorChooser/Title":
                    language.ColorChooser.Title = reader.Value;
                    break;
                case "ColorChooser/Red":
                    language.ColorChooser.Red = reader.Value;
                    break;
                case "ColorChooser/Green":
                    language.ColorChooser.Green = reader.Value;
                    break;
                case "ColorChooser/Blue":
                    language.ColorChooser.Blue = reader.Value;
                    break;
                case "ColorChooser/Alpha":
                    language.ColorChooser.Alpha = reader.Value;
                    break;
                case "ColumnPaste/Title":
                    language.ColumnPaste.Title = reader.Value;
                    break;
                case "ColumnPaste/ChooseColumn":
                    language.ColumnPaste.ChooseColumn = reader.Value;
                    break;
                case "ColumnPaste/OverwriteShiftCellsDown":
                    language.ColumnPaste.OverwriteShiftCellsDown = reader.Value;
                    break;
                case "ColumnPaste/Overwrite":
                    language.ColumnPaste.Overwrite = reader.Value;
                    break;
                case "ColumnPaste/ShiftCellsDown":
                    language.ColumnPaste.ShiftCellsDown = reader.Value;
                    break;
                case "ColumnPaste/TimeCodesOnly":
                    language.ColumnPaste.TimeCodesOnly = reader.Value;
                    break;
                case "ColumnPaste/TextOnly":
                    language.ColumnPaste.TextOnly = reader.Value;
                    break;
                case "ColumnPaste/OriginalTextOnly":
                    language.ColumnPaste.OriginalTextOnly = reader.Value;
                    break;
                case "CompareSubtitles/Title":
                    language.CompareSubtitles.Title = reader.Value;
                    break;
                case "CompareSubtitles/PreviousDifference":
                    language.CompareSubtitles.PreviousDifference = reader.Value;
                    break;
                case "CompareSubtitles/NextDifference":
                    language.CompareSubtitles.NextDifference = reader.Value;
                    break;
                case "CompareSubtitles/SubtitlesNotAlike":
                    language.CompareSubtitles.SubtitlesNotAlike = reader.Value;
                    break;
                case "CompareSubtitles/XNumberOfDifference":
                    language.CompareSubtitles.XNumberOfDifference = reader.Value;
                    break;
                case "CompareSubtitles/XNumberOfDifferenceAndPercentChanged":
                    language.CompareSubtitles.XNumberOfDifferenceAndPercentChanged = reader.Value;
                    break;
                case "CompareSubtitles/XNumberOfDifferenceAndPercentLettersChanged":
                    language.CompareSubtitles.XNumberOfDifferenceAndPercentLettersChanged = reader.Value;
                    break;
                case "CompareSubtitles/ShowOnlyDifferences":
                    language.CompareSubtitles.ShowOnlyDifferences = reader.Value;
                    break;
                case "CompareSubtitles/IgnoreLineBreaks":
                    language.CompareSubtitles.IgnoreLineBreaks = reader.Value;
                    break;
                case "CompareSubtitles/IgnoreFormatting":
                    language.CompareSubtitles.IgnoreFormatting = reader.Value;
                    break;
                case "CompareSubtitles/OnlyLookForDifferencesInText":
                    language.CompareSubtitles.OnlyLookForDifferencesInText = reader.Value;
                    break;
                case "CompareSubtitles/CannotCompareWithImageBasedSubtitles":
                    language.CompareSubtitles.CannotCompareWithImageBasedSubtitles = reader.Value;
                    break;
                case "DCinemaProperties/Title":
                    language.DCinemaProperties.Title = reader.Value;
                    break;
                case "DCinemaProperties/TitleSmpte":
                    language.DCinemaProperties.TitleSmpte = reader.Value;
                    break;
                case "DCinemaProperties/SubtitleId":
                    language.DCinemaProperties.SubtitleId = reader.Value;
                    break;
                case "DCinemaProperties/GenerateId":
                    language.DCinemaProperties.GenerateId = reader.Value;
                    break;
                case "DCinemaProperties/MovieTitle":
                    language.DCinemaProperties.MovieTitle = reader.Value;
                    break;
                case "DCinemaProperties/ReelNumber":
                    language.DCinemaProperties.ReelNumber = reader.Value;
                    break;
                case "DCinemaProperties/Language":
                    language.DCinemaProperties.Language = reader.Value;
                    break;
                case "DCinemaProperties/IssueDate":
                    language.DCinemaProperties.IssueDate = reader.Value;
                    break;
                case "DCinemaProperties/EditRate":
                    language.DCinemaProperties.EditRate = reader.Value;
                    break;
                case "DCinemaProperties/TimeCodeRate":
                    language.DCinemaProperties.TimeCodeRate = reader.Value;
                    break;
                case "DCinemaProperties/StartTime":
                    language.DCinemaProperties.StartTime = reader.Value;
                    break;
                case "DCinemaProperties/Font":
                    language.DCinemaProperties.Font = reader.Value;
                    break;
                case "DCinemaProperties/FontId":
                    language.DCinemaProperties.FontId = reader.Value;
                    break;
                case "DCinemaProperties/FontUri":
                    language.DCinemaProperties.FontUri = reader.Value;
                    break;
                case "DCinemaProperties/FontColor":
                    language.DCinemaProperties.FontColor = reader.Value;
                    break;
                case "DCinemaProperties/FontEffect":
                    language.DCinemaProperties.FontEffect = reader.Value;
                    break;
                case "DCinemaProperties/FontEffectColor":
                    language.DCinemaProperties.FontEffectColor = reader.Value;
                    break;
                case "DCinemaProperties/FontSize":
                    language.DCinemaProperties.FontSize = reader.Value;
                    break;
                case "DCinemaProperties/TopBottomMargin":
                    language.DCinemaProperties.TopBottomMargin = reader.Value;
                    break;
                case "DCinemaProperties/FadeUpTime":
                    language.DCinemaProperties.FadeUpTime = reader.Value;
                    break;
                case "DCinemaProperties/FadeDownTime":
                    language.DCinemaProperties.FadeDownTime = reader.Value;
                    break;
                case "DCinemaProperties/ZPosition":
                    language.DCinemaProperties.ZPosition = reader.Value;
                    break;
                case "DCinemaProperties/ZPositionHelp":
                    language.DCinemaProperties.ZPositionHelp = reader.Value;
                    break;
                case "DCinemaProperties/ChooseColor":
                    language.DCinemaProperties.ChooseColor = reader.Value;
                    break;
                case "DCinemaProperties/Generate":
                    language.DCinemaProperties.Generate = reader.Value;
                    break;
                case "DurationsBridgeGaps/Title":
                    language.DurationsBridgeGaps.Title = reader.Value;
                    break;
                case "DurationsBridgeGaps/GapsBridgedX":
                    language.DurationsBridgeGaps.GapsBridgedX = reader.Value;
                    break;
                case "DurationsBridgeGaps/GapToNext":
                    language.DurationsBridgeGaps.GapToNext = reader.Value;
                    break;
                case "DurationsBridgeGaps/BridgeGapsSmallerThanXPart1":
                    language.DurationsBridgeGaps.BridgeGapsSmallerThanXPart1 = reader.Value;
                    break;
                case "DurationsBridgeGaps/BridgeGapsSmallerThanXPart2":
                    language.DurationsBridgeGaps.BridgeGapsSmallerThanXPart2 = reader.Value;
                    break;
                case "DurationsBridgeGaps/MinMillisecondsBetweenLines":
                    language.DurationsBridgeGaps.MinMillisecondsBetweenLines = reader.Value;
                    break;
                case "DurationsBridgeGaps/ProlongEndTime":
                    language.DurationsBridgeGaps.ProlongEndTime = reader.Value;
                    break;
                case "DurationsBridgeGaps/DivideEven":
                    language.DurationsBridgeGaps.DivideEven = reader.Value;
                    break;
                case "DvdSubRip/Title":
                    language.DvdSubRip.Title = reader.Value;
                    break;
                case "DvdSubRip/DvdGroupTitle":
                    language.DvdSubRip.DvdGroupTitle = reader.Value;
                    break;
                case "DvdSubRip/IfoFile":
                    language.DvdSubRip.IfoFile = reader.Value;
                    break;
                case "DvdSubRip/IfoFiles":
                    language.DvdSubRip.IfoFiles = reader.Value;
                    break;
                case "DvdSubRip/VobFiles":
                    language.DvdSubRip.VobFiles = reader.Value;
                    break;
                case "DvdSubRip/Add":
                    language.DvdSubRip.Add = reader.Value;
                    break;
                case "DvdSubRip/Remove":
                    language.DvdSubRip.Remove = reader.Value;
                    break;
                case "DvdSubRip/Clear":
                    language.DvdSubRip.Clear = reader.Value;
                    break;
                case "DvdSubRip/MoveUp":
                    language.DvdSubRip.MoveUp = reader.Value;
                    break;
                case "DvdSubRip/MoveDown":
                    language.DvdSubRip.MoveDown = reader.Value;
                    break;
                case "DvdSubRip/Languages":
                    language.DvdSubRip.Languages = reader.Value;
                    break;
                case "DvdSubRip/PalNtsc":
                    language.DvdSubRip.PalNtsc = reader.Value;
                    break;
                case "DvdSubRip/Pal":
                    language.DvdSubRip.Pal = reader.Value;
                    break;
                case "DvdSubRip/Ntsc":
                    language.DvdSubRip.Ntsc = reader.Value;
                    break;
                case "DvdSubRip/StartRipping":
                    language.DvdSubRip.StartRipping = reader.Value;
                    break;
                case "DvdSubRip/Abort":
                    language.DvdSubRip.Abort = reader.Value;
                    break;
                case "DvdSubRip/AbortedByUser":
                    language.DvdSubRip.AbortedByUser = reader.Value;
                    break;
                case "DvdSubRip/ReadingSubtitleData":
                    language.DvdSubRip.ReadingSubtitleData = reader.Value;
                    break;
                case "DvdSubRip/RippingVobFileXofYZ":
                    language.DvdSubRip.RippingVobFileXofYZ = reader.Value;
                    break;
                case "DvdSubRip/WrongIfoType":
                    language.DvdSubRip.WrongIfoType = reader.Value;
                    break;
                case "DvdSubRipChooseLanguage/Title":
                    language.DvdSubRipChooseLanguage.Title = reader.Value;
                    break;
                case "DvdSubRipChooseLanguage/ChooseLanguageStreamId":
                    language.DvdSubRipChooseLanguage.ChooseLanguageStreamId = reader.Value;
                    break;
                case "DvdSubRipChooseLanguage/UnknownLanguage":
                    language.DvdSubRipChooseLanguage.UnknownLanguage = reader.Value;
                    break;
                case "DvdSubRipChooseLanguage/SubtitleImageXofYAndWidthXHeight":
                    language.DvdSubRipChooseLanguage.SubtitleImageXofYAndWidthXHeight = reader.Value;
                    break;
                case "DvdSubRipChooseLanguage/SubtitleImage":
                    language.DvdSubRipChooseLanguage.SubtitleImage = reader.Value;
                    break;
                case "EbuSaveOptions/Title":
                    language.EbuSaveOptions.Title = reader.Value;
                    break;
                case "EbuSaveOptions/GeneralSubtitleInformation":
                    language.EbuSaveOptions.GeneralSubtitleInformation = reader.Value;
                    break;
                case "EbuSaveOptions/CodePageNumber":
                    language.EbuSaveOptions.CodePageNumber = reader.Value;
                    break;
                case "EbuSaveOptions/DiskFormatCode":
                    language.EbuSaveOptions.DiskFormatCode = reader.Value;
                    break;
                case "EbuSaveOptions/DisplayStandardCode":
                    language.EbuSaveOptions.DisplayStandardCode = reader.Value;
                    break;
                case "EbuSaveOptions/ColorRequiresTeletext":
                    language.EbuSaveOptions.ColorRequiresTeletext = reader.Value;
                    break;
                case "EbuSaveOptions/CharacterCodeTable":
                    language.EbuSaveOptions.CharacterCodeTable = reader.Value;
                    break;
                case "EbuSaveOptions/LanguageCode":
                    language.EbuSaveOptions.LanguageCode = reader.Value;
                    break;
                case "EbuSaveOptions/OriginalProgramTitle":
                    language.EbuSaveOptions.OriginalProgramTitle = reader.Value;
                    break;
                case "EbuSaveOptions/OriginalEpisodeTitle":
                    language.EbuSaveOptions.OriginalEpisodeTitle = reader.Value;
                    break;
                case "EbuSaveOptions/TranslatedProgramTitle":
                    language.EbuSaveOptions.TranslatedProgramTitle = reader.Value;
                    break;
                case "EbuSaveOptions/TranslatedEpisodeTitle":
                    language.EbuSaveOptions.TranslatedEpisodeTitle = reader.Value;
                    break;
                case "EbuSaveOptions/TranslatorsName":
                    language.EbuSaveOptions.TranslatorsName = reader.Value;
                    break;
                case "EbuSaveOptions/SubtitleListReferenceCode":
                    language.EbuSaveOptions.SubtitleListReferenceCode = reader.Value;
                    break;
                case "EbuSaveOptions/CountryOfOrigin":
                    language.EbuSaveOptions.CountryOfOrigin = reader.Value;
                    break;
                case "EbuSaveOptions/TimeCodeStatus":
                    language.EbuSaveOptions.TimeCodeStatus = reader.Value;
                    break;
                case "EbuSaveOptions/TimeCodeStartOfProgramme":
                    language.EbuSaveOptions.TimeCodeStartOfProgramme = reader.Value;
                    break;
                case "EbuSaveOptions/RevisionNumber":
                    language.EbuSaveOptions.RevisionNumber = reader.Value;
                    break;
                case "EbuSaveOptions/MaxNoOfDisplayableChars":
                    language.EbuSaveOptions.MaxNoOfDisplayableChars = reader.Value;
                    break;
                case "EbuSaveOptions/MaxNumberOfDisplayableRows":
                    language.EbuSaveOptions.MaxNumberOfDisplayableRows = reader.Value;
                    break;
                case "EbuSaveOptions/DiskSequenceNumber":
                    language.EbuSaveOptions.DiskSequenceNumber = reader.Value;
                    break;
                case "EbuSaveOptions/TotalNumberOfDisks":
                    language.EbuSaveOptions.TotalNumberOfDisks = reader.Value;
                    break;
                case "EbuSaveOptions/Import":
                    language.EbuSaveOptions.Import = reader.Value;
                    break;
                case "EbuSaveOptions/TextAndTimingInformation":
                    language.EbuSaveOptions.TextAndTimingInformation = reader.Value;
                    break;
                case "EbuSaveOptions/JustificationCode":
                    language.EbuSaveOptions.JustificationCode = reader.Value;
                    break;
                case "EbuSaveOptions/VerticalPosition":
                    language.EbuSaveOptions.VerticalPosition = reader.Value;
                    break;
                case "EbuSaveOptions/MarginTop":
                    language.EbuSaveOptions.MarginTop = reader.Value;
                    break;
                case "EbuSaveOptions/MarginBottom":
                    language.EbuSaveOptions.MarginBottom = reader.Value;
                    break;
                case "EbuSaveOptions/NewLineRows":
                    language.EbuSaveOptions.NewLineRows = reader.Value;
                    break;
                case "EbuSaveOptions/Teletext":
                    language.EbuSaveOptions.Teletext = reader.Value;
                    break;
                case "EbuSaveOptions/UseBox":
                    language.EbuSaveOptions.UseBox = reader.Value;
                    break;
                case "EbuSaveOptions/DoubleHeight":
                    language.EbuSaveOptions.DoubleHeight = reader.Value;
                    break;
                case "EbuSaveOptions/Errors":
                    language.EbuSaveOptions.Errors = reader.Value;
                    break;
                case "EbuSaveOptions/ErrorsX":
                    language.EbuSaveOptions.ErrorsX = reader.Value;
                    break;
                case "EbuSaveOptions/MaxLengthError":
                    language.EbuSaveOptions.MaxLengthError = reader.Value;
                    break;
                case "EbuSaveOptions/TextUnchangedPresentation":
                    language.EbuSaveOptions.TextUnchangedPresentation = reader.Value;
                    break;
                case "EbuSaveOptions/TextLeftJustifiedText":
                    language.EbuSaveOptions.TextLeftJustifiedText = reader.Value;
                    break;
                case "EbuSaveOptions/TextCenteredText":
                    language.EbuSaveOptions.TextCenteredText = reader.Value;
                    break;
                case "EbuSaveOptions/TextRightJustifiedText":
                    language.EbuSaveOptions.TextRightJustifiedText = reader.Value;
                    break;
                case "EbuSaveOptions/UseBoxForOneNewLine":
                    language.EbuSaveOptions.UseBoxForOneNewLine = reader.Value;
                    break;
                case "EffectKaraoke/Title":
                    language.EffectKaraoke.Title = reader.Value;
                    break;
                case "EffectKaraoke/ChooseColor":
                    language.EffectKaraoke.ChooseColor = reader.Value;
                    break;
                case "EffectKaraoke/TotalMilliseconds":
                    language.EffectKaraoke.TotalMilliseconds = reader.Value;
                    break;
                case "EffectKaraoke/EndDelayInMilliseconds":
                    language.EffectKaraoke.EndDelayInMilliseconds = reader.Value;
                    break;
                case "EffectTypewriter/Title":
                    language.EffectTypewriter.Title = reader.Value;
                    break;
                case "EffectTypewriter/TotalMilliseconds":
                    language.EffectTypewriter.TotalMilliseconds = reader.Value;
                    break;
                case "EffectTypewriter/EndDelayInMilliseconds":
                    language.EffectTypewriter.EndDelayInMilliseconds = reader.Value;
                    break;
                case "ExportCustomText/Title":
                    language.ExportCustomText.Title = reader.Value;
                    break;
                case "ExportCustomText/Formats":
                    language.ExportCustomText.Formats = reader.Value;
                    break;
                case "ExportCustomText/New":
                    language.ExportCustomText.New = reader.Value;
                    break;
                case "ExportCustomText/Edit":
                    language.ExportCustomText.Edit = reader.Value;
                    break;
                case "ExportCustomText/Delete":
                    language.ExportCustomText.Delete = reader.Value;
                    break;
                case "ExportCustomText/SaveAs":
                    language.ExportCustomText.SaveAs = reader.Value;
                    break;
                case "ExportCustomText/SaveSubtitleAs":
                    language.ExportCustomText.SaveSubtitleAs = reader.Value;
                    break;
                case "ExportCustomText/SubtitleExportedInCustomFormatToX":
                    language.ExportCustomText.SubtitleExportedInCustomFormatToX = reader.Value;
                    break;
                case "ExportCustomTextFormat/Title":
                    language.ExportCustomTextFormat.Title = reader.Value;
                    break;
                case "ExportCustomTextFormat/Template":
                    language.ExportCustomTextFormat.Template = reader.Value;
                    break;
                case "ExportCustomTextFormat/Header":
                    language.ExportCustomTextFormat.Header = reader.Value;
                    break;
                case "ExportCustomTextFormat/TextLine":
                    language.ExportCustomTextFormat.TextLine = reader.Value;
                    break;
                case "ExportCustomTextFormat/TimeCode":
                    language.ExportCustomTextFormat.TimeCode = reader.Value;
                    break;
                case "ExportCustomTextFormat/NewLine":
                    language.ExportCustomTextFormat.NewLine = reader.Value;
                    break;
                case "ExportCustomTextFormat/Footer":
                    language.ExportCustomTextFormat.Footer = reader.Value;
                    break;
                case "ExportCustomTextFormat/DoNotModify":
                    language.ExportCustomTextFormat.DoNotModify = reader.Value;
                    break;
                case "ExportFcpXmlAdvanced/Title":
                    language.ExportFcpXmlAdvanced.Title = reader.Value;
                    break;
                case "ExportFcpXmlAdvanced/FontName":
                    language.ExportFcpXmlAdvanced.FontName = reader.Value;
                    break;
                case "ExportFcpXmlAdvanced/FontSize":
                    language.ExportFcpXmlAdvanced.FontSize = reader.Value;
                    break;
                case "ExportFcpXmlAdvanced/FontFace":
                    language.ExportFcpXmlAdvanced.FontFace = reader.Value;
                    break;
                case "ExportFcpXmlAdvanced/FontFaceRegular":
                    language.ExportFcpXmlAdvanced.FontFaceRegular = reader.Value;
                    break;
                case "ExportFcpXmlAdvanced/Alignment":
                    language.ExportFcpXmlAdvanced.Alignment = reader.Value;
                    break;
                case "ExportFcpXmlAdvanced/Baseline":
                    language.ExportFcpXmlAdvanced.Baseline = reader.Value;
                    break;
                case "ExportPngXml/Title":
                    language.ExportPngXml.Title = reader.Value;
                    break;
                case "ExportPngXml/ImageSettings":
                    language.ExportPngXml.ImageSettings = reader.Value;
                    break;
                case "ExportPngXml/FontFamily":
                    language.ExportPngXml.FontFamily = reader.Value;
                    break;
                case "ExportPngXml/FontSize":
                    language.ExportPngXml.FontSize = reader.Value;
                    break;
                case "ExportPngXml/FontColor":
                    language.ExportPngXml.FontColor = reader.Value;
                    break;
                case "ExportPngXml/BorderColor":
                    language.ExportPngXml.BorderColor = reader.Value;
                    break;
                case "ExportPngXml/BorderWidth":
                    language.ExportPngXml.BorderWidth = reader.Value;
                    break;
                case "ExportPngXml/BorderStyle":
                    language.ExportPngXml.BorderStyle = reader.Value;
                    break;
                case "ExportPngXml/BorderStyleOneBox":
                    language.ExportPngXml.BorderStyleOneBox = reader.Value;
                    break;
                case "ExportPngXml/BorderStyleBoxForEachLine":
                    language.ExportPngXml.BorderStyleBoxForEachLine = reader.Value;
                    break;
                case "ExportPngXml/BorderStyleNormalWidthX":
                    language.ExportPngXml.BorderStyleNormalWidthX = reader.Value;
                    break;
                case "ExportPngXml/ShadowColor":
                    language.ExportPngXml.ShadowColor = reader.Value;
                    break;
                case "ExportPngXml/ShadowWidth":
                    language.ExportPngXml.ShadowWidth = reader.Value;
                    break;
                case "ExportPngXml/Transparency":
                    language.ExportPngXml.Transparency = reader.Value;
                    break;
                case "ExportPngXml/ImageFormat":
                    language.ExportPngXml.ImageFormat = reader.Value;
                    break;
                case "ExportPngXml/FullFrameImage":
                    language.ExportPngXml.FullFrameImage = reader.Value;
                    break;
                case "ExportPngXml/SimpleRendering":
                    language.ExportPngXml.SimpleRendering = reader.Value;
                    break;
                case "ExportPngXml/AntiAliasingWithTransparency":
                    language.ExportPngXml.AntiAliasingWithTransparency = reader.Value;
                    break;
                case "ExportPngXml/Text3D":
                    language.ExportPngXml.Text3D = reader.Value;
                    break;
                case "ExportPngXml/SideBySide3D":
                    language.ExportPngXml.SideBySide3D = reader.Value;
                    break;
                case "ExportPngXml/HalfTopBottom3D":
                    language.ExportPngXml.HalfTopBottom3D = reader.Value;
                    break;
                case "ExportPngXml/Depth":
                    language.ExportPngXml.Depth = reader.Value;
                    break;
                case "ExportPngXml/ExportAllLines":
                    language.ExportPngXml.ExportAllLines = reader.Value;
                    break;
                case "ExportPngXml/XImagesSavedInY":
                    language.ExportPngXml.XImagesSavedInY = reader.Value;
                    break;
                case "ExportPngXml/VideoResolution":
                    language.ExportPngXml.VideoResolution = reader.Value;
                    break;
                case "ExportPngXml/Align":
                    language.ExportPngXml.Align = reader.Value;
                    break;
                case "ExportPngXml/Left":
                    language.ExportPngXml.Left = reader.Value;
                    break;
                case "ExportPngXml/Right":
                    language.ExportPngXml.Right = reader.Value;
                    break;
                case "ExportPngXml/Center":
                    language.ExportPngXml.Center = reader.Value;
                    break;
                case "ExportPngXml/CenterLeftJustify":
                    language.ExportPngXml.CenterLeftJustify = reader.Value;
                    break;
                case "ExportPngXml/CenterTopJustify":
                    language.ExportPngXml.CenterTopJustify = reader.Value;
                    break;
                case "ExportPngXml/BottomMargin":
                    language.ExportPngXml.BottomMargin = reader.Value;
                    break;
                case "ExportPngXml/LeftRightMargin":
                    language.ExportPngXml.LeftRightMargin = reader.Value;
                    break;
                case "ExportPngXml/SaveBluRraySupAs":
                    language.ExportPngXml.SaveBluRraySupAs = reader.Value;
                    break;
                case "ExportPngXml/SaveVobSubAs":
                    language.ExportPngXml.SaveVobSubAs = reader.Value;
                    break;
                case "ExportPngXml/SaveFabImageScriptAs":
                    language.ExportPngXml.SaveFabImageScriptAs = reader.Value;
                    break;
                case "ExportPngXml/SaveDvdStudioProStlAs":
                    language.ExportPngXml.SaveDvdStudioProStlAs = reader.Value;
                    break;
                case "ExportPngXml/SaveDigitalCinemaInteropAs":
                    language.ExportPngXml.SaveDigitalCinemaInteropAs = reader.Value;
                    break;
                case "ExportPngXml/SavePremiereEdlAs":
                    language.ExportPngXml.SavePremiereEdlAs = reader.Value;
                    break;
                case "ExportPngXml/SaveFcpAs":
                    language.ExportPngXml.SaveFcpAs = reader.Value;
                    break;
                case "ExportPngXml/SaveDostAs":
                    language.ExportPngXml.SaveDostAs = reader.Value;
                    break;
                case "ExportPngXml/SomeLinesWereTooLongX":
                    language.ExportPngXml.SomeLinesWereTooLongX = reader.Value;
                    break;
                case "ExportPngXml/LineHeight":
                    language.ExportPngXml.LineHeight = reader.Value;
                    break;
                case "ExportPngXml/BoxSingleLine":
                    language.ExportPngXml.BoxSingleLine = reader.Value;
                    break;
                case "ExportPngXml/BoxMultiLine":
                    language.ExportPngXml.BoxMultiLine = reader.Value;
                    break;
                case "ExportPngXml/Forced":
                    language.ExportPngXml.Forced = reader.Value;
                    break;
                case "ExportPngXml/ChooseBackgroundColor":
                    language.ExportPngXml.ChooseBackgroundColor = reader.Value;
                    break;
                case "ExportPngXml/SaveImageAs":
                    language.ExportPngXml.SaveImageAs = reader.Value;
                    break;
                case "ExportPngXml/FcpUseFullPathUrl":
                    language.ExportPngXml.FcpUseFullPathUrl = reader.Value;
                    break;
                case "ExportText/Title":
                    language.ExportText.Title = reader.Value;
                    break;
                case "ExportText/Preview":
                    language.ExportText.Preview = reader.Value;
                    break;
                case "ExportText/ExportOptions":
                    language.ExportText.ExportOptions = reader.Value;
                    break;
                case "ExportText/FormatText":
                    language.ExportText.FormatText = reader.Value;
                    break;
                case "ExportText/None":
                    language.ExportText.None = reader.Value;
                    break;
                case "ExportText/MergeAllLines":
                    language.ExportText.MergeAllLines = reader.Value;
                    break;
                case "ExportText/UnbreakLines":
                    language.ExportText.UnbreakLines = reader.Value;
                    break;
                case "ExportText/RemoveStyling":
                    language.ExportText.RemoveStyling = reader.Value;
                    break;
                case "ExportText/ShowLineNumbers":
                    language.ExportText.ShowLineNumbers = reader.Value;
                    break;
                case "ExportText/AddNewLineAfterLineNumber":
                    language.ExportText.AddNewLineAfterLineNumber = reader.Value;
                    break;
                case "ExportText/ShowTimeCode":
                    language.ExportText.ShowTimeCode = reader.Value;
                    break;
                case "ExportText/AddNewLineAfterTimeCode":
                    language.ExportText.AddNewLineAfterTimeCode = reader.Value;
                    break;
                case "ExportText/AddNewLineAfterTexts":
                    language.ExportText.AddNewLineAfterTexts = reader.Value;
                    break;
                case "ExportText/AddNewLineBetweenSubtitles":
                    language.ExportText.AddNewLineBetweenSubtitles = reader.Value;
                    break;
                case "ExportText/TimeCodeFormat":
                    language.ExportText.TimeCodeFormat = reader.Value;
                    break;
                case "ExportText/Srt":
                    language.ExportText.Srt = reader.Value;
                    break;
                case "ExportText/Milliseconds":
                    language.ExportText.Milliseconds = reader.Value;
                    break;
                case "ExportText/HHMMSSFF":
                    language.ExportText.HHMMSSFF = reader.Value;
                    break;
                case "ExportText/TimeCodeSeparator":
                    language.ExportText.TimeCodeSeparator = reader.Value;
                    break;
                case "ExtractDateTimeInfo/Title":
                    language.ExtractDateTimeInfo.Title = reader.Value;
                    break;
                case "ExtractDateTimeInfo/OpenVideoFile":
                    language.ExtractDateTimeInfo.OpenVideoFile = reader.Value;
                    break;
                case "ExtractDateTimeInfo/StartFrom":
                    language.ExtractDateTimeInfo.StartFrom = reader.Value;
                    break;
                case "ExtractDateTimeInfo/DateTimeFormat":
                    language.ExtractDateTimeInfo.DateTimeFormat = reader.Value;
                    break;
                case "ExtractDateTimeInfo/Example":
                    language.ExtractDateTimeInfo.Example = reader.Value;
                    break;
                case "ExtractDateTimeInfo/GenerateSubtitle":
                    language.ExtractDateTimeInfo.GenerateSubtitle = reader.Value;
                    break;
                case "FindDialog/Title":
                    language.FindDialog.Title = reader.Value;
                    break;
                case "FindDialog/Find":
                    language.FindDialog.Find = reader.Value;
                    break;
                case "FindDialog/Normal":
                    language.FindDialog.Normal = reader.Value;
                    break;
                case "FindDialog/CaseSensitive":
                    language.FindDialog.CaseSensitive = reader.Value;
                    break;
                case "FindDialog/RegularExpression":
                    language.FindDialog.RegularExpression = reader.Value;
                    break;
                case "FindDialog/WholeWord":
                    language.FindDialog.WholeWord = reader.Value;
                    break;
                case "FindDialog/Count":
                    language.FindDialog.Count = reader.Value;
                    break;
                case "FindDialog/XNumberOfMatches":
                    language.FindDialog.XNumberOfMatches = reader.Value;
                    break;
                case "FindDialog/OneMatch":
                    language.FindDialog.OneMatch = reader.Value;
                    break;
                case "FindSubtitleLine/Title":
                    language.FindSubtitleLine.Title = reader.Value;
                    break;
                case "FindSubtitleLine/Find":
                    language.FindSubtitleLine.Find = reader.Value;
                    break;
                case "FindSubtitleLine/FindNext":
                    language.FindSubtitleLine.FindNext = reader.Value;
                    break;
                case "FixCommonErrors/Title":
                    language.FixCommonErrors.Title = reader.Value;
                    break;
                case "FixCommonErrors/Step1":
                    language.FixCommonErrors.Step1 = reader.Value;
                    break;
                case "FixCommonErrors/WhatToFix":
                    language.FixCommonErrors.WhatToFix = reader.Value;
                    break;
                case "FixCommonErrors/Example":
                    language.FixCommonErrors.Example = reader.Value;
                    break;
                case "FixCommonErrors/SelectAll":
                    language.FixCommonErrors.SelectAll = reader.Value;
                    break;
                case "FixCommonErrors/InverseSelection":
                    language.FixCommonErrors.InverseSelection = reader.Value;
                    break;
                case "FixCommonErrors/Back":
                    language.FixCommonErrors.Back = reader.Value;
                    break;
                case "FixCommonErrors/Next":
                    language.FixCommonErrors.Next = reader.Value;
                    break;
                case "FixCommonErrors/Step2":
                    language.FixCommonErrors.Step2 = reader.Value;
                    break;
                case "FixCommonErrors/Fixes":
                    language.FixCommonErrors.Fixes = reader.Value;
                    break;
                case "FixCommonErrors/Log":
                    language.FixCommonErrors.Log = reader.Value;
                    break;
                case "FixCommonErrors/Function":
                    language.FixCommonErrors.Function = reader.Value;
                    break;
                case "FixCommonErrors/RemovedEmptyLine":
                    language.FixCommonErrors.RemovedEmptyLine = reader.Value;
                    break;
                case "FixCommonErrors/RemovedEmptyLineAtTop":
                    language.FixCommonErrors.RemovedEmptyLineAtTop = reader.Value;
                    break;
                case "FixCommonErrors/RemovedEmptyLineAtBottom":
                    language.FixCommonErrors.RemovedEmptyLineAtBottom = reader.Value;
                    break;
                case "FixCommonErrors/RemovedEmptyLineInMiddle":
                    language.FixCommonErrors.RemovedEmptyLineInMiddle = reader.Value;
                    break;
                case "FixCommonErrors/RemovedEmptyLinesUnsedLineBreaks":
                    language.FixCommonErrors.RemovedEmptyLinesUnsedLineBreaks = reader.Value;
                    break;
                case "FixCommonErrors/EmptyLinesRemovedX":
                    language.FixCommonErrors.EmptyLinesRemovedX = reader.Value;
                    break;
                case "FixCommonErrors/FixOverlappingDisplayTimes":
                    language.FixCommonErrors.FixOverlappingDisplayTimes = reader.Value;
                    break;
                case "FixCommonErrors/FixShortDisplayTimes":
                    language.FixCommonErrors.FixShortDisplayTimes = reader.Value;
                    break;
                case "FixCommonErrors/FixLongDisplayTimes":
                    language.FixCommonErrors.FixLongDisplayTimes = reader.Value;
                    break;
                case "FixCommonErrors/FixShortGaps":
                    language.FixCommonErrors.FixShortGaps = reader.Value;
                    break;
                case "FixCommonErrors/FixInvalidItalicTags":
                    language.FixCommonErrors.FixInvalidItalicTags = reader.Value;
                    break;
                case "FixCommonErrors/RemoveUnneededSpaces":
                    language.FixCommonErrors.RemoveUnneededSpaces = reader.Value;
                    break;
                case "FixCommonErrors/RemoveUnneededPeriods":
                    language.FixCommonErrors.RemoveUnneededPeriods = reader.Value;
                    break;
                case "FixCommonErrors/FixMissingSpaces":
                    language.FixCommonErrors.FixMissingSpaces = reader.Value;
                    break;
                case "FixCommonErrors/BreakLongLines":
                    language.FixCommonErrors.BreakLongLines = reader.Value;
                    break;
                case "FixCommonErrors/RemoveLineBreaks":
                    language.FixCommonErrors.RemoveLineBreaks = reader.Value;
                    break;
                case "FixCommonErrors/RemoveLineBreaksAll":
                    language.FixCommonErrors.RemoveLineBreaksAll = reader.Value;
                    break;
                case "FixCommonErrors/FixUppercaseIInsindeLowercaseWords":
                    language.FixCommonErrors.FixUppercaseIInsindeLowercaseWords = reader.Value;
                    break;
                case "FixCommonErrors/FixDoubleApostrophes":
                    language.FixCommonErrors.FixDoubleApostrophes = reader.Value;
                    break;
                case "FixCommonErrors/AddPeriods":
                    language.FixCommonErrors.AddPeriods = reader.Value;
                    break;
                case "FixCommonErrors/StartWithUppercaseLetterAfterParagraph":
                    language.FixCommonErrors.StartWithUppercaseLetterAfterParagraph = reader.Value;
                    break;
                case "FixCommonErrors/StartWithUppercaseLetterAfterPeriodInsideParagraph":
                    language.FixCommonErrors.StartWithUppercaseLetterAfterPeriodInsideParagraph = reader.Value;
                    break;
                case "FixCommonErrors/StartWithUppercaseLetterAfterColon":
                    language.FixCommonErrors.StartWithUppercaseLetterAfterColon = reader.Value;
                    break;
                case "FixCommonErrors/FixLowercaseIToUppercaseI":
                    language.FixCommonErrors.FixLowercaseIToUppercaseI = reader.Value;
                    break;
                case "FixCommonErrors/FixCommonOcrErrors":
                    language.FixCommonErrors.FixCommonOcrErrors = reader.Value;
                    break;
                case "FixCommonErrors/CommonOcrErrorsFixed":
                    language.FixCommonErrors.CommonOcrErrorsFixed = reader.Value;
                    break;
                case "FixCommonErrors/RemoveSpaceBetweenNumber":
                    language.FixCommonErrors.RemoveSpaceBetweenNumber = reader.Value;
                    break;
                case "FixCommonErrors/FixDialogsOnOneLine":
                    language.FixCommonErrors.FixDialogsOnOneLine = reader.Value;
                    break;
                case "FixCommonErrors/RemoveSpaceBetweenNumbersFixed":
                    language.FixCommonErrors.RemoveSpaceBetweenNumbersFixed = reader.Value;
                    break;
                case "FixCommonErrors/FixTurkishAnsi":
                    language.FixCommonErrors.FixTurkishAnsi = reader.Value;
                    break;
                case "FixCommonErrors/FixDanishLetterI":
                    language.FixCommonErrors.FixDanishLetterI = reader.Value;
                    break;
                case "FixCommonErrors/FixSpanishInvertedQuestionAndExclamationMarks":
                    language.FixCommonErrors.FixSpanishInvertedQuestionAndExclamationMarks = reader.Value;
                    break;
                case "FixCommonErrors/AddMissingQuote":
                    language.FixCommonErrors.AddMissingQuote = reader.Value;
                    break;
                case "FixCommonErrors/AddMissingQuotes":
                    language.FixCommonErrors.AddMissingQuotes = reader.Value;
                    break;
                case "FixCommonErrors/FixHyphens":
                    language.FixCommonErrors.FixHyphens = reader.Value;
                    break;
                case "FixCommonErrors/FixHyphensAdd":
                    language.FixCommonErrors.FixHyphensAdd = reader.Value;
                    break;
                case "FixCommonErrors/FixHyphen":
                    language.FixCommonErrors.FixHyphen = reader.Value;
                    break;
                case "FixCommonErrors/XHyphensFixed":
                    language.FixCommonErrors.XHyphensFixed = reader.Value;
                    break;
                case "FixCommonErrors/AddMissingQuotesExample":
                    language.FixCommonErrors.AddMissingQuotesExample = reader.Value;
                    break;
                case "FixCommonErrors/XMissingQuotesAdded":
                    language.FixCommonErrors.XMissingQuotesAdded = reader.Value;
                    break;
                case "FixCommonErrors/Fix3PlusLines":
                    language.FixCommonErrors.Fix3PlusLines = reader.Value;
                    break;
                case "FixCommonErrors/Fix3PlusLine":
                    language.FixCommonErrors.Fix3PlusLine = reader.Value;
                    break;
                case "FixCommonErrors/X3PlusLinesFixed":
                    language.FixCommonErrors.X3PlusLinesFixed = reader.Value;
                    break;
                case "FixCommonErrors/Analysing":
                    language.FixCommonErrors.Analysing = reader.Value;
                    break;
                case "FixCommonErrors/NothingToFix":
                    language.FixCommonErrors.NothingToFix = reader.Value;
                    break;
                case "FixCommonErrors/FixesFoundX":
                    language.FixCommonErrors.FixesFoundX = reader.Value;
                    break;
                case "FixCommonErrors/XFixesApplied":
                    language.FixCommonErrors.XFixesApplied = reader.Value;
                    break;
                case "FixCommonErrors/NothingFixableBut":
                    language.FixCommonErrors.NothingFixableBut = reader.Value;
                    break;
                case "FixCommonErrors/XFixedBut":
                    language.FixCommonErrors.XFixedBut = reader.Value;
                    break;
                case "FixCommonErrors/XCouldBeFixedBut":
                    language.FixCommonErrors.XCouldBeFixedBut = reader.Value;
                    break;
                case "FixCommonErrors/UncheckedFixLowercaseIToUppercaseI":
                    language.FixCommonErrors.UncheckedFixLowercaseIToUppercaseI = reader.Value;
                    break;
                case "FixCommonErrors/XIsChangedToUppercase":
                    language.FixCommonErrors.XIsChangedToUppercase = reader.Value;
                    break;
                case "FixCommonErrors/FixFirstLetterToUppercaseAfterParagraph":
                    language.FixCommonErrors.FixFirstLetterToUppercaseAfterParagraph = reader.Value;
                    break;
                case "FixCommonErrors/MergeShortLine":
                    language.FixCommonErrors.MergeShortLine = reader.Value;
                    break;
                case "FixCommonErrors/MergeShortLineAll":
                    language.FixCommonErrors.MergeShortLineAll = reader.Value;
                    break;
                case "FixCommonErrors/XLineBreaksAdded":
                    language.FixCommonErrors.XLineBreaksAdded = reader.Value;
                    break;
                case "FixCommonErrors/BreakLongLine":
                    language.FixCommonErrors.BreakLongLine = reader.Value;
                    break;
                case "FixCommonErrors/FixLongDisplayTime":
                    language.FixCommonErrors.FixLongDisplayTime = reader.Value;
                    break;
                case "FixCommonErrors/FixInvalidItalicTag":
                    language.FixCommonErrors.FixInvalidItalicTag = reader.Value;
                    break;
                case "FixCommonErrors/FixShortDisplayTime":
                    language.FixCommonErrors.FixShortDisplayTime = reader.Value;
                    break;
                case "FixCommonErrors/FixOverlappingDisplayTime":
                    language.FixCommonErrors.FixOverlappingDisplayTime = reader.Value;
                    break;
                case "FixCommonErrors/FixShortGap":
                    language.FixCommonErrors.FixShortGap = reader.Value;
                    break;
                case "FixCommonErrors/FixInvalidItalicTagsExample":
                    language.FixCommonErrors.FixInvalidItalicTagsExample = reader.Value;
                    break;
                case "FixCommonErrors/RemoveUnneededSpacesExample":
                    language.FixCommonErrors.RemoveUnneededSpacesExample = reader.Value;
                    break;
                case "FixCommonErrors/RemoveUnneededPeriodsExample":
                    language.FixCommonErrors.RemoveUnneededPeriodsExample = reader.Value;
                    break;
                case "FixCommonErrors/FixMissingSpacesExample":
                    language.FixCommonErrors.FixMissingSpacesExample = reader.Value;
                    break;
                case "FixCommonErrors/FixUppercaseIInsindeLowercaseWordsExample":
                    language.FixCommonErrors.FixUppercaseIInsindeLowercaseWordsExample = reader.Value;
                    break;
                case "FixCommonErrors/FixLowercaseIToUppercaseIExample":
                    language.FixCommonErrors.FixLowercaseIToUppercaseIExample = reader.Value;
                    break;
                case "FixCommonErrors/StartTimeLaterThanEndTime":
                    language.FixCommonErrors.StartTimeLaterThanEndTime = reader.Value;
                    break;
                case "FixCommonErrors/UnableToFixStartTimeLaterThanEndTime":
                    language.FixCommonErrors.UnableToFixStartTimeLaterThanEndTime = reader.Value;
                    break;
                case "FixCommonErrors/XFixedToYZ":
                    language.FixCommonErrors.XFixedToYZ = reader.Value;
                    break;
                case "FixCommonErrors/UnableToFixTextXY":
                    language.FixCommonErrors.UnableToFixTextXY = reader.Value;
                    break;
                case "FixCommonErrors/XOverlappingTimestampsFixed":
                    language.FixCommonErrors.XOverlappingTimestampsFixed = reader.Value;
                    break;
                case "FixCommonErrors/XDisplayTimesProlonged":
                    language.FixCommonErrors.XDisplayTimesProlonged = reader.Value;
                    break;
                case "FixCommonErrors/XInvalidHtmlTagsFixed":
                    language.FixCommonErrors.XInvalidHtmlTagsFixed = reader.Value;
                    break;
                case "FixCommonErrors/XDisplayTimesShortned":
                    language.FixCommonErrors.XDisplayTimesShortned = reader.Value;
                    break;
                case "FixCommonErrors/XGapsFixed":
                    language.FixCommonErrors.XGapsFixed = reader.Value;
                    break;
                case "FixCommonErrors/XLinesUnbreaked":
                    language.FixCommonErrors.XLinesUnbreaked = reader.Value;
                    break;
                case "FixCommonErrors/UnneededSpace":
                    language.FixCommonErrors.UnneededSpace = reader.Value;
                    break;
                case "FixCommonErrors/XUnneededSpacesRemoved":
                    language.FixCommonErrors.XUnneededSpacesRemoved = reader.Value;
                    break;
                case "FixCommonErrors/UnneededPeriod":
                    language.FixCommonErrors.UnneededPeriod = reader.Value;
                    break;
                case "FixCommonErrors/XUnneededPeriodsRemoved":
                    language.FixCommonErrors.XUnneededPeriodsRemoved = reader.Value;
                    break;
                case "FixCommonErrors/FixMissingSpace":
                    language.FixCommonErrors.FixMissingSpace = reader.Value;
                    break;
                case "FixCommonErrors/XMissingSpacesAdded":
                    language.FixCommonErrors.XMissingSpacesAdded = reader.Value;
                    break;
                case "FixCommonErrors/FixUppercaseIInsideLowercaseWord":
                    language.FixCommonErrors.FixUppercaseIInsideLowercaseWord = reader.Value;
                    break;
                case "FixCommonErrors/XPeriodsAdded":
                    language.FixCommonErrors.XPeriodsAdded = reader.Value;
                    break;
                case "FixCommonErrors/FixMissingPeriodAtEndOfLine":
                    language.FixCommonErrors.FixMissingPeriodAtEndOfLine = reader.Value;
                    break;
                case "FixCommonErrors/XDoubleApostrophesFixed":
                    language.FixCommonErrors.XDoubleApostrophesFixed = reader.Value;
                    break;
                case "FixCommonErrors/XUppercaseIsFoundInsideLowercaseWords":
                    language.FixCommonErrors.XUppercaseIsFoundInsideLowercaseWords = reader.Value;
                    break;
                case "FixCommonErrors/RefreshFixes":
                    language.FixCommonErrors.RefreshFixes = reader.Value;
                    break;
                case "FixCommonErrors/ApplyFixes":
                    language.FixCommonErrors.ApplyFixes = reader.Value;
                    break;
                case "FixCommonErrors/AutoBreak":
                    language.FixCommonErrors.AutoBreak = reader.Value;
                    break;
                case "FixCommonErrors/Unbreak":
                    language.FixCommonErrors.Unbreak = reader.Value;
                    break;
                case "FixCommonErrors/FixDoubleDash":
                    language.FixCommonErrors.FixDoubleDash = reader.Value;
                    break;
                case "FixCommonErrors/FixDoubleGreaterThan":
                    language.FixCommonErrors.FixDoubleGreaterThan = reader.Value;
                    break;
                case "FixCommonErrors/FixEllipsesStart":
                    language.FixCommonErrors.FixEllipsesStart = reader.Value;
                    break;
                case "FixCommonErrors/FixMissingOpenBracket":
                    language.FixCommonErrors.FixMissingOpenBracket = reader.Value;
                    break;
                case "FixCommonErrors/FixMusicNotation":
                    language.FixCommonErrors.FixMusicNotation = reader.Value;
                    break;
                case "FixCommonErrors/XFixDoubleDash":
                    language.FixCommonErrors.XFixDoubleDash = reader.Value;
                    break;
                case "FixCommonErrors/XFixDoubleGreaterThan":
                    language.FixCommonErrors.XFixDoubleGreaterThan = reader.Value;
                    break;
                case "FixCommonErrors/XFixEllipsesStart":
                    language.FixCommonErrors.XFixEllipsesStart = reader.Value;
                    break;
                case "FixCommonErrors/XFixMissingOpenBracket":
                    language.FixCommonErrors.XFixMissingOpenBracket = reader.Value;
                    break;
                case "FixCommonErrors/XFixMusicNotation":
                    language.FixCommonErrors.XFixMusicNotation = reader.Value;
                    break;
                case "FixCommonErrors/FixDoubleDashExample":
                    language.FixCommonErrors.FixDoubleDashExample = reader.Value;
                    break;
                case "FixCommonErrors/FixDoubleGreaterThanExample":
                    language.FixCommonErrors.FixDoubleGreaterThanExample = reader.Value;
                    break;
                case "FixCommonErrors/FixEllipsesStartExample":
                    language.FixCommonErrors.FixEllipsesStartExample = reader.Value;
                    break;
                case "FixCommonErrors/FixMissingOpenBracketExample":
                    language.FixCommonErrors.FixMissingOpenBracketExample = reader.Value;
                    break;
                case "FixCommonErrors/FixMusicNotationExample":
                    language.FixCommonErrors.FixMusicNotationExample = reader.Value;
                    break;
                case "FixCommonErrors/NumberOfImportantLogMessages":
                    language.FixCommonErrors.NumberOfImportantLogMessages = reader.Value;
                    break;
                case "FixCommonErrors/FixedOkXY":
                    language.FixCommonErrors.FixedOkXY = reader.Value;
                    break;
                case "FixCommonErrors/FixOcrErrorExample":
                    language.FixCommonErrors.FixOcrErrorExample = reader.Value;
                    break;
                case "FixCommonErrors/FixSpaceBetweenNumbersExample":
                    language.FixCommonErrors.FixSpaceBetweenNumbersExample = reader.Value;
                    break;
                case "FixCommonErrors/FixDialogsOneLineExample":
                    language.FixCommonErrors.FixDialogsOneLineExample = reader.Value;
                    break;
                case "FixCommonErrors/SelectDefault":
                    language.FixCommonErrors.SelectDefault = reader.Value;
                    break;
                case "GetDictionaries/Title":
                    language.GetDictionaries.Title = reader.Value;
                    break;
                case "GetDictionaries/DescriptionLine1":
                    language.GetDictionaries.DescriptionLine1 = reader.Value;
                    break;
                case "GetDictionaries/DescriptionLine2":
                    language.GetDictionaries.DescriptionLine2 = reader.Value;
                    break;
                case "GetDictionaries/ChooseLanguageAndClickDownload":
                    language.GetDictionaries.ChooseLanguageAndClickDownload = reader.Value;
                    break;
                case "GetDictionaries/OpenDictionariesFolder":
                    language.GetDictionaries.OpenDictionariesFolder = reader.Value;
                    break;
                case "GetDictionaries/Download":
                    language.GetDictionaries.Download = reader.Value;
                    break;
                case "GetDictionaries/XDownloaded":
                    language.GetDictionaries.XDownloaded = reader.Value;
                    break;
                case "GetTesseractDictionaries/Title":
                    language.GetTesseractDictionaries.Title = reader.Value;
                    break;
                case "GetTesseractDictionaries/DescriptionLine1":
                    language.GetTesseractDictionaries.DescriptionLine1 = reader.Value;
                    break;
                case "GetTesseractDictionaries/DownloadFailed":
                    language.GetTesseractDictionaries.DownloadFailed = reader.Value;
                    break;
                case "GetTesseractDictionaries/GetDictionariesHere":
                    language.GetTesseractDictionaries.GetDictionariesHere = reader.Value;
                    break;
                case "GetTesseractDictionaries/ChooseLanguageAndClickDownload":
                    language.GetTesseractDictionaries.ChooseLanguageAndClickDownload = reader.Value;
                    break;
                case "GetTesseractDictionaries/OpenDictionariesFolder":
                    language.GetTesseractDictionaries.OpenDictionariesFolder = reader.Value;
                    break;
                case "GetTesseractDictionaries/Download":
                    language.GetTesseractDictionaries.Download = reader.Value;
                    break;
                case "GetTesseractDictionaries/XDownloaded":
                    language.GetTesseractDictionaries.XDownloaded = reader.Value;
                    break;
                case "GoogleTranslate/Title":
                    language.GoogleTranslate.Title = reader.Value;
                    break;
                case "GoogleTranslate/From":
                    language.GoogleTranslate.From = reader.Value;
                    break;
                case "GoogleTranslate/To":
                    language.GoogleTranslate.To = reader.Value;
                    break;
                case "GoogleTranslate/Translate":
                    language.GoogleTranslate.Translate = reader.Value;
                    break;
                case "GoogleTranslate/PleaseWait":
                    language.GoogleTranslate.PleaseWait = reader.Value;
                    break;
                case "GoogleTranslate/PoweredByGoogleTranslate":
                    language.GoogleTranslate.PoweredByGoogleTranslate = reader.Value;
                    break;
                case "GoogleTranslate/PoweredByMicrosoftTranslate":
                    language.GoogleTranslate.PoweredByMicrosoftTranslate = reader.Value;
                    break;
                case "GoogleTranslate/MsClientSecretNeeded":
                    language.GoogleTranslate.MsClientSecretNeeded = reader.Value;
                    break;
                case "GoogleTranslate/GoogleApiKeyNeeded":
                    language.GoogleTranslate.GoogleApiKeyNeeded = reader.Value;
                    break;
                case "GoogleTranslate/GoogleNoApiKeyWarning":
                    language.GoogleTranslate.GoogleNoApiKeyWarning = reader.Value;
                    break;
                case "GoogleOrMicrosoftTranslate/Title":
                    language.GoogleOrMicrosoftTranslate.Title = reader.Value;
                    break;
                case "GoogleOrMicrosoftTranslate/From":
                    language.GoogleOrMicrosoftTranslate.From = reader.Value;
                    break;
                case "GoogleOrMicrosoftTranslate/To":
                    language.GoogleOrMicrosoftTranslate.To = reader.Value;
                    break;
                case "GoogleOrMicrosoftTranslate/Translate":
                    language.GoogleOrMicrosoftTranslate.Translate = reader.Value;
                    break;
                case "GoogleOrMicrosoftTranslate/SourceText":
                    language.GoogleOrMicrosoftTranslate.SourceText = reader.Value;
                    break;
                case "GoogleOrMicrosoftTranslate/GoogleTranslate":
                    language.GoogleOrMicrosoftTranslate.GoogleTranslate = reader.Value;
                    break;
                case "GoogleOrMicrosoftTranslate/MicrosoftTranslate":
                    language.GoogleOrMicrosoftTranslate.MicrosoftTranslate = reader.Value;
                    break;
                case "GoToLine/Title":
                    language.GoToLine.Title = reader.Value;
                    break;
                case "GoToLine/XIsNotAValidNumber":
                    language.GoToLine.XIsNotAValidNumber = reader.Value;
                    break;
                case "ImportImages/Title":
                    language.ImportImages.Title = reader.Value;
                    break;
                case "ImportImages/ImageFiles":
                    language.ImportImages.ImageFiles = reader.Value;
                    break;
                case "ImportImages/Input":
                    language.ImportImages.Input = reader.Value;
                    break;
                case "ImportImages/InputDescription":
                    language.ImportImages.InputDescription = reader.Value;
                    break;
                case "ImportImages/Remove":
                    language.ImportImages.Remove = reader.Value;
                    break;
                case "ImportImages/RemoveAll":
                    language.ImportImages.RemoveAll = reader.Value;
                    break;
                case "ImportSceneChanges/Title":
                    language.ImportSceneChanges.Title = reader.Value;
                    break;
                case "ImportSceneChanges/OpenTextFile":
                    language.ImportSceneChanges.OpenTextFile = reader.Value;
                    break;
                case "ImportSceneChanges/Generate":
                    language.ImportSceneChanges.Generate = reader.Value;
                    break;
                case "ImportSceneChanges/Import":
                    language.ImportSceneChanges.Import = reader.Value;
                    break;
                case "ImportSceneChanges/TextFiles":
                    language.ImportSceneChanges.TextFiles = reader.Value;
                    break;
                case "ImportSceneChanges/TimeCodes":
                    language.ImportSceneChanges.TimeCodes = reader.Value;
                    break;
                case "ImportSceneChanges/Frames":
                    language.ImportSceneChanges.Frames = reader.Value;
                    break;
                case "ImportSceneChanges/Seconds":
                    language.ImportSceneChanges.Seconds = reader.Value;
                    break;
                case "ImportSceneChanges/Milliseconds":
                    language.ImportSceneChanges.Milliseconds = reader.Value;
                    break;
                case "ImportSceneChanges/GetSceneChangesWithFfmpeg":
                    language.ImportSceneChanges.GetSceneChangesWithFfmpeg = reader.Value;
                    break;
                case "ImportSceneChanges/Sensitivity":
                    language.ImportSceneChanges.Sensitivity = reader.Value;
                    break;
                case "ImportSceneChanges/SensitivityDescription":
                    language.ImportSceneChanges.SensitivityDescription = reader.Value;
                    break;
                case "ImportSceneChanges/NoSceneChangesFound":
                    language.ImportSceneChanges.NoSceneChangesFound = reader.Value;
                    break;
                case "ImportText/Title":
                    language.ImportText.Title = reader.Value;
                    break;
                case "ImportText/OneSubtitleIsOneFile":
                    language.ImportText.OneSubtitleIsOneFile = reader.Value;
                    break;
                case "ImportText/OpenTextFile":
                    language.ImportText.OpenTextFile = reader.Value;
                    break;
                case "ImportText/OpenTextFiles":
                    language.ImportText.OpenTextFiles = reader.Value;
                    break;
                case "ImportText/ImportOptions":
                    language.ImportText.ImportOptions = reader.Value;
                    break;
                case "ImportText/Splitting":
                    language.ImportText.Splitting = reader.Value;
                    break;
                case "ImportText/AutoSplitText":
                    language.ImportText.AutoSplitText = reader.Value;
                    break;
                case "ImportText/OneLineIsOneSubtitle":
                    language.ImportText.OneLineIsOneSubtitle = reader.Value;
                    break;
                case "ImportText/TwoLinesAreOneSubtitle":
                    language.ImportText.TwoLinesAreOneSubtitle = reader.Value;
                    break;
                case "ImportText/LineBreak":
                    language.ImportText.LineBreak = reader.Value;
                    break;
                case "ImportText/SplitAtBlankLines":
                    language.ImportText.SplitAtBlankLines = reader.Value;
                    break;
                case "ImportText/MergeShortLines":
                    language.ImportText.MergeShortLines = reader.Value;
                    break;
                case "ImportText/RemoveEmptyLines":
                    language.ImportText.RemoveEmptyLines = reader.Value;
                    break;
                case "ImportText/RemoveLinesWithoutLetters":
                    language.ImportText.RemoveLinesWithoutLetters = reader.Value;
                    break;
                case "ImportText/GenerateTimeCodes":
                    language.ImportText.GenerateTimeCodes = reader.Value;
                    break;
                case "ImportText/GapBetweenSubtitles":
                    language.ImportText.GapBetweenSubtitles = reader.Value;
                    break;
                case "ImportText/Auto":
                    language.ImportText.Auto = reader.Value;
                    break;
                case "ImportText/Fixed":
                    language.ImportText.Fixed = reader.Value;
                    break;
                case "ImportText/Refresh":
                    language.ImportText.Refresh = reader.Value;
                    break;
                case "ImportText/TextFiles":
                    language.ImportText.TextFiles = reader.Value;
                    break;
                case "ImportText/PreviewLinesModifiedX":
                    language.ImportText.PreviewLinesModifiedX = reader.Value;
                    break;
                case "ImportText/TimeCodes":
                    language.ImportText.TimeCodes = reader.Value;
                    break;
                case "ImportText/SplitAtEndChars":
                    language.ImportText.SplitAtEndChars = reader.Value;
                    break;
                case "Interjections/Title":
                    language.Interjections.Title = reader.Value;
                    break;
                case "JoinSubtitles/Title":
                    language.JoinSubtitles.Title = reader.Value;
                    break;
                case "JoinSubtitles/Information":
                    language.JoinSubtitles.Information = reader.Value;
                    break;
                case "JoinSubtitles/NumberOfLines":
                    language.JoinSubtitles.NumberOfLines = reader.Value;
                    break;
                case "JoinSubtitles/StartTime":
                    language.JoinSubtitles.StartTime = reader.Value;
                    break;
                case "JoinSubtitles/EndTime":
                    language.JoinSubtitles.EndTime = reader.Value;
                    break;
                case "JoinSubtitles/FileName":
                    language.JoinSubtitles.FileName = reader.Value;
                    break;
                case "JoinSubtitles/Join":
                    language.JoinSubtitles.Join = reader.Value;
                    break;
                case "JoinSubtitles/TotalNumberOfLinesX":
                    language.JoinSubtitles.TotalNumberOfLinesX = reader.Value;
                    break;
                case "JoinSubtitles/AlreadyCorrectTimeCodes":
                    language.JoinSubtitles.AlreadyCorrectTimeCodes = reader.Value;
                    break;
                case "JoinSubtitles/AppendTimeCodes":
                    language.JoinSubtitles.AppendTimeCodes = reader.Value;
                    break;
                case "JoinSubtitles/AddMs":
                    language.JoinSubtitles.AddMs = reader.Value;
                    break;
                case "LanguageNames/NotSpecified":
                    language.LanguageNames.NotSpecified = reader.Value;
                    break;
                case "LanguageNames/UnknownCodeX":
                    language.LanguageNames.UnknownCodeX = reader.Value;
                    break;
                case "LanguageNames/aaName":
                    language.LanguageNames.aaName = reader.Value;
                    break;
                case "LanguageNames/abName":
                    language.LanguageNames.abName = reader.Value;
                    break;
                case "LanguageNames/afName":
                    language.LanguageNames.afName = reader.Value;
                    break;
                case "LanguageNames/amName":
                    language.LanguageNames.amName = reader.Value;
                    break;
                case "LanguageNames/arName":
                    language.LanguageNames.arName = reader.Value;
                    break;
                case "LanguageNames/asName":
                    language.LanguageNames.asName = reader.Value;
                    break;
                case "LanguageNames/ayName":
                    language.LanguageNames.ayName = reader.Value;
                    break;
                case "LanguageNames/azName":
                    language.LanguageNames.azName = reader.Value;
                    break;
                case "LanguageNames/baName":
                    language.LanguageNames.baName = reader.Value;
                    break;
                case "LanguageNames/beName":
                    language.LanguageNames.beName = reader.Value;
                    break;
                case "LanguageNames/bgName":
                    language.LanguageNames.bgName = reader.Value;
                    break;
                case "LanguageNames/bhName":
                    language.LanguageNames.bhName = reader.Value;
                    break;
                case "LanguageNames/biName":
                    language.LanguageNames.biName = reader.Value;
                    break;
                case "LanguageNames/bnName":
                    language.LanguageNames.bnName = reader.Value;
                    break;
                case "LanguageNames/boName":
                    language.LanguageNames.boName = reader.Value;
                    break;
                case "LanguageNames/brName":
                    language.LanguageNames.brName = reader.Value;
                    break;
                case "LanguageNames/caName":
                    language.LanguageNames.caName = reader.Value;
                    break;
                case "LanguageNames/coName":
                    language.LanguageNames.coName = reader.Value;
                    break;
                case "LanguageNames/csName":
                    language.LanguageNames.csName = reader.Value;
                    break;
                case "LanguageNames/cyName":
                    language.LanguageNames.cyName = reader.Value;
                    break;
                case "LanguageNames/daName":
                    language.LanguageNames.daName = reader.Value;
                    break;
                case "LanguageNames/deName":
                    language.LanguageNames.deName = reader.Value;
                    break;
                case "LanguageNames/dzName":
                    language.LanguageNames.dzName = reader.Value;
                    break;
                case "LanguageNames/elName":
                    language.LanguageNames.elName = reader.Value;
                    break;
                case "LanguageNames/enName":
                    language.LanguageNames.enName = reader.Value;
                    break;
                case "LanguageNames/eoName":
                    language.LanguageNames.eoName = reader.Value;
                    break;
                case "LanguageNames/esName":
                    language.LanguageNames.esName = reader.Value;
                    break;
                case "LanguageNames/etName":
                    language.LanguageNames.etName = reader.Value;
                    break;
                case "LanguageNames/euName":
                    language.LanguageNames.euName = reader.Value;
                    break;
                case "LanguageNames/faName":
                    language.LanguageNames.faName = reader.Value;
                    break;
                case "LanguageNames/fiName":
                    language.LanguageNames.fiName = reader.Value;
                    break;
                case "LanguageNames/fjName":
                    language.LanguageNames.fjName = reader.Value;
                    break;
                case "LanguageNames/foName":
                    language.LanguageNames.foName = reader.Value;
                    break;
                case "LanguageNames/frName":
                    language.LanguageNames.frName = reader.Value;
                    break;
                case "LanguageNames/fyName":
                    language.LanguageNames.fyName = reader.Value;
                    break;
                case "LanguageNames/gaName":
                    language.LanguageNames.gaName = reader.Value;
                    break;
                case "LanguageNames/gdName":
                    language.LanguageNames.gdName = reader.Value;
                    break;
                case "LanguageNames/glName":
                    language.LanguageNames.glName = reader.Value;
                    break;
                case "LanguageNames/gnName":
                    language.LanguageNames.gnName = reader.Value;
                    break;
                case "LanguageNames/guName":
                    language.LanguageNames.guName = reader.Value;
                    break;
                case "LanguageNames/haName":
                    language.LanguageNames.haName = reader.Value;
                    break;
                case "LanguageNames/heName":
                    language.LanguageNames.heName = reader.Value;
                    break;
                case "LanguageNames/hiName":
                    language.LanguageNames.hiName = reader.Value;
                    break;
                case "LanguageNames/hrName":
                    language.LanguageNames.hrName = reader.Value;
                    break;
                case "LanguageNames/huName":
                    language.LanguageNames.huName = reader.Value;
                    break;
                case "LanguageNames/hyName":
                    language.LanguageNames.hyName = reader.Value;
                    break;
                case "LanguageNames/iaName":
                    language.LanguageNames.iaName = reader.Value;
                    break;
                case "LanguageNames/idName":
                    language.LanguageNames.idName = reader.Value;
                    break;
                case "LanguageNames/ieName":
                    language.LanguageNames.ieName = reader.Value;
                    break;
                case "LanguageNames/ikName":
                    language.LanguageNames.ikName = reader.Value;
                    break;
                case "LanguageNames/isName":
                    language.LanguageNames.isName = reader.Value;
                    break;
                case "LanguageNames/itName":
                    language.LanguageNames.itName = reader.Value;
                    break;
                case "LanguageNames/iuName":
                    language.LanguageNames.iuName = reader.Value;
                    break;
                case "LanguageNames/jaName":
                    language.LanguageNames.jaName = reader.Value;
                    break;
                case "LanguageNames/jvName":
                    language.LanguageNames.jvName = reader.Value;
                    break;
                case "LanguageNames/kaName":
                    language.LanguageNames.kaName = reader.Value;
                    break;
                case "LanguageNames/kkName":
                    language.LanguageNames.kkName = reader.Value;
                    break;
                case "LanguageNames/klName":
                    language.LanguageNames.klName = reader.Value;
                    break;
                case "LanguageNames/kmName":
                    language.LanguageNames.kmName = reader.Value;
                    break;
                case "LanguageNames/knName":
                    language.LanguageNames.knName = reader.Value;
                    break;
                case "LanguageNames/koName":
                    language.LanguageNames.koName = reader.Value;
                    break;
                case "LanguageNames/ksName":
                    language.LanguageNames.ksName = reader.Value;
                    break;
                case "LanguageNames/kuName":
                    language.LanguageNames.kuName = reader.Value;
                    break;
                case "LanguageNames/kyName":
                    language.LanguageNames.kyName = reader.Value;
                    break;
                case "LanguageNames/laName":
                    language.LanguageNames.laName = reader.Value;
                    break;
                case "LanguageNames/lbName":
                    language.LanguageNames.lbName = reader.Value;
                    break;
                case "LanguageNames/lnName":
                    language.LanguageNames.lnName = reader.Value;
                    break;
                case "LanguageNames/loName":
                    language.LanguageNames.loName = reader.Value;
                    break;
                case "LanguageNames/ltName":
                    language.LanguageNames.ltName = reader.Value;
                    break;
                case "LanguageNames/lvName":
                    language.LanguageNames.lvName = reader.Value;
                    break;
                case "LanguageNames/mgName":
                    language.LanguageNames.mgName = reader.Value;
                    break;
                case "LanguageNames/miName":
                    language.LanguageNames.miName = reader.Value;
                    break;
                case "LanguageNames/mkName":
                    language.LanguageNames.mkName = reader.Value;
                    break;
                case "LanguageNames/mlName":
                    language.LanguageNames.mlName = reader.Value;
                    break;
                case "LanguageNames/mnName":
                    language.LanguageNames.mnName = reader.Value;
                    break;
                case "LanguageNames/moName":
                    language.LanguageNames.moName = reader.Value;
                    break;
                case "LanguageNames/mrName":
                    language.LanguageNames.mrName = reader.Value;
                    break;
                case "LanguageNames/msName":
                    language.LanguageNames.msName = reader.Value;
                    break;
                case "LanguageNames/mtName":
                    language.LanguageNames.mtName = reader.Value;
                    break;
                case "LanguageNames/myName":
                    language.LanguageNames.myName = reader.Value;
                    break;
                case "LanguageNames/naName":
                    language.LanguageNames.naName = reader.Value;
                    break;
                case "LanguageNames/neName":
                    language.LanguageNames.neName = reader.Value;
                    break;
                case "LanguageNames/nlName":
                    language.LanguageNames.nlName = reader.Value;
                    break;
                case "LanguageNames/noName":
                    language.LanguageNames.noName = reader.Value;
                    break;
                case "LanguageNames/ocName":
                    language.LanguageNames.ocName = reader.Value;
                    break;
                case "LanguageNames/omName":
                    language.LanguageNames.omName = reader.Value;
                    break;
                case "LanguageNames/orName":
                    language.LanguageNames.orName = reader.Value;
                    break;
                case "LanguageNames/paName":
                    language.LanguageNames.paName = reader.Value;
                    break;
                case "LanguageNames/plName":
                    language.LanguageNames.plName = reader.Value;
                    break;
                case "LanguageNames/psName":
                    language.LanguageNames.psName = reader.Value;
                    break;
                case "LanguageNames/ptName":
                    language.LanguageNames.ptName = reader.Value;
                    break;
                case "LanguageNames/quName":
                    language.LanguageNames.quName = reader.Value;
                    break;
                case "LanguageNames/rmName":
                    language.LanguageNames.rmName = reader.Value;
                    break;
                case "LanguageNames/rnName":
                    language.LanguageNames.rnName = reader.Value;
                    break;
                case "LanguageNames/roName":
                    language.LanguageNames.roName = reader.Value;
                    break;
                case "LanguageNames/ruName":
                    language.LanguageNames.ruName = reader.Value;
                    break;
                case "LanguageNames/rwName":
                    language.LanguageNames.rwName = reader.Value;
                    break;
                case "LanguageNames/saName":
                    language.LanguageNames.saName = reader.Value;
                    break;
                case "LanguageNames/sdName":
                    language.LanguageNames.sdName = reader.Value;
                    break;
                case "LanguageNames/sgName":
                    language.LanguageNames.sgName = reader.Value;
                    break;
                case "LanguageNames/shName":
                    language.LanguageNames.shName = reader.Value;
                    break;
                case "LanguageNames/siName":
                    language.LanguageNames.siName = reader.Value;
                    break;
                case "LanguageNames/skName":
                    language.LanguageNames.skName = reader.Value;
                    break;
                case "LanguageNames/slName":
                    language.LanguageNames.slName = reader.Value;
                    break;
                case "LanguageNames/smName":
                    language.LanguageNames.smName = reader.Value;
                    break;
                case "LanguageNames/snName":
                    language.LanguageNames.snName = reader.Value;
                    break;
                case "LanguageNames/soName":
                    language.LanguageNames.soName = reader.Value;
                    break;
                case "LanguageNames/sqName":
                    language.LanguageNames.sqName = reader.Value;
                    break;
                case "LanguageNames/srName":
                    language.LanguageNames.srName = reader.Value;
                    break;
                case "LanguageNames/ssName":
                    language.LanguageNames.ssName = reader.Value;
                    break;
                case "LanguageNames/stName":
                    language.LanguageNames.stName = reader.Value;
                    break;
                case "LanguageNames/suName":
                    language.LanguageNames.suName = reader.Value;
                    break;
                case "LanguageNames/svName":
                    language.LanguageNames.svName = reader.Value;
                    break;
                case "LanguageNames/swName":
                    language.LanguageNames.swName = reader.Value;
                    break;
                case "LanguageNames/taName":
                    language.LanguageNames.taName = reader.Value;
                    break;
                case "LanguageNames/teName":
                    language.LanguageNames.teName = reader.Value;
                    break;
                case "LanguageNames/tgName":
                    language.LanguageNames.tgName = reader.Value;
                    break;
                case "LanguageNames/thName":
                    language.LanguageNames.thName = reader.Value;
                    break;
                case "LanguageNames/tiName":
                    language.LanguageNames.tiName = reader.Value;
                    break;
                case "LanguageNames/tkName":
                    language.LanguageNames.tkName = reader.Value;
                    break;
                case "LanguageNames/tlName":
                    language.LanguageNames.tlName = reader.Value;
                    break;
                case "LanguageNames/tnName":
                    language.LanguageNames.tnName = reader.Value;
                    break;
                case "LanguageNames/toName":
                    language.LanguageNames.toName = reader.Value;
                    break;
                case "LanguageNames/trName":
                    language.LanguageNames.trName = reader.Value;
                    break;
                case "LanguageNames/tsName":
                    language.LanguageNames.tsName = reader.Value;
                    break;
                case "LanguageNames/ttName":
                    language.LanguageNames.ttName = reader.Value;
                    break;
                case "LanguageNames/twName":
                    language.LanguageNames.twName = reader.Value;
                    break;
                case "LanguageNames/ugName":
                    language.LanguageNames.ugName = reader.Value;
                    break;
                case "LanguageNames/ukName":
                    language.LanguageNames.ukName = reader.Value;
                    break;
                case "LanguageNames/urName":
                    language.LanguageNames.urName = reader.Value;
                    break;
                case "LanguageNames/uzName":
                    language.LanguageNames.uzName = reader.Value;
                    break;
                case "LanguageNames/viName":
                    language.LanguageNames.viName = reader.Value;
                    break;
                case "LanguageNames/voName":
                    language.LanguageNames.voName = reader.Value;
                    break;
                case "LanguageNames/woName":
                    language.LanguageNames.woName = reader.Value;
                    break;
                case "LanguageNames/xhName":
                    language.LanguageNames.xhName = reader.Value;
                    break;
                case "LanguageNames/yiName":
                    language.LanguageNames.yiName = reader.Value;
                    break;
                case "LanguageNames/yoName":
                    language.LanguageNames.yoName = reader.Value;
                    break;
                case "LanguageNames/zaName":
                    language.LanguageNames.zaName = reader.Value;
                    break;
                case "LanguageNames/zhName":
                    language.LanguageNames.zhName = reader.Value;
                    break;
                case "LanguageNames/zuName":
                    language.LanguageNames.zuName = reader.Value;
                    break;
                case "Main/SaveChangesToUntitled":
                    language.Main.SaveChangesToUntitled = reader.Value;
                    break;
                case "Main/SaveChangesToX":
                    language.Main.SaveChangesToX = reader.Value;
                    break;
                case "Main/SaveChangesToUntitledOriginal":
                    language.Main.SaveChangesToUntitledOriginal = reader.Value;
                    break;
                case "Main/SaveChangesToOriginalX":
                    language.Main.SaveChangesToOriginalX = reader.Value;
                    break;
                case "Main/SaveSubtitleAs":
                    language.Main.SaveSubtitleAs = reader.Value;
                    break;
                case "Main/SaveOriginalSubtitleAs":
                    language.Main.SaveOriginalSubtitleAs = reader.Value;
                    break;
                case "Main/NoSubtitleLoaded":
                    language.Main.NoSubtitleLoaded = reader.Value;
                    break;
                case "Main/VisualSyncSelectedLines":
                    language.Main.VisualSyncSelectedLines = reader.Value;
                    break;
                case "Main/VisualSyncTitle":
                    language.Main.VisualSyncTitle = reader.Value;
                    break;
                case "Main/BeforeVisualSync":
                    language.Main.BeforeVisualSync = reader.Value;
                    break;
                case "Main/VisualSyncPerformedOnSelectedLines":
                    language.Main.VisualSyncPerformedOnSelectedLines = reader.Value;
                    break;
                case "Main/VisualSyncPerformed":
                    language.Main.VisualSyncPerformed = reader.Value;
                    break;
                case "Main/FileXIsLargerThan10MB":
                    language.Main.FileXIsLargerThan10MB = reader.Value;
                    break;
                case "Main/ContinueAnyway":
                    language.Main.ContinueAnyway = reader.Value;
                    break;
                case "Main/BeforeLoadOf":
                    language.Main.BeforeLoadOf = reader.Value;
                    break;
                case "Main/LoadedSubtitleX":
                    language.Main.LoadedSubtitleX = reader.Value;
                    break;
                case "Main/LoadedEmptyOrShort":
                    language.Main.LoadedEmptyOrShort = reader.Value;
                    break;
                case "Main/FileIsEmptyOrShort":
                    language.Main.FileIsEmptyOrShort = reader.Value;
                    break;
                case "Main/FileNotFound":
                    language.Main.FileNotFound = reader.Value;
                    break;
                case "Main/SavedSubtitleX":
                    language.Main.SavedSubtitleX = reader.Value;
                    break;
                case "Main/SavedOriginalSubtitleX":
                    language.Main.SavedOriginalSubtitleX = reader.Value;
                    break;
                case "Main/FileOnDiskModified":
                    language.Main.FileOnDiskModified = reader.Value;
                    break;
                case "Main/OverwriteModifiedFile":
                    language.Main.OverwriteModifiedFile = reader.Value;
                    break;
                case "Main/FileXIsReadOnly":
                    language.Main.FileXIsReadOnly = reader.Value;
                    break;
                case "Main/UnableToSaveSubtitleX":
                    language.Main.UnableToSaveSubtitleX = reader.Value;
                    break;
                case "Main/BeforeNew":
                    language.Main.BeforeNew = reader.Value;
                    break;
                case "Main/New":
                    language.Main.New = reader.Value;
                    break;
                case "Main/BeforeConvertingToX":
                    language.Main.BeforeConvertingToX = reader.Value;
                    break;
                case "Main/ConvertedToX":
                    language.Main.ConvertedToX = reader.Value;
                    break;
                case "Main/BeforeShowEarlier":
                    language.Main.BeforeShowEarlier = reader.Value;
                    break;
                case "Main/BeforeShowLater":
                    language.Main.BeforeShowLater = reader.Value;
                    break;
                case "Main/LineNumberX":
                    language.Main.LineNumberX = reader.Value;
                    break;
                case "Main/OpenVideoFile":
                    language.Main.OpenVideoFile = reader.Value;
                    break;
                case "Main/NewFrameRateUsedToCalculateTimeCodes":
                    language.Main.NewFrameRateUsedToCalculateTimeCodes = reader.Value;
                    break;
                case "Main/NewFrameRateUsedToCalculateFrameNumbers":
                    language.Main.NewFrameRateUsedToCalculateFrameNumbers = reader.Value;
                    break;
                case "Main/FindContinue":
                    language.Main.FindContinue = reader.Value;
                    break;
                case "Main/FindContinueTitle":
                    language.Main.FindContinueTitle = reader.Value;
                    break;
                case "Main/ReplaceContinueNotFound":
                    language.Main.ReplaceContinueNotFound = reader.Value;
                    break;
                case "Main/ReplaceXContinue":
                    language.Main.ReplaceXContinue = reader.Value;
                    break;
                case "Main/ReplaceContinueTitle":
                    language.Main.ReplaceContinueTitle = reader.Value;
                    break;
                case "Main/SearchingForXFromLineY":
                    language.Main.SearchingForXFromLineY = reader.Value;
                    break;
                case "Main/XFoundAtLineNumberY":
                    language.Main.XFoundAtLineNumberY = reader.Value;
                    break;
                case "Main/XNotFound":
                    language.Main.XNotFound = reader.Value;
                    break;
                case "Main/BeforeReplace":
                    language.Main.BeforeReplace = reader.Value;
                    break;
                case "Main/MatchFoundX":
                    language.Main.MatchFoundX = reader.Value;
                    break;
                case "Main/NoMatchFoundX":
                    language.Main.NoMatchFoundX = reader.Value;
                    break;
                case "Main/FoundNothingToReplace":
                    language.Main.FoundNothingToReplace = reader.Value;
                    break;
                case "Main/ReplaceCountX":
                    language.Main.ReplaceCountX = reader.Value;
                    break;
                case "Main/NoXFoundAtLineY":
                    language.Main.NoXFoundAtLineY = reader.Value;
                    break;
                case "Main/OneReplacementMade":
                    language.Main.OneReplacementMade = reader.Value;
                    break;
                case "Main/BeforeChangesMadeInSourceView":
                    language.Main.BeforeChangesMadeInSourceView = reader.Value;
                    break;
                case "Main/UnableToParseSourceView":
                    language.Main.UnableToParseSourceView = reader.Value;
                    break;
                case "Main/GoToLineNumberX":
                    language.Main.GoToLineNumberX = reader.Value;
                    break;
                case "Main/CreateAdjustChangesApplied":
                    language.Main.CreateAdjustChangesApplied = reader.Value;
                    break;
                case "Main/SelectedLines":
                    language.Main.SelectedLines = reader.Value;
                    break;
                case "Main/BeforeDisplayTimeAdjustment":
                    language.Main.BeforeDisplayTimeAdjustment = reader.Value;
                    break;
                case "Main/DisplayTimeAdjustedX":
                    language.Main.DisplayTimeAdjustedX = reader.Value;
                    break;
                case "Main/DisplayTimesAdjustedX":
                    language.Main.DisplayTimesAdjustedX = reader.Value;
                    break;
                case "Main/StarTimeAdjustedX":
                    language.Main.StarTimeAdjustedX = reader.Value;
                    break;
                case "Main/BeforeCommonErrorFixes":
                    language.Main.BeforeCommonErrorFixes = reader.Value;
                    break;
                case "Main/CommonErrorsFixedInSelectedLines":
                    language.Main.CommonErrorsFixedInSelectedLines = reader.Value;
                    break;
                case "Main/CommonErrorsFixed":
                    language.Main.CommonErrorsFixed = reader.Value;
                    break;
                case "Main/BeforeRenumbering":
                    language.Main.BeforeRenumbering = reader.Value;
                    break;
                case "Main/RenumberedStartingFromX":
                    language.Main.RenumberedStartingFromX = reader.Value;
                    break;
                case "Main/BeforeRemovalOfTextingForHearingImpaired":
                    language.Main.BeforeRemovalOfTextingForHearingImpaired = reader.Value;
                    break;
                case "Main/TextingForHearingImpairedRemovedOneLine":
                    language.Main.TextingForHearingImpairedRemovedOneLine = reader.Value;
                    break;
                case "Main/TextingForHearingImpairedRemovedXLines":
                    language.Main.TextingForHearingImpairedRemovedXLines = reader.Value;
                    break;
                case "Main/SubtitleSplitted":
                    language.Main.SubtitleSplitted = reader.Value;
                    break;
                case "Main/SubtitleAppendPrompt":
                    language.Main.SubtitleAppendPrompt = reader.Value;
                    break;
                case "Main/SubtitleAppendPromptTitle":
                    language.Main.SubtitleAppendPromptTitle = reader.Value;
                    break;
                case "Main/OpenSubtitleToAppend":
                    language.Main.OpenSubtitleToAppend = reader.Value;
                    break;
                case "Main/AppendViaVisualSyncTitle":
                    language.Main.AppendViaVisualSyncTitle = reader.Value;
                    break;
                case "Main/AppendSynchronizedSubtitlePrompt":
                    language.Main.AppendSynchronizedSubtitlePrompt = reader.Value;
                    break;
                case "Main/BeforeAppend":
                    language.Main.BeforeAppend = reader.Value;
                    break;
                case "Main/SubtitleAppendedX":
                    language.Main.SubtitleAppendedX = reader.Value;
                    break;
                case "Main/SubtitleNotAppended":
                    language.Main.SubtitleNotAppended = reader.Value;
                    break;
                case "Main/GoogleTranslate":
                    language.Main.GoogleTranslate = reader.Value;
                    break;
                case "Main/MicrosoftTranslate":
                    language.Main.MicrosoftTranslate = reader.Value;
                    break;
                case "Main/BeforeGoogleTranslation":
                    language.Main.BeforeGoogleTranslation = reader.Value;
                    break;
                case "Main/SelectedLinesTranslated":
                    language.Main.SelectedLinesTranslated = reader.Value;
                    break;
                case "Main/SubtitleTranslated":
                    language.Main.SubtitleTranslated = reader.Value;
                    break;
                case "Main/TranslateSwedishToDanish":
                    language.Main.TranslateSwedishToDanish = reader.Value;
                    break;
                case "Main/TranslateSwedishToDanishWarning":
                    language.Main.TranslateSwedishToDanishWarning = reader.Value;
                    break;
                case "Main/TranslatingViaNikseDkMt":
                    language.Main.TranslatingViaNikseDkMt = reader.Value;
                    break;
                case "Main/BeforeSwedishToDanishTranslation":
                    language.Main.BeforeSwedishToDanishTranslation = reader.Value;
                    break;
                case "Main/TranslationFromSwedishToDanishComplete":
                    language.Main.TranslationFromSwedishToDanishComplete = reader.Value;
                    break;
                case "Main/TranslationFromSwedishToDanishFailed":
                    language.Main.TranslationFromSwedishToDanishFailed = reader.Value;
                    break;
                case "Main/BeforeUndo":
                    language.Main.BeforeUndo = reader.Value;
                    break;
                case "Main/UndoPerformed":
                    language.Main.UndoPerformed = reader.Value;
                    break;
                case "Main/RedoPerformed":
                    language.Main.RedoPerformed = reader.Value;
                    break;
                case "Main/NothingToUndo":
                    language.Main.NothingToUndo = reader.Value;
                    break;
                case "Main/InvalidLanguageNameX":
                    language.Main.InvalidLanguageNameX = reader.Value;
                    break;
                case "Main/DoNotDisplayMessageAgain":
                    language.Main.DoNotDisplayMessageAgain = reader.Value;
                    break;
                case "Main/NumberOfCorrectedWords":
                    language.Main.NumberOfCorrectedWords = reader.Value;
                    break;
                case "Main/NumberOfSkippedWords":
                    language.Main.NumberOfSkippedWords = reader.Value;
                    break;
                case "Main/NumberOfCorrectWords":
                    language.Main.NumberOfCorrectWords = reader.Value;
                    break;
                case "Main/NumberOfWordsAddedToDictionary":
                    language.Main.NumberOfWordsAddedToDictionary = reader.Value;
                    break;
                case "Main/NumberOfNameHits":
                    language.Main.NumberOfNameHits = reader.Value;
                    break;
                case "Main/SpellCheck":
                    language.Main.SpellCheck = reader.Value;
                    break;
                case "Main/BeforeSpellCheck":
                    language.Main.BeforeSpellCheck = reader.Value;
                    break;
                case "Main/SpellCheckChangedXToY":
                    language.Main.SpellCheckChangedXToY = reader.Value;
                    break;
                case "Main/BeforeAddingTagX":
                    language.Main.BeforeAddingTagX = reader.Value;
                    break;
                case "Main/TagXAdded":
                    language.Main.TagXAdded = reader.Value;
                    break;
                case "Main/LineXOfY":
                    language.Main.LineXOfY = reader.Value;
                    break;
                case "Main/XLinesSavedAsY":
                    language.Main.XLinesSavedAsY = reader.Value;
                    break;
                case "Main/XLinesDeleted":
                    language.Main.XLinesDeleted = reader.Value;
                    break;
                case "Main/BeforeDeletingXLines":
                    language.Main.BeforeDeletingXLines = reader.Value;
                    break;
                case "Main/DeleteXLinesPrompt":
                    language.Main.DeleteXLinesPrompt = reader.Value;
                    break;
                case "Main/OneLineDeleted":
                    language.Main.OneLineDeleted = reader.Value;
                    break;
                case "Main/BeforeDeletingOneLine":
                    language.Main.BeforeDeletingOneLine = reader.Value;
                    break;
                case "Main/DeleteOneLinePrompt":
                    language.Main.DeleteOneLinePrompt = reader.Value;
                    break;
                case "Main/BeforeInsertLine":
                    language.Main.BeforeInsertLine = reader.Value;
                    break;
                case "Main/LineInserted":
                    language.Main.LineInserted = reader.Value;
                    break;
                case "Main/BeforeLineUpdatedInListView":
                    language.Main.BeforeLineUpdatedInListView = reader.Value;
                    break;
                case "Main/BeforeSplitLine":
                    language.Main.BeforeSplitLine = reader.Value;
                    break;
                case "Main/LineSplitted":
                    language.Main.LineSplitted = reader.Value;
                    break;
                case "Main/BeforeMergeLines":
                    language.Main.BeforeMergeLines = reader.Value;
                    break;
                case "Main/LinesMerged":
                    language.Main.LinesMerged = reader.Value;
                    break;
                case "Main/BeforeSettingColor":
                    language.Main.BeforeSettingColor = reader.Value;
                    break;
                case "Main/BeforeSettingFontName":
                    language.Main.BeforeSettingFontName = reader.Value;
                    break;
                case "Main/BeforeTypeWriterEffect":
                    language.Main.BeforeTypeWriterEffect = reader.Value;
                    break;
                case "Main/BeforeKaraokeEffect":
                    language.Main.BeforeKaraokeEffect = reader.Value;
                    break;
                case "Main/BeforeImportingDvdSubtitle":
                    language.Main.BeforeImportingDvdSubtitle = reader.Value;
                    break;
                case "Main/OpenMatroskaFile":
                    language.Main.OpenMatroskaFile = reader.Value;
                    break;
                case "Main/MatroskaFiles":
                    language.Main.MatroskaFiles = reader.Value;
                    break;
                case "Main/NoSubtitlesFound":
                    language.Main.NoSubtitlesFound = reader.Value;
                    break;
                case "Main/NotAValidMatroskaFileX":
                    language.Main.NotAValidMatroskaFileX = reader.Value;
                    break;
                case "Main/BlurayNotSubtitlesFound":
                    language.Main.BlurayNotSubtitlesFound = reader.Value;
                    break;
                case "Main/ParsingMatroskaFile":
                    language.Main.ParsingMatroskaFile = reader.Value;
                    break;
                case "Main/ParsingTransportStreamFile":
                    language.Main.ParsingTransportStreamFile = reader.Value;
                    break;
                case "Main/BeforeImportFromMatroskaFile":
                    language.Main.BeforeImportFromMatroskaFile = reader.Value;
                    break;
                case "Main/SubtitleImportedFromMatroskaFile":
                    language.Main.SubtitleImportedFromMatroskaFile = reader.Value;
                    break;
                case "Main/DropFileXNotAccepted":
                    language.Main.DropFileXNotAccepted = reader.Value;
                    break;
                case "Main/DropOnlyOneFile":
                    language.Main.DropOnlyOneFile = reader.Value;
                    break;
                case "Main/OpenAnsiSubtitle":
                    language.Main.OpenAnsiSubtitle = reader.Value;
                    break;
                case "Main/BeforeChangeCasing":
                    language.Main.BeforeChangeCasing = reader.Value;
                    break;
                case "Main/CasingCompleteMessageNoNames":
                    language.Main.CasingCompleteMessageNoNames = reader.Value;
                    break;
                case "Main/CasingCompleteMessageOnlyNames":
                    language.Main.CasingCompleteMessageOnlyNames = reader.Value;
                    break;
                case "Main/CasingCompleteMessage":
                    language.Main.CasingCompleteMessage = reader.Value;
                    break;
                case "Main/BeforeChangeFrameRate":
                    language.Main.BeforeChangeFrameRate = reader.Value;
                    break;
                case "Main/BeforeAdjustSpeedInPercent":
                    language.Main.BeforeAdjustSpeedInPercent = reader.Value;
                    break;
                case "Main/FrameRateChangedFromXToY":
                    language.Main.FrameRateChangedFromXToY = reader.Value;
                    break;
                case "Main/IdxFileNotFoundWarning":
                    language.Main.IdxFileNotFoundWarning = reader.Value;
                    break;
                case "Main/InvalidVobSubHeader":
                    language.Main.InvalidVobSubHeader = reader.Value;
                    break;
                case "Main/OpenVobSubFile":
                    language.Main.OpenVobSubFile = reader.Value;
                    break;
                case "Main/VobSubFiles":
                    language.Main.VobSubFiles = reader.Value;
                    break;
                case "Main/OpenBluRaySupFile":
                    language.Main.OpenBluRaySupFile = reader.Value;
                    break;
                case "Main/BluRaySupFiles":
                    language.Main.BluRaySupFiles = reader.Value;
                    break;
                case "Main/OpenXSubFiles":
                    language.Main.OpenXSubFiles = reader.Value;
                    break;
                case "Main/XSubFiles":
                    language.Main.XSubFiles = reader.Value;
                    break;
                case "Main/BeforeImportingVobSubFile":
                    language.Main.BeforeImportingVobSubFile = reader.Value;
                    break;
                case "Main/BeforeImportingBluRaySupFile":
                    language.Main.BeforeImportingBluRaySupFile = reader.Value;
                    break;
                case "Main/BeforeImportingBdnXml":
                    language.Main.BeforeImportingBdnXml = reader.Value;
                    break;
                case "Main/BeforeShowSelectedLinesEarlierLater":
                    language.Main.BeforeShowSelectedLinesEarlierLater = reader.Value;
                    break;
                case "Main/ShowAllLinesXSecondsLinesEarlier":
                    language.Main.ShowAllLinesXSecondsLinesEarlier = reader.Value;
                    break;
                case "Main/ShowAllLinesXSecondsLinesLater":
                    language.Main.ShowAllLinesXSecondsLinesLater = reader.Value;
                    break;
                case "Main/ShowSelectedLinesXSecondsLinesEarlier":
                    language.Main.ShowSelectedLinesXSecondsLinesEarlier = reader.Value;
                    break;
                case "Main/ShowSelectedLinesXSecondsLinesLater":
                    language.Main.ShowSelectedLinesXSecondsLinesLater = reader.Value;
                    break;
                case "Main/ShowSelectionAndForwardXSecondsLinesEarlier":
                    language.Main.ShowSelectionAndForwardXSecondsLinesEarlier = reader.Value;
                    break;
                case "Main/ShowSelectionAndForwardXSecondsLinesLater":
                    language.Main.ShowSelectionAndForwardXSecondsLinesLater = reader.Value;
                    break;
                case "Main/DoubleWordsViaRegEx":
                    language.Main.DoubleWordsViaRegEx = reader.Value;
                    break;
                case "Main/BeforeSortX":
                    language.Main.BeforeSortX = reader.Value;
                    break;
                case "Main/SortedByX":
                    language.Main.SortedByX = reader.Value;
                    break;
                case "Main/BeforeAutoBalanceSelectedLines":
                    language.Main.BeforeAutoBalanceSelectedLines = reader.Value;
                    break;
                case "Main/NumberOfLinesAutoBalancedX":
                    language.Main.NumberOfLinesAutoBalancedX = reader.Value;
                    break;
                case "Main/BeforeRemoveLineBreaksInSelectedLines":
                    language.Main.BeforeRemoveLineBreaksInSelectedLines = reader.Value;
                    break;
                case "Main/NumberOfWithRemovedLineBreakX":
                    language.Main.NumberOfWithRemovedLineBreakX = reader.Value;
                    break;
                case "Main/BeforeMultipleReplace":
                    language.Main.BeforeMultipleReplace = reader.Value;
                    break;
                case "Main/NumberOfLinesReplacedX":
                    language.Main.NumberOfLinesReplacedX = reader.Value;
                    break;
                case "Main/NameXAddedToNameList":
                    language.Main.NameXAddedToNameList = reader.Value;
                    break;
                case "Main/NameXNotAddedToNameList":
                    language.Main.NameXNotAddedToNameList = reader.Value;
                    break;
                case "Main/WordXAddedToUserDic":
                    language.Main.WordXAddedToUserDic = reader.Value;
                    break;
                case "Main/WordXNotAddedToUserDic":
                    language.Main.WordXNotAddedToUserDic = reader.Value;
                    break;
                case "Main/OcrReplacePairXAdded":
                    language.Main.OcrReplacePairXAdded = reader.Value;
                    break;
                case "Main/OcrReplacePairXNotAdded":
                    language.Main.OcrReplacePairXNotAdded = reader.Value;
                    break;
                case "Main/XLinesSelected":
                    language.Main.XLinesSelected = reader.Value;
                    break;
                case "Main/UnicodeMusicSymbolsAnsiWarning":
                    language.Main.UnicodeMusicSymbolsAnsiWarning = reader.Value;
                    break;
                case "Main/NegativeTimeWarning":
                    language.Main.NegativeTimeWarning = reader.Value;
                    break;
                case "Main/BeforeMergeShortLines":
                    language.Main.BeforeMergeShortLines = reader.Value;
                    break;
                case "Main/MergedShortLinesX":
                    language.Main.MergedShortLinesX = reader.Value;
                    break;
                case "Main/BeforeSplitLongLines":
                    language.Main.BeforeSplitLongLines = reader.Value;
                    break;
                case "Main/LongLinesSplitX":
                    language.Main.LongLinesSplitX = reader.Value;
                    break;
                case "Main/BeforeDurationsBridgeGap":
                    language.Main.BeforeDurationsBridgeGap = reader.Value;
                    break;
                case "Main/BeforeSetMinimumDisplayTimeBetweenParagraphs":
                    language.Main.BeforeSetMinimumDisplayTimeBetweenParagraphs = reader.Value;
                    break;
                case "Main/XMinimumDisplayTimeBetweenParagraphsChanged":
                    language.Main.XMinimumDisplayTimeBetweenParagraphsChanged = reader.Value;
                    break;
                case "Main/BeforeImportText":
                    language.Main.BeforeImportText = reader.Value;
                    break;
                case "Main/TextImported":
                    language.Main.TextImported = reader.Value;
                    break;
                case "Main/BeforePointSynchronization":
                    language.Main.BeforePointSynchronization = reader.Value;
                    break;
                case "Main/PointSynchronizationDone":
                    language.Main.PointSynchronizationDone = reader.Value;
                    break;
                case "Main/BeforeTimeCodeImport":
                    language.Main.BeforeTimeCodeImport = reader.Value;
                    break;
                case "Main/TimeCodeImportedFromXY":
                    language.Main.TimeCodeImportedFromXY = reader.Value;
                    break;
                case "Main/BeforeInsertSubtitleAtVideoPosition":
                    language.Main.BeforeInsertSubtitleAtVideoPosition = reader.Value;
                    break;
                case "Main/BeforeSetStartTimeAndOffsetTheRest":
                    language.Main.BeforeSetStartTimeAndOffsetTheRest = reader.Value;
                    break;
                case "Main/BeforeSetEndTimeAndOffsetTheRest":
                    language.Main.BeforeSetEndTimeAndOffsetTheRest = reader.Value;
                    break;
                case "Main/BeforeSetEndAndVideoPosition":
                    language.Main.BeforeSetEndAndVideoPosition = reader.Value;
                    break;
                case "Main/ContinueWithCurrentSpellCheck":
                    language.Main.ContinueWithCurrentSpellCheck = reader.Value;
                    break;
                case "Main/CharactersPerSecond":
                    language.Main.CharactersPerSecond = reader.Value;
                    break;
                case "Main/GetFrameRateFromVideoFile":
                    language.Main.GetFrameRateFromVideoFile = reader.Value;
                    break;
                case "Main/NetworkMessage":
                    language.Main.NetworkMessage = reader.Value;
                    break;
                case "Main/NetworkUpdate":
                    language.Main.NetworkUpdate = reader.Value;
                    break;
                case "Main/NetworkInsert":
                    language.Main.NetworkInsert = reader.Value;
                    break;
                case "Main/NetworkDelete":
                    language.Main.NetworkDelete = reader.Value;
                    break;
                case "Main/NetworkNewUser":
                    language.Main.NetworkNewUser = reader.Value;
                    break;
                case "Main/NetworkByeUser":
                    language.Main.NetworkByeUser = reader.Value;
                    break;
                case "Main/NetworkUnableToConnectToServer":
                    language.Main.NetworkUnableToConnectToServer = reader.Value;
                    break;
                case "Main/UserAndAction":
                    language.Main.UserAndAction = reader.Value;
                    break;
                case "Main/NetworkMode":
                    language.Main.NetworkMode = reader.Value;
                    break;
                case "Main/XStartedSessionYAtZ":
                    language.Main.XStartedSessionYAtZ = reader.Value;
                    break;
                case "Main/OpenOtherSubtitle":
                    language.Main.OpenOtherSubtitle = reader.Value;
                    break;
                case "Main/BeforeToggleDialogDashes":
                    language.Main.BeforeToggleDialogDashes = reader.Value;
                    break;
                case "Main/ExportPlainTextAs":
                    language.Main.ExportPlainTextAs = reader.Value;
                    break;
                case "Main/TextFiles":
                    language.Main.TextFiles = reader.Value;
                    break;
                case "Main/SubtitleExported":
                    language.Main.SubtitleExported = reader.Value;
                    break;
                case "Main/LineNumberXErrorReadingFromSourceLineY":
                    language.Main.LineNumberXErrorReadingFromSourceLineY = reader.Value;
                    break;
                case "Main/LineNumberXErrorReadingTimeCodeFromSourceLineY":
                    language.Main.LineNumberXErrorReadingTimeCodeFromSourceLineY = reader.Value;
                    break;
                case "Main/LineNumberXExpectedNumberFromSourceLineY":
                    language.Main.LineNumberXExpectedNumberFromSourceLineY = reader.Value;
                    break;
                case "Main/BeforeGuessingTimeCodes":
                    language.Main.BeforeGuessingTimeCodes = reader.Value;
                    break;
                case "Main/BeforeAutoDuration":
                    language.Main.BeforeAutoDuration = reader.Value;
                    break;
                case "Main/BeforeColumnPaste":
                    language.Main.BeforeColumnPaste = reader.Value;
                    break;
                case "Main/BeforeColumnDelete":
                    language.Main.BeforeColumnDelete = reader.Value;
                    break;
                case "Main/BeforeColumnImportText":
                    language.Main.BeforeColumnImportText = reader.Value;
                    break;
                case "Main/BeforeColumnShiftCellsDown":
                    language.Main.BeforeColumnShiftCellsDown = reader.Value;
                    break;
                case "Main/BeforeX":
                    language.Main.BeforeX = reader.Value;
                    break;
                case "Main/LinesUpdatedX":
                    language.Main.LinesUpdatedX = reader.Value;
                    break;
                case "Main/ErrorLoadingPluginXErrorY":
                    language.Main.ErrorLoadingPluginXErrorY = reader.Value;
                    break;
                case "Main/BeforeRunningPluginXVersionY":
                    language.Main.BeforeRunningPluginXVersionY = reader.Value;
                    break;
                case "Main/UnableToReadPluginResult":
                    language.Main.UnableToReadPluginResult = reader.Value;
                    break;
                case "Main/UnableToCreateBackupDirectory":
                    language.Main.UnableToCreateBackupDirectory = reader.Value;
                    break;
                case "Main/BeforeDisplaySubtitleJoin":
                    language.Main.BeforeDisplaySubtitleJoin = reader.Value;
                    break;
                case "Main/SubtitlesJoined":
                    language.Main.SubtitlesJoined = reader.Value;
                    break;
                case "Main/StatusLog":
                    language.Main.StatusLog = reader.Value;
                    break;
                case "Main/XSceneChangesImported":
                    language.Main.XSceneChangesImported = reader.Value;
                    break;
                case "Main/PluginXExecuted":
                    language.Main.PluginXExecuted = reader.Value;
                    break;
                case "Main/NotAValidXSubFile":
                    language.Main.NotAValidXSubFile = reader.Value;
                    break;
                case "Main/BeforeMergeLinesWithSameText":
                    language.Main.BeforeMergeLinesWithSameText = reader.Value;
                    break;
                case "Main/ImportTimeCodesDifferentNumberOfLinesWarning":
                    language.Main.ImportTimeCodesDifferentNumberOfLinesWarning = reader.Value;
                    break;
                case "Main/ParsingTransportStream":
                    language.Main.ParsingTransportStream = reader.Value;
                    break;
                case "Main/XPercentCompleted":
                    language.Main.XPercentCompleted = reader.Value;
                    break;
                case "Main/NextX":
                    language.Main.NextX = reader.Value;
                    break;
                case "Main/PromptInsertSubtitleOverlap":
                    language.Main.PromptInsertSubtitleOverlap = reader.Value;
                    break;
                case "Main/SubtitleContainsNegativeDurationsX":
                    language.Main.SubtitleContainsNegativeDurationsX = reader.Value;
                    break;
                case "Main/SetPlayRateX":
                    language.Main.SetPlayRateX = reader.Value;
                    break;
                case "Main/ErrorLoadIdx":
                    language.Main.ErrorLoadIdx = reader.Value;
                    break;
                case "Main/ErrorLoadRar":
                    language.Main.ErrorLoadRar = reader.Value;
                    break;
                case "Main/ErrorLoadZip":
                    language.Main.ErrorLoadZip = reader.Value;
                    break;
                case "Main/ErrorLoad7Zip":
                    language.Main.ErrorLoad7Zip = reader.Value;
                    break;
                case "Main/ErrorLoadPng":
                    language.Main.ErrorLoadPng = reader.Value;
                    break;
                case "Main/ErrorLoadJpg":
                    language.Main.ErrorLoadJpg = reader.Value;
                    break;
                case "Main/ErrorLoadSrr":
                    language.Main.ErrorLoadSrr = reader.Value;
                    break;
                case "Main/ErrorLoadTorrent":
                    language.Main.ErrorLoadTorrent = reader.Value;
                    break;
                case "Main/ErrorLoadBinaryZeroes":
                    language.Main.ErrorLoadBinaryZeroes = reader.Value;
                    break;
                case "Main/ErrorDirectoryDropNotAllowed":
                    language.Main.ErrorDirectoryDropNotAllowed = reader.Value;
                    break;
                case "Main/NoSupportEncryptedVobSub":
                    language.Main.NoSupportEncryptedVobSub = reader.Value;
                    break;
                case "Main/NoSupportHereBluRaySup":
                    language.Main.NoSupportHereBluRaySup = reader.Value;
                    break;
                case "Main/NoSupportHereDvdSup":
                    language.Main.NoSupportHereDvdSup = reader.Value;
                    break;
                case "Main/NoSupportHereVobSub":
                    language.Main.NoSupportHereVobSub = reader.Value;
                    break;
                case "Main/NoSupportHereDivx":
                    language.Main.NoSupportHereDivx = reader.Value;
                    break;
                case "Main/Menu/File/Title":
                    language.Main.Menu.File.Title = reader.Value;
                    break;
                case "Main/Menu/File/New":
                    language.Main.Menu.File.New = reader.Value;
                    break;
                case "Main/Menu/File/Open":
                    language.Main.Menu.File.Open = reader.Value;
                    break;
                case "Main/Menu/File/OpenKeepVideo":
                    language.Main.Menu.File.OpenKeepVideo = reader.Value;
                    break;
                case "Main/Menu/File/Reopen":
                    language.Main.Menu.File.Reopen = reader.Value;
                    break;
                case "Main/Menu/File/Save":
                    language.Main.Menu.File.Save = reader.Value;
                    break;
                case "Main/Menu/File/SaveAs":
                    language.Main.Menu.File.SaveAs = reader.Value;
                    break;
                case "Main/Menu/File/RestoreAutoBackup":
                    language.Main.Menu.File.RestoreAutoBackup = reader.Value;
                    break;
                case "Main/Menu/File/AdvancedSubStationAlphaProperties":
                    language.Main.Menu.File.AdvancedSubStationAlphaProperties = reader.Value;
                    break;
                case "Main/Menu/File/SubStationAlphaProperties":
                    language.Main.Menu.File.SubStationAlphaProperties = reader.Value;
                    break;
                case "Main/Menu/File/EbuProperties":
                    language.Main.Menu.File.EbuProperties = reader.Value;
                    break;
                case "Main/Menu/File/DvdStuioProProperties":
                    language.Main.Menu.File.DvdStuioProProperties = reader.Value;
                    break;
                case "Main/Menu/File/TimedTextProperties":
                    language.Main.Menu.File.TimedTextProperties = reader.Value;
                    break;
                case "Main/Menu/File/PacProperties":
                    language.Main.Menu.File.PacProperties = reader.Value;
                    break;
                case "Main/Menu/File/OpenOriginal":
                    language.Main.Menu.File.OpenOriginal = reader.Value;
                    break;
                case "Main/Menu/File/SaveOriginal":
                    language.Main.Menu.File.SaveOriginal = reader.Value;
                    break;
                case "Main/Menu/File/CloseOriginal":
                    language.Main.Menu.File.CloseOriginal = reader.Value;
                    break;
                case "Main/Menu/File/OpenContainingFolder":
                    language.Main.Menu.File.OpenContainingFolder = reader.Value;
                    break;
                case "Main/Menu/File/Compare":
                    language.Main.Menu.File.Compare = reader.Value;
                    break;
                case "Main/Menu/File/Statistics":
                    language.Main.Menu.File.Statistics = reader.Value;
                    break;
                case "Main/Menu/File/Plugins":
                    language.Main.Menu.File.Plugins = reader.Value;
                    break;
                case "Main/Menu/File/ImportOcrFromDvd":
                    language.Main.Menu.File.ImportOcrFromDvd = reader.Value;
                    break;
                case "Main/Menu/File/ImportOcrVobSubSubtitle":
                    language.Main.Menu.File.ImportOcrVobSubSubtitle = reader.Value;
                    break;
                case "Main/Menu/File/ImportBluRaySupFile":
                    language.Main.Menu.File.ImportBluRaySupFile = reader.Value;
                    break;
                case "Main/Menu/File/ImportXSub":
                    language.Main.Menu.File.ImportXSub = reader.Value;
                    break;
                case "Main/Menu/File/ImportSubtitleFromMatroskaFile":
                    language.Main.Menu.File.ImportSubtitleFromMatroskaFile = reader.Value;
                    break;
                case "Main/Menu/File/ImportSubtitleWithManualChosenEncoding":
                    language.Main.Menu.File.ImportSubtitleWithManualChosenEncoding = reader.Value;
                    break;
                case "Main/Menu/File/ImportText":
                    language.Main.Menu.File.ImportText = reader.Value;
                    break;
                case "Main/Menu/File/ImportImages":
                    language.Main.Menu.File.ImportImages = reader.Value;
                    break;
                case "Main/Menu/File/ImportTimecodes":
                    language.Main.Menu.File.ImportTimecodes = reader.Value;
                    break;
                case "Main/Menu/File/Export":
                    language.Main.Menu.File.Export = reader.Value;
                    break;
                case "Main/Menu/File/ExportBdnXml":
                    language.Main.Menu.File.ExportBdnXml = reader.Value;
                    break;
                case "Main/Menu/File/ExportBluRaySup":
                    language.Main.Menu.File.ExportBluRaySup = reader.Value;
                    break;
                case "Main/Menu/File/ExportVobSub":
                    language.Main.Menu.File.ExportVobSub = reader.Value;
                    break;
                case "Main/Menu/File/ExportCavena890":
                    language.Main.Menu.File.ExportCavena890 = reader.Value;
                    break;
                case "Main/Menu/File/ExportEbu":
                    language.Main.Menu.File.ExportEbu = reader.Value;
                    break;
                case "Main/Menu/File/ExportPac":
                    language.Main.Menu.File.ExportPac = reader.Value;
                    break;
                case "Main/Menu/File/ExportPlainText":
                    language.Main.Menu.File.ExportPlainText = reader.Value;
                    break;
                case "Main/Menu/File/ExportAdobeEncoreFabImageScript":
                    language.Main.Menu.File.ExportAdobeEncoreFabImageScript = reader.Value;
                    break;
                case "Main/Menu/File/ExportKoreanAtsFilePair":
                    language.Main.Menu.File.ExportKoreanAtsFilePair = reader.Value;
                    break;
                case "Main/Menu/File/ExportAvidStl":
                    language.Main.Menu.File.ExportAvidStl = reader.Value;
                    break;
                case "Main/Menu/File/ExportDvdStudioProStl":
                    language.Main.Menu.File.ExportDvdStudioProStl = reader.Value;
                    break;
                case "Main/Menu/File/ExportCapMakerPlus":
                    language.Main.Menu.File.ExportCapMakerPlus = reader.Value;
                    break;
                case "Main/Menu/File/ExportCaptionsInc":
                    language.Main.Menu.File.ExportCaptionsInc = reader.Value;
                    break;
                case "Main/Menu/File/ExportCheetahCap":
                    language.Main.Menu.File.ExportCheetahCap = reader.Value;
                    break;
                case "Main/Menu/File/ExportUltech130":
                    language.Main.Menu.File.ExportUltech130 = reader.Value;
                    break;
                case "Main/Menu/File/ExportCustomTextFormat":
                    language.Main.Menu.File.ExportCustomTextFormat = reader.Value;
                    break;
                case "Main/Menu/File/Exit":
                    language.Main.Menu.File.Exit = reader.Value;
                    break;
                case "Main/Menu/Edit/Title":
                    language.Main.Menu.Edit.Title = reader.Value;
                    break;
                case "Main/Menu/Edit/Undo":
                    language.Main.Menu.Edit.Undo = reader.Value;
                    break;
                case "Main/Menu/Edit/Redo":
                    language.Main.Menu.Edit.Redo = reader.Value;
                    break;
                case "Main/Menu/Edit/ShowUndoHistory":
                    language.Main.Menu.Edit.ShowUndoHistory = reader.Value;
                    break;
                case "Main/Menu/Edit/InsertUnicodeSymbol":
                    language.Main.Menu.Edit.InsertUnicodeSymbol = reader.Value;
                    break;
                case "Main/Menu/Edit/InsertUnicodeControlCharacters":
                    language.Main.Menu.Edit.InsertUnicodeControlCharacters = reader.Value;
                    break;
                case "Main/Menu/Edit/InsertUnicodeControlCharactersLRM":
                    language.Main.Menu.Edit.InsertUnicodeControlCharactersLRM = reader.Value;
                    break;
                case "Main/Menu/Edit/InsertUnicodeControlCharactersRLM":
                    language.Main.Menu.Edit.InsertUnicodeControlCharactersRLM = reader.Value;
                    break;
                case "Main/Menu/Edit/InsertUnicodeControlCharactersLRE":
                    language.Main.Menu.Edit.InsertUnicodeControlCharactersLRE = reader.Value;
                    break;
                case "Main/Menu/Edit/InsertUnicodeControlCharactersRLE":
                    language.Main.Menu.Edit.InsertUnicodeControlCharactersRLE = reader.Value;
                    break;
                case "Main/Menu/Edit/InsertUnicodeControlCharactersLRO":
                    language.Main.Menu.Edit.InsertUnicodeControlCharactersLRO = reader.Value;
                    break;
                case "Main/Menu/Edit/InsertUnicodeControlCharactersRLO":
                    language.Main.Menu.Edit.InsertUnicodeControlCharactersRLO = reader.Value;
                    break;
                case "Main/Menu/Edit/Find":
                    language.Main.Menu.Edit.Find = reader.Value;
                    break;
                case "Main/Menu/Edit/FindNext":
                    language.Main.Menu.Edit.FindNext = reader.Value;
                    break;
                case "Main/Menu/Edit/Replace":
                    language.Main.Menu.Edit.Replace = reader.Value;
                    break;
                case "Main/Menu/Edit/MultipleReplace":
                    language.Main.Menu.Edit.MultipleReplace = reader.Value;
                    break;
                case "Main/Menu/Edit/GoToSubtitleNumber":
                    language.Main.Menu.Edit.GoToSubtitleNumber = reader.Value;
                    break;
                case "Main/Menu/Edit/RightToLeftMode":
                    language.Main.Menu.Edit.RightToLeftMode = reader.Value;
                    break;
                case "Main/Menu/Edit/FixTrlViaUnicodeControlCharacters":
                    language.Main.Menu.Edit.FixTrlViaUnicodeControlCharacters = reader.Value;
                    break;
                case "Main/Menu/Edit/ReverseRightToLeftStartEnd":
                    language.Main.Menu.Edit.ReverseRightToLeftStartEnd = reader.Value;
                    break;
                case "Main/Menu/Edit/ShowOriginalTextInAudioAndVideoPreview":
                    language.Main.Menu.Edit.ShowOriginalTextInAudioAndVideoPreview = reader.Value;
                    break;
                case "Main/Menu/Edit/ModifySelection":
                    language.Main.Menu.Edit.ModifySelection = reader.Value;
                    break;
                case "Main/Menu/Edit/InverseSelection":
                    language.Main.Menu.Edit.InverseSelection = reader.Value;
                    break;
                case "Main/Menu/Tools/Title":
                    language.Main.Menu.Tools.Title = reader.Value;
                    break;
                case "Main/Menu/Tools/AdjustDisplayDuration":
                    language.Main.Menu.Tools.AdjustDisplayDuration = reader.Value;
                    break;
                case "Main/Menu/Tools/ApplyDurationLimits":
                    language.Main.Menu.Tools.ApplyDurationLimits = reader.Value;
                    break;
                case "Main/Menu/Tools/DurationsBridgeGap":
                    language.Main.Menu.Tools.DurationsBridgeGap = reader.Value;
                    break;
                case "Main/Menu/Tools/FixCommonErrors":
                    language.Main.Menu.Tools.FixCommonErrors = reader.Value;
                    break;
                case "Main/Menu/Tools/StartNumberingFrom":
                    language.Main.Menu.Tools.StartNumberingFrom = reader.Value;
                    break;
                case "Main/Menu/Tools/RemoveTextForHearingImpaired":
                    language.Main.Menu.Tools.RemoveTextForHearingImpaired = reader.Value;
                    break;
                case "Main/Menu/Tools/ChangeCasing":
                    language.Main.Menu.Tools.ChangeCasing = reader.Value;
                    break;
                case "Main/Menu/Tools/ChangeFrameRate":
                    language.Main.Menu.Tools.ChangeFrameRate = reader.Value;
                    break;
                case "Main/Menu/Tools/ChangeSpeedInPercent":
                    language.Main.Menu.Tools.ChangeSpeedInPercent = reader.Value;
                    break;
                case "Main/Menu/Tools/MergeShortLines":
                    language.Main.Menu.Tools.MergeShortLines = reader.Value;
                    break;
                case "Main/Menu/Tools/MergeDuplicateText":
                    language.Main.Menu.Tools.MergeDuplicateText = reader.Value;
                    break;
                case "Main/Menu/Tools/MergeSameTimeCodes":
                    language.Main.Menu.Tools.MergeSameTimeCodes = reader.Value;
                    break;
                case "Main/Menu/Tools/SplitLongLines":
                    language.Main.Menu.Tools.SplitLongLines = reader.Value;
                    break;
                case "Main/Menu/Tools/MinimumDisplayTimeBetweenParagraphs":
                    language.Main.Menu.Tools.MinimumDisplayTimeBetweenParagraphs = reader.Value;
                    break;
                case "Main/Menu/Tools/SortBy":
                    language.Main.Menu.Tools.SortBy = reader.Value;
                    break;
                case "Main/Menu/Tools/NetflixQualityCheck":
                    language.Main.Menu.Tools.NetflixQualityCheck = reader.Value;
                    break;
                case "Main/Menu/Tools/Number":
                    language.Main.Menu.Tools.Number = reader.Value;
                    break;
                case "Main/Menu/Tools/StartTime":
                    language.Main.Menu.Tools.StartTime = reader.Value;
                    break;
                case "Main/Menu/Tools/EndTime":
                    language.Main.Menu.Tools.EndTime = reader.Value;
                    break;
                case "Main/Menu/Tools/Duration":
                    language.Main.Menu.Tools.Duration = reader.Value;
                    break;
                case "Main/Menu/Tools/TextAlphabetically":
                    language.Main.Menu.Tools.TextAlphabetically = reader.Value;
                    break;
                case "Main/Menu/Tools/TextSingleLineMaximumLength":
                    language.Main.Menu.Tools.TextSingleLineMaximumLength = reader.Value;
                    break;
                case "Main/Menu/Tools/TextTotalLength":
                    language.Main.Menu.Tools.TextTotalLength = reader.Value;
                    break;
                case "Main/Menu/Tools/TextNumberOfLines":
                    language.Main.Menu.Tools.TextNumberOfLines = reader.Value;
                    break;
                case "Main/Menu/Tools/TextNumberOfCharactersPerSeconds":
                    language.Main.Menu.Tools.TextNumberOfCharactersPerSeconds = reader.Value;
                    break;
                case "Main/Menu/Tools/WordsPerMinute":
                    language.Main.Menu.Tools.WordsPerMinute = reader.Value;
                    break;
                case "Main/Menu/Tools/Style":
                    language.Main.Menu.Tools.Style = reader.Value;
                    break;
                case "Main/Menu/Tools/Ascending":
                    language.Main.Menu.Tools.Ascending = reader.Value;
                    break;
                case "Main/Menu/Tools/Descending":
                    language.Main.Menu.Tools.Descending = reader.Value;
                    break;
                case "Main/Menu/Tools/MakeNewEmptyTranslationFromCurrentSubtitle":
                    language.Main.Menu.Tools.MakeNewEmptyTranslationFromCurrentSubtitle = reader.Value;
                    break;
                case "Main/Menu/Tools/BatchConvert":
                    language.Main.Menu.Tools.BatchConvert = reader.Value;
                    break;
                case "Main/Menu/Tools/GenerateTimeAsText":
                    language.Main.Menu.Tools.GenerateTimeAsText = reader.Value;
                    break;
                case "Main/Menu/Tools/MeasurementConverter":
                    language.Main.Menu.Tools.MeasurementConverter = reader.Value;
                    break;
                case "Main/Menu/Tools/SplitSubtitle":
                    language.Main.Menu.Tools.SplitSubtitle = reader.Value;
                    break;
                case "Main/Menu/Tools/AppendSubtitle":
                    language.Main.Menu.Tools.AppendSubtitle = reader.Value;
                    break;
                case "Main/Menu/Tools/JoinSubtitles":
                    language.Main.Menu.Tools.JoinSubtitles = reader.Value;
                    break;
                case "Main/Menu/Video/Title":
                    language.Main.Menu.Video.Title = reader.Value;
                    break;
                case "Main/Menu/Video/OpenVideo":
                    language.Main.Menu.Video.OpenVideo = reader.Value;
                    break;
                case "Main/Menu/Video/OpenVideoFromUrl":
                    language.Main.Menu.Video.OpenVideoFromUrl = reader.Value;
                    break;
                case "Main/Menu/Video/OpenDvd":
                    language.Main.Menu.Video.OpenDvd = reader.Value;
                    break;
                case "Main/Menu/Video/ChooseAudioTrack":
                    language.Main.Menu.Video.ChooseAudioTrack = reader.Value;
                    break;
                case "Main/Menu/Video/CloseVideo":
                    language.Main.Menu.Video.CloseVideo = reader.Value;
                    break;
                case "Main/Menu/Video/SetVideoOffset":
                    language.Main.Menu.Video.SetVideoOffset = reader.Value;
                    break;
                case "Main/Menu/Video/SmptTimeMode":
                    language.Main.Menu.Video.SmptTimeMode = reader.Value;
                    break;
                case "Main/Menu/Video/GenerateTextFromVideo":
                    language.Main.Menu.Video.GenerateTextFromVideo = reader.Value;
                    break;
                case "Main/Menu/Video/GenerateImportSceneChanges":
                    language.Main.Menu.Video.GenerateImportSceneChanges = reader.Value;
                    break;
                case "Main/Menu/Video/RemoveSceneChanges":
                    language.Main.Menu.Video.RemoveSceneChanges = reader.Value;
                    break;
                case "Main/Menu/Video/WaveformBatchGenerate":
                    language.Main.Menu.Video.WaveformBatchGenerate = reader.Value;
                    break;
                case "Main/Menu/Video/ShowHideVideo":
                    language.Main.Menu.Video.ShowHideVideo = reader.Value;
                    break;
                case "Main/Menu/Video/ShowHideWaveform":
                    language.Main.Menu.Video.ShowHideWaveform = reader.Value;
                    break;
                case "Main/Menu/Video/ShowHideWaveformAndSpectrogram":
                    language.Main.Menu.Video.ShowHideWaveformAndSpectrogram = reader.Value;
                    break;
                case "Main/Menu/Video/UnDockVideoControls":
                    language.Main.Menu.Video.UnDockVideoControls = reader.Value;
                    break;
                case "Main/Menu/Video/ReDockVideoControls":
                    language.Main.Menu.Video.ReDockVideoControls = reader.Value;
                    break;
                case "Main/Menu/SpellCheck/Title":
                    language.Main.Menu.SpellCheck.Title = reader.Value;
                    break;
                case "Main/Menu/SpellCheck/SpellCheck":
                    language.Main.Menu.SpellCheck.SpellCheck = reader.Value;
                    break;
                case "Main/Menu/SpellCheck/SpellCheckFromCurrentLine":
                    language.Main.Menu.SpellCheck.SpellCheckFromCurrentLine = reader.Value;
                    break;
                case "Main/Menu/SpellCheck/FindDoubleWords":
                    language.Main.Menu.SpellCheck.FindDoubleWords = reader.Value;
                    break;
                case "Main/Menu/SpellCheck/FindDoubleLines":
                    language.Main.Menu.SpellCheck.FindDoubleLines = reader.Value;
                    break;
                case "Main/Menu/SpellCheck/GetDictionaries":
                    language.Main.Menu.SpellCheck.GetDictionaries = reader.Value;
                    break;
                case "Main/Menu/SpellCheck/AddToNameList":
                    language.Main.Menu.SpellCheck.AddToNameList = reader.Value;
                    break;
                case "Main/Menu/Synchronization/Title":
                    language.Main.Menu.Synchronization.Title = reader.Value;
                    break;
                case "Main/Menu/Synchronization/AdjustAllTimes":
                    language.Main.Menu.Synchronization.AdjustAllTimes = reader.Value;
                    break;
                case "Main/Menu/Synchronization/VisualSync":
                    language.Main.Menu.Synchronization.VisualSync = reader.Value;
                    break;
                case "Main/Menu/Synchronization/PointSync":
                    language.Main.Menu.Synchronization.PointSync = reader.Value;
                    break;
                case "Main/Menu/Synchronization/PointSyncViaOtherSubtitle":
                    language.Main.Menu.Synchronization.PointSyncViaOtherSubtitle = reader.Value;
                    break;
                case "Main/Menu/AutoTranslate/Title":
                    language.Main.Menu.AutoTranslate.Title = reader.Value;
                    break;
                case "Main/Menu/AutoTranslate/TranslatePoweredByGoogle":
                    language.Main.Menu.AutoTranslate.TranslatePoweredByGoogle = reader.Value;
                    break;
                case "Main/Menu/AutoTranslate/TranslatePoweredByMicrosoft":
                    language.Main.Menu.AutoTranslate.TranslatePoweredByMicrosoft = reader.Value;
                    break;
                case "Main/Menu/AutoTranslate/TranslateFromSwedishToDanish":
                    language.Main.Menu.AutoTranslate.TranslateFromSwedishToDanish = reader.Value;
                    break;
                case "Main/Menu/Options/Title":
                    language.Main.Menu.Options.Title = reader.Value;
                    break;
                case "Main/Menu/Options/Settings":
                    language.Main.Menu.Options.Settings = reader.Value;
                    break;
                case "Main/Menu/Options/ChooseLanguage":
                    language.Main.Menu.Options.ChooseLanguage = reader.Value;
                    break;
                case "Main/Menu/Networking/Title":
                    language.Main.Menu.Networking.Title = reader.Value;
                    break;
                case "Main/Menu/Networking/StartNewSession":
                    language.Main.Menu.Networking.StartNewSession = reader.Value;
                    break;
                case "Main/Menu/Networking/JoinSession":
                    language.Main.Menu.Networking.JoinSession = reader.Value;
                    break;
                case "Main/Menu/Networking/ShowSessionInfoAndLog":
                    language.Main.Menu.Networking.ShowSessionInfoAndLog = reader.Value;
                    break;
                case "Main/Menu/Networking/Chat":
                    language.Main.Menu.Networking.Chat = reader.Value;
                    break;
                case "Main/Menu/Networking/LeaveSession":
                    language.Main.Menu.Networking.LeaveSession = reader.Value;
                    break;
                case "Main/Menu/Help/CheckForUpdates":
                    language.Main.Menu.Help.CheckForUpdates = reader.Value;
                    break;
                case "Main/Menu/Help/Title":
                    language.Main.Menu.Help.Title = reader.Value;
                    break;
                case "Main/Menu/Help/Help":
                    language.Main.Menu.Help.Help = reader.Value;
                    break;
                case "Main/Menu/Help/About":
                    language.Main.Menu.Help.About = reader.Value;
                    break;
                case "Main/Menu/ToolBar/New":
                    language.Main.Menu.ToolBar.New = reader.Value;
                    break;
                case "Main/Menu/ToolBar/Open":
                    language.Main.Menu.ToolBar.Open = reader.Value;
                    break;
                case "Main/Menu/ToolBar/Save":
                    language.Main.Menu.ToolBar.Save = reader.Value;
                    break;
                case "Main/Menu/ToolBar/SaveAs":
                    language.Main.Menu.ToolBar.SaveAs = reader.Value;
                    break;
                case "Main/Menu/ToolBar/Find":
                    language.Main.Menu.ToolBar.Find = reader.Value;
                    break;
                case "Main/Menu/ToolBar/Replace":
                    language.Main.Menu.ToolBar.Replace = reader.Value;
                    break;
                case "Main/Menu/ToolBar/FixCommonErrors":
                    language.Main.Menu.ToolBar.FixCommonErrors = reader.Value;
                    break;
                case "Main/Menu/ToolBar/RemoveTextForHi":
                    language.Main.Menu.ToolBar.RemoveTextForHi = reader.Value;
                    break;
                case "Main/Menu/ToolBar/VisualSync":
                    language.Main.Menu.ToolBar.VisualSync = reader.Value;
                    break;
                case "Main/Menu/ToolBar/SpellCheck":
                    language.Main.Menu.ToolBar.SpellCheck = reader.Value;
                    break;
                case "Main/Menu/ToolBar/NetflixQualityCheck":
                    language.Main.Menu.ToolBar.NetflixQualityCheck = reader.Value;
                    break;
                case "Main/Menu/ToolBar/Settings":
                    language.Main.Menu.ToolBar.Settings = reader.Value;
                    break;
                case "Main/Menu/ToolBar/Help":
                    language.Main.Menu.ToolBar.Help = reader.Value;
                    break;
                case "Main/Menu/ToolBar/ShowHideWaveform":
                    language.Main.Menu.ToolBar.ShowHideWaveform = reader.Value;
                    break;
                case "Main/Menu/ToolBar/ShowHideVideo":
                    language.Main.Menu.ToolBar.ShowHideVideo = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SizeAllColumnsToFit":
                    language.Main.Menu.ContextMenu.SizeAllColumnsToFit = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/AdvancedSubStationAlphaSetStyle":
                    language.Main.Menu.ContextMenu.AdvancedSubStationAlphaSetStyle = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SubStationAlphaSetStyle":
                    language.Main.Menu.ContextMenu.SubStationAlphaSetStyle = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SetActor":
                    language.Main.Menu.ContextMenu.SetActor = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SubStationAlphaStyles":
                    language.Main.Menu.ContextMenu.SubStationAlphaStyles = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/AdvancedSubStationAlphaStyles":
                    language.Main.Menu.ContextMenu.AdvancedSubStationAlphaStyles = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/TimedTextSetRegion":
                    language.Main.Menu.ContextMenu.TimedTextSetRegion = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/TimedTextSetStyle":
                    language.Main.Menu.ContextMenu.TimedTextSetStyle = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/TimedTextStyles":
                    language.Main.Menu.ContextMenu.TimedTextStyles = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/TimedTextSetLanguage":
                    language.Main.Menu.ContextMenu.TimedTextSetLanguage = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SamiSetStyle":
                    language.Main.Menu.ContextMenu.SamiSetStyle = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/NuendoSetStyle":
                    language.Main.Menu.ContextMenu.NuendoSetStyle = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Cut":
                    language.Main.Menu.ContextMenu.Cut = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Copy":
                    language.Main.Menu.ContextMenu.Copy = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Paste":
                    language.Main.Menu.ContextMenu.Paste = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Delete":
                    language.Main.Menu.ContextMenu.Delete = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SplitLineAtCursorPosition":
                    language.Main.Menu.ContextMenu.SplitLineAtCursorPosition = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SplitLineAtCursorAndWaveformPosition":
                    language.Main.Menu.ContextMenu.SplitLineAtCursorAndWaveformPosition = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/AutoDurationCurrentLine":
                    language.Main.Menu.ContextMenu.AutoDurationCurrentLine = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SelectAll":
                    language.Main.Menu.ContextMenu.SelectAll = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/InsertFirstLine":
                    language.Main.Menu.ContextMenu.InsertFirstLine = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/InsertBefore":
                    language.Main.Menu.ContextMenu.InsertBefore = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/InsertAfter":
                    language.Main.Menu.ContextMenu.InsertAfter = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/InsertSubtitleAfter":
                    language.Main.Menu.ContextMenu.InsertSubtitleAfter = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/CopyToClipboard":
                    language.Main.Menu.ContextMenu.CopyToClipboard = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Column":
                    language.Main.Menu.ContextMenu.Column = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnDeleteText":
                    language.Main.Menu.ContextMenu.ColumnDeleteText = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnDeleteTextAndShiftCellsUp":
                    language.Main.Menu.ContextMenu.ColumnDeleteTextAndShiftCellsUp = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnInsertEmptyTextAndShiftCellsDown":
                    language.Main.Menu.ContextMenu.ColumnInsertEmptyTextAndShiftCellsDown = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnInsertTextFromSubtitle":
                    language.Main.Menu.ContextMenu.ColumnInsertTextFromSubtitle = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnImportTextAndShiftCellsDown":
                    language.Main.Menu.ContextMenu.ColumnImportTextAndShiftCellsDown = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnPasteFromClipboard":
                    language.Main.Menu.ContextMenu.ColumnPasteFromClipboard = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnTextUp":
                    language.Main.Menu.ContextMenu.ColumnTextUp = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnTextDown":
                    language.Main.Menu.ContextMenu.ColumnTextDown = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ColumnCopyOriginalTextToCurrent":
                    language.Main.Menu.ContextMenu.ColumnCopyOriginalTextToCurrent = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Split":
                    language.Main.Menu.ContextMenu.Split = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/MergeSelectedLines":
                    language.Main.Menu.ContextMenu.MergeSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/MergeSelectedLinesAsDialog":
                    language.Main.Menu.ContextMenu.MergeSelectedLinesAsDialog = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/MergeWithLineBefore":
                    language.Main.Menu.ContextMenu.MergeWithLineBefore = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/MergeWithLineAfter":
                    language.Main.Menu.ContextMenu.MergeWithLineAfter = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveFormatting":
                    language.Main.Menu.ContextMenu.RemoveFormatting = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveFormattingAll":
                    language.Main.Menu.ContextMenu.RemoveFormattingAll = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveFormattingBold":
                    language.Main.Menu.ContextMenu.RemoveFormattingBold = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveFormattingItalic":
                    language.Main.Menu.ContextMenu.RemoveFormattingItalic = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveFormattingUnderline":
                    language.Main.Menu.ContextMenu.RemoveFormattingUnderline = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveFormattingColor":
                    language.Main.Menu.ContextMenu.RemoveFormattingColor = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveFormattingFontName":
                    language.Main.Menu.ContextMenu.RemoveFormattingFontName = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveFormattingAlignment":
                    language.Main.Menu.ContextMenu.RemoveFormattingAlignment = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Underline":
                    language.Main.Menu.ContextMenu.Underline = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Box":
                    language.Main.Menu.ContextMenu.Box = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Color":
                    language.Main.Menu.ContextMenu.Color = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/FontName":
                    language.Main.Menu.ContextMenu.FontName = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Superscript":
                    language.Main.Menu.ContextMenu.Superscript = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Subscript":
                    language.Main.Menu.ContextMenu.Subscript = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/Alignment":
                    language.Main.Menu.ContextMenu.Alignment = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/AutoBalanceSelectedLines":
                    language.Main.Menu.ContextMenu.AutoBalanceSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveLineBreaksFromSelectedLines":
                    language.Main.Menu.ContextMenu.RemoveLineBreaksFromSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/TypewriterEffect":
                    language.Main.Menu.ContextMenu.TypewriterEffect = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/KaraokeEffect":
                    language.Main.Menu.ContextMenu.KaraokeEffect = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ShowSelectedLinesEarlierLater":
                    language.Main.Menu.ContextMenu.ShowSelectedLinesEarlierLater = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/VisualSyncSelectedLines":
                    language.Main.Menu.ContextMenu.VisualSyncSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/GoogleAndMicrosoftTranslateSelectedLine":
                    language.Main.Menu.ContextMenu.GoogleAndMicrosoftTranslateSelectedLine = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/TranslateSelectedLines":
                    language.Main.Menu.ContextMenu.TranslateSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/AdjustDisplayDurationForSelectedLines":
                    language.Main.Menu.ContextMenu.AdjustDisplayDurationForSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/FixCommonErrorsInSelectedLines":
                    language.Main.Menu.ContextMenu.FixCommonErrorsInSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/ChangeCasingForSelectedLines":
                    language.Main.Menu.ContextMenu.ChangeCasingForSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/SaveSelectedLines":
                    language.Main.Menu.ContextMenu.SaveSelectedLines = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/WebVTTSetNewVoice":
                    language.Main.Menu.ContextMenu.WebVTTSetNewVoice = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/WebVTTRemoveVoices":
                    language.Main.Menu.ContextMenu.WebVTTRemoveVoices = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/NewActor":
                    language.Main.Menu.ContextMenu.NewActor = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveActors":
                    language.Main.Menu.ContextMenu.RemoveActors = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/EditBookmark":
                    language.Main.Menu.ContextMenu.EditBookmark = reader.Value;
                    break;
                case "Main/Menu/ContextMenu/RemoveBookmark":
                    language.Main.Menu.ContextMenu.RemoveBookmark = reader.Value;
                    break;
                case "Main/Controls/SubtitleFormat":
                    language.Main.Controls.SubtitleFormat = reader.Value;
                    break;
                case "Main/Controls/FileEncoding":
                    language.Main.Controls.FileEncoding = reader.Value;
                    break;
                case "Main/Controls/ListView":
                    language.Main.Controls.ListView = reader.Value;
                    break;
                case "Main/Controls/SourceView":
                    language.Main.Controls.SourceView = reader.Value;
                    break;
                case "Main/Controls/UndoChangesInEditPanel":
                    language.Main.Controls.UndoChangesInEditPanel = reader.Value;
                    break;
                case "Main/Controls/Previous":
                    language.Main.Controls.Previous = reader.Value;
                    break;
                case "Main/Controls/Next":
                    language.Main.Controls.Next = reader.Value;
                    break;
                case "Main/Controls/AutoBreak":
                    language.Main.Controls.AutoBreak = reader.Value;
                    break;
                case "Main/Controls/Unbreak":
                    language.Main.Controls.Unbreak = reader.Value;
                    break;
                case "Main/VideoControls/Translate":
                    language.Main.VideoControls.Translate = reader.Value;
                    break;
                case "Main/VideoControls/CreateAndAdjust":
                    language.Main.VideoControls.CreateAndAdjust = reader.Value;
                    break;
                case "Main/VideoControls/Create":
                    language.Main.VideoControls.Create = reader.Value;
                    break;
                case "Main/VideoControls/Adjust":
                    language.Main.VideoControls.Adjust = reader.Value;
                    break;
                case "Main/VideoControls/SelectCurrentElementWhilePlaying":
                    language.Main.VideoControls.SelectCurrentElementWhilePlaying = reader.Value;
                    break;
                case "Main/VideoControls/AutoRepeat":
                    language.Main.VideoControls.AutoRepeat = reader.Value;
                    break;
                case "Main/VideoControls/AutoRepeatOn":
                    language.Main.VideoControls.AutoRepeatOn = reader.Value;
                    break;
                case "Main/VideoControls/AutoRepeatCount":
                    language.Main.VideoControls.AutoRepeatCount = reader.Value;
                    break;
                case "Main/VideoControls/AutoContinue":
                    language.Main.VideoControls.AutoContinue = reader.Value;
                    break;
                case "Main/VideoControls/AutoContinueOn":
                    language.Main.VideoControls.AutoContinueOn = reader.Value;
                    break;
                case "Main/VideoControls/DelayInSeconds":
                    language.Main.VideoControls.DelayInSeconds = reader.Value;
                    break;
                case "Main/VideoControls/OriginalText":
                    language.Main.VideoControls.OriginalText = reader.Value;
                    break;
                case "Main/VideoControls/Previous":
                    language.Main.VideoControls.Previous = reader.Value;
                    break;
                case "Main/VideoControls/Stop":
                    language.Main.VideoControls.Stop = reader.Value;
                    break;
                case "Main/VideoControls/PlayCurrent":
                    language.Main.VideoControls.PlayCurrent = reader.Value;
                    break;
                case "Main/VideoControls/Next":
                    language.Main.VideoControls.Next = reader.Value;
                    break;
                case "Main/VideoControls/Playing":
                    language.Main.VideoControls.Playing = reader.Value;
                    break;
                case "Main/VideoControls/RepeatingLastTime":
                    language.Main.VideoControls.RepeatingLastTime = reader.Value;
                    break;
                case "Main/VideoControls/RepeatingXTimesLeft":
                    language.Main.VideoControls.RepeatingXTimesLeft = reader.Value;
                    break;
                case "Main/VideoControls/AutoContinueInOneSecond":
                    language.Main.VideoControls.AutoContinueInOneSecond = reader.Value;
                    break;
                case "Main/VideoControls/AutoContinueInXSeconds":
                    language.Main.VideoControls.AutoContinueInXSeconds = reader.Value;
                    break;
                case "Main/VideoControls/StillTypingAutoContinueStopped":
                    language.Main.VideoControls.StillTypingAutoContinueStopped = reader.Value;
                    break;
                case "Main/VideoControls/InsertNewSubtitleAtVideoPosition":
                    language.Main.VideoControls.InsertNewSubtitleAtVideoPosition = reader.Value;
                    break;
                case "Main/VideoControls/Auto":
                    language.Main.VideoControls.Auto = reader.Value;
                    break;
                case "Main/VideoControls/PlayFromJustBeforeText":
                    language.Main.VideoControls.PlayFromJustBeforeText = reader.Value;
                    break;
                case "Main/VideoControls/Pause":
                    language.Main.VideoControls.Pause = reader.Value;
                    break;
                case "Main/VideoControls/GoToSubtitlePositionAndPause":
                    language.Main.VideoControls.GoToSubtitlePositionAndPause = reader.Value;
                    break;
                case "Main/VideoControls/SetStartTime":
                    language.Main.VideoControls.SetStartTime = reader.Value;
                    break;
                case "Main/VideoControls/SetEndTimeAndGoToNext":
                    language.Main.VideoControls.SetEndTimeAndGoToNext = reader.Value;
                    break;
                case "Main/VideoControls/AdjustedViaEndTime":
                    language.Main.VideoControls.AdjustedViaEndTime = reader.Value;
                    break;
                case "Main/VideoControls/SetEndTime":
                    language.Main.VideoControls.SetEndTime = reader.Value;
                    break;
                case "Main/VideoControls/SetstartTimeAndOffsetOfRest":
                    language.Main.VideoControls.SetstartTimeAndOffsetOfRest = reader.Value;
                    break;
                case "Main/VideoControls/SearchTextOnline":
                    language.Main.VideoControls.SearchTextOnline = reader.Value;
                    break;
                case "Main/VideoControls/GoogleTranslate":
                    language.Main.VideoControls.GoogleTranslate = reader.Value;
                    break;
                case "Main/VideoControls/GoogleIt":
                    language.Main.VideoControls.GoogleIt = reader.Value;
                    break;
                case "Main/VideoControls/SecondsBackShort":
                    language.Main.VideoControls.SecondsBackShort = reader.Value;
                    break;
                case "Main/VideoControls/SecondsForwardShort":
                    language.Main.VideoControls.SecondsForwardShort = reader.Value;
                    break;
                case "Main/VideoControls/VideoPosition":
                    language.Main.VideoControls.VideoPosition = reader.Value;
                    break;
                case "Main/VideoControls/TranslateTip":
                    language.Main.VideoControls.TranslateTip = reader.Value;
                    break;
                case "Main/VideoControls/BeforeChangingTimeInWaveformX":
                    language.Main.VideoControls.BeforeChangingTimeInWaveformX = reader.Value;
                    break;
                case "Main/VideoControls/NewTextInsertAtX":
                    language.Main.VideoControls.NewTextInsertAtX = reader.Value;
                    break;
                case "Main/VideoControls/Center":
                    language.Main.VideoControls.Center = reader.Value;
                    break;
                case "Main/VideoControls/PlayRate":
                    language.Main.VideoControls.PlayRate = reader.Value;
                    break;
                case "MatroskaSubtitleChooser/Title":
                    language.MatroskaSubtitleChooser.Title = reader.Value;
                    break;
                case "MatroskaSubtitleChooser/TitleMp4":
                    language.MatroskaSubtitleChooser.TitleMp4 = reader.Value;
                    break;
                case "MatroskaSubtitleChooser/PleaseChoose":
                    language.MatroskaSubtitleChooser.PleaseChoose = reader.Value;
                    break;
                case "MatroskaSubtitleChooser/TrackXLanguageYTypeZ":
                    language.MatroskaSubtitleChooser.TrackXLanguageYTypeZ = reader.Value;
                    break;
                case "MeasurementConverter/Title":
                    language.MeasurementConverter.Title = reader.Value;
                    break;
                case "MeasurementConverter/ConvertFrom":
                    language.MeasurementConverter.ConvertFrom = reader.Value;
                    break;
                case "MeasurementConverter/ConvertTo":
                    language.MeasurementConverter.ConvertTo = reader.Value;
                    break;
                case "MeasurementConverter/CopyToClipboard":
                    language.MeasurementConverter.CopyToClipboard = reader.Value;
                    break;
                case "MeasurementConverter/Celsius":
                    language.MeasurementConverter.Celsius = reader.Value;
                    break;
                case "MeasurementConverter/Fahrenheit":
                    language.MeasurementConverter.Fahrenheit = reader.Value;
                    break;
                case "MeasurementConverter/Miles":
                    language.MeasurementConverter.Miles = reader.Value;
                    break;
                case "MeasurementConverter/Kilometers":
                    language.MeasurementConverter.Kilometers = reader.Value;
                    break;
                case "MeasurementConverter/Meters":
                    language.MeasurementConverter.Meters = reader.Value;
                    break;
                case "MeasurementConverter/Yards":
                    language.MeasurementConverter.Yards = reader.Value;
                    break;
                case "MeasurementConverter/Feet":
                    language.MeasurementConverter.Feet = reader.Value;
                    break;
                case "MeasurementConverter/Inches":
                    language.MeasurementConverter.Inches = reader.Value;
                    break;
                case "MeasurementConverter/Pounds":
                    language.MeasurementConverter.Pounds = reader.Value;
                    break;
                case "MeasurementConverter/Kilos":
                    language.MeasurementConverter.Kilos = reader.Value;
                    break;
                case "MergeDoubleLines/Title":
                    language.MergeDoubleLines.Title = reader.Value;
                    break;
                case "MergeDoubleLines/MaxMillisecondsBetweenLines":
                    language.MergeDoubleLines.MaxMillisecondsBetweenLines = reader.Value;
                    break;
                case "MergeDoubleLines/IncludeIncrementing":
                    language.MergeDoubleLines.IncludeIncrementing = reader.Value;
                    break;
                case "MergedShortLines/Title":
                    language.MergedShortLines.Title = reader.Value;
                    break;
                case "MergedShortLines/MaximumCharacters":
                    language.MergedShortLines.MaximumCharacters = reader.Value;
                    break;
                case "MergedShortLines/MaximumMillisecondsBetween":
                    language.MergedShortLines.MaximumMillisecondsBetween = reader.Value;
                    break;
                case "MergedShortLines/NumberOfMergesX":
                    language.MergedShortLines.NumberOfMergesX = reader.Value;
                    break;
                case "MergedShortLines/MergedText":
                    language.MergedShortLines.MergedText = reader.Value;
                    break;
                case "MergedShortLines/OnlyMergeContinuationLines":
                    language.MergedShortLines.OnlyMergeContinuationLines = reader.Value;
                    break;
                case "MergeTextWithSameTimeCodes/Title":
                    language.MergeTextWithSameTimeCodes.Title = reader.Value;
                    break;
                case "MergeTextWithSameTimeCodes/MaxDifferenceMilliseconds":
                    language.MergeTextWithSameTimeCodes.MaxDifferenceMilliseconds = reader.Value;
                    break;
                case "MergeTextWithSameTimeCodes/ReBreakLines":
                    language.MergeTextWithSameTimeCodes.ReBreakLines = reader.Value;
                    break;
                case "MergeTextWithSameTimeCodes/NumberOfMergesX":
                    language.MergeTextWithSameTimeCodes.NumberOfMergesX = reader.Value;
                    break;
                case "MergeTextWithSameTimeCodes/MergedText":
                    language.MergeTextWithSameTimeCodes.MergedText = reader.Value;
                    break;
                case "ModifySelection/Title":
                    language.ModifySelection.Title = reader.Value;
                    break;
                case "ModifySelection/Rule":
                    language.ModifySelection.Rule = reader.Value;
                    break;
                case "ModifySelection/CaseSensitive":
                    language.ModifySelection.CaseSensitive = reader.Value;
                    break;
                case "ModifySelection/DoWithMatches":
                    language.ModifySelection.DoWithMatches = reader.Value;
                    break;
                case "ModifySelection/MakeNewSelection":
                    language.ModifySelection.MakeNewSelection = reader.Value;
                    break;
                case "ModifySelection/AddToCurrentSelection":
                    language.ModifySelection.AddToCurrentSelection = reader.Value;
                    break;
                case "ModifySelection/SubtractFromCurrentSelection":
                    language.ModifySelection.SubtractFromCurrentSelection = reader.Value;
                    break;
                case "ModifySelection/IntersectWithCurrentSelection":
                    language.ModifySelection.IntersectWithCurrentSelection = reader.Value;
                    break;
                case "ModifySelection/MatchingLinesX":
                    language.ModifySelection.MatchingLinesX = reader.Value;
                    break;
                case "ModifySelection/Contains":
                    language.ModifySelection.Contains = reader.Value;
                    break;
                case "ModifySelection/StartsWith":
                    language.ModifySelection.StartsWith = reader.Value;
                    break;
                case "ModifySelection/EndsWith":
                    language.ModifySelection.EndsWith = reader.Value;
                    break;
                case "ModifySelection/NoContains":
                    language.ModifySelection.NoContains = reader.Value;
                    break;
                case "ModifySelection/RegEx":
                    language.ModifySelection.RegEx = reader.Value;
                    break;
                case "ModifySelection/UnequalLines":
                    language.ModifySelection.UnequalLines = reader.Value;
                    break;
                case "ModifySelection/EqualLines":
                    language.ModifySelection.EqualLines = reader.Value;
                    break;
                case "ModifySelection/DurationLessThan":
                    language.ModifySelection.DurationLessThan = reader.Value;
                    break;
                case "ModifySelection/DurationGreaterThan":
                    language.ModifySelection.DurationGreaterThan = reader.Value;
                    break;
                case "MultipleReplace/Title":
                    language.MultipleReplace.Title = reader.Value;
                    break;
                case "MultipleReplace/FindWhat":
                    language.MultipleReplace.FindWhat = reader.Value;
                    break;
                case "MultipleReplace/ReplaceWith":
                    language.MultipleReplace.ReplaceWith = reader.Value;
                    break;
                case "MultipleReplace/Normal":
                    language.MultipleReplace.Normal = reader.Value;
                    break;
                case "MultipleReplace/CaseSensitive":
                    language.MultipleReplace.CaseSensitive = reader.Value;
                    break;
                case "MultipleReplace/RegularExpression":
                    language.MultipleReplace.RegularExpression = reader.Value;
                    break;
                case "MultipleReplace/Description":
                    language.MultipleReplace.Description = reader.Value;
                    break;
                case "MultipleReplace/LinesFoundX":
                    language.MultipleReplace.LinesFoundX = reader.Value;
                    break;
                case "MultipleReplace/Remove":
                    language.MultipleReplace.Remove = reader.Value;
                    break;
                case "MultipleReplace/Add":
                    language.MultipleReplace.Add = reader.Value;
                    break;
                case "MultipleReplace/Update":
                    language.MultipleReplace.Update = reader.Value;
                    break;
                case "MultipleReplace/Enabled":
                    language.MultipleReplace.Enabled = reader.Value;
                    break;
                case "MultipleReplace/SearchType":
                    language.MultipleReplace.SearchType = reader.Value;
                    break;
                case "MultipleReplace/RemoveAll":
                    language.MultipleReplace.RemoveAll = reader.Value;
                    break;
                case "MultipleReplace/Import":
                    language.MultipleReplace.Import = reader.Value;
                    break;
                case "MultipleReplace/Export":
                    language.MultipleReplace.Export = reader.Value;
                    break;
                case "MultipleReplace/ImportRulesTitle":
                    language.MultipleReplace.ImportRulesTitle = reader.Value;
                    break;
                case "MultipleReplace/ExportRulesTitle":
                    language.MultipleReplace.ExportRulesTitle = reader.Value;
                    break;
                case "MultipleReplace/ChooseGroupsToImport":
                    language.MultipleReplace.ChooseGroupsToImport = reader.Value;
                    break;
                case "MultipleReplace/ChooseGroupsToExport":
                    language.MultipleReplace.ChooseGroupsToExport = reader.Value;
                    break;
                case "MultipleReplace/Rules":
                    language.MultipleReplace.Rules = reader.Value;
                    break;
                case "MultipleReplace/MoveToTop":
                    language.MultipleReplace.MoveToTop = reader.Value;
                    break;
                case "MultipleReplace/MoveToBottom":
                    language.MultipleReplace.MoveToBottom = reader.Value;
                    break;
                case "MultipleReplace/MoveSelectedRulesToGroup":
                    language.MultipleReplace.MoveSelectedRulesToGroup = reader.Value;
                    break;
                case "MultipleReplace/Groups":
                    language.MultipleReplace.Groups = reader.Value;
                    break;
                case "MultipleReplace/RulesForGroupX":
                    language.MultipleReplace.RulesForGroupX = reader.Value;
                    break;
                case "MultipleReplace/GroupName":
                    language.MultipleReplace.GroupName = reader.Value;
                    break;
                case "MultipleReplace/RenameGroup":
                    language.MultipleReplace.RenameGroup = reader.Value;
                    break;
                case "MultipleReplace/NewGroup":
                    language.MultipleReplace.NewGroup = reader.Value;
                    break;
                case "MultipleReplace/NothingToImport":
                    language.MultipleReplace.NothingToImport = reader.Value;
                    break;
                case "NetworkChat/Title":
                    language.NetworkChat.Title = reader.Value;
                    break;
                case "NetworkChat/Send":
                    language.NetworkChat.Send = reader.Value;
                    break;
                case "NetworkJoin/Title":
                    language.NetworkJoin.Title = reader.Value;
                    break;
                case "NetworkJoin/Information":
                    language.NetworkJoin.Information = reader.Value;
                    break;
                case "NetworkJoin/Join":
                    language.NetworkJoin.Join = reader.Value;
                    break;
                case "NetworkLogAndInfo/Title":
                    language.NetworkLogAndInfo.Title = reader.Value;
                    break;
                case "NetworkLogAndInfo/Log":
                    language.NetworkLogAndInfo.Log = reader.Value;
                    break;
                case "NetworkStart/Title":
                    language.NetworkStart.Title = reader.Value;
                    break;
                case "NetworkStart/ConnectionTo":
                    language.NetworkStart.ConnectionTo = reader.Value;
                    break;
                case "NetworkStart/Information":
                    language.NetworkStart.Information = reader.Value;
                    break;
                case "NetworkStart/Start":
                    language.NetworkStart.Start = reader.Value;
                    break;
                case "OpenVideoDvd/Title":
                    language.OpenVideoDvd.Title = reader.Value;
                    break;
                case "OpenVideoDvd/OpenDvdFrom":
                    language.OpenVideoDvd.OpenDvdFrom = reader.Value;
                    break;
                case "OpenVideoDvd/Disc":
                    language.OpenVideoDvd.Disc = reader.Value;
                    break;
                case "OpenVideoDvd/Folder":
                    language.OpenVideoDvd.Folder = reader.Value;
                    break;
                case "OpenVideoDvd/ChooseDrive":
                    language.OpenVideoDvd.ChooseDrive = reader.Value;
                    break;
                case "OpenVideoDvd/ChooseFolder":
                    language.OpenVideoDvd.ChooseFolder = reader.Value;
                    break;
                case "PluginsGet/Title":
                    language.PluginsGet.Title = reader.Value;
                    break;
                case "PluginsGet/InstalledPlugins":
                    language.PluginsGet.InstalledPlugins = reader.Value;
                    break;
                case "PluginsGet/GetPlugins":
                    language.PluginsGet.GetPlugins = reader.Value;
                    break;
                case "PluginsGet/Description":
                    language.PluginsGet.Description = reader.Value;
                    break;
                case "PluginsGet/Version":
                    language.PluginsGet.Version = reader.Value;
                    break;
                case "PluginsGet/Date":
                    language.PluginsGet.Date = reader.Value;
                    break;
                case "PluginsGet/Type":
                    language.PluginsGet.Type = reader.Value;
                    break;
                case "PluginsGet/OpenPluginsFolder":
                    language.PluginsGet.OpenPluginsFolder = reader.Value;
                    break;
                case "PluginsGet/GetPluginsInfo1":
                    language.PluginsGet.GetPluginsInfo1 = reader.Value;
                    break;
                case "PluginsGet/GetPluginsInfo2":
                    language.PluginsGet.GetPluginsInfo2 = reader.Value;
                    break;
                case "PluginsGet/PluginXDownloaded":
                    language.PluginsGet.PluginXDownloaded = reader.Value;
                    break;
                case "PluginsGet/Download":
                    language.PluginsGet.Download = reader.Value;
                    break;
                case "PluginsGet/Remove":
                    language.PluginsGet.Remove = reader.Value;
                    break;
                case "PluginsGet/UpdateAllX":
                    language.PluginsGet.UpdateAllX = reader.Value;
                    break;
                case "PluginsGet/UnableToDownloadPluginListX":
                    language.PluginsGet.UnableToDownloadPluginListX = reader.Value;
                    break;
                case "PluginsGet/NewVersionOfSubtitleEditRequired":
                    language.PluginsGet.NewVersionOfSubtitleEditRequired = reader.Value;
                    break;
                case "PluginsGet/UpdateAvailable":
                    language.PluginsGet.UpdateAvailable = reader.Value;
                    break;
                case "PluginsGet/UpdateAll":
                    language.PluginsGet.UpdateAll = reader.Value;
                    break;
                case "PluginsGet/XPluginsUpdated":
                    language.PluginsGet.XPluginsUpdated = reader.Value;
                    break;
                case "RegularExpressionContextMenu/WordBoundary":
                    language.RegularExpressionContextMenu.WordBoundary = reader.Value;
                    break;
                case "RegularExpressionContextMenu/NonWordBoundary":
                    language.RegularExpressionContextMenu.NonWordBoundary = reader.Value;
                    break;
                case "RegularExpressionContextMenu/NewLine":
                    language.RegularExpressionContextMenu.NewLine = reader.Value;
                    break;
                case "RegularExpressionContextMenu/NewLineShort":
                    language.RegularExpressionContextMenu.NewLineShort = reader.Value;
                    break;
                case "RegularExpressionContextMenu/AnyDigit":
                    language.RegularExpressionContextMenu.AnyDigit = reader.Value;
                    break;
                case "RegularExpressionContextMenu/NonDigit":
                    language.RegularExpressionContextMenu.NonDigit = reader.Value;
                    break;
                case "RegularExpressionContextMenu/AnyCharacter":
                    language.RegularExpressionContextMenu.AnyCharacter = reader.Value;
                    break;
                case "RegularExpressionContextMenu/AnyWhitespace":
                    language.RegularExpressionContextMenu.AnyWhitespace = reader.Value;
                    break;
                case "RegularExpressionContextMenu/NonSpaceCharacter":
                    language.RegularExpressionContextMenu.NonSpaceCharacter = reader.Value;
                    break;
                case "RegularExpressionContextMenu/ZeroOrMore":
                    language.RegularExpressionContextMenu.ZeroOrMore = reader.Value;
                    break;
                case "RegularExpressionContextMenu/OneOrMore":
                    language.RegularExpressionContextMenu.OneOrMore = reader.Value;
                    break;
                case "RegularExpressionContextMenu/InCharacterGroup":
                    language.RegularExpressionContextMenu.InCharacterGroup = reader.Value;
                    break;
                case "RegularExpressionContextMenu/NotInCharacterGroup":
                    language.RegularExpressionContextMenu.NotInCharacterGroup = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/Title":
                    language.RemoveTextFromHearImpaired.Title = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/RemoveTextConditions":
                    language.RemoveTextFromHearImpaired.RemoveTextConditions = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/RemoveTextBetween":
                    language.RemoveTextFromHearImpaired.RemoveTextBetween = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/SquareBrackets":
                    language.RemoveTextFromHearImpaired.SquareBrackets = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/Brackets":
                    language.RemoveTextFromHearImpaired.Brackets = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/Parentheses":
                    language.RemoveTextFromHearImpaired.Parentheses = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/QuestionMarks":
                    language.RemoveTextFromHearImpaired.QuestionMarks = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/And":
                    language.RemoveTextFromHearImpaired.And = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/RemoveTextBeforeColon":
                    language.RemoveTextFromHearImpaired.RemoveTextBeforeColon = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/OnlyIfTextIsUppercase":
                    language.RemoveTextFromHearImpaired.OnlyIfTextIsUppercase = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/OnlyIfInSeparateLine":
                    language.RemoveTextFromHearImpaired.OnlyIfInSeparateLine = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/LinesFoundX":
                    language.RemoveTextFromHearImpaired.LinesFoundX = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/RemoveTextIfContains":
                    language.RemoveTextFromHearImpaired.RemoveTextIfContains = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/RemoveTextIfAllUppercase":
                    language.RemoveTextFromHearImpaired.RemoveTextIfAllUppercase = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/RemoveInterjections":
                    language.RemoveTextFromHearImpaired.RemoveInterjections = reader.Value;
                    break;
                case "RemoveTextFromHearImpaired/EditInterjections":
                    language.RemoveTextFromHearImpaired.EditInterjections = reader.Value;
                    break;
                case "ReplaceDialog/Title":
                    language.ReplaceDialog.Title = reader.Value;
                    break;
                case "ReplaceDialog/FindWhat":
                    language.ReplaceDialog.FindWhat = reader.Value;
                    break;
                case "ReplaceDialog/Normal":
                    language.ReplaceDialog.Normal = reader.Value;
                    break;
                case "ReplaceDialog/CaseSensitive":
                    language.ReplaceDialog.CaseSensitive = reader.Value;
                    break;
                case "ReplaceDialog/RegularExpression":
                    language.ReplaceDialog.RegularExpression = reader.Value;
                    break;
                case "ReplaceDialog/ReplaceWith":
                    language.ReplaceDialog.ReplaceWith = reader.Value;
                    break;
                case "ReplaceDialog/Find":
                    language.ReplaceDialog.Find = reader.Value;
                    break;
                case "ReplaceDialog/Replace":
                    language.ReplaceDialog.Replace = reader.Value;
                    break;
                case "ReplaceDialog/ReplaceAll":
                    language.ReplaceDialog.ReplaceAll = reader.Value;
                    break;
                case "RestoreAutoBackup/Title":
                    language.RestoreAutoBackup.Title = reader.Value;
                    break;
                case "RestoreAutoBackup/Information":
                    language.RestoreAutoBackup.Information = reader.Value;
                    break;
                case "RestoreAutoBackup/DateAndTime":
                    language.RestoreAutoBackup.DateAndTime = reader.Value;
                    break;
                case "RestoreAutoBackup/FileName":
                    language.RestoreAutoBackup.FileName = reader.Value;
                    break;
                case "RestoreAutoBackup/Extension":
                    language.RestoreAutoBackup.Extension = reader.Value;
                    break;
                case "RestoreAutoBackup/NoBackedUpFilesFound":
                    language.RestoreAutoBackup.NoBackedUpFilesFound = reader.Value;
                    break;
                case "SeekSilence/Title":
                    language.SeekSilence.Title = reader.Value;
                    break;
                case "SeekSilence/SearchDirection":
                    language.SeekSilence.SearchDirection = reader.Value;
                    break;
                case "SeekSilence/Forward":
                    language.SeekSilence.Forward = reader.Value;
                    break;
                case "SeekSilence/Back":
                    language.SeekSilence.Back = reader.Value;
                    break;
                case "SeekSilence/LengthInSeconds":
                    language.SeekSilence.LengthInSeconds = reader.Value;
                    break;
                case "SeekSilence/MaxVolume":
                    language.SeekSilence.MaxVolume = reader.Value;
                    break;
                case "SetMinimumDisplayTimeBetweenParagraphs/Title":
                    language.SetMinimumDisplayTimeBetweenParagraphs.Title = reader.Value;
                    break;
                case "SetMinimumDisplayTimeBetweenParagraphs/PreviewLinesModifiedX":
                    language.SetMinimumDisplayTimeBetweenParagraphs.PreviewLinesModifiedX = reader.Value;
                    break;
                case "SetMinimumDisplayTimeBetweenParagraphs/ShowOnlyModifiedLines":
                    language.SetMinimumDisplayTimeBetweenParagraphs.ShowOnlyModifiedLines = reader.Value;
                    break;
                case "SetMinimumDisplayTimeBetweenParagraphs/MinimumMillisecondsBetweenParagraphs":
                    language.SetMinimumDisplayTimeBetweenParagraphs.MinimumMillisecondsBetweenParagraphs = reader.Value;
                    break;
                case "SetMinimumDisplayTimeBetweenParagraphs/FrameInfo":
                    language.SetMinimumDisplayTimeBetweenParagraphs.FrameInfo = reader.Value;
                    break;
                case "SetMinimumDisplayTimeBetweenParagraphs/OneFrameXisYMilliseconds":
                    language.SetMinimumDisplayTimeBetweenParagraphs.OneFrameXisYMilliseconds = reader.Value;
                    break;
                case "SetSyncPoint/Title":
                    language.SetSyncPoint.Title = reader.Value;
                    break;
                case "SetSyncPoint/SyncPointTimeCode":
                    language.SetSyncPoint.SyncPointTimeCode = reader.Value;
                    break;
                case "SetSyncPoint/ThreeSecondsBack":
                    language.SetSyncPoint.ThreeSecondsBack = reader.Value;
                    break;
                case "SetSyncPoint/HalfASecondBack":
                    language.SetSyncPoint.HalfASecondBack = reader.Value;
                    break;
                case "SetSyncPoint/HalfASecondForward":
                    language.SetSyncPoint.HalfASecondForward = reader.Value;
                    break;
                case "SetSyncPoint/ThreeSecondsForward":
                    language.SetSyncPoint.ThreeSecondsForward = reader.Value;
                    break;
                case "Settings/Title":
                    language.Settings.Title = reader.Value;
                    break;
                case "Settings/General":
                    language.Settings.General = reader.Value;
                    break;
                case "Settings/Toolbar":
                    language.Settings.Toolbar = reader.Value;
                    break;
                case "Settings/VideoPlayer":
                    language.Settings.VideoPlayer = reader.Value;
                    break;
                case "Settings/WaveformAndSpectrogram":
                    language.Settings.WaveformAndSpectrogram = reader.Value;
                    break;
                case "Settings/Tools":
                    language.Settings.Tools = reader.Value;
                    break;
                case "Settings/WordLists":
                    language.Settings.WordLists = reader.Value;
                    break;
                case "Settings/SsaStyle":
                    language.Settings.SsaStyle = reader.Value;
                    break;
                case "Settings/Network":
                    language.Settings.Network = reader.Value;
                    break;
                case "Settings/Rules":
                    language.Settings.Rules = reader.Value;
                    break;
                case "Settings/ShowToolBarButtons":
                    language.Settings.ShowToolBarButtons = reader.Value;
                    break;
                case "Settings/New":
                    language.Settings.New = reader.Value;
                    break;
                case "Settings/Open":
                    language.Settings.Open = reader.Value;
                    break;
                case "Settings/Save":
                    language.Settings.Save = reader.Value;
                    break;
                case "Settings/SaveAs":
                    language.Settings.SaveAs = reader.Value;
                    break;
                case "Settings/Find":
                    language.Settings.Find = reader.Value;
                    break;
                case "Settings/Replace":
                    language.Settings.Replace = reader.Value;
                    break;
                case "Settings/VisualSync":
                    language.Settings.VisualSync = reader.Value;
                    break;
                case "Settings/SpellCheck":
                    language.Settings.SpellCheck = reader.Value;
                    break;
                case "Settings/NetflixQualityCheck":
                    language.Settings.NetflixQualityCheck = reader.Value;
                    break;
                case "Settings/SettingsName":
                    language.Settings.SettingsName = reader.Value;
                    break;
                case "Settings/Help":
                    language.Settings.Help = reader.Value;
                    break;
                case "Settings/UnbreakNoSpace":
                    language.Settings.UnbreakNoSpace = reader.Value;
                    break;
                case "Settings/ToggleBookmarks":
                    language.Settings.ToggleBookmarks = reader.Value;
                    break;
                case "Settings/ToggleBookmarksWithComment":
                    language.Settings.ToggleBookmarksWithComment = reader.Value;
                    break;
                case "Settings/ClearBookmarks":
                    language.Settings.ClearBookmarks = reader.Value;
                    break;
                case "Settings/GoToBookmark":
                    language.Settings.GoToBookmark = reader.Value;
                    break;
                case "Settings/GoToPreviousBookmark":
                    language.Settings.GoToPreviousBookmark = reader.Value;
                    break;
                case "Settings/GoToNextBookmark":
                    language.Settings.GoToNextBookmark = reader.Value;
                    break;
                case "Settings/ChooseProfile":
                    language.Settings.ChooseProfile = reader.Value;
                    break;
                case "Settings/DuplicateLine":
                    language.Settings.DuplicateLine = reader.Value;
                    break;
                case "Settings/FontInUi":
                    language.Settings.FontInUi = reader.Value;
                    break;
                case "Settings/ShowFrameRate":
                    language.Settings.ShowFrameRate = reader.Value;
                    break;
                case "Settings/DefaultFrameRate":
                    language.Settings.DefaultFrameRate = reader.Value;
                    break;
                case "Settings/DefaultFileEncoding":
                    language.Settings.DefaultFileEncoding = reader.Value;
                    break;
                case "Settings/AutoDetectAnsiEncoding":
                    language.Settings.AutoDetectAnsiEncoding = reader.Value;
                    break;
                case "Settings/Profile":
                    language.Settings.Profile = reader.Value;
                    break;
                case "Settings/Profiles":
                    language.Settings.Profiles = reader.Value;
                    break;
                case "Settings/ImportProfiles":
                    language.Settings.ImportProfiles = reader.Value;
                    break;
                case "Settings/ExportProfiles":
                    language.Settings.ExportProfiles = reader.Value;
                    break;
                case "Settings/SubtitleLineMaximumLength":
                    language.Settings.SubtitleLineMaximumLength = reader.Value;
                    break;
                case "Settings/OptimalCharactersPerSecond":
                    language.Settings.OptimalCharactersPerSecond = reader.Value;
                    break;
                case "Settings/MaximumCharactersPerSecond":
                    language.Settings.MaximumCharactersPerSecond = reader.Value;
                    break;
                case "Settings/MaximumWordssPerMinute":
                    language.Settings.MaximumWordssPerMinute = reader.Value;
                    break;
                case "Settings/AutoWrapWhileTyping":
                    language.Settings.AutoWrapWhileTyping = reader.Value;
                    break;
                case "Settings/DurationMinimumMilliseconds":
                    language.Settings.DurationMinimumMilliseconds = reader.Value;
                    break;
                case "Settings/DurationMaximumMilliseconds":
                    language.Settings.DurationMaximumMilliseconds = reader.Value;
                    break;
                case "Settings/MinimumGapMilliseconds":
                    language.Settings.MinimumGapMilliseconds = reader.Value;
                    break;
                case "Settings/MaximumLines":
                    language.Settings.MaximumLines = reader.Value;
                    break;
                case "Settings/SubtitleFont":
                    language.Settings.SubtitleFont = reader.Value;
                    break;
                case "Settings/SubtitleFontSize":
                    language.Settings.SubtitleFontSize = reader.Value;
                    break;
                case "Settings/SubtitleBold":
                    language.Settings.SubtitleBold = reader.Value;
                    break;
                case "Settings/VideoAutoOpen":
                    language.Settings.VideoAutoOpen = reader.Value;
                    break;
                case "Settings/AllowVolumeBoost":
                    language.Settings.AllowVolumeBoost = reader.Value;
                    break;
                case "Settings/SubtitleCenter":
                    language.Settings.SubtitleCenter = reader.Value;
                    break;
                case "Settings/SubtitleFontColor":
                    language.Settings.SubtitleFontColor = reader.Value;
                    break;
                case "Settings/SubtitleBackgroundColor":
                    language.Settings.SubtitleBackgroundColor = reader.Value;
                    break;
                case "Settings/SpellChecker":
                    language.Settings.SpellChecker = reader.Value;
                    break;
                case "Settings/RememberRecentFiles":
                    language.Settings.RememberRecentFiles = reader.Value;
                    break;
                case "Settings/StartWithLastFileLoaded":
                    language.Settings.StartWithLastFileLoaded = reader.Value;
                    break;
                case "Settings/RememberSelectedLine":
                    language.Settings.RememberSelectedLine = reader.Value;
                    break;
                case "Settings/RememberPositionAndSize":
                    language.Settings.RememberPositionAndSize = reader.Value;
                    break;
                case "Settings/StartInSourceView":
                    language.Settings.StartInSourceView = reader.Value;
                    break;
                case "Settings/RemoveBlankLinesWhenOpening":
                    language.Settings.RemoveBlankLinesWhenOpening = reader.Value;
                    break;
                case "Settings/ShowLineBreaksAs":
                    language.Settings.ShowLineBreaksAs = reader.Value;
                    break;
                case "Settings/SaveAsFileNameFrom":
                    language.Settings.SaveAsFileNameFrom = reader.Value;
                    break;
                case "Settings/MainListViewDoubleClickAction":
                    language.Settings.MainListViewDoubleClickAction = reader.Value;
                    break;
                case "Settings/MainListViewColumnsInfo":
                    language.Settings.MainListViewColumnsInfo = reader.Value;
                    break;
                case "Settings/MainListViewNothing":
                    language.Settings.MainListViewNothing = reader.Value;
                    break;
                case "Settings/MainListViewVideoGoToPositionAndPause":
                    language.Settings.MainListViewVideoGoToPositionAndPause = reader.Value;
                    break;
                case "Settings/MainListViewVideoGoToPositionAndPlay":
                    language.Settings.MainListViewVideoGoToPositionAndPlay = reader.Value;
                    break;
                case "Settings/MainListViewEditText":
                    language.Settings.MainListViewEditText = reader.Value;
                    break;
                case "Settings/MainListViewVideoGoToPositionMinus1SecAndPause":
                    language.Settings.MainListViewVideoGoToPositionMinus1SecAndPause = reader.Value;
                    break;
                case "Settings/MainListViewVideoGoToPositionMinusHalfSecAndPause":
                    language.Settings.MainListViewVideoGoToPositionMinusHalfSecAndPause = reader.Value;
                    break;
                case "Settings/MainListViewVideoGoToPositionMinus1SecAndPlay":
                    language.Settings.MainListViewVideoGoToPositionMinus1SecAndPlay = reader.Value;
                    break;
                case "Settings/MainListViewEditTextAndPause":
                    language.Settings.MainListViewEditTextAndPause = reader.Value;
                    break;
                case "Settings/VideoFileName":
                    language.Settings.VideoFileName = reader.Value;
                    break;
                case "Settings/ExistingFileName":
                    language.Settings.ExistingFileName = reader.Value;
                    break;
                case "Settings/AutoBackup":
                    language.Settings.AutoBackup = reader.Value;
                    break;
                case "Settings/AutoBackupEveryMinute":
                    language.Settings.AutoBackupEveryMinute = reader.Value;
                    break;
                case "Settings/AutoBackupEveryFiveMinutes":
                    language.Settings.AutoBackupEveryFiveMinutes = reader.Value;
                    break;
                case "Settings/AutoBackupEveryFifteenMinutes":
                    language.Settings.AutoBackupEveryFifteenMinutes = reader.Value;
                    break;
                case "Settings/AutoBackupDeleteAfter":
                    language.Settings.AutoBackupDeleteAfter = reader.Value;
                    break;
                case "Settings/AutoBackupDeleteAfterOneMonth":
                    language.Settings.AutoBackupDeleteAfterOneMonth = reader.Value;
                    break;
                case "Settings/AutoBackupDeleteAfterThreeMonths":
                    language.Settings.AutoBackupDeleteAfterThreeMonths = reader.Value;
                    break;
                case "Settings/AutoBackupDeleteAfterSixMonths":
                    language.Settings.AutoBackupDeleteAfterSixMonths = reader.Value;
                    break;
                case "Settings/CheckForUpdates":
                    language.Settings.CheckForUpdates = reader.Value;
                    break;
                case "Settings/AutoSave":
                    language.Settings.AutoSave = reader.Value;
                    break;
                case "Settings/AllowEditOfOriginalSubtitle":
                    language.Settings.AllowEditOfOriginalSubtitle = reader.Value;
                    break;
                case "Settings/PromptDeleteLines":
                    language.Settings.PromptDeleteLines = reader.Value;
                    break;
                case "Settings/TimeCodeMode":
                    language.Settings.TimeCodeMode = reader.Value;
                    break;
                case "Settings/TimeCodeModeHHMMSSMS":
                    language.Settings.TimeCodeModeHHMMSSMS = reader.Value;
                    break;
                case "Settings/TimeCodeModeHHMMSSFF":
                    language.Settings.TimeCodeModeHHMMSSFF = reader.Value;
                    break;
                case "Settings/VideoEngine":
                    language.Settings.VideoEngine = reader.Value;
                    break;
                case "Settings/DirectShow":
                    language.Settings.DirectShow = reader.Value;
                    break;
                case "Settings/DirectShowDescription":
                    language.Settings.DirectShowDescription = reader.Value;
                    break;
                case "Settings/MpcHc":
                    language.Settings.MpcHc = reader.Value;
                    break;
                case "Settings/MpcHcDescription":
                    language.Settings.MpcHcDescription = reader.Value;
                    break;
                case "Settings/MpvPlayer":
                    language.Settings.MpvPlayer = reader.Value;
                    break;
                case "Settings/MpvPlayerDescription":
                    language.Settings.MpvPlayerDescription = reader.Value;
                    break;
                case "Settings/MpvHandlesPreviewText":
                    language.Settings.MpvHandlesPreviewText = reader.Value;
                    break;
                case "Settings/VlcMediaPlayer":
                    language.Settings.VlcMediaPlayer = reader.Value;
                    break;
                case "Settings/VlcMediaPlayerDescription":
                    language.Settings.VlcMediaPlayerDescription = reader.Value;
                    break;
                case "Settings/VlcBrowseToLabel":
                    language.Settings.VlcBrowseToLabel = reader.Value;
                    break;
                case "Settings/ShowStopButton":
                    language.Settings.ShowStopButton = reader.Value;
                    break;
                case "Settings/ShowMuteButton":
                    language.Settings.ShowMuteButton = reader.Value;
                    break;
                case "Settings/ShowFullscreenButton":
                    language.Settings.ShowFullscreenButton = reader.Value;
                    break;
                case "Settings/PreviewFontSize":
                    language.Settings.PreviewFontSize = reader.Value;
                    break;
                case "Settings/MainWindowVideoControls":
                    language.Settings.MainWindowVideoControls = reader.Value;
                    break;
                case "Settings/CustomSearchTextAndUrl":
                    language.Settings.CustomSearchTextAndUrl = reader.Value;
                    break;
                case "Settings/WaveformAppearance":
                    language.Settings.WaveformAppearance = reader.Value;
                    break;
                case "Settings/WaveformGridColor":
                    language.Settings.WaveformGridColor = reader.Value;
                    break;
                case "Settings/WaveformShowGridLines":
                    language.Settings.WaveformShowGridLines = reader.Value;
                    break;
                case "Settings/WaveformShowCps":
                    language.Settings.WaveformShowCps = reader.Value;
                    break;
                case "Settings/WaveformShowWpm":
                    language.Settings.WaveformShowWpm = reader.Value;
                    break;
                case "Settings/ReverseMouseWheelScrollDirection":
                    language.Settings.ReverseMouseWheelScrollDirection = reader.Value;
                    break;
                case "Settings/WaveformAllowOverlap":
                    language.Settings.WaveformAllowOverlap = reader.Value;
                    break;
                case "Settings/WaveformSetVideoPosMoveStartEnd":
                    language.Settings.WaveformSetVideoPosMoveStartEnd = reader.Value;
                    break;
                case "Settings/WaveformFocusMouseEnter":
                    language.Settings.WaveformFocusMouseEnter = reader.Value;
                    break;
                case "Settings/WaveformListViewFocusMouseEnter":
                    language.Settings.WaveformListViewFocusMouseEnter = reader.Value;
                    break;
                case "Settings/WaveformBorderHitMs1":
                    language.Settings.WaveformBorderHitMs1 = reader.Value;
                    break;
                case "Settings/WaveformBorderHitMs2":
                    language.Settings.WaveformBorderHitMs2 = reader.Value;
                    break;
                case "Settings/WaveformColor":
                    language.Settings.WaveformColor = reader.Value;
                    break;
                case "Settings/WaveformSelectedColor":
                    language.Settings.WaveformSelectedColor = reader.Value;
                    break;
                case "Settings/WaveformBackgroundColor":
                    language.Settings.WaveformBackgroundColor = reader.Value;
                    break;
                case "Settings/WaveformTextColor":
                    language.Settings.WaveformTextColor = reader.Value;
                    break;
                case "Settings/WaveformTextFontSize":
                    language.Settings.WaveformTextFontSize = reader.Value;
                    break;
                case "Settings/WaveformAndSpectrogramsFolderEmpty":
                    language.Settings.WaveformAndSpectrogramsFolderEmpty = reader.Value;
                    break;
                case "Settings/WaveformAndSpectrogramsFolderInfo":
                    language.Settings.WaveformAndSpectrogramsFolderInfo = reader.Value;
                    break;
                case "Settings/Spectrogram":
                    language.Settings.Spectrogram = reader.Value;
                    break;
                case "Settings/GenerateSpectrogram":
                    language.Settings.GenerateSpectrogram = reader.Value;
                    break;
                case "Settings/SpectrogramAppearance":
                    language.Settings.SpectrogramAppearance = reader.Value;
                    break;
                case "Settings/SpectrogramOneColorGradient":
                    language.Settings.SpectrogramOneColorGradient = reader.Value;
                    break;
                case "Settings/SpectrogramClassic":
                    language.Settings.SpectrogramClassic = reader.Value;
                    break;
                case "Settings/WaveformUseFFmpeg":
                    language.Settings.WaveformUseFFmpeg = reader.Value;
                    break;
                case "Settings/DownloadFFmpeg":
                    language.Settings.DownloadFFmpeg = reader.Value;
                    break;
                case "Settings/WaveformFFmpegPath":
                    language.Settings.WaveformFFmpegPath = reader.Value;
                    break;
                case "Settings/WaveformBrowseToFFmpeg":
                    language.Settings.WaveformBrowseToFFmpeg = reader.Value;
                    break;
                case "Settings/WaveformBrowseToVLC":
                    language.Settings.WaveformBrowseToVLC = reader.Value;
                    break;
                case "Settings/SubStationAlphaStyle":
                    language.Settings.SubStationAlphaStyle = reader.Value;
                    break;
                case "Settings/ChooseColor":
                    language.Settings.ChooseColor = reader.Value;
                    break;
                case "Settings/SsaOutline":
                    language.Settings.SsaOutline = reader.Value;
                    break;
                case "Settings/SsaShadow":
                    language.Settings.SsaShadow = reader.Value;
                    break;
                case "Settings/SsaOpaqueBox":
                    language.Settings.SsaOpaqueBox = reader.Value;
                    break;
                case "Settings/Testing123":
                    language.Settings.Testing123 = reader.Value;
                    break;
                case "Settings/Language":
                    language.Settings.Language = reader.Value;
                    break;
                case "Settings/NamesIgnoreLists":
                    language.Settings.NamesIgnoreLists = reader.Value;
                    break;
                case "Settings/AddName":
                    language.Settings.AddName = reader.Value;
                    break;
                case "Settings/AddWord":
                    language.Settings.AddWord = reader.Value;
                    break;
                case "Settings/Remove":
                    language.Settings.Remove = reader.Value;
                    break;
                case "Settings/AddPair":
                    language.Settings.AddPair = reader.Value;
                    break;
                case "Settings/UserWordList":
                    language.Settings.UserWordList = reader.Value;
                    break;
                case "Settings/OcrFixList":
                    language.Settings.OcrFixList = reader.Value;
                    break;
                case "Settings/Location":
                    language.Settings.Location = reader.Value;
                    break;
                case "Settings/UseOnlineNames":
                    language.Settings.UseOnlineNames = reader.Value;
                    break;
                case "Settings/WordAddedX":
                    language.Settings.WordAddedX = reader.Value;
                    break;
                case "Settings/WordAlreadyExists":
                    language.Settings.WordAlreadyExists = reader.Value;
                    break;
                case "Settings/WordNotFound":
                    language.Settings.WordNotFound = reader.Value;
                    break;
                case "Settings/RemoveX":
                    language.Settings.RemoveX = reader.Value;
                    break;
                case "Settings/CannotUpdateNamesOnline":
                    language.Settings.CannotUpdateNamesOnline = reader.Value;
                    break;
                case "Settings/ProxyServerSettings":
                    language.Settings.ProxyServerSettings = reader.Value;
                    break;
                case "Settings/ProxyAddress":
                    language.Settings.ProxyAddress = reader.Value;
                    break;
                case "Settings/ProxyAuthentication":
                    language.Settings.ProxyAuthentication = reader.Value;
                    break;
                case "Settings/ProxyUserName":
                    language.Settings.ProxyUserName = reader.Value;
                    break;
                case "Settings/ProxyPassword":
                    language.Settings.ProxyPassword = reader.Value;
                    break;
                case "Settings/ProxyDomain":
                    language.Settings.ProxyDomain = reader.Value;
                    break;
                case "Settings/NetworkSessionSettings":
                    language.Settings.NetworkSessionSettings = reader.Value;
                    break;
                case "Settings/NetworkSessionNewSound":
                    language.Settings.NetworkSessionNewSound = reader.Value;
                    break;
                case "Settings/PlayXSecondsAndBack":
                    language.Settings.PlayXSecondsAndBack = reader.Value;
                    break;
                case "Settings/StartSceneIndex":
                    language.Settings.StartSceneIndex = reader.Value;
                    break;
                case "Settings/EndSceneIndex":
                    language.Settings.EndSceneIndex = reader.Value;
                    break;
                case "Settings/FirstPlusX":
                    language.Settings.FirstPlusX = reader.Value;
                    break;
                case "Settings/LastMinusX":
                    language.Settings.LastMinusX = reader.Value;
                    break;
                case "Settings/FixCommonerrors":
                    language.Settings.FixCommonerrors = reader.Value;
                    break;
                case "Settings/RemoveTextForHi":
                    language.Settings.RemoveTextForHi = reader.Value;
                    break;
                case "Settings/MergeLinesShorterThan":
                    language.Settings.MergeLinesShorterThan = reader.Value;
                    break;
                case "Settings/MusicSymbol":
                    language.Settings.MusicSymbol = reader.Value;
                    break;
                case "Settings/MusicSymbolsReplace":
                    language.Settings.MusicSymbolsReplace = reader.Value;
                    break;
                case "Settings/FixCommonOcrErrorsUseHardcodedRules":
                    language.Settings.FixCommonOcrErrorsUseHardcodedRules = reader.Value;
                    break;
                case "Settings/FixCommonerrorsFixShortDisplayTimesAllowMoveStartTime":
                    language.Settings.FixCommonerrorsFixShortDisplayTimesAllowMoveStartTime = reader.Value;
                    break;
                case "Settings/FixCommonErrorsSkipStepOne":
                    language.Settings.FixCommonErrorsSkipStepOne = reader.Value;
                    break;
                case "Settings/Shortcuts":
                    language.Settings.Shortcuts = reader.Value;
                    break;
                case "Settings/Shortcut":
                    language.Settings.Shortcut = reader.Value;
                    break;
                case "Settings/Control":
                    language.Settings.Control = reader.Value;
                    break;
                case "Settings/Alt":
                    language.Settings.Alt = reader.Value;
                    break;
                case "Settings/Shift":
                    language.Settings.Shift = reader.Value;
                    break;
                case "Settings/Key":
                    language.Settings.Key = reader.Value;
                    break;
                case "Settings/ListViewAndTextBox":
                    language.Settings.ListViewAndTextBox = reader.Value;
                    break;
                case "Settings/ListView":
                    language.Settings.ListView = reader.Value;
                    break;
                case "Settings/TextBox":
                    language.Settings.TextBox = reader.Value;
                    break;
                case "Settings/UpdateShortcut":
                    language.Settings.UpdateShortcut = reader.Value;
                    break;
                case "Settings/ToggleDockUndockOfVideoControls":
                    language.Settings.ToggleDockUndockOfVideoControls = reader.Value;
                    break;
                case "Settings/CreateSetEndAddNewAndGoToNew":
                    language.Settings.CreateSetEndAddNewAndGoToNew = reader.Value;
                    break;
                case "Settings/AdjustViaEndAutoStart":
                    language.Settings.AdjustViaEndAutoStart = reader.Value;
                    break;
                case "Settings/AdjustViaEndAutoStartAndGoToNext":
                    language.Settings.AdjustViaEndAutoStartAndGoToNext = reader.Value;
                    break;
                case "Settings/AdjustSetEndTimeAndGoToNext":
                    language.Settings.AdjustSetEndTimeAndGoToNext = reader.Value;
                    break;
                case "Settings/AdjustSetStartAutoDurationAndGoToNext":
                    language.Settings.AdjustSetStartAutoDurationAndGoToNext = reader.Value;
                    break;
                case "Settings/AdjustSetEndNextStartAndGoToNext":
                    language.Settings.AdjustSetEndNextStartAndGoToNext = reader.Value;
                    break;
                case "Settings/AdjustStartDownEndUpAndGoToNext":
                    language.Settings.AdjustStartDownEndUpAndGoToNext = reader.Value;
                    break;
                case "Settings/AdjustSelected100MsForward":
                    language.Settings.AdjustSelected100MsForward = reader.Value;
                    break;
                case "Settings/AdjustSelected100MsBack":
                    language.Settings.AdjustSelected100MsBack = reader.Value;
                    break;
                case "Settings/AdjustStartXMsBack":
                    language.Settings.AdjustStartXMsBack = reader.Value;
                    break;
                case "Settings/AdjustStartXMsForward":
                    language.Settings.AdjustStartXMsForward = reader.Value;
                    break;
                case "Settings/AdjustEndXMsBack":
                    language.Settings.AdjustEndXMsBack = reader.Value;
                    break;
                case "Settings/AdjustEndXMsForward":
                    language.Settings.AdjustEndXMsForward = reader.Value;
                    break;
                case "Settings/AdjustSetStartTimeKeepDuration":
                    language.Settings.AdjustSetStartTimeKeepDuration = reader.Value;
                    break;
                case "Settings/AdjustSetEndAndOffsetTheRest":
                    language.Settings.AdjustSetEndAndOffsetTheRest = reader.Value;
                    break;
                case "Settings/AdjustSetEndAndOffsetTheRestAndGoToNext":
                    language.Settings.AdjustSetEndAndOffsetTheRestAndGoToNext = reader.Value;
                    break;
                case "Settings/AdjustExtendCurrentSubtitle":
                    language.Settings.AdjustExtendCurrentSubtitle = reader.Value;
                    break;
                case "Settings/RecalculateDurationOfCurrentSubtitle":
                    language.Settings.RecalculateDurationOfCurrentSubtitle = reader.Value;
                    break;
                case "Settings/MainCreateStartDownEndUp":
                    language.Settings.MainCreateStartDownEndUp = reader.Value;
                    break;
                case "Settings/MergeDialog":
                    language.Settings.MergeDialog = reader.Value;
                    break;
                case "Settings/GoToNext":
                    language.Settings.GoToNext = reader.Value;
                    break;
                case "Settings/GoToPrevious":
                    language.Settings.GoToPrevious = reader.Value;
                    break;
                case "Settings/GoToCurrentSubtitleStart":
                    language.Settings.GoToCurrentSubtitleStart = reader.Value;
                    break;
                case "Settings/GoToCurrentSubtitleEnd":
                    language.Settings.GoToCurrentSubtitleEnd = reader.Value;
                    break;
                case "Settings/GoToPreviousSubtitleAndFocusVideo":
                    language.Settings.GoToPreviousSubtitleAndFocusVideo = reader.Value;
                    break;
                case "Settings/GoToNextSubtitleAndFocusVideo":
                    language.Settings.GoToNextSubtitleAndFocusVideo = reader.Value;
                    break;
                case "Settings/ToggleFocus":
                    language.Settings.ToggleFocus = reader.Value;
                    break;
                case "Settings/ToggleDialogDashes":
                    language.Settings.ToggleDialogDashes = reader.Value;
                    break;
                case "Settings/ToggleMusicSymbols":
                    language.Settings.ToggleMusicSymbols = reader.Value;
                    break;
                case "Settings/Alignment":
                    language.Settings.Alignment = reader.Value;
                    break;
                case "Settings/AlignmentN1":
                    language.Settings.AlignmentN1 = reader.Value;
                    break;
                case "Settings/AlignmentN2":
                    language.Settings.AlignmentN2 = reader.Value;
                    break;
                case "Settings/AlignmentN3":
                    language.Settings.AlignmentN3 = reader.Value;
                    break;
                case "Settings/AlignmentN4":
                    language.Settings.AlignmentN4 = reader.Value;
                    break;
                case "Settings/AlignmentN5":
                    language.Settings.AlignmentN5 = reader.Value;
                    break;
                case "Settings/AlignmentN6":
                    language.Settings.AlignmentN6 = reader.Value;
                    break;
                case "Settings/AlignmentN7":
                    language.Settings.AlignmentN7 = reader.Value;
                    break;
                case "Settings/AlignmentN8":
                    language.Settings.AlignmentN8 = reader.Value;
                    break;
                case "Settings/AlignmentN9":
                    language.Settings.AlignmentN9 = reader.Value;
                    break;
                case "Settings/CopyTextOnly":
                    language.Settings.CopyTextOnly = reader.Value;
                    break;
                case "Settings/CopyTextOnlyFromOriginalToCurrent":
                    language.Settings.CopyTextOnlyFromOriginalToCurrent = reader.Value;
                    break;
                case "Settings/AutoDurationSelectedLines":
                    language.Settings.AutoDurationSelectedLines = reader.Value;
                    break;
                case "Settings/ReverseStartAndEndingForRtl":
                    language.Settings.ReverseStartAndEndingForRtl = reader.Value;
                    break;
                case "Settings/VerticalZoom":
                    language.Settings.VerticalZoom = reader.Value;
                    break;
                case "Settings/VerticalZoomOut":
                    language.Settings.VerticalZoomOut = reader.Value;
                    break;
                case "Settings/WaveformSeekSilenceForward":
                    language.Settings.WaveformSeekSilenceForward = reader.Value;
                    break;
                case "Settings/WaveformSeekSilenceBack":
                    language.Settings.WaveformSeekSilenceBack = reader.Value;
                    break;
                case "Settings/WaveformAddTextHere":
                    language.Settings.WaveformAddTextHere = reader.Value;
                    break;
                case "Settings/WaveformAddTextHereFromClipboard":
                    language.Settings.WaveformAddTextHereFromClipboard = reader.Value;
                    break;
                case "Settings/SetParagraphAsSelection":
                    language.Settings.SetParagraphAsSelection = reader.Value;
                    break;
                case "Settings/WaveformPlayNewSelection":
                    language.Settings.WaveformPlayNewSelection = reader.Value;
                    break;
                case "Settings/WaveformPlayNewSelectionEnd":
                    language.Settings.WaveformPlayNewSelectionEnd = reader.Value;
                    break;
                case "Settings/WaveformPlayFirstSelectedSubtitle":
                    language.Settings.WaveformPlayFirstSelectedSubtitle = reader.Value;
                    break;
                case "Settings/WaveformFocusListView":
                    language.Settings.WaveformFocusListView = reader.Value;
                    break;
                case "Settings/WaveformGoToPreviousSceneChange":
                    language.Settings.WaveformGoToPreviousSceneChange = reader.Value;
                    break;
                case "Settings/WaveformGoToNextSceneChange":
                    language.Settings.WaveformGoToNextSceneChange = reader.Value;
                    break;
                case "Settings/WaveformToggleSceneChange":
                    language.Settings.WaveformToggleSceneChange = reader.Value;
                    break;
                case "Settings/GoBack1Frame":
                    language.Settings.GoBack1Frame = reader.Value;
                    break;
                case "Settings/GoForward1Frame":
                    language.Settings.GoForward1Frame = reader.Value;
                    break;
                case "Settings/GoBack1FrameWithPlay":
                    language.Settings.GoBack1FrameWithPlay = reader.Value;
                    break;
                case "Settings/GoForward1FrameWithPlay":
                    language.Settings.GoForward1FrameWithPlay = reader.Value;
                    break;
                case "Settings/GoBack100Milliseconds":
                    language.Settings.GoBack100Milliseconds = reader.Value;
                    break;
                case "Settings/GoForward100Milliseconds":
                    language.Settings.GoForward100Milliseconds = reader.Value;
                    break;
                case "Settings/GoBack500Milliseconds":
                    language.Settings.GoBack500Milliseconds = reader.Value;
                    break;
                case "Settings/GoForward500Milliseconds":
                    language.Settings.GoForward500Milliseconds = reader.Value;
                    break;
                case "Settings/GoBack1Second":
                    language.Settings.GoBack1Second = reader.Value;
                    break;
                case "Settings/GoForward1Second":
                    language.Settings.GoForward1Second = reader.Value;
                    break;
                case "Settings/GoBack5Seconds":
                    language.Settings.GoBack5Seconds = reader.Value;
                    break;
                case "Settings/GoForward5Seconds":
                    language.Settings.GoForward5Seconds = reader.Value;
                    break;
                case "Settings/WaveformGoToPrevSubtitle":
                    language.Settings.WaveformGoToPrevSubtitle = reader.Value;
                    break;
                case "Settings/WaveformGoToNextSubtitle":
                    language.Settings.WaveformGoToNextSubtitle = reader.Value;
                    break;
                case "Settings/TogglePlayPause":
                    language.Settings.TogglePlayPause = reader.Value;
                    break;
                case "Settings/Pause":
                    language.Settings.Pause = reader.Value;
                    break;
                case "Settings/Fullscreen":
                    language.Settings.Fullscreen = reader.Value;
                    break;
                case "Settings/PlayRateSlower":
                    language.Settings.PlayRateSlower = reader.Value;
                    break;
                case "Settings/PlayRateFaster":
                    language.Settings.PlayRateFaster = reader.Value;
                    break;
                case "Settings/VideoResetSpeedAndZoom":
                    language.Settings.VideoResetSpeedAndZoom = reader.Value;
                    break;
                case "Settings/CustomSearch1":
                    language.Settings.CustomSearch1 = reader.Value;
                    break;
                case "Settings/CustomSearch2":
                    language.Settings.CustomSearch2 = reader.Value;
                    break;
                case "Settings/CustomSearch3":
                    language.Settings.CustomSearch3 = reader.Value;
                    break;
                case "Settings/CustomSearch4":
                    language.Settings.CustomSearch4 = reader.Value;
                    break;
                case "Settings/CustomSearch5":
                    language.Settings.CustomSearch5 = reader.Value;
                    break;
                case "Settings/SyntaxColoring":
                    language.Settings.SyntaxColoring = reader.Value;
                    break;
                case "Settings/ListViewSyntaxColoring":
                    language.Settings.ListViewSyntaxColoring = reader.Value;
                    break;
                case "Settings/SyntaxColorDurationIfTooSmall":
                    language.Settings.SyntaxColorDurationIfTooSmall = reader.Value;
                    break;
                case "Settings/SyntaxColorDurationIfTooLarge":
                    language.Settings.SyntaxColorDurationIfTooLarge = reader.Value;
                    break;
                case "Settings/SyntaxColorTextIfTooLong":
                    language.Settings.SyntaxColorTextIfTooLong = reader.Value;
                    break;
                case "Settings/SyntaxColorTextMoreThanMaxLines":
                    language.Settings.SyntaxColorTextMoreThanMaxLines = reader.Value;
                    break;
                case "Settings/SyntaxColorOverlap":
                    language.Settings.SyntaxColorOverlap = reader.Value;
                    break;
                case "Settings/SyntaxColorGap":
                    language.Settings.SyntaxColorGap = reader.Value;
                    break;
                case "Settings/SyntaxErrorColor":
                    language.Settings.SyntaxErrorColor = reader.Value;
                    break;
                case "Settings/GoToFirstSelectedLine":
                    language.Settings.GoToFirstSelectedLine = reader.Value;
                    break;
                case "Settings/GoToNextEmptyLine":
                    language.Settings.GoToNextEmptyLine = reader.Value;
                    break;
                case "Settings/MergeSelectedLines":
                    language.Settings.MergeSelectedLines = reader.Value;
                    break;
                case "Settings/MergeSelectedLinesAndAutoBreak":
                    language.Settings.MergeSelectedLinesAndAutoBreak = reader.Value;
                    break;
                case "Settings/MergeSelectedLinesAndUnbreak":
                    language.Settings.MergeSelectedLinesAndUnbreak = reader.Value;
                    break;
                case "Settings/MergeSelectedLinesAndUnbreakCjk":
                    language.Settings.MergeSelectedLinesAndUnbreakCjk = reader.Value;
                    break;
                case "Settings/MergeSelectedLinesOnlyFirstText":
                    language.Settings.MergeSelectedLinesOnlyFirstText = reader.Value;
                    break;
                case "Settings/MergeSelectedLinesBilingual":
                    language.Settings.MergeSelectedLinesBilingual = reader.Value;
                    break;
                case "Settings/SplitSelectedLineBilingual":
                    language.Settings.SplitSelectedLineBilingual = reader.Value;
                    break;
                case "Settings/ToggleTranslationMode":
                    language.Settings.ToggleTranslationMode = reader.Value;
                    break;
                case "Settings/SwitchOriginalAndTranslation":
                    language.Settings.SwitchOriginalAndTranslation = reader.Value;
                    break;
                case "Settings/MergeOriginalAndTranslation":
                    language.Settings.MergeOriginalAndTranslation = reader.Value;
                    break;
                case "Settings/MergeWithNext":
                    language.Settings.MergeWithNext = reader.Value;
                    break;
                case "Settings/MergeWithPrevious":
                    language.Settings.MergeWithPrevious = reader.Value;
                    break;
                case "Settings/ShortcutIsAlreadyDefinedX":
                    language.Settings.ShortcutIsAlreadyDefinedX = reader.Value;
                    break;
                case "Settings/ToggleTranslationAndOriginalInPreviews":
                    language.Settings.ToggleTranslationAndOriginalInPreviews = reader.Value;
                    break;
                case "Settings/ListViewColumnDelete":
                    language.Settings.ListViewColumnDelete = reader.Value;
                    break;
                case "Settings/ListViewColumnDeleteAndShiftUp":
                    language.Settings.ListViewColumnDeleteAndShiftUp = reader.Value;
                    break;
                case "Settings/ListViewColumnInsert":
                    language.Settings.ListViewColumnInsert = reader.Value;
                    break;
                case "Settings/ListViewColumnPaste":
                    language.Settings.ListViewColumnPaste = reader.Value;
                    break;
                case "Settings/ListViewColumnTextUp":
                    language.Settings.ListViewColumnTextUp = reader.Value;
                    break;
                case "Settings/ListViewColumnTextDown":
                    language.Settings.ListViewColumnTextDown = reader.Value;
                    break;
                case "Settings/ListViewFocusWaveform":
                    language.Settings.ListViewFocusWaveform = reader.Value;
                    break;
                case "Settings/ListViewGoToNextError":
                    language.Settings.ListViewGoToNextError = reader.Value;
                    break;
                case "Settings/ShowBeamer":
                    language.Settings.ShowBeamer = reader.Value;
                    break;
                case "Settings/MainTextBoxMoveLastWordDown":
                    language.Settings.MainTextBoxMoveLastWordDown = reader.Value;
                    break;
                case "Settings/MainTextBoxMoveFirstWordFromNextUp":
                    language.Settings.MainTextBoxMoveFirstWordFromNextUp = reader.Value;
                    break;
                case "Settings/MainTextBoxMoveFirstWordUpCurrent":
                    language.Settings.MainTextBoxMoveFirstWordUpCurrent = reader.Value;
                    break;
                case "Settings/MainTextBoxMoveLastWordDownCurrent":
                    language.Settings.MainTextBoxMoveLastWordDownCurrent = reader.Value;
                    break;
                case "Settings/MainTextBoxSelectionToLower":
                    language.Settings.MainTextBoxSelectionToLower = reader.Value;
                    break;
                case "Settings/MainTextBoxSelectionToUpper":
                    language.Settings.MainTextBoxSelectionToUpper = reader.Value;
                    break;
                case "Settings/MainTextBoxSelectionToRuby":
                    language.Settings.MainTextBoxSelectionToRuby = reader.Value;
                    break;
                case "Settings/MainTextBoxToggleAutoDuration":
                    language.Settings.MainTextBoxToggleAutoDuration = reader.Value;
                    break;
                case "Settings/MainTextBoxAutoBreak":
                    language.Settings.MainTextBoxAutoBreak = reader.Value;
                    break;
                case "Settings/MainTextBoxAutoBreakFromPos":
                    language.Settings.MainTextBoxAutoBreakFromPos = reader.Value;
                    break;
                case "Settings/MainTextBoxAutoBreakFromPosAndGoToNext":
                    language.Settings.MainTextBoxAutoBreakFromPosAndGoToNext = reader.Value;
                    break;
                case "Settings/MainTextBoxUnbreak":
                    language.Settings.MainTextBoxUnbreak = reader.Value;
                    break;
                case "Settings/MainFileSaveAll":
                    language.Settings.MainFileSaveAll = reader.Value;
                    break;
                case "Settings/Miscellaneous":
                    language.Settings.Miscellaneous = reader.Value;
                    break;
                case "Settings/CpsIncludesSpace":
                    language.Settings.CpsIncludesSpace = reader.Value;
                    break;
                case "Settings/UseDoNotBreakAfterList":
                    language.Settings.UseDoNotBreakAfterList = reader.Value;
                    break;
                case "Settings/BreakEarlyForLineEnding":
                    language.Settings.BreakEarlyForLineEnding = reader.Value;
                    break;
                case "Settings/BreakByPixelWidth":
                    language.Settings.BreakByPixelWidth = reader.Value;
                    break;
                case "Settings/BreakPreferBottomHeavy":
                    language.Settings.BreakPreferBottomHeavy = reader.Value;
                    break;
                case "Settings/BreakEarlyForDashDialog":
                    language.Settings.BreakEarlyForDashDialog = reader.Value;
                    break;
                case "Settings/BreakEarlyForComma":
                    language.Settings.BreakEarlyForComma = reader.Value;
                    break;
                case "Settings/GoogleTranslate":
                    language.Settings.GoogleTranslate = reader.Value;
                    break;
                case "Settings/GoogleTranslateApiKey":
                    language.Settings.GoogleTranslateApiKey = reader.Value;
                    break;
                case "Settings/MicrosoftBingTranslator":
                    language.Settings.MicrosoftBingTranslator = reader.Value;
                    break;
                case "Settings/HowToSignUp":
                    language.Settings.HowToSignUp = reader.Value;
                    break;
                case "Settings/MicrosoftTranslateApiKey":
                    language.Settings.MicrosoftTranslateApiKey = reader.Value;
                    break;
                case "Settings/MicrosoftTranslateTokenEndpoint":
                    language.Settings.MicrosoftTranslateTokenEndpoint = reader.Value;
                    break;
                case "Settings/FontNote":
                    language.Settings.FontNote = reader.Value;
                    break;
                case "Settings/RestoreDefaultSettings":
                    language.Settings.RestoreDefaultSettings = reader.Value;
                    break;
                case "Settings/RestoreDefaultSettingsMsg":
                    language.Settings.RestoreDefaultSettingsMsg = reader.Value;
                    break;
                case "SettingsMpv/Title":
                    language.SettingsMpv.Title = reader.Value;
                    break;
                case "SettingsMpv/DownloadMpv":
                    language.SettingsMpv.DownloadMpv = reader.Value;
                    break;
                case "SettingsMpv/DownloadMpvFailed":
                    language.SettingsMpv.DownloadMpvFailed = reader.Value;
                    break;
                case "SettingsMpv/DownloadMpvOk":
                    language.SettingsMpv.DownloadMpvOk = reader.Value;
                    break;
                case "SettingsFfmpeg/Title":
                    language.SettingsFfmpeg.Title = reader.Value;
                    break;
                case "SettingsFfmpeg/XDownloadFailed":
                    language.SettingsFfmpeg.XDownloadFailed = reader.Value;
                    break;
                case "SettingsFfmpeg/XDownloadOk":
                    language.SettingsFfmpeg.XDownloadOk = reader.Value;
                    break;
                case "SetVideoOffset/Title":
                    language.SetVideoOffset.Title = reader.Value;
                    break;
                case "SetVideoOffset/Description":
                    language.SetVideoOffset.Description = reader.Value;
                    break;
                case "SetVideoOffset/RelativeToCurrentVideoPosition":
                    language.SetVideoOffset.RelativeToCurrentVideoPosition = reader.Value;
                    break;
                case "SetVideoOffset/KeepTimeCodes":
                    language.SetVideoOffset.KeepTimeCodes = reader.Value;
                    break;
                case "SetVideoOffset/Reset":
                    language.SetVideoOffset.Reset = reader.Value;
                    break;
                case "ShowEarlierLater/Title":
                    language.ShowEarlierLater.Title = reader.Value;
                    break;
                case "ShowEarlierLater/TitleAll":
                    language.ShowEarlierLater.TitleAll = reader.Value;
                    break;
                case "ShowEarlierLater/ShowEarlier":
                    language.ShowEarlierLater.ShowEarlier = reader.Value;
                    break;
                case "ShowEarlierLater/ShowLater":
                    language.ShowEarlierLater.ShowLater = reader.Value;
                    break;
                case "ShowEarlierLater/TotalAdjustmentX":
                    language.ShowEarlierLater.TotalAdjustmentX = reader.Value;
                    break;
                case "ShowEarlierLater/AllLines":
                    language.ShowEarlierLater.AllLines = reader.Value;
                    break;
                case "ShowEarlierLater/SelectedLinesOnly":
                    language.ShowEarlierLater.SelectedLinesOnly = reader.Value;
                    break;
                case "ShowEarlierLater/SelectedLinesAndForward":
                    language.ShowEarlierLater.SelectedLinesAndForward = reader.Value;
                    break;
                case "ShowHistory/Title":
                    language.ShowHistory.Title = reader.Value;
                    break;
                case "ShowHistory/SelectRollbackPoint":
                    language.ShowHistory.SelectRollbackPoint = reader.Value;
                    break;
                case "ShowHistory/Time":
                    language.ShowHistory.Time = reader.Value;
                    break;
                case "ShowHistory/Description":
                    language.ShowHistory.Description = reader.Value;
                    break;
                case "ShowHistory/CompareHistoryItems":
                    language.ShowHistory.CompareHistoryItems = reader.Value;
                    break;
                case "ShowHistory/CompareWithCurrent":
                    language.ShowHistory.CompareWithCurrent = reader.Value;
                    break;
                case "ShowHistory/Rollback":
                    language.ShowHistory.Rollback = reader.Value;
                    break;
                case "SpellCheck/Title":
                    language.SpellCheck.Title = reader.Value;
                    break;
                case "SpellCheck/FullText":
                    language.SpellCheck.FullText = reader.Value;
                    break;
                case "SpellCheck/WordNotFound":
                    language.SpellCheck.WordNotFound = reader.Value;
                    break;
                case "SpellCheck/Language":
                    language.SpellCheck.Language = reader.Value;
                    break;
                case "SpellCheck/Change":
                    language.SpellCheck.Change = reader.Value;
                    break;
                case "SpellCheck/ChangeAll":
                    language.SpellCheck.ChangeAll = reader.Value;
                    break;
                case "SpellCheck/SkipOnce":
                    language.SpellCheck.SkipOnce = reader.Value;
                    break;
                case "SpellCheck/SkipAll":
                    language.SpellCheck.SkipAll = reader.Value;
                    break;
                case "SpellCheck/AddToUserDictionary":
                    language.SpellCheck.AddToUserDictionary = reader.Value;
                    break;
                case "SpellCheck/AddToNamesAndIgnoreList":
                    language.SpellCheck.AddToNamesAndIgnoreList = reader.Value;
                    break;
                case "SpellCheck/AddToOcrReplaceList":
                    language.SpellCheck.AddToOcrReplaceList = reader.Value;
                    break;
                case "SpellCheck/Abort":
                    language.SpellCheck.Abort = reader.Value;
                    break;
                case "SpellCheck/Use":
                    language.SpellCheck.Use = reader.Value;
                    break;
                case "SpellCheck/UseAlways":
                    language.SpellCheck.UseAlways = reader.Value;
                    break;
                case "SpellCheck/Suggestions":
                    language.SpellCheck.Suggestions = reader.Value;
                    break;
                case "SpellCheck/SpellCheckProgress":
                    language.SpellCheck.SpellCheckProgress = reader.Value;
                    break;
                case "SpellCheck/EditWholeText":
                    language.SpellCheck.EditWholeText = reader.Value;
                    break;
                case "SpellCheck/EditWordOnly":
                    language.SpellCheck.EditWordOnly = reader.Value;
                    break;
                case "SpellCheck/AddXToNames":
                    language.SpellCheck.AddXToNames = reader.Value;
                    break;
                case "SpellCheck/AddXToUserDictionary":
                    language.SpellCheck.AddXToUserDictionary = reader.Value;
                    break;
                case "SpellCheck/AutoFixNames":
                    language.SpellCheck.AutoFixNames = reader.Value;
                    break;
                case "SpellCheck/CheckOneLetterWords":
                    language.SpellCheck.CheckOneLetterWords = reader.Value;
                    break;
                case "SpellCheck/TreatINQuoteAsING":
                    language.SpellCheck.TreatINQuoteAsING = reader.Value;
                    break;
                case "SpellCheck/RememberUseAlwaysList":
                    language.SpellCheck.RememberUseAlwaysList = reader.Value;
                    break;
                case "SpellCheck/ImageText":
                    language.SpellCheck.ImageText = reader.Value;
                    break;
                case "SpellCheck/SpellCheckCompleted":
                    language.SpellCheck.SpellCheckCompleted = reader.Value;
                    break;
                case "SpellCheck/SpellCheckAborted":
                    language.SpellCheck.SpellCheckAborted = reader.Value;
                    break;
                case "SpellCheck/SpacesNotAllowed":
                    language.SpellCheck.SpacesNotAllowed = reader.Value;
                    break;
                case "SpellCheck/UndoX":
                    language.SpellCheck.UndoX = reader.Value;
                    break;
                case "NetflixQualityCheck/GlyphCheckReport":
                    language.NetflixQualityCheck.GlyphCheckReport = reader.Value;
                    break;
                case "NetflixQualityCheck/WhiteSpaceCheckReport":
                    language.NetflixQualityCheck.WhiteSpaceCheckReport = reader.Value;
                    break;
                case "NetflixQualityCheck/ReportPrompt":
                    language.NetflixQualityCheck.ReportPrompt = reader.Value;
                    break;
                case "NetflixQualityCheck/OpenReportInFolder":
                    language.NetflixQualityCheck.OpenReportInFolder = reader.Value;
                    break;
                case "NetflixQualityCheck/FoundXIssues":
                    language.NetflixQualityCheck.FoundXIssues = reader.Value;
                    break;
                case "NetflixQualityCheck/CheckOk":
                    language.NetflixQualityCheck.CheckOk = reader.Value;
                    break;
                case "NetflixQualityCheck/MaximumXCharsPerSecond":
                    language.NetflixQualityCheck.MaximumXCharsPerSecond = reader.Value;
                    break;
                case "NetflixQualityCheck/MaximumLineLength":
                    language.NetflixQualityCheck.MaximumLineLength = reader.Value;
                    break;
                case "NetflixQualityCheck/MinimumDuration":
                    language.NetflixQualityCheck.MinimumDuration = reader.Value;
                    break;
                case "Split/Title":
                    language.Split.Title = reader.Value;
                    break;
                case "Split/SplitOptions":
                    language.Split.SplitOptions = reader.Value;
                    break;
                case "Split/Lines":
                    language.Split.Lines = reader.Value;
                    break;
                case "Split/Characters":
                    language.Split.Characters = reader.Value;
                    break;
                case "Split/NumberOfEqualParts":
                    language.Split.NumberOfEqualParts = reader.Value;
                    break;
                case "Split/SubtitleInfo":
                    language.Split.SubtitleInfo = reader.Value;
                    break;
                case "Split/NumberOfLinesX":
                    language.Split.NumberOfLinesX = reader.Value;
                    break;
                case "Split/NumberOfCharactersX":
                    language.Split.NumberOfCharactersX = reader.Value;
                    break;
                case "Split/Output":
                    language.Split.Output = reader.Value;
                    break;
                case "Split/FileName":
                    language.Split.FileName = reader.Value;
                    break;
                case "Split/OutputFolder":
                    language.Split.OutputFolder = reader.Value;
                    break;
                case "Split/DoSplit":
                    language.Split.DoSplit = reader.Value;
                    break;
                case "Split/Basic":
                    language.Split.Basic = reader.Value;
                    break;
                case "SplitLongLines/Title":
                    language.SplitLongLines.Title = reader.Value;
                    break;
                case "SplitLongLines/SingleLineMaximumLength":
                    language.SplitLongLines.SingleLineMaximumLength = reader.Value;
                    break;
                case "SplitLongLines/LineMaximumLength":
                    language.SplitLongLines.LineMaximumLength = reader.Value;
                    break;
                case "SplitLongLines/LineContinuationBeginEndStrings":
                    language.SplitLongLines.LineContinuationBeginEndStrings = reader.Value;
                    break;
                case "SplitLongLines/NumberOfSplits":
                    language.SplitLongLines.NumberOfSplits = reader.Value;
                    break;
                case "SplitLongLines/LongestSingleLineIsXAtY":
                    language.SplitLongLines.LongestSingleLineIsXAtY = reader.Value;
                    break;
                case "SplitLongLines/LongestLineIsXAtY":
                    language.SplitLongLines.LongestLineIsXAtY = reader.Value;
                    break;
                case "SplitSubtitle/Title":
                    language.SplitSubtitle.Title = reader.Value;
                    break;
                case "SplitSubtitle/Description1":
                    language.SplitSubtitle.Description1 = reader.Value;
                    break;
                case "SplitSubtitle/Description2":
                    language.SplitSubtitle.Description2 = reader.Value;
                    break;
                case "SplitSubtitle/Split":
                    language.SplitSubtitle.Split = reader.Value;
                    break;
                case "SplitSubtitle/Done":
                    language.SplitSubtitle.Done = reader.Value;
                    break;
                case "SplitSubtitle/NothingToSplit":
                    language.SplitSubtitle.NothingToSplit = reader.Value;
                    break;
                case "SplitSubtitle/SavePartOneAs":
                    language.SplitSubtitle.SavePartOneAs = reader.Value;
                    break;
                case "SplitSubtitle/SavePartTwoAs":
                    language.SplitSubtitle.SavePartTwoAs = reader.Value;
                    break;
                case "SplitSubtitle/Part1":
                    language.SplitSubtitle.Part1 = reader.Value;
                    break;
                case "SplitSubtitle/Part2":
                    language.SplitSubtitle.Part2 = reader.Value;
                    break;
                case "SplitSubtitle/UnableToSaveFileX":
                    language.SplitSubtitle.UnableToSaveFileX = reader.Value;
                    break;
                case "SplitSubtitle/OverwriteExistingFiles":
                    language.SplitSubtitle.OverwriteExistingFiles = reader.Value;
                    break;
                case "SplitSubtitle/FolderNotFoundX":
                    language.SplitSubtitle.FolderNotFoundX = reader.Value;
                    break;
                case "SplitSubtitle/Untitled":
                    language.SplitSubtitle.Untitled = reader.Value;
                    break;
                case "StartNumberingFrom/Title":
                    language.StartNumberingFrom.Title = reader.Value;
                    break;
                case "StartNumberingFrom/StartFromNumber":
                    language.StartNumberingFrom.StartFromNumber = reader.Value;
                    break;
                case "StartNumberingFrom/PleaseEnterAValidNumber":
                    language.StartNumberingFrom.PleaseEnterAValidNumber = reader.Value;
                    break;
                case "Statistics/Title":
                    language.Statistics.Title = reader.Value;
                    break;
                case "Statistics/TitleWithFileName":
                    language.Statistics.TitleWithFileName = reader.Value;
                    break;
                case "Statistics/GeneralStatistics":
                    language.Statistics.GeneralStatistics = reader.Value;
                    break;
                case "Statistics/MostUsed":
                    language.Statistics.MostUsed = reader.Value;
                    break;
                case "Statistics/MostUsedLines":
                    language.Statistics.MostUsedLines = reader.Value;
                    break;
                case "Statistics/MostUsedWords":
                    language.Statistics.MostUsedWords = reader.Value;
                    break;
                case "Statistics/NothingFound":
                    language.Statistics.NothingFound = reader.Value;
                    break;
                case "Statistics/NumberOfLinesX":
                    language.Statistics.NumberOfLinesX = reader.Value;
                    break;
                case "Statistics/LengthInFormatXinCharactersY":
                    language.Statistics.LengthInFormatXinCharactersY = reader.Value;
                    break;
                case "Statistics/NumberOfCharactersInTextOnly":
                    language.Statistics.NumberOfCharactersInTextOnly = reader.Value;
                    break;
                case "Statistics/TotalDuration":
                    language.Statistics.TotalDuration = reader.Value;
                    break;
                case "Statistics/TotalCharsPerSecond":
                    language.Statistics.TotalCharsPerSecond = reader.Value;
                    break;
                case "Statistics/TotalWords":
                    language.Statistics.TotalWords = reader.Value;
                    break;
                case "Statistics/NumberOfItalicTags":
                    language.Statistics.NumberOfItalicTags = reader.Value;
                    break;
                case "Statistics/NumberOfBoldTags":
                    language.Statistics.NumberOfBoldTags = reader.Value;
                    break;
                case "Statistics/NumberOfUnderlineTags":
                    language.Statistics.NumberOfUnderlineTags = reader.Value;
                    break;
                case "Statistics/NumberOfFontTags":
                    language.Statistics.NumberOfFontTags = reader.Value;
                    break;
                case "Statistics/NumberOfAlignmentTags":
                    language.Statistics.NumberOfAlignmentTags = reader.Value;
                    break;
                case "Statistics/LineLengthMinimum":
                    language.Statistics.LineLengthMinimum = reader.Value;
                    break;
                case "Statistics/LineLengthMaximum":
                    language.Statistics.LineLengthMaximum = reader.Value;
                    break;
                case "Statistics/LineLengthAverage":
                    language.Statistics.LineLengthAverage = reader.Value;
                    break;
                case "Statistics/LinesPerSubtitleAverage":
                    language.Statistics.LinesPerSubtitleAverage = reader.Value;
                    break;
                case "Statistics/SingleLineLengthMinimum":
                    language.Statistics.SingleLineLengthMinimum = reader.Value;
                    break;
                case "Statistics/SingleLineLengthMaximum":
                    language.Statistics.SingleLineLengthMaximum = reader.Value;
                    break;
                case "Statistics/SingleLineLengthAverage":
                    language.Statistics.SingleLineLengthAverage = reader.Value;
                    break;
                case "Statistics/DurationMinimum":
                    language.Statistics.DurationMinimum = reader.Value;
                    break;
                case "Statistics/DurationMaximum":
                    language.Statistics.DurationMaximum = reader.Value;
                    break;
                case "Statistics/DurationAverage":
                    language.Statistics.DurationAverage = reader.Value;
                    break;
                case "Statistics/CharactersPerSecondMinimum":
                    language.Statistics.CharactersPerSecondMinimum = reader.Value;
                    break;
                case "Statistics/CharactersPerSecondMaximum":
                    language.Statistics.CharactersPerSecondMaximum = reader.Value;
                    break;
                case "Statistics/CharactersPerSecondAverage":
                    language.Statistics.CharactersPerSecondAverage = reader.Value;
                    break;
                case "Statistics/Export":
                    language.Statistics.Export = reader.Value;
                    break;
                case "SubStationAlphaProperties/Title":
                    language.SubStationAlphaProperties.Title = reader.Value;
                    break;
                case "SubStationAlphaProperties/TitleSubstationAlpha":
                    language.SubStationAlphaProperties.TitleSubstationAlpha = reader.Value;
                    break;
                case "SubStationAlphaProperties/Script":
                    language.SubStationAlphaProperties.Script = reader.Value;
                    break;
                case "SubStationAlphaProperties/ScriptTitle":
                    language.SubStationAlphaProperties.ScriptTitle = reader.Value;
                    break;
                case "SubStationAlphaProperties/OriginalScript":
                    language.SubStationAlphaProperties.OriginalScript = reader.Value;
                    break;
                case "SubStationAlphaProperties/Translation":
                    language.SubStationAlphaProperties.Translation = reader.Value;
                    break;
                case "SubStationAlphaProperties/Editing":
                    language.SubStationAlphaProperties.Editing = reader.Value;
                    break;
                case "SubStationAlphaProperties/Timing":
                    language.SubStationAlphaProperties.Timing = reader.Value;
                    break;
                case "SubStationAlphaProperties/SyncPoint":
                    language.SubStationAlphaProperties.SyncPoint = reader.Value;
                    break;
                case "SubStationAlphaProperties/UpdatedBy":
                    language.SubStationAlphaProperties.UpdatedBy = reader.Value;
                    break;
                case "SubStationAlphaProperties/UpdateDetails":
                    language.SubStationAlphaProperties.UpdateDetails = reader.Value;
                    break;
                case "SubStationAlphaProperties/Resolution":
                    language.SubStationAlphaProperties.Resolution = reader.Value;
                    break;
                case "SubStationAlphaProperties/VideoResolution":
                    language.SubStationAlphaProperties.VideoResolution = reader.Value;
                    break;
                case "SubStationAlphaProperties/Options":
                    language.SubStationAlphaProperties.Options = reader.Value;
                    break;
                case "SubStationAlphaProperties/WrapStyle":
                    language.SubStationAlphaProperties.WrapStyle = reader.Value;
                    break;
                case "SubStationAlphaProperties/Collision":
                    language.SubStationAlphaProperties.Collision = reader.Value;
                    break;
                case "SubStationAlphaProperties/ScaleBorderAndShadow":
                    language.SubStationAlphaProperties.ScaleBorderAndShadow = reader.Value;
                    break;
                case "SubStationAlphaStyles/Title":
                    language.SubStationAlphaStyles.Title = reader.Value;
                    break;
                case "SubStationAlphaStyles/TitleSubstationAlpha":
                    language.SubStationAlphaStyles.TitleSubstationAlpha = reader.Value;
                    break;
                case "SubStationAlphaStyles/Styles":
                    language.SubStationAlphaStyles.Styles = reader.Value;
                    break;
                case "SubStationAlphaStyles/Properties":
                    language.SubStationAlphaStyles.Properties = reader.Value;
                    break;
                case "SubStationAlphaStyles/Name":
                    language.SubStationAlphaStyles.Name = reader.Value;
                    break;
                case "SubStationAlphaStyles/Font":
                    language.SubStationAlphaStyles.Font = reader.Value;
                    break;
                case "SubStationAlphaStyles/FontName":
                    language.SubStationAlphaStyles.FontName = reader.Value;
                    break;
                case "SubStationAlphaStyles/FontSize":
                    language.SubStationAlphaStyles.FontSize = reader.Value;
                    break;
                case "SubStationAlphaStyles/UseCount":
                    language.SubStationAlphaStyles.UseCount = reader.Value;
                    break;
                case "SubStationAlphaStyles/Primary":
                    language.SubStationAlphaStyles.Primary = reader.Value;
                    break;
                case "SubStationAlphaStyles/Secondary":
                    language.SubStationAlphaStyles.Secondary = reader.Value;
                    break;
                case "SubStationAlphaStyles/Tertiary":
                    language.SubStationAlphaStyles.Tertiary = reader.Value;
                    break;
                case "SubStationAlphaStyles/Outline":
                    language.SubStationAlphaStyles.Outline = reader.Value;
                    break;
                case "SubStationAlphaStyles/Shadow":
                    language.SubStationAlphaStyles.Shadow = reader.Value;
                    break;
                case "SubStationAlphaStyles/Back":
                    language.SubStationAlphaStyles.Back = reader.Value;
                    break;
                case "SubStationAlphaStyles/Alignment":
                    language.SubStationAlphaStyles.Alignment = reader.Value;
                    break;
                case "SubStationAlphaStyles/TopLeft":
                    language.SubStationAlphaStyles.TopLeft = reader.Value;
                    break;
                case "SubStationAlphaStyles/TopCenter":
                    language.SubStationAlphaStyles.TopCenter = reader.Value;
                    break;
                case "SubStationAlphaStyles/TopRight":
                    language.SubStationAlphaStyles.TopRight = reader.Value;
                    break;
                case "SubStationAlphaStyles/MiddleLeft":
                    language.SubStationAlphaStyles.MiddleLeft = reader.Value;
                    break;
                case "SubStationAlphaStyles/MiddleCenter":
                    language.SubStationAlphaStyles.MiddleCenter = reader.Value;
                    break;
                case "SubStationAlphaStyles/MiddleRight":
                    language.SubStationAlphaStyles.MiddleRight = reader.Value;
                    break;
                case "SubStationAlphaStyles/BottomLeft":
                    language.SubStationAlphaStyles.BottomLeft = reader.Value;
                    break;
                case "SubStationAlphaStyles/BottomCenter":
                    language.SubStationAlphaStyles.BottomCenter = reader.Value;
                    break;
                case "SubStationAlphaStyles/BottomRight":
                    language.SubStationAlphaStyles.BottomRight = reader.Value;
                    break;
                case "SubStationAlphaStyles/Colors":
                    language.SubStationAlphaStyles.Colors = reader.Value;
                    break;
                case "SubStationAlphaStyles/Margins":
                    language.SubStationAlphaStyles.Margins = reader.Value;
                    break;
                case "SubStationAlphaStyles/MarginLeft":
                    language.SubStationAlphaStyles.MarginLeft = reader.Value;
                    break;
                case "SubStationAlphaStyles/MarginRight":
                    language.SubStationAlphaStyles.MarginRight = reader.Value;
                    break;
                case "SubStationAlphaStyles/MarginVertical":
                    language.SubStationAlphaStyles.MarginVertical = reader.Value;
                    break;
                case "SubStationAlphaStyles/Border":
                    language.SubStationAlphaStyles.Border = reader.Value;
                    break;
                case "SubStationAlphaStyles/PlusShadow":
                    language.SubStationAlphaStyles.PlusShadow = reader.Value;
                    break;
                case "SubStationAlphaStyles/OpaqueBox":
                    language.SubStationAlphaStyles.OpaqueBox = reader.Value;
                    break;
                case "SubStationAlphaStyles/Import":
                    language.SubStationAlphaStyles.Import = reader.Value;
                    break;
                case "SubStationAlphaStyles/Export":
                    language.SubStationAlphaStyles.Export = reader.Value;
                    break;
                case "SubStationAlphaStyles/Copy":
                    language.SubStationAlphaStyles.Copy = reader.Value;
                    break;
                case "SubStationAlphaStyles/CopyOfY":
                    language.SubStationAlphaStyles.CopyOfY = reader.Value;
                    break;
                case "SubStationAlphaStyles/CopyXOfY":
                    language.SubStationAlphaStyles.CopyXOfY = reader.Value;
                    break;
                case "SubStationAlphaStyles/New":
                    language.SubStationAlphaStyles.New = reader.Value;
                    break;
                case "SubStationAlphaStyles/Remove":
                    language.SubStationAlphaStyles.Remove = reader.Value;
                    break;
                case "SubStationAlphaStyles/RemoveAll":
                    language.SubStationAlphaStyles.RemoveAll = reader.Value;
                    break;
                case "SubStationAlphaStyles/ImportStyleFromFile":
                    language.SubStationAlphaStyles.ImportStyleFromFile = reader.Value;
                    break;
                case "SubStationAlphaStyles/ExportStyleToFile":
                    language.SubStationAlphaStyles.ExportStyleToFile = reader.Value;
                    break;
                case "SubStationAlphaStyles/ChooseStyle":
                    language.SubStationAlphaStyles.ChooseStyle = reader.Value;
                    break;
                case "SubStationAlphaStyles/StyleAlreadyExits":
                    language.SubStationAlphaStyles.StyleAlreadyExits = reader.Value;
                    break;
                case "SubStationAlphaStyles/StyleXExportedToFileY":
                    language.SubStationAlphaStyles.StyleXExportedToFileY = reader.Value;
                    break;
                case "SubStationAlphaStyles/StyleXImportedFromFileY":
                    language.SubStationAlphaStyles.StyleXImportedFromFileY = reader.Value;
                    break;
                case "PointSync/Title":
                    language.PointSync.Title = reader.Value;
                    break;
                case "PointSync/TitleViaOtherSubtitle":
                    language.PointSync.TitleViaOtherSubtitle = reader.Value;
                    break;
                case "PointSync/SyncHelp":
                    language.PointSync.SyncHelp = reader.Value;
                    break;
                case "PointSync/SetSyncPoint":
                    language.PointSync.SetSyncPoint = reader.Value;
                    break;
                case "PointSync/RemoveSyncPoint":
                    language.PointSync.RemoveSyncPoint = reader.Value;
                    break;
                case "PointSync/SyncPointsX":
                    language.PointSync.SyncPointsX = reader.Value;
                    break;
                case "PointSync/Info":
                    language.PointSync.Info = reader.Value;
                    break;
                case "PointSync/ApplySync":
                    language.PointSync.ApplySync = reader.Value;
                    break;
                case "TransportStreamSubtitleChooser/Title":
                    language.TransportStreamSubtitleChooser.Title = reader.Value;
                    break;
                case "TransportStreamSubtitleChooser/PidLineImage":
                    language.TransportStreamSubtitleChooser.PidLineImage = reader.Value;
                    break;
                case "TransportStreamSubtitleChooser/PidLineTeletext":
                    language.TransportStreamSubtitleChooser.PidLineTeletext = reader.Value;
                    break;
                case "TransportStreamSubtitleChooser/SubLine":
                    language.TransportStreamSubtitleChooser.SubLine = reader.Value;
                    break;
                case "UnknownSubtitle/Title":
                    language.UnknownSubtitle.Title = reader.Value;
                    break;
                case "UnknownSubtitle/Message":
                    language.UnknownSubtitle.Message = reader.Value;
                    break;
                case "UnknownSubtitle/ImportAsPlainText":
                    language.UnknownSubtitle.ImportAsPlainText = reader.Value;
                    break;
                case "VisualSync/Title":
                    language.VisualSync.Title = reader.Value;
                    break;
                case "VisualSync/StartScene":
                    language.VisualSync.StartScene = reader.Value;
                    break;
                case "VisualSync/EndScene":
                    language.VisualSync.EndScene = reader.Value;
                    break;
                case "VisualSync/Synchronize":
                    language.VisualSync.Synchronize = reader.Value;
                    break;
                case "VisualSync/HalfASecondBack":
                    language.VisualSync.HalfASecondBack = reader.Value;
                    break;
                case "VisualSync/ThreeSecondsBack":
                    language.VisualSync.ThreeSecondsBack = reader.Value;
                    break;
                case "VisualSync/PlayXSecondsAndBack":
                    language.VisualSync.PlayXSecondsAndBack = reader.Value;
                    break;
                case "VisualSync/FindText":
                    language.VisualSync.FindText = reader.Value;
                    break;
                case "VisualSync/GoToSubPosition":
                    language.VisualSync.GoToSubPosition = reader.Value;
                    break;
                case "VisualSync/KeepChangesTitle":
                    language.VisualSync.KeepChangesTitle = reader.Value;
                    break;
                case "VisualSync/KeepChangesMessage":
                    language.VisualSync.KeepChangesMessage = reader.Value;
                    break;
                case "VisualSync/SynchronizationDone":
                    language.VisualSync.SynchronizationDone = reader.Value;
                    break;
                case "VisualSync/StartSceneMustComeBeforeEndScene":
                    language.VisualSync.StartSceneMustComeBeforeEndScene = reader.Value;
                    break;
                case "VisualSync/Tip":
                    language.VisualSync.Tip = reader.Value;
                    break;
                case "VobSubEditCharacters/Title":
                    language.VobSubEditCharacters.Title = reader.Value;
                    break;
                case "VobSubEditCharacters/ChooseCharacter":
                    language.VobSubEditCharacters.ChooseCharacter = reader.Value;
                    break;
                case "VobSubEditCharacters/ImageCompareFiles":
                    language.VobSubEditCharacters.ImageCompareFiles = reader.Value;
                    break;
                case "VobSubEditCharacters/CurrentCompareImage":
                    language.VobSubEditCharacters.CurrentCompareImage = reader.Value;
                    break;
                case "VobSubEditCharacters/TextAssociatedWithImage":
                    language.VobSubEditCharacters.TextAssociatedWithImage = reader.Value;
                    break;
                case "VobSubEditCharacters/IsItalic":
                    language.VobSubEditCharacters.IsItalic = reader.Value;
                    break;
                case "VobSubEditCharacters/Update":
                    language.VobSubEditCharacters.Update = reader.Value;
                    break;
                case "VobSubEditCharacters/Delete":
                    language.VobSubEditCharacters.Delete = reader.Value;
                    break;
                case "VobSubEditCharacters/ImageDoubleSize":
                    language.VobSubEditCharacters.ImageDoubleSize = reader.Value;
                    break;
                case "VobSubEditCharacters/ImageFileNotFound":
                    language.VobSubEditCharacters.ImageFileNotFound = reader.Value;
                    break;
                case "VobSubEditCharacters/Image":
                    language.VobSubEditCharacters.Image = reader.Value;
                    break;
                case "VobSubOcr/Title":
                    language.VobSubOcr.Title = reader.Value;
                    break;
                case "VobSubOcr/TitleBluRay":
                    language.VobSubOcr.TitleBluRay = reader.Value;
                    break;
                case "VobSubOcr/OcrMethod":
                    language.VobSubOcr.OcrMethod = reader.Value;
                    break;
                case "VobSubOcr/OcrViaTesseractVersionX":
                    language.VobSubOcr.OcrViaTesseractVersionX = reader.Value;
                    break;
                case "VobSubOcr/OcrViaImageCompare":
                    language.VobSubOcr.OcrViaImageCompare = reader.Value;
                    break;
                case "VobSubOcr/OcrViaModi":
                    language.VobSubOcr.OcrViaModi = reader.Value;
                    break;
                case "VobSubOcr/OcrViaNOCR":
                    language.VobSubOcr.OcrViaNOCR = reader.Value;
                    break;
                case "VobSubOcr/TesseractEngineMode":
                    language.VobSubOcr.TesseractEngineMode = reader.Value;
                    break;
                case "VobSubOcr/TesseractEngineModeLegacy":
                    language.VobSubOcr.TesseractEngineModeLegacy = reader.Value;
                    break;
                case "VobSubOcr/TesseractEngineModeNeural":
                    language.VobSubOcr.TesseractEngineModeNeural = reader.Value;
                    break;
                case "VobSubOcr/TesseractEngineModeBoth":
                    language.VobSubOcr.TesseractEngineModeBoth = reader.Value;
                    break;
                case "VobSubOcr/TesseractEngineModeDefault":
                    language.VobSubOcr.TesseractEngineModeDefault = reader.Value;
                    break;
                case "VobSubOcr/Language":
                    language.VobSubOcr.Language = reader.Value;
                    break;
                case "VobSubOcr/ImageDatabase":
                    language.VobSubOcr.ImageDatabase = reader.Value;
                    break;
                case "VobSubOcr/NoOfPixelsIsSpace":
                    language.VobSubOcr.NoOfPixelsIsSpace = reader.Value;
                    break;
                case "VobSubOcr/MaxErrorPercent":
                    language.VobSubOcr.MaxErrorPercent = reader.Value;
                    break;
                case "VobSubOcr/New":
                    language.VobSubOcr.New = reader.Value;
                    break;
                case "VobSubOcr/Edit":
                    language.VobSubOcr.Edit = reader.Value;
                    break;
                case "VobSubOcr/StartOcr":
                    language.VobSubOcr.StartOcr = reader.Value;
                    break;
                case "VobSubOcr/Stop":
                    language.VobSubOcr.Stop = reader.Value;
                    break;
                case "VobSubOcr/StartOcrFrom":
                    language.VobSubOcr.StartOcrFrom = reader.Value;
                    break;
                case "VobSubOcr/LoadingVobSubImages":
                    language.VobSubOcr.LoadingVobSubImages = reader.Value;
                    break;
                case "VobSubOcr/LoadingImageCompareDatabase":
                    language.VobSubOcr.LoadingImageCompareDatabase = reader.Value;
                    break;
                case "VobSubOcr/ConvertingImageCompareDatabase":
                    language.VobSubOcr.ConvertingImageCompareDatabase = reader.Value;
                    break;
                case "VobSubOcr/SubtitleImage":
                    language.VobSubOcr.SubtitleImage = reader.Value;
                    break;
                case "VobSubOcr/SubtitleText":
                    language.VobSubOcr.SubtitleText = reader.Value;
                    break;
                case "VobSubOcr/UnableToCreateCharacterDatabaseFolder":
                    language.VobSubOcr.UnableToCreateCharacterDatabaseFolder = reader.Value;
                    break;
                case "VobSubOcr/SubtitleImageXofY":
                    language.VobSubOcr.SubtitleImageXofY = reader.Value;
                    break;
                case "VobSubOcr/ImagePalette":
                    language.VobSubOcr.ImagePalette = reader.Value;
                    break;
                case "VobSubOcr/UseCustomColors":
                    language.VobSubOcr.UseCustomColors = reader.Value;
                    break;
                case "VobSubOcr/Transparent":
                    language.VobSubOcr.Transparent = reader.Value;
                    break;
                case "VobSubOcr/TransparentMinAlpha":
                    language.VobSubOcr.TransparentMinAlpha = reader.Value;
                    break;
                case "VobSubOcr/TransportStream":
                    language.VobSubOcr.TransportStream = reader.Value;
                    break;
                case "VobSubOcr/TransportStreamGrayscale":
                    language.VobSubOcr.TransportStreamGrayscale = reader.Value;
                    break;
                case "VobSubOcr/TransportStreamGetColor":
                    language.VobSubOcr.TransportStreamGetColor = reader.Value;
                    break;
                case "VobSubOcr/PromptForUnknownWords":
                    language.VobSubOcr.PromptForUnknownWords = reader.Value;
                    break;
                case "VobSubOcr/TryToGuessUnkownWords":
                    language.VobSubOcr.TryToGuessUnkownWords = reader.Value;
                    break;
                case "VobSubOcr/AutoBreakSubtitleIfMoreThanTwoLines":
                    language.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines = reader.Value;
                    break;
                case "VobSubOcr/AllFixes":
                    language.VobSubOcr.AllFixes = reader.Value;
                    break;
                case "VobSubOcr/GuessesUsed":
                    language.VobSubOcr.GuessesUsed = reader.Value;
                    break;
                case "VobSubOcr/UnknownWords":
                    language.VobSubOcr.UnknownWords = reader.Value;
                    break;
                case "VobSubOcr/UnknownWordToGuessInLine":
                    language.VobSubOcr.UnknownWordToGuessInLine = reader.Value;
                    break;
                case "VobSubOcr/OcrAutoCorrectionSpellChecking":
                    language.VobSubOcr.OcrAutoCorrectionSpellChecking = reader.Value;
                    break;
                case "VobSubOcr/FixOcrErrors":
                    language.VobSubOcr.FixOcrErrors = reader.Value;
                    break;
                case "VobSubOcr/ImportTextWithMatchingTimeCodes":
                    language.VobSubOcr.ImportTextWithMatchingTimeCodes = reader.Value;
                    break;
                case "VobSubOcr/ImportNewTimeCodes":
                    language.VobSubOcr.ImportNewTimeCodes = reader.Value;
                    break;
                case "VobSubOcr/SaveSubtitleImageAs":
                    language.VobSubOcr.SaveSubtitleImageAs = reader.Value;
                    break;
                case "VobSubOcr/SaveAllSubtitleImagesAsBdnXml":
                    language.VobSubOcr.SaveAllSubtitleImagesAsBdnXml = reader.Value;
                    break;
                case "VobSubOcr/SaveAllSubtitleImagesWithHtml":
                    language.VobSubOcr.SaveAllSubtitleImagesWithHtml = reader.Value;
                    break;
                case "VobSubOcr/XImagesSavedInY":
                    language.VobSubOcr.XImagesSavedInY = reader.Value;
                    break;
                case "VobSubOcr/DictionaryX":
                    language.VobSubOcr.DictionaryX = reader.Value;
                    break;
                case "VobSubOcr/RightToLeft":
                    language.VobSubOcr.RightToLeft = reader.Value;
                    break;
                case "VobSubOcr/ShowOnlyForcedSubtitles":
                    language.VobSubOcr.ShowOnlyForcedSubtitles = reader.Value;
                    break;
                case "VobSubOcr/UseTimeCodesFromIdx":
                    language.VobSubOcr.UseTimeCodesFromIdx = reader.Value;
                    break;
                case "VobSubOcr/NoMatch":
                    language.VobSubOcr.NoMatch = reader.Value;
                    break;
                case "VobSubOcr/AutoTransparentBackground":
                    language.VobSubOcr.AutoTransparentBackground = reader.Value;
                    break;
                case "VobSubOcr/CaptureTopAlign":
                    language.VobSubOcr.CaptureTopAlign = reader.Value;
                    break;
                case "VobSubOcr/InspectCompareMatchesForCurrentImage":
                    language.VobSubOcr.InspectCompareMatchesForCurrentImage = reader.Value;
                    break;
                case "VobSubOcr/EditLastAdditions":
                    language.VobSubOcr.EditLastAdditions = reader.Value;
                    break;
                case "VobSubOcr/SetUnitalicFactor":
                    language.VobSubOcr.SetUnitalicFactor = reader.Value;
                    break;
                case "VobSubOcr/DiscardTitle":
                    language.VobSubOcr.DiscardTitle = reader.Value;
                    break;
                case "VobSubOcr/DiscardText":
                    language.VobSubOcr.DiscardText = reader.Value;
                    break;
                case "VobSubOcr/MinLineSplitHeight":
                    language.VobSubOcr.MinLineSplitHeight = reader.Value;
                    break;
                case "VobSubOcr/FallbackToX":
                    language.VobSubOcr.FallbackToX = reader.Value;
                    break;
                case "VobSubOcr/ImagePreProcessing":
                    language.VobSubOcr.ImagePreProcessing = reader.Value;
                    break;
                case "VobSubOcr/EditImageDb":
                    language.VobSubOcr.EditImageDb = reader.Value;
                    break;
                case "VobSubOcrCharacter/Title":
                    language.VobSubOcrCharacter.Title = reader.Value;
                    break;
                case "VobSubOcrCharacter/ShrinkSelection":
                    language.VobSubOcrCharacter.ShrinkSelection = reader.Value;
                    break;
                case "VobSubOcrCharacter/ExpandSelection":
                    language.VobSubOcrCharacter.ExpandSelection = reader.Value;
                    break;
                case "VobSubOcrCharacter/SubtitleImage":
                    language.VobSubOcrCharacter.SubtitleImage = reader.Value;
                    break;
                case "VobSubOcrCharacter/Characters":
                    language.VobSubOcrCharacter.Characters = reader.Value;
                    break;
                case "VobSubOcrCharacter/CharactersAsText":
                    language.VobSubOcrCharacter.CharactersAsText = reader.Value;
                    break;
                case "VobSubOcrCharacter/Italic":
                    language.VobSubOcrCharacter.Italic = reader.Value;
                    break;
                case "VobSubOcrCharacter/Abort":
                    language.VobSubOcrCharacter.Abort = reader.Value;
                    break;
                case "VobSubOcrCharacter/Skip":
                    language.VobSubOcrCharacter.Skip = reader.Value;
                    break;
                case "VobSubOcrCharacter/Nordic":
                    language.VobSubOcrCharacter.Nordic = reader.Value;
                    break;
                case "VobSubOcrCharacter/Spanish":
                    language.VobSubOcrCharacter.Spanish = reader.Value;
                    break;
                case "VobSubOcrCharacter/German":
                    language.VobSubOcrCharacter.German = reader.Value;
                    break;
                case "VobSubOcrCharacter/AutoSubmitOnFirstChar":
                    language.VobSubOcrCharacter.AutoSubmitOnFirstChar = reader.Value;
                    break;
                case "VobSubOcrCharacter/EditLastX":
                    language.VobSubOcrCharacter.EditLastX = reader.Value;
                    break;
                case "VobSubOcrCharacterInspect/Title":
                    language.VobSubOcrCharacterInspect.Title = reader.Value;
                    break;
                case "VobSubOcrCharacterInspect/InspectItems":
                    language.VobSubOcrCharacterInspect.InspectItems = reader.Value;
                    break;
                case "VobSubOcrCharacterInspect/AddBetterMatch":
                    language.VobSubOcrCharacterInspect.AddBetterMatch = reader.Value;
                    break;
                case "VobSubOcrCharacterInspect/Add":
                    language.VobSubOcrCharacterInspect.Add = reader.Value;
                    break;
                case "VobSubOcrNewFolder/Title":
                    language.VobSubOcrNewFolder.Title = reader.Value;
                    break;
                case "VobSubOcrNewFolder/Message":
                    language.VobSubOcrNewFolder.Message = reader.Value;
                    break;
                case "VobSubOcrSetItalicFactor/Title":
                    language.VobSubOcrSetItalicFactor.Title = reader.Value;
                    break;
                case "VobSubOcrSetItalicFactor/Description":
                    language.VobSubOcrSetItalicFactor.Description = reader.Value;
                    break;
                case "OcrPreprocessing/Title":
                    language.OcrPreprocessing.Title = reader.Value;
                    break;
                case "OcrPreprocessing/Colors":
                    language.OcrPreprocessing.Colors = reader.Value;
                    break;
                case "OcrPreprocessing/AdjustAlpha":
                    language.OcrPreprocessing.AdjustAlpha = reader.Value;
                    break;
                case "OcrPreprocessing/OriginalImage":
                    language.OcrPreprocessing.OriginalImage = reader.Value;
                    break;
                case "OcrPreprocessing/PostImage":
                    language.OcrPreprocessing.PostImage = reader.Value;
                    break;
                case "OcrPreprocessing/BinaryThreshold":
                    language.OcrPreprocessing.BinaryThreshold = reader.Value;
                    break;
                case "OcrPreprocessing/InvertColors":
                    language.OcrPreprocessing.InvertColors = reader.Value;
                    break;
                case "OcrPreprocessing/YellowToWhite":
                    language.OcrPreprocessing.YellowToWhite = reader.Value;
                    break;
                case "OcrPreprocessing/ColorToWhite":
                    language.OcrPreprocessing.ColorToWhite = reader.Value;
                    break;
                case "OcrPreprocessing/ColorToRemove":
                    language.OcrPreprocessing.ColorToRemove = reader.Value;
                    break;
                case "OcrPreprocessing/Cropping":
                    language.OcrPreprocessing.Cropping = reader.Value;
                    break;
                case "OcrPreprocessing/CropTransparentColors":
                    language.OcrPreprocessing.CropTransparentColors = reader.Value;
                    break;
                case "Watermark/Title":
                    language.Watermark.Title = reader.Value;
                    break;
                case "Watermark/WatermarkX":
                    language.Watermark.WatermarkX = reader.Value;
                    break;
                case "Watermark/GenerateWatermarkTitle":
                    language.Watermark.GenerateWatermarkTitle = reader.Value;
                    break;
                case "Watermark/SpreadOverEntireSubtitle":
                    language.Watermark.SpreadOverEntireSubtitle = reader.Value;
                    break;
                case "Watermark/CurrentLineOnlyX":
                    language.Watermark.CurrentLineOnlyX = reader.Value;
                    break;
                case "Watermark/Generate":
                    language.Watermark.Generate = reader.Value;
                    break;
                case "Watermark/Remove":
                    language.Watermark.Remove = reader.Value;
                    break;
                case "Watermark/BeforeWatermark":
                    language.Watermark.BeforeWatermark = reader.Value;
                    break;
                case "Watermark/ErrorUnicodeEncodingOnly":
                    language.Watermark.ErrorUnicodeEncodingOnly = reader.Value;
                    break;
                case "Waveform/ClickToAddWaveform":
                    language.Waveform.ClickToAddWaveform = reader.Value;
                    break;
                case "Waveform/ClickToAddWaveformAndSpectrogram":
                    language.Waveform.ClickToAddWaveformAndSpectrogram = reader.Value;
                    break;
                case "Waveform/Seconds":
                    language.Waveform.Seconds = reader.Value;
                    break;
                case "Waveform/ZoomIn":
                    language.Waveform.ZoomIn = reader.Value;
                    break;
                case "Waveform/ZoomOut":
                    language.Waveform.ZoomOut = reader.Value;
                    break;
                case "Waveform/AddParagraphHere":
                    language.Waveform.AddParagraphHere = reader.Value;
                    break;
                case "Waveform/AddParagraphHereAndPasteText":
                    language.Waveform.AddParagraphHereAndPasteText = reader.Value;
                    break;
                case "Waveform/SetParagraphAsSelection":
                    language.Waveform.SetParagraphAsSelection = reader.Value;
                    break;
                case "Waveform/FocusTextBox":
                    language.Waveform.FocusTextBox = reader.Value;
                    break;
                case "Waveform/DeleteParagraph":
                    language.Waveform.DeleteParagraph = reader.Value;
                    break;
                case "Waveform/Split":
                    language.Waveform.Split = reader.Value;
                    break;
                case "Waveform/SplitAtCursor":
                    language.Waveform.SplitAtCursor = reader.Value;
                    break;
                case "Waveform/MergeWithPrevious":
                    language.Waveform.MergeWithPrevious = reader.Value;
                    break;
                case "Waveform/MergeWithNext":
                    language.Waveform.MergeWithNext = reader.Value;
                    break;
                case "Waveform/PlaySelection":
                    language.Waveform.PlaySelection = reader.Value;
                    break;
                case "Waveform/ShowWaveformAndSpectrogram":
                    language.Waveform.ShowWaveformAndSpectrogram = reader.Value;
                    break;
                case "Waveform/ShowWaveformOnly":
                    language.Waveform.ShowWaveformOnly = reader.Value;
                    break;
                case "Waveform/ShowSpectrogramOnly":
                    language.Waveform.ShowSpectrogramOnly = reader.Value;
                    break;
                case "Waveform/AddSceneChange":
                    language.Waveform.AddSceneChange = reader.Value;
                    break;
                case "Waveform/RemoveSceneChange":
                    language.Waveform.RemoveSceneChange = reader.Value;
                    break;
                case "Waveform/GuessTimeCodes":
                    language.Waveform.GuessTimeCodes = reader.Value;
                    break;
                case "Waveform/SeekSilence":
                    language.Waveform.SeekSilence = reader.Value;
                    break;
                case "Waveform/InsertSubtitleHere":
                    language.Waveform.InsertSubtitleHere = reader.Value;
                    break;
                case "Waveform/CharsSecX":
                    language.Waveform.CharsSecX = reader.Value;
                    break;
                case "Waveform/WordsMinX":
                    language.Waveform.WordsMinX = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/Title":
                    language.WaveformGenerateTimeCodes.Title = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/StartFrom":
                    language.WaveformGenerateTimeCodes.StartFrom = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/CurrentVideoPosition":
                    language.WaveformGenerateTimeCodes.CurrentVideoPosition = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/Beginning":
                    language.WaveformGenerateTimeCodes.Beginning = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/DeleteLines":
                    language.WaveformGenerateTimeCodes.DeleteLines = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/FromCurrentVideoPosition":
                    language.WaveformGenerateTimeCodes.FromCurrentVideoPosition = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/DetectOptions":
                    language.WaveformGenerateTimeCodes.DetectOptions = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/ScanBlocksOfMs":
                    language.WaveformGenerateTimeCodes.ScanBlocksOfMs = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/BlockAverageVolMin1":
                    language.WaveformGenerateTimeCodes.BlockAverageVolMin1 = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/BlockAverageVolMin2":
                    language.WaveformGenerateTimeCodes.BlockAverageVolMin2 = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/BlockAverageVolMax1":
                    language.WaveformGenerateTimeCodes.BlockAverageVolMax1 = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/BlockAverageVolMax2":
                    language.WaveformGenerateTimeCodes.BlockAverageVolMax2 = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/SplitLongLinesAt1":
                    language.WaveformGenerateTimeCodes.SplitLongLinesAt1 = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/SplitLongLinesAt2":
                    language.WaveformGenerateTimeCodes.SplitLongLinesAt2 = reader.Value;
                    break;
                case "WaveformGenerateTimeCodes/Other":
                    language.WaveformGenerateTimeCodes.Other = reader.Value;
                    break;
                case "WebVttNewVoice/Title":
                    language.WebVttNewVoice.Title = reader.Value;
                    break;
                case "WebVttNewVoice/VoiceName":
                    language.WebVttNewVoice.VoiceName = reader.Value;
                    break;

            }
        }
    }
}
