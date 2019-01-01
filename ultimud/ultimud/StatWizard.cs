using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;

namespace Ultimud
{
	/// <summary>
	/// Summary description for StatWizard.
	/// </summary>
	public class StatWizard : System.Windows.Forms.Form
	{
		private Damdai.Forms.Wizard wizard1;
		private Damdai.Forms.WizardPage startPage;
		private Damdai.Forms.WizardPage namePage;
		private Damdai.Forms.WizardPage minMaxPage;
		private Damdai.Forms.WizardPage regenPage;
		private Damdai.Forms.WizardPage summaryPage;
		private Damdai.Forms.InfoPage infoPage1;
		private Damdai.Forms.Header header1;
		private System.Windows.Forms.TextBox name;
		private Damdai.Forms.Header header2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown min;
		private System.Windows.Forms.Label label2;
		private Damdai.Forms.Header header3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Panel statPanel;
		private Ultimud.StatView statView1;
		private System.Windows.Forms.NumericUpDown regenAmount;
		private Damdai.Forms.InfoContainer infoContainer1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.RichTextBox summary;
		private System.Windows.Forms.Panel regenPanel;
		private System.Windows.Forms.NumericUpDown max;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox onMin;
		private System.Windows.Forms.NumericUpDown defaultValue;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.RadioButton constantRegenAmount;
		private System.Windows.Forms.RadioButton variableRegenAmount;
		private System.Windows.Forms.NumericUpDown regenFrequency;
		private System.Windows.Forms.Label label9;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public StatWizard()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			statView1.stats.Columns[1].Text = "Percentage";

			ArrayList triggerActions = Object.TriggerActions;
			foreach(System.Reflection.MethodInfo method in triggerActions) {
				onMin.Items.Add(method.Name);
			}
			onMin.Tag = triggerActions;
			onMin.SelectedIndex = 0;
		}

