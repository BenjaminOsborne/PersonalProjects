using System;
using System.IO;
using System.Text;
using System.Threading;
using clawSoft.clawPDF.Core.Settings.Enums;

namespace clawSoft.clawPDF.Core.Helper
{
    public static class BennyConfig
    {
        private static readonly string _fileTimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
        private static readonly string _parentFolderName = $"Run_{_fileTimeStamp}";

        private static readonly Lazy<string> _lazyLogPath = new Lazy<string>(() => _GenerateFilePath("Log", ".txt"));

        private static int _outputFileCounter = 0;
        private const string _version = "V8";

        private const string _parentDirectory = @"C:\BennyLogger";
        
        public static bool Alter => true;
        public static bool UseJpeg => true;

        public static string GetLogFilePath() => _lazyLogPath.Value;
        
        public static string GenerateOutputFile(OutputFormat type) => _GenerateFilePath($"File_{Interlocked.Increment(ref _outputFileCounter)}", $".{_GetType(type)}");

        private static string _GetType(OutputFormat type)
        {
            switch (type)
            {
                case OutputFormat.Pdf:
                case OutputFormat.PdfA1B:
                case OutputFormat.PdfA2B:
                case OutputFormat.PdfX:
                    return "pdf";
                
                case OutputFormat.Jpeg:
                    return "jpg";
                
                case OutputFormat.Png:
                    return "png";
                
                case OutputFormat.Tif:
                    return "tif";
                
                case OutputFormat.Txt:
                    return "txt";
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static string _GenerateFilePath(string fileNamePrefix, string suffix)
        {
            var directory = $@"{_parentDirectory}\{_parentFolderName}";
            Directory.CreateDirectory(directory);
            return $@"{directory}\{_version}_{fileNamePrefix}{suffix}";
        }
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
                File.AppendAllText(BennyConfig.GetLogFilePath(), $@"{log}{_nl}{_nl}");
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
