using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.Ocr.Tesseract
{
    public class TesseractMultiThreadRunner
    {
        public delegate void OcrDone(int index, TesseractThreadRunner.ImageJob job);
        private readonly OcrDone _callback;
        private readonly Queue<TesseractThreadRunner.ImageJob> _jobQueue;
        private static readonly object QueueLock = new object();
        private readonly TesseractMultiRunner _tesseractRunner;
        private bool _abort;

        public TesseractMultiThreadRunner(OcrDone callback = null)
        {
            _jobQueue = new Queue<TesseractThreadRunner.ImageJob>();
            _callback = callback;
            _tesseractRunner = new TesseractMultiRunner();
            _jobs = new List<TesseractThreadRunner.ImageJob>();
        }

        private void DoOcr(object j)
        {
            if (_abort)
            {
                return;
            }

            var jobs = (List<TesseractThreadRunner.ImageJob>)j;
            var job = jobs.First();
            var results = _tesseractRunner.Run(job.LanguageCode, job.PsmMode, job.EngineMode, jobs.Select(p => p.FileName).ToList(), job.Run302);

            lock (QueueLock)
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    jobs[i].Completed = DateTime.UtcNow;
                    jobs[i].Result = results.Count - 1 >= i ? results[i] : "MISSING!";
                }
                job.Completed = DateTime.UtcNow;
            }
        }

        private const int BundleSize = 3;
        private List<TesseractThreadRunner.ImageJob> _jobs;

        public void AddImageJob(Bitmap bmp, int index, string language, string psmMode, string engineMode, bool run302, bool music302, bool isLast)
        {
            var job = new TesseractThreadRunner.ImageJob
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

            _jobs.Add(job);
            if (_jobs.Count >= BundleSize || isLast)
            {
                ThreadPool.QueueUserWorkItem(DoOcr, _jobs);
                _jobs = new List<TesseractThreadRunner.ImageJob>();
            }
            _jobQueue.Enqueue(job);
        }

        public void CheckQueue()
        {
            if (_jobQueue.Count == 0)
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
                    _callback?.Invoke(job.Index, job);
                }
            }
        }

        public void Cancel()
        {
            _abort = true;
        }
    }
}
