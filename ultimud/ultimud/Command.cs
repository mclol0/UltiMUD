using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using LuaInterface;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Command : ISaveable
    {
        static List<Command> commands_ = new List<Command>();
        static Dictionary<Guid, Guid> lastTellFrom_ = new Dictionary<Guid, Guid>();
        string name_;
        string script_;
        bool incapacitated_ = true;
        bool system_ = false;
        List<MenuFunction> menufunctions_ = new List<MenuFunction>();
        uint priority_;
        bool hidden_;
        List<string> aliases_ = new List<string>();

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Server.InputHandler[] menuhandlers_;

        #region Properties
        public static List<Command> Commands
        {
            get { return commands_; }
            set { commands_ = value; }
        }
        static string Path
        {
            get
            {
                if (Server.Instance != null)
                    return Server.Instance.ServerPath + "\\Commands";
                if (MainForm.Instance != null)
                    return MainForm.Instance.ServerPath + "\\Commands";
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\muds\\Unnamed\\Commands");
            }
        }
        public string Name
        {
            get { return this.name_; }
            set 
            {
                if (!string.IsNullOrEmpty(this.name_) && this.name_ != value)
                    this.Delete();
                this.name_ = value;
            }
        }

        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string Script
        {
            get { return this.script_; }
            set { this.script_ = value; }
        }

        [DefaultValue(true)]
        [Description("Can this ability be used if unconscious or dead?")]
        public bool CanUseIncapacitated
        {
            get { return this.incapacitated_; }
            set { this.incapacitated_ = value; }
        }
        
        [DefaultValue(false)]
        [Description("If true, this command cannot be used by players. It can only be invoked through DoCommand().")]
        public bool SystemCommand
        {
            get { return this.system_; }
            set { this.system_ = value; }
        }
        
        [TypeConverter(typeof(Damdai.TypeConverters.ListToStringConverter))]
        [Description("These menu functions are called for each argument required for this command.")]
        public List<MenuFunction> MenuFunctions
        {
            get { return this.menufunctions_; }
            set { this.menufunctions_ = value; }
        }

        [DefaultValue(0)]
        [Description("The priority of this command when more than 1 command is matched by user input. (0 = no priority, 1 = top priority)")]
        public uint Priority
        {
            get { return this.priority_; }
            set { this.priority_ = value; }
        }
        
        [Description("This property can be used in your COMMANDS script to determine if it will appear in the command list.")]
        [DefaultValue(false)]
        public bool Hidden
        {
            get { return this.hidden_; }
            set { this.hidden_ = value; }
        }
        
        [Description("This is a list of aliases that a user can enter to invoke this command instead of the command name. (Example: name=northeast, alias=ne)")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.ListToStringConverter))]
        public List<string> Aliases
        {
            get { return this.aliases_; }
            set { this.aliases_ = value; }
        }
        #endregion

        internal static void Init(string loadpath, string serverpath)
        {
            lock (commands_)
            {
                commands_.Clear();
                string cmdspath = !string.IsNullOrEmpty(loadpath) ? (loadpath + "\\Commands") : null;
                if (!string.IsNullOrEmpty(cmdspath) && Directory.Exists(cmdspath))
                {
                    foreach (FileInfo file in new DirectoryInfo(cmdspath).GetFiles("*.cmd"))
                    {
                        Command cmd = Damdai.Serialization.Xml.Deserialize<Command>(file.FullName);
                        commands_.Add(cmd);
                    }
                }
                else if (!string.IsNullOrEmpty(serverpath) && Directory.Exists(serverpath + "\\Commands"))
                {
                    foreach (FileInfo file in new DirectoryInfo(serverpath + "\\Commands").GetFiles("*.cmd"))
                    {
                        Command cmd = Damdai.Serialization.Xml.Deserialize<Command>(file.FullName);
                        commands_.Add(cmd);
                    }
                }

                if (commands_.Count == 0)
                {
                    SeedDefaultCommands(serverpath);
                }

                commands_.Sort(delegate(Command cmd1, Command cmd2)
                {
                    return cmd1.priority_.CompareTo(cmd2.priority_);
                });
            }
        }

        static Command CreateDefaultCommand(string name, string methodName, bool canUseIncapacitated, uint priority, bool sysCmd, bool hidden, params string[] aliases)
        {
            Command cmd = new Command();
            cmd.name_ = name;
            cmd.incapacitated_ = canUseIncapacitated;
            cmd.priority_ = priority;
            cmd.system_ = sysCmd;
            cmd.hidden_ = hidden;
            if (aliases != null && aliases.Length > 0)
                cmd.aliases_.AddRange(aliases);
            cmd.script_ = "luanet.load_assembly('Ultimud')\r\nCommand = luanet.import_type('Ultimud.Command')\r\nfinished = Command." + methodName + "(user)\r\nreturn finished";
            cmd.Init();
            return cmd;
        }

        static void SeedDefaultCommands(string serverpath)
        {
            List<Command> list = new List<Command>();

            // Movement & Observation
            list.Add(CreateDefaultCommand("look", "DoLook", false, 1, false, false, "l"));
            list.Add(CreateDefaultCommand("north", "DoNorth", false, 1, false, false, "n"));
            list.Add(CreateDefaultCommand("south", "DoSouth", false, 1, false, false, "s"));
            list.Add(CreateDefaultCommand("east", "DoEast", false, 1, false, false, "e"));
            list.Add(CreateDefaultCommand("west", "DoWest", false, 1, false, false, "w"));
            list.Add(CreateDefaultCommand("northeast", "DoNortheast", false, 0, false, false, "ne"));
            list.Add(CreateDefaultCommand("northwest", "DoNorthwest", false, 0, false, false, "nw"));
            list.Add(CreateDefaultCommand("southeast", "DoSoutheast", false, 0, false, false, "se"));
            list.Add(CreateDefaultCommand("southwest", "DoSouthwest", false, 0, false, false, "sw"));
            list.Add(CreateDefaultCommand("travel", "DoTravel", false, 0, false, false, "move", "go"));
            list.Add(CreateDefaultCommand("stop", "DoStop", false, 0, false, false));
            list.Add(CreateDefaultCommand("enter", "DoEnter", false, 0, false, false));
            list.Add(CreateDefaultCommand("follow", "DoFollow", false, 0, false, false));
            list.Add(CreateDefaultCommand("ditch", "DoDitch", false, 0, false, false));
            list.Add(CreateDefaultCommand("group", "DoGroup", true, 0, false, false, "party"));
            list.Add(CreateDefaultCommand("order", "DoOrder", false, 0, false, false));

            // Positions
            list.Add(CreateDefaultCommand("position", "DoPosition", true, 0, false, false));
            list.Add(CreateDefaultCommand("stand", "DoStand", true, 0, false, false));
            list.Add(CreateDefaultCommand("sit", "DoSit", true, 0, false, false));
            list.Add(CreateDefaultCommand("rest", "DoRest", true, 0, false, false));
            list.Add(CreateDefaultCommand("sleep", "DoSleep", true, 0, false, false));

            // Communication
            list.Add(CreateDefaultCommand("who", "DoWho", true, 1, false, false));
            list.Add(CreateDefaultCommand("say", "DoSay", false, 1, false, false, "'"));
            list.Add(CreateDefaultCommand("chat", "DoChat", true, 0, false, false, "ooc", "gossip"));
            list.Add(CreateDefaultCommand("tell", "DoTell", true, 0, false, false));
            list.Add(CreateDefaultCommand("reply", "DoReply", true, 0, false, false));
            list.Add(CreateDefaultCommand("whisper", "DoWhisper", false, 0, false, false));
            list.Add(CreateDefaultCommand("emote", "DoEmote", false, 0, false, false, ":"));
            list.Add(CreateDefaultCommand("channels", "DoChannels", true, 0, false, false));
            list.Add(CreateDefaultCommand("socials", "DoSocials", true, 0, false, false));
            list.Add(CreateDefaultCommand("forums", "DoForums", true, 0, false, false, "board", "boards"));

            // Character Info & Inventory
            list.Add(CreateDefaultCommand("stats", "DoStats", true, 1, false, false, "score", "sc"));
            list.Add(CreateDefaultCommand("inventory", "DoInventory", true, 1, false, false, "i", "inv"));
            list.Add(CreateDefaultCommand("equipment", "DoEquipment", true, 1, false, false, "eq"));
            list.Add(CreateDefaultCommand("abilities", "DoAbilities", true, 0, false, false, "skills", "spells"));
            list.Add(CreateDefaultCommand("status", "DoStatus", true, 0, false, false, "affects"));
            list.Add(CreateDefaultCommand("money", "DoMoney", true, 0, false, false, "gold"));

            // Items & Containers
            list.Add(CreateDefaultCommand("get", "DoGet", false, 1, false, false, "take", "pickup"));
            list.Add(CreateDefaultCommand("put", "DoPut", false, 0, false, false));
            list.Add(CreateDefaultCommand("drop", "DoDrop", false, 0, false, false));
            list.Add(CreateDefaultCommand("use", "DoUse", false, 0, false, false, "wear", "wield", "equip", "eat"));
            list.Add(CreateDefaultCommand("remove", "DoRemove", false, 0, false, false, "unequip"));
            list.Add(CreateDefaultCommand("give", "DoGive", false, 0, false, false));
            list.Add(CreateDefaultCommand("givemoney", "DoGiveMoney", false, 0, false, false));
            list.Add(CreateDefaultCommand("destroy", "DoDestroyItem", false, 0, false, false));
            list.Add(CreateDefaultCommand("examine", "DoExamine", false, 0, false, false, "exa", "lookat"));
            list.Add(CreateDefaultCommand("open", "DoOpen", false, 0, false, false));
            list.Add(CreateDefaultCommand("close", "DoClose", false, 0, false, false));
            list.Add(CreateDefaultCommand("lock", "DoLock", false, 0, false, false));
            list.Add(CreateDefaultCommand("unlock", "DoUnlock", false, 0, false, false));

            // Shops & Training
            list.Add(CreateDefaultCommand("list", "DoShopList", false, 0, false, false, "shop"));
            list.Add(CreateDefaultCommand("buy", "DoBuy", false, 0, false, false));
            list.Add(CreateDefaultCommand("sell", "DoSell", false, 0, false, false));
            list.Add(CreateDefaultCommand("evaluate", "DoEvaluate", false, 0, false, false, "value"));
            list.Add(CreateDefaultCommand("pay", "DoGiveMoney", false, 0, false, false));
            list.Add(CreateDefaultCommand("learn", "DoLearn", false, 0, false, false));
            list.Add(CreateDefaultCommand("teach", "DoTeachPlayer", false, 0, false, false));

            // World & System
            list.Add(CreateDefaultCommand("time", "DoTime", true, 0, false, false));
            list.Add(CreateDefaultCommand("weather", "DoWeather", true, 0, false, false));
            list.Add(CreateDefaultCommand("set", "DoSet", true, 0, false, false, "settings"));
            list.Add(CreateDefaultCommand("alias", "DoAlias", true, 0, false, false, "aliases"));
            list.Add(CreateDefaultCommand("commands", "DoCommands", true, 0, false, false));
            list.Add(CreateDefaultCommand("help", "DoHelp", true, 1, false, false, "?"));
            list.Add(CreateDefaultCommand("save", "DoSave", true, 0, false, false));
            list.Add(CreateDefaultCommand("password", "DoPassword", true, 0, false, false));
            list.Add(CreateDefaultCommand("delete", "DoDelete", true, 0, false, false));
            list.Add(CreateDefaultCommand("quit", "DoQuit", true, 0, false, false));

            // Script Commands
            list.Add(CreateDefaultCommand("peace", "DoPeace", true, 0, true, true));
            list.Add(CreateDefaultCommand("force", "DoForce", true, 0, true, true));
            list.Add(CreateDefaultCommand("createitem", "DoCreateItem", true, 0, true, true));
            list.Add(CreateDefaultCommand("print", "DoPrint", true, 0, true, true));
            list.Add(CreateDefaultCommand("summonnpc", "DoSummonNpc", true, 0, true, true));

            string saveDir = !string.IsNullOrEmpty(serverpath) ? (serverpath + "\\Commands") : Path;
            try
            {
                if (!string.IsNullOrEmpty(saveDir))
                    Directory.CreateDirectory(saveDir);
            }
            catch { }

            foreach (Command c in list)
            {
                commands_.Add(c);
                try
                {
                    if (!string.IsNullOrEmpty(saveDir))
                        Damdai.Serialization.Xml.Serialize(c, saveDir + "\\" + c.name_ + ".cmd");
                }
                catch { }
            }
        }

        [Obfuscation(Exclude = false)]
        public static Command Get(string name, bool includesyscommands)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            Command cmd = null;
            lock (commands_)
            {
                foreach (Command c in Command.Commands)
                {
                    if ((!c.SystemCommand || includesyscommands) && !string.IsNullOrEmpty(c.name_) &&
                        (c.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase) ||
                         (c.aliases_ != null && c.aliases_.Exists(delegate(string a) { return a.Equals(name, StringComparison.CurrentCultureIgnoreCase); }))))
                    {
                        if (c.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) ||
                            (c.aliases_ != null && c.aliases_.Exists(delegate(string a) { return a.Equals(name, StringComparison.CurrentCultureIgnoreCase); })))
                        {
                            return c;
                        }
                        if (c.priority_ == 1)
                            return c;
                        else if (cmd == null || cmd.priority_ == 0 || (c.priority_ > 0 && c.priority_ < cmd.priority_))
                            cmd = c;
                    }
                }
            }
            return cmd;
        }

        [Obfuscation(Exclude = false)]
        public static bool GetArguments(User user, params Server.InputHandler[] handlers)
        {
            if (user.CurrentCommandArgs.Count != handlers.Length)
                foreach (Server.InputHandler handler in handlers)
                    handler(user);

            if (user.CurrentCommandArgs.Count >= handlers.Length)
                return true;

            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public bool DoScript(User user)
        {
            lock (this)
            {
                if (this.lua_ == null || this.menuhandlers_ == null)
                    this.Init();

                if (this.menuhandlers_.Length == 0 || Ultimud.Command.GetArguments(user, this.menuhandlers_))
                {
                    try
                    {
                        this.lua_["user"] = user;
                        this.lua_["finished"] = null;
                        object[] rvals = this.lua_.DoString(this.script_);
                        object fin = this.lua_["finished"];
                        if (fin is bool)
                            return (bool)fin;
                        if (rvals != null && rvals.Length > 0 && rvals[0] is bool)
                            return (bool)rvals[0];
                        return (user.Menus.Count == 0 && (user.InputHandler == null || user.InputHandler == this.DoScript));
                    }
                    catch (AbortCommandException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Command: " + this.Name + ": " + e.Message, "LUA");
#if DEBUG
                        user.Send(e.Message);
#endif
                    }
                    return true;
                }
            }

            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public void Save()
        {
            if (!string.IsNullOrEmpty(this.name_))
            {
                Directory.CreateDirectory(Path);
                Damdai.Serialization.Xml.Serialize(this, Path + "\\" + this.name_ + ".cmd");
                this.Init();
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void Delete()
        {
            if (!string.IsNullOrEmpty(this.name_))
            {
                string file = Path + "\\" + this.name_ + ".cmd";
                if (File.Exists(file))
                    File.Delete(file);
            }
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if(string.IsNullOrEmpty(this.name_))
                return base.ToString();
            else
                return this.name_;
        }

        [Obfuscation(Exclude = false)]
        void Init()
        {
            lock (this)
            {
                List<Server.InputHandler> list = new List<Server.InputHandler>();
                foreach (MenuFunction menufn in this.menufunctions_)
                    if (!string.IsNullOrEmpty(menufn.Function) && menufn.Handler != null)
                        list.Add(menufn.Handler);
                this.menuhandlers_ = list.ToArray();
                this.lua_ = new Lua();
            }
        }

        #region Built-In Command Implementations
        public static bool DoLook(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                user.Send(user.Character.GenerateLookDescription(false));
                return true;
            }
            return DoExamine(user);
        }

        public static bool DoExamine(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Send(user.Character.GenerateLookDescription(false));
                return true;
            }
            if (arg.Equals("in", StringComparison.OrdinalIgnoreCase))
            {
                string nextArg = user.GetArgument();
                if (!string.IsNullOrEmpty(nextArg))
                    arg = nextArg;
            }

            Object target = user.Character.FindItemInInventory(arg);
            if (target == null)
                target = user.Character.GetEquippedItem(arg);
            if (target == null)
                target = user.Character.FindNearby<Object>(arg, 0, true);
            if (target == null)
                target = user.Character.FindNearby<Object>(arg, 5, true);

            if (target == null)
            {
                user.Send("You don't see '" + arg + "' here.");
                return true;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{color:brightcyan}" + target.Name + "{color:reset}");
            if (!string.IsNullOrEmpty(target.Description))
                sb.AppendLine(target.Description);

            if (target is Character)
            {
                Character ch = (Character)target;
                sb.AppendLine(ch.Name + " is " + ch.Position.ToString().ToLower() + ".");
                if (ch.Equipment != null && ch.Equipment.Count > 0)
                {
                    sb.AppendLine("\r\nEquipment:");
                    sb.AppendLine(Server.Instance.Settings.EqFormat.ToString(null, ch));
                }
            }
            else if (target is IContainer && target.ObjectFlags.Get((int)Object.ObjFlags.Container))
            {
                IContainer container = (IContainer)target;
                if (container.Closed)
                {
                    sb.AppendLine("It is closed.");
                }
                else
                {
                    List<Item> contents = target.GetChildren<Item>();
                    if (contents.Count == 0)
                        sb.AppendLine("It is empty.");
                    else
                    {
                        sb.AppendLine("Contents:");
                        foreach (Item item in contents)
                            sb.AppendLine("  " + item.Name);
                    }
                }
            }

            user.Send(sb.ToString().TrimEnd('\r', '\n'));
            return true;
        }

        static bool MoveInDirection(User user, Area.Direction dir)
        {
            if (user.Character.Position != Object.Positions.Stopped &&
                user.Character.Position != Object.Positions.Moving)
            {
                user.Send("You must stand up first!");
                return true;
            }

            if (user.Character.CurrentAction != null)
                user.Character.Stop(false);

            bool moved = user.Character.Move(
                dir,
                new String("You move {dir}.", null, null, null, "{1} leaves {dir}.", null),
                new String(null, null, null, null, "{1} arrives from the {dir}.", null),
                false);

            if (!moved)
            {
                user.Send("You cannot move " + dir.ToString().ToLower() + ".");
            }
            else
            {
                user.Send(user.Character.GenerateLookDescription(!user.Flags.Get((int)User.UserFlags.AutoMap)));
            }
            return true;
        }

        public static bool DoNorth(User user) { return MoveInDirection(user, Area.Direction.North); }
        public static bool DoSouth(User user) { return MoveInDirection(user, Area.Direction.South); }
        public static bool DoEast(User user) { return MoveInDirection(user, Area.Direction.East); }
        public static bool DoWest(User user) { return MoveInDirection(user, Area.Direction.West); }
        public static bool DoNortheast(User user) { return MoveInDirection(user, Area.Direction.Northeast); }
        public static bool DoNorthwest(User user) { return MoveInDirection(user, Area.Direction.Northwest); }
        public static bool DoSoutheast(User user) { return MoveInDirection(user, Area.Direction.Southeast); }
        public static bool DoSouthwest(User user) { return MoveInDirection(user, Area.Direction.Southwest); }

        public static bool DoTravel(User user)
        {
            if (user.Character.CurrentAction != null)
                user.Character.Stop(false);

            if (GetArguments(user, User.SelectDestinationOrDirection, User.SelectSpeed))
            {
                object arg1 = user.GetArg(0);
                int speed = 1;
                if (user.CurrentCommandArgs.Count > 1 && user.GetArg(1) is int)
                    speed = (int)user.GetArg(1);

                Stat speedStat = (Server.Instance.Settings.SpeedStat != Guid.Empty ? user.Character.GetStat(Server.Instance.Settings.SpeedStat) : null) ?? user.Character.GetStat("speed");
                int maxSpeed = (speedStat != null ? (int)speedStat.Current : 5);
                speed = Math.Max(1, Math.Min(speed, Math.Max(1, maxSpeed)));

                if (arg1 is Area.Direction || arg1 is Area.ShortDirection)
                {
                    Area.Direction dir = (Area.Direction)arg1;
                    if (Server.Instance.Settings.SimpleMovement)
                        return MoveInDirection(user, dir);
                    user.Character.ReplaceCurrentAction(new TravelAction(user.Character, dir, speed));
                    return true;
                }
                else if (arg1 is Object)
                {
                    Object target = (Object)arg1;
                    if (target.Distance(user.Character) > 0)
                    {
                        user.Character.ReplaceCurrentAction(new TravelAction(user.Character, target, speed));
                        return true;
                    }
                }

                user.Send("Invalid destination.");
                return true;
            }
            return false;
        }

        public static bool DoStop(User user)
        {
            user.Character.Stop(false);
            return true;
        }

        public static bool DoEnter(User user)
        {
            if (user.Character.Parent is Vehicle && string.IsNullOrEmpty(user.Command))
            {
                user.Character.Enter((Vehicle)user.Character.Parent);
                user.Send(user.Character.GenerateLookDescription(!user.Flags.Get((int)User.UserFlags.AutoMap)));
                return true;
            }
            if (!string.IsNullOrEmpty(user.Command))
            {
                Vehicle veh = user.Character.FindNearby<Vehicle>(user.Command.Trim(), 0, true);
                if (veh != null)
                {
                    user.Command = null;
                    user.Character.Enter(veh);
                    return true;
                }
            }
            if (GetArguments(user, User.SelectPortal))
            {
                IPortal portal = user.GetCommandArg<IPortal>(0);
                if (portal != null)
                {
                    user.Character.Enter(portal);
                    user.Send(user.Character.GenerateLookDescription(!user.Flags.Get((int)User.UserFlags.AutoMap)));
                }
                else
                {
                    user.Send("Invalid entrance.");
                }
                return true;
            }
            return false;
        }

        public static bool DoFollow(User user)
        {
            string arg1 = user.GetArgument();
            if (string.IsNullOrEmpty(arg1))
            {
                if (user.Character.Master != null)
                {
                    user.Send("You stop following " + user.Character.Master.Name + ".");
                    user.Character.Master = null;
                    user.Character.Stop(false);
                }
                else
                {
                    user.Send("Follow whom? Usage: follow <target>");
                }
                return true;
            }

            if (arg1.Equals("self", StringComparison.OrdinalIgnoreCase) || arg1.Equals(user.Character.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (user.Character.Master != null)
                {
                    user.Send("You stop following " + user.Character.Master.Name + ".");
                    user.Character.Master = null;
                }
                user.Character.Stop(false);
                return true;
            }

            Object o = user.Character.FindNearby<Object>(arg1, 0, true);
            if (o == null)
            {
                user.Send("Target not found: " + arg1);
                return true;
            }

            int speed = 5;
            Stat speedStat = user.Character.GetStat("speed");
            if (speedStat != null)
                speed = Math.Max(1, (int)speedStat.Current);

            if (o is Character)
            {
                Character targetChar = (Character)o;
                user.Character.Master = targetChar;
                user.Character.Area.Send(new String(
                    "You start following {2}.", "You start following {2}.",
                    "{1} starts following you.", "{1} starts following you.",
                    "{1} starts following {2}.", null), user.Character, targetChar, 0);
            }
            user.Character.ReplaceCurrentAction(new FollowAction(user.Character, o, 0, speed));
            return true;
        }

        public static bool DoDitch(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                int count = 0;
                foreach (Character ch in user.Character.NearbyObjects<Character>(5, true, user.Character))
                {
                    if (ch.Master == user.Character)
                    {
                        ch.Master = null;
                        ch.Stop(false);
                        ch.Send(user.Character.Name + " ditches you.");
                        count++;
                    }
                }
                user.Send(count > 0 ? "You ditch your followers." : "No one is following you.");
                return true;
            }

            Character target = user.Character.FindNearby<Character>(arg, 5, true);
            if (target != null && target.Master == user.Character)
            {
                target.Master = null;
                target.Stop(false);
                user.Send("You ditch " + target.Name + ".");
                target.Send(user.Character.Name + " ditches you.");
            }
            else
            {
                user.Send("They aren't following you.");
            }
            return true;
        }

        public static bool DoGroup(User user)
        {
            Character leader = user.Character.Master ?? user.Character;
            StringTable table = new StringTable("Member", "Role", "Position");
            table.AddRow(leader.Name, "Leader", leader.Position.ToString());
            if (user.Character.Area != null)
            {
                foreach (Character ch in user.Character.Area.GetChildren<Character>())
                {
                    if (ch != leader && ch.Master == leader)
                        table.AddRow(ch.Name, "Follower", ch.Position.ToString());
                }
            }
            user.Send(table.ToString());
            return true;
        }

        public static bool DoOrder(User user)
        {
            if (GetArguments(user, User.SelectCharacterRoom, User.EnterMessage))
            {
                Character target = user.GetCommandArg<Character>(0);
                string cmd = user.GetCommandArg<string>(1);
                if (target == null || target == user.Character)
                    user.Send("Invalid target.");
                else if (target.Master != user.Character)
                    user.Send("They aren't under your control.");
                else
                {
                    user.Send("You order " + target.Name + " to '" + cmd + "'.");
                    target.DoCommand(cmd);
                }
                return true;
            }
            return false;
        }

        public static bool DoPosition(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Send("You are currently " + user.Character.Position.ToString().ToLower() + ". Usage: stand, sit, rest, sleep");
                return true;
            }
            if ("stand".StartsWith(arg, StringComparison.OrdinalIgnoreCase) || "standing".StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                return DoStand(user);
            if ("sit".StartsWith(arg, StringComparison.OrdinalIgnoreCase) || "sitting".StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                return DoSit(user);
            if ("rest".StartsWith(arg, StringComparison.OrdinalIgnoreCase) || "resting".StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                return DoRest(user);
            if ("sleep".StartsWith(arg, StringComparison.OrdinalIgnoreCase) || "sleeping".StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                return DoSleep(user);
            user.Send("Invalid position: " + arg);
            return true;
        }

        public static bool DoStand(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            if (user.Character.Position == Object.Positions.Stopped)
            {
                user.Send("You are already standing.");
                return true;
            }
            user.Character.Position = Object.Positions.Stopped;
            user.Character.Area.Send(new String("You stand up.", null, null, null, "{1} stands up.", null), user.Character, null, 0);
            return true;
        }

        public static bool DoSit(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            if (user.Character.Position == Object.Positions.Sitting)
            {
                user.Send("You are already sitting.");
                return true;
            }
            user.Character.Stop(true);
            user.Character.Position = Object.Positions.Sitting;
            user.Character.Area.Send(new String("You sit down.", null, null, null, "{1} sits down.", null), user.Character, null, 0);
            return true;
        }

        public static bool DoRest(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            if (user.Character.Position == Object.Positions.Resting)
            {
                user.Send("You are already resting.");
                return true;
            }
            user.Character.Stop(true);
            user.Character.Position = Object.Positions.Resting;
            user.Character.Area.Send(new String("You sit down and rest.", null, null, null, "{1} sits down and rests.", null), user.Character, null, 0);
            return true;
        }

        public static bool DoSleep(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            if (user.Character.Position == Object.Positions.Sleeping)
            {
                user.Send("You are already asleep.");
                return true;
            }
            user.Character.Stop(true);
            user.Character.Position = Object.Positions.Sleeping;
            user.Character.Area.Send(new String("You lie down and go to sleep.", null, null, null, "{1} lies down and goes to sleep.", null), user.Character, null, 0);
            return true;
        }

        public static bool DoWho(User user)
        {
            user.Send(Server.Instance.Settings.WhoFormat.ToString(user.GetArgument(), user));
            return true;
        }

        public static bool DoSay(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Say what?");
                return true;
            }
            if (user.Flags.Get((int)User.UserFlags.Silenced))
            {
                user.Send("You are silenced.");
                return true;
            }
            String str = new String(
                "{color:brightwhite}You say, \"" + msg + "\"{color:reset}", null,
                null, null,
                "{color:brightwhite}{1} says, \"" + msg + "\"{color:reset}", null);
            user.Character.Area.Send(str, user.Character, null, 0, new Damdai.Function<bool, IObject>[] { Object.CanHear });
            return true;
        }

        public static bool DoChat(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Chat what?");
                return true;
            }
            if (user.Flags.Get((int)User.UserFlags.Silenced))
            {
                user.Send("You are silenced.");
                return true;
            }
            user.Send("{color:brightcyan}[CHAT] You: " + msg + "{color:reset}");
            Server.Instance.Echo("{color:brightcyan}[CHAT] " + user.Character.Name + ": " + msg + "{color:reset}", user.Character);
            return true;
        }

        public static bool DoTell(User user)
        {
            string targetName = user.GetArgument();
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(msg))
            {
                user.Send("Usage: tell <player> <message>");
                return true;
            }
            if (user.Flags.Get((int)User.UserFlags.Silenced))
            {
                user.Send("You are silenced.");
                return true;
            }

            User targetUser = null;
            foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
            {
                if (u != null && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != null &&
                    u.Character.Name.StartsWith(targetName, StringComparison.CurrentCultureIgnoreCase))
                {
                    targetUser = u;
                    break;
                }
            }

            if (targetUser == null)
            {
                user.Send("No player by that name is online.");
                return true;
            }

            lock (lastTellFrom_)
                lastTellFrom_[targetUser.Id] = user.Id;

            user.Send("{color:brightmagenta}You tell " + targetUser.Character.Name + ", \"" + msg + "\"{color:reset}");
            targetUser.Send("{color:brightmagenta}" + user.Character.Name + " tells you, \"" + msg + "\"{color:reset}");
            return true;
        }

        public static bool DoReply(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Reply what?");
                return true;
            }

            Guid senderId = Guid.Empty;
            lock (lastTellFrom_)
                lastTellFrom_.TryGetValue(user.Id, out senderId);

            if (senderId == Guid.Empty)
            {
                user.Send("No one has sent you a tell recently.");
                return true;
            }

            User targetUser = null;
            foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
            {
                if (u != null && u.Id == senderId && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != null)
                {
                    targetUser = u;
                    break;
                }
            }

            if (targetUser == null)
            {
                user.Send("They are no longer online.");
                return true;
            }

            lock (lastTellFrom_)
                lastTellFrom_[targetUser.Id] = user.Id;

            user.Send("{color:brightmagenta}You tell " + targetUser.Character.Name + ", \"" + msg + "\"{color:reset}");
            targetUser.Send("{color:brightmagenta}" + user.Character.Name + " tells you, \"" + msg + "\"{color:reset}");
            return true;
        }

        public static bool DoWhisper(User user)
        {
            string targetName = user.GetArgument();
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(msg))
            {
                user.Send("Usage: whisper <target> <message>");
                return true;
            }

            Character target = user.Character.FindNearby<Character>(targetName, 0, true);
            if (target == null)
            {
                user.Send("They aren't here.");
                return true;
            }

            String str = new String(
                "{color:magenta}You whisper to {2}, \"" + msg + "\"{color:reset}",
                "{color:magenta}You whisper to {2}, \"" + msg + "\"{color:reset}",
                "{color:magenta}{1} whispers to you, \"" + msg + "\"{color:reset}",
                "{color:magenta}{1} whispers to you, \"" + msg + "\"{color:reset}",
                "{1} whispers something quietly to {2}.", null);
            user.Character.Area.Send(str, user.Character, target, 0, new Damdai.Function<bool, IObject>[] { Object.CanHear });
            return true;
        }

        public static bool DoEmote(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Emote what?");
                return true;
            }
            String str = new String(
                user.Character.Name + " " + msg, null,
                null, null,
                "{1} " + msg, null);
            user.Character.Area.Send(str, user.Character, null, 0);
            return true;
        }

        public static bool DoStats(User user)
        {
            user.Send(Server.Instance.Settings.StatsFormat.ToString(user.GetArgument(), user));
            return true;
        }

        public static bool DoInventory(User user)
        {
            List<Item> list = user.Character.Inventory;
            string numFmt = Server.Instance.Settings.NumberFormat;
            if (list.Count < 1)
            {
                user.Send("Your inventory is empty.\r\nGold: " + user.Character.Value.ToString(numFmt) + "\r\nYou are currently using " + user.Character.Equipment.Count + " item(s).");
                return true;
            }

            Dictionary<string, List<Item>> dictionary = Utils.ConsolidateList<Item>(list);
            Menu menu = new Menu("You are carrying (Gold: " + user.Character.Value.ToString(numFmt) + "):");
            foreach (List<Item> items in dictionary.Values)
                menu.AddOption(new Menu.Option(items[0].Name + (items.Count > 1 ? " (" + items.Count + ")" : ""), null));

            user.Send(menu.ToString());
            return true;
        }

        public static bool DoEquipment(User user)
        {
            if (user.Character.Equipment.Count == 0)
            {
                user.Send("You aren't using anything.");
                return true;
            }
            user.Send(Server.Instance.Settings.EqFormat.ToString(user.GetArgument(), user.Character));
            return true;
        }

        public static bool DoAbilities(User user)
        {
            if (user.Character.Abilities.Count < 1)
            {
                user.Send("You have no abilities.");
                return true;
            }
            user.Send(Server.Instance.Settings.AbilitiesFormat.ToString(user.GetArgument(), user));
            return true;
        }

        public static bool DoStatus(User user)
        {
            bool found = false;
            StringTable table = new StringTable("Status", "Duration");
            table.Columns[1].Alignment = StringTable.Column.Align.Right;
            foreach (StatusEffect status in user.Character.StatusEffects)
            {
                StatusEffectBlueprint bp = status.Blueprint;
                if (bp != null && bp.Visible)
                {
                    table.AddRow(bp.Name, (status.IsPermanent || status.Duration == null ? "Permanent" : status.SecondsRemaining.ToString() + "s"));
                    found = true;
                }
            }

            if (found)
                user.Send(table.ToString());
            else
                user.Send("You aren't affected by any status effects.");
            return true;
        }

        public static bool DoMoney(User user)
        {
            user.Send("You have $" + user.Character.Value.ToString(Server.Instance.Settings.NumberFormat) + ".");
            return true;
        }

        public static bool DoGet(User user)
        {
            if (user.CurrentCommandArgs.Count == 0 && !string.IsNullOrEmpty(user.Command))
            {
                string firstArg = user.GetArgument();
                string secondArg = user.GetArgument();
                if (firstArg.Equals("from", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(secondArg))
                {
                    user.Command = secondArg + " " + user.Command;
                    return DoGetFromContainer(user);
                }
                else if (!string.IsNullOrEmpty(secondArg))
                {
                    if (secondArg.Equals("from", StringComparison.OrdinalIgnoreCase))
                        secondArg = user.GetArgument();

                    IContainer container = user.Character.FindChild<IContainer>(secondArg, false, false) ?? user.Character.FindNearby<IContainer>(secondArg, 0, true);
                    if (container == null || !((Object)container).ObjectFlags.Get((int)Object.ObjFlags.Container))
                    {
                        user.Send("Invalid container: " + secondArg);
                        return true;
                    }
                    if (container.Closed)
                    {
                        user.Send(String.Capitalize(container.Name) + " is closed.");
                        return true;
                    }
                    if (container.Locked)
                    {
                        user.Send(String.Capitalize(container.Name) + " is locked.");
                        return true;
                    }
                    Item item = container.FindChild<Item>(firstArg, false, false);
                    if (item == null)
                    {
                        user.Send("Item not found in " + container.Name + ": " + firstArg);
                        return true;
                    }
                    user.Character.Get(item, container);
                    return true;
                }
                else
                {
                    Item roomItem = user.Character.FindNearby<Item>(firstArg, 0, true);
                    if (roomItem != null)
                    {
                        if (roomItem.ObjectFlags.Get((int)Object.ObjFlags.Money))
                        {
                            user.Character.Value += roomItem.Value;
                            user.Character.Area.Send(new String(
                                "You pick up $" + roomItem.Value + ".", null, null, null,
                                "{1} picks up some money.", null), user.Character, null, 0);
                            roomItem.Parent = null;
                        }
                        else
                        {
                            user.Character.PickUp(roomItem);
                        }
                        return true;
                    }
                    user.Send("Target not found: " + firstArg);
                    return true;
                }
            }

            if (GetArguments(user, User.SelectItemRoom))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                {
                    if (selectedItem.ObjectFlags.Get((int)Object.ObjFlags.Money))
                    {
                        user.Character.Value += selectedItem.Value;
                        user.Character.Area.Send(new String(
                            "You pick up $" + selectedItem.Value + ".", null, null, null,
                            "{1} picks up some money.", null), user.Character, null, 0);
                        selectedItem.Parent = null;
                    }
                    else
                    {
                        user.Character.PickUp(selectedItem);
                    }
                }
                else
                    user.Send("Invalid target.");
                return true;
            }
            return false;
        }

        public static bool DoGetFromContainer(User user)
        {
            if (GetArguments(user, User.SelectContainer, User.SelectItemFromContainer))
            {
                IContainer container = user.GetCommandArg<IContainer>(0);
                if (container == null || !((Object)container).ObjectFlags.Get((int)Object.ObjFlags.Container))
                    user.Send("Invalid container.");
                else
                {
                    Item target = user.GetCommandArg<Item>(1);
                    if (target == null || target == container)
                        user.Send("Item not found in container.");
                    else
                        user.Character.Get(target, container);
                }
                return true;
            }
            return false;
        }

        public static bool DoPut(User user)
        {
            if (GetArguments(user, User.SelectItemInInventory, User.SelectContainer))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem == null)
                    user.Send("Item not found.");
                else
                {
                    IContainer container = user.GetCommandArg<IContainer>(1);
                    if (container == null || container == selectedItem || !((Object)container).ObjectFlags.Get((int)Object.ObjFlags.Container))
                        user.Send("Invalid container.");
                    else if (container.Closed)
                        user.Send(String.Capitalize(container.Name) + " is closed.");
                    else if (container.Locked)
                        user.Send(String.Capitalize(container.Name) + " is locked.");
                    else
                        user.Character.Put(selectedItem, container);
                }
                return true;
            }
            return false;
        }

        public static bool DoDrop(User user)
        {
            if (user.CurrentCommandArgs.Count == 0 && !string.IsNullOrEmpty(user.Command))
            {
                string trimmed = user.Command.Trim();
                int moneyAmt;
                if (trimmed.StartsWith("$") && int.TryParse(trimmed.Substring(1), out moneyAmt) && moneyAmt > 0)
                {
                    user.Command = null;
                    if (moneyAmt > user.Character.Value)
                        user.Send("You don't have that much money.");
                    else
                        user.Character.DropMoney(moneyAmt, false);
                    return true;
                }
            }

            if (GetArguments(user, User.SelectItemInInventory))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                    user.Character.Drop(selectedItem, false);
                else
                    user.Send("Item not found.");
                return true;
            }
            return false;
        }

        public static bool DoUse(User user)
        {
            if (GetArguments(user, User.SelectItemInInventory))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                    user.Character.Use(selectedItem, false);
                else
                    user.Send("Item not found.");
                return true;
            }
            return false;
        }

        public static bool DoRemove(User user)
        {
            if (GetArguments(user, User.SelectEquippedItem))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                    user.Character.Remove(selectedItem.EquipLocation);
                else
                    user.Send("Item not found.");
                return true;
            }
            return false;
        }

        public static bool DoGive(User user)
        {
            if (GetArguments(user, User.SelectItemInInventory, User.SelectCharacterRoom))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem == null)
                    user.Send("Item not found.");
                else
                {
                    Character target = user.GetCommandArg<Character>(1);
                    if (target == null || target == user.Character)
                        user.Send("Invalid target.");
                    else
                    {
                        user.Character.Give(selectedItem, target);
                        if (selectedItem.ObjectFlags.Get((int)Object.ObjFlags.Money))
                            user.Character.Value -= selectedItem.Value;
                    }
                }
                return true;
            }
            return false;
        }

        static bool SelectMoneyAmount(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Menus.Enqueue(new Menu(SelectMoneyAmount, "How much money? "));
                return false;
            }
            user.CurrentCommandArgs.Add(arg.TrimStart('$'));
            return true;
        }

        public static bool DoGiveMoney(User user)
        {
            if (user.CurrentCommandArgs.Count == 0 && !string.IsNullOrEmpty(user.Command))
            {
                string trimmed = user.Command.Trim();
                string[] parts = trimmed.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                int testAmt;
                if (parts.Length == 2 && int.TryParse(parts[0].TrimStart('$'), out testAmt))
                    user.Command = parts[1] + " " + parts[0];
            }

            if (GetArguments(user, User.SelectCharacterRoom, SelectMoneyAmount))
            {
                Character ch = user.GetCommandArg<Character>(0);
                if (ch == null || ch == user.Character)
                    user.Send("Invalid target.");
                else
                {
                    string arg = user.GetCommandArg<string>(1);
                    int amount = 0;
                    if (!int.TryParse(arg, out amount) || amount <= 0)
                        user.Send("Amount must be a positive number.");
                    else if (amount > user.Character.Value)
                        user.Send("You can't give more than you have.");
                    else
                    {
                        user.Character.Value -= amount;
                        ch.Value += amount;
                        user.Character.Area.Send(new String(
                            "You give {2} $" + amount + ".", "You give {2} $" + amount + ".",
                            "{1} gives you $" + amount + ".", "{1} gives you $" + amount + ".",
                            "{1} gives {2} some money.", null), user.Character, ch, 0);
                    }
                }
                return true;
            }
            return false;
        }

        public static bool DoDestroyItem(User user)
        {
            if (GetArguments(user, User.SelectItemInInventory))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                {
                    user.Send("You destroy " + selectedItem.Name + ".");
                    selectedItem.Parent = null;
                }
                else
                    user.Send("Item not found in inventory.");
                return true;
            }
            return false;
        }

        public static bool DoOpen(User user)
        {
            if (GetArguments(user, User.SelectCloseableObject))
            {
                ICloseable o = user.GetCommandArg<ICloseable>(0);
                if (o == null)
                    user.Send("Invalid target.");
                else if (!o.Closeable)
                    user.Send("You can't do that.");
                else
                    user.Character.Open(o);
                return true;
            }
            return false;
        }

        public static bool DoClose(User user)
        {
            if (GetArguments(user, User.SelectCloseableObject))
            {
                ICloseable o = user.GetCommandArg<ICloseable>(0);
                if (o == null)
                    user.Send("Invalid target.");
                else if (!o.Closeable)
                    user.Send("You can't do that.");
                else
                    user.Character.Close(o);
                return true;
            }
            return false;
        }

        public static bool DoLock(User user)
        {
            if (GetArguments(user, User.SelectCloseableObject))
            {
                ICloseable o = user.GetCommandArg<ICloseable>(0);
                if (o == null)
                    user.Send("Invalid target.");
                else if (!o.Closeable)
                    user.Send("You can't do that.");
                else
                    user.Character.Lock(o);
                return true;
            }
            return false;
        }

        public static bool DoUnlock(User user)
        {
            if (GetArguments(user, User.SelectCloseableObject))
            {
                ICloseable o = user.GetCommandArg<ICloseable>(0);
                if (o == null)
                    user.Send("Invalid target.");
                else if (!o.Closeable)
                    user.Send("You can't do that.");
                else
                    user.Character.Unlock(o);
                return true;
            }
            return false;
        }

        public static bool DoShopList(User user)
        {
            List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
            if (shops.Count < 1)
            {
                user.Send("There is no shop here.");
                return true;
            }

            Shop shop = shops[0];
            StringTable table = new StringTable("Item", "Qty.", "Price");
            table.Columns[1].Alignment = StringTable.Column.Align.Right;
            table.Columns[2].Alignment = StringTable.Column.Align.Right;

            foreach (Guid id in shop.Items)
            {
                Item item = Server.Instance.Get<Item>(id);
                if (item != null)
                    table.AddRow(item.Name, "--", "$" + shop.SellPrice(item).ToString(Server.Instance.Settings.NumberFormat));
            }

            Dictionary<string, List<Item>> stock = Utils.ConsolidateList<Item>(shop.Stock);
            foreach (KeyValuePair<string, List<Item>> pair in stock)
                table.AddRow(pair.Value[0].Name, pair.Value.Count.ToString(), "$" + shop.SellPrice(pair.Value[0]).ToString(Server.Instance.Settings.NumberFormat));

            table.Sort("Item");
            user.Send(shop.Name + " Inventory:\r\n" + table.ToString());
            return true;
        }

        public static bool DoBuy(User user)
        {
            if (GetArguments(user, User.SelectItemToBuy))
            {
                Shop shop = user.GetCommandArg<Shop>(0);
                Item selectedItem = user.GetCommandArg<Item>(1);
                if (shop == null)
                {
                    user.Send("There is nothing to buy here.");
                }
                else if (selectedItem != null)
                {
                    shop.Sell(selectedItem, user.Character);
                }
                else
                {
                    user.Send("This shop doesn't carry any items with that name.");
                }
                return true;
            }
            return false;
        }

        public static bool DoSell(User user)
        {
            if (GetArguments(user, User.SelectItemToSell))
            {
                Shop shop = user.GetCommandArg<Shop>(0);
                Item selectedItem = user.GetCommandArg<Item>(1);
                if (shop == null)
                    user.Send("There is nowhere to sell here.");
                else if (selectedItem != null)
                    shop.Buy(selectedItem, user.Character);
                else
                    user.Send("Item not found in inventory.");
                return true;
            }
            return false;
        }

        public static bool DoEvaluate(User user)
        {
            List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
            if (shops.Count < 1)
            {
                user.Send("There is no shopkeeper here to evaluate your item.");
                return true;
            }
            if (GetArguments(user, User.SelectItemInInventory))
            {
                Item item = user.GetCommandArg<Item>(0);
                if (item == null)
                    user.Send("Item not found.");
                else
                    user.Send(shops[0].Name + " will pay $" + shops[0].BuyPrice(item).ToString(Server.Instance.Settings.NumberFormat) + " for " + item.Name + ".");
                return true;
            }
            return false;
        }

        public static bool DoLearn(User user)
        {
            if (GetArguments(user, User.SelectNpcRoom, User.SelectAbility))
            {
                Npc npc = user.GetCommandArg<Npc>(0);
                Ability ability = user.GetCommandArg<Ability>(1);
                if (npc == null || !npc.CanTeach)
                {
                    user.Send("They have nothing to teach you.");
                    return true;
                }
                if (ability == null || !npc.TeachableAbilities.ContainsKey(ability.Id))
                {
                    user.Send("They don't teach that ability.");
                    return true;
                }
                if (user.Character.Abilities.ContainsKey(ability.Id))
                {
                    user.Send("You already know that ability.");
                    return true;
                }
                if (!user.Character.CanUse(ability, true))
                {
                    user.Send("You don't meet the requirements to learn that ability.");
                    return true;
                }

                int cost = npc.TeachableAbilities[ability.Id];
                if (cost > user.Character.Value)
                {
                    user.Send("You don't have enough money.");
                    return true;
                }

                user.Character.Value -= cost;
                user.Character.Abilities.Add(ability.Id, 0);
                user.Send(Server.Instance.Settings.GetColorCode(Settings.Text.AbilityImprovements) +
                    "You have learned a new ability! (" + ability.Name + ")");
                return true;
            }
            return false;
        }

        public static bool DoTeachPlayer(User user)
        {
            string targetName = user.GetArgument();
            string abilityName = user.GetArgument();
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(abilityName))
            {
                user.Send("Usage: teach <player> <ability>");
                return true;
            }

            Character target = user.Character.FindNearby<Character>(targetName, 0, true);
            Ability ability = Server.Instance.Get<Ability>(abilityName);
            if (target == null)
            {
                user.Send("Target not found: " + targetName);
                return true;
            }
            if (ability == null || (! (user.Character is Npc) && !user.Character.Abilities.ContainsKey(ability.Id)))
            {
                user.Send("You don't know that ability.");
                return true;
            }
            if (target.Abilities.ContainsKey(ability.Id))
            {
                user.Send(target.Name + " already knows " + ability.Name + ".");
                return true;
            }
            if (!target.CanUse(ability, true))
            {
                user.Send(target.Name + " cannot learn " + ability.Name + ".");
                return true;
            }

            target.Abilities.Add(ability.Id, 0);
            user.Send("You teach " + ability.Name + " to " + target.Name + ".");
            target.Send(Server.Instance.Settings.GetColorCode(Settings.Text.AbilityImprovements) +
                user.Character.Name + " has taught you a new ability! (" + ability.Name + ")");
            return true;
        }

        public static bool DoTime(User user)
        {
            TimeSpan uptime = DateTime.Now - Server.Instance.StartTime;
            user.Send("Server Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                "\r\nUptime:      " + (int)uptime.TotalHours + "h " + uptime.Minutes + "m " + uptime.Seconds + "s");
            return true;
        }

        public static bool DoWeather(User user)
        {
            Area area = user.Character.Area;
            if (area == null)
            {
                user.Send("You cannot tell the weather here.");
                return true;
            }
            Area.Layer layer = user.Character.OnLayer;
            string weather = (layer != null && !Damdai.Collections.Util.IsEmpty(layer.WeatherConditions.PossibleConditions)
                ? layer.WeatherConditions.ToString()
                : area.WeatherConditions.ToString());
            user.Send("Current weather in " + area.Name + ": " + weather);
            return true;
        }

        public static bool DoSet(User user)
        {
            if (GetArguments(user, User.SelectSetting))
            {
                string cmd = user.GetCommandArg<string>(0);
                Server.InputHandler handler = User.GetSettingHandler(cmd);
                if (handler == null)
                    user.Send("Invalid setting: " + cmd);
                else
                    return handler(user);
                return true;
            }
            return false;
        }

        public static bool DoAlias(User user)
        {
            if (user.CurrentCommandArgs.Count == 0 && !string.IsNullOrEmpty(user.Command))
            {
                string trimmed = user.Command.Trim();
                string[] parts = trimmed.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 &&
                    !parts[0].Equals("add", StringComparison.OrdinalIgnoreCase) &&
                    !parts[0].Equals("delete", StringComparison.OrdinalIgnoreCase) &&
                    !parts[0].Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    user.Command = "add " + trimmed;
                }
            }

            if (GetArguments(user, User.SelectAliasCommand))
            {
                string arg = user.GetCommandArg<string>(0);
                Server.InputHandler handler = Alias.GetHandler(arg);
                if (handler == null)
                {
                    user.Send("Invalid option: " + arg);
                    return true;
                }
                else
                {
                    user.CurrentCommandArgs.Clear();
                    return handler(user);
                }
            }
            return false;
        }

        public static bool DoForums(User user)
        {
            if (GetArguments(user, User.SelectForumCommand))
            {
                string arg = user.GetCommandArg<string>(0);
                Server.InputHandler handler = MessageBoard.GetHandler(arg);
                if (handler == null)
                {
                    user.Send("Invalid option: " + arg);
                    return true;
                }
                else
                {
                    user.CurrentCommandArgs.Clear();
                    return handler(user);
                }
            }
            return false;
        }

        public static bool DoChannels(User user)
        {
            Menu menu = new Menu();
            foreach (Channel channel in Server.Instance.DataSets[typeof(Channel)].Values)
                menu.AddOption(channel.Name);

            if (menu.Options != null)
                user.Send(menu.ToString());
            else
                user.Send("There are no channels.");
            return true;
        }

        public static bool DoSocials(User user)
        {
            int maxLength = 0;
            SortedDictionary<string, Social> socials = new SortedDictionary<string, Social>();
            foreach (Social social in Server.Instance.DataSets[typeof(Social)].Values)
            {
                if (!string.IsNullOrEmpty(social.Name) && !socials.ContainsKey(social.Name))
                {
                    socials.Add(social.Name, social);
                    if (social.Name.Length > maxLength)
                        maxLength = social.Name.Length;
                }
            }

            if (socials.Count == 0)
            {
                user.Send("There are no socials.");
                return true;
            }

            int maxColumns = Math.Max(1, 70 / Math.Max(1, maxLength));
            string str = string.Empty;
            int count = 0;
            foreach (Social social in socials.Values)
            {
                str += string.Format("{0,-" + maxLength + "}  ", social.Name);
                if (++count % maxColumns == 0)
                    str += "\r\n";
            }
            user.Send(str.TrimEnd('\r', '\n'));
            return true;
        }

        public static bool DoCommands(User user)
        {
            Menu menu = new Menu();
            foreach (Command c in commands_)
            {
                if (c.SystemCommand || c.Hidden)
                    continue;

                string text = c.Name;
                if (!c.CanUseIncapacitated && user.Character.Incapacitated)
                    text = "{color:gray}" + c.Name + "{color:reset}";
                menu.AddOption(new Menu.Option(c.Name, text, null));
            }
            user.Send(menu.ToString());
            return true;
        }

        public static bool DoHelp(User user)
        {
            if (GetArguments(user, User.SelectHelp))
            {
                object arg = user.GetArg(0);
                Help help = arg as Help;
                if (help == null && arg is string)
                    help = Help.Get((string)arg);

                if (help == null)
                    user.Send("No help available for that topic.");
                else
                    user.Send(String.Format(help.Print()).TrimEnd('\r', '\n'));
                return true;
            }
            return false;
        }

        public static bool DoSave(User user)
        {
            user.Save();
            user.Send("Character saved.");
            return true;
        }

        public static bool DoPassword(User user)
        {
            if (GetArguments(user, User.EnterPassword, User.ConfirmPassword))
            {
                string pass1 = user.GetCommandArg<string>(0);
                string pass2 = user.GetCommandArg<string>(1);
                if (pass1 == pass2)
                {
                    user.SetPassword(pass1);
                    user.Save();
                    user.Send("Password changed.");
                }
                else
                {
                    user.SendError("Passwords do not match.");
                }
                return true;
            }
            return false;
        }

        public static bool DoDelete(User user)
        {
            if (GetArguments(user, User.EnterPassword, User.ConfirmPassword))
            {
                string arg1 = user.GetCommandArg<string>(0);
                string arg2 = user.GetCommandArg<string>(1);
                if (arg1 == arg2 && user.VerifyPassword(arg2))
                {
                    string name = user.Character.Name;
                    user.Send("Thank you for playing " + Server.Instance.Settings.Name + ".");
                    user.Character.DropItems(false);
                    if (user.Character.Area != null)
                        user.Character.Area.Send(new String(null, null, null, null, "{1} disintegrates!", null), user.Character, null, 0);
                    Server.Instance.Settings.InfoChannel.Broadcast(InfoChannel.Event.Deleted, name + " has deleted.");
                    user.Delete();
                    Server.Instance.Unload(user, false);
                    Server.Instance.Disconnect(user);
                }
                else
                {
                    user.Send("Invalid password or passwords do not match.");
                }
                return true;
            }
            return false;
        }

        public static bool DoQuit(User user)
        {
            if (user.Character.FightSystem != null)
            {
                user.Send("You are still involved in a fight!");
                return true;
            }

            string name = user.Character.Name;
            String str = new String("You fade out of existence.", null, null, null, "{1} fades out of existence.", null);
            if (user.Character.Area != null)
                user.Character.Area.Send(str, user.Character, null, 0);
            user.Send("Goodbye.");
            Server.Instance.Settings.InfoChannel.Broadcast(InfoChannel.Event.Logout, name + " has logged out.");
            Server.Instance.Unload(user, true);
            Server.Instance.Disconnect(user);
            Trace.WriteLine(name + " has quit.");
            return true;
        }

        // Script Commands
        public static bool DoPeace(User user)
        {
            if (user.Character.FightSystem != null)
            {
                user.Character.FightSystem.Finish(null);
                if (user.Character.CurrentAction is AbilityAction)
                    user.Character.Stop(false);
            }
            return true;
        }

        public static bool DoForce(User user)
        {
            Npc npc = user.Character as Npc;
            if (npc == null)
                return true;

            string arg1 = user.GetArgument();
            string arg2 = user.Command;
            user.Command = null;

            if (!string.IsNullOrEmpty(arg1) && !string.IsNullOrEmpty(arg2))
            {
                Character ch = npc.FindNearby<Character>(arg1, 0, false);
                if (ch != null && ch.User != null)
                {
                    ch.User.Command = arg2;
                    Server.Instance.ProcessCommand(ch.User, true);
                    ch.User.FinishInput();
                }
            }
            return true;
        }

        public static bool DoCreateItem(User user)
        {
            string arg1 = user.Command;
            user.Command = null;
            if (!string.IsNullOrEmpty(arg1))
            {
                Item bp = Server.Instance.Get<Item>(arg1);
                if (bp != null)
                {
                    Item item = Item.Create(bp);
                    if (item != null)
                        item.Parent = user.Character;
                }
            }
            return true;
        }

        public static bool DoPrint(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (!string.IsNullOrEmpty(msg) && user.Character.Area != null)
            {
                user.Character.Area.Send(new String(msg, msg, msg, msg, msg, msg), user.Character, null, 0);
            }
            return true;
        }

        public static bool DoSummonNpc(User user)
        {
            string arg1 = user.Command;
            user.Command = null;
            if (!string.IsNullOrEmpty(arg1))
            {
                Npc bp = Server.Instance.Get<Npc>(arg1);
                if (bp != null)
                {
                    Npc npc = Npc.Create(bp);
                    if (npc != null)
                        npc.Location = user.Character.Location;
                }
            }
            return true;
        }
        #endregion
    }

    internal class AbortCommandException : Exception
    {
    }

    [Obfuscation(Exclude = false)]
    internal class MenuFunctionConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> list = new List<string>();
            MethodInfo[] methods = typeof(User).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(User) && method.ReturnType == typeof(bool))
                    list.Add(method.Name);
            }
            list.Sort();
            return new StandardValuesCollection(list);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return true;
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value;
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return value;
        }
    }

    [Serializable]
    [Obfuscation(Exclude = true)]
    public class MenuFunction
    {
        string fnname_;
        Server.InputHandler handler_;

        [TypeConverter(typeof(MenuFunctionConverter))]
        public string Function
        {
            get { return this.fnname_; }
            set { this.fnname_ = value; this.handler_ = null; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public Server.InputHandler Handler
        {
            get
            {
                if (this.handler_ == null && !string.IsNullOrEmpty(this.fnname_))
                {
                    MethodInfo method = typeof(User).GetMethod(this.fnname_, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    if (method != null)
                        this.handler_ = Delegate.CreateDelegate(typeof(Server.InputHandler), method) as Server.InputHandler;
                }
                return this.handler_;
            }
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.fnname_))
                return base.ToString();
            else
                return this.fnname_;
        }
    }
}
