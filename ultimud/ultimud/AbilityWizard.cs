using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Ultimud
{
	/// <summary>
	/// Summary description for AbilityWizard.
	/// </summary>
	public class AbilityWizard : System.Windows.Forms.Form
	{
		private Damdai.Forms.Wizard wizard1;
		private Damdai.Forms.InfoPage infoPage1;
		private Damdai.Forms.Header header1;
		private System.Windows.Forms.TextBox name;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label nameStatus;
		private Damdai.Forms.WizardPage startPage;
		private Damdai.Forms.WizardPage namePage;
		private Damdai.Forms.WizardPage messagePage;
		private Damdai.Forms.WizardPage rangePage;
		private Damdai.Forms.WizardPage statCostPage;
		private Damdai.Forms.WizardPage statModsPage;
		private Damdai.Forms.WizardPage summaryPage;
		private Damdai.Forms.Header header2;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private Damdai.Forms.Header header3;
		private Damdai.Forms.Header header4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown range;
		private System.Windows.Forms.NumericUpDown area;
		private Damdai.Forms.Header header5;
		private Damdai.Forms.InfoContainer infoContainer1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.RichTextBox summary;
		private System.Windows.Forms.Label label5;
		private Damdai.Forms.WizardPage prepareRecoverPage;
		private System.Windows.Forms.NumericUpDown recover;
		private System.Windows.Forms.NumericUpDown prepare;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private Damdai.Forms.Header header6;
		private System.Windows.Forms.TextBox prepareMessage;
		private System.Windows.Forms.Label prepareLabel;
		private System.Windows.Forms.TextBox performMessage;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox failMessage;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox missMessage;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox hitMessage;
		private Damdai.Forms.WizardPage requirementsPage;
		private Damdai.Forms.WizardPage improvementsPage;
		private Damdai.Forms.Header header7;
		private System.Windows.Forms.CheckedListBox requirements;
		private Damdai.Forms.Header header8;
		private Ultimud.StatView statCost;
		private Ultimud.StatView improvements;
		private Ultimud.StatView targetStatMods;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AbilityWizard()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			foreach(Ability a in Server.Instance.Abilities.Values) {
				this.requirements.Items.Add(a.Name);
			}

			improvements.stats.Columns[1].Text = "Chance";
			improvements.statValue.Maximum = 100;
			improvements.statValue.Minimum = 0;
		}

		public AbilityWizard(Ability a):this() {
			ability_ = a;
			name.Text = a.Name;
			prepare.Value = a.PrepareTime;
			recover.Value = a.RecoverTime;
			prepareMessage.Text = a.PrepareMessage;
			performMessage.Text = a.PerformMessage;
			failMessage.Text = a.FailMessage;
			missMessage.Text = a.MissMessage;
			hitMessage.Text = a.HitMessage;
			range.Value = a.Range;
			area.Value = a.AreaEffect;

			if(ability_.StatCost != null) {
				foreach(DictionaryEntry de in ability_.StatCost) {
					statCost.SetStat((string)de.Key, (int)de.Value);
				}
			}
			if(ability_.TargetStatModifications != null) {
				foreach(DictionaryEntry de in ability_.TargetStatModifications) {
					Stat.Blueprint stat = Server.Instance.GetStat((Guid)de.Key);
					targetStatMods.SetStat(stat.Name, (decimal)de.Value);
				}
			}
			if(ability_.Requirements != null) {
				foreach(Guid abilityID in ability_.Requirements) {
					Ability ability = Server.Instance.GetAbility(abilityID);
					if(ability != null) {
						this.requirements.SetItemChecked(this.requirements.FindString(ability.Name), true);
					}
				}
			}
			if(ability_.Improvements != null) {
				foreach(DictionaryEntry de in ability_.Improvements) {
					Guid statID = (Guid)de.Key;
					int chance = (int)de.Value;
					Stat.Blueprint stat = Server.Instance.GetStat(statID);
					if(stat != null) {
						this.improvements.SetStat(stat.Name, (decimal)chance);
					}
				}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AbilityWizard));
			this.wizard1 = new Damdai.Forms.Wizard();
			this.statModsPage = new Damdai.Forms.WizardPage();
			this.targetStatMods = new Ultimud.StatView();
			this.header5 = new Damdai.Forms.Header();
			this.statCostPage = new Damdai.Forms.WizardPage();
			this.statCost = new Ultimud.StatView();
			this.header3 = new Damdai.Forms.Header();
			this.rangePage = new Damdai.Forms.WizardPage();
			this.area = new System.Windows.Forms.NumericUpDown();
			this.range = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.header4 = new Damdai.Forms.Header();
			this.messagePage = new Damdai.Forms.WizardPage();
			this.label10 = new System.Windows.Forms.Label();
			this.hitMessage = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.failMessage = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.missMessage = new System.Windows.Forms.TextBox();
			this.prepareLabel = new System.Windows.Forms.Label();
			this.prepareMessage = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.performMessage = new System.Windows.Forms.TextBox();
			this.header2 = new Damdai.Forms.Header();
			this.prepareRecoverPage = new Damdai.Forms.WizardPage();
			this.header6 = new Damdai.Forms.Header();
			this.recover = new System.Windows.Forms.NumericUpDown();
			this.prepare = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.namePage = new Damdai.Forms.WizardPage();
			this.nameStatus = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.name = new System.Windows.Forms.TextBox();
			this.header1 = new Damdai.Forms.Header();
			this.startPage = new Damdai.Forms.WizardPage();
			this.infoPage1 = new Damdai.Forms.InfoPage();
			this.summaryPage = new Damdai.Forms.WizardPage();
			this.infoContainer1 = new Damdai.Forms.InfoContainer();
			this.summary = new System.Windows.Forms.RichTextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.improvementsPage = new Damdai.Forms.WizardPage();
			this.improvements = new Ultimud.StatView();
			this.header8 = new Damdai.Forms.Header();
			this.requirementsPage = new Damdai.Forms.WizardPage();
			this.requirements = new System.Windows.Forms.CheckedListBox();
			this.header7 = new Damdai.Forms.Header();
			this.wizard1.SuspendLayout();
			this.statModsPage.SuspendLayout();
			this.statCostPage.SuspendLayout();
			this.rangePage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.area)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.range)).BeginInit();
			this.messagePage.SuspendLayout();
			this.prepareRecoverPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.recover)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.prepare)).BeginInit();
			this.namePage.SuspendLayout();
			this.startPage.SuspendLayout();
			this.summaryPage.SuspendLayout();
			this.infoContainer1.SuspendLayout();
			this.improvementsPage.SuspendLayout();
			this.requirementsPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// wizard1
			// 
			this.wizard1.Controls.Add(this.messagePage);
			this.wizard1.Controls.Add(this.prepareRecoverPage);
			this.wizard1.Controls.Add(this.namePage);
			this.wizard1.Controls.Add(this.startPage);
			this.wizard1.Controls.Add(this.summaryPage);
			this.wizard1.Controls.Add(this.improvementsPage);
			this.wizard1.Controls.Add(this.requirementsPage);
			this.wizard1.Controls.Add(this.statModsPage);
			this.wizard1.Controls.Add(this.statCostPage);
			this.wizard1.Controls.Add(this.rangePage);
			this.wizard1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wizard1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.wizard1.Location = new System.Drawing.Point(0, 0);
			this.wizard1.Name = "wizard1";
			this.wizard1.Pages.AddRange(new Damdai.Forms.WizardPage[] {
																		  this.startPage,
																		  this.namePage,
																		  this.prepareRecoverPage,
																		  this.messagePage,
																		  this.rangePage,
																		  this.statCostPage,
																		  this.statModsPage,
																		  this.requirementsPage,
																		  this.improvementsPage,
																		  this.summaryPage});
			this.wizard1.Size = new System.Drawing.Size(520, 416);
			this.wizard1.TabIndex = 0;
			// 
			// statModsPage
			// 
			this.statModsPage.Controls.Add(this.targetStatMods);
			this.statModsPage.Controls.Add(this.header5);
			this.statModsPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statModsPage.IsFinishPage = false;
			this.statModsPage.Location = new System.Drawing.Point(0, 0);
			this.statModsPage.Name = "statModsPage";
			this.statModsPage.Size = new System.Drawing.Size(520, 368);
			this.statModsPage.TabIndex = 6;
			// 
			// targetStatMods
			// 
			this.targetStatMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.targetStatMods.Location = new System.Drawing.Point(24, 88);
			this.targetStatMods.Name = "targetStatMods";
			this.targetStatMods.Size = new System.Drawing.Size(472, 264);
			this.targetStatMods.TabIndex = 6;
			// 
			// header5
			// 
			this.header5.BackColor = System.Drawing.SystemColors.Control;
			this.header5.CausesValidation = false;
			this.header5.Description = "Set the target stat modifiers of this ability.";
			this.header5.Dock = System.Windows.Forms.DockStyle.Top;
			this.header5.Image = ((System.Drawing.Image)(resources.GetObject("header5.Image")));
			this.header5.Location = new System.Drawing.Point(0, 0);
			this.header5.Name = "header5";
			this.header5.Size = new System.Drawing.Size(520, 64);
			this.header5.TabIndex = 5;
			this.header5.Title = "Target Stat Modifiers";
			// 
			// statCostPage
			// 
			this.statCostPage.Controls.Add(this.statCost);
			this.statCostPage.Controls.Add(this.header3);
			this.statCostPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statCostPage.IsFinishPage = false;
			this.statCostPage.Location = new System.Drawing.Point(0, 0);
			this.statCostPage.Name = "statCostPage";
			this.statCostPage.Size = new System.Drawing.Size(520, 368);
			this.statCostPage.TabIndex = 5;
			// 
			// statCost
			// 
			this.statCost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.statCost.Location = new System.Drawing.Point(24, 88);
			this.statCost.Name = "statCost";
			this.statCost.Size = new System.Drawing.Size(472, 264);
			this.statCost.TabIndex = 4;
			// 
			// header3
			// 
			this.header3.BackColor = System.Drawing.SystemColors.Control;
			this.header3.CausesValidation = false;
			this.header3.Description = "Set the stat cost, if any.";
			this.header3.Dock = System.Windows.Forms.DockStyle.Top;
			this.header3.Image = ((System.Drawing.Image)(resources.GetObject("header3.Image")));
			this.header3.Location = new System.Drawing.Point(0, 0);
			this.header3.Name = "header3";
			this.header3.Size = new System.Drawing.Size(520, 64);
			this.header3.TabIndex = 3;
			this.header3.Title = "Stat Cost";
			// 
			// rangePage
			// 
			this.rangePage.Controls.Add(this.area);
			this.rangePage.Controls.Add(this.range);
			this.rangePage.Controls.Add(this.label3);
			this.rangePage.Controls.Add(this.label2);
			this.rangePage.Controls.Add(this.header4);
			this.rangePage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rangePage.IsFinishPage = false;
			this.rangePage.Location = new System.Drawing.Point(0, 0);
			this.rangePage.Name = "rangePage";
			this.rangePage.Size = new System.Drawing.Size(520, 368);
			this.rangePage.TabIndex = 4;
			// 
			// area
			// 
			this.area.Location = new System.Drawing.Point(272, 182);
			this.area.Maximum = new System.Decimal(new int[] {
																 20,
																 0,
																 0,
																 0});
			this.area.Name = "area";
			this.area.Size = new System.Drawing.Size(56, 21);
			this.area.TabIndex = 4;
			// 
			// range
			// 
			this.range.Location = new System.Drawing.Point(272, 150);
			this.range.Maximum = new System.Decimal(new int[] {
																  20,
																  0,
																  0,
																  0});
			this.range.Name = "range";
			this.range.Size = new System.Drawing.Size(56, 21);
			this.range.TabIndex = 3;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(192, 182);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 21);
			this.label3.TabIndex = 2;
			this.label3.Text = "Area Effect";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(192, 150);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 21);
			this.label2.TabIndex = 1;
			this.label2.Text = "Range";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// header4
			// 
			this.header4.BackColor = System.Drawing.SystemColors.Control;
			this.header4.CausesValidation = false;
			this.header4.Description = "Set the range and area effect of this ability.";
			this.header4.Dock = System.Windows.Forms.DockStyle.Top;
			this.header4.Image = ((System.Drawing.Image)(resources.GetObject("header4.Image")));
			this.header4.Location = new System.Drawing.Point(0, 0);
			this.header4.Name = "header4";
			this.header4.Size = new System.Drawing.Size(520, 64);
			this.header4.TabIndex = 0;
			this.header4.Title = "Range/Area Effect";
			// 
			// messagePage
			// 
			this.messagePage.Controls.Add(this.label10);
			this.messagePage.Controls.Add(this.hitMessage);
			this.messagePage.Controls.Add(this.label6);
			this.messagePage.Controls.Add(this.failMessage);
			this.messagePage.Controls.Add(this.label9);
			this.messagePage.Controls.Add(this.missMessage);
			this.messagePage.Controls.Add(this.prepareLabel);
			this.messagePage.Controls.Add(this.prepareMessage);
			this.messagePage.Controls.Add(this.label5);
			this.messagePage.Controls.Add(this.richTextBox1);
			this.messagePage.Controls.Add(this.performMessage);
			this.messagePage.Controls.Add(this.header2);
			this.messagePage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagePage.IsFinishPage = false;
			this.messagePage.Location = new System.Drawing.Point(0, 0);
			this.messagePage.Name = "messagePage";
			this.messagePage.Size = new System.Drawing.Size(520, 368);
			this.messagePage.TabIndex = 3;
			this.messagePage.CloseFromNext += new Damdai.Forms.PageEventHandler(this.messagePage_CloseFromNext);
			this.messagePage.ShowFromNext += new System.EventHandler(this.messagePage_ShowFromNext);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(68, 336);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(92, 21);
			this.label10.TabIndex = 11;
			this.label10.Text = "Hit Message";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// hitMessage
			// 
			this.hitMessage.Location = new System.Drawing.Point(172, 336);
			this.hitMessage.Name = "hitMessage";
			this.hitMessage.Size = new System.Drawing.Size(280, 21);
			this.hitMessage.TabIndex = 5;
			this.hitMessage.Text = "{1} hit<s> {2}!";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(68, 288);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(96, 21);
			this.label6.TabIndex = 9;
			this.label6.Text = "Fail Message";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// failMessage
			// 
			this.failMessage.Location = new System.Drawing.Point(172, 288);
			this.failMessage.Name = "failMessage";
			this.failMessage.Size = new System.Drawing.Size(280, 21);
			this.failMessage.TabIndex = 3;
			this.failMessage.Text = "";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(68, 312);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(92, 21);
			this.label9.TabIndex = 8;
			this.label9.Text = "Miss Message";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// missMessage
			// 
			this.missMessage.Location = new System.Drawing.Point(172, 312);
			this.missMessage.Name = "missMessage";
			this.missMessage.Size = new System.Drawing.Size(280, 21);
			this.missMessage.TabIndex = 4;
			this.missMessage.Text = "{1} miss<es> {2}!";
			// 
			// prepareLabel
			// 
			this.prepareLabel.Location = new System.Drawing.Point(68, 240);
			this.prepareLabel.Name = "prepareLabel";
			this.prepareLabel.Size = new System.Drawing.Size(96, 21);
			this.prepareLabel.TabIndex = 5;
			this.prepareLabel.Text = "Prepare Message";
			this.prepareLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// prepareMessage
			// 
			this.prepareMessage.Location = new System.Drawing.Point(172, 240);
			this.prepareMessage.Name = "prepareMessage";
			this.prepareMessage.Size = new System.Drawing.Size(280, 21);
			this.prepareMessage.TabIndex = 1;
			this.prepareMessage.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(68, 264);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(92, 21);
			this.label5.TabIndex = 3;
			this.label5.Text = "Perform Message";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// richTextBox1
			// 
			this.richTextBox1.BackColor = System.Drawing.SystemColors.Control;
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Enabled = false;
			this.richTextBox1.ForeColor = System.Drawing.SystemColors.InactiveCaption;
			this.richTextBox1.Location = new System.Drawing.Point(62, 80);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(396, 144);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = @"Variables:
	{1}		Caster
	{2}		Target
	<whatever>	Include text in brackets if message is not to caster.

