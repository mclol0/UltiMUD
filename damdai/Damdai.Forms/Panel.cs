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
	public class Panel : System.Windows.Forms.Panel {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Panel() {
			InitializeComponent();
			Font = new Font("arial", 8);
			//SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			BackColor = Color.DarkGray;
		}

		public Panel(Color color, string text, Font font) {
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
			this.Name = "Panel";
			this.Size = new System.Drawing.Size(150, 24);
		}
		#endregion

		const int gradientStretch_ = 400;
		ContentAlignment textAlign_ = ContentAlignment.MiddleLeft;
		Color borderColor_;
		float borderWidth_ = 1;
		bool windowsStyle_;
		bool titleBorder_;
		bool autoForeColor_ = true;

		[Browsable(true), Category("Appearance")]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; Invalidate(); }
		}

		[Browsable(true), Category("Appearance")]
		public ContentAlignment TextAlign {
			get { return textAlign_; }
			set { textAlign_ = value; Invalidate(); }
		}

		[Browsable(true), Category("Appearance")]
		public override Color BackColor {
			get { return base.BackColor; }
			set {
				base.BackColor = value;
				BorderColor = value;
			}
		}

		[Browsable(true), Category("Appearance")]
		public Color BorderColor {
			get { return borderColor_; }
			set { borderColor_ = value; Invalidate(); }
		}
		[Browsable(true), Category("Appearance")]
		public float BorderWidth {
			get { return borderWidth_; }
			set { borderWidth_ = value; Invalidate(); }
		}
		[Browsable(true), Category("Appearance")]
		public bool TitleBorder {
			get { return titleBorder_; }
			set { titleBorder_ = value; Invalidate(); }
		}
		[Browsable(true), Category("Appearance")]
		public bool AutoForeColor {
			get { return autoForeColor_; }
			set { autoForeColor_ = value; Invalidate(); }
		}
		/*[Browsable(true), Category("Appearance")]
		public bool windowsStyle {
			get { return windowsStyle_; }
			set { windowsStyle_ = value; Invalidate(); }
		}*/

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
			if(windowsStyle_) {
				base.OnPaintBackground(e);
				return;
			}
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
				g.FillPolygon(new SolidBrush(Color.White), points);
				g.FillPolygon(b, points);
				g.DrawPolygon(new Pen(borderColor_, borderWidth_), points);
			}
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			if(windowsStyle_) {
				base.OnPaint(e);
				return;
			}
			Graphics g = e.Graphics;
			int stringWidth = (int)g.MeasureString(Text, Font).Width;
			int stringHeight = (int)g.MeasureString(Text, Font).Height;
			Point p = new Point((Width-stringWidth)/2, (int)(Height-stringHeight)/2);
			switch(TextAlign) {
			case ContentAlignment.TopLeft:
				p.X = 5;
				p.Y = 3;
				break;
			case ContentAlignment.MiddleLeft:
				p.X = 5;
				break;
			case ContentAlignment.BottomLeft:
				p.X = 5;
				p.Y = Height-stringHeight-3;
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				p.X = Width-5-stringWidth;
				break;
			}

			if(titleBorder_ && (TextAlign == ContentAlignment.TopLeft)) {
				Point[] points = {
									 new Point(0, stringHeight+6),
									 new Point(stringWidth+10, stringHeight+6),
									 new Point(stringWidth+20, 0),
									 new Point(0+3, 0),
									 new Point(0, 3),
				};
				//using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(0, 0, stringWidth+30, stringHeight+6), Color.FromArgb(100, BackColor), Color.White, LinearGradientMode.Horizontal)) {
				using(SolidBrush b = new SolidBrush(Color.FromArgb(75, BackColor))) {
						g.FillPolygon(b, points);
				}
				g.DrawLines(new System.Drawing.Pen(BorderColor, 1), points);
			}

			Color c = ForeColor;
			if(autoForeColor_) {
				int gray = (int)(BackColor.R*0.3 + BackColor.G*0.59+ BackColor.B*0.11);
				c = gray < 63 ? Color.White : Color.Black;
			}
			g.DrawString(Text, Font, new SolidBrush(c), p);
		}
	}
}
