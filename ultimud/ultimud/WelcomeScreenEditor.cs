using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Ultimud {
	/// <summary>
	/// Summary description for WinForm.
	/// </summary>
	public class WelcomeScreenEditor : System.Windows.Forms.Form
	{
		bool saved_ = true;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.RichTextBox textBox;

		private string welcomeScreen_;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ImageList imageList1;

		public string welcomeScreen {
			get { return welcomeScreen_; }
			set {
				welcomeScreen_ = value;
				textBox.Text = value;
				statusBar1.Text = textBox.Text.Length + " bytes";
			}
		}

		public WelcomeScreenEditor()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WelcomeScreenEditor));
			this.textBox = new System.Windows.Forms.RichTextBox();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// textBox
			// 
			this.textBox.BackColor = System.Drawing.Color.Black;
			this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox.ForeColor = System.Drawing.Color.White;
			this.textBox.Location = new System.Drawing.Point(0, 28);
			this.textBox.Name = "textBox";
			this.textBox.Size = new System.Drawing.Size(552, 340);
			this.textBox.TabIndex = 1;
			this.textBox.Text = "Welcome to UltiMUD";
			this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 368);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(552, 22);
			this.statusBar1.TabIndex = 3;
			// 
			// toolBar1
			// 
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
						this.toolBarButton1,
						this.toolBarButton2});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.ImageList = this.imageList1;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(552, 28);
			this.toolBar1.TabIndex = 4;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.ImageIndex = 0;
			this.toolBarButton1.ToolTipText = "Save";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.ImageIndex = 1;
			this.toolBarButton2.ToolTipText = "Cancel";
			// 
			// imageList1
			// 
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.White;
			// 
			// WelcomeScreenEditor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(552, 390);
			this.Controls.Add(this.textBox);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.toolBar1);
			this.Name = "WelcomeScreenEditor";
			this.Text = "Edit welcome screen...";
			this.ResumeLayout(false);
		}
		#endregion

		private void textBox_TextChanged(object sender, System.EventArgs e) {
			statusBar1.Text = textBox.TextLength + " bytes";
			saved_ = false;
		}

		void Save() {
			welcomeScreen_ = textBox.Text;
			saved_ = true;
		}

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

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e) {
			switch(toolBar1.Buttons.IndexOf(e.Button)) {
			case 0:
				Save();
				break;
			case 1:
				saved_ = true;
				Close();
				break;
			}
		}
	}
}
