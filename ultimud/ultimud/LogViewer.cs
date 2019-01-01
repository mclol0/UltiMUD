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
	public class LogViewer : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.StatusBar statusBar1;

		public LogViewer()
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
			Hide();
			return;
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
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.SuspendLayout();
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
						| System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.BackColor = System.Drawing.Color.Black;
			this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
						this.columnHeader1,
						this.columnHeader2});
			this.listView1.ForeColor = System.Drawing.Color.White;
			this.listView1.FullRowSelect = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(472, 360);
			this.listView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listView1.TabIndex = 0;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Time";
			this.columnHeader1.Width = 129;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Message";
			this.columnHeader2.Width = 343;
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 358);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(472, 24);
			this.statusBar1.TabIndex = 1;
			// 
			// LogViewer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 382);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.listView1);
			this.Name = "LogViewer";
			this.Text = "Log";
			this.ResumeLayout(false);
		}
		#endregion

		public void OnLog(DateTime timestamp, string str, Color color) {
			if(InvokeRequired) {
				Invoke(new Server.LogHandler(OnLog), new object[] {timestamp, str, color});
			} else {
				ListViewItem item = new ListViewItem(new string[] {timestamp.ToString(), str});
				item.ForeColor = color;
				listView1.Items.Add(item);
			}
		}
	}
}
