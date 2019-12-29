using System.IO;
using System.Windows.Forms;
using clawSoft.clawPDF.Core.Helper;

namespace clawPDF.Bridge
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args != null || args.Length != 0)
            {
                try
                {
                    string inffile = args[0].Split('=')[1];
                    if (File.Exists(inffile)) start(inffile);
                }
                catch
                {
                }
            }
        }

        /// <summary> BIO: Of interest: launches the main exe with expected args... </summary>
        private static void start(string infFile)
        {
            INIFile iniFile = new INIFile(infFile);
            string username = iniFile.Read("0", "Username");
            var workDir = Path.GetDirectoryName(Application.ExecutablePath);
            var appPath = workDir + @"\" + "clawPDF.exe";
            var cmdLine = appPath + " /INFODATAFILE=" + infFile;

            BennyLogger.Log("Bridge start: (username|appPath|cmdLine)", username, appPath, cmdLine);

            ProcessExtensions.StartProcessAsUser(username, appPath, cmdLine, workDir, true);
            //ProcessExtensions.StartProcessAsUser(username, Path.GetDirectoryName(Application.ExecutablePath) + @"\" + "clawPDF.exe", Path.GetDirectoryName(Application.ExecutablePath) + @"\" + "clawPDF.exe" + " /INFODATAFILE=" + infFile, Path.GetDirectoryName(Application.ExecutablePath), true);
        }
    }
}