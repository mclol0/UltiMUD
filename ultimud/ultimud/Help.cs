using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using System.Reflection;
using System.Text;
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
            if (string.IsNullOrEmpty(arg))
                return null;
            lock (help_)
            {
                Help exact = help_.Find(delegate(Help help)
                {
                    return !string.IsNullOrEmpty(help.name_) && help.name_.Equals(arg, StringComparison.CurrentCultureIgnoreCase);
                });
                if (exact != null)
                    return exact;

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
            {
                string dir = !string.IsNullOrEmpty(path_) ? path_ : (Server.Instance != null ? Server.Instance.ServerPath + "\\Help" : null);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                    Damdai.Serialization.Xml.Serialize(this, dir + "\\" + this.name_ + ".help");
                }
            }
        }

        [Obfuscation(Exclude = false)]
        public void Delete()
        {
            if (!string.IsNullOrEmpty(this.name_))
            {
                string dir = !string.IsNullOrEmpty(path_) ? path_ : (Server.Instance != null ? Server.Instance.ServerPath + "\\Help" : null);
                if (!string.IsNullOrEmpty(dir))
                {
                    string file = dir + "\\" + this.name_ + ".help";
                    if (File.Exists(file))
                        File.Delete(file);
                }
            }
        }

        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Help)))
                Server.Instance.DataSets.Add(typeof(Help), new Dictionary<Guid, IIdentifiable>());

            HelpFiles.Clear();
            if (string.IsNullOrEmpty(serverpath) && Server.Instance != null)
                serverpath = Server.Instance.ServerPath;

            path_ = serverpath + "\\Help";
            Directory.CreateDirectory(path_);

            string loadhelppath = !string.IsNullOrEmpty(loadpath) ? (loadpath + "\\Help") : path_;
            if (Directory.Exists(loadhelppath))
            {
                foreach (FileInfo file in new DirectoryInfo(loadhelppath).GetFiles("*.help"))
                    help_.Add(Damdai.Serialization.Xml.Deserialize<Help>(file.FullName));
            }
            else if (Directory.Exists(path_))
            {
                foreach (FileInfo file in new DirectoryInfo(path_).GetFiles("*.help"))
                    help_.Add(Damdai.Serialization.Xml.Deserialize<Help>(file.FullName));
            }

            LoadOldHelpFile(loadpath);
        }
        
        internal static void PostInit()
        {
            if (Server.Instance.DataSets.ContainsKey(typeof(Help)))
            {
                if (Server.Instance.DataSets[typeof(Help)].Count > 0)
                {
                    foreach (Help help in Server.Instance.DataSets[typeof(Help)].Values)
                    {
                        help_.Add(help);
                        help.Save();
                    }
                }
                Server.Instance.DataSets.Remove(typeof(Help));
            }

            if (help_.Count == 0)
            {
                SeedDefaultHelp();
            }
            else if (Get("COMMANDS") == null)
            {
                Help cmdHelp = CreateCommandsHelp();
                help_.Add(cmdHelp);
                cmdHelp.Save();
            }
        }

        static void SeedDefaultHelp()
        {
            List<Help> defaults = new List<Help>();
            defaults.Add(new Help("NEWBIE",
                "Welcome to UltiMUD!\r\n\r\n" +
                "Essential commands to get started:\r\n" +
                "  LOOK (L)          - View your surroundings and nearby map.\r\n" +
                "  N, S, E, W        - Move North, South, East, or West.\r\n" +
                "  NE, NW, SE, SW    - Move diagonally.\r\n" +
                "  TRAVEL <target>   - Travel continuously in a direction or toward a target.\r\n" +
                "  STOP              - Stop current movement or action.\r\n" +
                "  STATS (SCORE)     - View your character's attributes, health, and gold.\r\n" +
                "  INVENTORY (I)     - View items you are carrying.\r\n" +
                "  EQUIPMENT (EQ)    - View items you are currently wearing or wielding.\r\n" +
                "  WHO               - See all players currently online.\r\n" +
                "  SAY <message>     - Speak to others in your room.\r\n" +
                "  CHAT <message>    - Speak on the global chat channel.\r\n" +
                "  COMMANDS          - List all available commands.\r\n" +
                "  HELP <topic>      - View detailed help on any topic."));

            defaults.Add(new Help("MOVEMENT",
                "Movement Commands:\r\n" +
                "  NORTH (N), SOUTH (S), EAST (E), WEST (W)\r\n" +
                "  NORTHEAST (NE), NORTHWEST (NW), SOUTHEAST (SE), SOUTHWEST (SW)\r\n" +
                "  TRAVEL <dir|target> [speed] - Travel toward a direction or visible object.\r\n" +
                "  STOP                        - Stop travelling or performing an action.\r\n" +
                "  ENTER [portal]              - Enter a portal, building, or dungeon entrance.\r\n" +
                "  FOLLOW <target>             - Follow another character or path.\r\n" +
                "  DITCH [target]              - Stop a follower from following you.\r\n" +
                "  STAND, SIT, REST, SLEEP     - Change your character's position."));

            defaults.Add(new Help("COMBAT",
                "Combat & Abilities:\r\n" +
                "  ABILITIES (SKILLS)  - List all abilities and skills your character knows.\r\n" +
                "  STATUS (AFFECTS)    - View active status effects and their durations.\r\n" +
                "  LEARN               - Learn a new ability from a teacher in the room.\r\n" +
                "  USE <item>          - Equip a weapon/armor or consume an edible item.\r\n" +
                "  REMOVE <item>       - Unequip an item you are wearing.\r\n" +
                "  GROUP               - View your current party/followers."));

            defaults.Add(new Help("COMMUNICATION",
                "Communication Commands:\r\n" +
                "  SAY <msg> (' <msg>)       - Speak to everyone at your location.\r\n" +
                "  CHAT <msg> (OOC <msg>)    - Broadcast a message to all players.\r\n" +
                "  TELL <player> <msg>       - Send a private message to a player.\r\n" +
                "  REPLY <msg>               - Reply to the last player who sent you a tell.\r\n" +
                "  WHISPER <target> <msg>    - Whisper privately to someone in the same room.\r\n" +
                "  EMOTE <action> (: <act>)  - Perform a custom roleplay action.\r\n" +
                "  CHANNELS                  - List communication channels.\r\n" +
                "  SOCIALS                   - List available social emotes.\r\n" +
                "  FORUMS                    - Read and post messages on message boards."));

            defaults.Add(new Help("ITEMS",
                "Item & Shop Commands:\r\n" +
                "  GET <item> [container]    - Pick up an item from the ground or a container.\r\n" +
                "  PUT <item> <container>    - Place an item into a container.\r\n" +
                "  DROP <item|amount>        - Drop an item or pile of money on the ground.\r\n" +
                "  GIVE <item> <target>      - Give an item to another character.\r\n" +
                "  GIVEMONEY <target> <amt>  - Give money to another character.\r\n" +
                "  EXAMINE <target>          - Closely inspect an item, container, or character.\r\n" +
                "  OPEN / CLOSE <object>     - Open or close a door or container.\r\n" +
                "  LOCK / UNLOCK <object>    - Lock or unlock a door or container with a key.\r\n" +
                "  LIST (SHOP)               - View items for sale at a shop.\r\n" +
                "  BUY <item>                - Purchase an item from a shop.\r\n" +
                "  SELL <item>               - Sell an item from your inventory to a shop.\r\n" +
                "  EVALUATE <item>           - Check how much a shop will pay for an item."));

            defaults.Add(CreateCommandsHelp());

            foreach (Help h in defaults)
            {
                help_.Add(h);
                h.Save();
            }
        }

        static Help CreateCommandsHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available MUD Commands:");
            int count = 0;
            foreach (Command c in Command.Commands)
            {
                if (!c.SystemCommand && !c.Hidden && !string.IsNullOrEmpty(c.Name))
                {
                    sb.AppendFormat("{0,-16}", c.Name);
                    if (++count % 4 == 0)
                        sb.AppendLine();
                }
            }
            return new Help("COMMANDS", sb.ToString().TrimEnd('\r', '\n'));
        }
        
        //Deprecated.
        [Obfuscation(Exclude = false)]
        public static void LoadOldHelpFile(string path)
        {
            if (!string.IsNullOrEmpty(path) && path.Contains("\\muds"))
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
