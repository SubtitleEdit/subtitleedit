using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.AudioToText;

namespace Nikse.SubtitleEdit.Logic.Download;

/// <summary>
/// Central registry of SHA-256 hashes for downloadable artifacts (release archives).
/// Used to decide whether a locally installed download is up to date, outdated, or unrecognized.
/// </summary>
public static class DownloadHashManager
{
    public enum UpdateStatus
    {
        /// <summary>Hash is not in the known set — do not prompt for updates.</summary>
        Unknown,

        /// <summary>Hash matches the latest known release.</summary>
        UpToDate,

        /// <summary>Hash is known but older than the latest release.</summary>
        UpdateAvailable,
    }

    public static class CrispAsr
    {
        // Hashes of the release archive (.zip / .tar.gz) — used when a sidecar hash exists alongside the install.
        public const string WindowsCuda = "CrispAsr.Windows.Cuda";
        public const string WindowsVulkan = "CrispAsr.Windows.Vulkan";
        public const string WindowsCpu = "CrispAsr.Windows.Cpu";
        public const string WindowsCpuLegacy = "CrispAsr.Windows.CpuLegacy";
        public const string MacOs = "CrispAsr.MacOs";
        public const string Linux = "CrispAsr.Linux";
        public const string LinuxCuda = "CrispAsr.Linux.Cuda";
        public const string LinuxArm = "CrispAsr.Linux.Arm";

        // Hashes of the unpacked main executable (crispasr.exe / crispasr) — used to detect the
        // installed version when no sidecar is present (e.g. installs from older SE builds).
        public const string WindowsCudaExecutable = "CrispAsr.Windows.Cuda.Executable";
        public const string WindowsVulkanExecutable = "CrispAsr.Windows.Vulkan.Executable";
        public const string WindowsCpuExecutable = "CrispAsr.Windows.Cpu.Executable";
        public const string WindowsCpuLegacyExecutable = "CrispAsr.Windows.CpuLegacy.Executable";
        public const string MacOsExecutable = "CrispAsr.MacOs.Executable";
        public const string LinuxExecutable = "CrispAsr.Linux.Executable";
        public const string LinuxCudaExecutable = "CrispAsr.Linux.Cuda.Executable";
        public const string LinuxArmExecutable = "CrispAsr.Linux.Arm.Executable";
    }

    public static class LlamaCpp
    {
        // Hashes of the release archive (.zip / .tar.gz) — used when a sidecar hash exists alongside the install.
        public const string WindowsCpu = "LlamaCpp.Windows.Cpu";
        public const string WindowsVulkan = "LlamaCpp.Windows.Vulkan";
        public const string WindowsCuda = "LlamaCpp.Windows.Cuda";
        public const string LinuxCpu = "LlamaCpp.Linux.Cpu";
        public const string LinuxVulkan = "LlamaCpp.Linux.Vulkan";
        public const string LinuxArm64Cpu = "LlamaCpp.Linux.Arm64.Cpu";
        public const string LinuxArm64Vulkan = "LlamaCpp.Linux.Arm64.Vulkan";
        public const string MacOsArm64 = "LlamaCpp.MacOs.Arm64";
        public const string MacOsX64 = "LlamaCpp.MacOs.X64";

        // CUDA runtime sidecar archive (NVIDIA cudart redistributable) downloaded
        // separately by LlamaCppDownloadService.DownloadCudaRuntime when the user
        // picks the Windows CUDA variant.
        public const string WindowsCudaRuntime = "LlamaCpp.Windows.CudaRuntime";

        // Hashes of the unpacked llama-server / llama-server.exe — used to detect the installed
        // version when no sidecar is present (e.g. installs from older SE builds). The Windows
        // CPU/Vulkan/CUDA builds ship an identical llama-server.exe (the backend lives in the
        // ggml-*.dll), as do the two Linux builds, so there is a single executable key per OS
        // (plus per-arch on macOS / Linux) — enough to tell "outdated" from "up to date".
        public const string WindowsExecutable = "LlamaCpp.Windows.Executable";
        public const string LinuxExecutable = "LlamaCpp.Linux.Executable";
        public const string LinuxArm64Executable = "LlamaCpp.Linux.Arm64.Executable";
        public const string MacOsArm64Executable = "LlamaCpp.MacOs.Arm64.Executable";
        public const string MacOsX64Executable = "LlamaCpp.MacOs.X64.Executable";
    }

    public static class OmniVoice
    {
        // Hashes of the release archive (.zip) - used at download time to fail fast on a corrupt
        // or tampered file, and later to recognise the installed version from the .installed.sha256
        // sidecar. There is no executable-hash fallback because OmniVoice has only ever shipped via
        // SE with a sidecar (no legacy installs to migrate).
        public const string WindowsCpu = "OmniVoice.Windows.Cpu";
        public const string WindowsVulkan = "OmniVoice.Windows.Vulkan";
        public const string WindowsCuda = "OmniVoice.Windows.Cuda";
        public const string MacOs = "OmniVoice.MacOs";
        public const string LinuxX64 = "OmniVoice.Linux.X64";
        public const string LinuxArm64 = "OmniVoice.Linux.Arm64";
        public const string Voices = "OmniVoice.Voices";
    }

    public static class Qwen3TtsCpp
    {
        // Hashes of the release archive (.zip) - same role as OmniVoice's set. Index 0 must match
        // whatever release Qwen3TtsCppDownloadService.cs is pinned to, otherwise installs will be
        // misreported as "update available" immediately after a fresh download.
        public const string WindowsVulkan = "Qwen3TtsCpp.Windows.Vulkan";
        public const string WindowsCpu = "Qwen3TtsCpp.Windows.Cpu";
        public const string WindowsCuda = "Qwen3TtsCpp.Windows.Cuda";
        public const string MacOs = "Qwen3TtsCpp.MacOs";
        public const string LinuxX64 = "Qwen3TtsCpp.Linux.X64";
        public const string LinuxArm64 = "Qwen3TtsCpp.Linux.Arm64";
        public const string Voices = "Qwen3TtsCpp.Voices";
    }

    public static class Qwen3TtsCrispAsr
    {
        // Hashes of the GGUF model files served by cstr's HuggingFace repos. Unlike the
        // qwen3-tts.cpp archive keys these never change unless cstr re-uploads a model, so
        // each key holds a single hash rather than a release-pinned list.
        public const string VoiceDesignTalker = "Qwen3TtsCrispAsr.VoiceDesignTalker";
        public const string CustomVoiceTalker = "Qwen3TtsCrispAsr.CustomVoiceTalker";
        public const string Codec = "Qwen3TtsCrispAsr.Codec";
    }

    public static class VibeVoiceCrispAsr
    {
        // SHA-256 of the GGUF talker files served by cstr's HuggingFace repo. Same shape as
        // the Qwen3 keys — pulled from the HF tree API's lfs.oid for each file, verified
        // against a local sha256sum of the Q8_0 file.
        public const string TalkerQ4K = "VibeVoiceCrispAsr.TalkerQ4K";
        public const string TalkerQ8_0 = "VibeVoiceCrispAsr.TalkerQ8_0";
        public const string TalkerF16 = "VibeVoiceCrispAsr.TalkerF16";
    }

    public static class IndexTtsCrispAsr
    {
        // SHA-256 of the GPT talker (3 quants) and the shared BigVGAN codec. Verified
        // against local sha256sum of the Q8_0 + bigvgan files.
        public const string TalkerQ4K = "IndexTtsCrispAsr.TalkerQ4K";
        public const string TalkerQ8_0 = "IndexTtsCrispAsr.TalkerQ8_0";
        public const string TalkerF16 = "IndexTtsCrispAsr.TalkerF16";
        public const string Codec = "IndexTtsCrispAsr.Codec";
    }

    public static class KokoroTtsCpp
    {
        // Hashes of the release archive (.zip) - same role as OmniVoice's set. Index 0 must match
        // whatever release KokoroTtsCppDownloadService.cs is pinned to.
        public const string Windows = "KokoroTtsCpp.Windows";
        public const string MacOs = "KokoroTtsCpp.MacOs";
        public const string LinuxX64 = "KokoroTtsCpp.Linux.X64";
        public const string LinuxArm64 = "KokoroTtsCpp.Linux.Arm64";
    }

    public static class Piper
    {
        // Hashes of the release archive (.zip / .tar.gz) - recognised from the .installed.sha256
        // sidecar written at install time. Piper is pinned to a single fixed rhasspy/piper release,
        // so each key currently has just one known hash.
        public const string Windows = "Piper.Windows";
        public const string MacOs = "Piper.MacOs";
        public const string LinuxX64 = "Piper.Linux.X64";
        public const string LinuxArm64 = "Piper.Linux.Arm64";
    }

    public static class WhisperCpp
    {
        // Hashes of the release archive (.zip) — used when a sidecar hash exists alongside the install.
        // Each backend has its own folder (Cpp / CppCuBlas / CppVulkan), so the variant is implicit.
        public const string WindowsBlas = "WhisperCpp.Windows.Blas";       // whisper-blas-bin-x64.zip — default Cpp on Windows
        public const string WindowsCuBlas = "WhisperCpp.Windows.CuBlas";   // whisper-cublas-12.4.0-bin-x64.zip
        public const string WindowsVulkan = "WhisperCpp.Windows.Vulkan";   // whisper-vulkan-x64.zip
        public const string MacOs = "WhisperCpp.MacOs";                    // whisper-mac.zip
        public const string LinuxVulkan = "WhisperCpp.Linux.Vulkan";       // whisper-vulkan-linux64.zip — default Cpp on Linux
        public const string LinuxCuda = "WhisperCpp.Linux.Cuda";           // whisper-cuda-linux64.zip — CuBlas backend on Linux

