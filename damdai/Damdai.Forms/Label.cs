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
	public class Label : System.Windows.Forms.Label {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Label() {
			InitializeComponent();
			BackColor = Color.DarkGray;
			Font = new Font("arial", 8);
		}

		public Label(Color color, string text, Font font) {
			// This call is required by the Windows.Forms Designer.
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			//SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			//this.BackColor = Color.Transparent;
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
			this.Name = "Label";
			this.Size = new System.Drawing.Size(150, 24);
		}
		#endregion

		const int gradientStretch_ = 400;

		Control GetParentWithBackColor(Control parent) {
			if(parent.BackColor == Color.Transparent) {
				return GetParentWithBackColor(parent.Parent);
			}
			return parent;
		}
		void PaintBackground(Graphics g) {
			Control parent = GetParentWithBackColor(Parent);
			if(parent.GetType().Namespace == "Dcd") {
				using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(-gradientStretch_, 0, Width+gradientStretch_, Height), parent.BackColor, Color.White, LinearGradientMode.Horizontal)) {
					g.FillRectangle(b, 0, 0, Width, Height);
				}
			} else {
				g.Clear(Parent.BackColor);
			}
		}

		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e) {
			if(Height == 0 || Width == 0) {
				return;
			}
			Graphics g = e.Graphics;
			PaintBackground(g);
			int width = this.Width-1;
			int height = this.Height-1;

			Point[] points = {
				new Point(3, 0),
				new Point(width-3, 0),
				new Point(width, 3),
				new Point(width, height-3),
				new Point(width-3, height),
				new Point(3, height),
				new Point(0, height-3),
				new Point(0, 3)
			};

			using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(-gradientStretch_, 0, Width+gradientStretch_, Height), BackColor, Color.White, LinearGradientMode.Horizontal)) {
				g.FillPolygon(b, points);
				g.DrawPolygon(new Pen(BackColor), points);
			}
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			Graphics g = e.Graphics;
			int stringWidth = (int)g.MeasureString(Text, Font).Width;
			Point p = new Point((Width-stringWidth)/2, (int)(Height-g.MeasureString(Text, Font).Height)/2);
			switch(TextAlign) {
			case ContentAlignment.TopLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.BottomLeft:
				p.X = 5;
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				p.X = Width-5-stringWidth;
				break;
			}
			g.DrawString(Text, Font, new SolidBrush(this.ForeColor), p);
		}
	}
}
