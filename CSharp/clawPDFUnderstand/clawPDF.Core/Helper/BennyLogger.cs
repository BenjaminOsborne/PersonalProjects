using System;
using System.IO;
using System.Text;

namespace clawSoft.clawPDF.Core.Helper
{
    public static class BennyLogger
    {
        private static readonly string _fileName = $@"C:\BennyLogger\Log_v1_{DateTime.Now.ToFileTimeUtc()}.txt";

        public static void Log(string context, params object[] extra) =>
            _Write(_OnNewLines(context, extra));

        private static void _Write(string log)
        {
            try
            {
                File.AppendAllText(_fileName, $@"{log}{Environment.NewLine}");
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