        // Hashes of the unpacked main executable (whisper-cli / whisper-cli.exe) — used to detect
        // the installed version when no sidecar is present (e.g. installs from older SE builds).
        // Linux Vulkan and Linux CUDA produce identical whisper-cli binaries (the backend lives in
        // libggml-*.so), so executable-hash fallback is intentionally Windows + Mac only.
        public const string WindowsBlasExecutable = "WhisperCpp.Windows.Blas.Executable";
        public const string WindowsCuBlasExecutable = "WhisperCpp.Windows.CuBlas.Executable";
        public const string WindowsVulkanExecutable = "WhisperCpp.Windows.Vulkan.Executable";
        public const string MacOsExecutable = "WhisperCpp.MacOs.Executable";
    }

    // For each key, hashes are ordered newest-first. Index 0 is the latest known release.
    // All hashes are lower-case hex SHA-256.
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> KnownHashes =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // CrispASR — https://github.com/CrispStrobe/CrispASR/releases
            // Index 0 must match whatever version CrispAsrDownloadService.cs is pinned to,
            // otherwise users will be prompted to "update" to the same version they just got.
            [CrispAsr.WindowsCuda] = new[]
            {
                "49d05553b40bc714a687b183a1287692858ab4ed10fc133e74541e05d1f0838b", // v0.6.12 (current download URL)
                "59735edc57b3d1a6ea10a50a46040977514da03a26e48eafa2fa2103ecca206b", // v0.6.11
                "9294f01473fe2cd5cc346897f763b04d91534475640f41b791f5343dc1007ff9", // v0.6.10
                "a9a04f7a9eb3eca727112f217338329907323ed4a69dc42523acc216bd1989d3", // v0.6.9
                "b2a3eb60bd674d8a176b885227783036014193a84e25f597ddf62a06ad363f2d", // v0.6.8
                "43dc3ed70aaac3eec976871905fadbf818d8dc34bc0d7f868d0a6b1622f4c63b", // v0.6.7
                "be610e9a8bb283cc283dc3d0df45b5f110ccb350a13443e9b8e4092345d78596", // v0.6.6
                "48d279a0d8358dde68d95ccdc6a5ac8e3eb8dd65dc2a400166f33dd072aa7202", // v0.6.2
                "85f78707ddd072e084d89fef9b0d63c0bd2afe017b72d2f0841ceba8c89a42c7", // v0.6.0
                "b2cde0597c6653d2a0c71738258d226c29fb84258b4d90e8f7d734ebdce01681", // v0.5.7
                "a9c92c4dbe62e88fb63f124d7f2ba3999e03785c7137dfba5265f56a59b23781", // v0.5.6
                "3d13af48ea00b7eab78a854e298c2bb06860801f87b2d7f0f161912fb9fae8c1", // v0.5.5
                "5e0f30065219a1ddf0bc03c74d10b411f210534d8318e87a9b9eb8cea99908df", // v0.5.4
                "a5296dc09d3cbd393ac694b39a629aaa521d7130ff0a369b90117fcab2bdd805", // v0.5.3
                "eee943adef921529e0a1c5d1d14f358cbafe8e5dc1260147e374b029932e774c", // v0.5.2
            },
            [CrispAsr.WindowsVulkan] = new[]
            {
                "3522ffa10bbe0d678fd1a0c51c67b9ecc7c31acb02bcb0f4b8d76fe6b9ce42fc", // v0.6.12 (current download URL)
                "7aeb3b823e747cdb89ff0a717d939c85467cabf90395481e2091d6cac678a98c", // v0.6.11
                "c9fa45a03e52c1bc642d713b8cf02953bfa7905a83911e723fe7aae546c3d41f", // v0.6.10
                "38047a59e03cd69f32f4e7dc8de39649e62c993a459289e4636659d4105769d7", // v0.6.9
                "da28d90f1883ea2cb0b08583b00f442a39b3732232f06ea6c9cd96e6c2d24b81", // v0.6.8
                "95dc640e612cc9cc95266268cc2fd03841f5fa82088a64bddb0c7ff881d7737d", // v0.6.7
                "3fda38ec66b75eca1d9145787ace497a4ca56ce2d6c218773f925f458790622d", // v0.6.6
                "b70420f6f2ed71f7390bc9f3960175ead212f41f03d0509d485902c1b2c8c882", // v0.6.2
                "1b779c606cf514b543455a3afc72c63046d1423494045feebe5ff5ea414811d9", // v0.6.0
                "999d582ed6d6ba22ec51fc02b5f2d43d48d2c69cd562e22760aad223b138c391", // v0.5.7
                "7f9c804e2a4d1c0a6b01bef2be9326e27a9ceb34534834e3c027b00dd5661c8b", // v0.5.6
                "902d1c5e8b83887cfe407048a56c67406074fa7724225ac6770cd56544487a76", // v0.5.5
                "1f0793e6279bc5e82a17eaacc6a2227842d4a839f9ed3b2e244b8515746a66bd", // v0.5.4
                "aadb324d33dea7c28ff703a64403169a0814dc71449f7d635f270e8bc1e0b7c3", // v0.5.3
                "b7c58fba959f8a7a013e458764cf526a4cf6595d8c6247b8893c37cb8c9dd000", // v0.5.2
            },
            [CrispAsr.WindowsCpu] = new[]
            {
                "90b3d102c58a73ea7f436c2651028774694ff4d7054cfd064020f0addcc152da", // v0.6.12 (current download URL)
                "439f6dc567d43326a25fbbe982e4936c20230423bbfc3e57b2e9e6bb39f4f1bd", // v0.6.11
                "3b04b6e4d601f8d5aeac2809a887138e07d20ab65704ac55279a7253b88d182c", // v0.6.10
                "44175685b630cb8e657dc9cc173c70218fcc8fdbcf2271a09c5061b8019b48ef", // v0.6.9
                "621c6e811eeba9873abfe8f01fb2f4c08c7190a14106384b3e11a4b25bc4c86d", // v0.6.7
                "05f629c4d022fb8a05a24b16cb155c45ab65e90dc0aa7eed46ae31feccf43de8", // v0.6.6
                "4b36e5634c1acc7f7387c9bce3b1302e8fbd8441b3e10d37b5d5952064bbc552", // v0.6.2
                "46fe3bc88966c973eef66b7c2271f95bb40b2b4bf338643e71834186cba0ae3d", // v0.6.0 (first non-legacy CPU build SE has shipped)
            },
            [CrispAsr.WindowsCpuLegacy] = new[]
            {
                "c088088922bf1da60c5ec8b29f975b1f3b576a8a6c184b8485351421e50f973c", // v0.6.12 (current download URL)
                "3cdf426fe74d61f80aa5ce0e4e939fcfd6135c7a9d46ab1bdb82084df4741917", // v0.6.11
                "ef8961d3b7ca069245670f692340ca0305686a670a3e10eb4f9978da6298a7c0", // v0.6.10
                "7d0631467890b4c865b89c01029c1332129e255b9755c77592d9ffe6380f344e", // v0.6.9
                "8a33a0fb0444e95f06ff1a48bb4b48436d6a59bd7c96c6e74db0221a1a215423", // v0.6.7
                "d9fd9306246cda7b4b3006441aad8ba755d617b066f6f033358b51c860d28f89", // v0.6.6
                "eb27d98fc8051d38dca76c0e0fc2a2b1fcbfbbac18267e781dbb2839367b9f18", // v0.6.2
                "5b59d9268f37c683cc8793322d553121d850163d7a8ac3ca8323a05270b5a999", // v0.6.0
                "e04be09ca8fb608c54de0d823a6a761adc93a16ab9ff5d4c7025e5515e1759e7", // v0.5.7
                "e8eddaa4a988be019919c8a0c3fae32680732f4b17ab2ee06a8756200cc4883a", // v0.5.6
                "c111fe567df600e52754e0eab93d2cd37527623328abe2c4f8dae283ae2ae059", // v0.5.5
                "bbc6422fb0346dc79a7be41b0800ffd67b42dfe81691084c4bc76c85a1caa985", // v0.5.4
                "88e62281ce19047e34290680ecab35ea2e24af6fc2b9edd6244234733a228703", // v0.5.3
                "7faa7c92b4b48a64fb653f13e0e985618d4495d6d05abc366b3fd4b27098e65c", // v0.5.2
            },
            [CrispAsr.MacOs] = new[]
            {
                "48a7652983a2a6f798f62ecdd3ef43708af2f0c9493c55dfb30474a23f24a41e", // v0.6.12 (current download URL)
                "242c0267525c5587e23591d43f60b480e9aff7bef80817ab51ddc882476f2020", // v0.6.11
                "b73b2af074477c5ca7e99235d5ddb80d71464501c6c7946ee54b8eb4da585a2c", // v0.6.10
                "df0989bbca7d87ab2343405058cb5fa3e985fc3a601e54ac35eb7a3ae10d27b4", // v0.6.9
                "8fa0fc2bd7ff8889de87c5b19bcf24c5882abd1aa9a03b98a980cd944a70c914", // v0.6.8
                "a2360c4c425345338cf468a19c978cabd61ddd9873d863075e882bb46695abd8", // v0.6.7
                "32dab6eb5f2be8150f3f66131dfdd09da5f7a2682ff1e594794bcbf51b5a3c91", // v0.6.6
                "8d8a882cd4887c521197e03389e1cfad693ccd85a78eb66af24d348e97add41b", // v0.6.2
                "0594f4d499f4fb78ecb2c8b25287fd61b7708339a501b4eb609cf0c508126fea", // v0.6.0
                "dd5ff92c4ba587e35c41667d0390fadbcc4c7e6397682369025fd1e526c99ccc", // v0.5.7
                "20bdcd64a2e33d1f111be9b9447073486080dc73b153bb6d51aed23f5e4d4c23", // v0.5.6
                "04881204e0d18fada97206cfda6f7ea5b9a8ca2a62992f66a798e51ea285d97d", // v0.5.5
                "87c5b462f97a47af2e7d6050674955ba5ae4a05e9a1a0e1e05690e2797c2889d", // v0.5.4
                "0a68e2c24e3985f7a57c1923f5f789e317f78e2193ce93bdc27f53ee26a3f145", // v0.5.3
                "24aeadd5b4d80e3a817ac11db8c8acd9fedf444e44fcf12e7ebec57d93d599d3", // v0.5.2
            },
            [CrispAsr.Linux] = new[]
            {
                "1fdca9e9f239519be3f339b899b5bf390d6d25428fec55470f8932f7df10c92e", // v0.6.11 (current download URL)
                "b2e3a20866ef282dbe08cf9c98491efd3396bffd6b688d0c13626593b83d3b0f", // v0.6.10
                "7d2642301b7185f6e18a219da8b9e363ce0972f9331f51eb91a6af530c16d484", // v0.6.9
                "da3ea0675168a3c7396858ffe9c483a401e581ae3bb080f8825a5e0cbc8b5589", // v0.6.8
                "54edc828b29ab25217582d906416f07109dfd6d085f9c407521d0411268a9665", // v0.6.7
                "2ac612bada388345c2dd9580353e2401e07924cb3fbe37c0cae6c5564b51068e", // v0.6.6
                "52a9b6791ef23c73b0448475712611cd97bbd8598ee0101e7f28ff3e8ba5906b", // v0.6.2
                "f63aba89b6371128d0cd11167e280d1b987853996c4110de9ce48dabe55b20f6", // v0.6.0
                "9981f330a96715bdd232a08ca6d485305e7e0e95c1eb1b51ffac68424f14d311", // v0.5.7
                "b7cd7b95180e85bb5f76d3b0c22c2ab8dfabc616df8baf2d918f215736131134", // v0.5.6
                "ee476a912b5525874a70c2dc52604915c97672104c8c0a207e8a9c6ebbbe1f37", // v0.5.5
                "ea751eaedcc5dc2a5772f9e3f1fd8aae87314d501b07fa6d4acd03bacf8e7ecb", // v0.5.4
                "74c96662f49b4ae4640d12f152cc892dad2f21c354810a4bc38a630dc0da8195", // v0.5.3
                "4763b6f92f4813da7380a8da82eb1b234189c1c67e8cacb119a5d115f041ec30", // v0.5.2
            },
            [CrispAsr.LinuxCuda] = new[]
            {
                "b1b772fd5d17c6ab288830a7afc07cffc448951fd9b673f73f77686c563302b3", // v0.6.11 (current download URL)
                "76223ab25faaf03be98afd9c934932e29bb527f32642123395435d47e3089228", // v0.6.10
                "870c37ef44afc8d63f216c192719c9cc5291493eeac43d2709d64fe5c826cecb", // v0.6.9
                "9a493e0f19421e3b73307a48113dd18c326756dfe751bd3a9bc4bc8d049137d1", // v0.6.8
                "0e2ebbcb8012285a8c2089c5254d61f8e4e870e3397f220bcd9708ec569f3037", // v0.6.7 (first SE-tracked Linux CUDA build)
            },
            [CrispAsr.LinuxArm] = new[]
            {
                "8a8edb05d25ba793bcba90c528332ae17ef9324d689aa3ffd7c9dda48262bdbb", // v0.6.11 (current download URL)
                "f8b6e6f080dc5bd227b39417a48d525de3962c85519fa40debc1aec4e0251bfa", // v0.6.10
                "b3dacccb8d16afb88a7c426edbeefaa6c8bee0f6bc5a5e6493fb4616c2ec32d1", // v0.6.9 (first SE-tracked Linux ARM64 build)
            },

