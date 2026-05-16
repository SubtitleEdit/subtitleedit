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

        // Hashes of the unpacked main executable (crispasr.exe / crispasr) — used to detect the
        // installed version when no sidecar is present (e.g. installs from older SE builds).
        public const string WindowsCudaExecutable = "CrispAsr.Windows.Cuda.Executable";
        public const string WindowsVulkanExecutable = "CrispAsr.Windows.Vulkan.Executable";
        public const string WindowsCpuExecutable = "CrispAsr.Windows.Cpu.Executable";
        public const string WindowsCpuLegacyExecutable = "CrispAsr.Windows.CpuLegacy.Executable";
        public const string MacOsExecutable = "CrispAsr.MacOs.Executable";
        public const string LinuxExecutable = "CrispAsr.Linux.Executable";
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
                "be610e9a8bb283cc283dc3d0df45b5f110ccb350a13443e9b8e4092345d78596", // v0.6.6 (current download URL)
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
                "3fda38ec66b75eca1d9145787ace497a4ca56ce2d6c218773f925f458790622d", // v0.6.6 (current download URL)
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
                "05f629c4d022fb8a05a24b16cb155c45ab65e90dc0aa7eed46ae31feccf43de8", // v0.6.6 (current download URL)
                "4b36e5634c1acc7f7387c9bce3b1302e8fbd8441b3e10d37b5d5952064bbc552", // v0.6.2
                "46fe3bc88966c973eef66b7c2271f95bb40b2b4bf338643e71834186cba0ae3d", // v0.6.0 (first non-legacy CPU build SE has shipped)
            },
            [CrispAsr.WindowsCpuLegacy] = new[]
            {
                "d9fd9306246cda7b4b3006441aad8ba755d617b066f6f033358b51c860d28f89", // v0.6.6 (current download URL)
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
                "32dab6eb5f2be8150f3f66131dfdd09da5f7a2682ff1e594794bcbf51b5a3c91", // v0.6.6 (current download URL)
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
                "2ac612bada388345c2dd9580353e2401e07924cb3fbe37c0cae6c5564b51068e", // v0.6.6 (current download URL)
                "52a9b6791ef23c73b0448475712611cd97bbd8598ee0101e7f28ff3e8ba5906b", // v0.6.2
                "f63aba89b6371128d0cd11167e280d1b987853996c4110de9ce48dabe55b20f6", // v0.6.0
                "9981f330a96715bdd232a08ca6d485305e7e0e95c1eb1b51ffac68424f14d311", // v0.5.7
                "b7cd7b95180e85bb5f76d3b0c22c2ab8dfabc616df8baf2d918f215736131134", // v0.5.6
                "ee476a912b5525874a70c2dc52604915c97672104c8c0a207e8a9c6ebbbe1f37", // v0.5.5
                "ea751eaedcc5dc2a5772f9e3f1fd8aae87314d501b07fa6d4acd03bacf8e7ecb", // v0.5.4
                "74c96662f49b4ae4640d12f152cc892dad2f21c354810a4bc38a630dc0da8195", // v0.5.3
                "4763b6f92f4813da7380a8da82eb1b234189c1c67e8cacb119a5d115f041ec30", // v0.5.2
            },

            // SHA-256 of crispasr.exe / crispasr extracted from each archive above.
            // CUDA hashes intentionally omitted (the CUDA archive is ~700 MB; not yet hashed).
            [CrispAsr.WindowsVulkanExecutable] = new[]
            {
                "fc9a592eb6cfa41d74f165e65cabe607712a06d546ba77bc3e3a93823ac9d3f3", // v0.6.6 (current download URL)
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
                "69f01b93054cf0c359eaa31f80f6ff06452f52de5306588aa2c743c0a5a6c4f5", // v0.6.6 (current download URL)
                "b63eb3a28ae7da8c0fcb486fa0f460a07ad85ace545359a338fc053bdea69dbf", // v0.6.2
                "28da4dcf6e72738b408400ebdef00203f8e70dccf392446ecee90a15c971e186", // v0.6.0 (first non-legacy CPU build SE has shipped)
            },
            [CrispAsr.WindowsCpuLegacyExecutable] = new[]
            {
                "352483e379cdeff5d42d2cd4f95c3c41f73f451986b26ee7a65843281cf4153c", // v0.6.6 (current download URL)
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
                "31e955f45739471c5d1464d8e8c484704124715e13c944c27b6d47db6857ef06", // v0.6.6 (current download URL)
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
                "abfb92985fed2b69e25473b7dd6a39b637f7e3025d7b379077a762d7cf8df8de", // v0.6.6 (current download URL)
                "fc60809052f50622663ae4912088f9f547023e4b6d528e902aa0e1a6619fe387", // v0.6.2
                "63b6a4197e74f5ddbd873f7db1e9e962fddd4a35b0fbc42eb7c898b5c9c1d964", // v0.6.0
                "96e3db2930ab3b46687711ee3744499c186d5e9eff1858e10137fbdf3b4a3614", // v0.5.7
                "34c9d121d96113015b2e2ba75faf268afa123d9bc847f32e819e3bc987beec47", // v0.5.6
                "380239b91448bcc2ce95ebb2179f3b8f5c4168d2f30f1a2c43e5f4d81d2bc79d", // v0.5.5
                "7ea6e45b16f396c5cfee8447b0245e872399e504b6dc3d8bb81c3a3c262acbb5", // v0.5.4
                "a92723269e7e16b93184aadadef3868396e75994665066b817218a0d208c1d2e", // v0.5.3
                "b99c7d6f51652f7bcddfb6b5bd73f11c541f9947256f040758a20bd1c7ad6591", // v0.5.2
            },

            // llama.cpp — https://github.com/ggml-org/llama.cpp/releases
            // Index 0 must match whatever version LlamaCppDownloadService.cs is pinned to,
            // otherwise users will be prompted to "update" to the same version they just got.
            [LlamaCpp.WindowsCpu] = new[]
            {
                "15739e9a11aa587d8232d2bc473a7dd7b3ac7f2f2384276309bc91474084ab1e", // b9174 (current download URL)
                "8e2266646ede106a112023340adc25a2c2150243826e9dfe4bd9f7ed30f8e042", // b9145
            },
            [LlamaCpp.WindowsVulkan] = new[]
            {
                "120b9c822f1c55e7e6f05d77743ebfb53a8c622fa8d34fc57847fdb9f628cb7a", // b9174 (current download URL)
                "5b54c452458bf6ad06d030a18458c484848b9def4d56b7b94171ff464e670d66", // b9145
            },
            [LlamaCpp.WindowsCuda] = new[]
            {
                "edbdcb670f04f6fac0da35cffeff1fa9ae22319cce327074d19b39f98b6c4838", // b9174 (current download URL)
                "a323ccfa87aaa31c47d62be75181799f16f2fe4636786c09bd8beeb6ac722266", // b9145
            },
            [LlamaCpp.LinuxCpu] = new[]
            {
                "94325e296a4561cb95161321b546685a6821513b4fab1a7cb28f76d4003c56d1", // b9174 (current download URL)
                "4cb45bdbf358bc15c77acb84e83e311a3f2c9247b9980f4f1886ebe3bd7e46d0", // b9145
            },
            [LlamaCpp.LinuxVulkan] = new[]
            {
                "d5787fa775220b3c0f896947885fa0f3dedddbb5388bd68603376a2a97252e72", // b9174 (current download URL)
                "b4a28b06d973f662b9465b6e0e786d8991cdab4d24101f5d88b0b8f688a97aea", // b9145
            },
            [LlamaCpp.LinuxArm64Cpu] = new[]
            {
                "7ad708abbb58a6dd5a148eed24d9d2ccf155f57cde8cf3c0e44f73d0d2b95c6d", // b9174 (current download URL)
            },
            [LlamaCpp.LinuxArm64Vulkan] = new[]
            {
                "84adac128b781f495804fb84d0c27d27e21cde6da03338b9ce0931ee324304f2", // b9174 (current download URL)
            },
            [LlamaCpp.MacOsArm64] = new[]
            {
                "39406deaac6083000446530e68194b77bb64c64ecf0533040a49d8e7cfceb363", // b9174 (current download URL)
                "3c0cef9bcf8898c3bd169e94fa2acec249b8ebc94d8fade2c165b6d6a01e2693", // b9145
            },
            [LlamaCpp.MacOsX64] = new[]
            {
                "f4040d729027dbc4352023ed32cbc5157a12a740223ec0d93d42032817554c6b", // b9174 (current download URL)
                "0391b2dfe4a4f384916ebf98c33bfe1205443cde8bffec7563b73671d3060149", // b9145
            },

            // SHA-256 of llama-server / llama-server.exe extracted from each archive above.
            [LlamaCpp.WindowsExecutable] = new[]
            {
                "569a751e9e95c8b52683ce640120997fe684d2b629e40552cf8945032a082569", // b9174 (current download URL)
                "528ff8471ff7072ac75494a80467a0fced073ec11dcbf96a0e1c3ec0334b9dd0", // b9145
            },
            [LlamaCpp.LinuxExecutable] = new[]
            {
                "fc6322995eafb9146db0454acb2b2d3f14244fa3eecb62ba742c129f8b4f5de1", // b9174 (current download URL)
                "855bd9540073d9f020b9b77ad5866ea3ea74ca570ea3fac486da8676f0eb6933", // b9145
            },
            [LlamaCpp.LinuxArm64Executable] = new[]
            {
                "a39b9d8200591e3e16f14b933f97ba0856e464097e0ba79cd3fb101313bdde96", // b9174 (current download URL)
            },
            [LlamaCpp.MacOsArm64Executable] = new[]
            {
                "1ebdcadd5939620f1cdd9e5d2d6c7ba2902e85301f0f2ba15f51cb5beb8b4fcc", // b9174 (current download URL)
                "61b18501af97dce62cb7e1c019981734d7b0e1003122079ac212edaeb86a36e4", // b9145
            },
            [LlamaCpp.MacOsX64Executable] = new[]
            {
                "a63265bf5136200b2a8c2f86404151975090a58dae24df934a6054b52348aa1c", // b9174 (current download URL)
                "c33c8f47da7d7751cd07c9ab32e77c84b3f5ac56b56299016f62b2d184fe3dfb", // b9145
            },

            // whisper.cpp — https://github.com/ggml-org/whisper.cpp/releases (and SE-repackaged builds
            // under https://github.com/SubtitleEdit/support-files/releases). Index 0 must match
            // whatever URL WhisperDownloadService.cs is pinned to.
            [WhisperCpp.WindowsBlas] = new[]
            {
                "5f8d6b1bcdf86edf898d21379468ae8329bb783803dab9c5dbc1fa65e7f2da6c", // whispercpp-184 / v1.8.4 (current download URL)
            },
            [WhisperCpp.WindowsCuBlas] = new[]
            {
                "b07cff4e59831b227896018facbb6334907bf324a342c84597c44f087823d252", // v1.8.4 (current download URL — fetched directly from ggml-org/whisper.cpp)
            },
            [WhisperCpp.WindowsVulkan] = new[]
            {
                "42f90548f551f525666b96bd734f4ee23e58f82dcc2a1b68c4eba5e453ba7080", // whispercpp-184 / v1.8.4 (current download URL)
            },
            [WhisperCpp.MacOs] = new[]
            {
                "81dbd530f21a6daf10b1c9cece61d7e56d774e3bcb3d21af4d17299caa532a4d", // whispercpp-184 / v1.8.4 (current download URL)
            },
            [WhisperCpp.LinuxVulkan] = new[]
            {
                "7a7d131b5fbb605fef6a8d39b6f2480480d8a26ec8a1dbec3b3740485023158d", // whispercpp-184 / v1.8.4 (current download URL)
            },
            [WhisperCpp.LinuxCuda] = new[]
            {
                "708ea1c502ac5082d4eb9afc86c8adb9d67d76da25e70fd81cb6fae2cfcf00ce", // whispercpp-184 / v1.8.4 (current download URL)
            },

            // SHA-256 of whisper-cli / whisper-cli.exe extracted from each archive above.
            [WhisperCpp.WindowsBlasExecutable] = new[]
            {
                "80865086479dfedb0ce8d0c03f061629dd83e9d1e86b14343e5cf9fd01927098", // whispercpp-184 / v1.8.4 (current download URL)
            },
            [WhisperCpp.WindowsCuBlasExecutable] = new[]
            {
                "03947f51efb42abc82a0208cb0ead74822eff8e88a41df4c1f4536f6b78188ae", // v1.8.4 (current download URL)
            },
            [WhisperCpp.WindowsVulkanExecutable] = new[]
            {
                "69a270fb42b98fc4927e6e9a79bda16ec7bf152825e2af68df09ff99f43479e6", // whispercpp-184 / v1.8.4 (current download URL)
            },
            [WhisperCpp.MacOsExecutable] = new[]
            {
                "11d902af004d1e79538f8f801b4a42ac3a21094370c9063f67d885d04dccdd96", // whispercpp-184 / v1.8.4 (current download URL)
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
    /// Resolves the CrispASR hash key for the current OS and (Windows-only) variant.
    /// Returns null if the platform / variant combination is unknown.
    /// </summary>
    public static string? ResolveCrispAsrKey(string? windowsVariant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return CrispAsr.Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return CrispAsr.MacOs;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return windowsVariant switch
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
    /// Reverse of <see cref="ResolveCrispAsrKey"/> for Windows variants only:
    /// returns "cuda" / "vulkan" / "cpu" / "cpu-legacy" matching the given key, or null otherwise.
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
    /// Resolves the CrispASR executable-hash key for the current OS and (Windows-only) variant.
    /// Used as a fallback when no sidecar hash exists alongside the install.
    /// </summary>
    public static string? ResolveCrispAsrExecutableKey(string? windowsVariant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return CrispAsr.LinuxExecutable;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return CrispAsr.MacOsExecutable;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return windowsVariant switch
            {
                "cuda" => CrispAsr.WindowsCudaExecutable,
                "cpu" => CrispAsr.WindowsCpuExecutable,
                "cpu-legacy" => CrispAsr.WindowsCpuLegacyExecutable,
                "vulkan" => CrispAsr.WindowsVulkanExecutable,
                _ => null,
            };
        }

        return null;
    }

    /// <summary>
    /// Detects which Windows CrispASR variant is installed.
    /// Returns "cuda" / "vulkan" / "cpu" / "cpu-legacy", or null if the folder doesn't look like a CrispASR install.
    /// CUDA and Vulkan are identified by their backend DLLs. CPU vs CPU-legacy share an identical layout
    /// (only crispasr.exe at the top level), so we disambiguate from the .installed.sha256 sidecar
    /// recorded at install time, falling back to hashing crispasr.exe against the known CPU-legacy set.
    /// Unknown CPU EXE hashes default to "cpu" (the modern build).
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
