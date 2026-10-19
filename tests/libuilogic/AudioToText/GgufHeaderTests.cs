using System.Text;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace LibUiLogicTests.AudioToText;

/// <summary>
/// GGUF header parsing and the Qwen3 ASR CPP compatibility check (issue #16232): several
/// unrelated projects publish "Qwen3-ASR GGUF" files and only the qwen3-asr.cpp layout
/// (audio encoder + LLM in one file) can be loaded by the engine Subtitle Edit ships.
/// </summary>
public class GgufHeaderTests
{
    private sealed class GgufBuilder
    {
        private readonly List<(string Key, Action<BinaryWriter> Write)> _keyValues = new();
        private readonly List<string> _tensors = new();

        public GgufBuilder String(string key, string value)
        {
            _keyValues.Add((key, w =>
            {
                w.Write(8u);
                WriteString(w, value);
            }));
            return this;
        }

        public GgufBuilder UInt32(string key, uint value)
        {
            _keyValues.Add((key, w =>
            {
                w.Write(4u);
                w.Write(value);
            }));
            return this;
        }

        public GgufBuilder Float32(string key, float value)
        {
            _keyValues.Add((key, w =>
            {
                w.Write(6u);
                w.Write(value);
            }));
            return this;
        }

        public GgufBuilder Bool(string key, bool value)
        {
            _keyValues.Add((key, w =>
            {
                w.Write(7u);
                w.Write(value ? (byte)1 : (byte)0);
            }));
            return this;
        }

        public GgufBuilder StringArray(string key, params string[] values)
        {
            _keyValues.Add((key, w =>
            {
                w.Write(9u); // array
                w.Write(8u); // of strings
                w.Write((ulong)values.Length);
                foreach (var v in values)
                {
                    WriteString(w, v);
                }
            }));
            return this;
        }

        public GgufBuilder Int32Array(string key, params int[] values)
        {
            _keyValues.Add((key, w =>
            {
                w.Write(9u); // array
                w.Write(5u); // of int32
                w.Write((ulong)values.Length);
                foreach (var v in values)
                {
                    w.Write(v);
                }
            }));
            return this;
        }

        public GgufBuilder Tensor(string name)
        {
            _tensors.Add(name);
            return this;
        }

        public byte[] Build(uint version = 3)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);
            w.Write(Encoding.ASCII.GetBytes("GGUF"));
            w.Write(version);
            w.Write((ulong)_tensors.Count);
            w.Write((ulong)_keyValues.Count);
            foreach (var (key, write) in _keyValues)
            {
                WriteString(w, key);
                write(w);
            }

            foreach (var tensor in _tensors)
            {
                WriteString(w, tensor);
                w.Write(2u); // n_dims
                w.Write(16ul);
                w.Write(32ul);
                w.Write(1u); // ggml type F16
                w.Write(0ul); // offset
            }

