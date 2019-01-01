using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace Damdai.Forms
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Window : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ImageList imageList1;
		public System.Windows.Forms.Label pin;
		public System.Windows.Forms.Label close;
		public System.Windows.Forms.Label resize;
		public Damdai.Forms.Panel mainPanel;
		public System.Windows.Forms.Panel container;
		private System.ComponentModel.IContainer components;

		public Window()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			Thread animationThread = new Thread(new ThreadStart(Animate));
			animationThread.Start();
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Window));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.mainPanel = new Damdai.Forms.Panel();
			this.container = new System.Windows.Forms.Panel();
			this.resize = new System.Windows.Forms.Label();
			this.pin = new System.Windows.Forms.Label();
			this.close = new System.Windows.Forms.Label();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// mainPanel
			// 
			this.mainPanel.AutoForeColor = true;
			this.mainPanel.BackColor = System.Drawing.Color.DarkGray;
			this.mainPanel.BorderColor = System.Drawing.Color.DarkGray;
			this.mainPanel.BorderWidth = 1F;
			this.mainPanel.Controls.Add(this.container);
			this.mainPanel.Controls.Add(this.resize);
			this.mainPanel.Controls.Add(this.pin);
			this.mainPanel.Controls.Add(this.close);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Font = new System.Drawing.Font("Magneto", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.mainPanel.Location = new System.Drawing.Point(0, 0);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = new System.Drawing.Size(368, 256);
			this.mainPanel.TabIndex = 0;
			this.mainPanel.Text = "Window Title";
			this.mainPanel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.mainPanel.TitleBorder = true;
			this.mainPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mainPanel_MouseDown);
			// 
			// container
			// 
			this.container.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.container.BackColor = System.Drawing.Color.Transparent;
			this.container.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.container.Location = new System.Drawing.Point(8, 24);
			this.container.Name = "container";
			this.container.Size = new System.Drawing.Size(352, 224);
			this.container.TabIndex = 9;
			// 
			// resize
			// 
			this.resize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.resize.BackColor = System.Drawing.Color.Transparent;
			this.resize.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
			this.resize.Location = new System.Drawing.Point(349, 245);
			this.resize.Name = "resize";
			this.resize.Size = new System.Drawing.Size(16, 8);
			this.resize.TabIndex = 10;
			this.resize.MouseUp += new System.Windows.Forms.MouseEventHandler(this.resize_MouseUp);
			this.resize.MouseMove += new System.Windows.Forms.MouseEventHandler(this.resize_MouseMove);
			this.resize.MouseDown += new System.Windows.Forms.MouseEventHandler(this.resize_MouseDown);
			// 
			// pin
			// 
			this.pin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pin.BackColor = System.Drawing.Color.Transparent;
			this.pin.ImageIndex = 1;
			this.pin.ImageList = this.imageList1;
			this.pin.Location = new System.Drawing.Point(320, 4);
			this.pin.Name = "pin";
			this.pin.Size = new System.Drawing.Size(16, 16);
			this.pin.TabIndex = 8;
			this.pin.Click += new System.EventHandler(this.pin_Click);
			// 
			// close
			// 
			this.close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.close.BackColor = System.Drawing.Color.Transparent;
			this.close.ImageIndex = 0;
			this.close.ImageList = this.imageList1;
			this.close.Location = new System.Drawing.Point(336, 4);
			this.close.Name = "close";
			this.close.Size = new System.Drawing.Size(16, 16);
			this.close.TabIndex = 7;
			this.close.Click += new System.EventHandler(this.close_Click);
			// 
			// Window
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Magenta;
			this.ClientSize = new System.Drawing.Size(368, 256);
			this.Controls.Add(this.mainPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Window";
			this.Opacity = 0.9;
			this.Text = "Window";
			this.TransparencyKey = System.Drawing.Color.Magenta;
			this.mainPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		Rectangle resizeFrame_ = new Rectangle();
		bool pinned_;
		bool activated_;
		bool fade_ = true;
		bool closeOnEscape_;

		#region properties
		public bool fade {
			get { return fade_; }
			set { fade_ = value; }
		}
		[Browsable(true), Category("Behavior")]
		public bool CloseOnEscape {
			get { return closeOnEscape_; }
			set { closeOnEscape_ = value; }
		}

		#endregion

		private void close_Click(object sender, System.EventArgs e) {
			Close();
		}

		#region Move
		//API functions to move the form
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HTCAPTION = 0x2;
		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture();
		[DllImportAttribute("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		void mainPanel_MouseDown(object sender, MouseEventArgs e) {
			ReleaseCapture();
			SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
		}
		#endregion

		#region Resize
		private void resize_MouseDown(object sender, MouseEventArgs e) {
			resizeFrame_.Location = new System.Drawing.Point(Left, Top);
			resizeFrame_.Size = new System.Drawing.Size(resizeFrame_.Width, resizeFrame_.Height);
			ControlPaint.DrawReversibleFrame(resizeFrame_, Color.Empty, System.Windows.Forms.FrameStyle.Thick);
		}
		private void resize_MouseUp(object sender, MouseEventArgs e) {
			resizeFrame_.Location = new System.Drawing.Point(Left, Top);
			resizeFrame_.Size = new System.Drawing.Size(resizeFrame_.Width, resizeFrame_.Height);
			ControlPaint.DrawReversibleFrame(resizeFrame_, Color.Empty, System.Windows.Forms.FrameStyle.Thick);
			Width = resizeFrame_.Width;
			Height = resizeFrame_.Height;
		}
		private void resize_MouseMove(object sender, MouseEventArgs e) {
			if(e.Button == MouseButtons.Left) {
				int resizeWidth_ = (MousePosition.X-this.Location.X);
				int resizeHeight_ = (MousePosition.Y-this.Location.Y);

				if(resizeWidth_ < MinimumSize.Width) {
					resizeWidth_ = MinimumSize.Width;
				}
				if(resizeHeight_ < MinimumSize.Height) {
					resizeHeight_ = MinimumSize.Height;
				}

				ControlPaint.DrawReversibleFrame(resizeFrame_, Color.Empty, System.Windows.Forms.FrameStyle.Thick);
				resizeFrame_.Location = new System.Drawing.Point(Left, Top);
				resizeFrame_.Size = new System.Drawing.Size(resizeWidth_, resizeHeight_);
				ControlPaint.DrawReversibleFrame(resizeFrame_, Color.Empty, System.Windows.Forms.FrameStyle.Thick);
			}
		}
		#endregion

		private void pin_Click(object sender, System.EventArgs e) {
			pinned_ = !pinned_;
			TopMost = pinned_;
			pin.ImageIndex = pinned_ ? 2 : 1;
		}

		protected override void OnActivated(EventArgs e) {
			base.OnActivated(e);
			activated_ = true;
		}

		protected override void OnDeactivate(EventArgs e) {
			base.OnDeactivate(e);
			activated_ = false;
		}

		void FadeIn() {
			if(fade_) {
				while(Opacity < 1) {
					Opacity += .02;
					System.Threading.Thread.Sleep(5);
				}
			}
			Opacity = 1;
		}

		void FadeOut() {
			if(fade_) {
				while(Opacity > .5) {
					Opacity -= .02;
					System.Threading.Thread.Sleep(5);
				}
			}
			Opacity = .5;
		}

		void Animate() {
			while(!this.Disposing) {
				Thread.Sleep(100);
				try {
					if(this.activated_) {
						FadeIn();
					} else {
						FadeOut();
					}
				} catch {
				}
			}
		}

		private void IMWindow_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			if(e.KeyCode == Keys.Escape && closeOnEscape_) {
				this.Close();
			}
		}
	}
}
