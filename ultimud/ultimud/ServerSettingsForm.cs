using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Ultimud
{
	/// <summary>
	/// Summary description for ServerSettingsForm.
	/// </summary>
	public class ServerSettingsForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.RichTextBox titleScreen;
		private Damdai.Forms.ListView colors;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ComboBox color;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ServerSettingsForm(ServerSettings settings)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//TODO: init color settings
			foreach(string color in Enum.GetNames(typeof(Telnet.Color))) {
				this.color.Items.Add(color);
			}

			this.settings_ = settings;
			this.titleScreen.Text = settings.TitleScreen;

			string[] colorCategories = Enum.GetNames(typeof(ServerSettings.Color));
			for(int x = 0; x < colorCategories.Length; ++x) {
				Telnet.Color color = (Telnet.Color)this.settings_.Colors[(ServerSettings.Color)x];
				ListViewItem i = new ListViewItem(new string[] {colorCategories[x], color.ToString()});
				i.Tag = color;
				this.colors.Items.Add(i);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.titleScreen = new System.Windows.Forms.RichTextBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.color = new System.Windows.Forms.ComboBox();
			this.colors = new Damdai.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.panel1 = new System.Windows.Forms.Panel();
			this.cancelButton = new System.Windows.Forms.Button();
			this.saveButton = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(512, 326);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.titleScreen);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(504, 300);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Title Screen";
			// 
			// titleScreen
			// 
			this.titleScreen.BackColor = System.Drawing.Color.Black;
			this.titleScreen.Dock = System.Windows.Forms.DockStyle.Fill;
			this.titleScreen.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.titleScreen.ForeColor = System.Drawing.Color.White;
			this.titleScreen.Location = new System.Drawing.Point(0, 0);
			this.titleScreen.Name = "titleScreen";
			this.titleScreen.Size = new System.Drawing.Size(504, 300);
			this.titleScreen.TabIndex = 2;
			this.titleScreen.Text = @"'||'  '|' '||    .    ||  '||    ||' '||'  '|' '||''|.
 ||    |   ||  .||.  ...   |||  |||   ||    |   ||   ||
 ||    |   ||   ||    ||   |'|..'||   ||    |   ||    ||
 ||    |   ||   ||    ||   | '|' ||   ||    |   ||    ||
  '|..'   .||.  '|.' .||. .|. | .||.   '|..'   .||...|'
Welcome to UltiMUD";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.color);
			this.tabPage2.Controls.Add(this.colors);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(504, 300);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Colors";
			// 
			// color
			// 
			this.color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.color.Location = new System.Drawing.Point(376, 272);
			this.color.Name = "color";
			this.color.Size = new System.Drawing.Size(121, 21);
			this.color.TabIndex = 1;
			this.color.Visible = false;
			// 
			// colors
			// 
			this.colors.AllowColumnReorder = true;
			this.colors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					 this.columnHeader1,
																					 this.columnHeader2});
			this.colors.Dock = System.Windows.Forms.DockStyle.Fill;
			this.colors.DoubleClickActivation = false;
			this.colors.FullRowSelect = true;
			this.colors.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.colors.Location = new System.Drawing.Point(0, 0);
			this.colors.Name = "colors";
			this.colors.Size = new System.Drawing.Size(504, 300);
			this.colors.TabIndex = 0;
			this.colors.View = System.Windows.Forms.View.Details;
			this.colors.SubItemEndEditing += new Damdai.Forms.SubItemEndEditingEventHandler(this.colors_SubItemEndEditing);
			this.colors.SubItemClicked += new Damdai.Forms.SubItemEventHandler(this.colors_SubItemClicked);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Category";
			this.columnHeader1.Width = 137;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Color";
			this.columnHeader2.Width = 153;
			// 
			// tabPage3
			// 
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(504, 300);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Other";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this.saveButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 326);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(512, 40);
			this.panel1.TabIndex = 1;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(424, 8);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(336, 8);
			this.saveButton.Name = "saveButton";
			this.saveButton.TabIndex = 0;
			this.saveButton.Text = "&Save";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// ServerSettingsForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(512, 366);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ServerSettingsForm";
			this.Text = "Settings";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		bool saved_;
		ServerSettings settings_;

		override protected void OnClosing(CancelEventArgs e) {
			if(saved_ == false) {
				switch(MessageBox.Show("Save changes?", "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) {
				case DialogResult.Yes:
					Save();
					break;
				}
			}
			base.OnClosing(e);
		}

		void Save() {
			//TODO: save ServerSettings object
			this.settings_.TitleScreen = this.titleScreen.Text;
			foreach(ListViewItem i in this.colors.Items) {
				this.settings_.Colors[(ServerSettings.Color)i.Index] = (Telnet.Color)i.Tag;
			}
			saved_ = true;
		}

		private void saveButton_Click(object sender, System.EventArgs e) {
			Save();
		}

		private void cancelButton_Click(object sender, System.EventArgs e) {
			saved_ = true;
			this.Close();
		}

		private void colors_SubItemClicked(object sender, Damdai.Forms.SubItemEventArgs e) {
			if(e.SubItem == 1) {
				this.color.SelectedIndex = (int)(Telnet.Color)e.Item.Tag;
				this.colors.StartEditing(this.color, e.Item, e.SubItem);
			}		
		}

		private void colors_SubItemEndEditing(object sender, Damdai.Forms.SubItemEndEditingEventArgs e) {
			if(e.SubItem == 1) {
				Telnet.Color color = (Telnet.Color)this.color.SelectedIndex;
				e.Item.SubItems[e.SubItem].Text = color.ToString();
				e.Item.Tag = color;
			}
		}
	}
}
