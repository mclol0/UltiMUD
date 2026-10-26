using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using Damdai;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    public partial class ObjectExplorer : UserControl
    {
        #region Properties
        Object SelectedObject
        {
            get
            {
                TreeNode node = this.treeView1.SelectedNode;
                if (node != null)
                {
                    return node.Tag as Object;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region Events
        public event Function_void<object> ObjectSelected;
        public event Function_void<Object> EditObject;
        public event Function_void<Object> GoTo;
        public event Function_void<User> MonitorUser;
        #endregion

        ToolStripTextBox filterTextBox_;
        string filterQuery_ = string.Empty;

        public ObjectExplorer()
        {
            InitializeComponent();
            StudioTheme.StyleToolStrip(this.toolStrip1, false);
            StudioTheme.StyleTreeView(this.treeView1);
            this.EnsureExtraNodes();
            this.InitSearchAndCloneTools();
            this.treeView1.Nodes[0].Expand();
            this.treeView1.MouseClick += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    this.treeView1.SelectedNode = this.treeView1.GetNodeAt(e.Location);
            };
            this.treeView1.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
        }

        void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null)
                return;

            object tag = e.Node.Tag;
            if (tag is Command || tag is Script || tag is Help)
            {
                this.OpenCodeOrTextEditor(tag);
            }
            else if (tag is Area || tag is Object)
            {
                if (this.EditObject != null)
                    this.EditObject(tag as Object);
            }
        }

        public bool OpenCodeOrTextEditor(object target)
        {
            if (target is Command)
            {
                Command cmd = target as Command;
                using (Damdai.Forms.TextEditorWindow editor = new Damdai.Forms.TextEditorWindow(true))
                {
                    editor.Text = cmd.Script ?? string.Empty;
                    if (editor.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        cmd.Script = editor.Text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                        cmd.Save();
                        return true;
                    }
                }
            }
            else if (target is Script)
            {
                Script scr = target as Script;
                using (Damdai.Forms.TextEditorWindow editor = new Damdai.Forms.TextEditorWindow(true))
                {
                    editor.Text = scr.Code ?? string.Empty;
                    if (editor.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        scr.Code = editor.Text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                        if (Server.Instance != null)
                            Server.Instance.Save();
                        return true;
                    }
                }
            }
            else if (target is Help)
            {
                Help h = target as Help;
                using (Damdai.Forms.TextEditorWindow editor = new Damdai.Forms.TextEditorWindow(false))
                {
                    editor.Text = h.Text ?? string.Empty;
                    if (editor.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        h.Text = editor.Text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                        h.Save();
                        return true;
                    }
                }
            }
            return false;
        }

        void InitSearchAndCloneTools()
        {
            if (this.filterTextBox_ != null)
                return;

            // Add + Create dropdown button on ObjectExplorer toolbar for intuitive 1-click creation
            ToolStripDropDownButton addBtn = new ToolStripDropDownButton("+ Create");
            addBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            addBtn.Font = StudioTheme.FontBold;
            addBtn.ForeColor = StudioTheme.AccentBlue;
            addBtn.ToolTipText = "Create a new Item, NPC, Ability, Race, Job/Class, Command, Script, or Help topic";

            addBtn.DropDownItems.Add("⚔️ New Item", null, this.newItemToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("🐉 New NPC", null, this.newNpcToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("✨ New Ability / Spell", null, this.newAbilityToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("🛡️ New Playable Race", null, this.newRaceToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("🧙 New Playable Class (Job)", null, this.newJobToolStripMenuItem_Click);
            addBtn.DropDownItems.Add(new ToolStripSeparator());
            addBtn.DropDownItems.Add("📜 New Lua Command", null, this.newCommandToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("⚙️ New System Lua Script", null, this.newScriptToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("📖 New Help File", null, this.newHelpFileToolStripMenuItem_Click);
            addBtn.DropDownItems.Add(new ToolStripSeparator());
            addBtn.DropDownItems.Add("📊 New Stat Blueprint", null, this.newStatBlueprintToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("🧪 New Status Effect", null, this.newStatusEffectToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("📂 New Ability Group", null, this.newAbilityGroupToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("🏷️ New Tag", null, this.newTagToolStripMenuItem_Click);
            addBtn.DropDownItems.Add(new ToolStripSeparator());
            addBtn.DropDownItems.Add("💬 New Chat Channel", null, this.newChannelToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("😊 New Social Emote", null, this.newSocialToolStripMenuItem_Click);
            addBtn.DropDownItems.Add("📋 New Message Board", null, this.newMessageBoardToolStripMenuItem_Click);

            this.toolStrip1.Items.Insert(0, new ToolStripSeparator());
            this.toolStrip1.Items.Insert(0, addBtn);

            this.toolStrip1.Items.Add(new ToolStripSeparator());
            ToolStripLabel searchLbl = new ToolStripLabel("🔍");
            searchLbl.ToolTipText = "Filter objects by name";
            this.toolStrip1.Items.Add(searchLbl);

            this.filterTextBox_ = new ToolStripTextBox();
            this.filterTextBox_.Size = new Size(95, 22);
            this.filterTextBox_.BackColor = Color.White;
            this.filterTextBox_.ToolTipText = "Type to filter Commands, Items, NPCs, Abilities, Scripts, Help & Blueprints";
            this.filterTextBox_.TextChanged += delegate
            {
                this.filterQuery_ = (this.filterTextBox_.Text ?? string.Empty).Trim();
                if (Server.Instance != null)
                    this.Generate(Server.Instance);
                if (!string.IsNullOrEmpty(this.filterQuery_))
                    this.treeView1.ExpandAll();
            };
            this.toolStrip1.Items.Add(this.filterTextBox_);

            ToolStripButton clearBtn = new ToolStripButton("×");
            clearBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            clearBtn.ToolTipText = "Clear search filter";
            clearBtn.Click += delegate
            {
                if (!string.IsNullOrEmpty(this.filterTextBox_.Text))
                    this.filterTextBox_.Text = string.Empty;
            };
            this.toolStrip1.Items.Add(clearBtn);

            // Add Edit Script/Text & Duplicate / Clone to Command, Generic Blueprint, and Help context menus
            if (this.commandMenuStrip != null && !this.commandMenuStrip.Items.ContainsKey("duplicateCommandItem"))
            {
                ToolStripMenuItem editCmd = new ToolStripMenuItem("📝 &Edit Lua Script...");
                editCmd.Font = StudioTheme.FontBold;
                editCmd.Click += delegate { if (this.treeView1.SelectedNode != null) this.OpenCodeOrTextEditor(this.treeView1.SelectedNode.Tag); };
                this.commandMenuStrip.Items.Insert(0, editCmd);

                ToolStripMenuItem dupCmd = new ToolStripMenuItem("📋 &Duplicate / Clone");
                dupCmd.Name = "duplicateCommandItem";
                dupCmd.Click += new EventHandler(this.duplicateSelectedNode_Click);
                this.commandMenuStrip.Items.Insert(1, dupCmd);
            }

            if (this.genericMenuStrip != null && !this.genericMenuStrip.Items.ContainsKey("duplicateGenericItem"))
            {
                ToolStripMenuItem dupGen = new ToolStripMenuItem("📋 &Duplicate / Clone");
                dupGen.Name = "duplicateGenericItem";
                dupGen.Click += new EventHandler(this.duplicateSelectedNode_Click);
                this.genericMenuStrip.Items.Insert(0, dupGen);
            }

            if (this.helpfileMenuStrip != null && !this.helpfileMenuStrip.Items.ContainsKey("duplicateHelpItem"))
            {
                ToolStripMenuItem editHelp = new ToolStripMenuItem("📝 &Edit Help Text...");
                editHelp.Font = StudioTheme.FontBold;
                editHelp.Click += delegate { if (this.treeView1.SelectedNode != null) this.OpenCodeOrTextEditor(this.treeView1.SelectedNode.Tag); };
                this.helpfileMenuStrip.Items.Insert(0, editHelp);

                ToolStripMenuItem dupHelp = new ToolStripMenuItem("📋 &Duplicate / Clone");
                dupHelp.Name = "duplicateHelpItem";
                dupHelp.Click += new EventHandler(this.duplicateSelectedNode_Click);
                this.helpfileMenuStrip.Items.Insert(1, dupHelp);
            }
        }

        bool MatchesFilter(string text)
        {
            if (string.IsNullOrEmpty(this.filterQuery_))
                return true;
            if (string.IsNullOrEmpty(text))
                return false;
            return text.IndexOf(this.filterQuery_, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void duplicateSelectedNode_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = this.treeView1.SelectedNode;
            if (selectedNode == null || selectedNode.Tag == null || Server.Instance == null)
                return;

            object tag = selectedNode.Tag;
            if (tag is Command)
            {
                Command src = tag as Command;
                Command copy = new Command();
                copy.Name = src.Name + "_copy";
                copy.Script = src.Script;
                copy.SystemCommand = src.SystemCommand;
                copy.Hidden = src.Hidden;
                copy.Priority = src.Priority;
                copy.CanUseIncapacitated = src.CanUseIncapacitated;
                if (src.Aliases != null)
                    copy.Aliases = new List<string>(src.Aliases);
                Command.Commands.Add(copy);
                copy.Save();
                this.AddTreeNode(this.treeView1.Nodes["server"].Nodes["commands"], copy, this.commandMenuStrip);
                if (this.ObjectSelected != null) this.ObjectSelected(copy);
            }
            else if (tag is Help)
            {
                Help src = tag as Help;
                Help copy = new Help(src.Name + "_COPY", src.Text);
                Help.HelpFiles.Add(copy);
                copy.Save();
                this.AddTreeNode(this.treeView1.Nodes["server"].Nodes["help"], copy, this.helpfileMenuStrip);
                if (this.ObjectSelected != null) this.ObjectSelected(copy);
            }
            else if (tag is IIdentifiable)
            {
                try
                {
                    // Deep clone via binary memory serialization
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        bf.Binder = new Server.SerializationBinder();
                        bf.Serialize(ms, tag);
                        ms.Position = 0;
                        IIdentifiable copy = bf.Deserialize(ms) as IIdentifiable;
                        if (copy != null)
                        {
                            copy.Id = Server.Instance.NewGuid();
                            copy.Name = (copy.Name ?? "Object") + " Copy";
                            Server.Instance.DataSets[copy.GetType()][copy.Id] = copy;
                            this.AddTreeNode(selectedNode.Parent, copy, this.genericMenuStrip);
                            Server.Instance.Save();
                            if (this.ObjectSelected != null) this.ObjectSelected(copy);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not duplicate object: " + ex.Message, "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public void SelectObjectInTree(object target)
        {
            if (target == null)
                return;
            TreeNode found = this.GetTreeNode(target);
            if (found != null)
            {
                this.treeView1.SelectedNode = found;
                found.EnsureVisible();
            }
        }

        void EnsureExtraNodes()
        {
            TreeNode serverNode = this.treeView1.Nodes["server"];
            if (serverNode == null)
                return;

            if (serverNode.Nodes["abilitygroups"] == null)
            {
                ContextMenuStrip abilityGroupsMenu = new ContextMenuStrip(this.components);
                ToolStripMenuItem newAbilityGroupItem = new ToolStripMenuItem("&New Ability Group");
                newAbilityGroupItem.Click += new EventHandler(this.newAbilityGroupToolStripMenuItem_Click);
                abilityGroupsMenu.Items.Add(newAbilityGroupItem);

                TreeNode abilityGroupsNode = new TreeNode("Ability Groups", 20, 20);
                abilityGroupsNode.Name = "abilitygroups";
                abilityGroupsNode.ContextMenuStrip = abilityGroupsMenu;

                TreeNode abilitiesNode = serverNode.Nodes["abilities"];
                int insertIdx = (abilitiesNode != null ? abilitiesNode.Index + 1 : serverNode.Nodes.Count);
                serverNode.Nodes.Insert(insertIdx, abilityGroupsNode);
            }

            if (serverNode.Nodes["tags"] == null)
            {
                ContextMenuStrip tagsMenu = new ContextMenuStrip(this.components);
                ToolStripMenuItem newTagItem = new ToolStripMenuItem("&New Tag");
                newTagItem.Click += new EventHandler(this.newTagToolStripMenuItem_Click);
                tagsMenu.Items.Add(newTagItem);

                TreeNode tagsNode = new TreeNode("Tags", 6, 6);
                tagsNode.Name = "tags";
                tagsNode.ContextMenuStrip = tagsMenu;

                TreeNode itemsNode = serverNode.Nodes["items"];
                int insertIdx = (itemsNode != null ? itemsNode.Index + 1 : serverNode.Nodes.Count);
                serverNode.Nodes.Insert(insertIdx, tagsNode);
            }

            if (this.playerMenuStrip != null && !this.playerMenuStrip.Items.ContainsKey("administratorToolStripMenuItem"))
            {
                ToolStripMenuItem adminItem = new ToolStripMenuItem("&Administrator");
                adminItem.Name = "administratorToolStripMenuItem";
                adminItem.Click += delegate(object sender, EventArgs e)
                {
                    if (this.treeView1.SelectedNode != null && this.treeView1.SelectedNode.Tag is User)
                    {
                        User user = this.treeView1.SelectedNode.Tag as User;
                        bool isAdmin = !user.Flags.Get((int)User.UserFlags.Administrator);
                        user.Flags.Set((int)User.UserFlags.Administrator, isAdmin);
                        adminItem.Checked = isAdmin;
                    }
                };
                this.playerMenuStrip.Items.Add(adminItem);
                this.playerMenuStrip.Opening += delegate(object sender, CancelEventArgs e)
                {
                    if (this.treeView1.SelectedNode != null && this.treeView1.SelectedNode.Tag is User)
                    {
                        User user = this.treeView1.SelectedNode.Tag as User;
                        this.silenceToolStripMenuItem.Checked = user.Flags.Get((int)User.UserFlags.Silenced);
                        adminItem.Checked = user.Flags.Get((int)User.UserFlags.Administrator);
                    }
                };
            }
        }

        void treeView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = this.treeView1.GetNodeAt(e.Location);
                if (node != null)
                    this.treeView1.SelectedNode = node;
            }
        }

        public void Generate(Server server)
        {
            this.EnsureExtraNodes();
            this.treeView1.Nodes[0].Tag = server;

            this.treeView1.BeginUpdate();

            this.LoadCommands();
            this.LoadPlayers();
            this.LoadHelp();
            this.LoadFromServer<Channel>("channels");
            this.LoadFromServer<Social>("socials");
            this.LoadFromServer<MessageBoard>("messageboards");
            this.LoadFromServer<StatBlueprint>("stats");
            this.LoadFromServer<StatusEffectBlueprint>("statuseffects");
            this.LoadFromServer<Ability>("abilities");
            this.LoadFromServer<AbilityGroup>("abilitygroups");
            this.LoadFromServer<Item>("items");
            this.LoadFromServer<Tag>("tags");
            this.LoadFromServer<Npc>("npcs");
            this.LoadFromServer<Job>("jobs");
            this.LoadFromServer<Race>("races");
            this.LoadFromServer<Script>("scripts");

            Object parent = server.MainArea;
            TreeNode worldnode = this.treeView1.Nodes["server"].Nodes["world"];
            worldnode.Nodes.Clear();
            this.Generate(worldnode, parent);

            this.treeView1.EndUpdate();
        }

        void LoadCommands()
        {
            TreeNode parentnode = this.treeView1.Nodes["server"].Nodes["commands"];
            parentnode.Nodes.Clear();
            List<Command> commands = Damdai.Collections.Util.CopyList<Command>(Command.Commands);
            commands.Sort(delegate(Command c1, Command c2) { return string.Compare(c1.Name, c2.Name); });
            foreach (Command cmd in commands)
            {
                if (!this.MatchesFilter(cmd.ToString()))
                    continue;
                TreeNode n = new TreeNode(cmd.ToString(), parentnode.ImageIndex, parentnode.SelectedImageIndex);
                n.Tag = cmd;
                n.ContextMenuStrip = this.commandMenuStrip;
                parentnode.Nodes.Add(n);
            }
            parentnode.Text = "Commands (" + parentnode.Nodes.Count + ")";
        }
        void LoadPlayers()
        {
            TreeNode parentnode = this.treeView1.Nodes["server"].Nodes["players"];
            parentnode.Nodes.Clear();
            List<User> list = Damdai.Collections.Util.CopyList<User>(Server.Instance.Users);
            list.Sort(delegate(User c1, User c2) { return string.Compare(c1.ToString(), c2.ToString()); });
            foreach (User u in list)
            {
                if (!this.MatchesFilter(u.ToString()))
                    continue;
                TreeNode n = new TreeNode(u.ToString(), parentnode.ImageIndex, parentnode.SelectedImageIndex);
                n.Tag = u;
                n.ContextMenuStrip = this.playerMenuStrip;
                parentnode.Nodes.Add(n);
            }
            parentnode.Text = "Players (" + parentnode.Nodes.Count + ")";
        }
        void LoadHelp()
        {
            TreeNode parentnode = this.treeView1.Nodes["server"].Nodes["help"];
            parentnode.Nodes.Clear();
            List<Help> list = Damdai.Collections.Util.CopyList<Help>(Help.HelpFiles);
            list.Sort(delegate(Help c1, Help c2) { return string.Compare(c1.Name, c2.Name); });
            foreach (Help help in list)
            {
                if (!this.MatchesFilter(help.Name))
                    continue;
                TreeNode n = new TreeNode(help.Name, parentnode.ImageIndex, parentnode.SelectedImageIndex);
                n.Tag = help;
                n.ContextMenuStrip = this.helpfileMenuStrip;
                parentnode.Nodes.Add(n);
            }
            parentnode.Text = "Help (" + parentnode.Nodes.Count + ")";
        }
        void LoadFromServer<T>(string nodekey) where T : IIdentifiable
        {
            TreeNode parentnode = this.treeView1.Nodes["server"].Nodes[nodekey];
            parentnode.Nodes.Clear();
            List<IIdentifiable> list = null;
            lock (Server.Instance.DataSets[typeof(T)])
                list = new List<IIdentifiable>(Server.Instance.DataSets[typeof(T)].Values);
            list.Sort(delegate(IIdentifiable c1, IIdentifiable c2) { return string.Compare(c1.Name, c2.Name); });
            foreach (T t in list)
            {
                if (!this.MatchesFilter(t.Name))
                    continue;
                TreeNode n = new TreeNode(t.Name, parentnode.ImageIndex, parentnode.SelectedImageIndex);
                n.Tag = t;
                n.ContextMenuStrip = this.genericMenuStrip;
                parentnode.Nodes.Add(n);
            }

            int index = parentnode.Text.IndexOf('(');
            if (index == -1)
                parentnode.Text += " (" + parentnode.Nodes.Count + ")";
            else
                parentnode.Text = parentnode.Text.Substring(0, index) + "(" + parentnode.Nodes.Count + ")";
        }

        void Generate(TreeNode node, Object o)
        {
            string name = (string.IsNullOrEmpty(o.Name) ? o.ToString() : o.Name);
            TreeNode n = new TreeNode(name);
            //n.ToolTipText = o.Id.ToString();
            n.Name = name;
            n.ImageIndex = n.SelectedImageIndex = o.IconIndex;
            n.Tag = o;
            n.ContextMenuStrip = this.objectMenuStrip;

            if (node == null)
            {
                this.treeView1.Nodes.Add(n);
            }
            else
            {
                int i = node.Nodes.IndexOfKey(name);
                if (i > -1)
                {
                    TreeNode groupNode = node.Nodes[i];
                    if (groupNode.Tag != null)
                    {
                        TreeNode parent = groupNode.Parent;
                        groupNode.Remove();
                        groupNode = new TreeNode(o.Name, new TreeNode[] { groupNode, n });
                        groupNode.Name = name;
                        groupNode.ImageIndex = groupNode.SelectedImageIndex = o.IconIndex;
                        parent.Nodes.Add(groupNode);
                        n.Name = null;
                    }
                    else
                    {
                        groupNode.Nodes.Add(n);
                    }
                    groupNode.Text = name + " (" + groupNode.Nodes.Count + ")";
                }
                else
                {
                    node.Nodes.Add(n);
                    if (node.Parent == null)
                        node.Expand();
                }
            }
            foreach (Object child in o.Children)
                this.Generate(n, child);
        }
        TreeNode GetTreeNode(object o)
        {
            foreach (TreeNode n in this.treeView1.Nodes)
            {
                if (n.Tag == o)
                    return n;
                else
                {
                    TreeNode node = this.GetTreeNode(n, o);
                    if (node != null)
                        return node;
                }
            }
            return null;
        }
        TreeNode GetTreeNode(TreeNode node, object o)
        {
            foreach (TreeNode n in node.Nodes)
            {
                if (n.Tag == o)
                    return n;
                else
                {
                    TreeNode subNode = this.GetTreeNode(n, o);
                    if (subNode != null)
                        return subNode;
                }
            }
            return null;
        }
        TreeNode GetRootNode(TreeNode node)
        {
            TreeNode parentnode = node.Parent;
            while (parentnode.Parent != null && parentnode.Parent != this.treeView1.Nodes["server"])
                parentnode = parentnode.Parent;
            return parentnode;
        }
        public void Add(object o)
        {
            if (o is User)
            {
                TreeNode existing = this.GetTreeNode(o);
                if (existing != null)
                {
                    this.Update(o);
                    return;
                }
                TreeNode parentnode = this.treeView1.Nodes["server"].Nodes["players"];
                TreeNode n = new TreeNode(o.ToString(), parentnode.ImageIndex, parentnode.SelectedImageIndex);
                n.Tag = o;
                n.ContextMenuStrip = this.playerMenuStrip;
                parentnode.Nodes.Add(n);
                parentnode.Text = "Players (" + parentnode.Nodes.Count + ")";
            }
        }
        public void Update(object o)
        {
            if (o is User)
            {
                TreeNode n = this.GetTreeNode(o);
                if (n != null)
                {
                    User user = o as User;
                    n.ForeColor = (user.Connected ? Color.Black : Color.Gray);
                    if (user.Connected)
                        n.Text = (user.Character != null && !string.IsNullOrEmpty(user.Character.Name) ? user.Character.Name + " (" + user.IpAddress + ")" : user.ToString());
                }
            }
        }
        public void Remove(object o)
        {
            TreeNode n = this.GetTreeNode(o);
            if (n != null)
            {
                if (n.Parent != null)
                    this.RemoveTreeNode(n);
                else
                    this.treeView1.Nodes.Remove(n);
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    this.Generate(Server.Instance);
                    break;
                case Keys.Delete:
                    TreeNode selectednode = this.treeView1.SelectedNode;
                    if(selectednode != null && this.GetRootNode(selectednode) == this.treeView1.Nodes["server"].Nodes["world"])
                        MessageBox.Show(this, "You cannot delete that object here. Try the area editor.", "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    else if (selectednode != null && selectednode.Tag != null && MessageBox.Show(this, "Delete " + selectednode.Tag.ToString() + "?", "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (selectednode.Tag is Command)
                            this.DeleteCommand(selectednode.Tag as Command, selectednode);
                        else if (selectednode.Tag is Help)
                            this.DeleteHelp(selectednode.Tag as Help, selectednode);
                        else if (selectednode.Tag is IIdentifiable)
                        {
                            Server.Instance.DataSets[selectednode.Tag.GetType()].Remove(((IIdentifiable)selectednode.Tag).Id);
                            this.RemoveTreeNode(selectednode);
                        }
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (this.ObjectSelected != null)
                this.ObjectSelected(e.Node.Tag);
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.Generate(Server.Instance);
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.treeView1.ExpandAll();
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.treeView1.CollapseAll();
        }
        private void silenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode != null && this.treeView1.SelectedNode.Tag is User)
            {
                this.silenceToolStripMenuItem.Checked = !this.silenceToolStripMenuItem.Checked;
                User user = this.treeView1.SelectedNode.Tag as User;
                user.Flags.Set((int)User.UserFlags.Silenced, this.silenceToolStripMenuItem.Checked);
            }
        }
        private void monitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode != null && this.MonitorUser != null)
                this.MonitorUser(this.treeView1.SelectedNode.Tag as User);
        }
        private void reloadCommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Command.Init(MainForm.Instance.ServerPath, MainForm.Instance.ServerPath);
            this.LoadCommands();
        }
        private void newCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Command cmd = new Command();
            Command.Commands.Add(cmd);
            this.AddTreeNode(this.treeView1.Nodes["server"].Nodes["commands"], cmd, this.commandMenuStrip);
            if (this.ObjectSelected != null) this.ObjectSelected(cmd);
        }

        void AddTreeNode(TreeNode parentnode, object tag, ContextMenuStrip menu)
        {
            TreeNode n = new TreeNode(tag.ToString(), parentnode.ImageIndex, parentnode.SelectedImageIndex);
            n.Tag = tag;
            n.ContextMenuStrip = menu;
            parentnode.Nodes.Add(n);
            this.treeView1.SelectedNode = n;
            int index = parentnode.Text.IndexOf('(');
            if (index == -1)
                parentnode.Text += " (" + parentnode.Nodes.Count + ")";
            else
                parentnode.Text = parentnode.Text.Substring(0, index) + "(" + parentnode.Nodes.Count + ")";
        }
        void RemoveTreeNode(TreeNode node)
        {
            TreeNode parentnode = node.Parent;
            node.Remove();
            int count = parentnode.Nodes.Count;
            int index = parentnode.Text.IndexOf('(');
            if (index == -1)
                parentnode.Text += " (" + count + ")";
            else
                parentnode.Text = parentnode.Text.Substring(0, index) + "(" + count + ")";
        }
        private void deleteCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DeleteCommand(this.treeView1.SelectedNode.Tag as Command, this.treeView1.SelectedNode);
        }
        void DeleteCommand(Command cmd, TreeNode node)
        {
            Command.Commands.Remove(cmd);
            cmd.Delete();
            this.RemoveTreeNode(node);
            if (this.ObjectSelected != null) this.ObjectSelected(null);
        }

        private void deleteGenericToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IIdentifiable o = this.treeView1.SelectedNode.Tag as IIdentifiable;
            if (o is StatBlueprint && (o as StatBlueprint).IsPermanent)
            {
                MessageBox.Show(this, "This stat blueprint cannot be deleted.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            Server.Instance.DataSets[this.treeView1.SelectedNode.Tag.GetType()].Remove(o.Id);
            this.RemoveTreeNode(this.treeView1.SelectedNode);
        }
        private void reloadHelpFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.Init(MainForm.Instance.ServerPath, MainForm.Instance.ServerPath);
            this.LoadHelp();
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DeleteHelp(this.treeView1.SelectedNode.Tag as Help, this.treeView1.SelectedNode);
        }
        void DeleteHelp(Help help, TreeNode node)
        {
            Help.HelpFiles.Remove(help);
            help.Delete();
            this.RemoveTreeNode(node);
            if (this.ObjectSelected != null) this.ObjectSelected(null);
        }

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode != null && this.EditObject != null)
                this.EditObject(this.treeView1.SelectedNode.Tag as Object);
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode != null && this.GoTo != null && this.treeView1.SelectedNode.Tag is Object)
                this.GoTo(this.treeView1.SelectedNode.Tag as Object);
        }

        void NewGenericObject<T>(TreeNode parentnode) where T : class, IIdentifiable, new()
        {
            T t = new T();
            Server.Instance.Add<T>(t);
            this.AddTreeNode(parentnode, t, this.genericMenuStrip);
            if (this.ObjectSelected != null) this.ObjectSelected(t);
        }
        private void newChannelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Channel>(this.treeView1.Nodes["server"].Nodes["channels"]);
        }
        private void newMessageBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<MessageBoard>(this.treeView1.Nodes["server"].Nodes["messageboards"]);
        }
        private void newSocialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Social>(this.treeView1.Nodes["server"].Nodes["socials"]);
        }
        private void newHelpFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help h = new Help();
            Help.HelpFiles.Add(h);
            this.AddTreeNode(this.treeView1.Nodes["server"].Nodes["help"], h, this.helpfileMenuStrip);
            if (this.ObjectSelected != null) this.ObjectSelected(h);
        }
        private void newStatBlueprintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<StatBlueprint>(this.treeView1.Nodes["server"].Nodes["stats"]);
        }
        private void newStatusEffectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<StatusEffectBlueprint>(this.treeView1.Nodes["server"].Nodes["statuseffects"]);
        }
        private void newAbilityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Ability>(this.treeView1.Nodes["server"].Nodes["abilities"]);
        }
        private void newItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Item>(this.treeView1.Nodes["server"].Nodes["items"]);
        }
        private void newNpcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Npc>(this.treeView1.Nodes["server"].Nodes["npcs"]);
        }
        private void newRaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Race>(this.treeView1.Nodes["server"].Nodes["races"]);
        }
        private void newJobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Job>(this.treeView1.Nodes["server"].Nodes["jobs"]);
        }
        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Script>(this.treeView1.Nodes["server"].Nodes["scripts"]);
        }
        private void newAbilityGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<AbilityGroup>(this.treeView1.Nodes["server"].Nodes["abilitygroups"]);
        }
        private void newTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NewGenericObject<Tag>(this.treeView1.Nodes["server"].Nodes["tags"]);
        }

        public void CreateNewItem() { this.newItemToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void CreateNewNpc() { this.newNpcToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void CreateNewAbility() { this.newAbilityToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void CreateNewRace() { this.newRaceToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void CreateNewJob() { this.newJobToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void CreateNewCommand() { this.newCommandToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void CreateNewScript() { this.newScriptToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void CreateNewHelp() { this.newHelpFileToolStripMenuItem_Click(this, EventArgs.Empty); }
        public void CreateNewStat() { this.newStatBlueprintToolStripMenuItem_Click(this, EventArgs.Empty); }

        public void DuplicateSelectedObject()
        {
            this.duplicateSelectedNode_Click(this, EventArgs.Empty);
        }

        public void DeleteSelectedObject()
        {
            TreeNode selectednode = this.treeView1.SelectedNode;
            if (selectednode == null || selectednode.Tag == null)
                return;
            if (this.GetRootNode(selectednode) == this.treeView1.Nodes["server"].Nodes["world"])
            {
                MessageBox.Show(this, "World map objects are deleted inside the Area Editor.", "Delete in Area Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(this, "Delete " + selectednode.Tag.ToString() + "?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (selectednode.Tag is Command)
                    this.DeleteCommand(selectednode.Tag as Command, selectednode);
                else if (selectednode.Tag is Help)
                    this.DeleteHelp(selectednode.Tag as Help, selectednode);
                else if (selectednode.Tag is IIdentifiable)
                {
                    if (selectednode.Tag is StatBlueprint && (selectednode.Tag as StatBlueprint).IsPermanent)
                    {
                        MessageBox.Show(this, "This stat blueprint is permanent and cannot be deleted.", "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    Server.Instance.DataSets[selectednode.Tag.GetType()].Remove(((IIdentifiable)selectednode.Tag).Id);
                    this.RemoveTreeNode(selectednode);
                    if (this.ObjectSelected != null) this.ObjectSelected(null);
                }
            }
        }
    }
}
