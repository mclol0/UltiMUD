using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Design;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public partial class Server
    {
        #region Members
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        static Server instance_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Socket mainSocket_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        DateTime startTime_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        System.Timers.Timer clock_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        System.Timers.Timer autoSaveTimer_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        bool running_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        bool paused_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        string serverPath_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        static Random randomNumberGenerator_ = new Random();

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Dictionary<Type, Dictionary<Guid, IIdentifiable>> dataSets_ = new Dictionary<Type, Dictionary<Guid, IIdentifiable>>();

        Area mainArea_;
        IFightSystem fightSystem_;
        Settings settings_ = new Settings();
        List<User> users_ = new List<User>();
        #endregion

        #region Properties
        [Browsable(false)]
        public bool IsRunning
        {
            get { return this.running_; }
        }

        [Browsable(false)]
        public bool IsPaused
        {
            get { return this.paused_; }
        }

        public void Pause()
        {
            this.paused_ = !this.paused_;
        }

        [Browsable(false)]
        internal string ServerPath
        {
            get
            {
                if (!string.IsNullOrEmpty(this.serverPath_))
                    return this.serverPath_;
                if (MainForm.Instance != null)
                    return MainForm.Instance.ServerPath;
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\muds\\" + String.ReplaceChars((this.Settings != null && !string.IsNullOrEmpty(this.Settings.Name)) ? this.Settings.Name : "Unnamed", "/\\:*?\"<>|", '_'));
            }
        }

        [Browsable(false)]
        internal bool UnusedRunning
        {
            get { return this.running_; }
        }

        [Browsable(false)]
        public System.Timers.Timer Clock
        {
            get { return this.clock_; }
        }

        [Browsable(false)]
        public DateTime StartTime
        {
            get { return this.startTime_; }
        }

        public Settings Settings
        {
            get { return this.settings_; }
            set { this.settings_ = value; }
        }

        [Browsable(false)]
        public Area MainArea
        {
            get { return this.mainArea_; }
            set { this.mainArea_ = value; }
        }

        [Browsable(false)]
        public List<User> Users
        {
            get { return this.users_; }
            set { this.users_ = value; }
        }

        [Browsable(false)]
        static public Server Instance
        {
            get { return instance_; }
        }

        [Editor(typeof(Utils.AssemblyObjectSelector<IFightSystem>), typeof(UITypeEditor))]
        public IFightSystem FightSystem
        {
            get { return this.fightSystem_; }
            set { this.fightSystem_ = value; }
        }

        [Browsable(false)]
        public static Random RandomNumber
        {
            get { return randomNumberGenerator_; }
        }

        [Browsable(false)]
        public Dictionary<Type, Dictionary<Guid, IIdentifiable>> DataSets
        {
            get { return this.dataSets_; }
        }
        #endregion

        #region Events
        [NonSerialized] public Function_void<User> UserConnected;
        [NonSerialized] public Function_void<User> UserDisconnected;
        [NonSerialized] public Function_void<User> UserReconnected;
        [NonSerialized] public Function_void<User> UserUnloaded;
        [NonSerialized] public Function_void<User> UserLoggedIn;
        [NonSerialized] public Function_void Started;
        [NonSerialized] public Function_void Stopped;
        [NonSerialized] public Function_void Loaded;
        public delegate bool InputHandler(User user);
        #endregion

        #region Methods
        public Server()
        {
            instance_ = this;
        }
        ~Server()
        {
            //this.Save();
        }

        [Obfuscation(Exclude = false)]
        void Init(string loadpath, string serverpath)
        {
            instance_ = this;
            this.serverPath_ = serverpath;
            this.clock_ = new System.Timers.Timer(1000);
            Debug.Assert(this.DataSets != null, "DataSets is null.");
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                MethodInfo method = type.GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic);
                if (method != null)
                {
                    Debug.WriteLine("Initializing " + type.Name, "SERVER");
                    method.Invoke(null, new object[] { loadpath, serverpath });
                }
            }
        }

        [Obfuscation(Exclude = false)]
        void PostInit()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                MethodInfo method = type.GetMethod("PostInit", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (method != null)
                {
                    Debug.WriteLine("Post Initializing " + type.Name, "SERVER");
                    method.Invoke(null, null);
                }
            }
        }
        
        public Guid NewGuid()
        {
            Guid id = Guid.Empty;
            do
            {
                id = Guid.NewGuid();
            } while (this.IdInUse(id));

            return id;
        }

        [Obfuscation(Exclude = false)]
        bool IdInUse(Guid id)
        {
            foreach (Dictionary<Guid, IIdentifiable> dataset in this.DataSets.Values)
                if (dataset.ContainsKey(id))
                    return true;
            return false;
        }

        [Obfuscation(Exclude = false)]
        static public Server Load(string filename, string serverpath)
        {
            Server server = null;
            if (File.Exists(filename))
            {
                Trace.WriteLine("Loading MUD: " + filename, "SERVER");
                server = Damdai.Serialization.Binary.Deserialize<Server>(filename, new SerializationBinder());
                string loadpath = filename.Substring(0, filename.LastIndexOf('\\'));
                server.Init(loadpath, serverpath);
                server.LoadDataSets(loadpath);
                while (server.Users.Count > 0)
                    server.Unload(server.Users[0], false);
            }
            else
            {
                server = new Server();
                Area area = new Area("Default Area");
                area.AreaFlags.Set((int)Area.AreaFlag.StartingArea, true);
                server.mainArea_ = area;
                server.Init(null, serverpath ?? server.ServerPath);
            }

            server.PostInit();
            if (server.Loaded != null) server.Loaded();
            return server;
        }

        [Obfuscation(Exclude = false)]
        void autoSaveTimer__Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!this.running_)
            {
                this.autoSaveTimer_.Stop();
            }
            else
            {
                this.Save();
                this.SaveUsers();
            }
        }

        [Obfuscation(Exclude = false)]
        public void Run(string listenAddress, int port)
        {
            if (!this.running_)
            {
                Trace.WriteLine("Running on port: " + port, "SERVER");

                try
                {
                    this.mainSocket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Any;
                    if (!string.IsNullOrEmpty(listenAddress) && !IPAddress.TryParse(listenAddress, out ip))
                        ip = IPAddress.Any;
                    this.mainSocket_.Bind(new IPEndPoint(ip, port));
                    this.mainSocket_.Listen(4);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.running_ = true;
                this.Settings.Port = port;

                Thread gameLoopThread = new Thread(new ThreadStart(this.GameLoop));
                gameLoopThread.Name = "Game Loop Thread";
                gameLoopThread.IsBackground = true;
                gameLoopThread.Start();

                Thread connectionThread = new Thread(new ThreadStart(this.AcceptConnections));
                connectionThread.Name = "Connection Thread";
                connectionThread.IsBackground = true;
                connectionThread.Start();

                Thread dataThread = new Thread(new ThreadStart(this.ProcessData));
                dataThread.Name = "Data Thread";
                dataThread.IsBackground = true;
                dataThread.Start();

                this.autoSaveTimer_ = new System.Timers.Timer(300000);
                this.autoSaveTimer_.Elapsed += new System.Timers.ElapsedEventHandler(autoSaveTimer__Elapsed);
                this.autoSaveTimer_.Start();

                this.startTime_ = DateTime.Now;
                this.clock_.Start();

                if (this.Started != null) this.Started();
            }
        }
        
        public void Stop()
        {
            Trace.WriteLine("Stopping.", "SERVER");
            this.running_ = false;
            if (this.autoSaveTimer_ != null)
                this.autoSaveTimer_.Stop();
            this.Broadcast("Stopping server.");
            this.CloseSockets();
            this.UnloadUsers();
            if (this.clock_ != null)
                this.clock_.Stop();
            if (this.Stopped != null) this.Stopped();
        }

        [Obfuscation(Exclude = false)]
        public void Save()
        {
            if (string.IsNullOrEmpty(this.Settings.Name))
            {
                Debug.WriteLine("Unable to save because the name of this MUD has not been set.", "WARNING");
                return;
            }

            try
            {
                lock (this)
                {
                    string path = this.ServerPath;
                    Directory.CreateDirectory(path);
                    string gameFile = path + "\\" + String.ReplaceChars(this.Settings.Name, "/\\:*?\"<>|", '_') + ".mud";
                    if (StoredSettings.Instance != null)
                        StoredSettings.Instance.GameFile = gameFile;
                    try
                    {
                        if (File.Exists(gameFile + ".new"))
                            File.Delete(gameFile + ".new");
                        if (File.Exists(gameFile + ".old"))
                            File.Delete(gameFile + ".old");
                        Damdai.Serialization.Binary.Serialize(this, gameFile + ".new");
                        if (File.Exists(gameFile))
                            File.Move(gameFile, gameFile + ".old");
                        File.Move(gameFile + ".new", gameFile);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.ToString(), "ERROR");
                        if (MainForm.Instance != null)
                            MessageBox.Show("Failed to save server.\r\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (File.Exists(gameFile + ".new"))
                            File.Delete(gameFile + ".new");
                    }

                    if (File.Exists(gameFile + ".old"))
                        File.Delete(gameFile + ".old");

                    this.SaveDataSets(path);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString(), "ERROR");
                if (MainForm.Instance != null)
                    MessageBox.Show(e.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [Obfuscation(Exclude = false)]
        public void SaveUsers()
        {
            lock (this.Users)
            {
                foreach (User user in this.Users)
                    if (user.Flags.Get((int)User.UserFlags.LoggedIn))
                        user.Save();
            }
        }

        [Obfuscation(Exclude = false)]
        void SaveDataSets(string path)
        {
            foreach (KeyValuePair<Type, Dictionary<Guid, IIdentifiable>> dataset in this.DataSets)
            {
                try
                {
                    Damdai.Serialization.Binary.Serialize(dataset.Value, path + "\\" + dataset.Key.Name + ".ds");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString(), "ERROR");
                    if (MainForm.Instance != null) MessageBox.Show("Failed to save " + dataset.Key.Name + " objects.\r\nContact damdai@hotmail.com.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        [Obfuscation(Exclude = false)]
        void LoadDataSets(string loadpath)
        {
            List<Type> types = new List<Type>();
            foreach (Type type in this.DataSets.Keys)
                types.Add(type);

            SerializationBinder binder = new SerializationBinder();
            foreach(Type type in types)
            {
                try
                {
                    string filepath = loadpath + "\\" + type.Name + ".ds";
                    if (File.Exists(filepath))
                    {
                        this.DataSets[type] =
                            Damdai.Serialization.Binary.Deserialize<Dictionary<Guid, IIdentifiable>>(filepath, binder);
                    }
                    else
                        Trace.WriteLine(type.Name + " dataset file not found.", "WARNING");
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString(), "ERROR");
                    if (MainForm.Instance != null) MessageBox.Show("Failed to load " + type.Name + " dataset.\r\nContact damdai@hotmail.com.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        [Obfuscation(Exclude = false)]
        void AcceptConnections()
        {
            while (this.running_)
            {
                try
                {
                    Socket s = this.mainSocket_.Accept();
                    this.HandleNewConnection(s);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Failed to accept new connection: " + e.Message, "ERROR");
                    break;
                }
            }
        }

        [Obfuscation(Exclude = false)]
        void HandleNewConnection(Socket socket)
        {
            socket.ReceiveTimeout = 10000;
            User user = new User(socket, this.NewGuid());
            Trace.WriteLine("User connected: " + user.IpAddress, "SERVER");

            Telnet.AdvertiseProtocols(user);
            user.Send(this.Settings.TitleScreen + "\r\n{color:reset}UltiMUD v" + this.GetType().Assembly.GetName().Version.ToString() + " (c) DAMDAI (www.ultimud.com)");

            this.Add(user);
            if (this.UserConnected != null) this.UserConnected(user);
            this.GetName(user);
            this.FlushOutput();
        }

        [Obfuscation(Exclude = false)]
        void ProcessData()
        {
            while (this.running_)
            {
                if (this.Users.Count > 0)
                {
                    try
                    {
                        lock (this.users_)
                        {
                            foreach (User user in this.Users)
                                if (user.Socket != null && user.Socket.Poll(10, System.Net.Sockets.SelectMode.SelectRead))
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.HandleInput), user);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Failed to process data: " + e.ToString(), "ERROR");
                    }
                }
                Thread.Sleep(100);
            }
        }

        [Obfuscation(Exclude = false)]
        void GameLoop()
        {
            while (this.running_)
            {
                try
                {
                    if (!this.paused_)
                        this.UpdateObjects();
                    this.UpdateUsers();
                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    Trace.WriteLine(e.Message, "ERROR");
                }
            }
        }

        [Obfuscation(Exclude = false)]
        void HandleInput(object o)
        {
            User user = o as User;
            if (user == null)
                return;
            try
            {
                if (!this.ReadFromSocket(user))
                    return;

                bool processedAny = false;
                lock (user.InputLock)
                {
                    while (true)
                    {
                        int newline = user.Input.IndexOfAny("\n\r".ToCharArray());
                        if (newline > -1)
                        {
                            processedAny = true;
                            user.ProcessingInput = true;
                            user.Command = user.Input.Substring(0, newline);

                            if (user.InputReceived != null)
                                user.InputReceived(user, user.Command);

                            if (user.Command.Trim() == "!")
                                user.Command = user.LastCommand ?? string.Empty;
                            else
                                user.LastCommand = user.Command;

                            int removeCount = newline + 1;
                            if (removeCount < user.Input.Length)
                            {
                                char c1 = user.Input[newline];
                                char c2 = user.Input[removeCount];
                                if ((c1 == '\r' && c2 == '\n') || (c1 == '\n' && c2 == '\r'))
                                    removeCount++;
                            }
                            user.Input = user.Input.Remove(0, removeCount).TrimStart(' ');

                            try
                            {
                                if (user.Command.Length > 0)
                                {
                                    while (user.CurrentMenu != null && !string.IsNullOrEmpty(user.Command) && user.CurrentMenu.ProcessCommand(user))
                                    {
                                    }

                                    if (user.CurrentMenu == null &&
                                        (!string.IsNullOrEmpty(user.Command) || user.CurrentCommandArgs.Count > 0))
                                    {
                                        if (user.CurrentInputHandler != null && user.CurrentInputHandler(user))
                                            user.FinishInput();
                                    }
                                }
                                else if (user.CurrentMenu != null && user.CurrentMenu.Options == null && user.CurrentMenu.InputHandler != null)
                                {
                                    if (user.CurrentMenu.ProcessCommand(user))
                                        user.FinishInput();
                                }

                                if (user.CurrentMenu != null)
                                {
                                    if (user.CurrentMenu.Prompt == null && user.CurrentMenu.Options == null)
                                    {
                                        Menu menu = user.Menus.Dequeue();
                                        if (menu.InputHandler(user))
                                            user.FinishInput();
                                    }

                                    if (user.CurrentMenu != null && user.CurrentMenu.Options != null)
                                        user.Send(user.CurrentMenu.ToString());
                                }
                            }
                            catch (AbortCommandException)
                            {
                                user.FinishInput();
                            }

                            user.FlushOutput();
                            user.SendPrompt();
                            user.ProcessingInput = false;
                            this.FlushOutput();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (processedAny)
                    user.LastActivity = DateTime.Now;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                Trace.WriteLine(e.Message, "ERROR");
                user.FinishInput();
                user.FlushOutput();
                user.SendPrompt();
                user.ProcessingInput = false;
                this.FlushOutput();
            }
        }

        [Obfuscation(Exclude = false)]
        bool ReadFromSocket(User user)
        {
            if (user == null || user.Socket == null)
                return false;

            byte[] buf = new byte[2048];
            int read = 0;
            try
            {
                lock (user.Socket)
                    read = user.Socket.Receive(buf);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error reading from socket, disconnecting user...", "ERROR");
                Debug.WriteLine(e.ToString());
                this.Disconnect(user);
                return false;
            }

            if (read <= 0)
            {
                Debug.WriteLine(read + " bytes read, disconnecting user...", "SERVER");
                this.Disconnect(user);
                return false;
            }
            else if (read > 1024)
            {
                Trace.WriteLine("Input overflow: " + read + " bytes read, disconnecting user...", "WARNING");
                user.SendError("Input overflow. Disconnecting...");
                this.Disconnect(user);
                return false;
            }

            int cleanLen = 0;
            byte[] cleanBuf = Telnet.StripTelnetNegotiation(buf, read, out cleanLen, user);
            if (cleanLen <= 0)
                return false;

            string decoded = System.Text.Encoding.UTF8.GetString(cleanBuf, 0, cleanLen);
            lock (user.InputLock)
            {
                user.Input += decoded;
                user.Input = String.FixString(user.Input);
            }

            if (user.ProcessingInput)
                return false;

            return true;
        }

        [Obfuscation(Exclude = false)]
        bool ReadFromSocketLegacyUnused(User user)
        {
            byte[] buf = new byte[2048];
            int read = 0;
            try
            {
                lock (user.Socket)
                    read = user.Socket.Receive(buf);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error reading from socket, disconnecting user...", "ERROR");
                Debug.WriteLine(e.ToString());
                this.Disconnect(user);
                return false;
            }

            if (read <= 0)
            {
                Debug.WriteLine(read + " bytes read, disconnecting user...", "SERVER");
                this.Disconnect(user);
                return false;
            }
            else if (read > 1024)
            {
                Trace.WriteLine("Input overflow: " + read + " bytes read, disconnecting user...", "WARNING");
                user.SendError("Input overflow. Disconnecting...");
                this.Disconnect(user);
                return false;
            }
            else if (user.ProcessingInput)
            {
                return false;
            }

            char[] chars = new char[read];
            System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
            d.GetChars(buf, 0, read, chars, 0);
            lock (user.Input)
            {
                user.Input += new string(chars);
                user.Input = String.FixString(user.Input);
            }
            return true;
        }

        [Obfuscation(Exclude = false)]
        public bool ProcessCommand(User user)
        {
            return this.ProcessCommand(user, false);
        }

        [Obfuscation(Exclude = false)]
        public bool ProcessCommand(User user, bool includescriptcommands)
        {
            Debug.Assert(user.Character != null, "Character is null.");
            if (user.Character == null)
            {
                Trace.WriteLine("Unable to process command, character is null.", "ERROR");
                return true;
            }
            if (user.Character.Recovering)
            {
                user.Send("You can't do anything while you're recovering.");
                return true;
            }

            user.CurrentCommandArgs.Clear();
            user.CurrentArg = null;
            user.Command = user.Command.Trim();
            string strCommand = user.GetArgument();

            if ((user.Character.FightSystem == null || user.Character.FightSystem.ConvertAlias(user)) && user.Aliases.ContainsKey(strCommand))
            {
                string[] aliasedCommand = user.Aliases[strCommand].Split(" ".ToCharArray(), 2);
                strCommand = aliasedCommand[0];
                if (aliasedCommand.Length > 1)
                    user.Command = user.Command.Insert(0, aliasedCommand[1] + " ");
            }

            //Debug.WriteLine("Processing command: " + strCommand, "SERVER");

            if (user.Character.FightSystem != null && user.Character.FightSystem.ProcessInput(user, strCommand))
                return true;

            Command command = null;

            if (command == null && (command = Command.Get(strCommand, includescriptcommands)) == null)
            {
                bool result = false;
                if (this.ProcessCommand<Ability>(user, strCommand, out result) ||
                    this.ProcessCommand<Channel>(user, strCommand, out result) ||
                    this.ProcessCommand<Social>(user, strCommand, out result))
                    return result;

                if (command == null)
                {
                    user.Send("Command not found: " + strCommand);
                    return true;
                }
            }
            else
            {
                if (!command.CanUseIncapacitated && user.Character.Incapacitated)
                {
                    user.Character.Send("You can't do that in your current state.");
                    return true;
                }
                if (user.Character.FightSystem == null || user.Character.FightSystem.CanUseCommand(user, command))
                {
                    user.InputHandler = command.DoScript;
                    return command.DoScript(user);
                }
            }
            return true;
        }

        [Obfuscation(Exclude = false)]
        bool ProcessCommand<T>(User user, string command, out bool result) where T : class, ICommand, new()
        {
            T t = new T().Get(command) as T;
            if (t != null)
            {
                result = t.ProcessCommand(user);
                return true;
            }
            result = false;
            return false;
        }

        [Obfuscation(Exclude = false)]
        void Add(User user)
        {
            lock (this.Users)
                this.Users.Add(user);
        }

        [Obfuscation(Exclude = false)]
        void Remove(User user)
        {
            lock (this.Users)
                this.Users.Remove(user);
        }
        
        public void Unload(User user, bool save)
        {
            user.Flags.Set((int)User.UserFlags.LoggedIn, false);
            if (save)
                user.Save();
            if (user.Character != null)
            {
                user.Character.Parent = null;
                user.Character.User = null;
            }
            this.Remove(user);
            if (this.UserUnloaded != null)
                this.UserUnloaded(user);
            user = null;
        }
        
        public void Disconnect(User user)
        {
            try
            {
                Trace.WriteLine((user.Character != null ? user.Character.Name : user.IpAddress) + " has disconnected.", "SERVER");
                user.FlushAllOutput();
                if (user.Character != null && user.Character.Area != null)
                {
                    String str = new String(null, null, null, null, "{1} suddenly gets a blank look on {1:sex(it's,his,her)} face.", null);
                    user.Character.Area.Send(str, user.Character, null, 0);
                    this.FlushOutput();
                }

                if (!user.Flags.Get((int)User.UserFlags.LoggedIn))
                {
                    this.Unload(user, false);
                }
                else if (user.Character != null)
                {
                    this.Settings.InfoChannel.Broadcast(InfoChannel.Event.Logout, user.Character.Name + " has disconnected.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString(), "ERROR");
            }
            finally
            {
                if (user.Socket != null)
                {
                    try { user.Socket.Close(); } catch { }
                    user.Socket = null;
                }
                if (this.UserDisconnected != null)
                    this.UserDisconnected(user);
            }
        }

        public void DisconnectLegacyUnused(User user)
        {
            try
            {
                Trace.WriteLine((user.Character != null ? user.Character.Name : user.IpAddress) + " has disconnected.", "SERVER");
                user.FlushAllOutput();
                if (user.Character != null && user.Character.Area != null)
                {
                    String str = new String(null, null, null, null, "{1} suddenly gets a blank look on {1:sex(it's,his,her)} face.", null);
                    user.Character.Area.Send(str, user.Character, null, 0);
                    this.FlushOutput();
                }

                if (!user.Flags.Get((int)User.UserFlags.LoggedIn))
                    this.Unload(user, false);
                else if (user.Character != null)
                    this.Settings.InfoChannel.Broadcast(InfoChannel.Event.Logout, user.Character.Name + " has disconnected.");
                    this.Unload(user, false);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString(), "ERROR");
            }
            finally
            {
                if (user.Socket != null)
                {
                    user.Socket.Close();
                    user.Socket = null;
                }
                if (this.UserDisconnected != null)
                    this.UserDisconnected(user);
            }
        }

        [Obfuscation(Exclude = false)]
        void CloseSockets()
        {
            if (this.mainSocket_ != null)
            {
                this.mainSocket_.Close();
                this.mainSocket_ = null;
            }

            foreach (User user in Damdai.Collections.Util.CopyList<User>(this.Users))
                this.Disconnect(user);
        }

        [Obfuscation(Exclude = false)]
        void UpdateObjects()
        {
            this.MainArea.Update();
            Server.Instance.FlushOutput();
        }

        [Obfuscation(Exclude = false)]
        void UpdateUsers()
        {
            DateTime now = DateTime.Now;
            foreach (User user in Damdai.Collections.Util.CopyList<User>(this.users_))
            {
                if (!user.Connected)
                {
                    if (user.Character == null || user.Character.FightSystem == null)
                        this.Unload(user, user.Flags.Get((int)User.UserFlags.LoggedIn));
                }
                else if (!user.Flags.Get((int)User.UserFlags.LoggedIn) && (now - user.LastActivity).TotalMinutes > 2)
                {
                    user.Send("Idle timeout, disconnecting...");
                    this.Disconnect(user);
                    this.Unload(user, false);
                }
            }
        }

        [Obfuscation(Exclude = false)]
        void UnloadUsers()
        {
            lock (this.users_)
            {
                while (this.Users.Count > 0)
                    this.Unload(this.Users[0], true);
            }
        }
        
        void Broadcast(string str)
        {
            foreach (User user in Damdai.Collections.Util.CopyList<User>(this.Users))
                user.Send(this.Settings.GetColorCode(Settings.Text.Server) + "SERVER: " + str);
        }
        
        public void Echo(string str, params Object[] ignore)
        {
            List<Object> ignoreList = new List<Object>(ignore);
            foreach (User user in Damdai.Collections.Util.CopyList<User>(this.Users))
                if (user.Flags.Get((int)User.UserFlags.LoggedIn) && !ignoreList.Contains(user.Character))
                    user.Send(str);
        }

        [Obfuscation(Exclude = false)]
        public void FlushOutput()
        {
            foreach (User user in Damdai.Collections.Util.CopyList<User>(this.Users))
                user.FlushOutput();
        }
        
        public List<Area> GetAreas()
        {
            return this.GetAreas(this.MainArea);
        }
        
        public List<Area> GetAreas(Area area)
        {
            if (this.MainArea == null)
            {
                Debug.WriteLine("Main area is null.", "WARNING");
                return null;
            }

            List<Area> areas = new List<Area>();
            areas.Add(area);
            foreach (Area a in area.GetChildren<Area>())
                areas.AddRange(this.GetAreas(a));

            return areas;
        }
        
        public Area GetArea(string str)
        {
            str = str.ToLower();
            foreach (Area area in this.GetAreas())
                if (area.Name.Equals(str, StringComparison.CurrentCultureIgnoreCase))
                    return area;

            return null;
        }

        [Obfuscation(Exclude = false)]
        public T Find<T>(Guid id) where T : Object
        {
            if (this.MainArea == null || this.MainArea.Id == id)
                return this.MainArea as T;
            else
                return this.MainArea.FindChild<T>(id, true);
        }

        [Obfuscation(Exclude = false)]
        public T Find<T>(string name) where T : Object
        {
            if (this.MainArea == null || (typeof(T) == typeof(Area) && this.MainArea.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase)))
                return this.MainArea as T;
            else
                return this.MainArea.FindChild<T>(name, false, true);
        }

        [Obfuscation(Exclude = false)]
        public void Add<T>(T t) where T : class, IIdentifiable
        {
            Dictionary<Guid, IIdentifiable> dataset = this.DataSets[typeof(T)];

            if (t.Id == Guid.Empty)
            {
                if (this.Get<T>(t.Name) != null)
                    throw new Exception("That name already exists.");
                t.Id = this.NewGuid();
            }

            lock (dataset)
            {
                if (dataset.ContainsKey(t.Id))
                    dataset[t.Id] = t;
                else
                    dataset.Add(t.Id, t);
            }
        }

        [Obfuscation(Exclude = false)]
        public void Remove<T>(Guid id)
        {
            Dictionary<Guid, IIdentifiable> dataset = this.DataSets[typeof(T)];
            lock (dataset)
                dataset.Remove(id);
        }

        [Obfuscation(Exclude = false)]
        public T Get<T>(Guid id) where T : class, IIdentifiable, new()
        {
            Dictionary<Guid, IIdentifiable> dataset = null;
            if (this.DataSets.TryGetValue(typeof(T), out dataset))
                if (id != Guid.Empty && dataset.ContainsKey(id))
                    return dataset[id] as T;
            return null;
        }

        [Obfuscation(Exclude = false)]
        public T Get<T>(string name) where T : class, IIdentifiable
        {
            Dictionary<Guid, IIdentifiable> dataset = this.DataSets[typeof(T)];
            lock (dataset)
            {
                foreach (T t in dataset.Values)
                    if (String.FindArgument(t.Name, name))
                        return t;
            }
            return null;
        }
        
        public IIdentifiable Get(string type, string name)
        {
            if (!type.StartsWith("Ultimud."))
                type = "Ultimud." + type;
            Type t = Type.GetType(type);
            Dictionary<Guid, IIdentifiable> dataset = this.DataSets[t];
            lock (dataset)
            {
                foreach (IIdentifiable i in dataset.Values)
                    if (String.FindArgument(i.Name, name))
                        return i;
            }
            return null;            
        }
        
        public IIdentifiable Get(string type, Guid id)
        {
            if (!type.StartsWith("Ultimud."))
                type = "Ultimud." + type;
            Type t = Type.GetType(type);
            Dictionary<Guid, IIdentifiable> dataset = null;
            if (this.DataSets.TryGetValue(t, out dataset))
                if (id != Guid.Empty && dataset.ContainsKey(id))
                    return dataset[id];
            return null;
        }
        
        public string GetObjectBlueprintName(Guid id)
        {
            Object o = this.GetObjectBlueprint(id);
            if (o != null)
                return o.Name;
            else
                return null;
        }
        
        public Object GetObjectBlueprint(Guid id)
        {
            Object o = this.Get<Item>(id);
            if (o == null)
                o = this.Get<Npc>(id);
            if (o != null)
                return o;
            return null;
        }

        [Obfuscation(Exclude = false)]
        bool ValidateName(string name, out string error)
        {
            Regex regex = new Regex("[^a-zA-Z]");
            if (regex.IsMatch(name))
            {
                error = "Your name may only contain letters.";
                return false;
            }
            if (name.Length <3 || name.Length > 15)
            {
                error = "Your name must be between 3 and 15 characters.";
                return false;
            }
            if (this.Get<Npc>(name) != null)
            {
                error = "That name is already in use.";
                return false;
            }
            error = null;
            return true;
        }

        [Obfuscation(Exclude = false)]
        void NotifyCentralServer()
        {
            //try
            //{
            //    using (com.damdai.www.WebService service = new com.damdai.www.WebService())
            //    {
            //        while (this.running_)
            //        {
            //            service.Register(this.Settings.Name, this.Settings.Description, this.Users.Count, this.Settings.Port);
            //            Thread.Sleep(60000);
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine("Failed to notify central server: " + e.Message);
            //}
        }

        [Obfuscation(Exclude = false)]
        void CheckAbilities(Character ch)
        {
            Race race = ch.Race;
            Job job = ch.Job;
            if (race != null)
            {
                foreach (Guid id in race.Abilities)
                    if (!ch.Abilities.ContainsKey(id))
                        ch.Abilities.Add(id, 0);
            }

            if (job != null)
            {
                foreach (Guid id in job.Abilities)
                    if (!ch.Abilities.ContainsKey(id))
                        ch.Abilities.Add(id, 0);
            }
        }

        [Obfuscation(Exclude = false)]
        public T Create<T>(string name) where T : class, IIdentifiable
        {
            T t = this.Get<T>(name);
            if (t == null) throw new ApplicationException("Blueprint not found: " + name);
            return (T)typeof(T).GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { t });
        }
        
        public object Create(string type, string name)
        {
            type = type.Replace("Ultimud.", null);
            if (type == "Item")
                return this.Create<Item>(name);
            else if (type == "Npc")
                return this.Create<Npc>(name);
            else return null;
        }

        #region Login Methods
        [Obfuscation(Exclude = false)]
        bool GetName(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                user.Menus.Enqueue(new Menu(this.GetName, "Name: "));
                return false;
            }

            System.Globalization.TextInfo i = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            string name = i.ToTitleCase(user.Command);
            user.Command = null;

            string error = null;
            if (!this.ValidateName(name, out error))
            {
                user.SendError(error);
                return this.GetName(user);
            }

            //Check if player already loaded.
            if (this.CheckReconnect(user, name))
                return true;

            //TEMP
#pragma warning disable 0429
            if (false && this.Users.Count >= 10)
#pragma warning restore 0429
            {
                user.Send("While in beta testing phase, this server will only allow 10 simultaneous connections.\r\nThis server is currently full.");
                this.Disconnect(user);
                this.Unload(user, false);
                return true;
            }

            user.Character = new Player();
            user.Character.Name = name;

            string filename = this.ServerPath + "\\Players\\" + user.Character.Name;
            if (!File.Exists(filename))
            {
                if (this.Settings.Flags.Get((int)Settings.Flag.Newlocked))
                {
                    user.Send("New accounts are not being allowed at this time.");
                    this.Disconnect(user);
                    this.Unload(user, false);
                    return true;
                }
                return this.QueryCreateUser(user);
            }
            else
            {
                try
                {
                    user.Load();
                    user.Flags.Set((int)User.UserFlags.LoggedIn, false);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Failed to load user: " + user.Character.Name, "ERROR");
                    Debug.WriteLine(e.ToString(), "ERROR");
                    user.Send("Failed to load your character.\r\nContact damdai@hotmail.com for assistance.");
                    this.Disconnect(user);
                    this.Unload(user, false);
                    return true;
                }
                return this.GetPassword(user);
            }
        }

        [Obfuscation(Exclude = false)]
        bool CheckReconnect(User user, string name)
        {
            foreach (User u in Damdai.Collections.Util.CopyList<User>(this.users_))
            {
                if (u.Character != null && u.Character.Name == name)
                {
                    if (u.Socket == null)
                    {
                        user.Init(u);
                        this.Remove(u);
                        if (this.UserUnloaded != null) this.UserUnloaded(u);
                        user.Send("Reconnecting...");
                        if (this.UserReconnected != null) this.UserReconnected(user);
                        this.GetPassword(user);
                    }
                    else
                    {
                        user.Send("A user by that name is already logged in.");
                        this.Disconnect(user);
                        this.Unload(user, false);
                    }
                    return true;
                }
            }

            return false;
        }

        [Obfuscation(Exclude = false)]
        bool GetPassword(User user)
        {
            if (user.Command == string.Empty)
            {
                Telnet.SetEcho(user, false);
                user.Menus.Enqueue(new Menu(new InputHandler(this.GetPassword), "Password: "));
                return false;
            }

            Telnet.SetEcho(user, true);
            string password = user.Command;
            user.Command = null;
            if (!user.VerifyPassword(password))
            {
                user.Send("Incorrect password.");
                this.GetPassword(user);
            }
            else
            {
                this.Login(user);
            }

            return true;
        }

        [Obfuscation(Exclude = false)]
        bool GetPasswordLegacyUnused(User user)
        {
            if (user.Command == string.Empty)
            {
                user.Menus.Enqueue(new Menu(new InputHandler(this.GetPassword), "Password: "));
                return false;
            }

            string password = user.Command;
            user.Command = null;
            if (password != user.Password)
            {
                user.Send("Incorrect password.");
                this.GetPassword(user);
            }
            else
            {
                this.Login(user);
            }

            return true;
        }

        [Obfuscation(Exclude = false)]
        bool QueryCreateUser(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                user.Menus.Enqueue(new Menu(new InputHandler(this.QueryCreateUser),
                    "User not found. Create new user " + user.Character.Name + "? "));
                return false;
            }

            string command = user.Command.ToLower();
            user.Command = null;
            if ("yes".StartsWith(command, StringComparison.CurrentCultureIgnoreCase))
            {
                this.GetNewPassword(user);
            }
            else if ("no".StartsWith(command, StringComparison.CurrentCultureIgnoreCase))
            {
                user.Character.Name = null;
                this.GetName(user);
            }
            else
            {
                user.Send("Please enter Yes or No.");
                return this.QueryCreateUser(user);
            }

            return true;
        }

        [Obfuscation(Exclude = false)]
        bool QueryCreateUserLegacyUnused(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                user.Menus.Enqueue(new Menu(new InputHandler(this.QueryCreateUser),
                    "User not found. Create new user " + user.Character.Name + "? "));
                return false;
            }

            string command = user.Command.ToLower();
            user.Command = null;
            if ("yes".StartsWith(command, StringComparison.CurrentCultureIgnoreCase))
                this.GetNewPassword(user);
            else// if ("no".StartsWith(command))
            {
                user.Character.Name = null;
                this.GetName(user);
            }

            return true;
        }

        [Obfuscation(Exclude = false)]
        bool GetNewPassword(User user)
        {
            if (user.Command == string.Empty)
            {
                Telnet.SetEcho(user, false);
                user.Menus.Enqueue(new Menu(new InputHandler(this.GetNewPassword), "New password: "));
                return false;
            }

            Telnet.SetEcho(user, true);
            string password = user.Command;
            user.Command = null;
            if (password.Length < 4)
            {
                user.SendError("Password must be at least 4 characters long.");
                return this.GetNewPassword(user);
            }
            user.SetPassword(password);
            this.ConfirmPassword(user);
            return true;
        }

        [Obfuscation(Exclude = false)]
        bool GetNewPasswordLegacyUnused(User user)
        {
            if (user.Command == string.Empty)
            {
                user.Menus.Enqueue(new Menu(new InputHandler(this.GetNewPassword), "New password: "));
                return false;
            }

            string password = user.Command;
            user.Command = null;
            //TODO: Check if password meets requirements.
            user.Password = password;
            this.ConfirmPassword(user);
            return true;
        }

        [Obfuscation(Exclude = false)]
        bool ConfirmPassword(User user)
        {
            if (user.Command == string.Empty)
            {
                Telnet.SetEcho(user, false);
                user.Menus.Enqueue(new Menu(new InputHandler(this.ConfirmPassword), "Confirm password: "));
                return false;
            }

            Telnet.SetEcho(user, true);
            string password = user.Command;
            user.Command = null;
            if (!user.VerifyPassword(password))
            {
                user.Send("Passwords don't match.");
                this.GetNewPassword(user);
            }
            else
            {
                this.GetSex(user);
            }

            return true;
        }

        [Obfuscation(Exclude = false)]
        bool ConfirmPasswordLegacyUnused(User user)
        {
            if (user.Command == string.Empty)
            {
                user.Menus.Enqueue(new Menu(new InputHandler(this.ConfirmPassword), "Confirm password: "));
                return false;
            }

            string password = user.Command;
            user.Command = null;
            if (password != user.Password)
            {
                user.Send("Passwords don't match.");
                this.GetNewPassword(user);
            }
            else
            {
                this.GetSex(user);
            }

            return true;
        }

        [Obfuscation(Exclude = false)]
        bool GetSex(User user)
        {
            string[] options = Enum.GetNames(typeof(Object.Sex));

            if (string.IsNullOrEmpty(user.Command))
            {
                user.Flags.Set((int)User.UserFlags.Spacing, true);
                user.Flags.Set((int)User.UserFlags.Creating, true);
                Menu menu = new Menu(new InputHandler(this.GetSex),
                    "Your sex is for roleplay purposes only. It has no affect on your stats, abilities, or potential.",
                    "Select your gender: ", false);
                menu.AddOptions(options);
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                string sex = user.GetArgument();

                for (int x = 0; x < options.Length; ++x)
                {
                    if (options[x] == sex)
                    {
                        user.Character.Gender = (Object.Sex)x;
                        break;
                    }
                }

                this.GetRace(user);
                return true;
            }
        }

        [Obfuscation(Exclude = false)]
        bool GetRace(User user)
        {
            List<Race> races = new List<Race>();
            lock (this.dataSets_[typeof(Race)])
            {
                foreach (Race r in this.DataSets[typeof(Race)].Values) if (r.Selectable) races.Add(r);
            }

            if (races.Count > 1)
            {
                if (string.IsNullOrEmpty(user.Command))
                {
                    Menu menu = new Menu(this.GetRace, this.settings_.SelectRaceHelp, this.settings_.SelectRacePrompt, false);
                    foreach (Race race in races)
                    {
                        string mods = String.ToString<StatModifier>(race.BaseStatModifiers);
                        menu.AddOption(new Menu.Option(race.Name, race.Name + (!string.IsNullOrEmpty(mods) ? " ({color:gray}" + mods + "{color:reset})" : null), null));
                    }
                    user.Menus.Enqueue(menu);
                    return false;
                }
                else
                {
                    string arg = user.GetArgument().ToLower();
                    foreach (Race race in races)
                    {
                        if (race.Name.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase))
                        {
                            user.Character.Race = race;
                            break;
                        }
                    }

                    if (user.Character.Race == null)
                    {
                        user.Send("Invalid selection: " + arg);
                        return false;
                    }
                }
            }
            else if (races.Count == 1)
                user.Character.Race = races[0];

            if (user.Character.Race != null)
                user.Character.Race.Apply(user.Character);

            this.GetJob(user);
            return true;
        }

        [Obfuscation(Exclude = false)]
        bool GetJob(User user)
        {
            List<Job> jobs = new List<Job>();
            lock (this.dataSets_[typeof(Job)])
            {
                foreach (Job j in this.DataSets[typeof(Job)].Values) if (j.Selectable) jobs.Add(j);
            }

            if (jobs.Count > 1)
            {
                if (string.IsNullOrEmpty(user.Command))
                {
                    Menu menu = new Menu(this.GetJob, this.settings_.SelectJobHelp, this.settings_.SelectJobPrompt, false);
                    foreach (Job job in jobs)
                    {
                        string mods = String.ToString<StatModifier>(job.BaseStatModifiers);
                        menu.AddOption(new Menu.Option(job.Name, job.Name + (!string.IsNullOrEmpty(mods) ? " ({color:gray}" + mods + "{color:reset})" : null), null));
                    }
                    user.Menus.Enqueue(menu);
                    return false;
                }
                else
                {
                    string arg = user.GetArgument().ToLower();
                    foreach (Job job in jobs)
                    {
                        if (job.Name.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase))
                        {
                            user.Character.Job = job;
                            break;
                        }
                    }

                    if (user.Character.Job == null)
                    {
                        user.Send("Invalid selection: " + arg);
                        return false;
                    }
                }
            }
            else if (jobs.Count == 1)
            {
                user.Character.Job = jobs[0];
            }

            if (user.Character.Job != null)
                user.Character.Job.Apply(user.Character);

            this.GetStartingArea(user);
            return true;
        }

        [Obfuscation(Exclude = false)]
        bool GetStartingArea(User user)
        {
            List<Location> startingLocations = new List<Location>();
            Race race = user.Character.Race;
            Job job = user.Character.Job;
            if (race != null && race.StartLocation != null && race.StartLocation.IsValid)
                startingLocations.Add(race.StartLocation);
            if (job != null && job.StartLocation != null && job.StartLocation.IsValid)
                startingLocations.Add(job.StartLocation);

            if (startingLocations.Count == 0)
            {
                foreach (Area area in this.GetAreas().FindAll(delegate(Area a) { return a.AreaFlags.Get((int)Area.AreaFlag.StartingArea); }))
                    startingLocations.Add(new Location(area, area.Start));
            }

            if (startingLocations.Count > 1)
            {
                if (string.IsNullOrEmpty(user.Command))
                {
                    Menu menu = new Menu(
                        new InputHandler(this.GetStartingArea), 
                        "Your starting location can greatly influence your character's development.", 
                        "Select starting area: ", false);
                    foreach (Location l in startingLocations)
                        menu.AddOption(l.Area.Name);

                    if (menu.Options == null)
                    {
                        user.StartLocation = new Location(this.MainArea, this.MainArea.Start);
                        this.FinishCreation(user);
                        return true;
                    }

                    user.Menus.Enqueue(menu);
                    return false;
                }
                else
                {
                    string arg = user.GetArgument();
                    foreach (Location l in startingLocations)
                    {
                        if (l.Area.Name.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase))
                        {
                            user.StartLocation = l;
                            break;
                        }
                    }

                    if (user.StartLocation == null)
                    {
                        user.Send("Invalid selection: " + arg);
                        return false;
                    }
                }
            }
            else if (startingLocations.Count == 1)
                user.StartLocation = startingLocations[0];
            else
                user.StartLocation = new Location(this.MainArea, this.MainArea.Start);

            this.FinishCreation(user);
            return true;
        }

        [Obfuscation(Exclude = false)]
        void FinishCreation(User user)
        {
            user.Flags.Set((int)User.UserFlags.Creating, false);
            string playersDir = this.ServerPath + "\\Players";
            if (!Directory.Exists(playersDir) || Directory.GetFiles(playersDir).Length == 0)
            {
                user.Flags.Set((int)User.UserFlags.Administrator, true);
            }
            Trace.WriteLine("New user: " + user.Character.Name, "SERVER");
            this.Settings.InfoChannel.Broadcast(InfoChannel.Event.NewCharacter, "Everyone welcome newcomer " + user.Character.Name + "!");
            user.Character.Id = NewGuid();
            user.Save();
            this.Login(user);
        }

        [Obfuscation(Exclude = false)]
        bool Login(User user)
        {
            user.Menus.Clear();
            user.InputHandler = new InputHandler(this.ProcessCommand);

            if (user.Flags.Get((int)User.UserFlags.LoggedIn))
            {
                String str = new String(null, null, null, null, "{1} snaps out of {1:sex(it's, his, her)} trance.", null);
                user.Character.Area.Send(str, user.Character, null, 0);
                return true;
            }

            if (!string.IsNullOrEmpty(this.Settings.MOTD))
                user.Send(String.Format(this.Settings.MOTD) + "\r\n");

            if (user.Character.Parent != null && user.Character.Parent is Area)
            {
                user.Character.Parent = user.Character.Parent;
            }
            else
            {
                if (user.Character.ParentId != Guid.Empty)
                    user.Character.Parent = this.Find<Object>(user.Character.ParentId);

                if (user.Character.Parent == null)
                {
                    if (user.StartLocation == null || user.StartLocation.Area == null)
                        user.StartLocation = new Location(this.MainArea, this.MainArea.Start);
                    user.Character.Location = user.StartLocation;
                }
            }

            this.Settings.InfoChannel.Broadcast(InfoChannel.Event.Login, user.Character.Name + " has logged in.");
            this.CheckAbilities(user.Character);
            user.Flags.Set((int)User.UserFlags.LoggedIn, true);
            Trace.WriteLine(user.Character.Name + " has logged in.", "SERVER");
            if(this.UserLoggedIn != null) this.UserLoggedIn(user);

            user.Character.Area.Send(user.Character.Name + " has arrived.", user.Character.Location, 0, user.Character, new Function<bool, IObject>[] { Object.CanHear }, user.Character);
            //user.Send(" ");
            user.SendHelp("Type COMMANDS for a list of possible commands.");

            return true;
        }
        #endregion

        [OnDeserializing]
        [Obfuscation(Exclude = false)]
        internal void OnDeserializaing(StreamingContext context)
        {
            instance_ = this;
            if (this.DataSets == null)
                this.dataSets_ = new Dictionary<Type, Dictionary<Guid, IIdentifiable>>();
        }

        [Obfuscation(Exclude = false)]
        public class ObjectBlueprintIdToNameConverter<T> : StringConverter where T : class
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value is Guid)
                {
                    Guid id = (Guid)value;
                    return Server.Instance.GetObjectBlueprintName(id);
                }

                return string.Empty;
            }
        }
        #endregion
    }
}
