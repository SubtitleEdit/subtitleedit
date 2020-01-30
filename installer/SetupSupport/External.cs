using System;
using System.IO;
using System.Runtime.InteropServices;
using Nikse.SetupSupport.WordLists;
using RGiesecke.DllExport;

namespace Nikse.SetupSupport
{
    public static class External
    {
        [DllExport(CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static bool CanUpdateWordlist([MarshalAs(UnmanagedType.LPWStr)] string fileName)
        {
            try
            {
                var file = new FileInfo(fileName);
                if (file.Exists)
                {
                    if (file.Name.EndsWith("_OCRFixReplaceList.xml", StringComparison.Ordinal))
                    {
                        return new OCRFixReplaceList(file).CanUpdate();
                    }
                    if (file.Name.EndsWith("_NoBreakAfterList.xml", StringComparison.Ordinal))
                    {
                        return new NoBreakAfterList(file).CanUpdate();
                    }
                    if (file.Name.EndsWith("_names.xml", StringComparison.Ordinal))
                    {
                        return new NamesList(file).CanUpdate();
                    }
                    if (file.Name.EndsWith("_user.xml", StringComparison.Ordinal))
                    {
                        return new SpellCheckList(file).CanUpdate();
                    }
                    if (file.Name.Equals("names.xml", StringComparison.Ordinal))
                    {
                        return new NamesList(file).CanUpdate();
                    }
                }
            }
            catch
            {
                // fail silently
            }
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static bool MergeWordlist([MarshalAs(UnmanagedType.LPWStr)] string fileName)
        {
            try
            {
                var file = new FileInfo(fileName);
                if (file.Exists)
                {
                    if (file.Name.EndsWith("_NoBreakAfterList.xml", StringComparison.Ordinal))
                    {
                        return new NoBreakAfterList(file).Merge();
                    }
                    if (file.Name.EndsWith("_user.xml", StringComparison.Ordinal))
                    {
                        return new SpellCheckList(file).Merge();
                    }
                }
            }
            catch
            {
                // fail silently
            }
            return false;
        }

    }
}
