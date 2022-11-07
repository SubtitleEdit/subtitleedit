namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public interface IWhisperModel
    {
        string ModelFolder { get;  }
        void CreateModelFolder();
        WhisperModel[] Models { get;  }
    }
}