using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.Ocr.Tesseract
{
    public static class Tesseract5
    {
        [DllImport("TesseractApi.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void tesseractApiCreate();
        [DllImport("TesseractApi.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool tesseractApiInit(string dataPath, string language);
        [DllImport("TesseractApi.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void tesseractApiSetImageData(IntPtr Data, int width, int height, int bits, int verticalResolution, int horizontalResolution);
        [DllImport("TesseractApi.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern string tesseractApiGetHOCR(int page);
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        public static bool createComplete = false;
        public static string initializedLanguage;

        public static void Create()
        {
            IntPtr loaded = GetModuleHandle("TesseractApi.dll");
            if (loaded == IntPtr.Zero)
            {
                SetDllDirectory(Configuration.TesseractDirectory);
            }
            tesseractApiCreate();
            createComplete = true;
        }

        public static bool Init(string dataPath, string language)
        {
            initializedLanguage = language;
            return tesseractApiInit(dataPath, language);
        }

        public static IntPtr imageData;

        public static TesseractLock tlock = new TesseractLock();

        public static void SetImage(System.Drawing.Image image)
        {
            Marshal.FreeHGlobal(imageData); //free previous image global buffer
            Bitmap bitmap = (Bitmap)image;
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            imageData = Marshal.AllocHGlobal(data.Stride * data.Height); //initialize global buffer for current image

            memcpy(imageData, data.Scan0, (UIntPtr)(data.Stride * data.Height));
            bitmap.UnlockBits(data);

            tesseractApiSetImageData(imageData, bitmap.Width, bitmap.Height, 32, (int)bitmap.VerticalResolution, (int)bitmap.HorizontalResolution); //bits are always 32 here because of bitmap format passed
        }

        public static string GetHOCR()
        {
            return tesseractApiGetHOCR(0); //0 is a page number starting from 0 (so 1st page)
        }

    }

    public class TesseractLock //dummy class for lock
    {
    }

}