Example:	{1} kick<s> {2}!

Will display the following:
	To caster:	You kick [target name]!
	To target:	[caster name] kicks you!
	To room:		[caster name] kicks [target name]!";
			// 
			// performMessage
			// 
			this.performMessage.Location = new System.Drawing.Point(172, 264);
			this.performMessage.Name = "performMessage";
			this.performMessage.Size = new System.Drawing.Size(280, 21);
			this.performMessage.TabIndex = 2;
			this.performMessage.Text = "";
			// 
			// header2
			// 
			this.header2.BackColor = System.Drawing.SystemColors.Control;
			this.header2.CausesValidation = false;
			this.header2.Description = "Enter the message that is displayed when this ability is used.";
			this.header2.Dock = System.Windows.Forms.DockStyle.Top;
			this.header2.Image = ((System.Drawing.Image)(resources.GetObject("header2.Image")));
			this.header2.Location = new System.Drawing.Point(0, 0);
			this.header2.Name = "header2";
			this.header2.Size = new System.Drawing.Size(520, 64);
			this.header2.TabIndex = 0;
			this.header2.Title = "Message";
			// 
			// prepareRecoverPage
			// 
			this.prepareRecoverPage.Controls.Add(this.header6);
			this.prepareRecoverPage.Controls.Add(this.recover);
			this.prepareRecoverPage.Controls.Add(this.prepare);
			this.prepareRecoverPage.Controls.Add(this.label7);
			this.prepareRecoverPage.Controls.Add(this.label8);
			this.prepareRecoverPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.prepareRecoverPage.IsFinishPage = false;
			this.prepareRecoverPage.Location = new System.Drawing.Point(0, 0);
			this.prepareRecoverPage.Name = "prepareRecoverPage";
			this.prepareRecoverPage.Size = new System.Drawing.Size(520, 368);
			this.prepareRecoverPage.TabIndex = 8;
			// 
			// header6
			// 
			this.header6.BackColor = System.Drawing.SystemColors.Control;
			this.header6.CausesValidation = false;
			this.header6.Description = "Set the prepare and recover times of this ability.";
			this.header6.Dock = System.Windows.Forms.DockStyle.Top;
			this.header6.Image = ((System.Drawing.Image)(resources.GetObject("header6.Image")));
			this.header6.Location = new System.Drawing.Point(0, 0);
			this.header6.Name = "header6";
			this.header6.Size = new System.Drawing.Size(520, 64);
			this.header6.TabIndex = 9;
			this.header6.Title = "Prepare/Recover Times";
			// 
			// recover
			// 
			this.recover.Location = new System.Drawing.Point(272, 182);
			this.recover.Maximum = new System.Decimal(new int[] {
																	20,
																	0,
																	0,
																	0});
			this.recover.Name = "recover";
			this.recover.Size = new System.Drawing.Size(56, 21);
			this.recover.TabIndex = 8;
			// 
			// prepare
			// 
			this.prepare.Location = new System.Drawing.Point(272, 150);
			this.prepare.Maximum = new System.Decimal(new int[] {
																	20,
																	0,
																	0,
																	0});
			this.prepare.Name = "prepare";
			this.prepare.Size = new System.Drawing.Size(56, 21);
			this.prepare.TabIndex = 7;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(192, 182);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(72, 21);
			this.label7.TabIndex = 6;
			this.label7.Text = "Recover";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(192, 150);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(72, 21);
			this.label8.TabIndex = 5;
			this.label8.Text = "Prepare";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// namePage
			// 
			this.namePage.Controls.Add(this.nameStatus);
			this.namePage.Controls.Add(this.label1);
			this.namePage.Controls.Add(this.name);
			this.namePage.Controls.Add(this.header1);
			this.namePage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.namePage.IsFinishPage = false;
			this.namePage.Location = new System.Drawing.Point(0, 0);
			this.namePage.Name = "namePage";
			this.namePage.Size = new System.Drawing.Size(520, 368);
			this.namePage.TabIndex = 2;
			this.namePage.CloseFromNext += new Damdai.Forms.PageEventHandler(this.namePage_CloseFromNext);
			// 
			// nameStatus
			// 
			this.nameStatus.ForeColor = System.Drawing.Color.Red;
			this.nameStatus.Location = new System.Drawing.Point(176, 184);
			this.nameStatus.Name = "nameStatus";
			this.nameStatus.Size = new System.Drawing.Size(176, 56);
			this.nameStatus.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(172, 141);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Name";
			// 
			// name
			// 
			this.name.Location = new System.Drawing.Point(172, 157);
			this.name.MaxLength = 20;
			this.name.Name = "name";
			this.name.Size = new System.Drawing.Size(176, 21);
			this.name.TabIndex = 1;
			this.name.Text = "";
			// 
			// header1
			// 
			this.header1.BackColor = System.Drawing.SystemColors.Control;
			this.header1.CausesValidation = false;
			this.header1.Description = "Enter the name of this ability.";
			this.header1.Dock = System.Windows.Forms.DockStyle.Top;
			this.header1.Image = ((System.Drawing.Image)(resources.GetObject("header1.Image")));
			this.header1.Location = new System.Drawing.Point(0, 0);
			this.header1.Name = "header1";
			this.header1.Size = new System.Drawing.Size(520, 64);
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
			this.startPage.Size = new System.Drawing.Size(520, 368);
			this.startPage.TabIndex = 1;
			// 
			// infoPage1
			// 
			this.infoPage1.BackColor = System.Drawing.Color.White;
			this.infoPage1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoPage1.Image = ((System.Drawing.Image)(resources.GetObject("infoPage1.Image")));
			this.infoPage1.Location = new System.Drawing.Point(0, 0);
			this.infoPage1.Name = "infoPage1";
			this.infoPage1.PageText = "This wizard enables you to edit an existing ability or create a new one.";
			this.infoPage1.PageTitle = "Welcome to the Ability Wizard";
			this.infoPage1.Size = new System.Drawing.Size(520, 368);
			this.infoPage1.TabIndex = 0;
			// 
			// summaryPage
			// 
			this.summaryPage.Controls.Add(this.infoContainer1);
			this.summaryPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.summaryPage.IsFinishPage = false;
			this.summaryPage.Location = new System.Drawing.Point(0, 0);
			this.summaryPage.Name = "summaryPage";
			this.summaryPage.Size = new System.Drawing.Size(520, 368);
			this.summaryPage.TabIndex = 7;
			this.summaryPage.CloseFromNext += new Damdai.Forms.PageEventHandler(this.summaryPage_CloseFromNext);
			this.summaryPage.ShowFromNext += new System.EventHandler(this.summaryPage_ShowFromNext);
			// 
			// infoContainer1
			// 
			this.infoContainer1.BackColor = System.Drawing.Color.White;
			this.infoContainer1.Controls.Add(this.summary);
			this.infoContainer1.Controls.Add(this.label4);
			this.infoContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoContainer1.Image = ((System.Drawing.Image)(resources.GetObject("infoContainer1.Image")));
			this.infoContainer1.Location = new System.Drawing.Point(0, 0);
			this.infoContainer1.Name = "infoContainer1";
			this.infoContainer1.PageTitle = "Summary";
			this.infoContainer1.Size = new System.Drawing.Size(520, 368);
			this.infoContainer1.TabIndex = 0;
			// 
			// summary
			// 
			this.summary.Location = new System.Drawing.Point(192, 96);
			this.summary.Name = "summary";
			this.summary.ReadOnly = true;
			this.summary.Size = new System.Drawing.Size(296, 248);
			this.summary.TabIndex = 9;
			this.summary.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(184, 56);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(312, 32);
			this.label4.TabIndex = 8;
			this.label4.Text = "If the following information is correct, click finish to complete the wizard, oth" +
				"erwise, click back to make changes.";
			// 
			// improvementsPage
			// 
			this.improvementsPage.Controls.Add(this.improvements);
			this.improvementsPage.Controls.Add(this.header8);
			this.improvementsPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.improvementsPage.IsFinishPage = false;
			this.improvementsPage.Location = new System.Drawing.Point(0, 0);
			this.improvementsPage.Name = "improvementsPage";
			this.improvementsPage.Size = new System.Drawing.Size(520, 368);
			this.improvementsPage.TabIndex = 10;
			// 
			// improvements
			// 
			this.improvements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.improvements.Location = new System.Drawing.Point(24, 88);
			this.improvements.Name = "improvements";
			this.improvements.Size = new System.Drawing.Size(472, 264);
			this.improvements.TabIndex = 5;
			// 
			// header8
			// 
			this.header8.BackColor = System.Drawing.SystemColors.Control;
			this.header8.CausesValidation = false;
			this.header8.Description = "Select the stats that have a chance of improving by using this ability.";
			this.header8.Dock = System.Windows.Forms.DockStyle.Top;
			this.header8.Image = ((System.Drawing.Image)(resources.GetObject("header8.Image")));
			this.header8.Location = new System.Drawing.Point(0, 0);
			this.header8.Name = "header8";
			this.header8.Size = new System.Drawing.Size(520, 64);
			this.header8.TabIndex = 0;
			this.header8.Title = "Improvements";
			// 
			// requirementsPage
			// 
			this.requirementsPage.Controls.Add(this.requirements);
			this.requirementsPage.Controls.Add(this.header7);
			this.requirementsPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.requirementsPage.IsFinishPage = false;
			this.requirementsPage.Location = new System.Drawing.Point(0, 0);
			this.requirementsPage.Name = "requirementsPage";
			this.requirementsPage.Size = new System.Drawing.Size(520, 368);
			this.requirementsPage.TabIndex = 9;
			// 
			// requirements
			// 
			this.requirements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.requirements.Location = new System.Drawing.Point(168, 88);
			this.requirements.Name = "requirements";
			this.requirements.Size = new System.Drawing.Size(184, 244);
			this.requirements.TabIndex = 1;
			// 
			// header7
			// 
			this.header7.BackColor = System.Drawing.SystemColors.Control;
			this.header7.CausesValidation = false;
			this.header7.Description = "Select the abilities that must be known before learning this one.";
			this.header7.Dock = System.Windows.Forms.DockStyle.Top;
			this.header7.Image = ((System.Drawing.Image)(resources.GetObject("header7.Image")));
			this.header7.Location = new System.Drawing.Point(0, 0);
			this.header7.Name = "header7";
			this.header7.Size = new System.Drawing.Size(520, 64);
			this.header7.TabIndex = 0;
			this.header7.Title = "Requirements";
			// 
			// AbilityWizard
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(520, 416);
			this.Controls.Add(this.wizard1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "AbilityWizard";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Ability Wizard";
			this.wizard1.ResumeLayout(false);
			this.statModsPage.ResumeLayout(false);
			this.statCostPage.ResumeLayout(false);
			this.rangePage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.area)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.range)).EndInit();
			this.messagePage.ResumeLayout(false);
			this.prepareRecoverPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.recover)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.prepare)).EndInit();
			this.namePage.ResumeLayout(false);
			this.startPage.ResumeLayout(false);
			this.summaryPage.ResumeLayout(false);
			this.infoContainer1.ResumeLayout(false);
			this.improvementsPage.ResumeLayout(false);
			this.requirementsPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		Ability ability_;

		ArrayList Requirements {
			get {
				ArrayList requirements = new ArrayList();
				foreach(object checkedItem in this.requirements.CheckedItems) {
					Ability a = Server.Instance.GetAbility(checkedItem.ToString());
					if(a != null) {
						requirements.Add(a.Id);
					}
				}
				return requirements;
			}
		}

		private void namePage_CloseFromNext(object sender, Damdai.Forms.PageEventArgs e) {
			string name = this.name.Text.Trim().ToLower();
			if(name == "") {
				nameStatus.Text = "You must enter a name.";
				e.Page = namePage;
				return;
			} else if(name.IndexOf(' ') != -1) {
				nameStatus.Text = "Name may not contain spaces, numbers, or non-alphabet character.";
				this.name.Focus();
				this.name.SelectAll();
				e.Page = namePage;
				return;
			} else if(Server.Instance.GetAbility(name) != null && (ability_ == null || ability_.Name != name)) {
				nameStatus.Text = "An ability with that name already exists.";
				this.name.Focus();
				this.name.SelectAll();
				e.Page = namePage;
				return;
			}
			nameStatus.Text = "";
		}

		private void messagePage_CloseFromNext(object sender, Damdai.Forms.PageEventArgs e) {
			string msg = this.performMessage.Text.Trim();
			if(msg == "") {
				MessageBox.Show("You must enter a perform message.");
				e.Page = messagePage;
				return;
			}
		}

		private void messagePage_ShowFromNext(object sender, System.EventArgs e) {
			if(prepare.Value > 0) {
				prepareLabel.Show();
				prepareMessage.Show();
			} else {
				prepareLabel.Hide();
				prepareMessage.Hide();
			}
		}

		private void summaryPage_ShowFromNext(object sender, System.EventArgs e) {
			summary.Text += "Name: " + name.Text.Trim();
			
			summary.Text += "\r\nPrepare Time: " + prepare.Value;
			summary.Text += "\r\nRecover Time: " + recover.Value;

			summary.Text += "\r\nMessages:";
			summary.Text += "\r\n\tPrepare: " + prepareMessage.Text.Trim();
			summary.Text += "\r\n\tPerform: " + performMessage.Text.Trim();
			summary.Text += "\r\n\tFail: " + failMessage.Text.Trim();
			summary.Text += "\r\n\tMiss: " + missMessage.Text.Trim();
			summary.Text += "\r\n\tHit: " + hitMessage.Text.Trim();

			summary.Text += "\r\nRange: " + range.Value;
			summary.Text += "\r\nArea Effect: " + area.Value;

			summary.Text += "\r\nStat Cost:";
			Hashtable stats = statCost.Stats;
			foreach(DictionaryEntry de in stats) {
				summary.Text += "\r\n\t" + de.Key + ": " + de.Value;
			}

			summary.Text += "\r\nTarget Stat Modifiers:";
			stats = targetStatMods.Stats;
			foreach(DictionaryEntry de in stats) {
				Stat.Blueprint stat = Server.Instance.GetStat((Guid)de.Key);
				summary.Text += "\r\n\t" + stat.Name + ": " + de.Value;
			}

			summary.Text += "\r\nRequirements:";
			foreach(object checkedItem in this.requirements.CheckedItems) {
				summary.Text += "\r\n\t" + checkedItem.ToString();
			}
			summary.Text += "\r\nImprovements:";
			foreach(DictionaryEntry de in this.improvements.Stats) {
				Stat.Blueprint stat = Server.Instance.GetStat((Guid)de.Key);
				summary.Text += "\r\n\t" + stat.Name + '\t' + (decimal)de.Value + "%";;
			}
		}

		private void summaryPage_CloseFromNext(object sender, Damdai.Forms.PageEventArgs e) {
			//TODO: ADD REQUIREMENTS
//			Ability a = null;
//			if(ability_ != null) {
//				Server.Instance.RemoveAbility(ability_.Id);
//				a = new Ability(
//					ability_.Id, 
//					name.Text.Trim(), 
//					(int)range.Value, 
//					(int)area.Value, 
//					(int)prepare.Value, 
//					(int)recover.Value, 
//					statCost.GetStats(), 
//					targetStatMods.GetStats(), 
//					prepareMessage.Text.Trim(),
//					performMessage.Text.Trim(),
//					hitMessage.Text.Trim(),
//					missMessage.Text.Trim(),
//					failMessage.Text.Trim(),
//					this.Requirements,
//					this.improvements.GetStats());
//			} else {
//				a = new Ability(
//					name.Text.Trim(), 
//					(int)range.Value, 
//					(int)area.Value, 
//					(int)prepare.Value, 
//					(int)recover.Value, 
//					statCost.GetStats(), 
//					targetStatMods.GetStats(), 
//					prepareMessage.Text.Trim(), 
//					performMessage.Text.Trim(),
//					hitMessage.Text.Trim(),
//					missMessage.Text.Trim(),
//					failMessage.Text.Trim(),
//					this.Requirements,
//					this.improvements.GetStats());
//			}
//			Server.Instance.AddAbility(a);
		}
	}
}
