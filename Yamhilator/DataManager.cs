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





using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Security.Cryptography;

namespace Phamhilator.Yam.UI
{
    internal static class DataManager
    {
        private static readonly string root = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "Data");
        private static readonly ConcurrentDictionary<string, object> activeFiles = new ConcurrentDictionary<string, object>();



        static DataManager()
        {
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            var allFiles = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                activeFiles[Path.GetFileNameWithoutExtension(file)] = false;
            }
        }



        public static bool DataExists(string owner, string key)
        {
            if (String.IsNullOrEmpty(owner) || String.IsNullOrEmpty(key)) { return false; }
            var safeKey = GetSafeFileName(owner, key);
            return activeFiles.ContainsKey(safeKey);
        }

        public static string LoadData(string owner, string key)
        {
            var k = GetSafeFileName(owner, key);
            if (!activeFiles.ContainsKey(k)) { throw new KeyNotFoundException(); }

            WaitForFile(k);

            activeFiles[k] = true;
            var data = File.ReadAllText(Path.Combine(root, k), Encoding.BigEndianUnicode);

            NotifyWaitingThreads(k);

            return data;
        }

        public static void SaveData(string owner, string key, string data)
        {
            var k = GetSafeFileName(owner, key);
            if (!activeFiles.ContainsKey(k))
            {
                activeFiles[k] = false;
            }

            WaitForFile(k);

            activeFiles[k] = true;
            File.WriteAllText(Path.Combine(root, k), data, Encoding.BigEndianUnicode);

            NotifyWaitingThreads(k);
        }

        public static void DeleteData(string owner, string key)
        {
            var k = GetSafeFileName(owner, key);
            if (!activeFiles.ContainsKey(k)) { throw new KeyNotFoundException(); }

            WaitForFile(k);

            activeFiles[k] = true;
            File.Delete(k);

            NotifyWaitingThreads(k);

            object temp;
            activeFiles.TryRemove(k, out temp);
        }



        private static void NotifyWaitingThreads(string safeKey)
        {
            lock (activeFiles[safeKey]) { Monitor.Pulse(activeFiles[safeKey]); }
            activeFiles[safeKey] = false;
        }

        private static void WaitForFile(string safeKey)
        {
            if ((bool?)activeFiles[safeKey] == true)
            {
                lock (activeFiles[safeKey]) { Monitor.Wait(activeFiles[safeKey]); }
            }
        }

        private static string GetSafeFileName(string owner, string key)
        {
            var upperOwner = owner.Trim().ToUpperInvariant();
            var bytes = Encoding.BigEndianUnicode.GetBytes(upperOwner + key);
            var hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
