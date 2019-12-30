using System;
using System.IO;
using System.Text;
using System.Threading;

namespace clawSoft.clawPDF.Core.Helper
{
    public static class BennyConfig
    {
        private static int _outputFileCounter = 1;
        private const string _version = "V4";

        public const string Directory = @"C:\BennyLogger";
        public static readonly string FileTimeStamp = DateTime.Now.ToFileTimeUtc().ToString();

        public static bool Alter => true;
        
        public static readonly string LogFileName = _GenerateFile("Log");
        
        public static string GenerateOutputFile() => _GenerateFile($"File_{Interlocked.Increment(ref _outputFileCounter)}");

        private static string _GenerateFile(string fileNamePrefix) => $@"{Directory}\{fileNamePrefix}_{_version}_{FileTimeStamp}.txt";
    }

    public static class BennyLogger
    {
        private static string _nl => Environment.NewLine;

        public static void Log(string context, params object[] extra) =>
            _Write(_OnNewLines(context, extra));

        public static void LogAnon<T>(string context, T anon)
        {
            var display = anon.ToString().Replace(", ", _nl);
            _Write($"{context}{_nl}{display}");
        }

        private static void _Write(string log)
        {
            try
            {
                File.AppendAllText(BennyConfig.LogFileName, $@"{log}{_nl}{_nl}");
            }
            catch
            {
            }
        }

        private static string _OnNewLines(string context, object[] extra)
        {
            if (extra.Length == 0)
            {
                return context;
            }

            var builder = new StringBuilder();
            builder.AppendLine(context);
            foreach (var obj in extra)
            {
                builder.AppendLine(obj.ToString());
            }
            return builder.ToString();
        }
    }
}
