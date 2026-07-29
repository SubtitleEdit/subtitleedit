namespace Nikse.SubtitleEdit.UiLogic.AudioToText
{
    public interface IWhisperModel
    {
        string ModelFolder { get;  }
        void CreateModelFolder();
        WhisperModel[] Models { get;  }
    }
}