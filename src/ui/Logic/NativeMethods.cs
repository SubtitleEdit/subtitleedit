using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic
{
    internal static class NativeMethods
    {

        #region Hunspell

        [DllImport("libhunspell", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr Hunspell_create(string affpath, string dpath);

        [DllImport("libhunspell")]
        internal static extern IntPtr Hunspell_destroy(IntPtr hunspellHandle);

        [DllImport("libhunspell", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern int Hunspell_spell(IntPtr hunspellHandle, string word);

        [DllImport("libhunspell", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern int Hunspell_suggest(IntPtr hunspellHandle, IntPtr slst, string word);

        [DllImport("libhunspell")]
        internal static extern void Hunspell_free_list(IntPtr hunspellHandle, IntPtr slst, int n);

        #endregion Hunspell

        #region Win32 API

        // Win32 API functions for dynamically loading DLLs
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AttachConsole(int dwProcessId);
        internal const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeConsole();

        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int vKey);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        internal static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int width, int height, int wFlags);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        internal const int WM_SETREDRAW = 0x0b;

        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        #endregion Win32 API

        #region VLC

        // LibVLC Core - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__core.html
        [DllImport("libvlc")]
        internal static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray)] string[] argv);

        [DllImport("libvlc")]
        internal static extern void libvlc_release(IntPtr libVlc);

        // LibVLC Media - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media.html
        [DllImport("libvlc")]
        internal static extern IntPtr libvlc_media_new_path(IntPtr instance, byte[] input);

        [DllImport("libvlc")]
        internal static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_release(IntPtr media);

        // LibVLC Audio Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__audio.html
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

        // LibVLC media player - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media__player.html
        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_play(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_stop(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_pause(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_set_hwnd(IntPtr mediaPlayer, IntPtr windowsHandle);

        [DllImport("libvlc")]
        internal static extern Int64 libvlc_media_player_get_time(IntPtr mediaPlayer);

        [DllImport("libvlc")]
        internal static extern void libvlc_media_player_set_time(IntPtr mediaPlayer, Int64 position);

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

        #endregion VLC

        #region MPV
        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mpv_create();


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mpv_initialize(IntPtr mpvHandle);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mpv_command(IntPtr mpvHandle, IntPtr utf8Strings);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mpv_terminate_destroy(IntPtr mpvHandle);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mpv_wait_event(IntPtr mpvHandle, double wait);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mpv_set_option(IntPtr mpvHandle, byte[] name, int format, ref long data);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mpv_set_option_string(IntPtr mpvHandle, byte[] name, byte[] value);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mpv_get_property_string(IntPtr mpvHandle, byte[] name);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mpv_get_property(IntPtr mpvHandle, byte[] name, int format, ref double data);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mpv_set_property(IntPtr mpvHandle, byte[] name, int format, ref byte[] data);


        [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mpv_free(IntPtr data);

        #endregion MPV

        #region Linux System

        internal const int LC_NUMERIC = 1;

        internal const int RTLD_NOW = 0x0001;
        internal const int RTLD_GLOBAL = 0x0100;

        [DllImport("libc.so.6")]
        internal static extern IntPtr setlocale(int category, string locale);

        [DllImport("libdl.so.2")]
        internal static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so.2")]
        internal static extern IntPtr dlclose(IntPtr handle);

        [DllImport("libdl.so.2")]
        internal static extern IntPtr dlsym(IntPtr handle, string symbol);

        #endregion

        #region Cross platform

        internal static IntPtr CrossLoadLibrary(string fileName)
        {
            if (Configuration.IsRunningOnWindows)
            {
                return LoadLibrary(fileName);
            }

            return dlopen(fileName, RTLD_NOW | RTLD_GLOBAL);
        }

        internal static void CrossFreeLibrary(IntPtr handle)
        {
            if (Configuration.IsRunningOnWindows)
            {
                FreeLibrary(handle);
            }
            else
            {
                dlclose(handle);
            }
        }

        internal static IntPtr CrossGetProcAddress(IntPtr handle, string name)
        {
            if (Configuration.IsRunningOnWindows)
            {
                return GetProcAddress(handle, name);
            }
            return dlsym(handle, name);
        }

        #endregion
    }
}
