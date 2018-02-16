using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PebbleTimelineAppPatcher
{
    static class Program
    {
        public static string bufferLog = "";
        private static MainUI mainForm;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new MainUI();
            Application.Run(mainForm);
        }

        public static void BeginProcessing(string path, string url)
        {
            //Set registry default to url
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\RomanPort\PebbleTimelineAppPatcher", "defaultUrl", url);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\RomanPort\PebbleTimelineAppPatcher", "defaultPath", path);
            //Process
            ProcessPBW(path, url);
            MainUI.refreshUITimer.Enabled = false; //Stop the timer
            mainForm.SetButtonState(true); //Reset the button
            mainForm.RefreshUI(null, null);
        }

        public static void BeginBatch(string[] paths, string url)
        {
            //Set registry default to url
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\RomanPort\PebbleTimelineAppPatcher", "defaultUrl", url);
            int ok = 0;
            int bad = 0;
            foreach(string line in paths)
            {
                bool isOk = ProcessPBW(line, url);
                if(isOk)
                {
                    ok++;
                } else
                {
                    bad++;
                }
            }
            //Done
            bufferLog = "Done! Ok: " + ok.ToString() + " Bad: " + bad.ToString();
            MainUI.refreshUITimer.Enabled = false; //Stop the timer
            mainForm.SetButtonState(true); //Reset the button
            mainForm.RefreshUI(null, null);
        }

        private static bool ProcessPBW(string path, string url)
        {
            
            string temporaryDir = Path.GetTempPath().TrimEnd('\\') + "\\PblTimelineAppPatcher_Temp_" + DateTime.UtcNow.Ticks.ToString() + "\\";
            url = url.TrimEnd('/') + "/";
            Directory.CreateDirectory(temporaryDir);
            //Rename old file
            bufferLog = "Backing up...";
            string oldPath = path;
            try
            {
                path = path + "_old";
                File.Move(oldPath, path);

            }
            catch (Exception ex)
            {
                bufferLog = "Failed to make backup of PBW. " + ex.Message;
                return false;
            }
            //Extract
            bufferLog = "Extracting...";
            try
            {
                ZipFile.ExtractToDirectory(path, temporaryDir);
            }
            catch (Exception ex)
            {
                //Throw error and exit
                bufferLog = "Error extracting. " + ex.Message;
                return false;
            }
            bufferLog = "Patching...";
            bool wasPatchDone = false;
            try
            {
                string jsPath = temporaryDir + "pebble-js-app.js";
                string javascript = File.ReadAllText(jsPath);
                int firstHash = javascript.GetHashCode();
                javascript = javascript.Replace("https://timeline-api.getpebble.com/", url);
                wasPatchDone = firstHash != javascript.GetHashCode();
                //Save back
                File.WriteAllText(jsPath, javascript);
            }
            catch (Exception ex)
            {
                bufferLog = "Error patching. Error: " + ex.Message;
                return false;
            }
            //Recompress aad log
            bufferLog = "Recompressing...";
            try
            {
                ZipFile.CreateFromDirectory(temporaryDir, oldPath);
            }
            catch (Exception ex)
            {
                bufferLog = "Failed to recompress PBW. Error: " + ex.Message;
                return false;
            }
            //Done!
            bufferLog = "Patch done! Was patch done: " + wasPatchDone.ToString();
            return true;
        }

    }
}