            // SHA-256 of crispasr.exe / crispasr extracted from each archive above.
            // Windows CUDA executable hash intentionally omitted (the CUDA archive is ~700 MB; not yet hashed).
            [CrispAsr.WindowsVulkanExecutable] = new[]
            {
                "801f811fdc57c1ccf2d682831acd856ff0d9397c5de342965ce9cd9aba78a5fb", // v0.6.12 (current download URL)
                "ce5c6c0da86714aa7df28f5f78cb75c707fb891d7bf0a09d0c68ed1c30112564", // v0.6.11
                "c2c1b7826de0caa76f15998a0b06bd663f88c601eb24f3cd0a4443036efb7231", // v0.6.10
                "90776be4e17bdb47dd9f912e43d201a90e89906e8845c56f06dd49bf7c797b5e", // v0.6.9
                "e0546b739f17b4e5fdbe732801e695006b48c6019247f4fe0b51149163ec9a00", // v0.6.8
                "137a80127527d4cec89a93616cf94e072b437ed4d674b4a0fe27839be26ab6a0", // v0.6.7
                "fc9a592eb6cfa41d74f165e65cabe607712a06d546ba77bc3e3a93823ac9d3f3", // v0.6.6
                "ac0b6c10d27de1d908759c4cfbbb6062937edac3f712cc51b8c2f5d37e73bcaf", // v0.6.2
                "9b3354d3b0e5b91fa2cdcc1ce65e880426dea026671038329f44ec994fa52454", // v0.6.0
                "1cc42365ff5862f60328a0871c933aa353d5f5a14aa48f37655d7a0f5d199ef4", // v0.5.7
                "413134696606f8febb7113cbadf358fc395a0dae1882efcec7500422c3813baa", // v0.5.6
                "280369f0863a7261ff34d928df331b1baec57e3be0ff9973c416e8a7fdb84181", // v0.5.5
                "50a4934aa3adb7bd9e78ddea407f9125f605ebf85f0f1fb286718a0216b5140d", // v0.5.4
                "6217ae5ff00fac3997225e930576aa9ff58b8708d8a66a86163c2c555bb88ec5", // v0.5.3
                "112862f031cfc65656e282a61b0b156f8f3037a3d2f606df826fb7ed824d3d92", // v0.5.2
            },
            [CrispAsr.WindowsCpuExecutable] = new[]
            {
                "82d1fc1d5812fc9606f9958df655d3761969d9c39da40dc299b374b222c81b0f", // v0.6.12 (current download URL)
                "3cacd6f6b1ca0de7c7420d33ffb5c0f162240a6278efcc81ae43f119f94314f0", // v0.6.11
                "1445a938cbd7d0e62b250a9333b81fdb53c609526d9706234f3fd6dadf7b903f", // v0.6.10
                "a9900bcc3c8bae6b092b940c9df210e5514c7faeecffa6f1d7c6bc85b4d02e6e", // v0.6.9
                "a528743af75665975bd7d2ca71cdd758fa7fb261b49755a6fdf7f0c113506ec4", // v0.6.7
                "69f01b93054cf0c359eaa31f80f6ff06452f52de5306588aa2c743c0a5a6c4f5", // v0.6.6
                "b63eb3a28ae7da8c0fcb486fa0f460a07ad85ace545359a338fc053bdea69dbf", // v0.6.2
                "28da4dcf6e72738b408400ebdef00203f8e70dccf392446ecee90a15c971e186", // v0.6.0 (first non-legacy CPU build SE has shipped)
            },
            [CrispAsr.WindowsCpuLegacyExecutable] = new[]
            {
                "c367916596b139868f1785e358ece9cc128ee4c8b264839a4e5a58ec8e56294d", // v0.6.12 (current download URL)
                "8cf5e71fcbc774a1c691989c5f1a8419104edf66455ff98f80b91589aac453e6", // v0.6.11
                "c4f945b2b671de52652b2e331959a6695ac3605b7664b50cffe255f03fd5e065", // v0.6.10
                "2bb24c9e0f1d4d887622b492dc018ea690df1b59858b0aeb18152cd1cb9f6938", // v0.6.9
                "d049f9da2d5599e308cb3a6432f64271c46e8acd4989556e7b9b730c55508f64", // v0.6.7
                "352483e379cdeff5d42d2cd4f95c3c41f73f451986b26ee7a65843281cf4153c", // v0.6.6
                "089c35a3775292e3d13c200c00612d9c26709e1007f78e0139dca1466b9d644e", // v0.6.2
                "a399e96790cd170c95c354f152f88e9cd1dc44b4a6becb4f5eb2b7f203d8a184", // v0.6.0
                "44c6865ac7795c6e09b65a516b0111b9fd77e72758ab25d1e1768bbb75a3a0c8", // v0.5.7
                "f4dbf3f11245632c8f56872e1f8a56185d336c17a858fe97876fb12e76bbe2e1", // v0.5.6
                "057e7c642f2da1cf5ddb84ddac8d740234d8b84ddf33dadcb2bae91c06692956", // v0.5.5
                "07ae4e499abb68fd64987a617c69aa0a007753eb9409d379229a5a871584025f", // v0.5.4
                "e05962d3fb38336f340150b285aa6c78c31fbb7c63791748934e30d9aac81a25", // v0.5.3
                "07f8e26b7068f9af0ebe0fe2fe4d4335c9f5c2719e753620655ba9a0961a6fdc", // v0.5.2
            },
            [CrispAsr.LinuxExecutable] = new[]
            {
                "1ab580b274f2614c52462d2b5883ac62be11871db9926436d9915c1adbc99ed9", // v0.6.11 (current download URL)
                "334e25029a409a340ec0727c88328fc53ffaa45af75fc42e59a47ace75232654", // v0.6.10
                "ee374d16df3fedf900b694f28f8b9500b415798444635ea8b967796dcfc2a1ee", // v0.6.9
                "6d2ae10f068cd152f2f0d49cb4de0fca39d7085a9b71e0ef2f514ded06e47627", // v0.6.8
                "425bbf26de4e6f742cb191c08eef1676a167c11a8f46cc47942b50551f929d32", // v0.6.7
                "31e955f45739471c5d1464d8e8c484704124715e13c944c27b6d47db6857ef06", // v0.6.6
                "32f23c42260c0f315b51818c674505b4e7c619af1b17d13f07ccad9a76a4aed0", // v0.6.2
                "ddc28fde90714947947f88867f45661d1e2a4a8ef578f1c2dd0c368acc2a4a44", // v0.6.0
                "4aff54e57adb62b40f8abf508147708262087ceef15a36b2732b0670d7947326", // v0.5.7
                "743ec20ddfeb0f182d3175080202f2b06636dc8f8da6514aec94e2ccdc5b33a2", // v0.5.6
                "467a599cd152e81706bb34f945e2a68bed09ca78f28ea3a875bbdd58c44996fd", // v0.5.5
                "d33905a3afb3372e0f8173eba8c65469db6dfafaa4786034a88fd7da2bbb2931", // v0.5.4
                "2733a81f64c742981bc0a7cf3dff51dc16fbaf028b10f481fb908178b49ba627", // v0.5.3
                "a6ef3181657f417d9fadbfae343110b42ae788d053de5e3f48407cae8da8f9d2", // v0.5.2
            },
            [CrispAsr.MacOsExecutable] = new[]
            {
                "ebed4020d36539d78cc609bf414601e31cc9669a3fc00d509f597d0d4d1175e7", // v0.6.12 (current download URL)
                "16b5bea6f9a012dfb67d39009972c25ba5c408414feae4cf8a20b1018f52df67", // v0.6.11
                "08a3f25d2ee29be87f9f6c1ed60fe48d21033dda71183088d7c4442fce8b04b0", // v0.6.10
                "3e0d5398091bb7af2d492bf0fdb34ea4664e1b781183c5a0f1ae0c9d7989fb9f", // v0.6.9
                "552421500735de8ccf15565e8e2bf5556c5ceb7b2f6c41609e3f91ce1f138a03", // v0.6.8
                "7d2edd77f31882885a41956fb7c5d8e8345c5532361c98180af26cea1a2ae1b6", // v0.6.7
                "abfb92985fed2b69e25473b7dd6a39b637f7e3025d7b379077a762d7cf8df8de", // v0.6.6
                "fc60809052f50622663ae4912088f9f547023e4b6d528e902aa0e1a6619fe387", // v0.6.2
                "63b6a4197e74f5ddbd873f7db1e9e962fddd4a35b0fbc42eb7c898b5c9c1d964", // v0.6.0
                "96e3db2930ab3b46687711ee3744499c186d5e9eff1858e10137fbdf3b4a3614", // v0.5.7
                "34c9d121d96113015b2e2ba75faf268afa123d9bc847f32e819e3bc987beec47", // v0.5.6
                "380239b91448bcc2ce95ebb2179f3b8f5c4168d2f30f1a2c43e5f4d81d2bc79d", // v0.5.5
                "7ea6e45b16f396c5cfee8447b0245e872399e504b6dc3d8bb81c3a3c262acbb5", // v0.5.4
                "a92723269e7e16b93184aadadef3868396e75994665066b817218a0d208c1d2e", // v0.5.3
                "b99c7d6f51652f7bcddfb6b5bd73f11c541f9947256f040758a20bd1c7ad6591", // v0.5.2
            },
            [CrispAsr.LinuxCudaExecutable] = new[]
            {
                "f5e36703104feead8bc51953c29dae372e27848e135fff5d973c8c456a8cd31b", // v0.6.11 (current download URL)
                "308c45c232f9d6e7f96840f43ceb786c5ff159b7e9de22af4485444d54a65e4a", // v0.6.10
                "6ae31308ff2856a42bc13de16470e95c5cc03a26ee117e3386005364ee384c75", // v0.6.9
                "363864a0f41e7f61a930c8f070e1b1357dc9b86063f747b719651890b446d0a7", // v0.6.8
                "6af1d3dace190fd7047ce90634764c80bf2fb6d64094e2a5821afe8e0c74bb65", // v0.6.7 (first SE-tracked Linux CUDA build)
            },
            [CrispAsr.LinuxArmExecutable] = new[]
            {
                "051c3f6fa9ccabc8cd8d914d6a9504c6e559e7d4e6634bc93fc9bc1a51293538", // v0.6.11 (current download URL)
                "b6eefd3d111d138000c3e04e83976358e80fdb72e9555bf20a7267d17bf21fe3", // v0.6.10
                "865b215977eb67035af8cd51bf17a657de5a69c8d076fa287652d27e69bac8c0", // v0.6.9 (first SE-tracked Linux ARM64 build)
            },

