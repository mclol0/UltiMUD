namespace Ultimud
{
    partial class AreaCanvas
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AreaCanvas));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setThisLocationAsTheStartingLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.goToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.addResetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.npcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addShopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.purgeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.thisLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.areaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.worldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.designexactToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.designfastToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripComboBox2 = new System.Windows.Forms.ToolStripComboBox();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setThisLocationAsTheStartingLocationToolStripMenuItem,
            this.toolStripSeparator1,
            this.goToToolStripMenuItem,
            this.toolStripSeparator2,
            this.addResetToolStripMenuItem,
            this.addShopToolStripMenuItem,
            this.toolStripSeparator5,
            this.deleteToolStripMenuItem,
            this.purgeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(275, 176);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // setThisLocationAsTheStartingLocationToolStripMenuItem
            // 
            this.setThisLocationAsTheStartingLocationToolStripMenuItem.Name = "setThisLocationAsTheStartingLocationToolStripMenuItem";
            this.setThisLocationAsTheStartingLocationToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.setThisLocationAsTheStartingLocationToolStripMenuItem.Text = "Set this location as the starting location";
            this.setThisLocationAsTheStartingLocationToolStripMenuItem.Click += new System.EventHandler(this.setThisLocationAsTheStartingLocationToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(271, 6);
            // 
            // goToToolStripMenuItem
            // 
            this.goToToolStripMenuItem.Name = "goToToolStripMenuItem";
            this.goToToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.goToToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.goToToolStripMenuItem.Text = "&Go to...";
            this.goToToolStripMenuItem.Click += new System.EventHandler(this.goToToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(271, 6);
            // 
            // addResetToolStripMenuItem
            // 
            this.addResetToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemToolStripMenuItem,
            this.npcToolStripMenuItem});
            this.addResetToolStripMenuItem.Name = "addResetToolStripMenuItem";
            this.addResetToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.addResetToolStripMenuItem.Text = "Add &Reset";
            // 
            // itemToolStripMenuItem
            // 
            this.itemToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("itemToolStripMenuItem.Image")));
            this.itemToolStripMenuItem.Name = "itemToolStripMenuItem";
            this.itemToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.itemToolStripMenuItem.Text = "&Item";
            this.itemToolStripMenuItem.Click += new System.EventHandler(this.itemToolStripMenuItem_Click);
            // 
            // npcToolStripMenuItem
            // 
            this.npcToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("npcToolStripMenuItem.Image")));
            this.npcToolStripMenuItem.Name = "npcToolStripMenuItem";
            this.npcToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.npcToolStripMenuItem.Text = "&Npc";
            this.npcToolStripMenuItem.Click += new System.EventHandler(this.npcToolStripMenuItem_Click);
            // 
            // addShopToolStripMenuItem
            // 
            this.addShopToolStripMenuItem.Name = "addShopToolStripMenuItem";
            this.addShopToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.addShopToolStripMenuItem.Text = "Add &Shop";
            this.addShopToolStripMenuItem.Click += new System.EventHandler(this.addShopToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(271, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.deleteToolStripMenuItem.Text = "&Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // purgeToolStripMenuItem
            // 
            this.purgeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.thisLocationToolStripMenuItem,
            this.areaToolStripMenuItem,
            this.worldToolStripMenuItem});
            this.purgeToolStripMenuItem.Name = "purgeToolStripMenuItem";
            this.purgeToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.purgeToolStripMenuItem.Text = "&Purge";
            // 
            // thisLocationToolStripMenuItem
            // 
            this.thisLocationToolStripMenuItem.Name = "thisLocationToolStripMenuItem";
            this.thisLocationToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.thisLocationToolStripMenuItem.Text = "&Location";
            this.thisLocationToolStripMenuItem.Click += new System.EventHandler(this.thisLocationToolStripMenuItem_Click);
            // 
            // areaToolStripMenuItem
            // 
            this.areaToolStripMenuItem.Name = "areaToolStripMenuItem";
            this.areaToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.areaToolStripMenuItem.Text = "A&rea";
            this.areaToolStripMenuItem.Click += new System.EventHandler(this.areaToolStripMenuItem_Click);
            // 
            // worldToolStripMenuItem
            // 
            this.worldToolStripMenuItem.Name = "worldToolStripMenuItem";
            this.worldToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.worldToolStripMenuItem.Text = "&World";
            this.worldToolStripMenuItem.Click += new System.EventHandler(this.worldToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox1,
            this.toolStripSeparator3,
            this.toolStripDropDownButton2,
            this.toolStripLabel1,
            this.toolStripSeparator4,
            this.toolStripComboBox2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(451, 25);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox1.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.designexactToolStripMenuItem,
            this.designfastToolStripMenuItem,
            this.gameToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(29, 22);
            this.toolStripDropDownButton2.Text = "View";
            // 
            // designexactToolStripMenuItem
            // 
            this.designexactToolStripMenuItem.Name = "designexactToolStripMenuItem";
            this.designexactToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.designexactToolStripMenuItem.Text = "Design (true)";
            this.designexactToolStripMenuItem.Click += new System.EventHandler(this.designexactToolStripMenuItem_Click);
            // 
            // designfastToolStripMenuItem
            // 
            this.designfastToolStripMenuItem.Name = "designfastToolStripMenuItem";
            this.designfastToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.designfastToolStripMenuItem.Text = "Design (fast)";
            this.designfastToolStripMenuItem.Click += new System.EventHandler(this.designfastToolStripMenuItem_Click);
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.gameToolStripMenuItem.Text = "Game";
            this.gameToolStripMenuItem.Click += new System.EventHandler(this.gameToolStripMenuItem_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(78, 22);
            this.toolStripLabel1.Text = "toolStripLabel1";
            this.toolStripLabel1.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripComboBox2
            // 
            this.toolStripComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox2.DropDownWidth = 75;
            this.toolStripComboBox2.Name = "toolStripComboBox2";
            this.toolStripComboBox2.Size = new System.Drawing.Size(75, 25);
            this.toolStripComboBox2.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox2_SelectedIndexChanged);
            // 
            // AreaCanvas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.toolStrip1);
            this.Name = "AreaCanvas";
            this.Size = new System.Drawing.Size(451, 306);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AreaCanvas2_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AreaCanvas2_MouseMove);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AreaCanvas2_KeyUp);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.AreaCanvas2_MouseUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AreaCanvas2_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem setThisLocationAsTheStartingLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem addResetToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        internal System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripMenuItem addShopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox2;
        private System.Windows.Forms.ToolStripMenuItem itemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem npcToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem purgeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem thisLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem areaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem worldToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripMenuItem designexactToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem designfastToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
    }
}
