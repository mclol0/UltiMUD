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

        public ObjectExplorer()
        {
            InitializeComponent();
            this.treeView1.Nodes[0].Expand();
            this.treeView1.MouseClick += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    this.treeView1.SelectedNode = this.treeView1.GetNodeAt(e.Location);
            };
        }

        void treeView1_MouseClick(object sender, MouseEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Generate(Server server)
        {
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
            this.LoadFromServer<Item>("items");
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
                        n.Text = user.Character.Name + " (" + user.IpAddress + ")";
                }
            }
        }
        public void Remove(Object o)
        {
            TreeNode n = this.GetTreeNode(o);
            if (n != null)
                this.treeView1.Nodes.Remove(n);
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

            //if (e.Node.Tag is Area)
            //{
            //    this.areaCanvas1.SetArea(e.Node.Tag as Area);
            //}
            //else
            //{
            //    this.selectedObject_ = e.Node.Tag as Object;
            //    Area area = this.selectedObject_.Area;
            //    this.areaCanvas1.SetArea(area);
            //    this.areaCanvas1.GoTo(this.selectedObject_.Location);
            //}
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
            this.AddTreeNode(this.treeView1.Nodes["server"].Nodes["help"], h, this.helpMenuStrip);
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
    }
}
