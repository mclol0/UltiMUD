using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using LuaInterface;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    internal class MudAuditorForm : Form
    {
        enum Severity
        {
            Pass,
            Warning,
            Error
        }

        class AuditEntry
        {
            public Severity Level;
            public string Category;
            public string TargetName;
            public string Message;
            public object TargetObject;
        }

        ListView listView_;
        Label summaryLabel_;
        List<AuditEntry> entries_ = new List<AuditEntry>();

        public MudAuditorForm()
        {
            this.Text = "UltiMUD World Readiness, Reference & Global Lua Script Auditor";
            this.Size = new Size(880, 560);
            this.MinimumSize = new Size(720, 440);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 66;
            headerPanel.BackColor = Color.FromArgb(20, 36, 64);
            headerPanel.Padding = new Padding(14, 10, 14, 10);

            Label titleLbl = new Label();
            titleLbl.Text = "MUD Health Check, Blueprint Reference & Lua Syntax Auditor";
            titleLbl.ForeColor = Color.White;
            titleLbl.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            titleLbl.AutoSize = true;
            titleLbl.Location = new Point(14, 10);

            this.summaryLabel_ = new Label();
            this.summaryLabel_.Text = "Scanning MUD...";
            this.summaryLabel_.ForeColor = Color.FromArgb(186, 210, 245);
            this.summaryLabel_.Font = new Font("Segoe UI", 9.2F, FontStyle.Regular);
            this.summaryLabel_.AutoSize = true;
            this.summaryLabel_.Location = new Point(14, 36);

            headerPanel.Controls.Add(titleLbl);
            headerPanel.Controls.Add(this.summaryLabel_);

            this.listView_ = new ListView();
            this.listView_.Dock = DockStyle.Fill;
            this.listView_.View = View.Details;
            this.listView_.FullRowSelect = true;
            this.listView_.GridLines = true;
            this.listView_.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.listView_.Columns.Add("Status", 85);
            this.listView_.Columns.Add("Category", 130);
            this.listView_.Columns.Add("Object / Script", 180);
            this.listView_.Columns.Add("Diagnostic Details (Double-click row to inspect object)", 440);
            this.listView_.DoubleClick += new EventHandler(this.listView__DoubleClick);

            Panel bottomBar = new Panel();
            bottomBar.Dock = DockStyle.Bottom;
            bottomBar.Height = 48;
            bottomBar.Padding = new Padding(12, 8, 12, 8);

            Button rescanBtn = new Button();
            rescanBtn.Text = "↻ Re-Scan MUD";
            rescanBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rescanBtn.Width = 130;
            rescanBtn.Height = 32;
            rescanBtn.Dock = DockStyle.Left;
            rescanBtn.Click += delegate { this.RunAudit(); };

            Button autoFixBtn = new Button();
            autoFixBtn.Text = "⚡ Auto-Fix Common Setup Issues";
            autoFixBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            autoFixBtn.BackColor = Color.FromArgb(37, 99, 235);
            autoFixBtn.ForeColor = Color.White;
            autoFixBtn.FlatStyle = FlatStyle.Flat;
            autoFixBtn.FlatAppearance.BorderSize = 0;
            autoFixBtn.Width = 235;
            autoFixBtn.Height = 32;
            autoFixBtn.Dock = DockStyle.Right;
            autoFixBtn.Cursor = Cursors.Hand;
            autoFixBtn.Click += new EventHandler(this.autoFixBtn__Click);

            Button openStarterBtn = new Button();
            openStarterBtn.Text = "✦ Open Starter Kit Wizard...";
            openStarterBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            openStarterBtn.Width = 195;
            openStarterBtn.Height = 32;
            openStarterBtn.Dock = DockStyle.Right;
            openStarterBtn.Click += delegate
            {
                using (MudStarterKitForm dlg = new MudStarterKitForm())
                {
                    dlg.ShowDialog(this);
                }
                this.RunAudit();
            };

            bottomBar.Controls.Add(this.listView_);
            this.Controls.Add(this.listView_);
            this.Controls.Add(bottomBar);
            this.Controls.Add(headerPanel);
            bottomBar.Controls.Add(rescanBtn);
            bottomBar.Controls.Add(openStarterBtn);
            bottomBar.Controls.Add(autoFixBtn);

            this.Shown += delegate { this.RunAudit(); };
        }

        void AddEntry(Severity level, string category, string targetName, string message, object targetObj)
        {
            AuditEntry e = new AuditEntry();
            e.Level = level;
            e.Category = category;
            e.TargetName = targetName;
            e.Message = message;
            e.TargetObject = targetObj;
            this.entries_.Add(e);
        }

        public void RunAudit()
        {
            this.entries_.Clear();
            Server srv = Server.Instance;
            if (srv == null)
            {
                this.AddEntry(Severity.Error, "Server", "Server.Instance", "No MUD server is currently loaded.", null);
                this.RenderEntries();
                return;
            }

            // 1. World Settings & Persistence Readiness
            if (srv.Settings == null || string.IsNullOrEmpty(srv.Settings.Name))
            {
                this.AddEntry(Severity.Error, "World Settings", "Settings.Name",
                    "MUD Name is empty! UltiMUD cannot save .mud files until Settings.Name is set.", srv.Settings);
            }
            else
            {
                this.AddEntry(Severity.Pass, "World Settings", srv.Settings.Name,
                    "MUD World Name is configured ('" + srv.Settings.Name + "').", srv.Settings);
            }

            // 2. Starting Area Check
            if (srv.MainArea == null)
            {
                this.AddEntry(Severity.Error, "World Map", "MainArea", "Server.MainArea is null.", srv);
            }
            else if (!srv.MainArea.AreaFlags.Get((int)Area.AreaFlag.StartingArea))
            {
                this.AddEntry(Severity.Warning, "World Map", srv.MainArea.Name,
                    "MainArea does not have the 'StartingArea' flag enabled; new characters may not have a default spawn area.", srv.MainArea);
            }
            else
            {
                this.AddEntry(Severity.Pass, "World Map", srv.MainArea.Name,
                    "StartingArea flag is enabled (Spawn point: " + srv.MainArea.Start + ").", srv.MainArea);
            }

            // 3. Fight System Check
            if (srv.FightSystem == null)
            {
                this.AddEntry(Severity.Warning, "Combat Engine", "Server.FightSystem",
                    "No FightSystem is selected on the Server object (AutomatedFightSystem, ManualFightSystem, or TurnBasedFightSystem).", srv);
            }
            else
            {
                this.AddEntry(Severity.Pass, "Combat Engine", srv.FightSystem.GetType().Name,
                    "Combat engine is configured (" + srv.FightSystem.GetType().Name + ").", srv);
            }

            // 4. Stat Blueprints Check
            int statCount = srv.DataSets.ContainsKey(typeof(StatBlueprint)) ? srv.DataSets[typeof(StatBlueprint)].Count : 0;
            if (statCount == 0)
            {
                this.AddEntry(Severity.Warning, "RPG Stats", "Stat Blueprints",
                    "0 Stat Blueprints defined. Use the Starter Kit Wizard to create hp, mp, mv, str, dex, int, etc.", srv);
            }
            else
            {
                this.AddEntry(Severity.Pass, "RPG Stats", "Stat Blueprints (" + statCount + ")",
                    statCount + " Stat Blueprint(s) configured.", srv);
            }

            // 5. Playable Races & Jobs Check
            int selectableRaces = 0;
            if (srv.DataSets.ContainsKey(typeof(Race)))
            {
                foreach (Race r in srv.DataSets[typeof(Race)].Values)
                {
                    if (r.Selectable) selectableRaces++;
                    if (r.StartItems != null)
                    {
                        foreach (Guid itemId in r.StartItems)
                        {
                            if (srv.Get<Item>(itemId) == null)
                                this.AddEntry(Severity.Warning, "Broken Reference", "Race: " + r.Name, "References a deleted StartItem (" + itemId + ").", r);
                        }
                    }
                }
            }
            if (selectableRaces == 0)
                this.AddEntry(Severity.Warning, "Character Creation", "Playable Races", "0 Selectable Races defined. New players may have no race options at login.", srv);
            else
                this.AddEntry(Severity.Pass, "Character Creation", "Playable Races (" + selectableRaces + ")", selectableRaces + " Selectable Race(s) ready for character creation.", srv);

            int selectableJobs = 0;
            if (srv.DataSets.ContainsKey(typeof(Job)))
            {
                foreach (Job j in srv.DataSets[typeof(Job)].Values)
                {
                    if (j.Selectable) selectableJobs++;
                    if (j.StartItems != null)
                    {
                        foreach (Guid itemId in j.StartItems)
                        {
                            if (srv.Get<Item>(itemId) == null)
                                this.AddEntry(Severity.Warning, "Broken Reference", "Job: " + j.Name, "References a deleted StartItem (" + itemId + ").", j);
                        }
                    }
                }
            }
            if (selectableJobs == 0)
                this.AddEntry(Severity.Warning, "Character Creation", "Playable Jobs", "0 Selectable Jobs/Classes defined. New players may have no class options at login.", srv);
            else
                this.AddEntry(Severity.Pass, "Character Creation", "Playable Jobs (" + selectableJobs + ")", selectableJobs + " Selectable Job(s)/Class(es) ready for character creation.", srv);

            // 6. Global Lua Syntax Validation across Commands, Scripts, Items & Area Regions
            int luaScriptsChecked = 0;
            int luaSyntaxErrors = 0;

            using (Lua lua = new Lua())
            {
                if (Command.Commands != null)
                {
                    foreach (Command cmd in Command.Commands)
                    {
                        if (cmd == null || string.IsNullOrEmpty(cmd.Script))
                            continue;
                        luaScriptsChecked++;
                        string err;
                        if (!ValidateLuaSyntax(lua, cmd.Script, "Command:" + cmd.Name, out err))
                        {
                            luaSyntaxErrors++;
                            this.AddEntry(Severity.Error, "Lua Syntax Error", "Command: " + cmd.Name, err, cmd);
                        }
                    }
                }

                if (srv.DataSets.ContainsKey(typeof(Script)))
                {
                    foreach (Script scr in srv.DataSets[typeof(Script)].Values)
                    {
                        if (scr == null || string.IsNullOrEmpty(scr.Code))
                            continue;
                        luaScriptsChecked++;
                        string err;
                        if (!ValidateLuaSyntax(lua, scr.Code, "Script:" + scr.Name, out err))
                        {
                            luaSyntaxErrors++;
                            this.AddEntry(Severity.Error, "Lua Syntax Error", "Script: " + scr.Name, err, scr);
                        }
                    }
                }

                if (srv.DataSets.ContainsKey(typeof(Item)))
                {
                    foreach (Item it in srv.DataSets[typeof(Item)].Values)
                    {
                        if (it == null || string.IsNullOrEmpty(it.OnUse))
                            continue;
                        luaScriptsChecked++;
                        string err;
                        if (!ValidateLuaSyntax(lua, it.OnUse, "Item.OnUse:" + it.Name, out err))
                        {
                            luaSyntaxErrors++;
                            this.AddEntry(Severity.Error, "Lua Syntax Error", "Item: " + it.Name, err, it);
                        }
                    }
                }

                if (srv.MainArea != null)
                {
                    this.AuditAreaScripts(lua, srv.MainArea, ref luaScriptsChecked, ref luaSyntaxErrors);
                }
            }

            if (luaSyntaxErrors == 0)
            {
                this.AddEntry(Severity.Pass, "Lua VM Validator", "All Lua Scripts (" + luaScriptsChecked + ")",
                    "Validated " + luaScriptsChecked + " Lua scripts across Commands, Scripts, Items & Regions with 0 syntax errors.", srv);
            }

            this.RenderEntries();
        }

        void AuditAreaScripts(Lua lua, Area area, ref int checkedCount, ref int errorCount)
        {
            if (area == null || area.Children == null)
                return;

            foreach (Object child in area.Children)
            {
                Area.Region reg = child as Area.Region;
                if (reg != null)
                {
                    if (!string.IsNullOrEmpty(reg.OnEnter))
                    {
                        checkedCount++;
                        string err;
                        if (!ValidateLuaSyntax(lua, reg.OnEnter, "Region.OnEnter:" + reg.Name, out err))
                        {
                            errorCount++;
                            this.AddEntry(Severity.Error, "Lua Syntax Error", "Region: " + reg.Name + " (OnEnter)", err, reg);
                        }
                    }
                    if (!string.IsNullOrEmpty(reg.OnLeave))
                    {
                        checkedCount++;
                        string err;
                        if (!ValidateLuaSyntax(lua, reg.OnLeave, "Region.OnLeave:" + reg.Name, out err))
                        {
                            errorCount++;
                            this.AddEntry(Severity.Error, "Lua Syntax Error", "Region: " + reg.Name + " (OnLeave)", err, reg);
                        }
                    }
                }

                Area subArea = child as Area;
                if (subArea != null)
                    this.AuditAreaScripts(lua, subArea, ref checkedCount, ref errorCount);
            }
        }

        static bool ValidateLuaSyntax(Lua lua, string code, string chunkName, out string errorMsg)
        {
            errorMsg = null;
            try
            {
                lua.DoString("return function()\r\n" + code + "\r\nend");
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        void RenderEntries()
        {
            this.listView_.BeginUpdate();
            this.listView_.Items.Clear();

            // Sort Errors first, then Warnings, then Passes
            this.entries_.Sort(delegate(AuditEntry a, AuditEntry b)
            {
                return b.Level.CompareTo(a.Level);
            });

            int passes = 0, warnings = 0, errors = 0;
            foreach (AuditEntry e in this.entries_)
            {
                string badge = e.Level == Severity.Error ? "[ERROR]" : (e.Level == Severity.Warning ? "[WARNING]" : "[PASS]");
                ListViewItem item = new ListViewItem(new string[] { badge, e.Category, e.TargetName, e.Message });
                item.Tag = e;
                if (e.Level == Severity.Error)
                {
                    errors++;
                    item.ForeColor = Color.FromArgb(185, 28, 28);
                    item.BackColor = Color.FromArgb(254, 242, 242);
                }
                else if (e.Level == Severity.Warning)
                {
                    warnings++;
                    item.ForeColor = Color.FromArgb(161, 98, 7);
                    item.BackColor = Color.FromArgb(254, 252, 232);
                }
                else
                {
                    passes++;
                    item.ForeColor = Color.FromArgb(21, 128, 61);
                }
                this.listView_.Items.Add(item);
            }

            this.listView_.EndUpdate();
            this.summaryLabel_.Text = string.Format("Scan Complete: {0} Passed  |  {1} Warning(s)  |  {2} Error(s)   (Double-click any row to select in PropertyGrid)",
                passes, warnings, errors);
        }

        void listView__DoubleClick(object sender, EventArgs e)
        {
            if (this.listView_.SelectedItems.Count == 0)
                return;
            AuditEntry entry = this.listView_.SelectedItems[0].Tag as AuditEntry;
            if (entry != null && entry.TargetObject != null && MainForm.Instance != null)
            {
                MainForm.Instance.SelectObjectInStudio(entry.TargetObject);
            }
        }

        void autoFixBtn__Click(object sender, EventArgs e)
        {
            Server srv = Server.Instance;
            if (srv == null)
                return;

            int fixedCount = 0;
            if (srv.Settings != null && string.IsNullOrEmpty(srv.Settings.Name))
            {
                srv.Settings.Name = "Realms of UltiMUD";
                fixedCount++;
            }

            if (srv.MainArea != null && !srv.MainArea.AreaFlags.Get((int)Area.AreaFlag.StartingArea))
            {
                srv.MainArea.AreaFlags.Set((int)Area.AreaFlag.StartingArea, true);
                fixedCount++;
            }

            if (srv.FightSystem == null)
            {
                srv.FightSystem = new AutomatedFightSystem();
                fixedCount++;
            }

            // Clean any orphaned StartItems references
            if (srv.DataSets.ContainsKey(typeof(Race)))
            {
                foreach (Race r in srv.DataSets[typeof(Race)].Values)
                {
                    if (r.StartItems != null)
                        fixedCount += r.StartItems.RemoveAll(delegate(Guid id) { return srv.Get<Item>(id) == null; });
                }
            }
            if (srv.DataSets.ContainsKey(typeof(Job)))
            {
                foreach (Job j in srv.DataSets[typeof(Job)].Values)
                {
                    if (j.StartItems != null)
                        fixedCount += j.StartItems.RemoveAll(delegate(Guid id) { return srv.Get<Item>(id) == null; });
                }
            }

            srv.Save();
            if (MainForm.Instance != null)
                MainForm.Instance.RefreshAllViews();

            this.RunAudit();
            MessageBox.Show(this, "Applied " + fixedCount + " automatic fix(es) and saved the MUD.", "Auto-Fix Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
