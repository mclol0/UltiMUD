using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace Damdai.Forms {
	/// <summary>
	/// Summary description for UserControl.
	/// </summary>
	public class StatusBar : System.Windows.Forms.StatusBar {
		Color backColor_;

		[Browsable(true), Category("Appearance")]
		override public Color BackColor {
			get { return backColor_; }
			set { backColor_ = value; Invalidate(); }
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public StatusBar(System.ComponentModel.IContainer container) {
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			container.Add(this);
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			BackColor = Color.Silver;
		}

		public StatusBar():base() {
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			BackColor = Color.Silver;
		}

		public StatusBar(Color color, string text, Font font):base() {
			InitializeComponent();
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			Text = text;
			BackColor = color;
			Font = font;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			//
			// Label
			//
			this.Name = "StatusBar";
		}
		#endregion

		const int gradientStretch_ = 400;
		
		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e) {
			Graphics g = e.Graphics;
			using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(-gradientStretch_, 0, Width+gradientStretch_, Height), BackColor, Color.White, LinearGradientMode.Horizontal)) {
				Rectangle r = new Rectangle(-1, 0, Width+2, Height);
				g.FillRectangle(new SolidBrush(Color.White), r);
				g.FillRectangle(b, r);
				g.DrawRectangle(new Pen(BackColor), r);
			}
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			Graphics g = e.Graphics;

			if(Text != "") {
				Point p = new Point(5, (int)(Height-g.MeasureString(Text, Font).Height)/2);
				g.DrawString(Text, Font, new SolidBrush(this.ForeColor), p);
			}

			int x = 5;
			int y = (int)(Height - g.MeasureString("123abcABC", Font).Height)/2;
			foreach(StatusBarPanel panel in Panels) {
				g.DrawString(panel.Text, Font, new SolidBrush(ForeColor), x, y);
				x += panel.Width + 5;
			}
		}
	}
}
