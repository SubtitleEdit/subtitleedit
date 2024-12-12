using System.IO;

namespace SevenZipExtractor
{
    internal class ArchiveStreamCallback : IArchiveExtractCallback, ICryptoGetTextPassword
    {
        private readonly uint fileNumber;
        private readonly Stream stream;

        public string Password { get; }

        public ArchiveStreamCallback(uint fileNumber, Stream stream, string password = null)
        {
            this.fileNumber = fileNumber;
            this.stream = stream;
            Password = password ?? "";
        }

        public void SetTotal(ulong total)
        {
        }

        public void SetCompleted(ref ulong completeValue)
        {
        }

        public int CryptoGetTextPassword(out string password)
        {
            password = this.Password;
            return 0;
        }

        public int GetStream(uint index, out ISequentialOutStream outStream, AskMode askExtractMode)
        {
            if ((index != this.fileNumber) || (askExtractMode != AskMode.kExtract))
            {
                outStream = null;
                return 0;
            }

            outStream = new OutStreamWrapper(this.stream);

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