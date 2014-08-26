using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    internal static class NativeMethods
    {

        #region Hunspell
        [DllImport("libhunspell")]
        internal static extern IntPtr Hunspell_create(string affpath, string dpath);

        [DllImport("libhunspell")]
        internal static extern IntPtr Hunspell_destroy(IntPtr hunspellHandle);

        [DllImport("libhunspell")]
        internal static extern int Hunspell_spell(IntPtr hunspellHandle, string word);

        [DllImport("libhunspell")]
        internal static extern int Hunspell_suggest(IntPtr hunspellHandle, IntPtr slst, string word);

        [DllImport("libhunspell")]
        internal static extern void Hunspell_free_list(IntPtr hunspellHandle, IntPtr slst, int n);
        #endregion 


        #region structures

        /// <summary>
        ///   The processo r_ architecture.
        /// </summary>
        internal enum PROCESSOR_ARCHITECTURE : ushort
        {
            /// <summary>
            ///   The intel.
            /// </summary>
            Intel = 0,

            /// <summary>
            ///   The mips.
            /// </summary>
            MIPS = 1,

            /// <summary>
            ///   The alpha.
            /// </summary>
            Alpha = 2,

            /// <summary>
            ///   The ppc.
            /// </summary>
            PPC = 3,

            /// <summary>
            ///   The shx.
            /// </summary>
            SHX = 4,

            /// <summary>
            ///   The arm.
            /// </summary>
            ARM = 5,

            /// <summary>
            ///   The i a 64.
            /// </summary>
            IA64 = 6,

            /// <summary>
            ///   The alpha 64.
            /// </summary>
            Alpha64 = 7,

            /// <summary>
            ///   The amd 64.
            /// </summary>
            Amd64 = 9,

            /// <summary>
            ///   The unknown.
            /// </summary>
            Unknown = 0xFFFF
        }

        /// <summary>
        ///   The syste m_ info.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            /// <summary>
            ///   The w processor architecture.
            /// </summary>
            internal PROCESSOR_ARCHITECTURE wProcessorArchitecture;

            /// <summary>
            ///   The w reserved.
            /// </summary>
            internal ushort wReserved;

            /// <summary>
            ///   The dw page size.
            /// </summary>
            internal uint dwPageSize;

            /// <summary>
            ///   The lp minimum application address.
            /// </summary>
            internal IntPtr lpMinimumApplicationAddress;

            /// <summary>
            ///   The lp maximum application address.
            /// </summary>
            internal IntPtr lpMaximumApplicationAddress;

            /// <summary>
            ///   The dw active processor mask.
            /// </summary>
            internal IntPtr dwActiveProcessorMask;

            /// <summary>
            ///   The dw number of processors.
            /// </summary>
            internal uint dwNumberOfProcessors;

            /// <summary>
            ///   The dw processor type.
            /// </summary>
            internal uint dwProcessorType;

            /// <summary>
            ///   The dw allocation granularity.
            /// </summary>
            internal uint dwAllocationGranularity;

            /// <summary>
            ///   The dw processor level.
            /// </summary>
            internal ushort dwProcessorLevel;

            /// <summary>
            ///   The dw processor revision.
            /// </summary>
            internal ushort dwProcessorRevision;
        }      
        #endregion

        #region Win32 API
        // Win32 API functions for loading dlls dynamic
        [DllImport("kernel32.dll")]
        internal static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int vKey);

        [DllImport("kernel32.dll")]
        internal static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        internal static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        internal static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int width, int height, int wFlags);
        #endregion


        // LibVLC Core - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__core.html
        [DllImport("libvlc")]
        internal static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray)] string[] argv);

        [DllImport("libvlc")]
        internal static extern IntPtr libvlc_get_version();

        [DllImport("libvlc")]
        internal static extern void libvlc_release(IntPtr libVlc);

        // LibVLC Media - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media.html
        [DllImport("libvlc")]
        internal static extern IntPtr libvlc_media_new_path(IntPtr instance, byte[] input);

        [DllImport("libvlc")]
        internal static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_release(IntPtr media);


        // LibVLC Video Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__video.html#g8f55326b8b51aecb59d8b8a446c3f118
        [DllImport("libvlc")]
        internal static extern void libvlc_video_get_size(IntPtr mediaPlayer, UInt32 number, out UInt32 x, out UInt32 y);

        [DllImport("libvlc")]
        internal static extern int libvlc_audio_get_track_count(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern int libvlc_audio_get_track(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern int libvlc_audio_set_track(IntPtr mediaPlayer, int trackNumber);

        // LibVLC Audio Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__audio.html
        [DllImport("libvlc")]
        internal static extern int libvlc_audio_get_volume(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_audio_set_volume(IntPtr mediaPlayer, int volume);


        // LibVLC Media Player - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media__player.html
        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_play(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_stop(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_pause(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_set_hwnd(IntPtr mediaPlayer, IntPtr windowsHandle);

        [DllImport("libvlc")]
        internal static extern int libvlc_media_player_is_playing(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern Int64 libvlc_media_player_get_time(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_set_time(IntPtr mediaPlayer, Int64 position);

        [DllImport("libvlc")]
        internal static extern float libvlc_media_player_get_fps(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern byte libvlc_media_player_get_state(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern Int64 libvlc_media_player_get_length(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_list_player_release(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern float libvlc_media_player_get_rate(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern int libvlc_media_player_set_rate(IntPtr mediaPlayer, float rate);

    }

}
