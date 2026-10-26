using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Ultimud {
	/// <summary>
	/// Summary description for StatView.
	/// </summary>
	public class StatView : System.Windows.Forms.UserControl {
		public System.Windows.Forms.NumericUpDown statValue;
		private System.Windows.Forms.Button newStatButton;
		public Damdai.Forms.ListView stats;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public StatView() {
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			this.Init();
			if(Server.Instance != null) {
				Server.Instance.StatAddedEvent += new Server.StatAddedHandler(OnStatAdded);
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.statValue = new System.Windows.Forms.NumericUpDown();
			this.newStatButton = new System.Windows.Forms.Button();
			this.stats = new Damdai.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			((System.ComponentModel.ISupportInitialize)(this.statValue)).BeginInit();
			this.SuspendLayout();
			// 
			// statValue
			// 
			this.statValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.statValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.statValue.Location = new System.Drawing.Point(8, 176);
			this.statValue.Maximum = new System.Decimal(new int[] {
																	  100000000,
																	  0,
																	  0,
																	  0});
			this.statValue.Minimum = new System.Decimal(new int[] {
																	  100000000,
																	  0,
																	  0,
																	  -2147483648});
			this.statValue.Name = "statValue";
			this.statValue.TabIndex = 7;
			this.statValue.ThousandsSeparator = true;
			this.statValue.Visible = false;
			// 
			// newStatButton
			// 
			this.newStatButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.newStatButton.Location = new System.Drawing.Point(296, 176);
			this.newStatButton.Name = "newStatButton";
			this.newStatButton.TabIndex = 6;
			this.newStatButton.Text = "New &Stat";
			this.newStatButton.Click += new System.EventHandler(this.newStatButton_Click);
			// 
			// stats
			// 
			this.stats.AllowColumnReorder = true;
			this.stats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.stats.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					this.columnHeader1,
																					this.columnHeader2});
			this.stats.DoubleClickActivation = false;
			this.stats.FullRowSelect = true;
			this.stats.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.stats.Location = new System.Drawing.Point(0, 0);
			this.stats.MultiSelect = false;
			this.stats.Name = "stats";
			this.stats.Size = new System.Drawing.Size(376, 168);
			this.stats.TabIndex = 5;
			this.stats.View = System.Windows.Forms.View.Details;
			this.stats.SubItemEndEditing += new Damdai.Forms.SubItemEndEditingEventHandler(this.stats_SubItemEndEditing);
			this.stats.SubItemClicked += new Damdai.Forms.SubItemEventHandler(this.stats_SubItemClicked);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Stat";
			this.columnHeader1.Width = 113;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Value";
			this.columnHeader2.Width = 117;
			// 
			// StatView
			// 
			this.Controls.Add(this.statValue);
			this.Controls.Add(this.newStatButton);
			this.Controls.Add(this.stats);
			this.Name = "StatView";
			this.Size = new System.Drawing.Size(376, 200);
			((System.ComponentModel.ISupportInitialize)(this.statValue)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		public StatValue.Collection StatValues {
			get {
				StatValue.Collection statValues = new StatValue.Collection();

				foreach(ListViewItem i in this.stats.Items) {
					int val = int.Parse(i.SubItems[1].Text);
					if(val != 0) {
						Stat.Blueprint stat = i.Tag as Stat.Blueprint;
						statValues.Add(new StatValue(stat.Id, val));
					}
				}

				return statValues;
			}
		}

		public Hashtable Stats {
			get {
				Hashtable t = new Hashtable();
				foreach(ListViewItem i in stats.Items) {
					int val = int.Parse(i.SubItems[1].Text);
					if(val != 0) {
						Stat.Blueprint stat = i.Tag as Stat.Blueprint;
						t.Add(stat, val);
					}
				}
				return t;
			}
		}

		public void Init() {
			if(Server.Instance != null) {
				stats.Items.Clear();
				foreach(Stat.Blueprint stat in Server.Instance.Stats.Values) {
					AddStat(stat, 0);
				}
			}
		}
	
		private void newStatButton_Click(object sender, System.EventArgs e) {
			StatWizard editor = new StatWizard();
			editor.ShowDialog(this);
		}

		public void AddStat(Stat.Blueprint stat, decimal val) {
			ListViewItem i = new ListViewItem(new string[] {stat.Name, val.ToString()});
			i.Tag = stat;
			stats.Items.Add(i);
		}

		private void stats_SubItemClicked(object sender, Damdai.Forms.SubItemEventArgs e) {
			if(e.SubItem == 1) {
				statValue.Value = decimal.Parse(e.Item.SubItems[e.SubItem].Text);
				statValue.Select(0, (int)statValue.Maximum);
				stats.StartEditing(statValue, e.Item, e.SubItem);
			}
		}

		private void stats_SubItemEndEditing(object sender, Damdai.Forms.SubItemEndEditingEventArgs e) {
			if(e.SubItem == 1) {
				e.Item.SubItems[e.SubItem].Text = statValue.Value.ToString();
			}
		}

		public void SetStat(string name, decimal value) {
			foreach(ListViewItem i in stats.Items) {
				if(i.Text == name) {
					i.SubItems[1].Text = value.ToString();
				}
			}
		}
		void OnStatAdded(Stat.Blueprint stat) {
			if(InvokeRequired) {
				this.Invoke(new Server.StatAddedHandler(OnStatAdded), new object[] {stat});
			} else {
				this.AddStat(stat, 0);
			}
		}
	}
}