            // llama.cpp — https://github.com/ggml-org/llama.cpp/releases
            // Index 0 must match whatever version LlamaCppDownloadService.cs is pinned to,
            // otherwise users will be prompted to "update" to the same version they just got.
            [LlamaCpp.WindowsCpu] = new[]
            {
                "ea7f96855a08c3ad2597d45808cd7304ef00607d99654b76cdf0e7bc37b18633", // b9297 (current download URL)
                "15739e9a11aa587d8232d2bc473a7dd7b3ac7f2f2384276309bc91474084ab1e", // b9174
                "8e2266646ede106a112023340adc25a2c2150243826e9dfe4bd9f7ed30f8e042", // b9145
            },
            [LlamaCpp.WindowsVulkan] = new[]
            {
                "533bfc014a28941b624b70ac177ada2d463e1b64b6ddaf4dfebfe4aef6ef931e", // b9297 (current download URL)
                "120b9c822f1c55e7e6f05d77743ebfb53a8c622fa8d34fc57847fdb9f628cb7a", // b9174
                "5b54c452458bf6ad06d030a18458c484848b9def4d56b7b94171ff464e670d66", // b9145
            },
            [LlamaCpp.WindowsCuda] = new[]
            {
                "ad58f4d4e19874c6588d430f6241ccf884cf08eb22c76fde61e76f7d959a4206", // b9297 (current download URL)
                "edbdcb670f04f6fac0da35cffeff1fa9ae22319cce327074d19b39f98b6c4838", // b9174
                "a323ccfa87aaa31c47d62be75181799f16f2fe4636786c09bd8beeb6ac722266", // b9145
            },
            [LlamaCpp.WindowsCudaRuntime] = new[]
            {
                "8c79a9b226de4b3cacfd1f83d24f962d0773be79f1e7b75c6af4ded7e32ae1d6", // b9297 (current download URL)
            },
            [LlamaCpp.LinuxCpu] = new[]
            {
                "7abf0249c7d73d100770779bf3a189ed7dcdfb42b13d3d4012a063c9954798ac", // b9297 (current download URL)
                "94325e296a4561cb95161321b546685a6821513b4fab1a7cb28f76d4003c56d1", // b9174
                "4cb45bdbf358bc15c77acb84e83e311a3f2c9247b9980f4f1886ebe3bd7e46d0", // b9145
            },
            [LlamaCpp.LinuxVulkan] = new[]
            {
                "f50222f987a431561c843c42cdb6d384bb40fd63a1c88cb2e63b85094bdb0f7e", // b9297 (current download URL)
                "d5787fa775220b3c0f896947885fa0f3dedddbb5388bd68603376a2a97252e72", // b9174
                "b4a28b06d973f662b9465b6e0e786d8991cdab4d24101f5d88b0b8f688a97aea", // b9145
            },
            [LlamaCpp.LinuxArm64Cpu] = new[]
            {
                "cd067ee2921a5b8c825d163d622947c2c554dbcc1d43ddeebedc7ecd5ca2a518", // b9297 (current download URL)
                "7ad708abbb58a6dd5a148eed24d9d2ccf155f57cde8cf3c0e44f73d0d2b95c6d", // b9174
            },
            [LlamaCpp.LinuxArm64Vulkan] = new[]
            {
                "78995ef0a75f3ffd7eacddb07bc214ab100c1503be7eab1efc3514e1b8c80969", // b9297 (current download URL)
                "84adac128b781f495804fb84d0c27d27e21cde6da03338b9ce0931ee324304f2", // b9174
            },
            [LlamaCpp.MacOsArm64] = new[]
            {
                "059658804b94007659364677192c451440429d39aac2443325b6320b9a244b0f", // b9297 (current download URL)
                "39406deaac6083000446530e68194b77bb64c64ecf0533040a49d8e7cfceb363", // b9174
                "3c0cef9bcf8898c3bd169e94fa2acec249b8ebc94d8fade2c165b6d6a01e2693", // b9145
            },
            [LlamaCpp.MacOsX64] = new[]
            {
                "7f179110941b3e82a24b8274c77b5cb90a7ae60dbbfd9587ceddea3a8eacce0e", // b9297 (current download URL)
                "f4040d729027dbc4352023ed32cbc5157a12a740223ec0d93d42032817554c6b", // b9174
                "0391b2dfe4a4f384916ebf98c33bfe1205443cde8bffec7563b73671d3060149", // b9145
            },

