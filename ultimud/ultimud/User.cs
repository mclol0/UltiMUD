using System;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Security.Cryptography;
using LuaInterface;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class User
    {
        public enum UserFlags
        {
            AutoMap,
            LoggedIn,
            Administrator,
            Spacing,
            Help,
            Creating,
            Silenced
        };

        #region Members
        Guid id_;
        string password_;
        string prompt_; //Deprecated.
        Character character_;
        BitArray flags_ = new BitArray(16);
        Location startLocation_;
        Dictionary<string, string> aliases_ = new Dictionary<string, string>();
        Dictionary<Guid, bool> channelsettings_ = new Dictionary<Guid, bool>();

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Socket socket_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        string input_ = string.Empty;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        string command_;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Server.InputHandler inputHandler_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Queue<Menu> menus_ = new Queue<Menu>();
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        string lastCommand_;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Dictionary<int, List<string>> output_ = new Dictionary<int, List<string>>();
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        bool processingInput_;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        List<object> currentCommandArgs_ = new List<object>();
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        static Dictionary<string, Server.InputHandler> settings_ = new Dictionary<string, Server.InputHandler>();
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        DateTime lastActivity_ = DateTime.Now;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        object currentarg_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_ = new Lua();

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        object inputLock_ = new object();
        #endregion

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [XmlIgnore]
        [Browsable(false)]
        internal object InputLock
        {
            get
            {
                if (this.inputLock_ == null)
                    this.inputLock_ = new object();
                return this.inputLock_;
            }
        }

        public string CustomPrompt
        {
            get { return this.prompt_; }
            set { this.prompt_ = value; }
        }

        [Browsable(false)]
        internal Guid UnusedId
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [XmlIgnore]
        internal Socket Socket
        {
            get { return this.socket_; }
            set { this.socket_ = value; }
        }

        [XmlIgnore]
        internal string Input
        {
            get { return this.input_; }
            set { this.input_ = value; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public string Command
        {
            get { return (this.command_ == null ? string.Empty : this.command_); }
            set { this.command_ = value; }
        }

        internal string Password
        {
            get { return this.password_; }
            set { this.password_ = value; }
        }

        [XmlIgnore]
        internal Server.InputHandler InputHandler
        {
            get { return this.inputHandler_; }
            set { this.inputHandler_ = value; }
        }

        [Browsable(false)]
        public string Prompt
        {
            get
            {
                if (this.Socket == null)
                    return null;

                if (this.CurrentMenu != null)
                    return this.CurrentMenu.Prompt;
                else
                {
                    if (this.Character == null || !this.Flags.Get((int)UserFlags.LoggedIn))
                        return string.Empty;
                    if (!string.IsNullOrEmpty(this.prompt_))
                        return String.ReplaceVars(this.prompt_, this.Character) + "\r\n";
                    try
                    {
                        lock (this.lua_)
                        {
                            if (Server.Instance == null || Server.Instance.Settings == null || string.IsNullOrEmpty(Server.Instance.Settings.Prompt))
                                return "> ";
                            this.lua_["user"] = this;
                            object[] res = this.lua_.DoString(Server.Instance.Settings.Prompt);
                            if (res != null && res.Length > 0 && res[0] != null)
                                return res[0].ToString();
                            return "> ";
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Prompt: " + e.Message, "LUA");
                        return "> ";
                    }
                }
            }
        }

        public static string HashPassword(string password)
        {
            if (password == null)
                password = string.Empty;
            if (password.StartsWith("$SHA256$"))
                return password;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);
                return "$SHA256$" + Convert.ToBase64String(hash);
            }
        }

        public void SetPassword(string password)
        {
            this.password_ = HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(this.password_) || password == null)
                return false;
            if (this.password_.StartsWith("$SHA256$"))
            {
                return this.password_ == HashPassword(password);
            }
            else
            {
                if (this.password_ == password)
                {
                    this.password_ = HashPassword(password);
                    return true;
                }
                return false;
            }
        }

        [XmlIgnore]
        internal Queue<Menu> Menus
        {
            get { return this.menus_; }
            set { this.menus_ = value; }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Character Character
        {
            get { return this.character_; }
            set
            {
                this.character_ = value;
                if (this.character_ != null)
                    this.character_.User = this;
            }
        }

        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<User.UserFlags>))]
        [Editor(typeof(FlagEditor<User.UserFlags>), typeof(UITypeEditor))]
        public BitArray Flags
        {
            get { return this.flags_; }
            set { this.flags_ = value; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public string LastCommand
        {
            get { return this.lastCommand_; }
            set { this.lastCommand_ = value; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public bool ProcessingInput
        {
            get { return this.processingInput_; }
            set { this.processingInput_ = value; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public List<object> CurrentCommandArgs
        {
            get { return this.currentCommandArgs_; }
            set { this.currentCommandArgs_ = value; }
        }

        [XmlIgnore]
        public string IpAddress
        {
            get {
                if (this.Socket != null && this.Socket.Connected)
                    return this.Socket.RemoteEndPoint.ToString();
                else
                    return "Not Connected";
            }
        }

        [XmlIgnore]
        public bool Connected
        {
            get { return (this.Socket != null && this.Socket.Connected); }
        }

        [Browsable(false)]
        public Menu CurrentMenu
        {
            get { return (this.Menus.Count > 0 ? this.Menus.Peek() : null); }
        }

        internal Server.InputHandler CurrentInputHandler
        {
            get
            {
                Menu menu = this.CurrentMenu;
                if (menu != null)
                    return menu.InputHandler;
                else
                    return this.InputHandler;
            }
        }

        [ReadOnly(true)]
        public Location StartLocation
        {
            get { return this.startLocation_; }
            set { this.startLocation_ = new Location(value.Area, value.Point); }
        }

        public Dictionary<string, string> Aliases
        {
            get { return this.aliases_; }
            set { this.aliases_ = value; }
        }

        [ReadOnly(true)]
        public DateTime LastActivity
        {
            get { return this.lastActivity_; }
            set { this.lastActivity_ = value; }
        }
        
        [Browsable(false)]
        public object CurrentArg
        {
            get { return this.currentarg_; }
            set { this.currentarg_ = value; }
        }
        #endregion

        #region Events
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        public Function_void<User, string> InputReceived;
        #endregion

        public User()
        {
        }
        internal User(Socket socket, Guid id)
        {
            this.Socket = socket;
            this.Id = id;
            this.Flags.Set((int)UserFlags.AutoMap, true);
            this.Flags.Set((int)UserFlags.Spacing, true);
            this.Flags.Set((int)UserFlags.Help, true);
        }
        public User(Character ch)
        {
            this.character_ = ch;
        }
        
        internal static void Init(string loadpath, string serverpath)
        {
            settings_.Clear();
            settings_.Add("prompt", new Server.InputHandler(User.SetPrompt));
            settings_.Add("map", new Server.InputHandler(User.SetMap));
            //new Setting("map", new InputHandler(User.SetMap));
            settings_.Add("spacing", new Server.InputHandler(User.SetSpacing));
            settings_.Add("help", new Server.InputHandler(User.SetHelp));
            settings_.Add("channel", new Server.InputHandler(User.SetChannel));

            string oldpath = loadpath + "\\pcs";
            string newpath = serverpath + "\\Players";
            if (!string.IsNullOrEmpty(loadpath) && !string.IsNullOrEmpty(serverpath) && Directory.Exists(oldpath))
                Directory.Move(oldpath, newpath);
        }
        
        [Obfuscation(Exclude = false)]
        internal void Init(User user)
        {
            this.Password = user.Password;
            this.prompt_ = user.prompt_;
            this.Character = user.Character;
            this.Flags = user.Flags;
            this.Aliases = user.Aliases;
            this.StartLocation = user.StartLocation;
        }
        
        [Obfuscation(Exclude = false)]
        public void Save()
        {
            if (this.Character != null)
            {
                lock (this.character_)
                {
                    string path = Server.Instance.ServerPath + "\\Players";
                    Directory.CreateDirectory(path);
                    Damdai.Serialization.Binary.Serialize(this, path + "\\" + this.Character.Name);
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        internal void Load()
        {
            string filename = Server.Instance.ServerPath + "\\Players\\" + this.Character.Name;
            User temp = Damdai.Serialization.Binary.Deserialize<User>(filename, new Server.SerializationBinder());
            this.Init(temp);
            this.Id = temp.Id;

            if (this.Character.Parent != null)
                this.Character.Parent.Children.Remove(this.Character);
        }
        
        public void Delete()
        {
            if (this.Character != null)
            {
                Trace.WriteLine("Deleting player: " + this.character_.Name);
                string path = Server.Instance.ServerPath + "\\Players\\" + this.Character.Name;
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        
        static internal bool SetPrompt(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                user.Menus.Enqueue(new Menu(SetPrompt, "New prompt: "));
                return false;
            }
            else
            {
                if (user.Command == "default")
                    user.CustomPrompt = null;
                else
                    user.CustomPrompt = user.Command + " ";

                user.FinishInput();
                return true;
            }
        }
        //static internal bool SetPromptOld(User user)
        //{
        //    if (string.IsNullOrEmpty(user.Command))
        //    {
        //        user.Menus.Enqueue(new Menu(SetPrompt, "New prompt: "));
        //        return false;
        //    }
        //    else
        //    {
        //        if (user.Command == "default")
        //            user.Prompt = null;
        //        else
        //            user.Prompt = user.Command + " ";

        //        user.FinishInput();
        //        return true;
        //    }
        //}
        static internal bool SetMap(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(SetMap, null);
                menu.AddOptions("on", "off");
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                arg = arg.ToLower();
                if (arg == "on")
                    user.Flags.Set((int)UserFlags.AutoMap, true);
                else if (arg == "off")
                    user.Flags.Set((int)UserFlags.AutoMap, false);
                else
                    user.Send("Invalid option: " + arg);

                user.FinishInput();
                return true;
            }
        }
        static internal bool SetSpacing(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(SetSpacing, null);
                menu.AddOptions("on", "off");
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                arg = arg.ToLower();
                if (arg == "on")
                    user.Flags.Set((int)UserFlags.Spacing, true);
                else if (arg == "off")
                    user.Flags.Set((int)UserFlags.Spacing, false);
                else
                    user.Send("Invalid option: " + arg);

                user.FinishInput();
                return true;
            }
        }
        static internal bool SetHelp(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(SetHelp, null);
                menu.AddOptions("on", "off");
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                arg = arg.ToLower();
                if (arg == "on")
                    user.Flags.Set((int)UserFlags.Help, true);
                else if (arg == "off")
                    user.Flags.Set((int)UserFlags.Help, false);
                else
                    user.Send("Invalid option: " + arg);

                user.FinishInput();
                return true;
            }
        }
        static internal bool SetChannel(User user)
        {
            Server.InputHandler getchannel = null;
            getchannel = delegate(User u)
            {
                string arg = null;
                if (user.CurrentArg != null)
                    user.CurrentCommandArgs.Add(user.GetCurrentArg());
                else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                {
                    Channel c = Server.Instance.Get<Channel>(arg);
                    if (c != null)
                        user.CurrentCommandArgs.Add(c);
                    else
                    {
                        user.Send("No such channel: " + arg);
                        throw new AbortCommandException();
                    }
                }
                else
                {
                    if (Server.Instance.DataSets[typeof(Channel)].Count == 0)
                    {
                        user.Send("There are no channels.");
                        throw new AbortCommandException();
                    }

                    Menu menu = new Menu(getchannel, null);
                    foreach (Channel c in Server.Instance.DataSets[typeof(Channel)].Values)
                        menu.AddOption(new Menu.Option(c.Name, c));
                    user.Menus.Enqueue(menu);
                    return false;
                }
                return true;
            };

            Server.InputHandler onoff = null;
            onoff = delegate(User u)
            {
                string arg = user.GetArgument();
                if (string.IsNullOrEmpty(arg))
                {
                    Menu menu = new Menu(onoff, null);
                    menu.AddOptions("on", "off");
                    user.Menus.Enqueue(menu);
                    return false;
                }
                else
                {
                    user.CurrentCommandArgs.Add(arg.ToLower());
                    return true;
                }
            };

            getchannel(user);
            onoff(user);

            if (user.CurrentCommandArgs.Count >= 2)
            {
                Channel c = user.GetCommandArg<Channel>(1);
                string s = user.GetCommandArg<string>(2);

                if (s == "off")
                    user.SetChannel(c, false, false);
                else
                    user.SetChannel(c, true, false);

                return true;
            }

            return false;
        }

        public bool GetChannelSetting(Channel channel)
        {
            bool on = false;
            if (!this.channelsettings_.TryGetValue(channel.Id, out on))
                return true;
            return on;
        }

        public void SetChannel(Channel channel, bool on, bool silent)
        {
            if (this.channelsettings_.ContainsKey(channel.Id))
                this.channelsettings_[channel.Id] = on;
            else
                this.channelsettings_.Add(channel.Id, on);

            if (!silent)
                this.Send(channel.Name.ToUpper() + " channel " + (on ? "on" : "off") + ".");
        }

        static internal Server.InputHandler GetSettingHandler(string str)
        {
            Server.InputHandler handler = null;
            settings_.TryGetValue(str, out handler);
            return handler;
        }

        public void Send(string str)
        {
            if (this.Socket != null && str != null && Telnet.Decolorize(str.Trim(' ', '\r', '\n')) != string.Empty)
            {
                //str = String.Capitalize(str);
                str += Telnet.ColorCode.Reset;
                lock (this.output_)
                {
                    int threadId = Thread.CurrentThread.GetHashCode();
                    List<string> list;
                    if (!this.output_.TryGetValue(threadId, out list))
                    {
                        list = new List<string>();
                        this.output_.Add(threadId, list);
                    }
                    list.Add(Telnet.Colorize(str));
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void SendPrompt()
        {
            if (this.Socket != null && this.Socket.Connected && this.Prompt != null)
            {
                string str = string.Empty;

                if (this.Flags.Get((int)UserFlags.Spacing) && (this.Flags.Get((int)UserFlags.LoggedIn) || this.Flags.Get((int)UserFlags.Creating)))
                {
                    str += "\r\n";
                }

                str += Telnet.Colorize(this.Prompt);
                byte[] data = Encoding.ASCII.GetBytes(str);
                try { this.Socket.Send(data); } catch (SocketException) { } catch (ObjectDisposedException) { }
            }
        }
        
        public void SendError(string str)
        {
            this.Send("{color:brightred}ERROR: " + str);
        }
        public void SendHelp(string str)
        {
            if (this.Flags.Get((int)UserFlags.Help))
            {
                this.Send(Server.Instance.Settings.GetColorCode(Settings.Text.Help) + "HELP: " + str);
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void FlushOutput()
        {
            if (this.Socket != null)
            {
                lock (this.output_)
                {
                    int threadid = Thread.CurrentThread.GetHashCode();
                    List<string> buffer;
                    if (this.output_.TryGetValue(threadid, out buffer))
                    {
                        StringBuilder sb = new StringBuilder();

                        if (this.Flags.Get((int)UserFlags.Spacing) && (this.Flags.Get((int)UserFlags.LoggedIn) || this.Flags.Get((int)UserFlags.Creating)))
                        {
                            sb.Append("\r\n");
                        }
                        if (!this.ProcessingInput)
                        {
                            sb.Append("\r\n");
                        }

                        foreach (string line in buffer)
                        {
                            sb.Append(line + "\r\n");
                        }

                        if (!this.ProcessingInput)
                        {
                            if (this.Flags.Get((int)UserFlags.Spacing) && (this.Flags.Get((int)UserFlags.LoggedIn) || this.Flags.Get((int)UserFlags.Creating)))
                            {
                                sb.Append("\r\n");
                            }
                            sb.Append(Telnet.Colorize(this.Prompt));
                        }

                        byte[] data = Encoding.ASCII.GetBytes(sb.ToString());
                        try { this.Socket.Send(data); } catch (SocketException) { } catch (ObjectDisposedException) { }
                        this.output_.Remove(threadid);
                    }
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void FlushAllOutput()
        {
            if (this.Socket != null)
            {
                lock (this.output_)
                {
                    foreach (List<string> buffer in this.output_.Values)
                    {
                        StringBuilder sb = new StringBuilder();

                        if (this.Flags.Get((int)UserFlags.Spacing) && (this.Flags.Get((int)UserFlags.LoggedIn) || this.Flags.Get((int)UserFlags.Creating)))
                        {
                            sb.Append("\r\n");
                        }
                        if (!this.ProcessingInput)
                        {
                            sb.Append("\r\n");
                        }
                        foreach (string line in buffer)
                        {
                            sb.Append(line + "\r\n");
                        }

                        byte[] data = Encoding.ASCII.GetBytes(sb.ToString());
                        try { this.Socket.Send(data); } catch (SocketException) { } catch (ObjectDisposedException) { }
                    }

                    this.output_.Clear();
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void FinishInput()
        {
            this.InputHandler = Server.Instance.ProcessCommand;
            this.Menus.Clear();
            this.CurrentCommandArgs.Clear();
            this.currentarg_ = null;
            this.Command = null;
        }

        public string GetArgument()
        {
            return String.GetArgument(ref this.command_);
        }
        
        public override string ToString()
        {
            return (this.character_ == null ? "connecting..." : this.character_.Name) + " [" + this.IpAddress + "]";
        }
        
        [Obfuscation(Exclude = false)]
        public T GetCommandArg<T>(int num)
        {
            return (T)this.currentCommandArgs_[num];
        }
        
        [Obfuscation(Exclude = false)]
        public T GetLastCommandArg<T>()
        {
            if (this.CurrentCommandArgs.Count > 0)
                return (T)this.CurrentCommandArgs[this.CurrentCommandArgs.Count - 1];
            else
                return default(T);
        }
        
        public object GetArg(int num)
        {
            return this.currentCommandArgs_[num];
        }
        public object GetLastCommandArg()
        {
            if (this.CurrentCommandArgs.Count > 0)
                return this.CurrentCommandArgs[this.CurrentCommandArgs.Count - 1];
            else
                return null;
        }
        public object GetCurrentArg()
        {
            object o = this.currentarg_;
            this.currentarg_ = null;
            return o;
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            this.currentCommandArgs_ = new List<object>();
            this.menus_ = new Queue<Menu>();
            this.output_ = new Dictionary<int, List<string>>();
            this.inputLock_ = new object();
            this.lua_ = new Lua();
            if (this.channelsettings_ == null)
                this.channelsettings_ = new Dictionary<Guid, bool>();
        }

        #region Menu Functions
        internal static bool EnterMessage(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                user.Menus.Enqueue(new Menu(EnterMessage, "Enter message: "));
                return false;
            }
            else
            {
                user.CurrentCommandArgs.Add(user.Command);
                user.Command = null;
                return true;
            }
        }
        internal static bool SelectSetting(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(SelectSetting, null);
                foreach (string command in settings_.Keys)
                    menu.AddOption(command);
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                user.CurrentCommandArgs.Add(arg);
                return true;
            }
        }
        internal static bool EnterPassword(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                Telnet.SetEcho(user, false);
                user.Menus.Enqueue(new Menu(EnterPassword, "Enter password: "));
                return false;
            }
            else
            {
                Telnet.SetEcho(user, true);
                string pass = user.GetArgument();
                if (pass.Length < 4)
                {
                    user.SendError("Password must be at least 4 characters long.");
                    return EnterPassword(user);
                }
                user.CurrentCommandArgs.Add(pass);
                return true;
            }
        }
        internal static bool ConfirmPassword(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                Telnet.SetEcho(user, false);
                user.Menus.Enqueue(new Menu(ConfirmPassword, "Confirm password: "));
                return false;
            }
            else
            {
                Telnet.SetEcho(user, true);
                user.CurrentCommandArgs.Add(user.GetArgument());
                return true;
            }
        }
        internal static bool ConfirmPasswordLegacyUnused(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                Telnet.SetEcho(user, false);
                user.Menus.Enqueue(new Menu(ConfirmPassword, "Confirm password: "));
                return false;
            }
            else
            {
                Telnet.SetEcho(user, true);
                string pass = user.GetArgument();
                if (pass.Length < 4)
                {
                    user.SendError("Password must be at least 4 characters long.");
                    return EnterPassword(user);
                }
                user.CurrentCommandArgs.Add(pass);
                return true;
            }
        }
        internal static bool SelectObjectWorld<T>(User user) where T : Object
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(Server.Instance.Find<T>(arg));
            else
            {
                Server.InputHandler inputhandler = null;
                Type type = typeof(T);
                if (type == typeof(Area))
                    inputhandler = SelectObjectWorld<Area>;
                else if (type == typeof(Npc))
                    inputhandler = SelectObjectWorld<Npc>;
                else if (type == typeof(Player))
                    inputhandler = SelectObjectWorld<Player>;
                else if (type == typeof(Character))
                    inputhandler = SelectObjectWorld<Character>;
                else if (type == typeof(Item))
                    inputhandler = SelectObjectWorld<Item>;
                Menu menu = new Menu(inputhandler, null);
                foreach (Area area in Server.Instance.GetAreas())
                    foreach (T t in area.GetChildren<T>())
                        if (t != user.character_)
                            menu.AddOption(new Menu.Option(t.Name, t));

                if (menu.Options == null)
                {
                    user.Send("No targets found.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectObjectArea<T>(User user) where T : Object
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.character_.Area.FindChild<T>(arg, false, false));
            else
            {
                Server.InputHandler inputhandler = null;
                Type type = typeof(T);
                if (type == typeof(Area))
                    inputhandler = SelectObjectArea<Area>;
                else if (type == typeof(Npc))
                    inputhandler = SelectObjectArea<Npc>;
                else if (type == typeof(Player))
                    inputhandler = SelectObjectArea<Player>;
                else if (type == typeof(Character))
                    inputhandler = SelectObjectArea<Character>;
                else if (type == typeof(Item))
                    inputhandler = SelectObjectArea<Item>;

                Menu menu = new Menu(inputhandler, null);
                foreach (T t in user.character_.Area.GetChildren<T>())
                    if (t != user.character_)
                        menu.AddOption(new Menu.Option(t.Name, t));

                if (menu.Options == null)
                {
                    user.Send("No targets found.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectObjectFar<T>(User user) where T : Object
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.character_.FindNearby<T>(arg, (int)Area.Range.Far, true));
            else
            {
                Server.InputHandler inputhandler = null;
                Type type = typeof(T);
                if (type == typeof(Area))
                    inputhandler = SelectObjectFar<Area>;
                else if (type == typeof(Npc))
                    inputhandler = SelectObjectFar<Npc>;
                else if (type == typeof(Player))
                    inputhandler = SelectObjectFar<Player>;
                else if (type == typeof(Character))
                    inputhandler = SelectObjectFar<Character>;
                else if (type == typeof(Item))
                    inputhandler = SelectObjectFar<Item>;

                Menu menu = new Menu(inputhandler, null);
                foreach (T t in user.character_.NearbyObjects<T>((int)Area.Range.Far, true, user.character_))
                    menu.AddOption(new Menu.Option(t.Name, t));

                if (menu.Options == null)
                {
                    user.Send("No targets found.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectObjectNearby<T>(User user) where T : Object
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.character_.FindNearby<T>(arg, (int)Area.Range.Nearby, true));
            else
            {
                Server.InputHandler inputhandler = null;
                Type type = typeof(T);
                if (type == typeof(Area))
                    inputhandler = SelectObjectNearby<Area>;
                else if (type == typeof(Npc))
                    inputhandler = SelectObjectNearby<Npc>;
                else if (type == typeof(Player))
                    inputhandler = SelectObjectNearby<Player>;
                else if (type == typeof(Character))
                    inputhandler = SelectObjectNearby<Character>;
                else if (type == typeof(Item))
                    inputhandler = SelectObjectNearby<Item>;
                Menu menu = new Menu(inputhandler, null);
                foreach (T t in user.character_.NearbyObjects<T>((int)Area.Range.Nearby, true, user.character_))
                    menu.AddOption(new Menu.Option(t.Name, t));

                if (menu.Options == null)
                {
                    user.Send("No targets found.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectObjectRoom<T>(User user) where T : Object
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
            {
                T t = user.character_.FindNearby<T>(arg, 0, true);
                if (t == null)
                {
                    user.Send("Target not found: " + arg);
                    throw new AbortCommandException();
                }
                else
                    user.CurrentCommandArgs.Add(t);
            }
            else
            {
                Server.InputHandler inputhandler = null;
                Type type = typeof(T);
                if (type == typeof(Area))
                    inputhandler = SelectObjectRoom<Area>;
                else if (type == typeof(Npc))
                    inputhandler = SelectObjectRoom<Npc>;
                else if (type == typeof(Player))
                    inputhandler = SelectObjectRoom<Player>;
                else if (type == typeof(Character))
                    inputhandler = SelectObjectRoom<Character>;
                else if (type == typeof(Item))
                    inputhandler = SelectObjectRoom<Item>;
                Menu menu = new Menu(inputhandler, null);
                foreach (T t in user.character_.NearbyObjects<T>(0, true, user.character_))
                    menu.AddOption(new Menu.Option(t.Name, t));

                if (menu.Options == null)
                {
                    user.Send("No targets found.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectDestinationOrDirection(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
            {
                List<string> dirs = new List<string>();
                dirs.AddRange(Enum.GetNames(typeof(Area.ShortDirection)));
                string dir = dirs.Find(delegate(string str) { return str.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase); });
                if (dir != null) user.CurrentCommandArgs.Add((Area.Direction)Enum.Parse(typeof(Area.ShortDirection), dir));
                else
                {
                    dirs.Clear();
                    dirs.AddRange(Enum.GetNames(typeof(Area.Direction)));
                    dir = dirs.Find(delegate(string str) { return str.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase); });
                    if (dir != null) user.CurrentCommandArgs.Add(Enum.Parse(typeof(Area.Direction), dir));
                    else user.CurrentCommandArgs.Add(user.Character.FindNearby<Object>(arg, System.Math.Max(5, (int)user.Character.GetStatCurrent("sight")), true));
                }
            }
            else
            {
                Menu menu = new Menu(SelectDestinationOrDirection, null);
                for (int x = 0; x < Enum.GetValues(typeof(Area.Direction)).Length; ++x)
                {
                    Area.Direction dir = (Area.Direction)x;
                    menu.AddOption(new Menu.Option(dir.ToString(), dir));
                }
                List<Object> targets = user.Character.NearbyObjects<Object>(System.Math.Max(5, (int)user.Character.GetStatCurrent("sight")), true);

                foreach (Object o in targets)
                    if (o.Distance(user.Character) > 0)
                        menu.AddOption(new Menu.Option(o.Name, o));

                user.Menus.Enqueue(menu);
                return false;
            }

            return true;
        }
        internal static bool SelectDestination(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.Character.FindNearby<Object>(arg, (int)user.Character.GetStatCurrent("sight"), true));
            else
            {
                Menu menu = new Menu(SelectDirection, null);
                menu = new Menu(SelectDestination, null);
                List<Object> targets = user.Character.NearbyObjects<Object>((int)user.Character.GetStatCurrent("sight"), true);

                foreach (Object o in targets)
                    if (o.Distance(user.Character) > 0)
                        menu.AddOption(new Menu.Option(o.Name, o));

                user.Menus.Enqueue(menu);
                return false;
            }

            return true;
        }
        internal static bool SelectDirection(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
            {
                List<string> dirs = new List<string>();
                dirs.AddRange(Enum.GetNames(typeof(Area.ShortDirection)));
                string dir = dirs.Find(delegate(string str) { return str.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase); });
                if (dir != null) user.CurrentCommandArgs.Add((Area.Direction)Enum.Parse(typeof(Area.ShortDirection), dir));
                else
                {
                    dirs.Clear();
                    dirs.AddRange(Enum.GetNames(typeof(Area.Direction)));
                    dir = dirs.Find(delegate(string str) { return str.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase); });
                    if (dir != null) user.CurrentCommandArgs.Add(Enum.Parse(typeof(Area.Direction), dir));
                    else user.Send("Invalid direction: " + arg);
                }
            }
            else
            {
                Menu menu = new Menu(SelectDirection, null);
                for (int x = 0; x < Enum.GetValues(typeof(Area.Direction)).Length; ++x)
                {
                    Area.Direction dir = (Area.Direction)x;
                    menu.AddOption(new Menu.Option(dir.ToString(), dir));
                }

                user.Menus.Enqueue(menu);
                return false;
            }

            return true;
        }
        internal static bool SelectSpeed(User user)
        {
            if (Server.Instance != null && Server.Instance.Settings != null && Server.Instance.Settings.SimpleMovement)
            {
                user.CurrentCommandArgs.Add(1);
                return true;
            }
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
            {
                int speed;
                if (arg.Equals("walk", StringComparison.OrdinalIgnoreCase))
                    user.CurrentCommandArgs.Add(1);
                else if (int.TryParse(arg, out speed))
                    user.CurrentCommandArgs.Add(speed);
                else
                {
                    user.Send("Invalid speed.");
                    throw new AbortCommandException();
                }
            }
            else
            {
                int maxSpeed = 5;
                Stat speed = (Server.Instance != null && Server.Instance.Settings != null && Server.Instance.Settings.SpeedStat != Guid.Empty)
                    ? user.Character.GetStat(Server.Instance.Settings.SpeedStat)
                    : null;
                if (speed == null)
                    speed = user.Character.GetStat("speed");
                if (speed != null)
                    maxSpeed = (int)speed.Current;
                Menu menu = new Menu(SelectSpeed, "How fast do you want to travel [1-" + maxSpeed + "]? ");
                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectSpeedLegacyUnused(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
            {
                int speed;
                if (int.TryParse(arg, out speed)) user.CurrentCommandArgs.Add(speed);
                else
                {
                    user.Send("Invalid speed.");
                    throw new AbortCommandException();
                }
            }
            else
            {
                int maxSpeed = 5;
                Stat speed = user.Character.GetStat("speed");
                if (speed != null)
                    maxSpeed = (int)speed.Current;
                Menu menu = new Menu(SelectSpeed, "How fast do you want to travel [1-" + maxSpeed + "]? ");
                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectObjectRoom(User user)
        {
            return SelectObjectRoom<Object>(user);
        }
        internal static bool SelectItemRoom(User user)
        {
            return SelectObjectRoom<Item>(user);
        }
        internal static bool SelectNpcRoom(User user)
        {
            return SelectObjectRoom<Npc>(user);
        }
        internal static bool SelectContainerRoom(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.Character.FindNearby<IContainer>(arg, 0, true));
            else
            {
                Menu menu = new Menu(SelectContainerRoom, null);
                List<IContainer> list = user.Character.NearbyObjects<IContainer>(0, true, user.Character);
                foreach (IContainer t in list)
                    if (!(t is Object) || ((Object)t).ObjectFlags.Get((int)Object.ObjFlags.Container))
                        menu.AddOption(t.Name);

                if (menu.Options == null)
                {
                    user.Send("There are no containers here.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectCloseableObject(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.Character.FindNearby<ICloseable>(arg, 1, true));
            else
            {
                Menu menu = new Menu(SelectCloseableObject, null);
                List<ICloseable> list = user.Character.NearbyObjects<ICloseable>(1, true, user.Character);
                foreach (ICloseable t in list)
                    if (t.Closeable)
                        menu.AddOption(new Menu.Option(t.Name, t));

                if (menu.Options == null)
                {
                    user.Send("There are no closeable things here.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectPortal(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.Character.FindNearby<IPortal>(arg, 0, true));
            else
            {
                Menu menu = new Menu(SelectPortal, null);
                List<IPortal> list = user.Character.NearbyObjects<IPortal>(0, true, user.Character);

                foreach (IPortal t in list)
                    menu.AddOption(new Menu.Option(t.Name, t));

                if (menu.Options == null)
                {
                    user.Send("There are no entrances here.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectCharacterRoom(User user)
        {
            return SelectObjectRoom<Character>(user);
        }
        internal static bool SelectItemInInventory(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.Character.FindItemInInventory(arg));
            else
            {
                Menu menu = new Menu(SelectItemInInventory, null);
                List<Item> items = user.Character.Inventory;
                if (items.Count < 1)
                {
                    user.Send("You have nothing.");
                    throw new AbortCommandException();
                }

                foreach (Item item in items)
                    menu.AddOption(new Menu.Option(item.Name, item));

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectEquippedItem(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(user.Character.GetEquippedItem(arg));
            else
            {
                Menu menu = new Menu(SelectEquippedItem, null);
                foreach (Item item in user.Character.Equipment.Values)
                    menu.AddOption(new Menu.Option(item.Name, item));

                if (menu.Options == null)
                {
                    user.Send("You aren't wearing anything.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectContainer(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
            {
                IContainer container = user.Character.FindChild<IContainer>(arg, false, false);
                if (container == null)
                    container = user.Character.FindNearby<IContainer>(arg, 0, true);
                if (container == null)
                {
                    user.Send("Container not found: " + arg);
                    throw new AbortCommandException();
                }
                user.CurrentCommandArgs.Add(container);
            }
            else
            {
                Menu menu = new Menu(SelectContainer, null);
                List<IContainer> list = user.Character.GetChildren<IContainer>();
                list.AddRange(user.Character.NearbyObjects<IContainer>(0, true));
                List<IContainer> containers = new List<IContainer>();
                foreach (IContainer container in list)
                    if (((Object)container).ObjectFlags.Get((int)Object.ObjFlags.Container))
                        containers.Add(container);

                if (containers.Count < 1)
                {
                    user.Send("There are no containers here.");
                    throw new AbortCommandException();
                }

                foreach (IContainer container in containers)
                    menu.AddOption(new Menu.Option(container.Name, container));

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectItemFromContainer(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
            {
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
                return true;
            }

            object lastcmd = user.GetLastCommandArg();
            if (lastcmd == null)
            {
                user.Menus.Enqueue(new Menu(SelectItemFromContainer, null));
                return false;
            }

            IContainer container = null;
            if (lastcmd is string)
            {
                string str = (string)lastcmd;
                container = user.Character.FindChild<IContainer>(str, false, false);
                if (container == null)
                    container = user.Character.FindNearby<IContainer>(str, 0, true);
            }
            else
                container = lastcmd as IContainer;

            if (container == null)
            {
                user.Send("Container not found.");
                throw new AbortCommandException();
            }
            else if (!((Object)container).ObjectFlags.Get((int)Object.ObjFlags.Container))
            {
                user.Send("That is not a container.");
                throw new AbortCommandException();
            }
            else if (container.Closed)
            {
                user.Send(String.Capitalize(container.Name) + " is closed.");
                throw new AbortCommandException();
            }
            else if (container.Locked)
            {
                user.Send(String.Capitalize(container.Name) + " is locked.");
                throw new AbortCommandException();
            }

            if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(container.FindChild<Item>(arg, false, false));
            else
            {
                Object o = container as Object;
                List<Item> items = o.GetChildren<Item>();
                if (items.Count < 1)
                {
                    user.Send(String.Capitalize(o.Name) + " is empty.");
                    throw new AbortCommandException();
                }

                Menu menu = new Menu(SelectItemFromContainer, null);
                foreach (Item item in items)
                    menu.AddOption(new Menu.Option(item.Name, item));
                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectMoneyAmount(User user)
        {
            string arg = null;
            if (!string.IsNullOrEmpty((arg = user.GetArgument())))
            {
                user.CurrentCommandArgs.Add(arg);
            }
            else
            {
                Menu menu = new Menu(SelectMoneyAmount, "How much do you want to give? [0-" + user.Character.Value + "] ");
                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectHelp(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(Help.Get(arg));
            else
            {
                if (Help.HelpFiles.Count == 0)
                {
                    user.Send("There is no help available.");
                    throw new AbortCommandException();
                }

                Menu menu = new Menu(SelectHelp, null);
                foreach (Help help in Help.HelpFiles)
                    menu.AddOption(new Menu.Option(help.Name, help));
                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        internal static bool SelectItemToBuy(User user)
        {
            List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
            if (shops.Count < 1)
            {
                user.Send("There is nothing to buy here.");
                throw new AbortCommandException();
            }

            Shop shop = shops[0];

            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                StringTable table = new StringTable("Item", "Qty.", "Price");
                table.Columns[1].Alignment = StringTable.Column.Align.Right;
                table.Columns[2].Alignment = StringTable.Column.Align.Right;
                Menu menu = new Menu(SelectItemToBuy, "What would you like to buy? ", true, table);
                foreach (Guid id in shop.Items)
                {
                    Item item = Server.Instance.Get<Item>(id);
                    if (item != null)
                    {
                        string text = item.Name;
                        menu.AddOption(new Menu.Option(item.Name, new string[] { item.Name, "--", "{color:green}$" + shop.SellPrice(item).ToString(Server.Instance.Settings.NumberFormat) + "{color:reset}" }, null));
                    }
                }

                Dictionary<string, List<Item>> items = Utils.ConsolidateList<Item>(shop.Stock);
                foreach (KeyValuePair<string, List<Item>> pair in items)
                    menu.AddOption(new Menu.Option(pair.Key, new string[] { pair.Value[0].Name, pair.Value.Count.ToString(), "{color:green}$" + shop.SellPrice(pair.Value[0]).ToString(Server.Instance.Settings.NumberFormat) + "{color:reset}" }, null));

                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                user.CurrentCommandArgs.Add(shop);
                user.CurrentCommandArgs.Add(shop.FindItem(arg));
                return true;
            }
        }
        internal static bool SelectItemToSell(User user)
        {
            List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
            if (shops.Count < 1)
            {
                user.Send("There is nowhere to sell here.");
                throw new AbortCommandException();
            }

            Shop shop = shops[0];

            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                if (user.Character.Inventory.Count == 0)
                {
                    user.Send("You have nothing to sell.");
                    throw new AbortCommandException();
                }
                StringTable table = new StringTable("Item", "Qty.", "Price");
                table.Columns[1].Alignment = StringTable.Column.Align.Right;
                table.Columns[2].Alignment = StringTable.Column.Align.Right;
                Menu menu = new Menu(SelectItemToSell, "What would you like to sell? ", true, table);
                Dictionary<string, List<Item>> items = Utils.ConsolidateList<Item>(user.Character.Inventory);
                foreach (KeyValuePair<string, List<Item>> pair in items)
                    if (pair.Value[0].Value > 0)
                        menu.AddOption(new Menu.Option(pair.Key, new string[] { pair.Value[0].Name, pair.Value.Count.ToString(), "{color:green}$" + shop.BuyPrice(pair.Value[0]).ToString(Server.Instance.Settings.NumberFormat) + "{color:reset}" }, null));

                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                user.CurrentCommandArgs.Add(shop);
                user.CurrentCommandArgs.Add(user.character_.FindItemInInventory(arg));
                return true;
            }
        }
        internal static bool SelectForumCommand(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(SelectForumCommand, null);
                foreach (string command in MessageBoard.Commands.Keys)
                    menu.AddOption(command);
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                user.CurrentCommandArgs.Add(arg);
                return true;
            }
        }
        internal static bool SelectAliasCommand(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(SelectAliasCommand, null);
                foreach (string command in Alias.Commands.Keys)
                    menu.AddOption(command);
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                user.CurrentCommandArgs.Add(arg);
                return true;
            }
        }
        internal static bool SelectAbility(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
            {
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
                return true;
            }

            object lastcmd = user.GetLastCommandArg();
            if (lastcmd == null)
            {
                user.Menus.Enqueue(new Menu(SelectAbility, null));
                return false;
            }

            Npc teacher = null;
            if (lastcmd is string)
            {
                string str = (string)lastcmd;
                teacher = user.Character.FindNearby<Npc>(str, 0, false);
            }
            else
                teacher = lastcmd as Npc;

            if (teacher == null)
            {
                user.Send("Teacher not found.");
                throw new AbortCommandException();
            }
            else if (!teacher.CanTeach)
            {
                user.Send("They have nothing to teach you.");
                throw new AbortCommandException();
            }

            if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(Server.Instance.Get<Ability>(arg));
            else
            {
                Menu menu = new Menu(SelectAbility, null);
                foreach (KeyValuePair<Guid, int> pair in teacher.TeachableAbilities)
                {
                    Guid abilityId = pair.Key;
                    int cost = pair.Value;
                    Ability ability = Server.Instance.Get<Ability>(abilityId);
                    if (ability != null)
                    {
                        string text = ability.Name;
                        if (user.Character.Abilities.ContainsKey(abilityId))
                            text = "{color:gray}" + text + "{color:reset}";
                        if (cost > 0)
                            text += " ({color:green}" + cost.ToString(Server.Instance.Settings.NumberFormat) + "{color:reset})";
                        menu.AddOption(new Menu.Option(ability.Name, text, ability));
                    }
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        #endregion
    }
}
