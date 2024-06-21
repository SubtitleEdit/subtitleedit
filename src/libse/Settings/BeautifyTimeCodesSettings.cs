namespace Nikse.SubtitleEdit.Core.Settings
{
    public class BeautifyTimeCodesSettings
    {
        public bool AlignTimeCodes { get; set; }
        public bool ExtractExactTimeCodes { get; set; }
        public bool SnapToShotChanges { get; set; }
        public int OverlapThreshold { get; set; }
        public BeautifyTimeCodesProfile Profile { get; set; }

        public BeautifyTimeCodesSettings()
        {
            AlignTimeCodes = true;
            ExtractExactTimeCodes = false;
            SnapToShotChanges = true;
            OverlapThreshold = 1000;
            Profile = new BeautifyTimeCodesProfile();
        }

        public class BeautifyTimeCodesProfile
        {
            // General
            public int Gap { get; set; }

            // In cues
            public int InCuesGap { get; set; }
            public int InCuesLeftGreenZone { get; set; }
            public int InCuesLeftRedZone { get; set; }
            public int InCuesRightRedZone { get; set; }
            public int InCuesRightGreenZone { get; set; }

            // Out cues
            public int OutCuesGap { get; set; }
            public int OutCuesLeftGreenZone { get; set; }
            public int OutCuesLeftRedZone { get; set; }
            public int OutCuesRightRedZone { get; set; }
            public int OutCuesRightGreenZone { get; set; }

            // Connected subtitles
            public int ConnectedSubtitlesInCueClosestLeftGap { get; set; }
            public int ConnectedSubtitlesInCueClosestRightGap { get; set; }
            public int ConnectedSubtitlesOutCueClosestLeftGap { get; set; }
            public int ConnectedSubtitlesOutCueClosestRightGap { get; set; }
            public int ConnectedSubtitlesLeftGreenZone { get; set; }
            public int ConnectedSubtitlesLeftRedZone { get; set; }
            public int ConnectedSubtitlesRightRedZone { get; set; }
            public int ConnectedSubtitlesRightGreenZone { get; set; }
            public int ConnectedSubtitlesTreatConnected { get; set; }

            // Chaining
            public bool ChainingGeneralUseZones { get; set; }
            public int ChainingGeneralMaxGap { get; set; }
            public int ChainingGeneralLeftGreenZone { get; set; }
            public int ChainingGeneralLeftRedZone { get; set; }
            public ChainingShotChangeBehaviorEnum ChainingGeneralShotChangeBehavior { get; set; }
            public bool ChainingInCueOnShotUseZones { get; set; }
            public int ChainingInCueOnShotMaxGap { get; set; }
            public int ChainingInCueOnShotLeftGreenZone { get; set; }
            public int ChainingInCueOnShotLeftRedZone { get; set; }
            public ChainingShotChangeBehaviorEnum ChainingInCueOnShotShotChangeBehavior { get; set; }
            public bool ChainingInCueOnShotCheckGeneral { get; set; }
            public bool ChainingOutCueOnShotUseZones { get; set; }
            public int ChainingOutCueOnShotMaxGap { get; set; }
            public int ChainingOutCueOnShotRightRedZone { get; set; }
            public int ChainingOutCueOnShotRightGreenZone { get; set; }
            public ChainingShotChangeBehaviorEnum ChainingOutCueOnShotShotChangeBehavior { get; set; }
            public bool ChainingOutCueOnShotCheckGeneral { get; set; }

            public enum Preset : int
            {
                Default = 0,
                Netflix = 1,
                SDI = 2,
            }

            public enum ChainingShotChangeBehaviorEnum : int
            {
                DontChain = 0,
                ExtendCrossingShotChange = 1,
                ExtendUntilShotChange = 2
            }

            public BeautifyTimeCodesProfile(Preset preset = Preset.Default)
            {
                switch (preset)
                {
                    case Preset.Netflix:
                        Gap = 2;

                        InCuesGap = 0;
                        InCuesLeftGreenZone = 12;
                        InCuesLeftRedZone = 7;
                        InCuesRightRedZone = 7;
                        InCuesRightGreenZone = 12;

                        OutCuesGap = 2;
                        OutCuesLeftGreenZone = 12;
                        OutCuesLeftRedZone = 7;
                        OutCuesRightRedZone = 7;
                        OutCuesRightGreenZone = 12;

                        ConnectedSubtitlesInCueClosestLeftGap = 2;
                        ConnectedSubtitlesInCueClosestRightGap = 0;
                        ConnectedSubtitlesOutCueClosestLeftGap = 2;
                        ConnectedSubtitlesOutCueClosestRightGap = 0;
                        ConnectedSubtitlesLeftGreenZone = 12;
                        ConnectedSubtitlesLeftRedZone = 7;
                        ConnectedSubtitlesRightRedZone = 7;
                        ConnectedSubtitlesRightGreenZone = 12;
                        ConnectedSubtitlesTreatConnected = 180;

                        ChainingGeneralUseZones = false;
                        ChainingGeneralMaxGap = 500;
                        ChainingGeneralLeftGreenZone = 12;
                        ChainingGeneralLeftRedZone = 11;
                        ChainingGeneralShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingInCueOnShotUseZones = false;
                        ChainingInCueOnShotMaxGap = 500;
                        ChainingInCueOnShotLeftGreenZone = 12;
                        ChainingInCueOnShotLeftRedZone = 11;
                        ChainingInCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingInCueOnShotCheckGeneral = true;
                        ChainingOutCueOnShotUseZones = false;
                        ChainingOutCueOnShotMaxGap = 500;
                        ChainingOutCueOnShotRightRedZone = 11;
                        ChainingOutCueOnShotRightGreenZone = 12;
                        ChainingOutCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingOutCueOnShotCheckGeneral = true;
                        break;
                    case Preset.SDI:
                        Gap = 4;

                        InCuesGap = 2;
                        InCuesLeftGreenZone = 12;
                        InCuesLeftRedZone = 7;
                        InCuesRightRedZone = 7;
                        InCuesRightGreenZone = 12;

                        OutCuesGap = 2;
                        OutCuesLeftGreenZone = 12;
                        OutCuesLeftRedZone = 7;
                        OutCuesRightRedZone = 7;
                        OutCuesRightGreenZone = 12;

                        ConnectedSubtitlesInCueClosestLeftGap = 2;
                        ConnectedSubtitlesInCueClosestRightGap = 2;
                        ConnectedSubtitlesOutCueClosestLeftGap = 2;
                        ConnectedSubtitlesOutCueClosestRightGap = 2;
                        ConnectedSubtitlesLeftGreenZone = 12;
                        ConnectedSubtitlesLeftRedZone = 7;
                        ConnectedSubtitlesRightRedZone = 7;
                        ConnectedSubtitlesRightGreenZone = 12;
                        ConnectedSubtitlesTreatConnected = 240;

                        ChainingGeneralUseZones = false;
                        ChainingGeneralMaxGap = 1000;
                        ChainingGeneralLeftGreenZone = 25;
                        ChainingGeneralLeftRedZone = 24;
                        ChainingGeneralShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendCrossingShotChange;
                        ChainingInCueOnShotUseZones = false;
                        ChainingInCueOnShotMaxGap = 1000;
                        ChainingInCueOnShotLeftGreenZone = 25;
                        ChainingInCueOnShotLeftRedZone = 24;
                        ChainingInCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendCrossingShotChange;
                        ChainingInCueOnShotCheckGeneral = true;
                        ChainingOutCueOnShotUseZones = true;
                        ChainingOutCueOnShotMaxGap = 500;
                        ChainingOutCueOnShotRightRedZone = 7;
                        ChainingOutCueOnShotRightGreenZone = 12;
                        ChainingOutCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendCrossingShotChange;
                        ChainingOutCueOnShotCheckGeneral = true;
                        break;
                    default:
                        Gap = 3;

                        InCuesGap = 0;
                        InCuesLeftGreenZone = 3;
                        InCuesLeftRedZone = 3;
                        InCuesRightRedZone = 5;
                        InCuesRightGreenZone = 5;

                        OutCuesGap = 0;
                        OutCuesLeftGreenZone = 10;
                        OutCuesLeftRedZone = 10;
                        OutCuesRightRedZone = 3;
                        OutCuesRightGreenZone = 12;

                        ConnectedSubtitlesInCueClosestLeftGap = 3;
                        ConnectedSubtitlesInCueClosestRightGap = 0;
                        ConnectedSubtitlesOutCueClosestLeftGap = 0;
                        ConnectedSubtitlesOutCueClosestRightGap = 3;
                        ConnectedSubtitlesLeftGreenZone = 3;
                        ConnectedSubtitlesLeftRedZone = 3;
                        ConnectedSubtitlesRightRedZone = 3;
                        ConnectedSubtitlesRightGreenZone = 3;
                        ConnectedSubtitlesTreatConnected = 180;

                        ChainingGeneralUseZones = false;
                        ChainingGeneralMaxGap = 1000;
                        ChainingGeneralLeftGreenZone = 25;
                        ChainingGeneralLeftRedZone = 24;
                        ChainingGeneralShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingInCueOnShotUseZones = false;
                        ChainingInCueOnShotMaxGap = 1000;
                        ChainingInCueOnShotLeftGreenZone = 25;
                        ChainingInCueOnShotLeftRedZone = 24;
                        ChainingInCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingInCueOnShotCheckGeneral = true;
                        ChainingOutCueOnShotUseZones = false;
                        ChainingOutCueOnShotMaxGap = 500;
                        ChainingOutCueOnShotRightRedZone = 11;
                        ChainingOutCueOnShotRightGreenZone = 12;
                        ChainingOutCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingOutCueOnShotCheckGeneral = true;
                        break;
                }
            }
        }
    }
}