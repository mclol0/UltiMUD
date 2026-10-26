using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    internal class TestClientForm : Form
    {
        static TestClientForm instance_;

        TcpClient client_;
        NetworkStream stream_;
        Thread readThread_;
        volatile bool connected_;

        RichTextBox terminalBox_;
        TextBox inputBox_;
        Label statusLabel_;
        Button connectBtn_;
        CheckBox localEchoCheck_;

        List<string> history_ = new List<string>();
        int historyIndex_ = -1;
        Color currentFg_ = Color.FromArgb(212, 212, 212);
        Color currentBg_ = Color.FromArgb(12, 14, 20);
        bool bold_ = false;

        public static void ShowPlaytester(IWin32Window owner, int port)
        {
            if (instance_ == null || instance_.IsDisposed)
                instance_ = new TestClientForm(port);
            if (!instance_.Visible)
                instance_.Show(owner);
            else
                instance_.Activate();
            instance_.EnsureConnected(port);
        }

        public TestClientForm(int port)
        {
            this.Text = "UltiMUD Built-In Playtester & Creator Test Client";
            this.Size = new Size(920, 660);
            this.MinimumSize = new Size(720, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;

            // Top Connection & Admin Cheat Bar
            Panel topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 68;
            topPanel.BackColor = Color.FromArgb(24, 28, 38);
            topPanel.Padding = new Padding(8, 6, 8, 6);

            // Row 1: Connection Status + Connect/Disconnect + Local Admin Cheats
            FlowLayoutPanel row1 = new FlowLayoutPanel();
            row1.Dock = DockStyle.Top;
            row1.Height = 30;
            row1.WrapContents = false;
            row1.BackColor = Color.Transparent;

            this.statusLabel_ = new Label();
            this.statusLabel_.Text = "Disconnected";
            this.statusLabel_.ForeColor = Color.FromArgb(248, 113, 113);
            this.statusLabel_.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.statusLabel_.AutoSize = true;
            this.statusLabel_.Margin = new Padding(2, 5, 10, 0);

            this.connectBtn_ = CreateToolbarButton("Connect (127.0.0.1)", Color.FromArgb(37, 99, 235), Color.White);
            this.connectBtn_.Click += delegate
            {
                if (this.connected_)
                    this.Disconnect();
                else
                    this.EnsureConnected(MainForm.Instance != null ? MainForm.Instance.ConfiguredPort : port);
            };

            Button clearBtn = CreateToolbarButton("Clear Screen", Color.FromArgb(51, 65, 85), Color.White);
            clearBtn.Click += delegate { this.terminalBox_.Clear(); };

            this.localEchoCheck_ = new CheckBox();
            this.localEchoCheck_.Text = "Echo Input";
            this.localEchoCheck_.Checked = true;
            this.localEchoCheck_.ForeColor = Color.FromArgb(203, 213, 225);
            this.localEchoCheck_.AutoSize = true;
            this.localEchoCheck_.Margin = new Padding(6, 5, 12, 0);

            ToolTip tip = new ToolTip();

            Button grantAdminBtn = CreateToolbarButton("★ Grant Admin", Color.FromArgb(147, 51, 234), Color.White);
            tip.SetToolTip(grantAdminBtn, "Instantly grant Administrator flag to your logged-in test character");
            grantAdminBtn.Click += delegate { this.ApplyCreatorAction("admin"); };

            Button restoreBtn = CreateToolbarButton("♥ Full Restore", Color.FromArgb(22, 163, 74), Color.White);
            tip.SetToolTip(restoreBtn, "Restore all stats (HP/MP/Stamina) of your test character to Max");
            restoreBtn.Click += delegate { this.ApplyCreatorAction("restore"); };

            Button goldBtn = CreateToolbarButton("$ +1,000 Gold", Color.FromArgb(202, 138, 4), Color.White);
            tip.SetToolTip(goldBtn, "Give +1,000 currency to your test character for testing shops & trainers");
            goldBtn.Click += delegate { this.ApplyCreatorAction("gold"); };

            Button tpBtn = CreateToolbarButton("⌖ Teleport to Map Selection", Color.FromArgb(14, 116, 144), Color.White);
            tip.SetToolTip(tpBtn, "Teleport your test character to the currently selected point on the MainForm Area Canvas");
            tpBtn.Click += delegate { this.ApplyCreatorAction("teleport"); };

            row1.Controls.Add(this.statusLabel_);
            row1.Controls.Add(this.connectBtn_);
            row1.Controls.Add(clearBtn);
            row1.Controls.Add(this.localEchoCheck_);
            row1.Controls.Add(grantAdminBtn);
            row1.Controls.Add(restoreBtn);
            row1.Controls.Add(goldBtn);
            row1.Controls.Add(tpBtn);

            // Row 2: Quick MUD Command Macros
            FlowLayoutPanel row2 = new FlowLayoutPanel();
            row2.Dock = DockStyle.Bottom;
            row2.Height = 28;
            row2.WrapContents = false;
            row2.BackColor = Color.Transparent;

            Label macroLbl = new Label();
            macroLbl.Text = "Quick Commands:";
            macroLbl.ForeColor = Color.FromArgb(148, 163, 184);
            macroLbl.AutoSize = true;
            macroLbl.Margin = new Padding(2, 5, 6, 0);
            row2.Controls.Add(macroLbl);

            string[] macros = new string[] { "look", "stats", "inventory", "equipment", "abilities", "who", "areas", "shops", "quests", "help", "wizhelp" };
            foreach (string m in macros)
            {
                string captured = m;
                Button b = CreateToolbarButton(captured, Color.FromArgb(30, 41, 59), Color.FromArgb(226, 232, 240));
                b.Height = 23;
                b.Margin = new Padding(2, 1, 2, 1);
                b.Click += delegate { this.SendCommand(captured); };
                row2.Controls.Add(b);
            }

            topPanel.Controls.Add(row2);
            topPanel.Controls.Add(row1);

            // Bottom Command Input Bar + Directional Compass Pad
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 38;
            bottomPanel.BackColor = Color.FromArgb(24, 28, 38);
            bottomPanel.Padding = new Padding(8, 6, 8, 6);

            Label promptLbl = new Label();
            promptLbl.Text = "Command >";
            promptLbl.ForeColor = Color.FromArgb(56, 189, 248);
            promptLbl.Font = new Font("Consolas", 10F, FontStyle.Bold);
            promptLbl.AutoSize = false;
            promptLbl.Width = 84;
            promptLbl.Dock = DockStyle.Left;
            promptLbl.TextAlign = ContentAlignment.MiddleLeft;

            FlowLayoutPanel dirPanel = new FlowLayoutPanel();
            dirPanel.Dock = DockStyle.Right;
            dirPanel.Width = 225;
            dirPanel.WrapContents = false;
            string[] dirs = new string[] { "n", "s", "w", "e", "nw", "ne", "sw", "se" };
            foreach (string d in dirs)
            {
                string capDir = d;
                Button db = CreateToolbarButton(capDir.ToUpperInvariant(), Color.FromArgb(51, 65, 85), Color.White);
                db.Width = 26;
                db.Height = 24;
                db.Margin = new Padding(1, 0, 1, 0);
                db.Padding = Padding.Empty;
                db.Click += delegate { this.SendCommand(capDir); };
                dirPanel.Controls.Add(db);
            }

            Button sendBtn = CreateToolbarButton("Send", Color.FromArgb(37, 99, 235), Color.White);
            sendBtn.Dock = DockStyle.Right;
            sendBtn.Width = 64;
            sendBtn.Click += delegate { this.SubmitInput(); };

            this.inputBox_ = new TextBox();
            this.inputBox_.Dock = DockStyle.Fill;
            this.inputBox_.BackColor = Color.FromArgb(15, 23, 42);
            this.inputBox_.ForeColor = Color.White;
            this.inputBox_.Font = new Font("Consolas", 10.5F, FontStyle.Regular);
            this.inputBox_.BorderStyle = BorderStyle.FixedSingle;
            this.inputBox_.KeyDown += new KeyEventHandler(this.inputBox__KeyDown);

            bottomPanel.Controls.Add(this.inputBox_);
            bottomPanel.Controls.Add(promptLbl);
            bottomPanel.Controls.Add(sendBtn);
            bottomPanel.Controls.Add(dirPanel);

            // Terminal RichTextBox
            this.terminalBox_ = new RichTextBox();
            this.terminalBox_.Dock = DockStyle.Fill;
            this.terminalBox_.ReadOnly = true;
            this.terminalBox_.BackColor = Color.FromArgb(12, 14, 20);
            this.terminalBox_.ForeColor = Color.FromArgb(212, 212, 212);
            this.terminalBox_.Font = new Font("Consolas", 10.5F, FontStyle.Regular);
            this.terminalBox_.BorderStyle = BorderStyle.None;
            this.terminalBox_.DetectUrls = false;
            this.terminalBox_.ScrollBars = RichTextBoxScrollBars.ForcedVertical;

            this.Controls.Add(this.terminalBox_);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);

            this.FormClosing += delegate
            {
                this.Disconnect();
            };
        }

        static Button CreateToolbarButton(string text, Color backColor, Color foreColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.AutoSize = true;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = foreColor;
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(3, 1, 3, 1);
            btn.Padding = new Padding(6, 1, 6, 1);
            return btn;
        }

        public void EnsureConnected(int port)
        {
            if (this.connected_)
                return;

            if (Server.Instance != null && !Server.Instance.IsRunning && MainForm.Instance != null)
            {
                DialogResult dr = MessageBox.Show(this,
                    "Your MUD server is not currently running.\r\nWould you like to start the server on port " + port + " and connect now?",
                    "Start Server & Playtest", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    MainForm.Instance.StartServerForPlaytest();
                    Thread.Sleep(250);
                }
                else
                {
                    return;
                }
            }

            try
            {
                this.AppendSystemMessage("Connecting to 127.0.0.1:" + port + "...\r\n", Color.FromArgb(56, 189, 248));
                this.client_ = new TcpClient();
                this.client_.NoDelay = true;
                this.client_.Connect("127.0.0.1", port);
                this.stream_ = this.client_.GetStream();
                this.connected_ = true;

                this.statusLabel_.Text = "Connected (127.0.0.1:" + port + ")";
                this.statusLabel_.ForeColor = Color.FromArgb(74, 222, 128);
                this.connectBtn_.Text = "Disconnect";
                this.connectBtn_.BackColor = Color.FromArgb(185, 28, 28);

                this.readThread_ = new Thread(new ThreadStart(this.ReadLoop));
                this.readThread_.IsBackground = true;
                this.readThread_.Name = "Playtester Read Thread";
                this.readThread_.Start();
                this.inputBox_.Focus();
            }
            catch (Exception ex)
            {
                this.AppendSystemMessage("Connection failed: " + ex.Message + "\r\n", Color.FromArgb(248, 113, 113));
            }
        }

        public void Disconnect()
        {
            this.connected_ = false;
            try { if (this.stream_ != null) this.stream_.Close(); } catch { }
            try { if (this.client_ != null) this.client_.Close(); } catch { }
            this.stream_ = null;
            this.client_ = null;

            if (!this.IsDisposed && this.statusLabel_ != null)
            {
                try
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        this.statusLabel_.Text = "Disconnected";
                        this.statusLabel_.ForeColor = Color.FromArgb(248, 113, 113);
                        this.connectBtn_.Text = "Connect (127.0.0.1)";
                        this.connectBtn_.BackColor = Color.FromArgb(37, 99, 235);
                    });
                }
                catch { }
            }
        }

        void ReadLoop()
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (this.connected_ && this.stream_ != null)
                {
                    int read = this.stream_.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;

                    // Strip raw Telnet IAC negotiation bytes (255, cmd, opt) so only text + ANSI sequences remain
                    List<byte> clean = new List<byte>(read);
                    for (int i = 0; i < read; i++)
                    {
                        byte b = buffer[i];
                        if (b == 255) // IAC
                        {
                            if (i + 1 < read)
                            {
                                byte cmd = buffer[i + 1];
                                if (cmd >= 251 && cmd <= 254)
                                    i += 2;
                                else
                                    i += 1;
                            }
                            continue;
                        }
                        clean.Add(b);
                    }

                    if (clean.Count > 0)
                    {
                        string text = Encoding.ASCII.GetString(clean.ToArray());
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            this.AppendAnsiText(text);
                        });
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (this.connected_)
                {
                    this.Disconnect();
                    try
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            this.AppendSystemMessage("\r\n[Connection closed by server]\r\n", Color.FromArgb(248, 113, 113));
                        });
                    }
                    catch { }
                }
            }
        }

        static readonly Regex AnsiRegex = new Regex(@"\x1B\[([0-9;]*)m", RegexOptions.Compiled);

        void AppendAnsiText(string text)
        {
            if (this.IsDisposed || this.terminalBox_.IsDisposed || string.IsNullOrEmpty(text))
                return;

            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            int lastIdx = 0;
            MatchCollection matches = AnsiRegex.Matches(text);
            foreach (Match m in matches)
            {
                if (m.Index > lastIdx)
                {
                    string chunk = text.Substring(lastIdx, m.Index - lastIdx);
                    this.WriteColoredChunk(chunk, this.currentFg_, this.currentBg_);
                }
                this.ApplyAnsiCodes(m.Groups[1].Value);
                lastIdx = m.Index + m.Length;
            }

            if (lastIdx < text.Length)
            {
                this.WriteColoredChunk(text.Substring(lastIdx), this.currentFg_, this.currentBg_);
            }

            this.terminalBox_.SelectionStart = this.terminalBox_.TextLength;
            this.terminalBox_.ScrollToCaret();
        }

        void WriteColoredChunk(string chunk, Color fg, Color bg)
        {
            if (string.IsNullOrEmpty(chunk))
                return;
            int start = this.terminalBox_.TextLength;
            this.terminalBox_.AppendText(chunk);
            this.terminalBox_.Select(start, this.terminalBox_.TextLength - start);
            this.terminalBox_.SelectionColor = fg;
            this.terminalBox_.SelectionBackColor = bg;
            this.terminalBox_.SelectionLength = 0;
        }

        void ApplyAnsiCodes(string codeStr)
        {
            if (string.IsNullOrEmpty(codeStr))
            {
                this.ResetAnsiColors();
                return;
            }

            string[] parts = codeStr.Split(';');
            foreach (string p in parts)
            {
                int code;
                if (!int.TryParse(p, out code))
                    continue;

                if (code == 0)
                {
                    this.ResetAnsiColors();
                }
                else if (code == 1)
                {
                    this.bold_ = true;
                    this.currentFg_ = BrightenColor(this.currentFg_);
                }
                else if (code == 22)
                {
                    this.bold_ = false;
                }
                else if (code >= 30 && code <= 37)
                {
                    this.currentFg_ = GetAnsiColor(code - 30, this.bold_);
                }
                else if (code >= 40 && code <= 47)
                {
                    this.currentBg_ = GetAnsiColor(code - 40, false);
                }
            }
        }

        void ResetAnsiColors()
        {
            this.bold_ = false;
            this.currentFg_ = Color.FromArgb(212, 212, 212);
            this.currentBg_ = Color.FromArgb(12, 14, 20);
        }

        static Color GetAnsiColor(int index, bool bright)
        {
            switch (index)
            {
                case 0: return bright ? Color.FromArgb(115, 115, 115) : Color.FromArgb(12, 14, 20);      // Black / DarkGray
                case 1: return bright ? Color.FromArgb(248, 113, 113) : Color.FromArgb(185, 28, 28);     // Red
                case 2: return bright ? Color.FromArgb(74, 222, 128) : Color.FromArgb(22, 163, 74);      // Green
                case 3: return bright ? Color.FromArgb(250, 204, 21) : Color.FromArgb(202, 138, 4);      // Yellow
                case 4: return bright ? Color.FromArgb(96, 165, 250) : Color.FromArgb(37, 99, 235);      // Blue
                case 5: return bright ? Color.FromArgb(232, 121, 249) : Color.FromArgb(147, 51, 234);    // Magenta
                case 6: return bright ? Color.FromArgb(34, 211, 238) : Color.FromArgb(14, 116, 144);     // Cyan
                case 7: return bright ? Color.FromArgb(255, 255, 255) : Color.FromArgb(212, 212, 212);   // White
                default: return Color.FromArgb(212, 212, 212);
            }
        }

        static Color BrightenColor(Color c)
        {
            return Color.FromArgb(
                Math.Min(255, c.R + 55),
                Math.Min(255, c.G + 55),
                Math.Min(255, c.B + 55));
        }

        void AppendSystemMessage(string msg, Color color)
        {
            this.WriteColoredChunk(msg, color, Color.FromArgb(12, 14, 20));
            this.terminalBox_.SelectionStart = this.terminalBox_.TextLength;
            this.terminalBox_.ScrollToCaret();
        }

        void inputBox__KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.SubmitInput();
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                if (this.history_.Count > 0 && this.historyIndex_ + 1 < this.history_.Count)
                {
                    this.historyIndex_++;
                    this.inputBox_.Text = this.history_[this.historyIndex_];
                    this.inputBox_.SelectionStart = this.inputBox_.Text.Length;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                if (this.historyIndex_ > 0)
                {
                    this.historyIndex_--;
                    this.inputBox_.Text = this.history_[this.historyIndex_];
                    this.inputBox_.SelectionStart = this.inputBox_.Text.Length;
                }
                else
                {
                    this.historyIndex_ = -1;
                    this.inputBox_.Clear();
                }
            }
        }

        void SubmitInput()
        {
            string cmd = this.inputBox_.Text;
            this.inputBox_.Clear();
            if (!string.IsNullOrEmpty(cmd))
            {
                this.history_.Insert(0, cmd);
                if (this.history_.Count > 100)
                    this.history_.RemoveAt(this.history_.Count - 1);
            }
            this.historyIndex_ = -1;
            this.SendCommand(cmd);
            this.inputBox_.Focus();
        }

        public void SendCommand(string cmd)
        {
            if (!this.connected_ || this.stream_ == null)
            {
                this.AppendSystemMessage("Not connected to server. Click 'Connect' first.\r\n", Color.FromArgb(248, 113, 113));
                return;
            }
            try
            {
                if (this.localEchoCheck_.Checked)
                    this.AppendSystemMessage("> " + cmd + "\r\n", Color.FromArgb(148, 163, 184));

                byte[] bytes = Encoding.ASCII.GetBytes((cmd ?? string.Empty) + "\r\n");
                this.stream_.Write(bytes, 0, bytes.Length);
                this.stream_.Flush();
            }
            catch (Exception ex)
            {
                this.AppendSystemMessage("Send error: " + ex.Message + "\r\n", Color.FromArgb(248, 113, 113));
            }
        }

        User FindLocalTestUser()
        {
            if (Server.Instance == null || Server.Instance.Users == null)
                return null;

            List<User> users = Damdai.Collections.Util.CopyList<User>(Server.Instance.Users);
            // Prefer logged-in user from 127.0.0.1
            for (int i = users.Count - 1; i >= 0; i--)
            {
                User u = users[i];
                if (u != null && u.Connected && u.Character != null && (u.IpAddress == "127.0.0.1" || string.IsNullOrEmpty(u.IpAddress)))
                    return u;
            }
            for (int i = users.Count - 1; i >= 0; i--)
            {
                User u = users[i];
                if (u != null && u.Connected && u.Character != null)
                    return u;
            }
            return null;
        }

        void ApplyCreatorAction(string action)
        {
            User u = this.FindLocalTestUser();
            if (u == null || u.Character == null)
            {
                this.AppendSystemMessage("[Creator Tool] Log in with a character in the terminal first!\r\n", Color.FromArgb(250, 204, 21));
                return;
            }

            switch (action)
            {
                case "admin":
                    u.Flags.Set((int)User.UserFlags.Administrator, true);
                    Server.Instance.SaveUsers();
                    this.AppendSystemMessage("[Creator Tool] Granted Administrator privileges to '" + u.Character.Name + "'! Type 'wizhelp' to view admin commands.\r\n", Color.FromArgb(192, 132, 252));
                    break;

                case "restore":
                    u.Character.Restore(true);
                    u.Character.Position = Object.Positions.Stopped;
                    this.AppendSystemMessage("[Creator Tool] Fully restored all stats on '" + u.Character.Name + "'!\r\n", Color.FromArgb(74, 222, 128));
                    break;

                case "gold":
                    u.Character.Value += 1000;
                    this.AppendSystemMessage("[Creator Tool] Added +1,000 currency to '" + u.Character.Name + "' (Total: " + u.Character.Value + ").\r\n", Color.FromArgb(250, 204, 21));
                    break;

                case "teleport":
                    if (MainForm.Instance != null)
                    {
                        Location targetLoc = MainForm.Instance.GetSelectedCanvasLocation();
                        if (targetLoc != null && targetLoc.Area != null)
                        {
                            u.Character.Location = targetLoc;
                            this.AppendSystemMessage("[Creator Tool] Teleported '" + u.Character.Name + "' to " + targetLoc.Area.Name + " (" + targetLoc.Point.X + ", " + targetLoc.Point.Y + ").\r\n", Color.FromArgb(56, 189, 248));
                            this.SendCommand("look");
                        }
                    }
                    break;
            }
        }
    }
}
