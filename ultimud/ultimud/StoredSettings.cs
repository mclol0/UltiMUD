using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public class StoredSettings
    {
        [Obfuscation(Exclude = false)] int port_ = 1234;
        [Obfuscation(Exclude = false)] string gamefile_;
        [Obfuscation(Exclude = false)] string updateUrl_;
        [Obfuscation(Exclude = false)] bool autoCheckForUpdates_ = true;
        [Obfuscation(Exclude = false)] static public StoredSettings Instance;
        [Obfuscation(Exclude = false)] static string savefile_ = System.Windows.Forms.Application.StartupPath + "\\settings.ini";

        #region Properties
        public int Port
        {
            get { return this.port_; }
            set { this.port_ = value; }
        }

        public string GameFile
        {
            get { return this.gamefile_; }
            set { this.gamefile_ = value; }
        }

        public string UpdateUrl
        {
            get { return this.updateUrl_; }
            set { this.updateUrl_ = value; }
        }

        public bool AutoCheckForUpdates
        {
            get { return this.autoCheckForUpdates_; }
            set { this.autoCheckForUpdates_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        public static void Save()
        {
            Damdai.Serialization.Xml.Serialize(Instance, savefile_);
        }

        [Obfuscation(Exclude = false)]
        public static void Load()
        {
            if (Instance != null)
                return;

            try
            {
                if (File.Exists(savefile_))
                    Instance = Damdai.Serialization.Xml.Deserialize<StoredSettings>(savefile_);
                else
                    Instance = new StoredSettings();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Failed to load settings: " + e.Message, "ERROR");
                Instance = new StoredSettings();
            }
        }
    }
}
