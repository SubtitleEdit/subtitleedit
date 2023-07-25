using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace Nikse.SubtitleEdit.Logic.SeJob
{
    public class SeJobHandler
    {
        public static SeJobModel LoadSeJob(byte[] bytes)
        {
            try
            {
                //var json = Unzip(bytes);
                var json = Encoding.UTF8.GetString(bytes);
                var model = JsonConvert.DeserializeObject<SeJobModel>(json);
                return model;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] SaveSeJob(SeJobModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var bytes = Encoding.UTF8.GetBytes(json);
            //var bytes = Zip(json);
            return bytes;
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            var bytes = new byte[4096];
            int bytesRead;
            while ((bytesRead = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, bytesRead);
            }
        }

        private static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        private static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}
