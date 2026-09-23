using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// #15206: "--device cuda --compute_type float16" on the CPU-only WhisperX build fails with
/// "Torch not compiled with CUDA enabled"; SE offers to strip the GPU-only arguments.
/// </summary>
public class NoCudaBuildTests
{
    [Theory]
    [InlineData("--device cuda --compute_type float16", "")]
    [InlineData("--device cuda", "")]
    [InlineData("--device=cuda:0 --batch_size 4", "--batch_size 4")]
    [InlineData("--batch_size 4 --device cuda --device_index 1 --diarize", "--batch_size 4 --diarize")]
    [InlineData("--compute_type int8_float16 --vad_method silero", "--vad_method silero")]
    [InlineData("--compute_type bfloat16", "")]
    public void GpuArguments_AreRemoved(string parameters, string expected)
    {
        Assert.Equal(expected, SpeechToTextViewModel.RemoveGpuParameters(parameters));
    }

    [Theory]
    [InlineData("--compute_type int8")]
    [InlineData("--compute_type float32 --batch_size 2")]
    [InlineData("--model_dir \"C:\\models\"")]
    [InlineData("")]
    public void CpuArguments_AreKept(string parameters)
    {
        Assert.Equal(parameters, SpeechToTextViewModel.RemoveGpuParameters(parameters));
    }

    [Theory]
    [InlineData("AssertionError: Torch not compiled with CUDA enabled", true)]
    [InlineData("ValueError: This CTranslate2 package was not compiled with CUDA support", true)]
    [InlineData("CUDA failed with error out of memory", false)]
    [InlineData("Transcript: [0.031 --> 2.001]  Hello", false)]
    public void NoCudaBuildError_IsRecognized(string line, bool expected)
    {
        Assert.Equal(expected, SpeechToTextViewModel.IsNoCudaBuildError(line));
    }
}
