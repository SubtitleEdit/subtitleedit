using System;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public static class WhisperEngineFactory
{
    /// <summary>
    /// Return WhisperEngine based on StaticName.
    /// </summary>
    /// <param name="staticName">StaticName property.</param>
    /// <returns>Whisper engine.</returns>
    /// <exception cref="NotImplementedException">If StaticName is not known.</exception>
    public static ISpeechToTextEngine MakeEngineFromStaticName(string staticName)
    {
        if (staticName == WhisperEngineCpp.StaticName)
        {
            return new WhisperEngineCpp();
        }

        if (staticName == WhisperEngineCppCuBlas.StaticName)
        {
            return new WhisperEngineCppCuBlas();
        }

        if (staticName == WhisperEngineCppVulkan.StaticName)
        {
            return new WhisperEngineCppVulkan();
        }

        if (staticName == WhisperEngineConstMe.StaticName)
        {
            return new WhisperEngineConstMe();
        }

        if (staticName == WhisperEngineOpenAi.StaticName)
        {
            return new WhisperEngineOpenAi();
        }

        if (staticName == WhisperEnginePurfviewFasterWhisperXxl.StaticName)
        {
            return new WhisperEnginePurfviewFasterWhisperXxl();
        }

        throw new NotImplementedException();
    }
}
