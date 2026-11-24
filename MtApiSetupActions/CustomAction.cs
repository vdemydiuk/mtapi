using System;
using System.IO;
using WixToolset.Dtf.WindowsInstaller;

namespace MtApiSetupActions
{
    public class CustomActions
    {
        // Installer properties that parametrize the action (set in Product.wxs of each installer).
        private const string ExpertFileNameProperty = "EXPERT_FILE_NAME";   // e.g. MtApi5.ex5
        private const string MqlFolderNameProperty = "MQL_FOLDER_NAME";     // MQL4 or MQL5
        private const string InstalledExpertFolderProperty = "ExpertFolder"; // folder the expert was installed to

        private const string TerminalFolder = @"MetaQuotes\Terminal";
        private const string ExpertsFolder = "Experts";

        /// <summary>
        /// Copies the installed expert file into the Experts folder of every MetaTrader terminal
        /// data folder (%APPDATA%\MetaQuotes\Terminal\{hash}\MQL4|MQL5\Experts).
        /// It never fails the installation: problems are only written to the installer log.
        /// </summary>
        [CustomAction]
        public static ActionResult InstallExpertFile(Session session)
        {
            session.Log("Begin action InstallExpertFile...");

            try
            {
                var fileName = session[ExpertFileNameProperty];
                var mqlFolder = session[MqlFolderNameProperty];
                var installedFolder = session[InstalledExpertFolderProperty];

                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(mqlFolder) || string.IsNullOrEmpty(installedFolder))
                {
                    session.Log("InstallExpertFile: required properties are not set, nothing to do.");
                    return ActionResult.Success;
                }

                var srcFile = Path.Combine(installedFolder, fileName);
                if (!File.Exists(srcFile))
                {
                    session.Log("InstallExpertFile: source file " + srcFile + " not found.");
                    return ActionResult.Success;
                }

                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var terminalsFolder = Path.Combine(appDataFolder, TerminalFolder);

                if (!Directory.Exists(terminalsFolder))
                {
                    session.Log("InstallExpertFile: MetaTrader is not installed (" + terminalsFolder + " not found).");
                    return ActionResult.Success;
                }

                foreach (var terminalDataFolder in Directory.GetDirectories(terminalsFolder))
                {
                    var mqlPath = Path.Combine(terminalDataFolder, mqlFolder);
                    if (!Directory.Exists(mqlPath))
                        continue; // terminal data folder of another platform (MT4/MT5)

                    var destFolder = Path.Combine(mqlPath, ExpertsFolder);
                    var destFile = Path.Combine(destFolder, fileName);

                    session.Log("InstallExpertFile: copy from " + srcFile + " to " + destFile);
                    try
                    {
                        Directory.CreateDirectory(destFolder);
                        File.Copy(srcFile, destFile, true);
                        session.Log("InstallExpertFile: " + fileName + " has been copied to " + destFile);
                    }
                    catch (Exception e)
                    {
                        session.Log("InstallExpertFile: failed to copy " + fileName + " to " + destFile + ". " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                session.Log("InstallExpertFile: unexpected error. " + e);
            }

            return ActionResult.Success;
        }
    }
}