            // SHA-256 of llama-server / llama-server.exe extracted from each archive above.
            // The Windows hash is the same across CPU / Vulkan / CUDA variants because all
            // three ship the identical dispatcher EXE that dynamically loads the backend DLLs.
            [LlamaCpp.WindowsExecutable] = new[]
            {
                "fd70ce6fc227cc2c5b284ad57b90e130e223bd3ef6efc0f1467a004a818dbf74", // b9297 (current download URL)
                "569a751e9e95c8b52683ce640120997fe684d2b629e40552cf8945032a082569", // b9174
                "528ff8471ff7072ac75494a80467a0fced073ec11dcbf96a0e1c3ec0334b9dd0", // b9145
            },
            [LlamaCpp.LinuxExecutable] = new[]
            {
                "3484ef2c04f19e01bd65121f28bb137b6e18558418d1e7fcdd77e88f92766d87", // b9297 (current download URL)
                "fc6322995eafb9146db0454acb2b2d3f14244fa3eecb62ba742c129f8b4f5de1", // b9174
                "855bd9540073d9f020b9b77ad5866ea3ea74ca570ea3fac486da8676f0eb6933", // b9145
            },
            [LlamaCpp.LinuxArm64Executable] = new[]
            {
                "95770e0b72f77f1d2e726365dd65dbd720a15b871378f77104b2e963edda1a89", // b9297 (current download URL)
                "a39b9d8200591e3e16f14b933f97ba0856e464097e0ba79cd3fb101313bdde96", // b9174
            },
            [LlamaCpp.MacOsArm64Executable] = new[]
            {
                "6dcf6d0ac4df68f51953699a3c0c40d7e18ce6417379fef23d104c017be3bcf1", // b9297 (current download URL)
                "1ebdcadd5939620f1cdd9e5d2d6c7ba2902e85301f0f2ba15f51cb5beb8b4fcc", // b9174
                "61b18501af97dce62cb7e1c019981734d7b0e1003122079ac212edaeb86a36e4", // b9145
            },
            [LlamaCpp.MacOsX64Executable] = new[]
            {
                "b0af61933b55b1c8b9c2ec2dfb56963fbbe8d107e659305dc2f79b4efc3d3152", // b9297 (current download URL)
                "a63265bf5136200b2a8c2f86404151975090a58dae24df934a6054b52348aa1c", // b9174
                "c33c8f47da7d7751cd07c9ab32e77c84b3f5ac56b56299016f62b2d184fe3dfb", // b9145
            },

            // OmniVoice — https://github.com/niksedk/omnivoice.cpp/releases
            // Index 0 must match whatever release OmniVoiceDownloadService.cs is pinned to,
            // otherwise users will be prompted to "update" to the same version they just got.
            [OmniVoice.WindowsCpu] = new[]
            {
                "4af38bcb264ef34d7d39a7c76af00993e32f731a17253b7e82a9b8a6140a736b", // omnivoice-2026-05-22 (current download URL)
                "ea1f0c6032f5a0333103ccffa7d7478c5ee00a35867776c277d717512587766c", // omnivoice-2026-05-17
            },
            [OmniVoice.WindowsVulkan] = new[]
            {
                "ff90f8eb8c36ea64d46bd1d245e109ef80cadf234417bdc2c78b7420ab8f9a43", // omnivoice-2026-05-22 (current download URL)
                "b461b8535892b3e6979aec79eff4d6a0109a31184008b778b6848d7d36ea9901", // omnivoice-2026-05-17
            },
            [OmniVoice.WindowsCuda] = new[]
            {
                "735d5672507b3796138cecabcde6512e6a1e60bc9e6e86efd0fa6ce13a361fa1", // omnivoice-2026-05-22 (current download URL)
                "79308e79bb47df9f2bd2d31c6a60fe5672b74f43c3caf9b0a09566408894789e", // omnivoice-2026-05-17
            },
            [OmniVoice.MacOs] = new[]
            {
                "d5ce6807adc50e6e1f1f92235828bad7c139c50042090a5361d0506e069ad1ba", // omnivoice-2026-05-22 (current download URL — RPATH fix)
                "bbe354cdf8995641074c46d02d7f8b9353fa4803452cb45a762de10e899aca8e", // omnivoice-2026-05-17
            },
            [OmniVoice.LinuxX64] = new[]
            {
                "c8eec5e66b362ddd3504658021a890a8acd4d2c092ba51a847fce1e80f4807e9", // omnivoice-2026-05-22 (current download URL — RPATH fix)
                "198046721ebcbe49ded959fd832ec4a091d899be49e2a26b549723e32b82daba", // omnivoice-2026-05-17
            },
            [OmniVoice.LinuxArm64] = new[]
            {
                "a46029c360398fc8775d6a4d703df158fa6f3ee6576e6efd3be0b7be424e5276", // omnivoice-2026-05-22 (current download URL — RPATH fix)
                "3802d4136a6ffdc37cce72b286ab7c17422d55964fec13d4e3eb36f26e6ae1fe", // omnivoice-2026-05-17
            },
            [OmniVoice.Voices] = new[]
            {
                "5d252eb78e8f4891279a36fa5127ea5ab80be35057eeaa5fadb49baeacd0c773", // omnivoice-2026-05-22 (current download URL — unchanged from -05-17, identical to support-files/omnivoice-26-06 copy)
            },

            // Qwen3 TTS — https://github.com/niksedk/qwen3-tts.cpp/releases
            // Index 0 must match whatever release Qwen3TtsCppDownloadService.cs is pinned to,
            // otherwise users will be prompted to "update" to the same version they just got.
            [Qwen3TtsCpp.WindowsVulkan] = new[]
            {
                "08fad701e80340e6b073f292303a0242589260672f341c459572fe3a20742941", // v0.4.6 (current download URL)
                "84bc6bfa5edeadecf969acc00f5798b12eb33a3c0b884143eafb942c2b0f6dd1", // v0.4.5
                "697b4871b803172ef2d30682e099008c646591e59835e655c598d275163723c8", // v0.4.4
                "42dff2cb2e921a337c588e5331b5ea963b777751303729a66bf0124be242297d", // v0.4.3
            },
            [Qwen3TtsCpp.WindowsCpu] = new[]
            {
                "df785c1cf96a7e960a91f98e589d602b387dbada8d441ab0825efba190498377", // v0.4.6 (current download URL)
                "9018c49f4d154ac0b1a0bc946a0689a75a6d1383c788ef4e70760609124f58c4", // v0.4.5
                "e09875f4d329b1c7623af2e7ea5fae2c838c1bd880712415220323f969370975", // v0.4.4
                "afa9fef938ddd8d22a0f5c955b5c3a2e402f49a6d1ddc587da2d07d6993f4120", // v0.4.3
            },
            [Qwen3TtsCpp.WindowsCuda] = new[]
            {
                "e2897a1910781f8ad789d75c498fbcaea4fb0e619a1c92a6b1dbc648fba78c1e", // v0.4.6 (current download URL)
                "e4a3f185cd5952ac9e9a092a2b490fdd3088d070681db46d98c5e2d6cd8303b1", // v0.4.5
                "cb53effe905d05e60e61f8dcadda00544d6d427d567be017c7ed4bea2148acb2", // v0.4.4
            },
            [Qwen3TtsCpp.MacOs] = new[]
            {
                "ec98eaa8613310b77142530f0f7a29070b8995f28a53a9a9ca947ee69f6af374", // v0.4.6 (current download URL)
                "95421febb5bf04d1807e1bd6a56cf47e9b8ce2a421922a9e13103771b0fbfa6c", // v0.4.5
                "faecc2b93d9586f8d7bdf2601343c5a14f8d6bd2546578ed2559a0e2d191412c", // v0.4.4
                "625f32817ff9e5cacea48db24ad8ae1149406e382a7e58068fc87182238baa07", // v0.4.3
            },
            [Qwen3TtsCpp.LinuxX64] = new[]
            {
                "ad0ad2aa49534c88b71bf8a4c9ef7ded5d663cb80658f49d4f723bae8ab8ee03", // v0.4.6 (current download URL)
                "cb5d3f137394688bd985835a45fd0fadabff4f450091f61f0a96423c0a21471a", // v0.4.5
                "c9ecec169ebed93909ac72b65e0acbb8958047f08c8b9cf4f5d07d0e3bfc2844", // v0.4.4
                "4456b19fa62ac52c6ef6302f36effc1336d559be0fde200222be302746dd2579", // v0.4.3
            },
            [Qwen3TtsCpp.LinuxArm64] = new[]
            {
                "60bd97d3c7008fdb093238296c1f801ee011fabccdf224f47d9e3e39ba0c2030", // v0.4.6 (current download URL)
                "30d9e1ed411a67d300eb72ae8c7069e344aa662c1cc10d41b0b37e2ca558b84b", // v0.4.5
                "bba376f4ead8356a496a48d37572e6c56d66161d52026f502e74afd050654f41", // v0.4.4
                "3e9bdc9d176ea13a109d8e8a813009e110df003a988d10dc317e58572517d014", // v0.4.3
            },
            [Qwen3TtsCpp.Voices] = new[]
            {
                "ace389f8326e23dc65c22c081d13efb28b9ee5a78e8586bc259d1148f7346e05", // qwen3-tts-cpp-2026-4 (current download URL)
            },

