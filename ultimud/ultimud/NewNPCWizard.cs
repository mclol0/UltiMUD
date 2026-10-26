using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Ultimud
{
    /// <summary>
    /// Summary description for NewObjectWizard.
    /// </summary>
    public class NewNPCWizard : System.Windows.Forms.Form
    {
        private Damdai.Forms.Wizard wizard1;
        private Damdai.Forms.WizardPage startPage;
        private Damdai.Forms.WizardPage namePage;
        private Damdai.Forms.WizardPage statsPage;
        private Damdai.Forms.WizardPage behaviorPage;
        private Damdai.Forms.WizardPage locationPage;
        private Damdai.Forms.WizardPage summaryPage;
        private Damdai.Forms.InfoPage infoPage1;
        private Damdai.Forms.Header header1;
        private Damdai.Forms.Header header2;
        private System.Windows.Forms.TextBox name;
        private Damdai.Forms.Header header3;
        private Damdai.Forms.ListView stats;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button newStatButton;
        private Damdai.Forms.Header header4;
        private Damdai.Forms.Header header5;
        private Ultimud.AreaCanvas areaCanvas1;
        private System.Windows.Forms.ComboBox area;
        private Damdai.Forms.InfoContainer infoContainer1;
        private System.Windows.Forms.RichTextBox summary;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown statValue;
        private System.Windows.Forms.TextBox statName;
        private Damdai.Forms.WizardPage endPage;
        private Damdai.Forms.WizardPage blueprintPage;
        private System.Windows.Forms.ComboBox blueprint;
        private Damdai.Forms.WizardPage sexPage;
        private Damdai.Forms.Header header6;
        private System.Windows.Forms.RadioButton male;
        private System.Windows.Forms.RadioButton female;
        private System.Windows.Forms.RadioButton none;
        private Damdai.Forms.InfoContainer infoContainer2;
        private System.Windows.Forms.CheckBox saveBlueprint;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public NewNPCWizard()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            foreach (Area a in Server.Instance.Areas)
            {
                area.Items.Add(a.Name);
            }
            area.SelectedIndex = 0;
            areaCanvas1.LocationClickedEvent += new AreaCanvas.PointHandler(SetLocation);
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NewNPCWizard));
            this.wizard1 = new Damdai.Forms.Wizard();
            this.sexPage = new Damdai.Forms.WizardPage();
            this.none = new System.Windows.Forms.RadioButton();
            this.female = new System.Windows.Forms.RadioButton();
            this.male = new System.Windows.Forms.RadioButton();
            this.header6 = new Damdai.Forms.Header();
            this.statsPage = new Damdai.Forms.WizardPage();
            this.statName = new System.Windows.Forms.TextBox();
            this.statValue = new System.Windows.Forms.NumericUpDown();
            this.newStatButton = new System.Windows.Forms.Button();
            this.stats = new Damdai.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.header3 = new Damdai.Forms.Header();
            this.behaviorPage = new Damdai.Forms.WizardPage();
            this.header4 = new Damdai.Forms.Header();
            this.locationPage = new Damdai.Forms.WizardPage();
            this.area = new System.Windows.Forms.ComboBox();
            this.areaCanvas1 = new Ultimud.AreaCanvas();
            this.header5 = new Damdai.Forms.Header();
            this.summaryPage = new Damdai.Forms.WizardPage();
            this.infoContainer1 = new Damdai.Forms.InfoContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.summary = new System.Windows.Forms.RichTextBox();
            this.endPage = new Damdai.Forms.WizardPage();
            this.namePage = new Damdai.Forms.WizardPage();
            this.label2 = new System.Windows.Forms.Label();
            this.name = new System.Windows.Forms.TextBox();
            this.header2 = new Damdai.Forms.Header();
            this.blueprintPage = new Damdai.Forms.WizardPage();
            this.blueprint = new System.Windows.Forms.ComboBox();
            this.header1 = new Damdai.Forms.Header();
            this.startPage = new Damdai.Forms.WizardPage();
            this.infoPage1 = new Damdai.Forms.InfoPage();
            this.infoContainer2 = new Damdai.Forms.InfoContainer();
            this.saveBlueprint = new System.Windows.Forms.CheckBox();
            this.wizard1.SuspendLayout();
            this.sexPage.SuspendLayout();
            this.statsPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statValue)).BeginInit();
            this.behaviorPage.SuspendLayout();
            this.locationPage.SuspendLayout();
            this.summaryPage.SuspendLayout();
            this.infoContainer1.SuspendLayout();
            this.endPage.SuspendLayout();
            this.namePage.SuspendLayout();
            this.blueprintPage.SuspendLayout();
            this.startPage.SuspendLayout();
            this.infoContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard1
            // 
            this.wizard1.Controls.Add(this.endPage);
            this.wizard1.Controls.Add(this.summaryPage);
            this.wizard1.Controls.Add(this.locationPage);
            this.wizard1.Controls.Add(this.behaviorPage);
            this.wizard1.Controls.Add(this.statsPage);
            this.wizard1.Controls.Add(this.sexPage);
            this.wizard1.Controls.Add(this.namePage);
            this.wizard1.Controls.Add(this.blueprintPage);
            this.wizard1.Controls.Add(this.startPage);
            this.wizard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizard1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.wizard1.Location = new System.Drawing.Point(0, 0);
            this.wizard1.Name = "wizard1";
            this.wizard1.Pages.AddRange(new Damdai.Forms.WizardPage[] {
																		  this.startPage,
																		  this.blueprintPage,
																		  this.namePage,
																		  this.sexPage,
																		  this.statsPage,
																		  this.behaviorPage,
																		  this.locationPage,
																		  this.summaryPage,
																		  this.endPage});
            this.wizard1.Size = new System.Drawing.Size(496, 358);
            this.wizard1.TabIndex = 0;
            // 
            // sexPage
            // 
            this.sexPage.Controls.Add(this.none);
            this.sexPage.Controls.Add(this.female);
            this.sexPage.Controls.Add(this.male);
            this.sexPage.Controls.Add(this.header6);
            this.sexPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sexPage.IsFinishPage = false;
            this.sexPage.Location = new System.Drawing.Point(0, 0);
            this.sexPage.Name = "sexPage";
            this.sexPage.Size = new System.Drawing.Size(496, 310);
            this.sexPage.TabIndex = 9;
            this.sexPage.CloseFromNext += new Damdai.Forms.PageEventHandler(this.sexPage_CloseFromNext);
            // 
            // none
            // 
            this.none.Location = new System.Drawing.Point(204, 171);
            this.none.Name = "none";
            this.none.Size = new System.Drawing.Size(88, 16);
            this.none.TabIndex = 3;
            this.none.Text = "None";
            // 
            // female
            // 
            this.female.Location = new System.Drawing.Point(204, 147);
            this.female.Name = "female";
            this.female.Size = new System.Drawing.Size(88, 16);
            this.female.TabIndex = 2;
            this.female.Text = "Female";
            // 
            // male
            // 
            this.male.Location = new System.Drawing.Point(204, 123);
            this.male.Name = "male";
            this.male.Size = new System.Drawing.Size(88, 16);
            this.male.TabIndex = 1;
            this.male.Text = "Male";
            // 
            // header6
            // 
            this.header6.BackColor = System.Drawing.SystemColors.Control;
            this.header6.CausesValidation = false;
            this.header6.Description = "Select the sex of this new NPC.";
            this.header6.Dock = System.Windows.Forms.DockStyle.Top;
            this.header6.Image = ((System.Drawing.Image)(resources.GetObject("header6.Image")));
            this.header6.Location = new System.Drawing.Point(0, 0);
            this.header6.Name = "header6";
            this.header6.Size = new System.Drawing.Size(496, 64);
            this.header6.TabIndex = 0;
            this.header6.Title = "Sex";
            // 
            // statsPage
            // 
            this.statsPage.Controls.Add(this.statName);
            this.statsPage.Controls.Add(this.statValue);
            this.statsPage.Controls.Add(this.newStatButton);
            this.statsPage.Controls.Add(this.stats);
            this.statsPage.Controls.Add(this.header3);
            this.statsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statsPage.IsFinishPage = false;
            this.statsPage.Location = new System.Drawing.Point(0, 0);
            this.statsPage.Name = "statsPage";
            this.statsPage.Size = new System.Drawing.Size(496, 310);
            this.statsPage.TabIndex = 4;
            this.statsPage.ShowFromNext += new System.EventHandler(this.statsPage_ShowFromNext);
            // 
            // statName
            // 
            this.statName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.statName.Location = new System.Drawing.Point(16, 280);
            this.statName.MaxLength = 20;
            this.statName.Name = "statName";
            this.statName.TabIndex = 4;
            this.statName.Text = "New Stat";
            this.statName.Visible = false;
            // 
            // statValue
            // 
            this.statValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.statValue.Location = new System.Drawing.Point(128, 280);
            this.statValue.Maximum = new System.Decimal(new int[] {
																	  100000000,
																	  0,
																	  0,
																	  0});
            this.statValue.Name = "statValue";
            this.statValue.TabIndex = 3;
            this.statValue.ThousandsSeparator = true;
            this.statValue.Visible = false;
            // 
            // newStatButton
            // 
            this.newStatButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.newStatButton.Location = new System.Drawing.Point(408, 280);
            this.newStatButton.Name = "newStatButton";
            this.newStatButton.TabIndex = 2;
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
            this.stats.Location = new System.Drawing.Point(16, 80);
            this.stats.MultiSelect = false;
            this.stats.Name = "stats";
            this.stats.Size = new System.Drawing.Size(464, 192);
            this.stats.TabIndex = 1;
            this.stats.View = System.Windows.Forms.View.Details;
            this.stats.SubItemEndEditing += new Damdai.Forms.SubItemEndEditingEventHandler(this.stats_SubItemEndEditing);
            this.stats.SubItemClicked += new Damdai.Forms.SubItemEventHandler(this.stats_SubItemClicked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Stat";
            this.columnHeader1.Width = 343;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 117;
            // 
            // header3
            // 
            this.header3.BackColor = System.Drawing.SystemColors.Control;
            this.header3.CausesValidation = false;
            this.header3.Description = "Select and configure this NPC\'s stats.";
            this.header3.Dock = System.Windows.Forms.DockStyle.Top;
            this.header3.Image = ((System.Drawing.Image)(resources.GetObject("header3.Image")));
            this.header3.Location = new System.Drawing.Point(0, 0);
            this.header3.Name = "header3";
            this.header3.Size = new System.Drawing.Size(496, 64);
            this.header3.TabIndex = 0;
            this.header3.Title = "Stats";
            // 
            // behaviorPage
            // 
            this.behaviorPage.Controls.Add(this.header4);
            this.behaviorPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.behaviorPage.IsFinishPage = false;
            this.behaviorPage.Location = new System.Drawing.Point(0, 0);
            this.behaviorPage.Name = "behaviorPage";
            this.behaviorPage.Size = new System.Drawing.Size(496, 310);
            this.behaviorPage.TabIndex = 5;
            // 
            // header4
            // 
            this.header4.BackColor = System.Drawing.SystemColors.Control;
            this.header4.CausesValidation = false;
            this.header4.Description = "Configure the behavior of this NPC.";
            this.header4.Dock = System.Windows.Forms.DockStyle.Top;
            this.header4.Image = ((System.Drawing.Image)(resources.GetObject("header4.Image")));
            this.header4.Location = new System.Drawing.Point(0, 0);
            this.header4.Name = "header4";
            this.header4.Size = new System.Drawing.Size(496, 64);
            this.header4.TabIndex = 0;
            this.header4.Title = "Behavior";
            // 
            // locationPage
            // 
            this.locationPage.Controls.Add(this.area);
            this.locationPage.Controls.Add(this.areaCanvas1);
            this.locationPage.Controls.Add(this.header5);
            this.locationPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.locationPage.IsFinishPage = false;
            this.locationPage.Location = new System.Drawing.Point(0, 0);
            this.locationPage.Name = "locationPage";
            this.locationPage.Size = new System.Drawing.Size(496, 310);
            this.locationPage.TabIndex = 6;
            // 
            // area
            // 
            this.area.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.area.Location = new System.Drawing.Point(8, 72);
            this.area.Name = "area";
            this.area.TabIndex = 2;
            this.area.SelectedIndexChanged += new System.EventHandler(this.area_SelectedIndexChanged);
            // 
            // areaCanvas1
            // 
            this.areaCanvas1.BackColor = System.Drawing.Color.DarkGray;
            this.areaCanvas1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.areaCanvas1.Editable = false;
            this.areaCanvas1.Location = new System.Drawing.Point(0, 64);
            this.areaCanvas1.Mark = new System.Drawing.Point(0, 0);
            this.areaCanvas1.Name = "areaCanvas1";
            this.areaCanvas1.Saved = true;
            this.areaCanvas1.Size = new System.Drawing.Size(496, 246);
            this.areaCanvas1.TabIndex = 1;
            // 
            // header5
            // 
            this.header5.BackColor = System.Drawing.SystemColors.Control;
            this.header5.CausesValidation = false;
            this.header5.Description = "Select the location for this NPC.";
            this.header5.Dock = System.Windows.Forms.DockStyle.Top;
            this.header5.Image = ((System.Drawing.Image)(resources.GetObject("header5.Image")));
            this.header5.Location = new System.Drawing.Point(0, 0);
            this.header5.Name = "header5";
            this.header5.Size = new System.Drawing.Size(496, 64);
            this.header5.TabIndex = 0;
            this.header5.Title = "Location";
            // 
            // summaryPage
            // 
            this.summaryPage.Controls.Add(this.infoContainer1);
            this.summaryPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summaryPage.IsFinishPage = false;
            this.summaryPage.Location = new System.Drawing.Point(0, 0);
            this.summaryPage.Name = "summaryPage";
            this.summaryPage.Size = new System.Drawing.Size(496, 310);
            this.summaryPage.TabIndex = 7;
            this.summaryPage.CloseFromNext += new Damdai.Forms.PageEventHandler(this.summaryPage_CloseFromNext);
            this.summaryPage.ShowFromNext += new System.EventHandler(this.summaryPage_ShowFromNext);
            // 
            // infoContainer1
            // 
            this.infoContainer1.BackColor = System.Drawing.Color.White;
            this.infoContainer1.Controls.Add(this.label1);
            this.infoContainer1.Controls.Add(this.summary);
            this.infoContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoContainer1.Image = ((System.Drawing.Image)(resources.GetObject("infoContainer1.Image")));
            this.infoContainer1.Location = new System.Drawing.Point(0, 0);
            this.infoContainer1.Name = "infoContainer1";
            this.infoContainer1.PageTitle = "Summary";
            this.infoContainer1.Size = new System.Drawing.Size(496, 310);
            this.infoContainer1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(200, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 40);
            this.label1.TabIndex = 9;
            this.label1.Text = "Review the following summary and click next to complete, or back to make changes." +
                "";
            // 
            // summary
            // 
            this.summary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.summary.Location = new System.Drawing.Point(200, 112);
            this.summary.Name = "summary";
            this.summary.ReadOnly = true;
            this.summary.Size = new System.Drawing.Size(264, 168);
            this.summary.TabIndex = 8;
            this.summary.Text = "";
            // 
            // endPage
            // 
            this.endPage.Controls.Add(this.infoContainer2);
            this.endPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.endPage.IsFinishPage = true;
            this.endPage.Location = new System.Drawing.Point(0, 0);
            this.endPage.Name = "endPage";
            this.endPage.Size = new System.Drawing.Size(496, 310);
            this.endPage.TabIndex = 8;
            // 
            // namePage
            // 
            this.namePage.Controls.Add(this.label2);
            this.namePage.Controls.Add(this.name);
            this.namePage.Controls.Add(this.header2);
            this.namePage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.namePage.IsFinishPage = false;
            this.namePage.Location = new System.Drawing.Point(0, 0);
            this.namePage.Name = "namePage";
            this.namePage.Size = new System.Drawing.Size(496, 310);
            this.namePage.TabIndex = 3;
            this.namePage.ShowFromBack += new System.EventHandler(this.namePage_Update);
            this.namePage.ShowFromNext += new System.EventHandler(this.namePage_Update);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(160, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Name";
            // 
            // name
            // 
            this.name.Location = new System.Drawing.Point(160, 145);
            this.name.MaxLength = 50;
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(176, 21);
            this.name.TabIndex = 1;
            this.name.Text = "";
            this.name.TextChanged += new System.EventHandler(this.namePage_Update);
            // 
            // header2
            // 
            this.header2.BackColor = System.Drawing.SystemColors.Control;
            this.header2.CausesValidation = false;
            this.header2.Description = "Enter the name of this NPC.";
            this.header2.Dock = System.Windows.Forms.DockStyle.Top;
            this.header2.Image = ((System.Drawing.Image)(resources.GetObject("header2.Image")));
            this.header2.Location = new System.Drawing.Point(0, 0);
            this.header2.Name = "header2";
            this.header2.Size = new System.Drawing.Size(496, 64);
            this.header2.TabIndex = 0;
            this.header2.Title = "Name";
            // 
            // blueprintPage
            // 
            this.blueprintPage.Controls.Add(this.blueprint);
            this.blueprintPage.Controls.Add(this.header1);
            this.blueprintPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blueprintPage.IsFinishPage = false;
            this.blueprintPage.Location = new System.Drawing.Point(0, 0);
            this.blueprintPage.Name = "blueprintPage";
            this.blueprintPage.Size = new System.Drawing.Size(496, 310);
            this.blueprintPage.TabIndex = 2;
            // 
            // blueprint
            // 
            this.blueprint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.blueprint.Items.AddRange(new object[] {
														   "None"});
            this.blueprint.Location = new System.Drawing.Point(170, 145);
            this.blueprint.Name = "blueprint";
            this.blueprint.Size = new System.Drawing.Size(156, 21);
            this.blueprint.TabIndex = 1;
            // 
            // header1
            // 
            this.header1.BackColor = System.Drawing.SystemColors.Control;
            this.header1.CausesValidation = false;
            this.header1.Description = "Select the blueprint you want to use as the basis for this NPC, or click next if " +
                "you don\'t want to use a blueprint.";
            this.header1.Dock = System.Windows.Forms.DockStyle.Top;
            this.header1.Image = ((System.Drawing.Image)(resources.GetObject("header1.Image")));
            this.header1.Location = new System.Drawing.Point(0, 0);
            this.header1.Name = "header1";
            this.header1.Size = new System.Drawing.Size(496, 64);
            this.header1.TabIndex = 0;
            this.header1.Title = "Blueprint";
            // 
            // startPage
            // 
            this.startPage.Controls.Add(this.infoPage1);
            this.startPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.startPage.IsFinishPage = false;
            this.startPage.Location = new System.Drawing.Point(0, 0);
            this.startPage.Name = "startPage";
            this.startPage.Size = new System.Drawing.Size(496, 310);
            this.startPage.TabIndex = 1;
            // 
            // infoPage1
            // 
            this.infoPage1.BackColor = System.Drawing.Color.White;
            this.infoPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPage1.Image = ((System.Drawing.Image)(resources.GetObject("infoPage1.Image")));
            this.infoPage1.Location = new System.Drawing.Point(0, 0);
            this.infoPage1.Name = "infoPage1";
            this.infoPage1.PageText = "This wizard enables you to create a new NPC.";
            this.infoPage1.PageTitle = "Welcome to the New NPC Wizard";
            this.infoPage1.Size = new System.Drawing.Size(496, 310);
            this.infoPage1.TabIndex = 1;
            // 
            // infoContainer2
            // 
            this.infoContainer2.BackColor = System.Drawing.Color.White;
            this.infoContainer2.Controls.Add(this.saveBlueprint);
            this.infoContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoContainer2.Image = ((System.Drawing.Image)(resources.GetObject("infoContainer2.Image")));
            this.infoContainer2.Location = new System.Drawing.Point(0, 0);
            this.infoContainer2.Name = "infoContainer2";
            this.infoContainer2.PageTitle = "New NPC Wizard Complete";
            this.infoContainer2.Size = new System.Drawing.Size(496, 310);
            this.infoContainer2.TabIndex = 0;
            // 
            // saveBlueprint
            // 
            this.saveBlueprint.Location = new System.Drawing.Point(240, 143);
            this.saveBlueprint.Name = "saveBlueprint";
            this.saveBlueprint.Size = new System.Drawing.Size(176, 24);
            this.saveBlueprint.TabIndex = 8;
            this.saveBlueprint.Text = "Save blueprint for this NPC?";
            // 
            // NewNPCWizard
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(496, 358);
            this.Controls.Add(this.wizard1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "NewNPCWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New NPC Wizard";
            this.wizard1.ResumeLayout(false);
            this.sexPage.ResumeLayout(false);
            this.statsPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.statValue)).EndInit();
            this.behaviorPage.ResumeLayout(false);
            this.locationPage.ResumeLayout(false);
            this.summaryPage.ResumeLayout(false);
            this.infoContainer1.ResumeLayout(false);
            this.endPage.ResumeLayout(false);
            this.namePage.ResumeLayout(false);
            this.blueprintPage.ResumeLayout(false);
            this.startPage.ResumeLayout(false);
            this.infoContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void namePage_Update(object sender, System.EventArgs e)
        {
            this.wizard1.NextEnabled = (this.name.Text.Trim().Length > 0);
        }

        private void statsPage_ShowFromNext(object sender, System.EventArgs e)
        {
            foreach (string str in Server.Instance.StatNames)
            {
                this.stats.Items.Add(new ListViewItem(new string[] { str, "0" }));
            }
        }

        private void newStatButton_Click(object sender, System.EventArgs e)
        {
            ListViewItem item = new ListViewItem(new string[] { "New Stat", "0" });
            this.stats.Items.Add(item);
            this.stats.StartEditing(this.statName, item, 0);
        }

        private void stats_SubItemClicked(object sender, Damdai.Forms.SubItemEventArgs e)
        {
            if (e.SubItem == 1)
            {
                if (e.Item.Tag != null)
                {
                    statValue.Value = (decimal)e.Item.Tag;
                }
                statValue.Select(0, (int)statValue.Maximum);
                stats.StartEditing(statValue, e.Item, e.SubItem);
            }
        }

        private void stats_SubItemEndEditing(object sender, Damdai.Forms.SubItemEndEditingEventArgs e)
        {
            if (e.SubItem == 0)
            {
                if (!Server.Instance.Stats.Contains(e.Item.SubItems[e.SubItem].Text.ToLower()))
                {
                    MessageBox.Show("A stat with that name already exists.", "Cannot Add Stat...");
                    e.Cancel = true;
                    stats.StartEditing(statName, e.Item, 0);
                    return;
                }
            }
            else if (e.SubItem == 1)
            {
                e.Item.Tag = statValue.Value;
            }
        }

        private void area_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            areaCanvas1.SetArea(Server.Instance.GetArea(area.SelectedItem.ToString()));
        }

        void SetLocation(Point p)
        {
            areaCanvas1.Mark = p;
        }

        public void SetLocation(Area a, Point p)
        {
            if (area != null)
            {
                for (int x = 0; x < area.Items.Count; ++x)
                {
                    if (area.Items[x].ToString() == a.Name)
                    {
                        area.SelectedIndex = x;
                        break;
                    }
                }
                SetLocation(p);
                areaCanvas1.GoTo(p);
            }
        }

        private void summaryPage_ShowFromNext(object sender, System.EventArgs e)
        {
            summary.Text = "Blueprint:";
            summary.Text += "\r\n\t" + blueprint.SelectedText;

            summary.Text += "\r\nName:";
            summary.Text += "\r\n\t" + name.Text;

            summary.Text += "\r\nSex:";
            if (male.Checked)
            {
                summary.Text += "\r\n\t" + male.Text;
            }
            else if (female.Checked)
            {
                summary.Text += "\r\n\t" + female.Text;
            }
            else if (none.Checked)
            {
                summary.Text += "\r\n\t" + none.Text;
            }

            summary.Text += "\r\nStats:";
            foreach (ListViewItem i in stats.Items)
            {
                if ((decimal)i.Tag != 0)
                {
                    summary.Text += "\r\n\t" + i.Text + "\t" + i.SubItems[1].Text;
                }
            }

            summary.Text += "\r\nLocation:";
            summary.Text += "\r\n\t" + area.Text + " " + areaCanvas1.Mark.ToString();
        }

        private void summaryPage_CloseFromNext(object sender, Damdai.Forms.PageEventArgs e)
        {
            //			Hashtable stats = new Hashtable();
            //			foreach(ListViewItem i in this.stats.Items) {
            //				if((decimal)i.Tag != 0) {
            //					Stat stat = new Stat(i.Text);
            //					stat.current = stat.max = (int)(decimal)i.Tag;
            //					stats.Add(stat.name, stat);
            //					Server.Instance.AddStat(stat.name);
            //				}
            //			}
            //			NPC npc = new NPC(name.Text, stats, areaCanvas1.area, areaCanvas1.mark);
        }

        private void sexPage_CloseFromNext(object sender, Damdai.Forms.PageEventArgs e)
        {
            if (!male.Checked && !female.Checked && !none.Checked)
            {
                e.Page = sexPage;
            }
        }
    }
}
