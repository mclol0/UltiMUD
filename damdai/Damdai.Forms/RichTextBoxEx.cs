using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an extended RichTextBox control.
	/// </summary>
	public class RichTextBoxEx : System.Windows.Forms.RichTextBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		[DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
		public static extern int SendMessage( IntPtr hWnd, int Msg, ref GETTEXTLENGTHEX wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
		public struct GETTEXTLENGTHEX
		{
			public Int32 uiFlags;
			public Int32 uiCodePage;
		}

        #region Scrollbar positions functions
        public unsafe System.Drawing.Point GetScrollPos()
        {
            Win32.POINT res = new Win32.POINT();
            IntPtr ptr = new IntPtr(&res);
            Win32.SendMessage(this.Handle, Win32.EM_GETSCROLLPOS, 0, ptr);
            return new System.Drawing.Point(res.x, res.y);
        }
        #endregion

		private const Int32 GTL_DEFAULT = 0;
		private const Int32 CP_ANSI = 1252;
		private const Int32 CP_UNICODE = 1200;
		private const Int32 GT_DEFAULT = 0;
		private const Int32 WM_USER = 0x400;
		private const Int32 EM_GETTEXTLENGTHEX = WM_USER + 95;
		private const Int32 WM_PAINT = 0x00f;
		public const Int32 WM_SETREDRAW = 0xB;
		private bool paint = true;
        object lock_ = new object();

		/// <summary>
		/// Gets or sets whether to paint or not.
		/// </summary>
		public bool EnablePaint
		{
			get
			{
				return this.paint;
			}
			set
			{
				if (value != this.paint)
				{
					this.paint = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.RichTextBoxEx"/> class.
		/// </summary>
		/// <param name="container"></param>
		public RichTextBoxEx(System.ComponentModel.IContainer container)
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			container.Add(this);
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public RichTextBoxEx()
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Gets the length of the text.
		/// </summary>
		public override int TextLength
		{
			get
			{
				int ret;
				GETTEXTLENGTHEX GTL = new GETTEXTLENGTHEX();

				GTL.uiFlags = GTL_DEFAULT;

				if (Marshal.SystemDefaultCharSize == 1)
					GTL.uiCodePage = CP_ANSI;
				else
					GTL.uiCodePage = CP_UNICODE;

				ret = SendMessage(this.Handle, EM_GETTEXTLENGTHEX, ref GTL,
					IntPtr.Zero);

				return ret;
                
			}
		}

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // Code courtesy of Mark Mihevc
            // sometimes we want to eat the paint message so we don't have to see all the
            // flicker from when we select the text to change the color.
            if (m.Msg == WM_PAINT)
            {
                if (this.paint)
                    base.WndProc(ref m); // if we decided to paint this control, just call the RichTextBox WndProc
                else
                    m.Result = IntPtr.Zero; //  not painting, must set this to IntPtr.Zero if not painting otherwise serious problems.
            }
            else
                base.WndProc(ref m); // message other than WM_PAINT, jsut do what you normally do.
        }

        public void AppendLine(string str, System.Drawing.Color color)
        {
            lock (this.lock_)
            {
                this.SelectionStart = this.TextLength;
                System.Drawing.Color previouscolor = this.SelectionColor;
                if (this.TextLength > 0)
                    this.AppendText("\r\n");
                this.SelectionColor = color;
                this.AppendText(str);
                this.SelectionColor = previouscolor;
                this.ScrollToCaret();
            }
        }
        public void AppendLine(string str)
        {
            lock (this.lock_)
            {
                this.SelectionStart = this.TextLength;
                if (this.TextLength > 0)
                    str = "\r\n" + str;
                this.AppendText(str);
                this.ScrollToCaret();
            }
        }

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}