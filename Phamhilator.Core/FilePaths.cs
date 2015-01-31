using System;
using System.IO;



namespace Phamhilator.Core
{
    internal static class FilePaths
    {
        public static string ProgramDir
        {
            get
            {
                return Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            }
        }

        public static string ConfigDir
        {
            get
            {
                return Path.Combine(ProgramDir, "Config");
            }
        }
    }
}
