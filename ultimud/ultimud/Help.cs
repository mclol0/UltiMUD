using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Help : ISaveable
    {
        [Obfuscation(Exclude = false)] Guid id_; //Deprecated.
        [Obfuscation(Exclude = false)] string name_;
        [Obfuscation(Exclude = false)] string text_;
        [Obfuscation(Exclude = false)] static List<Help> help_ = new List<Help>();
        [Obfuscation(Exclude = false)] static string path_;

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        public string Name
        {
            get { return this.name_; }
            set
            {
                if (!string.IsNullOrEmpty(this.name_) && this.name_ != value)
                    this.Delete();

                this.name_ = Telnet.Decolorize(value);
            }
        }
        
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string Text
        {
            get { return this.text_; }
            set { this.text_ = value; }
        }
        
        [Browsable(false)]
        public static List<Help> HelpFiles
        {
            get { return help_; }
        }
        #endregion

        public Help() { }
        public Help(string name, string text)
        {
            this.name_ = name;
            this.text_ = text;
        }
        //Deprecated.
        internal Help(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                this.name_ = reader.ReadLine();
                this.text_ = reader.ReadToEnd();
            }
        }

        [Obfuscation(Exclude = false)]
        public static Help Get(string arg)
        {
            lock (help_)
            {
                return help_.Find(delegate(Help help)
                {
                    return !string.IsNullOrEmpty(help.name_) && help.name_.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase);
                });
            }
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            else
                return base.ToString();
        }

        public string Print()
        {
            return Server.Instance.Settings.GetColorCode(Settings.Text.Help) + "[" + this.Name + "]\r\n" + this.Text;
        }

        [Obfuscation(Exclude = false)]
        public void Save()
        {
            if (!string.IsNullOrEmpty(this.name_))
                Damdai.Serialization.Xml.Serialize(this, path_ + "\\" + this.name_ + ".help");
        }

        [Obfuscation(Exclude = false)]
        public void Delete()
        {
            if (!string.IsNullOrEmpty(this.name_))
                File.Delete(path_ + "\\" + this.name_ + ".help");
        }

        
        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Help)))
                Server.Instance.DataSets.Add(typeof(Help), new Dictionary<Guid, IIdentifiable>());

            HelpFiles.Clear();
            string loadhelppath = loadpath + "\\Help";
            path_ = serverpath + "\\Help";
            Directory.CreateDirectory(path_);

            if (Directory.Exists(loadhelppath))
            {
                foreach (FileInfo file in new DirectoryInfo(loadhelppath).GetFiles("*.help"))
                    help_.Add(Damdai.Serialization.Xml.Deserialize<Help>(file.FullName));
            }

            LoadOldHelpFile(loadpath);
        }
        
        internal static void PostInit()
        {
            if (Server.Instance.DataSets[typeof(Help)].Count > 0)
            {
                foreach (Help help in Server.Instance.DataSets[typeof(Help)].Values)
                {
                    help_.Add(help);
                    help.Save();
                }
                Server.Instance.DataSets.Remove(typeof(Help));
            }
        }
        
        //Deprecated.
        [Obfuscation(Exclude = false)]
        public static void LoadOldHelpFile(string path)
        {
            if (path != null)
            {
                path = path.Substring(0, path.LastIndexOf("\\muds"));
                string helpfile = path + "\\help.zip";
                if (File.Exists(helpfile))
                {
                    using (FileStream stream = File.OpenRead(helpfile))
                    {
                        ZipFile zip = new ZipFile(stream);
                        foreach (ZipEntry e in zip)
                        {
                            Help help = new Help(zip.GetInputStream(e));
                            if (!help_.Exists(delegate(Help h) { return h.name_.Equals(help.name_, StringComparison.CurrentCultureIgnoreCase); }))
                            {
                                help_.Add(help);
                                help.Save();
                            }
                        }
                        zip.Close();
                    }
                    File.Move(helpfile, path + "\\~help.zip");
                }
            }
        }
    }
}
