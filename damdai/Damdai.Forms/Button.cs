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
	public class Button : Label {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		bool pressed_;
		bool hover_;

		private System.ComponentModel.Container components = null;

		public Button():base() {
			InitializeComponent();
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
			this.Name = "Button";
			this.Size = new System.Drawing.Size(100, 32);
		}
		#endregion

		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e) {
			Graphics g = e.Graphics;
			g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);
			int width = this.Width-1;
			int height = this.Height-1;
			int offset = 0;

			if(pressed_) {
				++width;
				++height;
				offset = 1;
			}

			Point[] points = {
				new Point(3, offset),
				new Point(width-3, offset),
				new Point(width, 3),
				new Point(width, height-3),
				new Point(width-3, height),
				new Point(3, height),
				new Point(offset, height-3),
				new Point(offset, 3)
			};

			using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.FromArgb((pressed_ ? 100 : hover_ ? 75 : 50), BackColor), Color.White, LinearGradientMode.Horizontal)) {
				g.FillPolygon(b, points);
				g.DrawPolygon(new Pen(BackColor, 2), points);
			}
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			Graphics g = e.Graphics;
			int stringWidth = (int)g.MeasureString(Text, Font).Width;
			Point p = new Point((Width-stringWidth)/2, (int)(Height-g.MeasureString(Text, Font).Height)/2);
			g.DrawString(Text, Font, new SolidBrush(this.ForeColor), p);
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			pressed_ = true;
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			pressed_ = false;
			Invalidate();
		}

		protected override void OnMouseEnter(EventArgs e) {
			hover_ = true;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e) {
			hover_ = false;
			Invalidate();
		}
	}
}
