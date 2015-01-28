using System;
using System.Collections.Generic;
using System.IO;
using System.Text;



namespace Phamhilator
{
    internal class CredManager
    {
        private readonly string emailPath = Path.Combine(FilePaths.ConfigDir, "credE");
        private readonly string passwordPath = Path.Combine(FilePaths.ConfigDir, "credP");


        internal string Email
        {
            get
            {
                if (!File.Exists(emailPath))
                {
                    CreateFile(emailPath);
                }

                return File.ReadAllText(emailPath, Encoding.UTF8);
            }

            set
            {
                if (!File.Exists(emailPath))
                {
                    CreateFile(emailPath);
                }

                File.WriteAllText(emailPath, value, Encoding.UTF8);
            }
        }

        internal string Password
        {
            get
            {
                if (!File.Exists(passwordPath))
                {
                    CreateFile(passwordPath);
                }

                return File.ReadAllText(passwordPath, Encoding.UTF8);
            }

            set
            {
                if (!File.Exists(passwordPath))
                {
                    CreateFile(passwordPath);
                }

                File.WriteAllText(passwordPath, value, Encoding.UTF8);
            }
        }



        private void CreateFile(string filePath)
        {
            var dirPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            File.Create(filePath).Dispose();
        }
    }
}
