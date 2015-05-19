/*
 * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Phamhilator.Yam.UI
{
    public static class DataUtilities
    {
        public static byte[] AseEncrypt(byte[] data, byte[] key)
        {
            using (var ms = new MemoryStream())
            using (var aes = new AesManaged() { Key = key, IV = GetIVFromKey(key) })
            {
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] AseDecrypt(byte[] data, byte[] key)
        {
            using (var ms = new MemoryStream())
            using (var aes = new AesManaged() { Key = key, IV = GetIVFromKey(key) })
            {
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] GZipCompress(byte[] data)
        {
            return data;
            //byte[] compressed;

            //using (var compStrm = new MemoryStream())
            //{
            //    using (var zipper = new GZipStream(compStrm, CompressionMode.Compress))
            //    using (var ms = new MemoryStream(data))
            //    {
            //        ms.CopyTo(zipper);
            //    }

            //    compressed = compStrm.ToArray();
            //}

            //return compressed;
        }

        public static byte[] GZipDecompress(byte[] data)
        {
            return data;
            //using (var msIn = new MemoryStream(data))
            //using (var unzipper = new GZipStream(msIn, CompressionMode.Decompress))
            //using (var msOut = new MemoryStream())
            //{
            //    unzipper.CopyTo(msOut);
            //    return msOut.ToArray();
            //}
        }



        private static byte[] GetIVFromKey(byte[] key)
        {
            var iv = new byte[16];

            for (var i = 0; i < 16; i++)
            {
                iv[i] = key[i];
            }

            return iv;
        }
    }
}
