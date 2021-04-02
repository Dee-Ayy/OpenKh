using OpenKh.Common;
using System;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Egs
{
    public class EgsEncryption
    {
        private static readonly byte[] MasterKey = new byte[]
        {
            0x7E, 0x88, 0x97, 0x55, 0x0B, 0x06, 0xF1, 0x08, 0xEB, 0xBB, 0x14, 0x1C, 0xD8, 0x7A, 0xEC, 0x41,
            0x34, 0xB2, 0xA3, 0x46, 0xEF, 0x6B, 0xFE, 0xE1, 0xCF, 0x53, 0xA5, 0x05, 0x12, 0xD2, 0x8E, 0x52,
            0x4A, 0x80, 0xE9, 0x81, 0xB0, 0xF0, 0xB4, 0x9C, 0xFF, 0x0F, 0x15, 0x13, 0xDA, 0x73, 0x4E, 0x77,
            0xBE, 0xD7, 0x30, 0xE5, 0xF6, 0x5A, 0x11, 0x37, 0x67, 0xBC, 0x83, 0x6F, 0x27, 0x76, 0xD0, 0xCD,
            0x69, 0x0D, 0x2E, 0x51, 0x42, 0x90, 0xB8, 0xB6, 0x4C, 0xAD, 0xCE, 0x5B, 0x1A, 0x1F, 0xF5, 0xAF,
            0x01, 0xF8, 0x5E, 0x3A, 0x6E, 0x68, 0x8B, 0xE8, 0x9F, 0xC9, 0xD9, 0x26, 0x92, 0x29, 0xC8, 0x33,
            0x98, 0x32, 0x54, 0xD4, 0x44, 0x25, 0x66, 0xAC, 0x5F, 0x99, 0x21, 0xE4, 0x8F, 0x1D, 0xC2, 0xD5,
            0xA4, 0x62, 0xF9, 0x02, 0x61, 0xDE, 0x59, 0xE7, 0x07, 0x9A, 0xFA, 0x2F, 0x95, 0x3F, 0x86, 0xD3,
            0x78, 0xA7, 0x75, 0xED, 0xD6, 0x2D, 0x64, 0x87, 0xBD, 0xC7, 0xC1, 0xAA, 0xF2, 0x8C, 0x17, 0xCB,
            0x31, 0x8A, 0xC3, 0xCC, 0x04, 0xEE, 0x6A, 0xAB, 0x5C, 0x22, 0x70, 0xCA, 0x9E, 0x71, 0x6D, 0x85,
            0x45, 0x5D, 0xB9, 0xA9, 0xA6, 0x10, 0x47, 0xFB, 0x82, 0x7D, 0x84, 0x7B, 0xC6, 0xE2, 0x38, 0xFC,
            0x2B, 0x0E, 0x20, 0x9D, 0xC5, 0xF3, 0x39, 0xA8, 0xA0, 0x65, 0x58, 0x43, 0x7C, 0xE3, 0x36, 0x18,
            0x72, 0x49, 0x79, 0xAE, 0xD1, 0x74, 0x40, 0xC4, 0x91, 0x4F, 0x24, 0x63, 0xBF, 0xBA, 0x23, 0x96,
            0x50, 0xB3, 0x57, 0xDF, 0x1E, 0x03, 0x48, 0x7F, 0x35, 0x4D, 0x3E, 0xE6, 0xA1, 0xDD, 0x09, 0x3C,
            0x3D, 0x3B, 0x56, 0x8D, 0x93, 0x2A, 0x9B, 0x4B, 0x0C, 0x28, 0xB1, 0xE0, 0x60, 0x89, 0x19, 0xDB,
            0x2C, 0xF7, 0x6C, 0xB5, 0x1B, 0x94, 0xC0, 0xDC, 0xEA, 0xB7, 0x0A, 0xF4, 0x16, 0xFD, 0xA2, 0x00,
        };
        private static readonly byte[] ScrambleKey = new byte[]
        {
            0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00,
            0x20, 0x00, 0x00, 0x00,
            0x40, 0x00, 0x00, 0x00,
            0x80, 0x00, 0x00, 0x00,
            0x1B, 0x00, 0x00, 0x00,
            0x36, 0x00, 0x00, 0x00,
        };

        public static byte[] Decrypt(Stream stream)
        {
            const int PassCount = 10;
            var key = GenerateKey(stream.ReadBytes(0x10), PassCount);
            var data = stream.ReadBytes(0x100);

            for (var i = 0; i < 0x100; i += 0x10)
                DecryptChunk(key, data, i, PassCount);
            return data;
        }

        public static byte[] GenerateKey(byte[] seed, int passCount)
        {
            var finalKey = new byte[0xB0];
            for (var i = 0; i < seed.Length; i++)
                finalKey[i] = seed[i] == 0 ? (byte)i : seed[i];

            for (var i = 0; i < passCount * 4; i++)
            {
                var frame = finalKey.AsSpan()[(0x0C + i * 4)..(0x10 + i * 4)];
                if ((i % 4) == 0)
                    frame = new byte[]
                    {
                        (byte)(MasterKey[frame[1]] ^ ScrambleKey[i + 0]),
                        (byte)(MasterKey[frame[2]] ^ ScrambleKey[i + 1]),
                        (byte)(MasterKey[frame[3]] ^ ScrambleKey[i + 2]),
                        (byte)(MasterKey[frame[0]] ^ ScrambleKey[i + 3]),
                    };

                finalKey[0x10 + i * 4] = (byte)(finalKey[0x00 + i * 4] ^ frame[0]);
                finalKey[0x11 + i * 4] = (byte)(finalKey[0x01 + i * 4] ^ frame[1]);
                finalKey[0x12 + i * 4] = (byte)(finalKey[0x02 + i * 4] ^ frame[2]);
                finalKey[0x13 + i * 4] = (byte)(finalKey[0x03 + i * 4] ^ frame[3]);
            }

            return finalKey;
        }

        public static void DecryptChunk(byte[] key, byte[] ptrData, int index, int passCount)
        {
            for (var i = passCount; i >= 0; i--)
            {
                ptrData[0x00 + index] ^= key[0x00 + 0x10 * i];
                ptrData[0x01 + index] ^= key[0x01 + 0x10 * i];
                ptrData[0x02 + index] ^= key[0x02 + 0x10 * i];
                ptrData[0x03 + index] ^= key[0x03 + 0x10 * i];
                ptrData[0x04 + index] ^= key[0x04 + 0x10 * i];
                ptrData[0x05 + index] ^= key[0x05 + 0x10 * i];
                ptrData[0x06 + index] ^= key[0x06 + 0x10 * i];
                ptrData[0x07 + index] ^= key[0x07 + 0x10 * i];
                ptrData[0x08 + index] ^= key[0x08 + 0x10 * i];
                ptrData[0x09 + index] ^= key[0x09 + 0x10 * i];
                ptrData[0x0A + index] ^= key[0x0A + 0x10 * i];
                ptrData[0x0B + index] ^= key[0x0B + 0x10 * i];
                ptrData[0x0C + index] ^= key[0x0C + 0x10 * i];
                ptrData[0x0D + index] ^= key[0x0D + 0x10 * i];
                ptrData[0x0E + index] ^= key[0x0E + 0x10 * i];
                ptrData[0x0F + index] ^= key[0x0F + 0x10 * i];
            }
        }
    }
}
