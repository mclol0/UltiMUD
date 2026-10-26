using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LuaInterface;
using Damdai;

namespace Ultimud
{
    /// <summary>
    /// Modern Dark Game Maker Studio & MUD IDE Workspace for UltiMUD.
    /// Replaces the dated 2006 TreeView+PropertyGrid-only workflow with an intuitive,
    /// visual multi-studio interface while preserving a 1-click toggle to Classic View.
    /// </summary>
    public class StudioWorkspaceControl : UserControl
    {
        public enum StudioTab
        {
            Dashboard = 0,
            MapPainter = 1,
            ItemsForge = 2,
            NpcBestiary = 3,
            AbilitiesStudio = 4,
            RacesClassesStats = 5,
            LuaCodeIde = 6,
            ServerConsole = 7
        }

        private MainForm mainForm_;
        private Server server_;
        private StudioTab activeTab_ = StudioTab.Dashboard;

        // Top Header Bar Controls
        private Panel topBar_;
        private Label mudTitleLabel_;
        private Panel serverStatusPill_;
        private Label serverStatusDot_;
        private TextBox portInputBox_;
        private Button startStopServerBtn_;
        private Button toggleClassicBtn_;

        // Left Navigation Rail
        private Panel navRail_;
        private List<Button> navButtons_ = new List<Button>();

        // Main Content Host
        private Panel contentHost_;
        private Dictionary<StudioTab, Control> tabPages_ = new Dictionary<StudioTab, Control>();

        // Dashboard Controls
        private TextBox dashMudNameBox_;
        private Label[] dashMetricValues_ = new Label[6];
        private PropertyGrid dashSettingsGrid_;

        // Inline 2D Map Painter Controls
        private AreaCanvas mapCanvas_;
        private ListBox mapLayersList_;
        private PropertyGrid mapInspectorGrid_;
        private TextBox mapObjNameBox_;
        private TextBox mapFillCharBox_;
        private ComboBox mapFillColorCombo_;
        private CheckBox mapFlagNoWalk_;
        private CheckBox mapFlagSafe_;
        private CheckBox mapFlagIndoors_;
        private CheckBox mapFlagSwim_;
        private Panel mapLayerQuickPanel_;
        private Label mapActiveToolLabel_;
        private bool suppressMapQuickEvents_ = false;

        // Blueprint Studio State (Items, NPCs, Abilities, Races/Jobs/Stats)
        private Type currentRpgSubType_ = typeof(Race);

        // Lua Code & Help IDE Controls
        private int ideCategory_ = 0; // 0 = Commands, 1 = System Scripts, 2 = Help Files
        private TextBox ideSearchBox_;
        private ListBox ideItemList_;
        private TextBox ideNameBox_;
        private TextBox ideAliasesBox_;
        private CheckBox ideSystemCheck_;
        private CheckBox ideHiddenCheck_;
        private RichTextBox ideCodeBox_;
        private Label ideSyntaxStatusLbl_;
        private object currentIdeObject_;

        // Live Server Console Controls
        private RichTextBox liveLogBox_;
        private TextBox liveConsoleInput_;
        private ListBox onlinePlayersList_;

        public StudioWorkspaceControl() : this(null)
        {
        }

        internal StudioWorkspaceControl(MainForm mainForm)
        {
            this.mainForm_ = mainForm;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.Font = StudioTheme.FontBase;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            this.BuildTopHeaderBar();
            this.BuildLeftNavRail();
            this.BuildContentHost();
        }

        public StudioTab ActiveTab
        {
            get { return this.activeTab_; }
        }

        internal AreaCanvas MapCanvas
        {
            get { return this.mapCanvas_; }
        }

        public void BindServer(Server server)
        {
            this.server_ = server;
            this.RefreshStudio();
        }

        public void AppendLogLine(string line)
        {
            if (this.liveLogBox_ == null || this.liveLogBox_.IsDisposed)
                return;
            if (this.liveLogBox_.TextLength > 0)
                this.liveLogBox_.AppendText("\r\n");
            this.liveLogBox_.AppendText(line);
            this.liveLogBox_.ScrollToCaret();
        }

        public void UpdateServerRunningState(bool running, int port, int userCount)
        {
            if (this.serverStatusDot_ != null)
            {
                if (running)
                {
                    this.serverStatusDot_.Text = "● ONLINE  (Port " + port + " • " + userCount + " Players)";
                    this.serverStatusDot_.ForeColor = Color.FromArgb(74, 222, 128);
                    this.startStopServerBtn_.Text = "■ Stop Server";
                    this.startStopServerBtn_.BackColor = Color.FromArgb(220, 38, 38);
                    this.portInputBox_.Enabled = false;
                }
                else
                {
                    this.serverStatusDot_.Text = "○ SERVER OFFLINE";
                    this.serverStatusDot_.ForeColor = Color.FromArgb(251, 191, 36);
                    this.startStopServerBtn_.Text = "▶ Start Server";
                    this.startStopServerBtn_.BackColor = Color.FromArgb(22, 163, 74);
                    this.portInputBox_.Enabled = true;
                }
            }
            this.RefreshOnlinePlayersList();
        }

        public void RefreshStudio()
        {
            if (this.server_ == null)
                this.server_ = Server.Instance;
            if (this.server_ == null)
                return;

            string mudName = (this.server_.Settings != null && !string.IsNullOrEmpty(this.server_.Settings.Name))
                ? this.server_.Settings.Name
                : "Unnamed MUD";
            if (this.mudTitleLabel_ != null)
                this.mudTitleLabel_.Text = "✦ " + mudName;

            if (this.portInputBox_ != null && this.mainForm_ != null && !this.portInputBox_.Focused)
                this.portInputBox_.Text = this.mainForm_.ConfiguredPort.ToString();

            this.UpdateServerRunningState(
                this.server_.IsRunning,
                this.mainForm_ != null ? this.mainForm_.ConfiguredPort : 4000,
                this.server_.Users != null ? this.server_.Users.Count : 0);

            this.UpdateNavBadges();
            this.ShowTab(this.activeTab_);
        }

        #region 1. Top Studio Header Bar

        private void BuildTopHeaderBar()
        {
            this.topBar_ = new Panel();
            this.topBar_.Dock = DockStyle.Top;
            this.topBar_.Height = 54;
            this.topBar_.BackColor = Color.FromArgb(11, 15, 25);
            this.topBar_.Padding = new Padding(12, 8, 12, 8);
            this.topBar_.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen borderPen = new Pen(Color.FromArgb(30, 41, 59), 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, this.topBar_.Height - 1, this.topBar_.Width, this.topBar_.Height - 1);
                }
            };

            // Left Brand & Project Actions
            FlowLayoutPanel leftFlow = new FlowLayoutPanel();
            leftFlow.Dock = DockStyle.Left;
            leftFlow.AutoSize = true;
            leftFlow.WrapContents = false;
            leftFlow.BackColor = Color.Transparent;
            leftFlow.Padding = new Padding(0, 2, 0, 0);

            Label brandBadge = new Label();
            brandBadge.Text = "⚡ ULTIMUD STUDIO";
            brandBadge.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            brandBadge.ForeColor = Color.FromArgb(96, 165, 250);
            brandBadge.AutoSize = true;
            brandBadge.Margin = new Padding(0, 6, 8, 0);
            leftFlow.Controls.Add(brandBadge);

            this.mudTitleLabel_ = new Label();
            this.mudTitleLabel_.Text = "✦ Unnamed MUD";
            this.mudTitleLabel_.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.mudTitleLabel_.ForeColor = Color.White;
            this.mudTitleLabel_.BackColor = Color.FromArgb(30, 41, 59);
            this.mudTitleLabel_.AutoSize = true;
            this.mudTitleLabel_.Padding = new Padding(8, 5, 8, 5);
            this.mudTitleLabel_.Margin = new Padding(0, 2, 10, 0);
            leftFlow.Controls.Add(this.mudTitleLabel_);

            leftFlow.Controls.Add(this.CreateHeaderButton("📄 New", Color.FromArgb(30, 41, 59), Color.FromArgb(226, 232, 240), 66, delegate
            {
                if (this.mainForm_ != null) this.mainForm_.TriggerNewMud();
            }));
            leftFlow.Controls.Add(this.CreateHeaderButton("📂 Open", Color.FromArgb(30, 41, 59), Color.FromArgb(226, 232, 240), 70, delegate
            {
                if (this.mainForm_ != null) this.mainForm_.TriggerOpenMud();
            }));
            leftFlow.Controls.Add(this.CreateHeaderButton("💾 Save All", Color.FromArgb(37, 99, 235), Color.White, 86, delegate
            {
                if (this.server_ != null)
                {
                    this.server_.Save();
                    MessageBox.Show(this, "All MUD world areas, blueprints, scripts, and settings have been saved!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }));

            // Center Server Control Capsule
            this.serverStatusPill_ = new Panel();
            this.serverStatusPill_.Width = 395;
            this.serverStatusPill_.Height = 36;
            this.serverStatusPill_.Location = new Point(455, 9);
            this.serverStatusPill_.BackColor = Color.FromArgb(20, 29, 47);
            this.serverStatusPill_.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(Color.FromArgb(51, 65, 85)))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, this.serverStatusPill_.Width - 1, this.serverStatusPill_.Height - 1);
                }
            };

            this.serverStatusDot_ = new Label();
            this.serverStatusDot_.Text = "○ SERVER OFFLINE";
            this.serverStatusDot_.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.serverStatusDot_.ForeColor = Color.FromArgb(251, 191, 36);
            this.serverStatusDot_.Location = new Point(10, 9);
            this.serverStatusDot_.Size = new Size(195, 18);
            this.serverStatusPill_.Controls.Add(this.serverStatusDot_);

            Label portLbl = new Label();
            portLbl.Text = "Port:";
            portLbl.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular);
            portLbl.ForeColor = Color.FromArgb(148, 163, 184);
            portLbl.Location = new Point(206, 10);
            portLbl.AutoSize = true;
            this.serverStatusPill_.Controls.Add(portLbl);

            this.portInputBox_ = new TextBox();
            this.portInputBox_.Text = "4000";
            this.portInputBox_.Location = new Point(238, 7);
            this.portInputBox_.Size = new Size(46, 22);
            this.portInputBox_.BackColor = Color.FromArgb(11, 15, 25);
            this.portInputBox_.ForeColor = Color.White;
            this.portInputBox_.BorderStyle = BorderStyle.FixedSingle;
            this.portInputBox_.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.portInputBox_.TextChanged += delegate
            {
                if (this.mainForm_ != null)
                    this.mainForm_.SetPortText(this.portInputBox_.Text);
            };
            this.serverStatusPill_.Controls.Add(this.portInputBox_);

            this.startStopServerBtn_ = new Button();
            this.startStopServerBtn_.Text = "▶ Start Server";
            this.startStopServerBtn_.Location = new Point(290, 4);
            this.startStopServerBtn_.Size = new Size(98, 28);
            this.startStopServerBtn_.FlatStyle = FlatStyle.Flat;
            this.startStopServerBtn_.FlatAppearance.BorderSize = 0;
            this.startStopServerBtn_.BackColor = Color.FromArgb(22, 163, 74);
            this.startStopServerBtn_.ForeColor = Color.White;
            this.startStopServerBtn_.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.startStopServerBtn_.Cursor = Cursors.Hand;
            this.startStopServerBtn_.Click += delegate
            {
                if (this.server_ != null && this.server_.IsRunning)
                {
                    this.server_.Stop();
                    this.UpdateServerRunningState(false, this.mainForm_.ConfiguredPort, 0);
                }
                else if (this.mainForm_ != null)
                {
                    this.mainForm_.StartServerForPlaytest();
                }
            };
            this.serverStatusPill_.Controls.Add(this.startStopServerBtn_);

            // Right Suite Action Buttons
            FlowLayoutPanel rightFlow = new FlowLayoutPanel();
            rightFlow.Dock = DockStyle.Right;
            rightFlow.AutoSize = true;
            rightFlow.WrapContents = false;
            rightFlow.BackColor = Color.Transparent;
            rightFlow.Padding = new Padding(0, 2, 0, 0);

            rightFlow.Controls.Add(this.CreateHeaderButton("🎮 Playtest Client", Color.FromArgb(21, 128, 61), Color.White, 122, delegate
            {
                if (this.mainForm_ != null)
                    TestClientForm.ShowPlaytester(this.mainForm_, this.mainForm_.ConfiguredPort);
            }));

            rightFlow.Controls.Add(this.CreateHeaderButton("✦ Starter Kit", Color.FromArgb(109, 40, 217), Color.White, 102, delegate
            {
                using (MudStarterKitForm dlg = new MudStarterKitForm())
                {
                    dlg.ShowDialog(this.FindForm());
                    this.RefreshStudio();
                }
            }));

            rightFlow.Controls.Add(this.CreateHeaderButton("✔ Audit MUD", Color.FromArgb(14, 116, 144), Color.White, 100, delegate
            {
                using (MudAuditorForm dlg = new MudAuditorForm())
                {
                    dlg.ShowDialog(this.FindForm());
                    this.RefreshStudio();
                }
            }));

            this.toggleClassicBtn_ = this.CreateHeaderButton("🗂️ Classic View", Color.FromArgb(51, 65, 85), Color.FromArgb(226, 232, 240), 108, delegate
            {
                if (this.mainForm_ != null)
                    this.mainForm_.ToggleClassicStudioMode();
            });
            rightFlow.Controls.Add(this.toggleClassicBtn_);