            // Some trailing "tensor data" so a truncated-header check does not trip.
            w.Write(new byte[64]);
            w.Flush();
            return ms.ToArray();
        }

        private static void WriteString(BinaryWriter w, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            w.Write((ulong)bytes.Length);
            w.Write(bytes);
        }
    }

    private static GgufBuilder Qwen3AsrCpp() => new GgufBuilder()
        .String("general.architecture", "qwen3-asr")
        .String("general.name", "Qwen3-ASR-0.6B")
        .UInt32("qwen3-asr.audio.encoder.layer_count", 18)
        .UInt32("qwen3-asr.audio.encoder.embedding_length", 896)
        .Float32("qwen3-asr.attention.layer_norm_rms_epsilon", 1e-6f)
        .StringArray("tokenizer.ggml.tokens", "<|endoftext|>", "a", "b", "日本")
        .Int32Array("tokenizer.ggml.token_type", 3, 1, 1, 1)
        .Bool("tokenizer.ggml.add_bos_token", false)
        .Tensor("audio.encoder.conv1.weight")
        .Tensor("audio.encoder.blk.0.attn_q.weight")
        .Tensor("token_embd.weight")
        .Tensor("blk.0.attn_q.weight")
        .Tensor("output.weight");

    private static GgufHeader Read(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return GgufHeader.Read(ms);
    }

    [Fact]
    public void Read_ParsesArchitectureKeysAndTensorNames_SkippingArraysAndScalars()
    {
        var header = Read(Qwen3AsrCpp().Build());

        Assert.Equal(3u, header.Version);
        Assert.Equal("qwen3-asr", header.Architecture);
        Assert.Equal("Qwen3-ASR-0.6B", header.StringValues["general.name"]);
        Assert.True(header.HasKey("tokenizer.ggml.tokens"));
        Assert.True(header.HasKey("tokenizer.ggml.add_bos_token"));
        Assert.True(header.HasKey("qwen3-asr.attention.layer_norm_rms_epsilon"));
        Assert.Equal(
            new[] { "audio.encoder.conv1.weight", "audio.encoder.blk.0.attn_q.weight", "token_embd.weight", "blk.0.attn_q.weight", "output.weight" },
            header.TensorNames);
    }

    [Fact]
    public void Read_Version2_IsAccepted()
    {
        var header = Read(Qwen3AsrCpp().Build(version: 2));

        Assert.Equal(2u, header.Version);
        Assert.Equal("qwen3-asr", header.Architecture);
    }

    [Fact]
    public void Read_NonGgufBytes_ThrowsInvalidData()
    {
        Assert.Throws<InvalidDataException>(() => Read(Encoding.ASCII.GetBytes("ONNX model or whatever, definitely not gguf")));
        Assert.Throws<InvalidDataException>(() => Read(new byte[] { 0x47, 0x47 }));
    }

    [Fact]
    public void Read_TruncatedHeader_ThrowsInvalidData()
    {
        var bytes = Qwen3AsrCpp().Build();
        var truncated = bytes.Take(bytes.Length / 2).ToArray();

        Assert.Throws<InvalidDataException>(() => Read(truncated));
    }

    [Fact]
    public void Read_NonSeekableStream_Works()
    {
        var bytes = Qwen3AsrCpp().Build();
        using var ms = new NonSeekableStream(bytes);

        var header = GgufHeader.Read(ms);

        Assert.Equal("qwen3-asr", header.Architecture);
        Assert.Contains("output.weight", header.TensorNames);
    }

    [Fact]
    public void Validator_Qwen3AsrCppLayout_IsCompatible()
    {
        var header = Read(Qwen3AsrCpp().Build());

        Assert.Null(Qwen3AsrCppModelValidator.GetIncompatibilityReason(header, "qwen3-asr-0.6b-q8_0.gguf"));
    }

    [Fact]
    public void Validator_Qwen3AsrCppLayout_EncoderKeyOnly_IsCompatible()
    {
        // Older conversions: the hparams keys are present even if tensor names differ.
        var header = Read(new GgufBuilder()
            .String("general.architecture", "qwen3-asr")
            .UInt32("qwen3-asr.audio.encoder.layer_count", 18)
            .Tensor("token_embd.weight")
            .Build());

        Assert.Null(Qwen3AsrCppModelValidator.GetIncompatibilityReason(header, "x.gguf"));
    }

    [Fact]
    public void Validator_CapsWriterDecoderOnlyLlamaCpp_IsRejectedWithEncoderHint()
    {
        // CapsWriter-Offline's qwen3_asr_llm.gguf: a llama.cpp decoder-only file, the audio
        // encoder lives in separate ONNX files.
        var header = Read(new GgufBuilder()
            .String("general.architecture", "qwen3")
            .UInt32("qwen3.block_count", 28)
            .StringArray("tokenizer.ggml.tokens", "a", "b")
            .Tensor("token_embd.weight")
            .Tensor("blk.0.attn_q.weight")
            .Tensor("output_norm.weight")
            .Build());

        var reason = Qwen3AsrCppModelValidator.GetIncompatibilityReason(header, "qwen3_asr_llm.gguf");

        Assert.NotNull(reason);
        Assert.Contains("qwen3_asr_llm.gguf", reason);
        Assert.Contains("without the audio encoder", reason);
        Assert.Contains("CapsWriter", reason);
        Assert.Contains("OpenVoiceOS", reason);
    }

    [Fact]
    public void Validator_LlamaCppMmproj_IsRejectedAsProjector()
    {
        // ggml-org/Qwen3-ASR-1.7B-GGUF ships the encoder as a separate mmproj file.
        var header = Read(new GgufBuilder()
            .String("general.architecture", "clip")
            .Bool("clip.has_audio_encoder", true)
            .UInt32("clip.audio.block_count", 18)
            .String("clip.audio.projector_type", "qwen3a")
            .Tensor("a.conv1d.0.weight")
            .Tensor("mm.0.weight")
            .Build());

        var reason = Qwen3AsrCppModelValidator.GetIncompatibilityReason(header, "mmproj-Qwen3-ASR-1.7B-Q8_0.gguf");

        Assert.NotNull(reason);
        Assert.Contains("mmproj", reason);
        Assert.Contains("OpenVoiceOS", reason);
    }

    [Fact]
    public void Validator_TranscribeCpp_IsRejectedAsDifferentLayout()
    {
        // handy-computer/Qwen3-ASR-1.7B-gguf: architecture "qwen3_asr" with stt.* keys.
        var header = Read(new GgufBuilder()
            .String("general.architecture", "qwen3_asr")
            .String("stt.variant", "qwen3-asr-1.7b")
            .UInt32("stt.qwen3_asr.encoder.n_layers", 24)
            .Tensor("enc.conv1.weight")
            .Tensor("dec.blk.0.attn_q.weight")
            .Build());

        var reason = Qwen3AsrCppModelValidator.GetIncompatibilityReason(header, "Qwen3-ASR-1.7B-Q4_K_M.gguf");

        Assert.NotNull(reason);
        Assert.Contains("transcribe.cpp", reason);
    }

    [Fact]
    public void Validator_WrongArchitectureWithEncoderTensors_IsRejected()
    {
        var header = Read(new GgufBuilder()
            .String("general.architecture", "whisper")
            .Tensor("audio.encoder.conv1.weight")
            .Build());

        var reason = Qwen3AsrCppModelValidator.GetIncompatibilityReason(header, "ggml-base.gguf");

        Assert.NotNull(reason);
        Assert.Contains("'whisper' model", reason);
    }

    [Fact]
    public void Validator_File_NotGguf_ReportsFileName()
    {
        var path = Path.Combine(Path.GetTempPath(), $"not-a-model-{Guid.NewGuid():N}.gguf");
        File.WriteAllBytes(path, Encoding.ASCII.GetBytes("\u0008\u0001\u0012\u0004onnx"));
        try
        {
            var reason = Qwen3AsrCppModelValidator.GetIncompatibilityReason(path);

            Assert.NotNull(reason);
            Assert.Contains(Path.GetFileName(path), reason);
            Assert.Contains("not a GGUF", reason);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Validator_File_Qwen3AsrCpp_IsCompatible()
    {
        var path = Path.Combine(Path.GetTempPath(), $"qwen3-asr-test-{Guid.NewGuid():N}.gguf");
        File.WriteAllBytes(path, Qwen3AsrCpp().Build());
        try
        {
            Assert.Null(Qwen3AsrCppModelValidator.GetIncompatibilityReason(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private sealed class NonSeekableStream : MemoryStream
    {
        public NonSeekableStream(byte[] bytes) : base(bytes, writable: false)
        {
        }

        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override long Length => throw new NotSupportedException();
    }
}
