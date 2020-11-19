namespace Nikse.SubtitleEdit.Core.Common
{
    // The MurmurHash3 algorithm was created by Austin Appleby and put into the public domain.  See http://code.google.com/p/smhasher
    // This code is based on https://gist.github.com/automatonic/3725443
    public class MurMurHash3
    {
        private const uint Seed = 144;

        /// <summary>
        /// Fast hashing of byte array
        /// </summary>
        /// <param name="arr">Byte array to hash</param>
        /// <returns>Hash value</returns>
        public static uint Hash(byte[] arr)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;
            uint h1 = Seed;
            uint k1;

            int length = arr.Length / 4;
            for (int i = 0; i < length; i++)
            {
                int idx = i * 4;
                k1 = (uint)(arr[idx] | arr[idx + 1] << 8 | arr[idx + 2] << 16 | arr[idx + 3] << 24);

                // bitmagic hash
                k1 *= c1;
                k1 = Rotl32(k1, 15);
                k1 *= c2;

                h1 ^= k1;
                h1 = Rotl32(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
            }

            switch (arr.Length % 4)
            {
                case 3:
                    k1 = (uint)(arr[arr.Length - 3] | arr[arr.Length - 2] << 8 | arr[arr.Length - 1] << 16);
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
                case 2:
                    k1 = (uint)(arr[arr.Length - 2] | arr[arr.Length - 1] << 8);
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
                case 1:
                    k1 = arr[arr.Length - 1];
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
            }

            // finalization, magic chants to wrap it all up
            h1 ^= (uint)arr.Length;
            h1 = Fmix(h1);

            return h1;
        }

        private static uint Rotl32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static uint Fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
    }

}