            this.topBar_.Controls.Add(rightFlow);
            this.topBar_.Controls.Add(this.serverStatusPill_);
            this.topBar_.Controls.Add(leftFlow);
            this.Controls.Add(this.topBar_);
        }

        private Button CreateHeaderButton(string text, Color bg, Color fg, int width, EventHandler onClick)
        {
            Button b = new Button();
            b.Text = text;
            b.Size = new Size(width, 32);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = ControlPaint.Light(bg, 0.2f);
            b.BackColor = bg;
            b.ForeColor = fg;
            b.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.Margin = new Padding(0, 0, 6, 0);
            if (onClick != null)
                b.Click += onClick;
            return b;
        }

        #endregion

        #region 2. Left Dark Navigation Rail

        private void BuildLeftNavRail()
        {
            this.navRail_ = new Panel();
            this.navRail_.Dock = DockStyle.Left;
            this.navRail_.Width = 224;
            this.navRail_.BackColor = Color.FromArgb(17, 24, 39);
            this.navRail_.Padding = new Padding(10, 12, 10, 12);
            this.navRail_.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen borderPen = new Pen(Color.FromArgb(30, 41, 59), 1))
                {
                    e.Graphics.DrawLine(borderPen, this.navRail_.Width - 1, 0, this.navRail_.Width - 1, this.navRail_.Height);
                }
            };

            Label navHeader = new Label();
            navHeader.Text = "WORKSPACES & EDITORS";
            navHeader.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            navHeader.ForeColor = Color.FromArgb(100, 116, 139);
            navHeader.Location = new Point(14, 12);
            navHeader.AutoSize = true;
            this.navRail_.Controls.Add(navHeader);

            string[] titles = new string[]
            {
                "🏠  Studio Dashboard",
                "🗺️  2D Map & World Painter",
                "⚔️  Items & Gear Forge",
                "🐉  NPC & Monster Bestiary",
                "✨  Abilities & Spells",
                "🛡️  Races, Classes & Stats",
                "📜  Lua Code & Help IDE",
                "🖥️  Live Server & Console"
            };

            string[] subtitles = new string[]
            {
                "Overview, Quick Setup & Settings",
                "Paint Rooms, Roads, Walls & Spawns",
                "Weapons, Armor, Potions & Loot",
                "Monsters, Shopkeepers & Trainers",
                "Combat Skills, Magic & Buffs",
                "Playable Origins & RPG Attributes",
                "Edit Commands, Scripts & Help",
                "Logs, Players & Admin Console"
            };

            this.navButtons_.Clear();
            int y = 34;
            for (int i = 0; i < titles.Length; i++)
            {
                StudioTab tab = (StudioTab)i;
                string title = titles[i];
                string sub = subtitles[i];

                Button btn = new Button();
                btn.Tag = tab;
                btn.Location = new Point(8, y);
                btn.Size = new Size(208, 52);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Cursor = Cursors.Hand;
                btn.Text = string.Empty;

                btn.Paint += delegate(object sender, PaintEventArgs pe)
                {
                    Button self = sender as Button;
                    StudioTab myTab = (StudioTab)self.Tag;
                    bool isActive = (this.activeTab_ == myTab);

                    Color bg = isActive ? Color.FromArgb(30, 41, 59) : Color.FromArgb(17, 24, 39);
                    pe.Graphics.Clear(bg);

                    if (isActive)
                    {
                        using (SolidBrush accent = new SolidBrush(Color.FromArgb(59, 130, 246)))
                        {
                            pe.Graphics.FillRectangle(accent, 0, 6, 4, self.Height - 12);
                        }
                        using (Pen border = new Pen(Color.FromArgb(51, 65, 85)))
                        {
                            pe.Graphics.DrawRectangle(border, 0, 0, self.Width - 1, self.Height - 1);
                        }
                    }

                    using (SolidBrush titleBrush = new SolidBrush(isActive ? Color.White : Color.FromArgb(203, 213, 225)))
                    using (SolidBrush subBrush = new SolidBrush(isActive ? Color.FromArgb(147, 197, 253) : Color.FromArgb(100, 116, 139)))
                    {
                        pe.Graphics.DrawString(title, new Font("Segoe UI", 9.25F, FontStyle.Bold), titleBrush, new PointF(12, 8));
                        pe.Graphics.DrawString(sub, new Font("Segoe UI", 7.75F, FontStyle.Regular), subBrush, new PointF(14, 28));
                    }
                };

                btn.Click += delegate(object sender, EventArgs e)
                {
                    StudioTab clickedTab = (StudioTab)((Button)sender).Tag;
                    this.ShowTab(clickedTab);
                };

                this.navRail_.Controls.Add(btn);
                this.navButtons_.Add(btn);
                y += 56;
            }

            // Bottom quick-help card in the sidebar
            Panel tipCard = new Panel();
            tipCard.Location = new Point(10, y + 10);
            tipCard.Size = new Size(204, 96);
            tipCard.BackColor = Color.FromArgb(15, 23, 42);
            tipCard.Paint += delegate(object s, PaintEventArgs pe)
            {
                using (Pen p = new Pen(Color.FromArgb(30, 41, 59)))
                {
                    pe.Graphics.DrawRectangle(p, 0, 0, tipCard.Width - 1, tipCard.Height - 1);
                }
            };

            Label tipTitle = new Label();
            tipTitle.Text = "💡 New to UltiMUD?";
            tipTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            tipTitle.ForeColor = Color.FromArgb(250, 204, 21);
            tipTitle.Location = new Point(10, 8);
            tipTitle.AutoSize = true;
            tipCard.Controls.Add(tipTitle);

            Label tipBody = new Label();
            tipBody.Text = "1. Click '✦ Starter Kit' to generate a complete RPG world.\r\n2. Paint maps in '2D Map Painter'.\r\n3. Click '🎮 Playtest Client'!";
            tipBody.Font = new Font("Segoe UI", 7.75F, FontStyle.Regular);
            tipBody.ForeColor = Color.FromArgb(148, 163, 184);
            tipBody.Location = new Point(10, 28);
            tipBody.Size = new Size(186, 62);
            tipCard.Controls.Add(tipBody);

            this.navRail_.Controls.Add(tipCard);
            this.Controls.Add(this.navRail_);
        }

        private void UpdateNavBadges()
        {
            foreach (Button b in this.navButtons_)
                b.Invalidate();
        }

        #endregion

        #region 3. Content Host & Tab Switching

        private void BuildContentHost()
        {
            this.contentHost_ = new Panel();
            this.contentHost_.Dock = DockStyle.Fill;
            this.contentHost_.BackColor = Color.FromArgb(241, 245, 249);
            this.Controls.Add(this.contentHost_);
            this.contentHost_.BringToFront();
        }

        public void ShowTab(StudioTab tab)
        {
            this.activeTab_ = tab;
            this.UpdateNavBadges();

            if (!this.tabPages_.ContainsKey(tab))
            {
                Control page = this.CreateTabPage(tab);
                page.Dock = DockStyle.Fill;
                this.tabPages_[tab] = page;
                this.contentHost_.Controls.Add(page);
            }

            foreach (KeyValuePair<StudioTab, Control> pair in this.tabPages_)
            {
                pair.Value.Visible = (pair.Key == tab);
            }

            Control activeControl = this.tabPages_[tab];
            activeControl.BringToFront();
            this.RefreshActiveTabContent(tab, activeControl);
        }

        private Control CreateTabPage(StudioTab tab)
        {
            switch (tab)
            {
                case StudioTab.Dashboard: return this.BuildDashboardPage();
                case StudioTab.MapPainter: return this.BuildMapPainterPage();
                case StudioTab.ItemsForge: return this.BuildBlueprintForgePage(typeof(Item), "⚔️ Items & Equipment Forge", "Create and customize weapons, armor, potions, containers, and quest items.");
                case StudioTab.NpcBestiary: return this.BuildBlueprintForgePage(typeof(Npc), "🐉 NPC & Monster Bestiary", "Design monsters, town guards, shopkeepers, skill trainers, and interactive quest NPCs.");
                case StudioTab.AbilitiesStudio: return this.BuildBlueprintForgePage(typeof(Ability), "✨ Abilities, Spells & Combat Skills", "Build melee attacks, spells, heals, and buffs with custom cooldowns and Lua effects.");
                case StudioTab.RacesClassesStats: return this.BuildRacesClassesStatsPage();
                case StudioTab.LuaCodeIde: return this.BuildLuaCodeIdePage();
                case StudioTab.ServerConsole: return this.BuildServerConsolePage();
                default: return this.BuildDashboardPage();
            }
        }

        private void RefreshActiveTabContent(StudioTab tab, Control page)
        {
            if (this.server_ == null)
                this.server_ = Server.Instance;
            if (this.server_ == null)
                return;

            switch (tab)
            {
                case StudioTab.Dashboard:
                    this.RefreshDashboardPage();
                    break;
                case StudioTab.MapPainter:
                    this.RefreshMapPainterPage();
                    break;
                case StudioTab.ItemsForge:
                case StudioTab.NpcBestiary:
                case StudioTab.AbilitiesStudio:
                case StudioTab.RacesClassesStats:
                    if (page.Tag is Action)
                        ((Action)page.Tag)();
                    break;
                case StudioTab.LuaCodeIde:
                    this.RefreshLuaIdeList();
                    break;
                case StudioTab.ServerConsole:
                    this.RefreshOnlinePlayersList();
                    break;
            }
        }

        private delegate void Action();

        #endregion

        #region Page 1: Studio Dashboard & Quick Setup

        private Control BuildDashboardPage()
        {
            Panel page = new Panel();
            page.BackColor = Color.FromArgb(241, 245, 249);
            page.Padding = new Padding(20);
            page.AutoScroll = true;

            // Hero Welcome Card
            Panel heroCard = new Panel();
            heroCard.Dock = DockStyle.Top;
            heroCard.Height = 118;
            heroCard.BackColor = Color.FromArgb(15, 23, 42);
            heroCard.Padding = new Padding(20, 16, 20, 16);

            Label heroTitle = new Label();
            heroTitle.Text = "Welcome to UltiMUD Creator Studio";
            heroTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            heroTitle.ForeColor = Color.White;
            heroTitle.Location = new Point(20, 14);
            heroTitle.AutoSize = true;
            heroCard.Controls.Add(heroTitle);

            Label heroSub = new Label();
            heroSub.Text = "Build your 2D tilemap world, customize RPG stats/races/classes, script commands in Lua, and playtest live in one place.";
            heroSub.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            heroSub.ForeColor = Color.FromArgb(148, 163, 184);
            heroSub.Location = new Point(22, 44);
            heroSub.AutoSize = true;
            heroCard.Controls.Add(heroSub);

            Label namePrompt = new Label();
            namePrompt.Text = "MUD World Title:";
            namePrompt.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            namePrompt.ForeColor = Color.FromArgb(147, 197, 253);
            namePrompt.Location = new Point(22, 78);
            namePrompt.AutoSize = true;
            heroCard.Controls.Add(namePrompt);

            this.dashMudNameBox_ = new TextBox();
            this.dashMudNameBox_.Location = new Point(132, 75);
            this.dashMudNameBox_.Size = new Size(220, 24);
            this.dashMudNameBox_.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.dashMudNameBox_.BackColor = Color.FromArgb(30, 41, 59);
            this.dashMudNameBox_.ForeColor = Color.White;
            this.dashMudNameBox_.BorderStyle = BorderStyle.FixedSingle;
            heroCard.Controls.Add(this.dashMudNameBox_);

            Button applyNameBtn = this.CreateHeaderButton("Update Name", Color.FromArgb(37, 99, 235), Color.White, 104, delegate
            {
                if (this.server_ != null && this.server_.Settings != null && !string.IsNullOrEmpty(this.dashMudNameBox_.Text.Trim()))
                {
                    this.server_.Settings.Name = this.dashMudNameBox_.Text.Trim();
                    this.server_.Save();
                    this.RefreshStudio();
                }
            });
            applyNameBtn.Location = new Point(360, 72);
            applyNameBtn.Height = 28;
            heroCard.Controls.Add(applyNameBtn);

            Button heroStarterBtn = this.CreateHeaderButton("✦ Populate with Starter Kit Wizard", Color.FromArgb(124, 58, 237), Color.White, 225, delegate
            {
                using (MudStarterKitForm dlg = new MudStarterKitForm())
                {
                    dlg.ShowDialog(this.FindForm());
                    this.RefreshStudio();
                }
            });
            heroStarterBtn.Location = new Point(476, 72);
            heroStarterBtn.Height = 28;
            heroCard.Controls.Add(heroStarterBtn);

            Button heroMapBtn = this.CreateHeaderButton("🗺️ Open 2D Map Painter →", Color.FromArgb(13, 148, 136), Color.White, 185, delegate
            {
                this.ShowTab(StudioTab.MapPainter);
            });
            heroMapBtn.Location = new Point(710, 72);
            heroMapBtn.Height = 28;
            heroCard.Controls.Add(heroMapBtn);

            // 6 Clickable Metric Cards Row
            FlowLayoutPanel metricsRow = new FlowLayoutPanel();
            metricsRow.Dock = DockStyle.Top;
            metricsRow.Height = 106;
            metricsRow.WrapContents = false;
            metricsRow.Padding = new Padding(0, 14, 0, 10);
            metricsRow.BackColor = Color.Transparent;

            string[] cardTitles = new string[]
            {
                "🗺️ Map Objects & Spawns",
                "⚔️ Items & Equipment",
                "🐉 NPCs & Bestiary",
                "✨ Abilities & Spells",
                "🛡️ Races, Classes & Stats",
                "📜 Commands & Help"
            };
            Color[] cardAccents = new Color[]
            {
                Color.FromArgb(37, 99, 235),
                Color.FromArgb(217, 119, 6),
                Color.FromArgb(220, 38, 38),
                Color.FromArgb(124, 58, 237),
                Color.FromArgb(13, 148, 136),
                Color.FromArgb(2, 132, 199)
            };
            StudioTab[] cardTargets = new StudioTab[]
            {
                StudioTab.MapPainter,
                StudioTab.ItemsForge,
                StudioTab.NpcBestiary,
                StudioTab.AbilitiesStudio,
                StudioTab.RacesClassesStats,
                StudioTab.LuaCodeIde
            };

            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                StudioTab targetTab = cardTargets[i];
                Color accent = cardAccents[i];

                Panel card = new Panel();
                card.Size = new Size(162, 80);
                card.Margin = new Padding(0, 0, 12, 0);
                card.BackColor = Color.White;
                card.Cursor = Cursors.Hand;
                card.Paint += delegate(object s, PaintEventArgs pe)
                {
                    using (SolidBrush topBar = new SolidBrush(accent))
                        pe.Graphics.FillRectangle(topBar, 0, 0, card.Width, 4);
                    using (Pen border = new Pen(Color.FromArgb(203, 213, 225)))
                        pe.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
                };

                Label valLbl = new Label();
                valLbl.Text = "0";
                valLbl.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
                valLbl.ForeColor = Color.FromArgb(15, 23, 42);
                valLbl.Location = new Point(12, 10);
                valLbl.AutoSize = true;
                valLbl.Cursor = Cursors.Hand;
                this.dashMetricValues_[idx] = valLbl;
                card.Controls.Add(valLbl);

                Label capLbl = new Label();
                capLbl.Text = cardTitles[i];
                capLbl.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
                capLbl.ForeColor = Color.FromArgb(71, 85, 105);
                capLbl.Location = new Point(12, 50);
                capLbl.AutoSize = true;
                capLbl.Cursor = Cursors.Hand;
                card.Controls.Add(capLbl);

                EventHandler goHandler = delegate { this.ShowTab(targetTab); };
                card.Click += goHandler;
                valLbl.Click += goHandler;
                capLbl.Click += goHandler;

                metricsRow.Controls.Add(card);
            }

            // Bottom Split: Left = Quick Workflow Launchpad, Right = Global MUD Settings & Formats Inspector
            SplitContainer bottomSplit = new SplitContainer();
            bottomSplit.Dock = DockStyle.Fill;
            bottomSplit.SplitterDistance = 440;
            bottomSplit.BackColor = Color.FromArgb(241, 245, 249);

            // Left Card: Step-by-Step Builder Guide & 1-Click Shortcuts
            Panel guideCard = new Panel();
            guideCard.Dock = DockStyle.Fill;
            guideCard.BackColor = Color.White;
            guideCard.Padding = new Padding(18);
            guideCard.Paint += delegate(object s, PaintEventArgs pe)
            {
                using (Pen border = new Pen(Color.FromArgb(203, 213, 225)))
                    pe.Graphics.DrawRectangle(border, 0, 0, guideCard.Width - 1, guideCard.Height - 1);
            };

            Label guideHeader = new Label();
            guideHeader.Text = "🚀 Quick-Build Studio Actions";
            guideHeader.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            guideHeader.ForeColor = Color.FromArgb(15, 23, 42);
            guideHeader.Dock = DockStyle.Top;
            guideHeader.Height = 28;

            FlowLayoutPanel actionGrid = new FlowLayoutPanel();
            actionGrid.Dock = DockStyle.Fill;
            actionGrid.FlowDirection = FlowDirection.TopDown;
            actionGrid.WrapContents = false;
            actionGrid.AutoScroll = true;
            actionGrid.Padding = new Padding(0, 6, 0, 0);

            actionGrid.Controls.Add(this.CreateDashboardActionRow(
                "🗺️ Paint Your 2D World Map",
                "Draw roads, indoor rooms, walls, doors, shops, and monster spawns directly on the interactive grid.",
                "Open Map Painter", Color.FromArgb(37, 99, 235), delegate { this.ShowTab(StudioTab.MapPainter); }));

            actionGrid.Controls.Add(this.CreateDashboardActionRow(
                "⚔️ Forge Weapons, Armor & Items",
                "Create equippable gear, consumables, containers, and stat-boosting items with visual controls.",
                "Open Item Forge", Color.FromArgb(217, 119, 6), delegate { this.ShowTab(StudioTab.ItemsForge); }));

            actionGrid.Controls.Add(this.CreateDashboardActionRow(
                "🐉 Populate NPCs, Bosses & Trainers",
                "Build friendly town NPCs, aggressive monsters, dialogue triggers, and ability trainers.",
                "Open Bestiary", Color.FromArgb(220, 38, 38), delegate { this.ShowTab(StudioTab.NpcBestiary); }));

            actionGrid.Controls.Add(this.CreateDashboardActionRow(
                "📜 Edit Lua Commands & Help Topics",
                "Customize all 100+ game commands, system formulas, and in-game HELP files in the built-in Lua IDE.",
                "Open Lua IDE", Color.FromArgb(2, 132, 199), delegate { this.ShowTab(StudioTab.LuaCodeIde); }));

            guideCard.Controls.Add(actionGrid);
            guideCard.Controls.Add(guideHeader);
            bottomSplit.Panel1.Controls.Add(guideCard);

            // Right Card: MUD Global Settings & Look/Who/Stats Formatting
            Panel settingsCard = new Panel();
            settingsCard.Dock = DockStyle.Fill;
            settingsCard.BackColor = Color.White;
            settingsCard.Padding = new Padding(14);
            settingsCard.Paint += delegate(object s, PaintEventArgs pe)
            {
                using (Pen border = new Pen(Color.FromArgb(203, 213, 225)))
                    pe.Graphics.DrawRectangle(border, 0, 0, settingsCard.Width - 1, settingsCard.Height - 1);
            };

            Panel settingsTopBar = new Panel();
            settingsTopBar.Dock = DockStyle.Top;
            settingsTopBar.Height = 36;

            Label settingsTitle = new Label();
            settingsTitle.Text = "⚙️ Global MUD Settings & Output Formats";
            settingsTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            settingsTitle.ForeColor = Color.FromArgb(15, 23, 42);
            settingsTitle.Location = new Point(0, 6);
            settingsTitle.AutoSize = true;
            settingsTopBar.Controls.Add(settingsTitle);

            this.dashSettingsGrid_ = new PropertyGrid();
            this.dashSettingsGrid_.Dock = DockStyle.Fill;
            StudioTheme.StylePropertyGrid(this.dashSettingsGrid_);
            this.dashSettingsGrid_.PropertyValueChanged += delegate
            {
                if (this.server_ != null)
                    this.server_.Save();
            };

            settingsCard.Controls.Add(this.dashSettingsGrid_);
            settingsCard.Controls.Add(settingsTopBar);
            bottomSplit.Panel2.Controls.Add(settingsCard);

            page.Controls.Add(bottomSplit);
            page.Controls.Add(metricsRow);
            page.Controls.Add(heroCard);
            return page;
        }

        private Panel CreateDashboardActionRow(string title, string desc, string btnText, Color btnColor, EventHandler onClick)
        {
            Panel row = new Panel();
            row.Size = new Size(390, 74);
            row.Margin = new Padding(0, 0, 0, 10);
            row.BackColor = Color.FromArgb(248, 250, 252);
            row.Paint += delegate(object s, PaintEventArgs pe)
            {
                using (Pen p = new Pen(Color.FromArgb(226, 232, 240)))
                    pe.Graphics.DrawRectangle(p, 0, 0, row.Width - 1, row.Height - 1);
            };

            Label t = new Label();
            t.Text = title;
            t.Font = new Font("Segoe UI", 9.25F, FontStyle.Bold);
            t.ForeColor = Color.FromArgb(15, 23, 42);
            t.Location = new Point(12, 8);
            t.AutoSize = true;
            row.Controls.Add(t);

            Label d = new Label();
            d.Text = desc;
            d.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            d.ForeColor = Color.FromArgb(71, 85, 105);
            d.Location = new Point(12, 28);
            d.Size = new Size(252, 40);
            row.Controls.Add(d);

            Button b = this.CreateHeaderButton(btnText, btnColor, Color.White, 114, onClick);
            b.Location = new Point(268, 20);
            row.Controls.Add(b);

            return row;
        }

        private void RefreshDashboardPage()
        {
            if (this.server_ == null)
                return;

            if (this.dashMudNameBox_ != null && this.server_.Settings != null && !this.dashMudNameBox_.Focused)
                this.dashMudNameBox_.Text = this.server_.Settings.Name;

            if (this.dashSettingsGrid_ != null)
                this.dashSettingsGrid_.SelectedObject = this.server_.Settings;

            int mapObjCount = (this.server_.MainArea != null)
                ? (this.server_.MainArea.Children.Count + this.server_.MainArea.Resets.Count)
                : 0;
            int itemCount = this.GetDataSetCount(typeof(Item));
            int npcCount = this.GetDataSetCount(typeof(Npc));
            int abilityCount = this.GetDataSetCount(typeof(Ability)) + this.GetDataSetCount(typeof(StatusEffectBlueprint));
            int rpgCount = this.GetDataSetCount(typeof(Race)) + this.GetDataSetCount(typeof(Job)) + this.GetDataSetCount(typeof(StatBlueprint));
            int codeCount = (Command.Commands != null ? Command.Commands.Count : 0) + this.GetDataSetCount(typeof(Script)) + (Help.HelpFiles != null ? Help.HelpFiles.Count : 0);

            int[] vals = new int[] { mapObjCount, itemCount, npcCount, abilityCount, rpgCount, codeCount };
            for (int i = 0; i < 6; i++)
            {
                if (this.dashMetricValues_[i] != null)
                    this.dashMetricValues_[i].Text = vals[i].ToString();
            }
        }

        private int GetDataSetCount(Type t)
        {
            if (this.server_ != null && this.server_.DataSets != null && this.server_.DataSets.ContainsKey(t))
                return this.server_.DataSets[t].Count;
            return 0;
        }

        #endregion

        #region Page 2: Inline 2D Map & World Painter Studio

        private Control BuildMapPainterPage()
        {
            Panel page = new Panel();
            page.BackColor = Color.FromArgb(15, 23, 42);

            // Left Palette Dock: Tool Modes, 1-Click Layer Brushes, Spawns & Map Object List
            Panel leftPalette = new Panel();
            leftPalette.Dock = DockStyle.Left;
            leftPalette.Width = 248;
            leftPalette.BackColor = Color.FromArgb(30, 41, 59);
            leftPalette.Padding = new Padding(10);

            Label paletteTitle = new Label();
            paletteTitle.Text = "🎨 MAP BRUSHES & TOOLS";
            paletteTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            paletteTitle.ForeColor = Color.FromArgb(147, 197, 253);
            paletteTitle.Location = new Point(10, 8);
            paletteTitle.AutoSize = true;
            leftPalette.Controls.Add(paletteTitle);

            this.mapActiveToolLabel_ = new Label();
            this.mapActiveToolLabel_.Text = "Tool: ✋ Pan Camera";
            this.mapActiveToolLabel_.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            this.mapActiveToolLabel_.ForeColor = Color.FromArgb(74, 222, 128);
            this.mapActiveToolLabel_.Location = new Point(10, 26);
            this.mapActiveToolLabel_.AutoSize = true;
            leftPalette.Controls.Add(this.mapActiveToolLabel_);

            // 4 Camera / Manipulation Mode Buttons (2x2 Grid)
            int by = 48;
            leftPalette.Controls.Add(this.CreateMapToolButton("✋ Pan Camera", 10, by, 110, Color.FromArgb(51, 65, 85), delegate
            {
                this.mapCanvas_.Mode = AreaCanvas.EditModes.Pan;
                this.mapActiveToolLabel_.Text = "Tool: ✋ Pan (Drag Map)";
            }));
            leftPalette.Controls.Add(this.CreateMapToolButton("👆 Select Tile", 126, by, 110, Color.FromArgb(51, 65, 85), delegate
            {
                this.mapCanvas_.Mode = AreaCanvas.EditModes.Select;
                this.mapActiveToolLabel_.Text = "Tool: 👆 Select Object";
            }));
            by += 32;
            leftPalette.Controls.Add(this.CreateMapToolButton("↔ Move Object", 10, by, 110, Color.FromArgb(51, 65, 85), delegate
            {
                this.mapCanvas_.Mode = AreaCanvas.EditModes.Move;
                this.mapActiveToolLabel_.Text = "Tool: ↔ Drag Selected";
            }));
            leftPalette.Controls.Add(this.CreateMapToolButton("✏️ Paint Tiles", 126, by, 110, Color.FromArgb(37, 99, 235), delegate
            {
                this.mapCanvas_.Mode = AreaCanvas.EditModes.Manipulate;
                this.mapActiveToolLabel_.Text = "Tool: ✏️ Paint Points";
            }));

            by += 36;
            Label drawHdr = new Label();
            drawHdr.Text = "1-CLICK PAINT NEW LAYER:";
            drawHdr.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            drawHdr.ForeColor = Color.FromArgb(148, 163, 184);
            drawHdr.Location = new Point(10, by);
            drawHdr.AutoSize = true;
            leftPalette.Controls.Add(drawHdr);

            by += 18;
            leftPalette.Controls.Add(this.CreateMapToolButton("🛣️ + Road / Path", 10, by, 110, Color.FromArgb(21, 128, 61), delegate
            {
                this.StartPaintingNewLayer(new Area.Path(), "Road / Path", '.', Telnet.Color.Yellow);
            }));
            leftPalette.Controls.Add(this.CreateMapToolButton("🏛️ + Room / Region", 126, by, 110, Color.FromArgb(29, 78, 216), delegate
            {
                this.StartPaintingNewLayer(new Area.Region(), "Room / Region", ':', Telnet.Color.BrightCyan);
            }));
            by += 32;
            leftPalette.Controls.Add(this.CreateMapToolButton("🧱 + Wall", 10, by, 110, Color.FromArgb(180, 83, 9), delegate
            {
                Area.Wall w = new Area.Wall();
                w.LayerFlags.Set((int)Area.Layer.LayerFlag.NoWalk, true);
                this.StartPaintingNewLayer(w, "Stone Wall", '#', Telnet.Color.Gray);
            }));
            leftPalette.Controls.Add(this.CreateMapToolButton("🚪 + Door", 126, by, 110, Color.FromArgb(161, 98, 7), delegate
            {
                this.StartPaintingNewLayer(new Area.Door(), "Wooden Door", '+', Telnet.Color.Yellow);
            }));
            by += 32;
            leftPalette.Controls.Add(this.CreateMapToolButton("🌀 + Exit / Portal", 10, by, 110, Color.FromArgb(109, 40, 217), delegate
            {
                this.StartPaintingNewLayer(new Area.Exit(), "Area Exit", '@', Telnet.Color.BrightMagenta);
            }));
            leftPalette.Controls.Add(this.CreateMapToolButton("🏕️ + NPC Zone", 126, by, 110, Color.FromArgb(14, 116, 144), delegate
            {
                this.StartPaintingNewLayer(new Area.NpcRegion(), "NPC Wander Zone", '^', Telnet.Color.Green);
            }));

            by += 36;
            Label spawnHdr = new Label();
            spawnHdr.Text = "PLACE SPAWNS AT TILE:";
            spawnHdr.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            spawnHdr.ForeColor = Color.FromArgb(148, 163, 184);
            spawnHdr.Location = new Point(10, by);
            spawnHdr.AutoSize = true;
            leftPalette.Controls.Add(spawnHdr);

            by += 18;
            leftPalette.Controls.Add(this.CreateMapToolButton("🐉 + NPC Spawn", 10, by, 110, Color.FromArgb(153, 27, 27), delegate
            {
                if (this.mapCanvas_.Area == null) return;
                Point pt = this.mapCanvas_.SelectedPoint != Point.Empty ? this.mapCanvas_.SelectedPoint : this.mapCanvas_.Area.Start;
                NpcReset nr = new NpcReset(new Location(this.mapCanvas_.Area, pt));
                this.mapCanvas_.Area.Resets.Add(nr);
                this.mapCanvas_.Select(nr);
                this.RefreshMapObjectsList();
            }));
            leftPalette.Controls.Add(this.CreateMapToolButton("⚔️ + Item Spawn", 126, by, 110, Color.FromArgb(146, 64, 14), delegate
            {
                if (this.mapCanvas_.Area == null) return;
                Point pt = this.mapCanvas_.SelectedPoint != Point.Empty ? this.mapCanvas_.SelectedPoint : this.mapCanvas_.Area.Start;
                ItemReset ir = new ItemReset(new Location(this.mapCanvas_.Area, pt));
                this.mapCanvas_.Area.Resets.Add(ir);
                this.mapCanvas_.Select(ir);
                this.RefreshMapObjectsList();
            }));
            by += 32;
            leftPalette.Controls.Add(this.CreateMapToolButton("🏪 + Place Shop", 10, by, 110, Color.FromArgb(15, 118, 110), delegate
            {
                if (this.mapCanvas_.Area == null) return;
                Point pt = this.mapCanvas_.SelectedPoint != Point.Empty ? this.mapCanvas_.SelectedPoint : this.mapCanvas_.Area.Start;
                Shop shop = new Shop();
                shop.Name = "General Store";
                shop.Location = new Location(this.mapCanvas_.Area, pt);
                shop.Parent = this.mapCanvas_.Area;
                this.mapCanvas_.Select(shop);
                this.RefreshMapObjectsList();
            }));
            leftPalette.Controls.Add(this.CreateMapToolButton("🟢 Set Start Tile", 126, by, 110, Color.FromArgb(22, 101, 52), delegate
            {
                if (this.mapCanvas_.Area == null) return;
                Point pt = this.mapCanvas_.SelectedPoint != Point.Empty ? this.mapCanvas_.SelectedPoint : this.mapCanvas_.Area.Start;
                this.mapCanvas_.Area.Start = pt;
                this.mapCanvas_.Invalidate();
            }));

            by += 36;
            Label listHdr = new Label();
            listHdr.Text = "MAP LAYERS & SPAWNS IN AREA:";
            listHdr.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            listHdr.ForeColor = Color.FromArgb(148, 163, 184);
            listHdr.Location = new Point(10, by);
            listHdr.AutoSize = true;
            leftPalette.Controls.Add(listHdr);

            Button delLayerBtn = this.CreateMapToolButton("🗑️ Delete Selected Map Object", 10, by + 18, 226, Color.FromArgb(127, 29, 29), delegate
            {
                object sel = this.mapLayersList_.SelectedItem;
                if (sel == null) return;
                if (sel is Reset && this.mapCanvas_.Area != null)
                {
                    this.mapCanvas_.Area.Resets.Remove(sel as Reset);
                }
                else if (sel is Object && !(sel is Area))
                {
                    ((Object)sel).Parent = null;
                }
                this.RefreshMapObjectsList();
                this.mapCanvas_.Invalidate();
            });
            leftPalette.Controls.Add(delLayerBtn);

            this.mapLayersList_ = new ListBox();
            this.mapLayersList_.Location = new Point(10, by + 50);
            this.mapLayersList_.Size = new Size(226, 260);
            this.mapLayersList_.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.mapLayersList_.BackColor = Color.FromArgb(15, 23, 42);
            this.mapLayersList_.ForeColor = Color.FromArgb(226, 232, 240);
            this.mapLayersList_.BorderStyle = BorderStyle.FixedSingle;
            this.mapLayersList_.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this.mapLayersList_.SelectedIndexChanged += delegate
            {
                object item = this.mapLayersList_.SelectedItem;
                if (item == null) return;
                if (item is Area.Layer)
                {
                    this.mapCanvas_.Edit(item as Area.Layer);
                    this.BindMapInspectorObject(item);
                }
                else if (item is ILocation)
                {
                    this.mapCanvas_.Select(item);
                    this.mapCanvas_.GoTo(((ILocation)item).Location);
                    this.BindMapInspectorObject(item);
                }
            };
            leftPalette.Controls.Add(this.mapLayersList_);

            // Right Inspector Dock: Visual Tile/Layer Customizer + Full PropertyGrid
            Panel rightInspector = new Panel();
            rightInspector.Dock = DockStyle.Right;
            rightInspector.Width = 310;
            rightInspector.BackColor = Color.White;
            rightInspector.Padding = new Padding(10);

            this.mapLayerQuickPanel_ = new Panel();
            this.mapLayerQuickPanel_.Dock = DockStyle.Top;
            this.mapLayerQuickPanel_.Height = 195;
            this.mapLayerQuickPanel_.BackColor = Color.FromArgb(248, 250, 252);
            this.mapLayerQuickPanel_.Padding = new Padding(10);
            this.mapLayerQuickPanel_.Paint += delegate(object s, PaintEventArgs pe)
            {
                using (Pen p = new Pen(Color.FromArgb(203, 213, 225)))
                    pe.Graphics.DrawRectangle(p, 0, 0, this.mapLayerQuickPanel_.Width - 1, this.mapLayerQuickPanel_.Height - 1);
            };

            Label qTitle = new Label();
            qTitle.Text = "✨ Quick Tile & Layer Appearance";
            qTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            qTitle.ForeColor = Color.FromArgb(30, 58, 138);
            qTitle.Location = new Point(10, 8);
            qTitle.AutoSize = true;
            this.mapLayerQuickPanel_.Controls.Add(qTitle);

            Label lblName = new Label();
            lblName.Text = "Name:";
            lblName.Location = new Point(10, 34);
            lblName.AutoSize = true;
            this.mapLayerQuickPanel_.Controls.Add(lblName);

            this.mapObjNameBox_ = new TextBox();
            this.mapObjNameBox_.Location = new Point(78, 31);
            this.mapObjNameBox_.Size = new Size(200, 22);
            this.mapObjNameBox_.TextChanged += delegate
            {
                if (this.suppressMapQuickEvents_) return;
                if (this.mapInspectorGrid_.SelectedObject is Object)
                {
                    ((Object)this.mapInspectorGrid_.SelectedObject).Name = this.mapObjNameBox_.Text;
                    this.mapCanvas_.Invalidate();
                }
            };
            this.mapLayerQuickPanel_.Controls.Add(this.mapObjNameBox_);

            Label lblChar = new Label();
            lblChar.Text = "ASCII Tile:";
            lblChar.Location = new Point(10, 62);
            lblChar.AutoSize = true;
            this.mapLayerQuickPanel_.Controls.Add(lblChar);

            this.mapFillCharBox_ = new TextBox();
            this.mapFillCharBox_.Location = new Point(78, 59);
            this.mapFillCharBox_.Size = new Size(34, 22);
            this.mapFillCharBox_.MaxLength = 1;
            this.mapFillCharBox_.Font = new Font("Consolas", 10F, FontStyle.Bold);
            this.mapFillCharBox_.TextAlign = HorizontalAlignment.Center;
            this.mapFillCharBox_.TextChanged += delegate
            {
                if (this.suppressMapQuickEvents_ || string.IsNullOrEmpty(this.mapFillCharBox_.Text)) return;
                if (this.mapInspectorGrid_.SelectedObject is Area.Layer)
                {
                    ((Area.Layer)this.mapInspectorGrid_.SelectedObject).FillChar = this.mapFillCharBox_.Text[0];
                    this.mapCanvas_.Invalidate();
                }
                else if (this.mapInspectorGrid_.SelectedObject is Area)
                {
                    ((Area)this.mapInspectorGrid_.SelectedObject).FillChar = this.mapFillCharBox_.Text[0];
                    this.mapCanvas_.Invalidate();
                }
            };
            this.mapLayerQuickPanel_.Controls.Add(this.mapFillCharBox_);

            // Quick ASCII preset buttons
            char[] presets = new char[] { '.', '#', ':', '~', '^', '+', '@', '=' };
            int px = 118;
            foreach (char ch in presets)
            {
                char captured = ch;
                Button pb = new Button();
                pb.Text = captured.ToString();
                pb.Location = new Point(px, 59);
                pb.Size = new Size(20, 22);
                pb.FlatStyle = FlatStyle.Flat;
                pb.FlatAppearance.BorderSize = 1;
                pb.Font = new Font("Consolas", 8.25F, FontStyle.Bold);
                pb.Click += delegate { this.mapFillCharBox_.Text = captured.ToString(); };
                this.mapLayerQuickPanel_.Controls.Add(pb);
                px += 20;
            }

            Label lblCol = new Label();
            lblCol.Text = "Tile Color:";
            lblCol.Location = new Point(10, 90);
            lblCol.AutoSize = true;
            this.mapLayerQuickPanel_.Controls.Add(lblCol);

            this.mapFillColorCombo_ = new ComboBox();
            this.mapFillColorCombo_.Location = new Point(78, 87);
            this.mapFillColorCombo_.Size = new Size(200, 22);
            this.mapFillColorCombo_.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (Telnet.Color tc in Enum.GetValues(typeof(Telnet.Color)))
                this.mapFillColorCombo_.Items.Add(tc);
            this.mapFillColorCombo_.SelectedIndexChanged += delegate
            {
                if (this.suppressMapQuickEvents_ || this.mapFillColorCombo_.SelectedItem == null) return;
                Telnet.Color col = (Telnet.Color)this.mapFillColorCombo_.SelectedItem;
                if (this.mapInspectorGrid_.SelectedObject is Area.Layer)
                {
                    ((Area.Layer)this.mapInspectorGrid_.SelectedObject).FillColor = col;
                    this.mapCanvas_.Invalidate();
                }
                else if (this.mapInspectorGrid_.SelectedObject is Area)
                {
                    ((Area)this.mapInspectorGrid_.SelectedObject).FillColor = col;
                    this.mapCanvas_.Invalidate();
                }
            };
            this.mapLayerQuickPanel_.Controls.Add(this.mapFillColorCombo_);

            this.mapFlagNoWalk_ = new CheckBox();
            this.mapFlagNoWalk_.Text = "Impassable (Wall)";
            this.mapFlagNoWalk_.Location = new Point(10, 120);
            this.mapFlagNoWalk_.Size = new Size(135, 20);
            this.mapFlagNoWalk_.CheckedChanged += delegate { this.ApplyMapLayerFlags(); };
            this.mapLayerQuickPanel_.Controls.Add(this.mapFlagNoWalk_);

            this.mapFlagSafe_ = new CheckBox();
            this.mapFlagSafe_.Text = "Safe Room (No PvP)";
            this.mapFlagSafe_.Location = new Point(148, 120);
            this.mapFlagSafe_.Size = new Size(135, 20);
            this.mapFlagSafe_.CheckedChanged += delegate { this.ApplyMapLayerFlags(); };
            this.mapLayerQuickPanel_.Controls.Add(this.mapFlagSafe_);

            this.mapFlagIndoors_ = new CheckBox();
            this.mapFlagIndoors_.Text = "Indoors / Roof";
            this.mapFlagIndoors_.Location = new Point(10, 144);
            this.mapFlagIndoors_.Size = new Size(135, 20);
            this.mapFlagIndoors_.CheckedChanged += delegate { this.ApplyMapLayerFlags(); };
            this.mapLayerQuickPanel_.Controls.Add(this.mapFlagIndoors_);

            this.mapFlagSwim_ = new CheckBox();
            this.mapFlagSwim_.Text = "Water / Swim";
            this.mapFlagSwim_.Location = new Point(148, 144);
            this.mapFlagSwim_.Size = new Size(135, 20);
            this.mapFlagSwim_.CheckedChanged += delegate { this.ApplyMapLayerFlags(); };
            this.mapLayerQuickPanel_.Controls.Add(this.mapFlagSwim_);

            Button saveMapQuickBtn = this.CreateHeaderButton("💾 Save Map Changes", Color.FromArgb(22, 163, 74), Color.White, 268, delegate
            {
                if (this.server_ != null) this.server_.Save();
            });
            saveMapQuickBtn.Location = new Point(10, 166);
            saveMapQuickBtn.Height = 24;
            this.mapLayerQuickPanel_.Controls.Add(saveMapQuickBtn);

            this.mapInspectorGrid_ = new PropertyGrid();
            this.mapInspectorGrid_.Dock = DockStyle.Fill;
            StudioTheme.StylePropertyGrid(this.mapInspectorGrid_);
            this.mapInspectorGrid_.PropertyValueChanged += delegate
            {
                this.mapCanvas_.Invalidate();
                this.RefreshMapObjectsList();
            };

            rightInspector.Controls.Add(this.mapInspectorGrid_);
            rightInspector.Controls.Add(this.mapLayerQuickPanel_);

            // Center Interactive AreaCanvas
            this.mapCanvas_ = new AreaCanvas();
            this.mapCanvas_.Dock = DockStyle.Fill;
            this.mapCanvas_.Editable = true;
            this.mapCanvas_.ViewType = AreaCanvas.View.Design1;
            this.mapCanvas_.SelectedEvent += delegate(object o)
            {
                this.BindMapInspectorObject(o);
            };
            this.mapCanvas_.DeselectedEvent += delegate(object o)
            {
                this.BindMapInspectorObject(this.mapCanvas_.Area);
            };
            this.mapCanvas_.AreaSelectedEvent += delegate(Area a)
            {
                this.BindMapInspectorObject(a);
                this.RefreshMapObjectsList();
            };
            this.mapCanvas_.LocationClickedEvent += delegate(Point pt)
            {
                this.mapCanvas_.SelectedPoint = pt;
            };

            page.Controls.Add(this.mapCanvas_);
            page.Controls.Add(rightInspector);
            page.Controls.Add(leftPalette);
            return page;
        }

        private Button CreateMapToolButton(string text, int x, int y, int width, Color bg, EventHandler onClick)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(width, 28);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = ControlPaint.Light(bg, 0.2f);
            b.BackColor = bg;
            b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            if (onClick != null)
                b.Click += onClick;
            return b;
        }

        private void StartPaintingNewLayer(Area.Layer layer, string defaultName, char fillChar, Telnet.Color fillColor)
        {
            if (this.mapCanvas_.Area == null)
                return;
            layer.Name = defaultName;
            layer.FillChar = fillChar;
            layer.FillColor = fillColor;
            this.mapCanvas_.Edit(layer);
            this.mapCanvas_.Mode = AreaCanvas.EditModes.Manipulate;
            this.mapActiveToolLabel_.Text = "Painting: " + defaultName + " (Click Tiles)";
            this.RefreshMapObjectsList();
            this.BindMapInspectorObject(layer);
        }

        private void ApplyMapLayerFlags()
        {
            if (this.suppressMapQuickEvents_)
                return;
            Area.Layer layer = this.mapInspectorGrid_.SelectedObject as Area.Layer;
            if (layer != null)
            {
                layer.LayerFlags.Set((int)Area.Layer.LayerFlag.NoWalk, this.mapFlagNoWalk_.Checked);
                layer.LayerFlags.Set((int)Area.Layer.LayerFlag.NoAttack, this.mapFlagSafe_.Checked);
                layer.LayerFlags.Set((int)Area.Layer.LayerFlag.Indoors, this.mapFlagIndoors_.Checked);
                layer.LayerFlags.Set((int)Area.Layer.LayerFlag.Swim, this.mapFlagSwim_.Checked);
                this.mapCanvas_.Invalidate();
            }
        }

        private void BindMapInspectorObject(object obj)
        {
            if (obj == null && this.mapCanvas_ != null)
                obj = this.mapCanvas_.Area;

            this.mapInspectorGrid_.SelectedObject = obj;
            this.suppressMapQuickEvents_ = true;
            try
            {
                if (obj is Area.Layer)
                {
                    Area.Layer l = obj as Area.Layer;
                    this.mapObjNameBox_.Text = l.Name ?? string.Empty;
                    this.mapFillCharBox_.Text = l.FillChar.ToString();
                    this.mapFillColorCombo_.SelectedItem = l.FillColor;
                    this.mapFlagNoWalk_.Enabled = this.mapFlagSafe_.Enabled = this.mapFlagIndoors_.Enabled = this.mapFlagSwim_.Enabled = true;
                    this.mapFlagNoWalk_.Checked = l.LayerFlags.Get((int)Area.Layer.LayerFlag.NoWalk);
                    this.mapFlagSafe_.Checked = l.LayerFlags.Get((int)Area.Layer.LayerFlag.NoAttack);
                    this.mapFlagIndoors_.Checked = l.LayerFlags.Get((int)Area.Layer.LayerFlag.Indoors);
                    this.mapFlagSwim_.Checked = l.LayerFlags.Get((int)Area.Layer.LayerFlag.Swim);
                }
                else
                {
                    Object o = obj as Object;
                    this.mapObjNameBox_.Text = (o != null ? o.Name : (obj != null ? obj.ToString() : string.Empty));
                    this.mapFlagNoWalk_.Enabled = this.mapFlagSafe_.Enabled = this.mapFlagIndoors_.Enabled = this.mapFlagSwim_.Enabled = false;
                }
            }
            finally
            {
                this.suppressMapQuickEvents_ = false;
            }
        }

        private void RefreshMapPainterPage()
        {
            if (this.server_ == null || this.mapCanvas_ == null)
                return;
            if (this.mapCanvas_.Area == null && this.server_.MainArea != null)
                this.mapCanvas_.SetArea(this.server_.MainArea);

            this.RefreshMapObjectsList();
            if (this.mapInspectorGrid_.SelectedObject == null)
                this.BindMapInspectorObject(this.mapCanvas_.Area);
        }

        private void RefreshMapObjectsList()
        {
            if (this.mapLayersList_ == null || this.mapCanvas_ == null || this.mapCanvas_.Area == null)
                return;

            this.mapLayersList_.BeginUpdate();
            this.mapLayersList_.Items.Clear();
            this.mapLayersList_.Items.Add(this.mapCanvas_.Area);
            foreach (Object child in Damdai.Collections.Util.CopyList<Object>(this.mapCanvas_.Area.Children))
            {
                if (child != null)
                    this.mapLayersList_.Items.Add(child);
            }
            foreach (Reset r in Damdai.Collections.Util.CopyList<Reset>(this.mapCanvas_.Area.Resets))
            {
                if (r != null)
                    this.mapLayersList_.Items.Add(r);
            }
            this.mapLayersList_.EndUpdate();
        }

        #endregion

        #region Pages 3, 4, 5: Visual Blueprint Studios (Items, NPCs, Abilities)

        private Control BuildBlueprintForgePage(Type blueprintType, string headerTitle, string headerSubtitle)
        {
            Panel page = new Panel();
            page.BackColor = Color.FromArgb(241, 245, 249);
            page.Padding = new Padding(16);

            // Left Browser Panel
            Panel leftBrowser = new Panel();
            leftBrowser.Dock = DockStyle.Left;
            leftBrowser.Width = 285;
            leftBrowser.BackColor = Color.White;
            leftBrowser.Padding = new Padding(12);
            leftBrowser.Paint += delegate(object s, PaintEventArgs pe)
            {
                using (Pen border = new Pen(Color.FromArgb(203, 213, 225)))
                    pe.Graphics.DrawRectangle(border, 0, 0, leftBrowser.Width - 1, leftBrowser.Height - 1);
            };

            Label browserTitle = new Label();
            browserTitle.Text = blueprintType.Name.ToUpperInvariant() + " BLUEPRINTS";
            browserTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            browserTitle.ForeColor = Color.FromArgb(71, 85, 105);
            browserTitle.Location = new Point(12, 10);
            browserTitle.AutoSize = true;
            leftBrowser.Controls.Add(browserTitle);

            TextBox searchBox = new TextBox();
            searchBox.Location = new Point(12, 32);
            searchBox.Size = new Size(260, 24);
            searchBox.Font = new Font("Segoe UI", 9F);
            leftBrowser.Controls.Add(searchBox);

            ListBox itemList = new ListBox();
            itemList.Location = new Point(12, 98);
            itemList.Size = new Size(260, 480);
            itemList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemList.BorderStyle = BorderStyle.FixedSingle;
            itemList.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            itemList.ItemHeight = 24;
            leftBrowser.Controls.Add(itemList);

            // Right Designer Host
            Panel rightHost = new Panel();
            rightHost.Dock = DockStyle.Fill;
            rightHost.Padding = new Padding(14, 0, 0, 0);

            Panel rightCard = new Panel();
            rightCard.Dock = DockStyle.Fill;
            rightCard.BackColor = Color.White;
            rightCard.Padding = new Padding(18);
            rightCard.Paint += delegate(object s, PaintEventArgs pe)
            {
                using (Pen border = new Pen(Color.FromArgb(203, 213, 225)))
                    pe.Graphics.DrawRectangle(border, 0, 0, rightCard.Width - 1, rightCard.Height - 1);
            };

            // Top Visual Quick-Designer Panel
            Panel visualPanel = new Panel();
            visualPanel.Dock = DockStyle.Top;
            visualPanel.Height = 200;
            visualPanel.BackColor = Color.FromArgb(248, 250, 252);
            visualPanel.Padding = new Padding(16);
            visualPanel.Paint += delegate(object s, PaintEventArgs pe)
            {
                using (Pen border = new Pen(Color.FromArgb(226, 232, 240)))
                    pe.Graphics.DrawRectangle(border, 0, 0, visualPanel.Width - 1, visualPanel.Height - 1);
            };

            Label hTitle = new Label();
            hTitle.Text = headerTitle;
            hTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            hTitle.ForeColor = Color.FromArgb(15, 23, 42);
            hTitle.Location = new Point(14, 10);
            hTitle.AutoSize = true;
            visualPanel.Controls.Add(hTitle);

            Label hSub = new Label();
            hSub.Text = headerSubtitle;
            hSub.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            hSub.ForeColor = Color.FromArgb(100, 116, 139);
            hSub.Location = new Point(16, 34);
            hSub.AutoSize = true;
            visualPanel.Controls.Add(hSub);

            // Dynamic Visual Form Controls
            Label nameLbl = new Label();
            nameLbl.Text = "Name:";
            nameLbl.Font = StudioTheme.FontBold;
            nameLbl.Location = new Point(16, 66);
            nameLbl.AutoSize = true;
            visualPanel.Controls.Add(nameLbl);

            TextBox nameBox = new TextBox();
            nameBox.Location = new Point(105, 63);
            nameBox.Size = new Size(240, 24);
            nameBox.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            visualPanel.Controls.Add(nameBox);

            PropertyGrid propGrid = new PropertyGrid();
            propGrid.Dock = DockStyle.Fill;
            StudioTheme.StylePropertyGrid(propGrid);

            bool suppressVisual = false;
            Action bindSelected = null;

            // Add Type-Specific Visual Controls inside visualPanel
            if (blueprintType == typeof(Item))
            {
                Label slotLbl = new Label();
                slotLbl.Text = "Equip Slot:";
                slotLbl.Font = StudioTheme.FontBold;
                slotLbl.Location = new Point(365, 66);
                slotLbl.AutoSize = true;
                visualPanel.Controls.Add(slotLbl);

                ComboBox slotCombo = new ComboBox();
                slotCombo.Location = new Point(445, 63);
                slotCombo.Size = new Size(165, 24);
                slotCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                foreach (Character.BodyParts bp in Enum.GetValues(typeof(Character.BodyParts)))
                    slotCombo.Items.Add(bp);
                visualPanel.Controls.Add(slotCombo);

                Label valLbl = new Label();
                valLbl.Text = "Gold Value:";
                valLbl.Font = StudioTheme.FontBold;
                valLbl.Location = new Point(16, 102);
                valLbl.AutoSize = true;
                visualPanel.Controls.Add(valLbl);

                NumericUpDown valNum = new NumericUpDown();
                valNum.Location = new Point(105, 99);
                valNum.Size = new Size(105, 24);
                valNum.Maximum = 10000000;
                valNum.Minimum = 0;
                visualPanel.Controls.Add(valNum);

                Label descLbl = new Label();
                descLbl.Text = "Look Desc:";
                descLbl.Font = StudioTheme.FontBold;
                descLbl.Location = new Point(225, 102);
                descLbl.AutoSize = true;
                visualPanel.Controls.Add(descLbl);

                TextBox descBox = new TextBox();
                descBox.Location = new Point(305, 99);
                descBox.Size = new Size(305, 24);
                visualPanel.Controls.Add(descBox);

                CheckBox chkContainer = new CheckBox();
                chkContainer.Text = "Container (Holds Items)";
                chkContainer.Location = new Point(16, 138);
                chkContainer.AutoSize = true;
                visualPanel.Controls.Add(chkContainer);

                CheckBox chkEdible = new CheckBox();
                chkEdible.Text = "Consumable / Potion (Edible)";
                chkEdible.Location = new Point(190, 138);
                chkEdible.AutoSize = true;
                visualPanel.Controls.Add(chkEdible);

                CheckBox chkImmovable = new CheckBox();
                chkImmovable.Text = "Fixed Fixture (Cannot Pick Up)";
                chkImmovable.Location = new Point(400, 138);
                chkImmovable.AutoSize = true;
                visualPanel.Controls.Add(chkImmovable);

                slotCombo.SelectedIndexChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Item)) return;
                    ((Item)propGrid.SelectedObject).EquipLocation = (Character.BodyParts)slotCombo.SelectedItem;
                    propGrid.Refresh();
                };
                valNum.ValueChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Item)) return;
                    ((Item)propGrid.SelectedObject).Value = (int)valNum.Value;
                    propGrid.Refresh();
                };
                descBox.TextChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Item)) return;
                    ((Item)propGrid.SelectedObject).Description = descBox.Text;
                };
                EventHandler flagChanged = delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Item)) return;
                    Item it = (Item)propGrid.SelectedObject;
                    it.ObjectFlags.Set((int)Object.ObjFlags.Container, chkContainer.Checked);
                    it.ObjectFlags.Set((int)Object.ObjFlags.Edible, chkEdible.Checked);
                    it.ObjectFlags.Set((int)Object.ObjFlags.Immovable, chkImmovable.Checked);
                    propGrid.Refresh();
                };
                chkContainer.CheckedChanged += flagChanged;
                chkEdible.CheckedChanged += flagChanged;
                chkImmovable.CheckedChanged += flagChanged;

                bindSelected = delegate
                {
                    Item it = propGrid.SelectedObject as Item;
                    if (it == null) return;
                    slotCombo.SelectedItem = it.EquipLocation;
                    valNum.Value = System.Math.Max(0, System.Math.Min(10000000, it.Value));
                    descBox.Text = it.Description ?? string.Empty;
                    chkContainer.Checked = it.ObjectFlags.Get((int)Object.ObjFlags.Container);
                    chkEdible.Checked = it.ObjectFlags.Get((int)Object.ObjFlags.Edible);
                    chkImmovable.Checked = it.ObjectFlags.Get((int)Object.ObjFlags.Immovable);
                };
            }
            else if (blueprintType == typeof(Npc))
            {
                Label goldLbl = new Label();
                goldLbl.Text = "Gold Dropped:";
                goldLbl.Font = StudioTheme.FontBold;
                goldLbl.Location = new Point(365, 66);
                goldLbl.AutoSize = true;
                visualPanel.Controls.Add(goldLbl);

                NumericUpDown goldNum = new NumericUpDown();
                goldNum.Location = new Point(465, 63);
                goldNum.Size = new Size(120, 24);
                goldNum.Maximum = 10000000;
                visualPanel.Controls.Add(goldNum);

                Label descLbl = new Label();
                descLbl.Text = "Room Desc:";
                descLbl.Font = StudioTheme.FontBold;
                descLbl.Location = new Point(16, 102);
                descLbl.AutoSize = true;
                visualPanel.Controls.Add(descLbl);

                TextBox descBox = new TextBox();
                descBox.Location = new Point(105, 99);
                descBox.Size = new Size(480, 24);
                visualPanel.Controls.Add(descBox);

                CheckBox chkAggro = new CheckBox();
                chkAggro.Text = "Aggressive (Attacks Players on Sight)";
                chkAggro.Location = new Point(16, 138);
                chkAggro.AutoSize = true;
                visualPanel.Controls.Add(chkAggro);

                CheckBox chkWander = new CheckBox();
                chkWander.Text = "Wanders / Patrols Area";
                chkWander.Location = new Point(265, 138);
                chkWander.AutoSize = true;
                visualPanel.Controls.Add(chkWander);

                CheckBox chkAttackable = new CheckBox();
                chkAttackable.Text = "Attackable (Mortal)";
                chkAttackable.Location = new Point(445, 138);
                chkAttackable.AutoSize = true;
                visualPanel.Controls.Add(chkAttackable);

                goldNum.ValueChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Npc)) return;
                    ((Npc)propGrid.SelectedObject).Value = (int)goldNum.Value;
                    propGrid.Refresh();
                };
                descBox.TextChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Npc)) return;
                    ((Npc)propGrid.SelectedObject).Description = descBox.Text;
                };
                EventHandler npcFlagsChanged = delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Npc)) return;
                    Npc n = (Npc)propGrid.SelectedObject;
                    n.NpcFlags.Set((int)Npc.Flags.Aggressive, chkAggro.Checked);
                    n.NpcFlags.Set((int)Npc.Flags.Wander, chkWander.Checked);
                    n.ObjectFlags.Set((int)Object.ObjFlags.Unattackable, !chkAttackable.Checked);
                    propGrid.Refresh();
                };
                chkAggro.CheckedChanged += npcFlagsChanged;
                chkWander.CheckedChanged += npcFlagsChanged;
                chkAttackable.CheckedChanged += npcFlagsChanged;

                bindSelected = delegate
                {
                    Npc n = propGrid.SelectedObject as Npc;
                    if (n == null) return;
                    goldNum.Value = System.Math.Max(0, System.Math.Min(10000000, n.Value));
                    descBox.Text = n.Description ?? string.Empty;
                    chkAggro.Checked = n.NpcFlags.Get((int)Npc.Flags.Aggressive);
                    chkWander.Checked = n.NpcFlags.Get((int)Npc.Flags.Wander);
                    chkAttackable.Checked = !n.ObjectFlags.Get((int)Object.ObjFlags.Unattackable);
                };
            }
            else if (blueprintType == typeof(Ability))
            {
                Label styleLbl = new Label();
                styleLbl.Text = "Style:";
                styleLbl.Font = StudioTheme.FontBold;
                styleLbl.Location = new Point(365, 66);
                styleLbl.AutoSize = true;
                visualPanel.Controls.Add(styleLbl);

                ComboBox styleCombo = new ComboBox();
                styleCombo.Location = new Point(415, 63);
                styleCombo.Size = new Size(140, 24);
                styleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                foreach (Ability.Styles st in Enum.GetValues(typeof(Ability.Styles)))
                    styleCombo.Items.Add(st);
                visualPanel.Controls.Add(styleCombo);

                Label targLbl = new Label();
                targLbl.Text = "Targets:";
                targLbl.Font = StudioTheme.FontBold;
                targLbl.Location = new Point(16, 102);
                targLbl.AutoSize = true;
                visualPanel.Controls.Add(targLbl);

                ComboBox targCombo = new ComboBox();
                targCombo.Location = new Point(105, 99);
                targCombo.Size = new Size(125, 24);
                targCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                foreach (Ability.Targets tg in Enum.GetValues(typeof(Ability.Targets)))
                    targCombo.Items.Add(tg);
                visualPanel.Controls.Add(targCombo);

                Label rngLbl = new Label();
                rngLbl.Text = "Max Range:";
                rngLbl.Font = StudioTheme.FontBold;
                rngLbl.Location = new Point(250, 102);
                rngLbl.AutoSize = true;
                visualPanel.Controls.Add(rngLbl);

                NumericUpDown rngNum = new NumericUpDown();
                rngNum.Location = new Point(335, 99);
                rngNum.Size = new Size(75, 24);
                rngNum.Minimum = -1;
                rngNum.Maximum = 100;
                visualPanel.Controls.Add(rngNum);

                CheckBox chkAuto = new CheckBox();
                chkAuto.Text = "Auto-Repeat in Combat";
                chkAuto.Location = new Point(16, 138);
                chkAuto.AutoSize = true;
                visualPanel.Controls.Add(chkAuto);

                CheckBox chkInterrupt = new CheckBox();
                chkInterrupt.Text = "Can Be Interrupted";
                chkInterrupt.Location = new Point(210, 138);
                chkInterrupt.AutoSize = true;
                visualPanel.Controls.Add(chkInterrupt);

                styleCombo.SelectedIndexChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Ability)) return;
                    ((Ability)propGrid.SelectedObject).Style = (Ability.Styles)styleCombo.SelectedItem;
                    propGrid.Refresh();
                };
                targCombo.SelectedIndexChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Ability)) return;
                    ((Ability)propGrid.SelectedObject).Target = (Ability.Targets)targCombo.SelectedItem;
                    propGrid.Refresh();
                };
                rngNum.ValueChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Ability)) return;
                    ((Ability)propGrid.SelectedObject).RangeMax = (int)rngNum.Value;
                    propGrid.Refresh();
                };
                chkAuto.CheckedChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Ability)) return;
                    ((Ability)propGrid.SelectedObject).Auto = chkAuto.Checked;
                    propGrid.Refresh();
                };
                chkInterrupt.CheckedChanged += delegate
                {
                    if (suppressVisual || !(propGrid.SelectedObject is Ability)) return;
                    ((Ability)propGrid.SelectedObject).CanInterrupt = chkInterrupt.Checked;
                    propGrid.Refresh();
                };

                bindSelected = delegate
                {
                    Ability ab = propGrid.SelectedObject as Ability;
                    if (ab == null) return;
                    styleCombo.SelectedItem = ab.Style;
                    targCombo.SelectedItem = ab.Target;
                    rngNum.Value = System.Math.Max(-1, System.Math.Min(100, ab.RangeMax));
                    chkAuto.Checked = ab.Auto;
                    chkInterrupt.Checked = ab.CanInterrupt;
                };
            }

            Action refreshList = delegate
            {
                if (this.server_ == null || !this.server_.DataSets.ContainsKey(blueprintType))
                    return;
                string q = (searchBox.Text ?? string.Empty).Trim();
                object prev = itemList.SelectedItem;
                itemList.BeginUpdate();
                itemList.Items.Clear();
                List<IIdentifiable> all = new List<IIdentifiable>(this.server_.DataSets[blueprintType].Values);
                all.Sort(delegate(IIdentifiable a, IIdentifiable b) { return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase); });
                foreach (IIdentifiable obj in all)
                {
                    if (string.IsNullOrEmpty(q) || (obj.Name != null && obj.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                        itemList.Items.Add(obj);
                }
                itemList.EndUpdate();
                if (prev != null && itemList.Items.Contains(prev))
                    itemList.SelectedItem = prev;
                else if (itemList.Items.Count > 0)
                    itemList.SelectedIndex = 0;
            };

            // Add + New, Duplicate, Delete Buttons on Left Browser
            Button btnNew = this.CreateHeaderButton("+ New", Color.FromArgb(37, 99, 235), Color.White, 84, delegate
            {
                if (this.server_ == null) return;
                IIdentifiable created = (IIdentifiable)Activator.CreateInstance(blueprintType);
                created.Name = "New " + blueprintType.Name;
                this.server_.Add(created);
                this.server_.Save();
                refreshList();
                itemList.SelectedItem = created;
            });
            btnNew.Location = new Point(12, 62);
            btnNew.Height = 28;
            leftBrowser.Controls.Add(btnNew);

            Button btnClone = this.CreateHeaderButton("📋 Clone", Color.FromArgb(71, 85, 105), Color.White, 84, delegate
            {
                IIdentifiable sel = itemList.SelectedItem as IIdentifiable;
                if (sel == null || this.server_ == null) return;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bf.Binder = new Server.SerializationBinder();
                    bf.Serialize(ms, sel);
                    ms.Position = 0;
                    IIdentifiable copy = bf.Deserialize(ms) as IIdentifiable;
                    if (copy != null)
                    {
                        copy.Id = this.server_.NewGuid();
                        copy.Name = (sel.Name ?? blueprintType.Name) + " Copy";
                        this.server_.DataSets[blueprintType][copy.Id] = copy;
                        this.server_.Save();
                        refreshList();
                        itemList.SelectedItem = copy;
                    }
                }
            });
            btnClone.Location = new Point(100, 62);
            btnClone.Height = 28;
            leftBrowser.Controls.Add(btnClone);

            Button btnDel = this.CreateHeaderButton("🗑️ Delete", Color.FromArgb(220, 38, 38), Color.White, 84, delegate
            {
                IIdentifiable sel = itemList.SelectedItem as IIdentifiable;
                if (sel == null || this.server_ == null) return;
                if (MessageBox.Show(this, "Delete '" + sel.Name + "'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.server_.DataSets[blueprintType].Remove(sel.Id);
                    this.server_.Save();
                    refreshList();
                }
            });
            btnDel.Location = new Point(188, 62);
            btnDel.Height = 28;
            leftBrowser.Controls.Add(btnDel);

            searchBox.TextChanged += delegate { refreshList(); };

            itemList.SelectedIndexChanged += delegate
            {
                IIdentifiable sel = itemList.SelectedItem as IIdentifiable;
                propGrid.SelectedObject = sel;
                suppressVisual = true;
                try
                {
                    nameBox.Text = sel != null ? (sel.Name ?? string.Empty) : string.Empty;
                    if (bindSelected != null)
                        bindSelected();
                }
                finally
                {
                    suppressVisual = false;
                }
            };

            nameBox.TextChanged += delegate
            {
                if (suppressVisual) return;
                IIdentifiable sel = propGrid.SelectedObject as IIdentifiable;
                if (sel != null)
                {
                    sel.Name = nameBox.Text;
                    propGrid.Refresh();
                }
            };

            propGrid.PropertyValueChanged += delegate
            {
                if (this.server_ != null)
                    this.server_.Save();
            };

            rightCard.Controls.Add(propGrid);
            rightCard.Controls.Add(visualPanel);
            rightHost.Controls.Add(rightCard);

            page.Controls.Add(rightHost);
            page.Controls.Add(leftBrowser);
            page.Tag = refreshList;
            return page;
        }

        #endregion

        #region Page 6: Races, Classes (Jobs), Stat Blueprints & Status Effects Studio

        private Control BuildRacesClassesStatsPage()
        {
            Panel page = new Panel();
            page.BackColor = Color.FromArgb(241, 245, 249);
            page.Padding = new Padding(16);

            // Sub-Navigation Bar at Top for switching between Races, Classes, Stats, and Status Effects
            Panel subBar = new Panel();
            subBar.Dock = DockStyle.Top;
            subBar.Height = 46;
            subBar.BackColor = Color.FromArgb(15, 23, 42);
            subBar.Padding = new Padding(10, 7, 10, 7);

            FlowLayoutPanel subFlow = new FlowLayoutPanel();
            subFlow.Dock = DockStyle.Fill;
            subFlow.WrapContents = false;

            ListBox itemList = new ListBox();
            PropertyGrid propGrid = new PropertyGrid();
            StudioTheme.StylePropertyGrid(propGrid);
            Label catHeaderLbl = new Label();

            Action refreshList = delegate
            {
                if (this.server_ == null || !this.server_.DataSets.ContainsKey(this.currentRpgSubType_))
                    return;
                catHeaderLbl.Text = "Editing: " + this.currentRpgSubType_.Name + " Blueprints";
                itemList.BeginUpdate();
                itemList.Items.Clear();
                List<IIdentifiable> all = new List<IIdentifiable>(this.server_.DataSets[this.currentRpgSubType_].Values);
                all.Sort(delegate(IIdentifiable a, IIdentifiable b) { return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase); });
                foreach (IIdentifiable o in all)
                    itemList.Items.Add(o);
                itemList.EndUpdate();
                if (itemList.Items.Count > 0)
                    itemList.SelectedIndex = 0;
                else
                    propGrid.SelectedObject = null;
            };

            subFlow.Controls.Add(this.CreateHeaderButton("🛡️ Playable Races", Color.FromArgb(37, 99, 235), Color.White, 145, delegate
            {
                this.currentRpgSubType_ = typeof(Race);
                refreshList();
            }));
            subFlow.Controls.Add(this.CreateHeaderButton("🧙 Classes / Jobs", Color.FromArgb(124, 58, 237), Color.White, 145, delegate
            {
                this.currentRpgSubType_ = typeof(Job);
                refreshList();
            }));
            subFlow.Controls.Add(this.CreateHeaderButton("📊 Stat Blueprints (HP/MP/STR)", Color.FromArgb(13, 148, 136), Color.White, 210, delegate
            {
                this.currentRpgSubType_ = typeof(StatBlueprint);
                refreshList();
            }));
            subFlow.Controls.Add(this.CreateHeaderButton("🧪 Status Effects (Buffs/DoTs)", Color.FromArgb(217, 119, 6), Color.White, 205, delegate
            {
                this.currentRpgSubType_ = typeof(StatusEffectBlueprint);
                refreshList();
            }));

            subBar.Controls.Add(subFlow);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 280;

            Panel leftCard = new Panel();
            leftCard.Dock = DockStyle.Fill;
            leftCard.BackColor = Color.White;
            leftCard.Padding = new Padding(12);

            catHeaderLbl.Text = "Editing: Race Blueprints";
            catHeaderLbl.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            catHeaderLbl.ForeColor = Color.FromArgb(15, 23, 42);
            catHeaderLbl.Location = new Point(12, 10);
            catHeaderLbl.AutoSize = true;
            leftCard.Controls.Add(catHeaderLbl);

            Button addBtn = this.CreateHeaderButton("+ Create New", Color.FromArgb(22, 163, 74), Color.White, 122, delegate
            {
                if (this.server_ == null) return;
                IIdentifiable obj = (IIdentifiable)Activator.CreateInstance(this.currentRpgSubType_);
                obj.Name = "New " + this.currentRpgSubType_.Name;
                this.server_.Add(obj);
                this.server_.Save();
                refreshList();
                itemList.SelectedItem = obj;
            });
            addBtn.Location = new Point(12, 36);
            addBtn.Height = 28;
            leftCard.Controls.Add(addBtn);

            Button delBtn = this.CreateHeaderButton("🗑️ Delete", Color.FromArgb(220, 38, 38), Color.White, 110, delegate
            {
                IIdentifiable sel = itemList.SelectedItem as IIdentifiable;
                if (sel == null || this.server_ == null) return;
                if (sel is StatBlueprint && ((StatBlueprint)sel).IsPermanent)
                {
                    MessageBox.Show(this, "Permanent system stats cannot be deleted.", "Protected Stat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show(this, "Delete '" + sel.Name + "'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.server_.DataSets[this.currentRpgSubType_].Remove(sel.Id);
                    this.server_.Save();
                    refreshList();
                }
            });
            delBtn.Location = new Point(142, 36);
            delBtn.Height = 28;
            leftCard.Controls.Add(delBtn);

            itemList.Location = new Point(12, 72);
            itemList.Size = new Size(254, 480);
            itemList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemList.Font = new Font("Segoe UI", 9.5F);
            itemList.SelectedIndexChanged += delegate
            {
                propGrid.SelectedObject = itemList.SelectedItem;
            };
            leftCard.Controls.Add(itemList);

            propGrid.Dock = DockStyle.Fill;
            propGrid.PropertyValueChanged += delegate
            {
                if (this.server_ != null) this.server_.Save();
            };

            split.Panel1.Controls.Add(leftCard);
            split.Panel2.Controls.Add(propGrid);

            page.Controls.Add(split);
            page.Controls.Add(subBar);
            page.Tag = refreshList;
            return page;
        }

        #endregion

        #region Page 7: Inline Lua Code & Help IDE

        private Control BuildLuaCodeIdePage()
        {
            Panel page = new Panel();
            page.BackColor = Color.FromArgb(15, 23, 42);
            page.Padding = new Padding(12);

            // Left Explorer Sidebar for Commands, Scripts, and Help Files
            Panel leftPane = new Panel();
            leftPane.Dock = DockStyle.Left;
            leftPane.Width = 285;
            leftPane.BackColor = Color.FromArgb(30, 41, 59);
            leftPane.Padding = new Padding(10);

            Button tabCmds = this.CreateMapToolButton("📜 Commands", 8, 8, 86, Color.FromArgb(37, 99, 235), delegate
            {
                this.ideCategory_ = 0;
                this.RefreshLuaIdeList();
            });
            Button tabScripts = this.CreateMapToolButton("⚙️ Scripts", 98, 8, 86, Color.FromArgb(109, 40, 217), delegate
            {
                this.ideCategory_ = 1;
                this.RefreshLuaIdeList();
            });
            Button tabHelp = this.CreateMapToolButton("📖 Help", 188, 8, 86, Color.FromArgb(13, 148, 136), delegate
            {
                this.ideCategory_ = 2;
                this.RefreshLuaIdeList();
            });
            leftPane.Controls.Add(tabCmds);
            leftPane.Controls.Add(tabScripts);
            leftPane.Controls.Add(tabHelp);

            this.ideSearchBox_ = new TextBox();
            this.ideSearchBox_.Location = new Point(8, 44);
            this.ideSearchBox_.Size = new Size(266, 24);
            this.ideSearchBox_.BackColor = Color.FromArgb(15, 23, 42);
            this.ideSearchBox_.ForeColor = Color.White;
            this.ideSearchBox_.BorderStyle = BorderStyle.FixedSingle;
            this.ideSearchBox_.TextChanged += delegate { this.RefreshLuaIdeList(); };
            leftPane.Controls.Add(this.ideSearchBox_);

            Button btnNewCode = this.CreateMapToolButton("+ New", 8, 74, 86, Color.FromArgb(22, 163, 74), delegate
            {
                if (this.ideCategory_ == 0)
                {
                    Command c = new Command();
                    c.Name = "new_cmd";
                    c.Script = "-- Custom Lua Command\r\nuser:Send('Hello from " + c.Name + "!')\r\nreturn true";
                    Command.Commands.Add(c);
                    c.Save();
                    this.RefreshLuaIdeList();
                    this.ideItemList_.SelectedItem = c;
                }
                else if (this.ideCategory_ == 1 && this.server_ != null)
                {
                    Script s = new Script(Script.Types.Server, "NewScript", "-- Lua System Script\r\n");
                    this.server_.Add<Script>(s);
                    this.server_.Save();
                    this.RefreshLuaIdeList();
                    this.ideItemList_.SelectedItem = s;
                }
                else if (this.ideCategory_ == 2)
                {
                    Help h = new Help("NEW_TOPIC", "Write your help topic content here.");
                    Help.HelpFiles.Add(h);
                    h.Save();
                    this.RefreshLuaIdeList();
                    this.ideItemList_.SelectedItem = h;
                }
            });
            Button btnDelCode = this.CreateMapToolButton("🗑️ Delete", 100, 74, 86, Color.FromArgb(185, 28, 28), delegate
            {
                object sel = this.ideItemList_.SelectedItem;
                if (sel == null) return;
                if (MessageBox.Show(this, "Delete '" + sel.ToString() + "'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
                if (sel is Command)
                {
                    ((Command)sel).Delete();
                    Command.Commands.Remove((Command)sel);
                }
                else if (sel is Script && this.server_ != null)
                {
                    this.server_.DataSets[typeof(Script)].Remove(((Script)sel).Id);
                    this.server_.Save();
                }
                else if (sel is Help)
                {
                    ((Help)sel).Delete();
                    Help.HelpFiles.Remove((Help)sel);
                }
                this.RefreshLuaIdeList();
            });
            Button btnApiRef = this.CreateMapToolButton("📘 API Docs", 190, 74, 84, Color.FromArgb(51, 65, 85), delegate
            {
                Damdai.Forms.TextEditorWindow.ShowScriptingGuide(this.FindForm());
            });
            leftPane.Controls.Add(btnNewCode);
            leftPane.Controls.Add(btnDelCode);
            leftPane.Controls.Add(btnApiRef);

            this.ideItemList_ = new ListBox();
            this.ideItemList_.Location = new Point(8, 108);
            this.ideItemList_.Size = new Size(266, 490);
            this.ideItemList_.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.ideItemList_.BackColor = Color.FromArgb(15, 23, 42);
            this.ideItemList_.ForeColor = Color.FromArgb(226, 232, 240);
            this.ideItemList_.BorderStyle = BorderStyle.FixedSingle;
            this.ideItemList_.Font = new Font("Consolas", 9.5F, FontStyle.Regular);
            this.ideItemList_.SelectedIndexChanged += delegate
            {
                this.BindIdeObject(this.ideItemList_.SelectedItem);
            };
            leftPane.Controls.Add(this.ideItemList_);

            // Right Code Editor Workspace
            Panel rightIde = new Panel();
            rightIde.Dock = DockStyle.Fill;
            rightIde.Padding = new Padding(12, 0, 0, 0);

            Panel metaBar = new Panel();
            metaBar.Dock = DockStyle.Top;
            metaBar.Height = 76;
            metaBar.BackColor = Color.FromArgb(30, 41, 59);
            metaBar.Padding = new Padding(12, 8, 12, 8);

            Label lblName = new Label();
            lblName.Text = "Name:";
            lblName.ForeColor = Color.FromArgb(148, 163, 184);
            lblName.Font = StudioTheme.FontBold;
            lblName.Location = new Point(12, 12);
            lblName.AutoSize = true;
            metaBar.Controls.Add(lblName);

            this.ideNameBox_ = new TextBox();
            this.ideNameBox_.Location = new Point(66, 9);
            this.ideNameBox_.Size = new Size(165, 24);
            this.ideNameBox_.BackColor = Color.FromArgb(15, 23, 42);
            this.ideNameBox_.ForeColor = Color.White;
            this.ideNameBox_.BorderStyle = BorderStyle.FixedSingle;
            this.ideNameBox_.Font = new Font("Consolas", 10F, FontStyle.Bold);
            metaBar.Controls.Add(this.ideNameBox_);

            Label lblAlias = new Label();
            lblAlias.Text = "Aliases (comma):";
            lblAlias.ForeColor = Color.FromArgb(148, 163, 184);
            lblAlias.Font = StudioTheme.FontBold;
            lblAlias.Location = new Point(244, 12);
            lblAlias.AutoSize = true;
            metaBar.Controls.Add(lblAlias);

            this.ideAliasesBox_ = new TextBox();
            this.ideAliasesBox_.Location = new Point(352, 9);
            this.ideAliasesBox_.Size = new Size(140, 24);
            this.ideAliasesBox_.BackColor = Color.FromArgb(15, 23, 42);
            this.ideAliasesBox_.ForeColor = Color.White;
            this.ideAliasesBox_.BorderStyle = BorderStyle.FixedSingle;
            metaBar.Controls.Add(this.ideAliasesBox_);

            this.ideSystemCheck_ = new CheckBox();
            this.ideSystemCheck_.Text = "System Only";
            this.ideSystemCheck_.ForeColor = Color.FromArgb(226, 232, 240);
            this.ideSystemCheck_.Location = new Point(505, 11);
            this.ideSystemCheck_.AutoSize = true;
            metaBar.Controls.Add(this.ideSystemCheck_);

            this.ideHiddenCheck_ = new CheckBox();
            this.ideHiddenCheck_.Text = "Hidden";
            this.ideHiddenCheck_.ForeColor = Color.FromArgb(226, 232, 240);
            this.ideHiddenCheck_.Location = new Point(605, 11);
            this.ideHiddenCheck_.AutoSize = true;
            metaBar.Controls.Add(this.ideHiddenCheck_);

            Button checkSyntaxBtn = this.CreateHeaderButton("⚡ Check Lua Syntax", Color.FromArgb(14, 116, 144), Color.White, 148, delegate
            {
                this.ValidateCurrentIdeLua();
            });
            checkSyntaxBtn.Location = new Point(12, 40);
            checkSyntaxBtn.Height = 28;
            metaBar.Controls.Add(checkSyntaxBtn);

            Button saveCodeBtn = this.CreateHeaderButton("💾 Save Script / Topic", Color.FromArgb(22, 163, 74), Color.White, 160, delegate
            {
                this.SaveCurrentIdeObject();
            });
            saveCodeBtn.Location = new Point(168, 40);
            saveCodeBtn.Height = 28;
            metaBar.Controls.Add(saveCodeBtn);

            this.ideSyntaxStatusLbl_ = new Label();
            this.ideSyntaxStatusLbl_.Text = "Ready — Edit Lua code or Help text below.";
            this.ideSyntaxStatusLbl_.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
            this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(147, 197, 253);
            this.ideSyntaxStatusLbl_.Location = new Point(340, 46);
            this.ideSyntaxStatusLbl_.AutoSize = true;
            metaBar.Controls.Add(this.ideSyntaxStatusLbl_);

            this.ideCodeBox_ = new RichTextBox();
            this.ideCodeBox_.Dock = DockStyle.Fill;
            this.ideCodeBox_.BackColor = Color.FromArgb(11, 15, 25);
            this.ideCodeBox_.ForeColor = Color.FromArgb(226, 232, 240);
            this.ideCodeBox_.Font = new Font("Consolas", 10.5F, FontStyle.Regular);
            this.ideCodeBox_.BorderStyle = BorderStyle.None;
            this.ideCodeBox_.AcceptsTab = true;
            this.ideCodeBox_.WordWrap = false;

            rightIde.Controls.Add(this.ideCodeBox_);
            rightIde.Controls.Add(metaBar);

            page.Controls.Add(rightIde);
            page.Controls.Add(leftPane);
            return page;
        }

        private void RefreshLuaIdeList()
        {
            if (this.ideItemList_ == null)
                return;

            string q = (this.ideSearchBox_ != null ? this.ideSearchBox_.Text : string.Empty).Trim();
            object prev = this.ideItemList_.SelectedItem;

            this.ideItemList_.BeginUpdate();
            this.ideItemList_.Items.Clear();

            if (this.ideCategory_ == 0 && Command.Commands != null)
            {
                List<Command> cmds = new List<Command>(Command.Commands);
                cmds.Sort(delegate(Command a, Command b) { return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase); });
                foreach (Command c in cmds)
                {
                    if (string.IsNullOrEmpty(q) || (c.Name != null && c.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                        this.ideItemList_.Items.Add(c);
                }
            }
            else if (this.ideCategory_ == 1 && this.server_ != null && this.server_.DataSets.ContainsKey(typeof(Script)))
            {
                List<IIdentifiable> scripts = new List<IIdentifiable>(this.server_.DataSets[typeof(Script)].Values);
                scripts.Sort(delegate(IIdentifiable a, IIdentifiable b) { return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase); });
                foreach (Script s in scripts)
                {
                    if (string.IsNullOrEmpty(q) || (s.Name != null && s.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                        this.ideItemList_.Items.Add(s);
                }
            }
            else if (this.ideCategory_ == 2 && Help.HelpFiles != null)
            {
                List<Help> helps = new List<Help>(Help.HelpFiles);
                helps.Sort(delegate(Help a, Help b) { return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase); });
                foreach (Help h in helps)
                {
                    if (string.IsNullOrEmpty(q) || (h.Name != null && h.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                        this.ideItemList_.Items.Add(h);
                }
            }

            this.ideItemList_.EndUpdate();

            if (prev != null && this.ideItemList_.Items.Contains(prev))
                this.ideItemList_.SelectedItem = prev;
            else if (this.ideItemList_.Items.Count > 0)
                this.ideItemList_.SelectedIndex = 0;
        }

        private void BindIdeObject(object obj)
        {
            this.currentIdeObject_ = obj;
            if (obj is Command)
            {
                Command c = obj as Command;
                this.ideNameBox_.Text = c.Name ?? string.Empty;
                this.ideAliasesBox_.Enabled = true;
                this.ideAliasesBox_.Text = (c.Aliases != null) ? string.Join(", ", c.Aliases.ToArray()) : string.Empty;
                this.ideSystemCheck_.Enabled = this.ideHiddenCheck_.Enabled = true;
                this.ideSystemCheck_.Checked = c.SystemCommand;
                this.ideHiddenCheck_.Checked = c.Hidden;
                this.ideCodeBox_.Text = c.Script ?? string.Empty;
                this.ideSyntaxStatusLbl_.Text = "Editing Lua Command: " + c.Name;
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(147, 197, 253);
            }
            else if (obj is Script)
            {
                Script s = obj as Script;
                this.ideNameBox_.Text = s.Name ?? string.Empty;
                this.ideAliasesBox_.Enabled = false;
                this.ideAliasesBox_.Text = s.ScriptType.ToString();
                this.ideSystemCheck_.Enabled = this.ideHiddenCheck_.Enabled = false;
                this.ideCodeBox_.Text = s.Code ?? string.Empty;
                this.ideSyntaxStatusLbl_.Text = "Editing System Script (" + s.ScriptType + "): " + s.Name;
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(147, 197, 253);
            }
            else if (obj is Help)
            {
                Help h = obj as Help;
                this.ideNameBox_.Text = h.Name ?? string.Empty;
                this.ideAliasesBox_.Enabled = false;
                this.ideAliasesBox_.Text = "Help File";
                this.ideSystemCheck_.Enabled = this.ideHiddenCheck_.Enabled = false;
                this.ideCodeBox_.Text = h.Text ?? string.Empty;
                this.ideSyntaxStatusLbl_.Text = "Editing Help Topic: " + h.Name;
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(147, 197, 253);
            }
        }

        private void ValidateCurrentIdeLua()
        {
            if (this.currentIdeObject_ is Help)
            {
                this.ideSyntaxStatusLbl_.Text = "✔ Help topic is plain text (no Lua syntax check needed).";
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(74, 222, 128);
                return;
            }
            string code = this.ideCodeBox_.Text ?? string.Empty;
            try
            {
                using (Lua lua = new Lua())
                {
                    lua.DoString("return function()\r\n" + code + "\r\nend");
                }
                this.ideSyntaxStatusLbl_.Text = "✔ Lua Syntax Valid! No compilation errors found.";
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(74, 222, 128);
            }
            catch (Exception ex)
            {
                this.ideSyntaxStatusLbl_.Text = "✖ Lua Error: " + ex.Message;
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(248, 113, 113);
            }
        }

        private void SaveCurrentIdeObject()
        {
            if (this.currentIdeObject_ == null)
                return;

            string cleanText = (this.ideCodeBox_.Text ?? string.Empty).Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");

            if (this.currentIdeObject_ is Command)
            {
                Command c = this.currentIdeObject_ as Command;
                if (!string.IsNullOrEmpty(this.ideNameBox_.Text.Trim()))
                    c.Name = this.ideNameBox_.Text.Trim();
                c.SystemCommand = this.ideSystemCheck_.Checked;
                c.Hidden = this.ideHiddenCheck_.Checked;
                c.Aliases.Clear();
                foreach (string part in this.ideAliasesBox_.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = part.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        c.Aliases.Add(trimmed);
                }
                c.Script = cleanText;
                c.Save();
                this.ideSyntaxStatusLbl_.Text = "✔ Saved Command '" + c.Name + ".cmd' (" + DateTime.Now.ToShortTimeString() + ")";
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(74, 222, 128);
            }
            else if (this.currentIdeObject_ is Script)
            {
                Script s = this.currentIdeObject_ as Script;
                if (!string.IsNullOrEmpty(this.ideNameBox_.Text.Trim()))
                    s.Name = this.ideNameBox_.Text.Trim();
                s.Code = cleanText;
                if (this.server_ != null)
                    this.server_.Save();
                this.ideSyntaxStatusLbl_.Text = "✔ Saved System Script '" + s.Name + "' (" + DateTime.Now.ToShortTimeString() + ")";
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(74, 222, 128);
            }
            else if (this.currentIdeObject_ is Help)
            {
                Help h = this.currentIdeObject_ as Help;
                if (!string.IsNullOrEmpty(this.ideNameBox_.Text.Trim()))
                    h.Name = this.ideNameBox_.Text.Trim();
                h.Text = cleanText;
                h.Save();
                this.ideSyntaxStatusLbl_.Text = "✔ Saved Help Topic '" + h.Name + ".help' (" + DateTime.Now.ToShortTimeString() + ")";
                this.ideSyntaxStatusLbl_.ForeColor = Color.FromArgb(74, 222, 128);
            }
        }

        #endregion

        #region Page 8: Live Server Console & Players Monitor

        private Control BuildServerConsolePage()
        {
            Panel page = new Panel();
            page.BackColor = Color.FromArgb(15, 23, 42);
            page.Padding = new Padding(16);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 680;

            // Left: Server Activity Log & Command Console
            Panel logCard = new Panel();
            logCard.Dock = DockStyle.Fill;
            logCard.BackColor = Color.FromArgb(30, 41, 59);
            logCard.Padding = new Padding(12);

            Label logHdr = new Label();
            logHdr.Text = "🖥️ LIVE SERVER ACTIVITY LOG & ADMIN CONSOLE";
            logHdr.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            logHdr.ForeColor = Color.FromArgb(147, 197, 253);
            logHdr.Dock = DockStyle.Top;
            logHdr.Height = 26;

            Panel inputBar = new Panel();
            inputBar.Dock = DockStyle.Bottom;
            inputBar.Height = 34;
            inputBar.Padding = new Padding(0, 4, 0, 0);

            this.liveConsoleInput_ = new TextBox();
            this.liveConsoleInput_.Dock = DockStyle.Fill;
            this.liveConsoleInput_.BackColor = Color.FromArgb(11, 15, 25);
            this.liveConsoleInput_.ForeColor = Color.White;
            this.liveConsoleInput_.Font = StudioTheme.FontMono;
            this.liveConsoleInput_.BorderStyle = BorderStyle.FixedSingle;

            Button runCmdBtn = this.CreateHeaderButton("Execute Command", Color.FromArgb(37, 99, 235), Color.White, 132, delegate
            {
                if (this.mainForm_ != null && !string.IsNullOrEmpty(this.liveConsoleInput_.Text.Trim()))
                {
                    string cmd = this.liveConsoleInput_.Text.Trim();
                    this.liveConsoleInput_.Clear();
                    this.mainForm_.ExecuteConsoleCommand(cmd);
                }
            });
            runCmdBtn.Dock = DockStyle.Right;
            this.liveConsoleInput_.KeyDown += delegate(object s, KeyEventArgs ke)
            {
                if (ke.KeyCode == Keys.Enter)
                {
                    ke.SuppressKeyPress = true;
                    runCmdBtn.PerformClick();
                }
            };

            inputBar.Controls.Add(this.liveConsoleInput_);
            inputBar.Controls.Add(runCmdBtn);

            this.liveLogBox_ = new RichTextBox();
            this.liveLogBox_.Dock = DockStyle.Fill;
            this.liveLogBox_.BackColor = Color.FromArgb(11, 15, 25);
            this.liveLogBox_.ForeColor = Color.FromArgb(226, 232, 240);
            this.liveLogBox_.Font = StudioTheme.FontMono;
            this.liveLogBox_.ReadOnly = true;
            this.liveLogBox_.BorderStyle = BorderStyle.None;
            this.liveLogBox_.Text = "UltiMUD Server Console Ready. Type 'help', 'status', 'users', 'broadcast <msg>', or 'save'.";

            logCard.Controls.Add(this.liveLogBox_);
            logCard.Controls.Add(inputBar);
            logCard.Controls.Add(logHdr);
            split.Panel1.Controls.Add(logCard);

            // Right: Connected Players & Playtester Quick-Launcher
            Panel playersCard = new Panel();
            playersCard.Dock = DockStyle.Fill;
            playersCard.BackColor = Color.FromArgb(30, 41, 59);
            playersCard.Padding = new Padding(12);

            Label pHdr = new Label();
            pHdr.Text = "👥 CONNECTED PLAYERS & PLAYTESTER";
            pHdr.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            pHdr.ForeColor = Color.FromArgb(74, 222, 128);
            pHdr.Location = new Point(12, 12);
            pHdr.AutoSize = true;
            playersCard.Controls.Add(pHdr);

            Button launchPlaytesterBtn = this.CreateHeaderButton("🎮 Open Built-In MUD Playtester Client", Color.FromArgb(22, 163, 74), Color.White, 260, delegate
            {
                if (this.mainForm_ != null)
                    TestClientForm.ShowPlaytester(this.mainForm_, this.mainForm_.ConfiguredPort);
            });
            launchPlaytesterBtn.Location = new Point(12, 40);
            playersCard.Controls.Add(launchPlaytesterBtn);

            Button grantAdminBtn = this.CreateHeaderButton("★ Toggle Admin on Selected Player", Color.FromArgb(217, 119, 6), Color.White, 260, delegate
            {
                User u = this.onlinePlayersList_.SelectedItem as User;
                if (u != null)
                {
                    bool isAdmin = !u.Flags.Get((int)User.UserFlags.Administrator);
                    u.Flags.Set((int)User.UserFlags.Administrator, isAdmin);
                    MessageBox.Show(this, u.ToString() + " Administrator = " + isAdmin, "Player Role Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
            grantAdminBtn.Location = new Point(12, 78);
            playersCard.Controls.Add(grantAdminBtn);

            this.onlinePlayersList_ = new ListBox();
            this.onlinePlayersList_.Location = new Point(12, 118);
            this.onlinePlayersList_.Size = new Size(260, 440);
            this.onlinePlayersList_.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.onlinePlayersList_.BackColor = Color.FromArgb(11, 15, 25);
            this.onlinePlayersList_.ForeColor = Color.White;
            this.onlinePlayersList_.BorderStyle = BorderStyle.FixedSingle;
            this.onlinePlayersList_.Font = new Font("Segoe UI", 9.5F);
            playersCard.Controls.Add(this.onlinePlayersList_);

            split.Panel2.Controls.Add(playersCard);
            page.Controls.Add(split);
            return page;
        }

        public void RefreshOnlinePlayersList()
        {
            if (this.onlinePlayersList_ == null || this.server_ == null || this.server_.Users == null)
                return;
            this.onlinePlayersList_.BeginUpdate();
            this.onlinePlayersList_.Items.Clear();
            foreach (User u in Damdai.Collections.Util.CopyList<User>(this.server_.Users))
            {
                if (u != null)
                    this.onlinePlayersList_.Items.Add(u);
            }
            this.onlinePlayersList_.EndUpdate();
        }

        #endregion
    }
}
