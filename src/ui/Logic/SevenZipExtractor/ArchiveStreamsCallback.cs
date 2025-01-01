﻿using System.Collections.Generic;
using System.IO;

namespace SevenZipExtractor
{
    internal class ArchiveStreamsCallback : IArchiveExtractCallback, ICryptoGetTextPassword
    {
        private readonly IList<Stream> streams;

        public string Password { get; }

        public ArchiveStreamsCallback(IList<Stream> streams, string password = null)
        {
            this.streams = streams;
            Password = password;
        }
        public int CryptoGetTextPassword(out string password)
        {
            password = Password;
            return 0;
        }

        public void SetTotal(ulong total)
        {
        }

        public void SetCompleted(ref ulong completeValue)
        {
        }

        public int GetStream(uint index, out ISequentialOutStream outStream, AskMode askExtractMode)
        {
            if (askExtractMode != AskMode.kExtract)
            {
                outStream = null;
                return 0;
            }

            if (this.streams == null)
            {
                outStream = null;
                return 0;
            }

            Stream stream = this.streams[(int) index];

            if (stream == null)
            {
                outStream = null;
                return 0;
            }

            outStream = new OutStreamWrapper(stream);

            return 0;
        }

        public void PrepareOperation(AskMode askExtractMode)
        {
        }

        public void SetOperationResult(OperationResult resultEOperationResult)
        {
        }
    }
}