            // Qwen3 TTS (CrispASR): CustomVoiceTalker is intentionally absent — its hash hasn't
            // been verified locally. VerifyFile skips when GetLatestKnownHash returns null, so
            // the download still works; integrity check just doesn't gate it yet.
            [Qwen3TtsCrispAsr.VoiceDesignTalker] = new[]
            {
                "ce9c6d69146891f7854ac46be3bf4e40f803fbebbfe7cdbd12ae3a4b24777295", // qwen3-tts-12hz-1.7b-voicedesign-q8_0.gguf
            },
            [Qwen3TtsCrispAsr.Codec] = new[]
            {
                "70dc95dbfdd9aa5d9d406236ff771d061bf17b0cda02a72513953355606e719b", // qwen3-tts-tokenizer-12hz.gguf
            },

            // VibeVoice (CrispASR) — cstr/vibevoice-1.5b-GGUF on HuggingFace.
            // Hashes are HF LFS oid (= SHA-256); Q8_0 verified by local sha256sum.
            [VibeVoiceCrispAsr.TalkerQ4K] = new[]
            {
                "fb034200191b7f593b0675302cceba38d209dd01c3e339a25cb1f143926d18ff", // vibevoice-1.5b-tts-q4_k.gguf
            },
            [VibeVoiceCrispAsr.TalkerQ8_0] = new[]
            {
                "d402c7afe796dddbac4a28c721fd9a8f14915a4f63b0a5120916bf287450dc99", // vibevoice-1.5b-tts-q8_0.gguf
            },
            [VibeVoiceCrispAsr.TalkerF16] = new[]
            {
                "64dface8a1080ae268191286ebd8033c18184af0ab8be6cc36038841bdba1405", // vibevoice-1.5b-tts-f16.gguf
            },

            // IndexTTS (CrispASR) — cstr/indextts-1.5-GGUF on HuggingFace.
            // Q8_0 and codec verified by local sha256sum; Q4_K and F16 pulled from HF API.
            [IndexTtsCrispAsr.TalkerQ4K] = new[]
            {
                "35a56e7a1c7c49c45586e6eb4b9c55df7711dcb07968f84d43fc7d71e079056e", // indextts-gpt-q4_k.gguf
            },
            [IndexTtsCrispAsr.TalkerQ8_0] = new[]
            {
                "9bd7edbfc9ecf9705ccc7fa08407f88e87fc0a3d7e7b7efc0195a8a335bea4b0", // indextts-gpt-q8_0.gguf
            },
            [IndexTtsCrispAsr.TalkerF16] = new[]
            {
                "37fb043a674feb045424ba2711d8d86a21705d3f85fd43a7bafd73b3ca5e76ce", // indextts-gpt.gguf (F16)
            },
            [IndexTtsCrispAsr.Codec] = new[]
            {
                "fcba9a322d80ef318da8a17c01e8a5e7f299ccdf881c62a43abf62cb3c104268", // indextts-bigvgan.gguf
            },

            // Kokoro TTS — https://github.com/niksedk/kokoro.cpp/releases
            // Index 0 must match whatever release KokoroTtsCppDownloadService.cs is pinned to.
            [KokoroTtsCpp.Windows] = new[]
            {
                "560014a5f82ccb2df2dc54b7701adfbad7d23d154783d70530c86a577a9ab918", // v0.1.2 (current download URL)
            },
            [KokoroTtsCpp.MacOs] = new[]
            {
                "39a1b4e15b48b364862ba29cf923507ee759f37d420de42bd41d333cd4dcc0ab", // v0.1.2 (current download URL)
            },
            [KokoroTtsCpp.LinuxX64] = new[]
            {
                "3383a9154a1d34f227ea4e8a1d5aff5dd60adf4061a3b14bf93e1ad8582eddf9", // v0.1.2 (current download URL)
            },
            [KokoroTtsCpp.LinuxArm64] = new[]
            {
                "673f49ffd2c0653b195a57f566038c05b2a962defc3e1c9c2664c8306d09352d", // v0.1.2 (current download URL)
            },

            // Piper — https://github.com/rhasspy/piper/releases. Pinned to release 2023.11.14-2 in
            // TtsDownloadService.cs; index 0 must match that release.
            [Piper.Windows] = new[]
            {
                "f3c58906402b24f3a96d92145f58acba6d86c9b5db896d207f78dc80811efcea", // 2023.11.14-2 (current download URL)
            },
            [Piper.MacOs] = new[]
            {
                "ced85c0a3df13945b1e623b878a48fdc2854d5c485b4b67f62857cf551deaf8b", // 2023.11.14-2 (current download URL)
            },
            [Piper.LinuxX64] = new[]
            {
                "a50cb45f355b7af1f6d758c1b360717877ba0a398cc8cbe6d2a7a3a26e225992", // 2023.11.14-2 (current download URL)
            },
            [Piper.LinuxArm64] = new[]
            {
                "fea0fd2d87c54dbc7078d0f878289f404bd4d6eea6e7444a77835d1537ab88eb", // 2023.11.14-2 (current download URL)
            },

            // whisper.cpp — https://github.com/ggml-org/whisper.cpp/releases (and SE-repackaged builds
            // under https://github.com/SubtitleEdit/support-files/releases). Index 0 must match
            // whatever URL WhisperDownloadService.cs is pinned to.
            [WhisperCpp.WindowsBlas] = new[]
            {
                "eb4a51548a65c58cb22890066145dfe1026d5bd597c52ef0ccb0477e83159c91", // whispercpp-186 / v1.8.6 (current download URL)
                "4a8a07e14c035bd6c1bcd55dedba5925f982f122d6bc05034f9bbe7e55f5c4b0", // whispercpp-185 / v1.8.5
                "5f8d6b1bcdf86edf898d21379468ae8329bb783803dab9c5dbc1fa65e7f2da6c", // whispercpp-184 / v1.8.4
            },
            [WhisperCpp.WindowsCuBlas] = new[]
            {
                "63b70c91fe2fd7449865c45f6422ab628439eacc6985d8309c77bfb65cc68a19", // v1.8.6 (current download URL — fetched directly from ggml-org/whisper.cpp)
                "ff50101f85a6026d39053771c25b42f5752ac05d5be9ee2e5d2632541adef231", // v1.8.5
                "b07cff4e59831b227896018facbb6334907bf324a342c84597c44f087823d252", // v1.8.4
            },
            [WhisperCpp.WindowsVulkan] = new[]
            {
                "27a7e9612a930355e801d7ae45cd926b079bd215ce0527c219d7bd6a5acd4ada", // whispercpp-186 / v1.8.6 (current download URL)
                "8a993d86fbad6cfacf3123be615a692f17e9a19957ddfa6e071c751deaf8df42", // whispercpp-185 / v1.8.5
                "42f90548f551f525666b96bd734f4ee23e58f82dcc2a1b68c4eba5e453ba7080", // whispercpp-184 / v1.8.4
            },
            [WhisperCpp.MacOs] = new[]
            {
                "9e7fb79d310a17cf992baa883fe1acfec693d2e72aadace784a3b8ac77eb2768", // whispercpp-186 / v1.8.6 (current download URL)
                "49ef4acfaef0b4989885c258f22eb1355592c5f343897508899a2b598cd683bf", // whispercpp-185 / v1.8.5
                "81dbd530f21a6daf10b1c9cece61d7e56d774e3bcb3d21af4d17299caa532a4d", // whispercpp-184 / v1.8.4
            },
            [WhisperCpp.LinuxVulkan] = new[]
            {
                "10aed3a2b28e5ad40fee8267d554f0824943e50e75bfe7bceb7c16b6e3fe8a45", // whispercpp-186 / v1.8.6 (current download URL)
                "c385a01228f85764cfd9b6072e078d03e44f151d71ab145e912425e5cc9a7c8e", // whispercpp-185 / v1.8.5
                "7a7d131b5fbb605fef6a8d39b6f2480480d8a26ec8a1dbec3b3740485023158d", // whispercpp-184 / v1.8.4
            },
            [WhisperCpp.LinuxCuda] = new[]
            {
                "b8922f7fc25ff4f602c887655882c4114c005177017335f39181cc0417f0cb03", // whispercpp-186 / v1.8.6 (current download URL)
                "1aee5fddee30f8486275d3b2bd1d568e92615e7b1b063d46c635cb9f44d7d13b", // whispercpp-185 / v1.8.5
                "708ea1c502ac5082d4eb9afc86c8adb9d67d76da25e70fd81cb6fae2cfcf00ce", // whispercpp-184 / v1.8.4
            },

