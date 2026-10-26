using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;

[assembly: AssemblyTitle("UltiMUD Auto-Updater")]
[assembly: AssemblyCompany("DAMDAI")]
[assembly: AssemblyProduct("UltiMUD")]
[assembly: AssemblyVersion("1.2.0.0")]

namespace Ultimud.Updater
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string zipPath = null;
            bool noRestart = false;
            bool silent = false;

            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (string.Equals(arg, "--no-restart", StringComparison.OrdinalIgnoreCase))
                        noRestart = true;
                    else if (string.Equals(arg, "--silent", StringComparison.OrdinalIgnoreCase))
                        silent = true;
                    else if (zipPath == null)
                        zipPath = arg;
                }
            }

            if (string.IsNullOrEmpty(zipPath))
            {
                string defaultZip = Path.Combine(appDir, "Update.zip");
                if (File.Exists(defaultZip))
                {
                    zipPath = defaultZip;
                }
                else
                {
                    string[] zips = Directory.GetFiles(appDir, "*.zip");
                    if (zips.Length > 0)
                        zipPath = zips[0];
                }
            }
            else if (!Path.IsPathRooted(zipPath))
            {
                zipPath = Path.Combine(appDir, zipPath);
            }

            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
            {
                if (!silent)
                {
                    MessageBox.Show("No update package found to install.", "UltiMUD Updater",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return 1;
            }

            try
            {
                WaitForUltiMudExit();
                ExtractZipSafe(zipPath, appDir);
                File.Delete(zipPath);

                if (!noRestart)
                {
                    string exePath = Path.Combine(appDir, "UltiMUD.exe");
                    if (File.Exists(exePath))
                    {
                        Process.Start(new ProcessStartInfo(exePath) { WorkingDirectory = appDir });
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Update failed: " + ex, "ERROR");
                if (!silent)
                {
                    MessageBox.Show("An error occurred while applying the update:\r\n" + ex.Message,
                        "UltiMUD Updater Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return 2;
            }
        }

        private static void WaitForUltiMudExit()
        {
            for (int i = 0; i < 30; i++)
            {
                Process[] procs = Process.GetProcessesByName("UltiMUD");
                if (procs == null || procs.Length == 0)
                    break;
                Thread.Sleep(250);
            }
        }

        public static void ExtractZipSafe(string zipFilePath, string targetDirectory)
        {
            string fullTargetDir = Path.GetFullPath(targetDirectory);
            if (!fullTargetDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                fullTargetDir += Path.DirectorySeparatorChar;

            using (FileStream fs = File.OpenRead(zipFilePath))
            using (ZipInputStream zipStream = new ZipInputStream(fs))
            {
                ZipEntry entry;
                byte[] buffer = new byte[4096];
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    string destPath = Path.GetFullPath(Path.Combine(fullTargetDir, entry.Name));
                    if (!destPath.StartsWith(fullTargetDir, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Blocked zip slip path traversal attempt: " + entry.Name);

                    // Do not overwrite the running Updater.exe binary itself
                    if (string.Equals(Path.GetFileName(destPath), "Updater.exe", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (entry.IsDirectory)
                    {
                        if (!Directory.Exists(destPath))
                            Directory.CreateDirectory(destPath);
                        continue;
                    }

                    string dirName = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);

                    using (FileStream outStream = File.Create(destPath))
                    {
                        int bytesRead;
                        while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }
        }
    }
}
