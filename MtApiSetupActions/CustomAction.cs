using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;

namespace MtApiSetupActions
{
    public class CustomActions
    {
        private const string MtApiFile = "MtApi.ex4";
        private const string TerminalFolder = @"\MetaQuotes\Terminal";
        private const string Mql4Folder = "MQL4";
        private const string InstalledExpertFolderProperty = "ExpertFolder";
        private const string DestinationExpertFolder = @"\Experts\";

        [CustomAction]
        public static ActionResult InstallEx4File(Session session)
        {
            session.Log("Begin action InstallEx4File...");

            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var metaquotesFolder = appDataFolder + TerminalFolder;

            if (Directory.Exists(metaquotesFolder))
            {
                var foldersFound = Directory.GetDirectories(metaquotesFolder, Mql4Folder, SearchOption.AllDirectories);
                Console.WriteLine(string.Join("\n", foldersFound));

                foreach (var folder in foldersFound)
                {
                    var srcFile = session[InstalledExpertFolderProperty] + MtApiFile;
                    var destFile = folder + DestinationExpertFolder + MtApiFile;

                    session.Log(string.Format("Try to copy from {0} to {1}", srcFile, destFile));

                    try
                    {
                        File.Copy(srcFile, destFile, true);
                    }
                    catch (Exception e)
                    {
                        session.Log(string.Format("Failed to copy MtApi4.ex4. {0}", e.Message));
                    }

                    session.Log(string.Format("MtApi.ex4 has been coppied to {0}", destFile));
                }
            }
            else
            {
                session.Log("MetaTrader is not installed");
            }

            return ActionResult.Success;
        }
    }
}
