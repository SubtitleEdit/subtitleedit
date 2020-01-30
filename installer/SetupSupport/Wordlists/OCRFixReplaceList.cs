using System;
using System.IO;
using System.Xml;

namespace Nikse.SetupSupport.WordLists
{
    internal class OCRFixReplaceList
    {
        private readonly string _fileName;

        public OCRFixReplaceList(FileInfo file)
        {
            _fileName = file.FullName;
        }

        public bool CanUpdate()
        {
            var document = new XmlDocument { XmlResolver = null };
            document.Load(_fileName);
            var root = document.DocumentElement;
            if (root.Name.Equals("OCRFixReplaceList", StringComparison.Ordinal) &&
                (root.GetAttribute("Update").Trim().Equals("Never", StringComparison.OrdinalIgnoreCase) ||
                 root.GetAttribute("DoNotUpdate").Trim().Equals("True", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }

    }
}
