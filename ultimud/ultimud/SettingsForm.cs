using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace dcd
{
	/// <summary>
	/// Summary description for WinForm.
	/// </summary>
	public class SettingsForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox wizlock;
		private System.Windows.Forms.CheckBox allowNewAccounts;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label2;

		public SettingsForm()
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
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.wizlock = new System.Windows.Forms.CheckBox();
			this.allowNewAccounts = new System.Windows.Forms.CheckBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Starting location";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// wizlock
			// 
			this.wizlock.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.wizlock.Location = new System.Drawing.Point(8, 32);
			this.wizlock.Name = "wizlock";
			this.wizlock.Size = new System.Drawing.Size(240, 24);
			this.wizlock.TabIndex = 3;
			this.wizlock.Text = "Restrict login to game administrators only";
			// 
			// allowNewAccounts
			// 
			this.allowNewAccounts.Checked = true;
			this.allowNewAccounts.CheckState = System.Windows.Forms.CheckState.Checked;
			this.allowNewAccounts.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.allowNewAccounts.Location = new System.Drawing.Point(8, 8);
			this.allowNewAccounts.Name = "allowNewAccounts";
			this.allowNewAccounts.Size = new System.Drawing.Size(224, 24);
			this.allowNewAccounts.TabIndex = 4;
			this.allowNewAccounts.Text = "Allow new player accounts to be created";
			// 
			// comboBox1
			// 
			this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox1.Items.AddRange(new object[] {
						"Choose another location..."});
			this.comboBox1.Location = new System.Drawing.Point(104, 64);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(176, 21);
			this.comboBox1.TabIndex = 5;
			// 
			// comboBox2
			// 
			this.comboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox2.Items.AddRange(new object[] {
						"Automated",
						"Turn-based",
						"Real-time"});
			this.comboBox2.Location = new System.Drawing.Point(104, 88);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(176, 21);
			this.comboBox2.TabIndex = 7;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 20);
			this.label2.TabIndex = 6;
			this.label2.Text = "Combat style";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SettingsForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 118);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.allowNewAccounts);
			this.Controls.Add(this.wizlock);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.Name = "SettingsForm";
			this.Text = "Settings";
			this.ResumeLayout(false);
		}
		#endregion

	}
}
