using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Ultimud
{
	/// <summary>
	/// Summary description for StatModifierForm.
	/// </summary>
	public class StatModifierForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox stat;
		private System.Windows.Forms.NumericUpDown amount;
		private System.Windows.Forms.NumericUpDown random;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Panel panel4;
		private Ultimud.StatView powerFactors;
		private Ultimud.StatView weakenFactors;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public StatModifierForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.stat.DataSource = Server.Instance.StatNames;
			this.stat.SelectedIndex = 0;
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
			this.label1 = new System.Windows.Forms.Label();
			this.stat = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.amount = new System.Windows.Forms.NumericUpDown();
			this.random = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.powerFactors = new Ultimud.StatView();
			this.label5 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.weakenFactors = new Ultimud.StatView();
			this.label6 = new System.Windows.Forms.Label();
			this.panel4 = new System.Windows.Forms.Panel();
			this.panel5 = new System.Windows.Forms.Panel();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.amount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.random)).BeginInit();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel5.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Stat";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// stat
			// 
			this.stat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.stat.Location = new System.Drawing.Point(80, 24);
			this.stat.Name = "stat";
			this.stat.Size = new System.Drawing.Size(121, 21);
			this.stat.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 23);
			this.label2.TabIndex = 2;
			this.label2.Text = "Amount";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// amount
			// 
			this.amount.Location = new System.Drawing.Point(80, 56);
			this.amount.Maximum = new System.Decimal(new int[] {
																   1000000000,
																   0,
																   0,
																   0});
			this.amount.Name = "amount";
			this.amount.TabIndex = 3;
			this.amount.Tag = "";
			this.amount.ThousandsSeparator = true;
			// 
			// random
			// 
			this.random.Location = new System.Drawing.Point(80, 88);
			this.random.Maximum = new System.Decimal(new int[] {
																   1000000,
																   0,
																   0,
																   0});
			this.random.Name = "random";
			this.random.TabIndex = 5;
			this.random.ThousandsSeparator = true;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 23);
			this.label3.TabIndex = 4;
			this.label3.Text = "Random";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.random);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.stat);
			this.panel1.Controls.Add(this.amount);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(472, 120);
			this.panel1.TabIndex = 8;
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.label4.Dock = System.Windows.Forms.DockStyle.Top;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.label4.Location = new System.Drawing.Point(0, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(472, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "Settings";
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.SystemColors.Control;
			this.panel2.Controls.Add(this.powerFactors);
			this.panel2.Controls.Add(this.label5);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(236, 150);
			this.panel2.TabIndex = 9;
			// 
			// powerFactors
			// 
			this.powerFactors.Dock = System.Windows.Forms.DockStyle.Fill;
			this.powerFactors.Location = new System.Drawing.Point(0, 16);
			this.powerFactors.Name = "powerFactors";
			this.powerFactors.Size = new System.Drawing.Size(236, 134);
			this.powerFactors.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.label5.Dock = System.Windows.Forms.DockStyle.Top;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.label5.Location = new System.Drawing.Point(0, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(236, 16);
			this.label5.TabIndex = 0;
			this.label5.Text = "Power Factors";
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.SystemColors.Control;
			this.panel3.Controls.Add(this.weakenFactors);
			this.panel3.Controls.Add(this.label6);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(236, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(236, 150);
			this.panel3.TabIndex = 10;
			// 
			// weakenFactors
			// 
			this.weakenFactors.Dock = System.Windows.Forms.DockStyle.Fill;
			this.weakenFactors.Location = new System.Drawing.Point(0, 16);
			this.weakenFactors.Name = "weakenFactors";
			this.weakenFactors.Size = new System.Drawing.Size(236, 134);
			this.weakenFactors.TabIndex = 2;
			// 
			// label6
			// 
			this.label6.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.label6.Dock = System.Windows.Forms.DockStyle.Top;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label6.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.label6.Location = new System.Drawing.Point(0, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(236, 16);
			this.label6.TabIndex = 0;
			this.label6.Text = "Weaken Factors";
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.panel3);
			this.panel4.Controls.Add(this.panel2);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.Location = new System.Drawing.Point(0, 120);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(472, 150);
			this.panel4.TabIndex = 11;
			// 
			// panel5
			// 
			this.panel5.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panel5.Controls.Add(this.cancelButton);
			this.panel5.Controls.Add(this.okButton);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel5.Location = new System.Drawing.Point(0, 270);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(472, 40);
			this.panel5.TabIndex = 12;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(392, 8);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(312, 8);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 0;
			this.okButton.Text = "&Ok";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// StatModifierForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 310);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel5);
			this.Name = "StatModifierForm";
			this.Text = "Stat Modifier";
			((System.ComponentModel.ISupportInitialize)(this.amount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.random)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void cancelButton_Click(object sender, System.EventArgs e) {
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void okButton_Click(object sender, System.EventArgs e) {
			//TODO: save StatModifier
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
