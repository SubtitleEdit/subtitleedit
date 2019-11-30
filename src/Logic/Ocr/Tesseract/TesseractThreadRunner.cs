using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.Ocr.Tesseract
{
    public class TesseractThreadRunner
    {
        public class ImageJob
        {
            public string FileName { get; set; }
            public int Index { get; set; }
            public string Result { get; set; }
            public DateTime Completed { get; set; }
            public string LanguageCode { get; set; }
            public string PsmMode { get; set; }
            public string EngineMode { get; set; }
            public bool Run302 { get; set; }
            public Bitmap Bitmap { get; set; }
        }

        public delegate void OcrDone(int index, ImageJob job);

        private static readonly object QueueLock = new object();
        private readonly TesseractRunner _tesseractRunner;
        private readonly Queue<ImageJob> _jobQueue;
        private readonly OcrDone _callback;
        private bool _abort;

        public TesseractThreadRunner(OcrDone callback = null)
        {
            // Collecting Tesseract errors in a TesseractRunner instance is not thread safe.
            _tesseractRunner = new TesseractRunner(collectErrors: false);
            _jobQueue = new Queue<ImageJob>();
            _callback = callback;
        }

        private void DoOcr(object state)
        {
            if (!_abort)
            {
                var job = state as ImageJob;
                job.Result = _tesseractRunner.Run(job.LanguageCode, job.PsmMode, job.EngineMode, job.FileName, job.Run302);
                lock (QueueLock)
                {
                    job.Completed = DateTime.UtcNow;
                }
            }
        }

        public void AddImageJob(Bitmap bmp, int index, string language, string psmMode, string engineMode, bool run302, bool music302)
        {
            var job = new ImageJob
            {
                FileName = FileUtil.GetTempFileName(".png"),
                Index = index,
                Completed = DateTime.MaxValue,
                Bitmap = bmp,
                LanguageCode = language + (music302 ? "+music" : string.Empty),
                PsmMode = psmMode,
                EngineMode = engineMode,
                Run302 = run302
            };
            bmp.Save(job.FileName, System.Drawing.Imaging.ImageFormat.Png);
            ThreadPool.QueueUserWorkItem(DoOcr, job);
            _jobQueue.Enqueue(job);
        }

        public void CheckQueue()
        {
            if (_abort || _jobQueue.Count == 0)
            {
                return;
            }

            lock (QueueLock)
            {
                var checkTime = DateTime.UtcNow;
                var job = _jobQueue.Peek();
                if (job != null && job.Completed < checkTime)
                {
                    _jobQueue.Dequeue();
                    if (!_abort)
                    {
                        _callback?.Invoke(job.Index, job);
                    }
                }
            }
        }

        public void Cancel()
        {
            _abort = true;
        }

    }
}