		public StatWizard(Stat.Blueprint stat):this() {
			this.stat_ = stat;
			this.Init(stat);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(StatWizard));
			this.wizard1 = new Damdai.Forms.Wizard();
			this.regenPage = new Damdai.Forms.WizardPage();
			this.regenPanel = new System.Windows.Forms.Panel();
			this.constantRegenAmount = new System.Windows.Forms.RadioButton();
			this.variableRegenAmount = new System.Windows.Forms.RadioButton();
			this.regenAmount = new System.Windows.Forms.NumericUpDown();
			this.statPanel = new System.Windows.Forms.Panel();
			this.statView1 = new Ultimud.StatView();
			this.label5 = new System.Windows.Forms.Label();
			this.regenFrequency = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.header3 = new Damdai.Forms.Header();
			this.summaryPage = new Damdai.Forms.WizardPage();
			this.infoContainer1 = new Damdai.Forms.InfoContainer();
			this.summary = new System.Windows.Forms.RichTextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.minMaxPage = new Damdai.Forms.WizardPage();
			this.defaultValue = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.onMin = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.max = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.min = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.header2 = new Damdai.Forms.Header();
			this.namePage = new Damdai.Forms.WizardPage();
			this.name = new System.Windows.Forms.TextBox();
			this.header1 = new Damdai.Forms.Header();
			this.startPage = new Damdai.Forms.WizardPage();
			this.infoPage1 = new Damdai.Forms.InfoPage();
			this.label9 = new System.Windows.Forms.Label();
			this.wizard1.SuspendLayout();
			this.regenPage.SuspendLayout();
			this.regenPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.regenAmount)).BeginInit();
			this.statPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.regenFrequency)).BeginInit();
			this.summaryPage.SuspendLayout();
			this.infoContainer1.SuspendLayout();
			this.minMaxPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.defaultValue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.max)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.min)).BeginInit();
			this.namePage.SuspendLayout();
			this.startPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// wizard1
			// 
			this.wizard1.Controls.Add(this.summaryPage);
			this.wizard1.Controls.Add(this.regenPage);
			this.wizard1.Controls.Add(this.minMaxPage);
			this.wizard1.Controls.Add(this.namePage);
			this.wizard1.Controls.Add(this.startPage);
			this.wizard1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wizard1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.wizard1.Location = new System.Drawing.Point(0, 0);
			this.wizard1.Name = "wizard1";
			this.wizard1.Pages.AddRange(new Damdai.Forms.WizardPage[] {
																		  this.startPage,
																		  this.namePage,
																		  this.minMaxPage,
																		  this.regenPage,
																		  this.summaryPage});
			this.wizard1.Size = new System.Drawing.Size(552, 390);
			this.wizard1.TabIndex = 0;
			// 
			// regenPage
			// 
			this.regenPage.Controls.Add(this.regenPanel);
			this.regenPage.Controls.Add(this.label5);
			this.regenPage.Controls.Add(this.regenFrequency);
			this.regenPage.Controls.Add(this.label4);
			this.regenPage.Controls.Add(this.label3);
			this.regenPage.Controls.Add(this.header3);
			this.regenPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.regenPage.IsFinishPage = false;
			this.regenPage.Location = new System.Drawing.Point(0, 0);
			this.regenPage.Name = "regenPage";
			this.regenPage.Size = new System.Drawing.Size(552, 342);
			this.regenPage.TabIndex = 4;
			// 
			// regenPanel
			// 
			this.regenPanel.Controls.Add(this.constantRegenAmount);
			this.regenPanel.Controls.Add(this.variableRegenAmount);
			this.regenPanel.Controls.Add(this.regenAmount);
			this.regenPanel.Controls.Add(this.statPanel);
			this.regenPanel.Location = new System.Drawing.Point(128, 120);
			this.regenPanel.Name = "regenPanel";
			this.regenPanel.Size = new System.Drawing.Size(300, 208);
			this.regenPanel.TabIndex = 9;
			// 
			// constantRegenAmount
			// 
			this.constantRegenAmount.Location = new System.Drawing.Point(8, 8);
			this.constantRegenAmount.Name = "constantRegenAmount";
			this.constantRegenAmount.Size = new System.Drawing.Size(184, 21);
			this.constantRegenAmount.TabIndex = 2;
			this.constantRegenAmount.Text = "Constant regeneration amount.";
			this.constantRegenAmount.CheckedChanged += new System.EventHandler(this.constant_CheckedChanged);
			// 
			// variableRegenAmount
			// 
			this.variableRegenAmount.Location = new System.Drawing.Point(8, 40);
			this.variableRegenAmount.Name = "variableRegenAmount";
			this.variableRegenAmount.Size = new System.Drawing.Size(292, 16);
			this.variableRegenAmount.TabIndex = 4;
			this.variableRegenAmount.Text = "Variable regeneration amount based on player\'s stats.";
			this.variableRegenAmount.CheckedChanged += new System.EventHandler(this.variable_CheckedChanged);
			// 
			// regenAmount
			// 
			this.regenAmount.Location = new System.Drawing.Point(200, 8);
			this.regenAmount.Name = "regenAmount";
			this.regenAmount.Size = new System.Drawing.Size(64, 21);
			this.regenAmount.TabIndex = 3;
			this.regenAmount.Visible = false;
			// 
			// statPanel
			// 
			this.statPanel.Controls.Add(this.statView1);
			this.statPanel.Location = new System.Drawing.Point(8, 64);
			this.statPanel.Name = "statPanel";
			this.statPanel.Size = new System.Drawing.Size(284, 139);
			this.statPanel.TabIndex = 7;
			this.statPanel.Visible = false;
			// 
			// statView1
			// 
			this.statView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statView1.Location = new System.Drawing.Point(0, 0);
			this.statView1.Name = "statView1";
			this.statView1.Size = new System.Drawing.Size(284, 139);
			this.statView1.TabIndex = 5;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(296, 96);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 23);
			this.label5.TabIndex = 4;
			this.label5.Text = "second(s)";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// regenFrequency
			// 
			this.regenFrequency.Location = new System.Drawing.Point(248, 96);
			this.regenFrequency.Name = "regenFrequency";
			this.regenFrequency.Size = new System.Drawing.Size(48, 21);
			this.regenFrequency.TabIndex = 1;
			this.regenFrequency.ValueChanged += new System.EventHandler(this.frequency_ValueChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(208, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(32, 23);
			this.label4.TabIndex = 2;
			this.label4.Text = "Every";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(136, 96);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 23);
			this.label3.TabIndex = 1;
			this.label3.Text = "Frequency:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// header3
			// 
			this.header3.BackColor = System.Drawing.SystemColors.Control;
			this.header3.CausesValidation = false;
			this.header3.Description = "Enter the regeneration settings for this stat.";
			this.header3.Dock = System.Windows.Forms.DockStyle.Top;
			this.header3.Image = ((System.Drawing.Image)(resources.GetObject("header3.Image")));
			this.header3.Location = new System.Drawing.Point(0, 0);
			this.header3.Name = "header3";
			this.header3.Size = new System.Drawing.Size(552, 64);
			this.header3.TabIndex = 0;
			this.header3.Title = "Regen";
			// 
			// summaryPage
			// 
			this.summaryPage.Controls.Add(this.infoContainer1);
			this.summaryPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.summaryPage.IsFinishPage = false;
			this.summaryPage.Location = new System.Drawing.Point(0, 0);
			this.summaryPage.Name = "summaryPage";
			this.summaryPage.Size = new System.Drawing.Size(552, 342);
			this.summaryPage.TabIndex = 5;
			this.summaryPage.CloseFromNext += new Damdai.Forms.PageEventHandler(this.summaryPage_CloseFromNext);
			this.summaryPage.ShowFromNext += new System.EventHandler(this.summaryPage_ShowFromNext);
			// 
			// infoContainer1
			// 
			this.infoContainer1.BackColor = System.Drawing.Color.White;
			this.infoContainer1.Controls.Add(this.summary);
			this.infoContainer1.Controls.Add(this.label6);
			this.infoContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoContainer1.Image = ((System.Drawing.Image)(resources.GetObject("infoContainer1.Image")));
			this.infoContainer1.Location = new System.Drawing.Point(0, 0);
			this.infoContainer1.Name = "infoContainer1";
			this.infoContainer1.PageTitle = "Summary";
			this.infoContainer1.Size = new System.Drawing.Size(552, 342);
			this.infoContainer1.TabIndex = 0;
			// 
			// summary
			// 
			this.summary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.summary.Location = new System.Drawing.Point(192, 112);
			this.summary.Name = "summary";
			this.summary.ReadOnly = true;
			this.summary.Size = new System.Drawing.Size(336, 208);
			this.summary.TabIndex = 9;
			this.summary.Text = "";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(208, 56);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(304, 32);
			this.label6.TabIndex = 8;
			this.label6.Text = "Check the following settings. If everything is ok, click finish to exit, otherwis" +
				"e click back to make changes.";
			// 
			// minMaxPage
			// 
			this.minMaxPage.Controls.Add(this.defaultValue);
			this.minMaxPage.Controls.Add(this.label8);
			this.minMaxPage.Controls.Add(this.onMin);
			this.minMaxPage.Controls.Add(this.label7);
			this.minMaxPage.Controls.Add(this.max);
			this.minMaxPage.Controls.Add(this.label2);
			this.minMaxPage.Controls.Add(this.min);
			this.minMaxPage.Controls.Add(this.label1);
			this.minMaxPage.Controls.Add(this.header2);
			this.minMaxPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.minMaxPage.IsFinishPage = false;
			this.minMaxPage.Location = new System.Drawing.Point(0, 0);
			this.minMaxPage.Name = "minMaxPage";
			this.minMaxPage.Size = new System.Drawing.Size(552, 342);
			this.minMaxPage.TabIndex = 3;
			// 
			// defaultValue
			// 
			this.defaultValue.Location = new System.Drawing.Point(272, 192);
			this.defaultValue.Maximum = new System.Decimal(new int[] {
																		 9999999,
																		 0,
																		 0,
																		 0});
			this.defaultValue.Minimum = new System.Decimal(new int[] {
																		 9999999,
																		 0,
																		 0,
																		 -2147483648});
			this.defaultValue.Name = "defaultValue";
			this.defaultValue.Size = new System.Drawing.Size(80, 21);
			this.defaultValue.TabIndex = 3;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(200, 192);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(64, 21);
			this.label8.TabIndex = 7;
			this.label8.Text = "Default";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// onMin
			// 
			this.onMin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.onMin.Items.AddRange(new object[] {
													   "Nothing"});
			this.onMin.Location = new System.Drawing.Point(272, 224);
			this.onMin.Name = "onMin";
			this.onMin.TabIndex = 4;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(88, 224);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(176, 21);
			this.label7.TabIndex = 5;
			this.label7.Text = "When stat reaches minimum, do:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// max
			// 
			this.max.Location = new System.Drawing.Point(272, 168);
			this.max.Maximum = new System.Decimal(new int[] {
																9999999,
																0,
																0,
																0});
			this.max.Minimum = new System.Decimal(new int[] {
																9999999,
																0,
																0,
																-2147483648});
			this.max.Name = "max";
			this.max.Size = new System.Drawing.Size(80, 21);
			this.max.TabIndex = 2;
			this.max.Value = new System.Decimal(new int[] {
															  100,
															  0,
															  0,
															  0});
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(200, 168);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 21);
			this.label2.TabIndex = 3;
			this.label2.Text = "Maximum";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// min
			// 
			this.min.Location = new System.Drawing.Point(272, 144);
			this.min.Maximum = new System.Decimal(new int[] {
																9999999,
																0,
																0,
																0});
			this.min.Minimum = new System.Decimal(new int[] {
																9999999,
																0,
																0,
																-2147483648});
			this.min.Name = "min";
			this.min.Size = new System.Drawing.Size(80, 21);
			this.min.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(200, 144);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 21);
			this.label1.TabIndex = 1;
			this.label1.Text = "Minimum";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// header2
			// 
			this.header2.BackColor = System.Drawing.SystemColors.Control;
			this.header2.CausesValidation = false;
			this.header2.Description = "Enter minimum and maximum values for this stat.";
			this.header2.Dock = System.Windows.Forms.DockStyle.Top;
			this.header2.Image = ((System.Drawing.Image)(resources.GetObject("header2.Image")));
			this.header2.Location = new System.Drawing.Point(0, 0);
			this.header2.Name = "header2";
			this.header2.Size = new System.Drawing.Size(552, 64);
			this.header2.TabIndex = 0;
			this.header2.Title = "Min/Max";
			// 
			// namePage
			// 
			this.namePage.Controls.Add(this.label9);
			this.namePage.Controls.Add(this.name);
			this.namePage.Controls.Add(this.header1);
			this.namePage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.namePage.IsFinishPage = false;
			this.namePage.Location = new System.Drawing.Point(0, 0);
			this.namePage.Name = "namePage";
			this.namePage.Size = new System.Drawing.Size(552, 342);
			this.namePage.TabIndex = 2;
			this.namePage.ShowFromBack += new System.EventHandler(this.namePage_Update);
			this.namePage.ShowFromNext += new System.EventHandler(this.namePage_Update);
			// 
			// name
			// 
			this.name.Location = new System.Drawing.Point(208, 169);
			this.name.Name = "name";
			this.name.Size = new System.Drawing.Size(136, 21);
			this.name.TabIndex = 1;
			this.name.Text = "";
			this.name.TextChanged += new System.EventHandler(this.namePage_Update);
			// 
			// header1
			// 
			this.header1.BackColor = System.Drawing.SystemColors.Control;
			this.header1.CausesValidation = false;
			this.header1.Description = "Enter the name of the stat.";
			this.header1.Dock = System.Windows.Forms.DockStyle.Top;
			this.header1.Image = ((System.Drawing.Image)(resources.GetObject("header1.Image")));
			this.header1.Location = new System.Drawing.Point(0, 0);
			this.header1.Name = "header1";
			this.header1.Size = new System.Drawing.Size(552, 64);
			this.header1.TabIndex = 0;
			this.header1.Title = "Name";
			// 
			// startPage
			// 
			this.startPage.Controls.Add(this.infoPage1);
			this.startPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.startPage.IsFinishPage = false;
			this.startPage.Location = new System.Drawing.Point(0, 0);
			this.startPage.Name = "startPage";
			this.startPage.Size = new System.Drawing.Size(552, 342);
			this.startPage.TabIndex = 1;
			// 
			// infoPage1
			// 
			this.infoPage1.BackColor = System.Drawing.Color.White;
			this.infoPage1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoPage1.Image = ((System.Drawing.Image)(resources.GetObject("infoPage1.Image")));
			this.infoPage1.Location = new System.Drawing.Point(0, 0);
			this.infoPage1.Name = "infoPage1";
			this.infoPage1.PageText = "This wizard enables you to create a new stat or edit an existing stat within the " +
				"game. UltiMUD Server comes with no predefined stats, leaving you total control t" +
				"o create any stat you wish to fit within your theme, such as HP, MP, speed, ener" +
				"gy, etc.";
			this.infoPage1.PageTitle = "Welcome to the Stat Wizard";
			this.infoPage1.Size = new System.Drawing.Size(552, 342);
			this.infoPage1.TabIndex = 0;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(208, 152);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 16);
			this.label9.TabIndex = 2;
			this.label9.Text = "Name";
			// 
			// StatWizard
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(552, 390);
			this.Controls.Add(this.wizard1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "StatWizard";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Stat Wizard";
			this.wizard1.ResumeLayout(false);
			this.regenPage.ResumeLayout(false);
			this.regenPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.regenAmount)).EndInit();
			this.statPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.regenFrequency)).EndInit();
			this.summaryPage.ResumeLayout(false);
			this.infoContainer1.ResumeLayout(false);
			this.minMaxPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.defaultValue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.max)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.min)).EndInit();
			this.namePage.ResumeLayout(false);
			this.startPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		Stat.Blueprint stat_;

		public Stat.Blueprint stat {
			get { return stat_; }
		}


		void Init(Stat.Blueprint stat) {
			this.name.Text = stat.Name;
			this.min.Value = stat.Min;
			this.max.Value = stat.Max;
			this.defaultValue.Value = stat.Default;

			ArrayList triggerActions = onMin.Tag as ArrayList;
			for(int x = 0; x < triggerActions.Count; ++x) {
				if(stat.onMin.Method == triggerActions[x]) {
					onMin.SelectedIndex = x+1;
					break;
				}
			}

			this.regenFrequency.Value = stat.RegenFrequency;
			if(stat.RegenAmount != 0) {
				this.constantRegenAmount.Checked = true;
				this.regenAmount.Value = stat.RegenAmount;
			} else {
				this.variableRegenAmount.Checked = true;
				StatValue.Collection statValues = stat.RegenFactors;
				if(statValues != null) {
					foreach(StatValue statValue in statValues) {
						Stat.Blueprint bp = Server.Instance.GetStat(statValue.StatId);
						if(bp != null) {
							statView1.AddStat(bp, statValue.Value);
						}
					}
				}
			}
		}

		private void constant_CheckedChanged(object sender, System.EventArgs e) {
			regenAmount.Visible = this.constantRegenAmount.Checked;
		}

		private void variable_CheckedChanged(object sender, System.EventArgs e) {
			statPanel.Visible = this.variableRegenAmount.Checked;
		}

		private void namePage_Update(object sender, System.EventArgs e) {
			wizard1.NextEnabled = name.Text.Trim().Length > 0;
		}

		private void frequency_ValueChanged(object sender, System.EventArgs e) {
			this.regenPanel.Enabled = (this.regenFrequency.Value != 0);
		}

		private void summaryPage_ShowFromNext(object sender, System.EventArgs e) {
			summary.Text = "Name: " + name.Text.Trim();

			summary.Text += "\r\nMin/Max: " + min.Value + "/" + max.Value;
			summary.Text += "\r\nDefault: " + defaultValue.Value;

			summary.Text += "\r\nRegen Frequency: every " + this.regenFrequency.Value + " second(s)";

			if(this.constantRegenAmount.Checked) {
				summary.Text += "\r\nRegen Amount: " + regenAmount.Value;
			} else if(this.variableRegenAmount.Checked) {
				summary.Text += "\r\nRegen Factors:";
				foreach(DictionaryEntry de in statView1.Stats) {
					Stat.Blueprint bp = de.Key as Stat.Blueprint;
					int percentage = (int)de.Value;
					summary.Text += "\r\n\t" + bp.Name + " " + percentage + "%";
				}
			}

			summary.Text += "\r\nWhat stat reaches minimum, do: " + onMin.Text;
		}

		private void summaryPage_CloseFromNext(object sender, Damdai.Forms.PageEventArgs e) {
			Stat.Blueprint stat = new Stat.Blueprint();
			stat.Name = this.name.Text.Trim();
			stat.Min = (int)this.min.Value;
			stat.Max = (int)this.max.Value;
			stat.Default = (int)this.defaultValue.Value;
			stat.RegenFrequency = (int)this.regenFrequency.Value;
			
			if(this.constantRegenAmount.Checked) {
				stat.RegenAmount = (int)this.regenAmount.Value;
			} else if(this.variableRegenAmount.Checked) {
				stat.RegenFactors = this.statView1.StatValues;
			}

			if(onMin.SelectedIndex > 0) {
				ArrayList triggerActions = this.onMin.Tag as ArrayList;
				MethodInfo triggerAction = triggerActions[onMin.SelectedIndex-1] as MethodInfo;
				stat.onMin = Delegate.CreateDelegate(typeof(Object.TriggerAction), triggerAction) as Object.TriggerAction;
			}

			try {
				if(this.stat_ != null) {
					Server.Instance.AddStat(stat_, stat);
				} else {
					Server.Instance.AddStat(stat);
				}
			} catch(Exception ex) {
				MessageBox.Show(this, ex.Message, "Error");
				e.Page = this.namePage;
			}

			stat_ = stat;
		}
	}
}
