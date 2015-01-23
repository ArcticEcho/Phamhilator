using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Yhamhilator
{
    internal class CredManager
    {
        private static readonly string rootConfig = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "Config");
        private readonly string emailPath = Path.Combine(rootConfig, "credE");
        private readonly string passwordPath = Path.Combine(rootConfig, "credP");


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
            //if (!File.Exists(filePath))
            //{
                var dirPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                File.Create(filePath).Dispose();
                //File.SetAttributes(emailPath, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
            //}
        }
    }
}
