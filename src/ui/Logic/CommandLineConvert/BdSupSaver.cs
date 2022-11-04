using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Forms;

namespace Nikse.SubtitleEdit.Logic.CommandLineConvert
{
    public static class BdSupSaver
    {
        public static void SaveBdSup(string fileName, Subtitle sub, IList<IBinaryParagraph> binaryParagraphs, ExportPngXml form, int width, int height, bool isImageBased, FileStream binarySubtitleFile, CancellationToken cancellationToken)
        {
            var queue = new Queue<Task<ExportPngXml.MakeBitmapParameter>>();
            var disposeList = new List<Task>();
            for (var index = 0; index < sub.Paragraphs.Count; index++)
            {
                CheckQueue(binarySubtitleFile, queue, disposeList,2);

                var mp = form.MakeMakeBitmapParameter(index, width, height);
                var task = GenerateImage(fileName, sub, binaryParagraphs, isImageBased, mp, index);
                queue.Enqueue(task);

                if (index % 10 == 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    System.Windows.Forms.Application.DoEvents();
                }
            }

            CheckQueue(binarySubtitleFile, queue, disposeList,0);
            Task.WaitAll(disposeList.ToArray());
        }

        private static void CheckQueue(Stream binarySubtitleFile, Queue<Task<ExportPngXml.MakeBitmapParameter>> queue, List<Task> disposeList, int max)
        {
            while (queue.Count > max)
            {
                var t = queue.Dequeue();
                t.Wait();

                binarySubtitleFile.Write(t.Result.Buffer, 0, t.Result.Buffer.Length);

                var task = DisposeImage(t.Result);
                disposeList.Add(task);
            }
        }

        private static Task<ExportPngXml.MakeBitmapParameter> GenerateImage(string fileName, Subtitle sub, IList<IBinaryParagraph> binaryParagraphs, bool isImageBased, ExportPngXml.MakeBitmapParameter mp, int index)
        {
            mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
            if (binaryParagraphs != null && binaryParagraphs.Count > 0)
            {
                if (index < binaryParagraphs.Count)
                {
                    mp.Bitmap = binaryParagraphs[index].GetBitmap();
                    mp.Forced = binaryParagraphs[index].IsForced;
                }
            }
            else if (isImageBased)
            {
                using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(fileName), sub.Paragraphs[index].Text))))
                {
                    mp.Bitmap = (Bitmap)Image.FromStream(ms);
                }
            }
            else
            {
                mp.Bitmap = ExportPngXml.GenerateImageFromTextWithStyle(mp);
            }

            ExportPngXml.MakeBluRaySupImage(mp);
            return Task.FromResult(mp);
        }

        private static Task DisposeImage(ExportPngXml.MakeBitmapParameter mp)
        {
            if (mp.Bitmap != null)
            {
                mp.Bitmap.Dispose();
                mp.Bitmap = null;
            }

            return Task.CompletedTask;
        }
    }
}