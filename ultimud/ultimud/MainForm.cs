using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Net;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Damdai.Forms;
using Damdai;

[assembly: ObfuscateAssemblyAttribute(true)]
namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    internal partial class MainForm : Form
    {
        static MainForm instance_;
        Server server_;
        Object selectedObject_ = null;
        System.Windows.Forms.Timer timer_ = new System.Windows.Forms.Timer();

        #region Properties

        public static MainForm Instance
        {
            get { return instance_; }
        }

        public string ServerPath
        {
            get { return Application.StartupPath + "\\data\\muds\\" + String.ReplaceChars(this.server_.Settings.Name, "/\\:*?\"<>|", '_'); }
        }

        #endregion

        public MainForm()
        {
            if (this.AlreadyRunning())
            {
                MessageBox.Show("UltiMUD Server is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(1);
            }

            StoredSettings.Load();

            InitializeComponent();

            instance_ = this;
            StreamWriter writer = File.CreateText(Application.StartupPath + "\\log.txt");
            writer.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(writer));
            LogTraceListener ltl = new LogTraceListener();
            ltl.LogEvent += new Function_void<string, string>(ltl_LogEvent);
            Trace.Listeners.Add(ltl);
            this.propertyGrid1.PropertyValueChanged += delegate(object s, PropertyValueChangedEventArgs e)
            {
                this.propertyGrid1.Tag = true;
            };
            this.objectTree1.EditObject += this.objectTree1_EditObjectEvent;
            this.objectTree1.ObjectSelected += delegate(object o)
            {
                if (this.propertyGrid1.SelectedObject is ISaveable && this.propertyGrid1.Tag != null)
                    ((ISaveable)this.propertyGrid1.SelectedObject).Save();
                this.propertyGrid1.SelectedObject = o;
                this.propertyGrid1.Tag = null;
                this.toolStripLabel3.Text = (o != null ? o.ToString() : null);
                this.objectTree1_ObjectSelectedEvent(o);
            };
            this.objectTree1.GoTo += delegate(Object o)
            {
                if (o is Area)
                {
                    Area area = o as Area;
                    this.areaCanvas1.SetArea(area);
                    this.areaCanvas1.GoTo(area.Start);
                }
                else
                    this.areaCanvas1.GoTo(o.Location);
            };
            this.objectTree1.MonitorUser += delegate(User user)
            {
                TabPage tp = new TabPage(user.ToString());
                PlayerMonitor pm = new PlayerMonitor(user);
                pm.Parent = tp;
                pm.Dock = DockStyle.Fill;
                pm.CloseEvent += delegate()
                {
                    this.tabControl1.TabPages.Remove(tp);
                };
                this.tabControl1.TabPages.Add(tp);
                this.tabControl1.SelectedTab = tp;
                this.splitContainer3.Panel2Collapsed = false;
            };
            Settings.NameChangedEvent += this.ServerSettings_NameChangedEvent;
            this.toolStripStatusLabel1.Text = string.Empty;
            this.toolStripStatusLabel2.Text = string.Empty;
            this.toolStripStatusLabel3.Text = string.Empty;
            this.areaCanvas1.ViewType = AreaCanvas.View.Design2;
            this.CheckForUpdates(true, true);
            this.ShowChangeLog();
            this.timer_.Interval = 3600000;
            this.timer_.Tick += this.timer__Tick;
            this.timer_.Start();
            this.toolStripTextBox2.Text = StoredSettings.Instance.Port.ToString();
            this.Open(StoredSettings.Instance.GameFile);
            if (this.server_ == null)
                this.Init(Server.Load(null, null));
        }

        void ltl_LogEvent(string t1, string t2)
        {
            if (this.InvokeRequired)
                this.Invoke(new Function_void<string, string>(this.ltl_LogEvent), t1, t2);
            else
            {
                if (this.richTextBox1.TextLength > 0)
                    this.richTextBox1.AppendText("\r\n");
                if (string.IsNullOrEmpty(t2))
                    this.richTextBox1.AppendText(DateTime.Now + ": " + t1);
                else
                    this.richTextBox1.AppendText(DateTime.Now + ": " + t2 + ": " + t1);
                this.richTextBox1.ScrollToCaret();
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSizeEx(IntPtr hProcess, IntPtr dwMinimumWorkingSetSize, IntPtr dwMaximumWorkingSetSize, uint Flags);

        void timer__Tick(object sender, EventArgs e)
        {
            try
            {
                SetProcessWorkingSetSizeEx(Process.GetCurrentProcess().Handle, new IntPtr(-1), new IntPtr(-1), 0);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "WARNING");
            }
        }
        void Open(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                if (this.server_ == null)
                {
                    string defaultMud = Path.Combine(Application.StartupPath, "data\\muds\\Unnamed\\Unnamed.mud");
                    if (File.Exists(defaultMud))
                        filename = defaultMud;
                    else
                        return;
                }
                else
                    return;
            }

            if (!File.Exists(filename))
            {
                string relPath = Path.Combine(Application.StartupPath, filename);
                if (File.Exists(relPath))
                {
                    filename = relPath;
                }
                else if (this.server_ == null)
                {
                    string fallback = Path.Combine(Application.StartupPath, "data\\muds\\Unnamed\\Unnamed.mud");
                    if (File.Exists(fallback))
                        filename = fallback;
                    else
                        return;
                }
            }

            if (File.Exists(filename))
            {
                try
                {
                    string fullPath = Path.GetFullPath(filename);
                    string serverPath = Path.GetDirectoryName(fullPath);
                    this.Init(Server.Load(fullPath, serverPath));
                }
                catch (Exception e)
                {
                    this.server_ = null;
                    Trace.WriteLine(e.ToString());
                    MessageBox.Show("That is not a valid MUD file.\r\nIf you believe this to be an error, email the following files to damdai@hotmail.com.\r\n\r\n\t" + filename,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //MessageBox.Show(e.ToString());
                    return;
                }
            }
            else
            {
                MessageBox.Show("File not found: " + filename, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void Init(Server server)
        {
            this.server_ = server;
            this.server_.UserConnected += server__UserConnectedEvent;
            this.server_.UserDisconnected += server__UserDisconnectedEvent;
            this.server_.UserReconnected += server__UserReconnectedEvent;
            this.server_.UserUnloaded += server__UserUnloadedEvent;
            this.server_.UserLoggedIn += server__UserLoginEvent;
            this.server_.Started += server__StartedEvent;
            this.server_.Stopped += server__StoppedEvent;
            this.server_.Clock.Elapsed += Clock_Elapsed;

            this.areaCanvas1.SetArea(this.server_.MainArea);
            this.objectTree1.Generate(server);

            this.Text = "UltiMUD - " + this.server_.Settings.Name;
            StoredSettings.Instance.GameFile = (this.server_ != null ? this.ServerPath + "\\" + String.ReplaceChars(this.server_.Settings.Name, "/\\:*?\"<>|", '_') + ".mud" : null);
            StoredSettings.Save();

            this.propertyGrid1.SelectedObject = server;
            //this.InitImportExportMenu();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.server_ != null)
            {
                if (this.server_.IsRunning)
                {
                    if (MessageBox.Show("The server is currently running. If you close this window, the server will be shutdown. Do you want to continue?",
                        "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                    this.server_.Stop();
                }
                this.server_.Save();
                if (this.propertyGrid1.SelectedObject is ISaveable && this.propertyGrid1.Tag != null)
                    ((ISaveable)this.propertyGrid1.SelectedObject).Save();
            }

            int port;
            if (int.TryParse(this.toolStripTextBox2.Text, out port))
                StoredSettings.Instance.Port = port;
            StoredSettings.Instance.GameFile = (this.server_ != null ? this.ServerPath + "\\" + String.ReplaceChars(this.server_.Settings.Name, "/\\:*?\"<>|", '_') + ".mud" : null);
            StoredSettings.Save();
            if (this.timer_ != null)
            {
                this.timer_.Stop();
                this.timer_.Dispose();
            }
            if (this.notifyIcon1 != null)
            {
                this.notifyIcon1.Visible = false;
                this.notifyIcon1.Dispose();
            }
            base.OnClosing(e);
        }
        bool ServerSettings_NameChangedEvent(string oldName, string newName)
        {
            string name = newName;
            newName = String.ReplaceChars(newName, "/\\:*?\"<>|", '_');
            oldName = String.ReplaceChars(oldName, "/\\:*?\"<>|", '_');
            string newPath = Application.StartupPath + "\\data\\muds\\" + newName + "\\";
            string oldPath = Application.StartupPath + "\\data\\muds\\" + oldName + "\\";
            if (!string.IsNullOrEmpty(oldName) && !string.IsNullOrEmpty(newName) && oldName != newName)
            {
                if (Directory.Exists(newPath))
                {
                    if (MessageBox.Show(
@"A MUD with that name already exists.
Do you want to overwrite it?
WARNING: All player data for that MUD will be lost.",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        return true;
                    }
                    else
                        Directory.Delete(newPath, true);
                }

                if (Directory.Exists(oldPath))
                    Directory.Move(oldPath, newPath);

                if (File.Exists(oldPath + "\\" + oldName + ".mud"))
                    File.Move(oldPath + "\\" + oldName + ".mud", newPath + "\\" + newName + ".mud");

                this.Text = "UltiMUD - " + name;
                StoredSettings.Instance.GameFile = (this.server_ != null ? newPath + "\\" + newName + ".mud" : null);
                StoredSettings.Save();
            }

            return false;
        }
        void objectTree1_EditObjectEvent(Object o)
        {
            if (o is Area)
            {
                AreaEditor editor = new AreaEditor(o as Area);
                editor.ShowDialog(this);
                this.areaCanvas1.Invalidate();
                Server.Instance.Save();
            }
            else if(o is Object)
            {
                Object obj = o as Object;
                if (obj.Area == null)
                {
                    MessageBox.Show("This object no longer exists.");
                    this.objectTree1.Generate(this.server_);
                    return;
                }
                AreaEditor editor = new AreaEditor(obj.Area, obj);
                editor.ShowDialog(this);
                this.areaCanvas1.Invalidate();
                Server.Instance.Save();
            }
        }
        private void objectTree1_ObjectSelectedEvent(object o)
        {
            this.selectedObject_ = null;
            if (o is Area)
            {
                this.areaCanvas1.SetArea(o as Area);
            }
            else if (o is User)
            {
                User user = o as User;
                if (user.Flags.Get((int)User.UserFlags.LoggedIn) && user.Character != null)
                {
                    this.selectedObject_ = user.Character;
                }
            }
            else if (o is Object)
            {
                this.selectedObject_ = o as Object;
            }

            if (this.selectedObject_ != null)
            {
                Area area = this.selectedObject_.Area;
                if (area != null)
                {
                    this.areaCanvas1.SetArea(area);
                    this.areaCanvas1.GoTo(this.selectedObject_.Location);
                }
            }
        }
        void Character_MovedEvent(Object o, Area.Direction dir)
        {
            if (InvokeRequired)
                this.Invoke(new Object.MovedHandler(this.Character_MovedEvent), new object[] { o, dir });
            else
            {
                if (this.areaCanvas1.Contains(o))
                {
                    if (o == this.selectedObject_)
                        this.areaCanvas1.GoTo(o.Location);
                    else
                        this.areaCanvas1.Invalidate();
                }
            }
        }
        void Clock_Elapsed(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (InvokeRequired)
                this.Invoke(new System.Timers.ElapsedEventHandler(this.Clock_Elapsed), new object[] { sender, args });
            else
            {
                string str = (DateTime.Now - this.server_.StartTime).ToString();
                int pos = str.LastIndexOf('.');
                if (pos != -1)
                    str = str.Substring(0, pos);
                this.toolStripStatusLabel3.Text = str;
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (this.server_ == null)
            {
                MessageBox.Show(this, "There is no server loaded.\r\nTo load a server, click 'open' or 'new' from the file menu.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((Command.Commands == null || Command.Commands.Count == 0) && MessageBox.Show(this, 
@"The MUD you are starting has no commands.
Without commands, players won't be able to do much.

To add commands, you can:
    1. Create them in the command editor (Edit > Commands).
    2. Copy them from another MUD.
    3. Download them from www.ultimud.com.

Would you still like to run this MUD?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;

            Async.Function_void(delegate() { this.server_.Run(this.toolStripTextBox1.Text, int.Parse(this.toolStripTextBox2.Text)); }, null).Invoke();
        }
        private void toolStripTextBox2_TextChanged(object sender, EventArgs e)
        {
            int port;
            if(!int.TryParse(this.toolStripTextBox2.Text, out port))
            {
                string str = string.Empty;
                foreach (char c in this.toolStripTextBox2.Text)
                {
                    if (c >= '0' && c <= '9')
                    {
                        str += c;
                    }
                }

                this.toolStripTextBox2.Text = str;
                this.toolStripTextBox2.AppendText(string.Empty);
            }
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (this.server_ != null && this.server_.IsRunning)
            {
                this.server_.Pause();
                this.toolStripButton2.Checked = this.server_.IsPaused;
            }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.server_.Stop();
        }
        void CalculateStats(Object parent, ref SortedDictionary<string, int> stats)
        {
            foreach (Object o in parent.GetChildren<Object>())
            {
                Type type = o.GetType();
                if (!stats.ContainsKey(type.Name))
                {
                    stats.Add(type.Name, 0);
                }
                stats[type.Name]++;
                this.CalculateStats(o, ref stats);
            }
        }
        void CheckForUpdates(bool silent, bool prompt)
        {
            //try
            //{
            //    this.toolStripStatusLabel1.Text = "Checking for updates...";
            //    using (com.damdai.www.WebService service = new Ultimud.com.damdai.www.WebService())
            //    {
            //        string url = null;
            //        if (service.UpdateAvailable(Assembly.GetExecutingAssembly().GetName().Version.ToString(), out url))
            //        {
            //            if (!prompt || MessageBox.Show("A new version is available.\r\nWould you like to upgrade to the latest version? (Recommended)",
            //                "Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            //            {
            //                this.toolStripStatusLabel1.Text = "Downloading update...";
            //                this.toolStripProgressBar1.Value = 0;
            //                this.toolStripProgressBar1.Visible = true;
            //                WebClient client = new WebClient();
            //                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            //                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            //                client.DownloadFileAsync(new Uri(url), url.Substring(url.LastIndexOf('/') + 1));
            //                return;
            //            }
            //        }
            //        else
            //        {
            //            if (!silent)
            //                MessageBox.Show("There are no updates available.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.ToString());
            //    if (!silent)
            //    {
            //        MessageBox.Show("An error occurred while checking for updates.\r\n" + e.Message, "Error",
            //            MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}

            //this.toolStripStatusLabel1.Text = null;
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.toolStripProgressBar1.Maximum = (int)e.TotalBytesToReceive;
            this.toolStripProgressBar1.Value = (int)e.BytesReceived;
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Download complete, updating...";
            using (Process p = new Process())
            {
                p.StartInfo.FileName = Application.StartupPath + "\\Updater.exe";
                p.Start();
                this.Close();
            }
        }
        void ShowChangeLog()
        {
            try
            {
                if (File.Exists(Application.StartupPath + "\\changes.README"))
                {
                    if (File.Exists(Application.StartupPath + "\\changes.txt"))
                    {
                        File.Delete(Application.StartupPath + "\\changes.txt");
                    }
                    File.Move(Application.StartupPath + "\\changes.README", Application.StartupPath + "\\changes.txt");
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = Application.StartupPath + "\\changes.txt";
                        p.Start();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred while attempting to show the change log.\r\n" + e.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        bool AlreadyRunning()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(processName);
            return (processes.Length > 1);
        }

        #region Server Events
        void server__StartedEvent()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Function_void(this.server__StartedEvent));
            }
            else
            {
                this.toolStripButton1.Enabled = false;
                this.toolStripButton2.Enabled = true;
                this.toolStripButton2.Checked = false;
                this.toolStripButton3.Enabled = true;
                this.toolStripTextBox1.Enabled = false;
                this.toolStripTextBox2.Enabled = false;
            }
        }
        void server__UserConnectedEvent(User user)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Function_void<User>(this.server__UserConnectedEvent), user);
            }
            else
            {
                this.objectTree1.Add(user);
                this.toolStripStatusLabel2.Text = this.server_.Users.Count + " user(s) online";
                this.areaCanvas1.Invalidate();
            }
        }
        void server__UserDisconnectedEvent(User user)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Function_void<User>(this.server__UserDisconnectedEvent), user);
            }
            else
            {
                if (user.Character != null)
                    user.Character.MovedEvent -= this.Character_MovedEvent;
                this.objectTree1.Update(user);
                this.areaCanvas1.Invalidate();
            }
        }
        void server__UserReconnectedEvent(User user)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Function_void<User>(this.server__UserReconnectedEvent), user);
            }
            else
            {
                this.objectTree1.Update(user);
                this.areaCanvas1.Invalidate();
            }
        }
        void server__UserUnloadedEvent(User user)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Function_void<User>(this.server__UserUnloadedEvent), user);
            }
            else
            {
                if (user.Character != null)
                    user.Character.MovedEvent -= this.Character_MovedEvent;
                this.objectTree1.Remove(user);
                this.toolStripStatusLabel2.Text = this.server_.Users.Count + " user(s) online";
                this.areaCanvas1.Invalidate();
            }
        }
        void server__StoppedEvent()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Function_void(this.server__StoppedEvent));
            }
            else
            {
                this.toolStripButton1.Enabled = true;
                this.toolStripButton2.Enabled = false;
                this.toolStripButton2.Checked = false;
                this.toolStripButton3.Enabled = false;
                this.toolStripTextBox1.Enabled = true;
                this.toolStripTextBox2.Enabled = true;
            }
        }
        void server__UserLoginEvent(User user)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Function_void<User>(this.server__UserLoginEvent), user);
            }
            else
            {
                this.objectTree1.Update(user);
                this.areaCanvas1.Invalidate();
                if (user.Character != null)
                {
                    user.Character.MovedEvent -= this.Character_MovedEvent;
                    user.Character.MovedEvent += new Object.MovedHandler(Character_MovedEvent);
                }
            }
        }
        #endregion

        #region File Menu

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.server_ != null && this.server_.IsRunning)
            {
                if (MessageBox.Show("A server is currently running.\r\nThe current server must be stopped if you want to create a new one.\r\nDo you want to continue?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    this.server_.Stop();
                else
                    return;

                this.server_.Save();
            }

            this.Init(Server.Load(null, null));
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.server_ != null && this.server_.IsRunning)
            {
                if (MessageBox.Show("A server is currently running.\r\nThe current server must be stopped if you want to load a different one.\r\nDo you want to continue?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    this.server_.Stop();
                else
                    return;

                this.server_.Save();
            }

            if (this.openFileDialog1.ShowDialog(this) == DialogResult.OK)
                this.Open(this.openFileDialog1.FileName);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region View Menu
        private void serverStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortedDictionary<string, int> stats = new SortedDictionary<string, int>();
            stats.Add(Server.Instance.MainArea.GetType().Name, 1);
            this.CalculateStats(Server.Instance.MainArea, ref stats);
            string str = string.Empty;
            foreach (KeyValuePair<string, int> pair in stats)
            {
                str += pair.Key + "s: " + pair.Value + "\r\n";
            }
            MessageBox.Show(str, "Server Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Help Menu
        private void indexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, this.helpProvider1.HelpNamespace);
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.CheckForUpdates(false, false);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Developed by: Damdai\r\nWebsite: http://www.ultimud.com \r\nVersion: " + Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.logToolStripMenuItem.Checked = !this.logToolStripMenuItem.Checked;
            this.splitContainer3.Panel2Collapsed = !this.logToolStripMenuItem.Checked;
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            this.splitContainer1.Panel2Collapsed = true;
        }
        private void objectPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.splitContainer1.Panel2Collapsed = false;
        }
    }

    public class LogTraceListener : TraceListener
    {
        public event Function_void<string, string> LogEvent;

        public override void Write(string message)
        {
            if (this.LogEvent != null)
                this.LogEvent(message, null);
        }
        public override void WriteLine(string message)
        {
            if (this.LogEvent != null)
                this.LogEvent(message, null);
        }
        public override void WriteLine(string message, string category)
        {
            if (this.LogEvent != null)
                this.LogEvent(message, category);
        }
    }
}