            // SHA-256 of whisper-cli / whisper-cli.exe extracted from each archive above.
            [WhisperCpp.WindowsBlasExecutable] = new[]
            {
                "c3ba7358316559cf80ae88e783daeb2f346d617d8074a0bd054998912cde979a", // whispercpp-186 / v1.8.6 (current download URL)
                "6a2e5cbd090c1dacc43461d1fac543e4b13881210f181c6d1e95267ec8c64c64", // whispercpp-185 / v1.8.5
                "80865086479dfedb0ce8d0c03f061629dd83e9d1e86b14343e5cf9fd01927098", // whispercpp-184 / v1.8.4
            },
            [WhisperCpp.WindowsCuBlasExecutable] = new[]
            {
                "ae283f6938fbe27aa12ad83bbfa0b4ca772dee21ffb54348eeb87c65eaf88b8a", // v1.8.6 (current download URL)
                "304eef3b9fc30b0b0d74f4ab756b6e5efe7b2f6f88813f79205631d1ebba448d", // v1.8.5
                "03947f51efb42abc82a0208cb0ead74822eff8e88a41df4c1f4536f6b78188ae", // v1.8.4
            },
            [WhisperCpp.WindowsVulkanExecutable] = new[]
            {
                "fe6afff595b1c3a08a129c2a9047e6b9d10107acd3964a29f54b5459f6795bd4", // whispercpp-186 / v1.8.6 (current download URL)
                "3d43e45f7b575dcc127d5a1c30bdb9f9f1650576007a31d955917027794672bd", // whispercpp-185 / v1.8.5
                "69a270fb42b98fc4927e6e9a79bda16ec7bf152825e2af68df09ff99f43479e6", // whispercpp-184 / v1.8.4
            },
            [WhisperCpp.MacOsExecutable] = new[]
            {
                "0b9f4894727ece186163e15e07795b70a333c5834e2916c6032ae04e57d3e1e8", // whispercpp-186 / v1.8.6 (current download URL)
                "0fd752e0384484eb3a72ce644135f20963879e80624b23f7f739eda187a23359", // whispercpp-185 / v1.8.5
                "11d902af004d1e79538f8f801b4a42ac3a21094370c9063f67d885d04dccdd96", // whispercpp-184 / v1.8.4
            },
        };

    /// <summary>
    /// Returns the update status of a hash relative to the known set for <paramref name="key"/>.
    /// Returns <see cref="UpdateStatus.Unknown"/> when the key or hash is not recognized,
    /// so callers will not prompt the user about updates for unfamiliar files.
    /// </summary>
    public static UpdateStatus GetStatus(string key, string? sha256Hash)
    {
        if (string.IsNullOrWhiteSpace(sha256Hash))
        {
            return UpdateStatus.Unknown;
        }

        if (!KnownHashes.TryGetValue(key, out var hashes) || hashes.Count == 0)
        {
            return UpdateStatus.Unknown;
        }

        if (hashes[0].Equals(sha256Hash, StringComparison.OrdinalIgnoreCase))
        {
            return UpdateStatus.UpToDate;
        }

        for (var i = 1; i < hashes.Count; i++)
        {
            if (hashes[i].Equals(sha256Hash, StringComparison.OrdinalIgnoreCase))
            {
                return UpdateStatus.UpdateAvailable;
            }
        }

        return UpdateStatus.Unknown;
    }

    /// <summary>
    /// Reads the <c>.installed.sha256</c> sidecar in <paramref name="installFolder"/> and returns
    /// the update status of the recorded build. Returns <see cref="UpdateStatus.Unknown"/> when the
    /// sidecar is missing or unreadable (e.g. installs predating hash tracking). Cheap - no file
    /// hashing - so it is safe to call while populating a combo box.
    /// </summary>
    public static UpdateStatus GetSidecarStatus(string installFolder)
    {
        try
        {
            var sidecar = Path.Combine(installFolder, ".installed.sha256");
            if (!File.Exists(sidecar))
            {
                return UpdateStatus.Unknown;
            }

            var lines = File.ReadAllLines(sidecar);
            if (lines.Length < 2)
            {
                return UpdateStatus.Unknown;
            }

            return GetStatus(lines[0].Trim(), lines[1].Trim());
        }
        catch
        {
            return UpdateStatus.Unknown;
        }
    }

    /// <summary>
    /// Returns the latest known SHA-256 hash for <paramref name="key"/>, or null if the key is unknown.
    /// </summary>
    public static string? GetLatestKnownHash(string key)
    {
        return KnownHashes.TryGetValue(key, out var hashes) && hashes.Count > 0
            ? hashes[0]
            : null;
    }

    /// <summary>
    /// Returns all known SHA-256 hashes for <paramref name="key"/> in newest-first order.
    /// </summary>
    public static IReadOnlyList<string> GetKnownHashes(string key)
    {
        return KnownHashes.TryGetValue(key, out var hashes)
            ? hashes
            : Array.Empty<string>();
    }

    /// <summary>
    /// Computes the lower-case hex SHA-256 of the file at <paramref name="filePath"/>.
    /// Returns null when the file does not exist.
    /// </summary>
    public static string? ComputeSha256(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return ComputeSha256(stream);
    }

    public static string ComputeSha256(Stream stream)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default)
    {
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Resolves the CrispASR hash key for the current OS and variant.
    /// On Windows the variant selects between cuda, vulkan, cpu, and cpu-legacy.
    /// On Linux x86_64 the variant selects between cuda and the default CPU build.
    /// On Linux ARM64 the variant is ignored (only one build).
    /// Returns null if the platform / variant combination is unknown.
    /// </summary>
    public static string? ResolveCrispAsrKey(string? variant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return CrispAsr.LinuxArm;
            }
            if (variant == "cuda")
            {
                return CrispAsr.LinuxCuda;
            }
            return CrispAsr.Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return CrispAsr.MacOs;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return variant switch
            {
                "cuda" => CrispAsr.WindowsCuda,
                "cpu" => CrispAsr.WindowsCpu,
                "cpu-legacy" => CrispAsr.WindowsCpuLegacy,
                "vulkan" => CrispAsr.WindowsVulkan,
                _ => null,
            };
        }

        return null;
    }

    /// <summary>
    /// Reverse of <see cref="ResolveCrispAsrKey"/> — returns the variant string matching the
    /// given hash key. For Windows: "cuda" / "vulkan" / "cpu" / "cpu-legacy". For Linux x86_64:
    /// "cuda" for the CUDA build, empty string for the default CPU build. Returns null otherwise.
    /// </summary>
    public static string? GetCrispAsrVariant(string key)
    {
        return key switch
        {
            CrispAsr.WindowsCuda or CrispAsr.WindowsCudaExecutable => "cuda",
            CrispAsr.WindowsVulkan or CrispAsr.WindowsVulkanExecutable => "vulkan",
            CrispAsr.WindowsCpu or CrispAsr.WindowsCpuExecutable => "cpu",
            CrispAsr.WindowsCpuLegacy or CrispAsr.WindowsCpuLegacyExecutable => "cpu-legacy",
            CrispAsr.LinuxCuda or CrispAsr.LinuxCudaExecutable => "cuda",
            CrispAsr.Linux or CrispAsr.LinuxExecutable => string.Empty,
            CrispAsr.LinuxArm or CrispAsr.LinuxArmExecutable => string.Empty,
            _ => null,
        };
    }

    /// <summary>
    /// Kept for backwards compatibility with callers that only handle Windows variants.
    /// Prefer <see cref="GetCrispAsrVariant"/> for new code.
    /// </summary>
    public static string? GetCrispAsrWindowsVariant(string key)
    {
        return key switch
        {
            CrispAsr.WindowsCuda or CrispAsr.WindowsCudaExecutable => "cuda",
            CrispAsr.WindowsVulkan or CrispAsr.WindowsVulkanExecutable => "vulkan",
            CrispAsr.WindowsCpu or CrispAsr.WindowsCpuExecutable => "cpu",
            CrispAsr.WindowsCpuLegacy or CrispAsr.WindowsCpuLegacyExecutable => "cpu-legacy",
            _ => null,
        };
    }

    /// <summary>
    /// Resolves the CrispASR executable-hash key for the current OS and variant.
    /// Used as a fallback when no sidecar hash exists alongside the install.
    /// </summary>
    public static string? ResolveCrispAsrExecutableKey(string? variant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return CrispAsr.LinuxArmExecutable;
            }
            if (variant == "cuda")
            {
                return CrispAsr.LinuxCudaExecutable;
            }
            return CrispAsr.LinuxExecutable;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return CrispAsr.MacOsExecutable;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return variant switch
            {
                "cuda" => CrispAsr.WindowsCudaExecutable,
                "vulkan" => CrispAsr.WindowsVulkanExecutable,
                "cpu" => CrispAsr.WindowsCpuExecutable,
                "cpu-legacy" => CrispAsr.WindowsCpuLegacyExecutable,
                _ => null,
            };
        }

        return null;
    }

    /// <summary>
    /// Detects which Windows CrispASR variant is installed.
    /// Returns "cuda" / "vulkan" / "cpu" / "cpu-legacy", or null if the folder doesn't look like a
    /// CrispASR install. CUDA and Vulkan are identified by their backend DLLs; CPU vs CPU-legacy
    /// is decided by sidecar or executable-hash match (CPU-legacy is the AVX2-less fallback).
    /// </summary>
    public static string? DetectCrispAsrWindowsVariant(string installFolder)
    {
        if (string.IsNullOrEmpty(installFolder) || !Directory.Exists(installFolder))
        {
            return null;
        }

        if (File.Exists(Path.Combine(installFolder, "ggml-cuda.dll")))
        {
            return "cuda";
        }

        if (File.Exists(Path.Combine(installFolder, "ggml-vulkan.dll")))
        {
            return "vulkan";
        }

        var exe = Path.Combine(installFolder, "crispasr.exe");
        if (!File.Exists(exe))
        {
            return null;
        }

        var sidecarKey = TryReadInstalledKey(installFolder);
        if (sidecarKey == CrispAsr.WindowsCpuLegacy)
        {
            return "cpu-legacy";
        }
        if (sidecarKey == CrispAsr.WindowsCpu)
        {
            return "cpu";
        }

        // No usable sidecar — match the EXE against known CPU-legacy hashes so older
        // installs (made before the sidecar existed) are still recognised as legacy.
        var exeHash = ComputeSha256(exe);
        if (exeHash != null)
        {
            foreach (var legacy in GetKnownHashes(CrispAsr.WindowsCpuLegacyExecutable))
            {
                if (legacy.Equals(exeHash, StringComparison.OrdinalIgnoreCase))
                {
                    return "cpu-legacy";
                }
            }
        }

        return "cpu";
    }

    /// <summary>
    /// Detects which Linux CrispASR variant is installed.
    /// Returns "cuda" for the statically-linked CUDA build, or null for the default CPU build.
    /// The Linux CUDA archive ships only crispasr + crispasr-quantize with CUDA libs linked in,
    /// so there is no backend .so to detect by filename — we use the .installed.sha256 sidecar,
    /// falling back to matching the crispasr binary against the known CUDA executable hashes.
    /// </summary>
    public static string? DetectCrispAsrLinuxVariant(string installFolder)
    {
        if (string.IsNullOrEmpty(installFolder) || !Directory.Exists(installFolder))
        {
            return null;
        }

        var sidecarKey = TryReadInstalledKey(installFolder);
        if (sidecarKey == CrispAsr.LinuxCuda)
        {
            return "cuda";
        }
        if (sidecarKey == CrispAsr.Linux)
        {
            return null;
        }

        var exe = Path.Combine(installFolder, "crispasr");
        if (!File.Exists(exe))
        {
            return null;
        }

        var exeHash = ComputeSha256(exe);
        if (exeHash != null)
        {
            foreach (var cuda in GetKnownHashes(CrispAsr.LinuxCudaExecutable))
            {
                if (cuda.Equals(exeHash, StringComparison.OrdinalIgnoreCase))
                {
                    return "cuda";
                }
            }
        }

        return null;
    }

    private static string? TryReadInstalledKey(string installFolder)
    {
        try
        {
            var sidecar = Path.Combine(installFolder, ".installed.sha256");
            if (!File.Exists(sidecar))
            {
                return null;
            }
            var firstLine = File.ReadLines(sidecar).FirstOrDefault();
            return string.IsNullOrWhiteSpace(firstLine) ? null : firstLine.Trim();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves the WhisperCpp archive hash key for the given engine choice on the current OS.
    /// <paramref name="whisperChoice"/> is one of the <c>WhisperChoice</c> constants (Cpp / CppCuBlas / CppVulkan).
    /// Returns null when the platform / choice combination is unsupported or unknown.
    /// </summary>
    public static string? ResolveWhisperCppKey(string? whisperChoice)
    {
        if (string.IsNullOrEmpty(whisperChoice))
        {
            return null;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return whisperChoice switch
            {
                WhisperChoice.Cpp => WhisperCpp.WindowsBlas,
                WhisperChoice.CppCuBlas => WhisperCpp.WindowsCuBlas,
                WhisperChoice.CppVulkan => WhisperCpp.WindowsVulkan,
                _ => null,
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return whisperChoice == WhisperChoice.Cpp ? WhisperCpp.MacOs : null;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return whisperChoice switch
            {
                WhisperChoice.Cpp => WhisperCpp.LinuxVulkan,        // SE-packaged Linux build for the default Cpp backend uses Vulkan.
                WhisperChoice.CppCuBlas => WhisperCpp.LinuxCuda,
                _ => null,
            };
        }

        return null;
    }

    /// <summary>
    /// Resolves the WhisperCpp executable-hash key for the given engine choice on the current OS.
    /// Used as a fallback when no sidecar hash exists alongside the install.
    /// Returns null on Linux: the same whisper-cli binary ships in both the Vulkan and CUDA archives,
    /// so executable hashing cannot disambiguate.
    /// </summary>
    public static string? ResolveWhisperCppExecutableKey(string? whisperChoice)
    {
        if (string.IsNullOrEmpty(whisperChoice))
        {
            return null;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return whisperChoice switch
            {
                WhisperChoice.Cpp => WhisperCpp.WindowsBlasExecutable,
                WhisperChoice.CppCuBlas => WhisperCpp.WindowsCuBlasExecutable,
                WhisperChoice.CppVulkan => WhisperCpp.WindowsVulkanExecutable,
                _ => null,
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return whisperChoice == WhisperChoice.Cpp ? WhisperCpp.MacOsExecutable : null;
        }

        return null;
    }

    /// <summary>
    /// Resolves the llama.cpp archive hash key for the current OS, architecture and (Windows/Linux)
    /// variant ("cpu" / "vulkan" / "cuda"). Returns null when the combination is unknown.
    /// </summary>
    public static string? ResolveLlamaCppKey(string? variant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return variant switch
            {
                "cuda" => LlamaCpp.WindowsCuda,
                "vulkan" => LlamaCpp.WindowsVulkan,
                "cpu" => LlamaCpp.WindowsCpu,
                _ => null,
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return variant == "vulkan" ? LlamaCpp.LinuxArm64Vulkan : LlamaCpp.LinuxArm64Cpu;
            }

            return variant == "vulkan" ? LlamaCpp.LinuxVulkan : LlamaCpp.LinuxCpu;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? LlamaCpp.MacOsArm64
                : LlamaCpp.MacOsX64;
        }

        return null;
    }

    /// <summary>
    /// Reverse of <see cref="ResolveLlamaCppKey"/> for Windows variants only:
    /// returns "cuda" / "vulkan" / "cpu" matching the given archive key, or null otherwise.
    /// </summary>
    public static string? GetLlamaCppWindowsVariant(string key)
    {
        return key switch
        {
            LlamaCpp.WindowsCuda => "cuda",
            LlamaCpp.WindowsVulkan => "vulkan",
            LlamaCpp.WindowsCpu => "cpu",
            _ => null,
        };
    }

    /// <summary>
    /// Resolves the llama.cpp executable-hash key for the current OS and architecture.
    /// Used as a fallback when no sidecar hash exists alongside the install. The Windows
    /// CPU/Vulkan/CUDA builds (and the two Linux builds) share an identical llama-server
    /// binary, so there is a single key per OS, plus per-arch on macOS.
    /// </summary>
    public static string? ResolveLlamaCppExecutableKey()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return LlamaCpp.WindowsExecutable;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? LlamaCpp.LinuxArm64Executable
                : LlamaCpp.LinuxExecutable;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? LlamaCpp.MacOsArm64Executable
                : LlamaCpp.MacOsX64Executable;
        }

        return null;
    }

    /// <summary>
    /// Resolves the OmniVoice archive hash key for the current OS, architecture and
    /// (Windows-only) variant ("cpu" / "vulkan" / "cuda"). Returns null when the
    /// combination is unknown.
    /// </summary>
    public static string? ResolveOmniVoiceKey(string? windowsVariant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return windowsVariant switch
            {
                "cuda" => OmniVoice.WindowsCuda,
                "vulkan" => OmniVoice.WindowsVulkan,
                "cpu" => OmniVoice.WindowsCpu,
                _ => OmniVoice.WindowsVulkan, // Vulkan is the recommended default on Windows.
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OmniVoice.MacOs;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? OmniVoice.LinuxArm64
                : OmniVoice.LinuxX64;
        }

        return null;
    }

    /// <summary>
    /// Resolves the Qwen3 TTS archive hash key for the current OS, architecture and
    /// (Windows-only) variant ("cpu" / "vulkan" / "cuda"). Returns null when the combination is unknown.
    /// </summary>
    public static string? ResolveQwen3TtsCppKey(string? windowsVariant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return windowsVariant switch
            {
                "cpu" => Qwen3TtsCpp.WindowsCpu,
                "vulkan" => Qwen3TtsCpp.WindowsVulkan,
                "cuda" => Qwen3TtsCpp.WindowsCuda,
                _ => Qwen3TtsCpp.WindowsVulkan, // Vulkan is the recommended default on Windows.
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Qwen3TtsCpp.MacOs;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? Qwen3TtsCpp.LinuxArm64
                : Qwen3TtsCpp.LinuxX64;
        }

        return null;
    }

    /// <summary>
    /// Resolves the Kokoro TTS archive hash key for the current OS and architecture.
    /// Returns null when the platform is unsupported.
    /// </summary>
    public static string? ResolveKokoroTtsCppKey()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return KokoroTtsCpp.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return KokoroTtsCpp.MacOs;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? KokoroTtsCpp.LinuxArm64
                : KokoroTtsCpp.LinuxX64;
        }

        return null;
    }

    /// <summary>
    /// Resolves the <see cref="Piper"/> archive key for the current OS/arch, matching the URL
    /// <c>TtsDownloadService</c> downloads. Returns null on an unsupported platform.
    /// </summary>
    public static string? ResolvePiperKey()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Piper.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Piper.MacOs;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? Piper.LinuxArm64
                : Piper.LinuxX64;
        }

        return null;
    }

    /// <summary>
    /// Detects which Windows llama.cpp variant is installed from its backend DLLs.
    /// Returns "cuda" / "vulkan" / "cpu", or null if the folder doesn't look like a llama.cpp install.
    /// </summary>
    public static string? DetectLlamaCppWindowsVariant(string installFolder)
    {
        if (string.IsNullOrEmpty(installFolder) || !Directory.Exists(installFolder))
        {
            return null;
        }

        if (!File.Exists(Path.Combine(installFolder, "llama-server.exe")))
        {
            return null;
        }

        if (File.Exists(Path.Combine(installFolder, "ggml-cuda.dll")))
        {
            return "cuda";
        }

        if (File.Exists(Path.Combine(installFolder, "ggml-vulkan.dll")))
        {
            return "vulkan";
        }

        return "cpu";
    }
}
