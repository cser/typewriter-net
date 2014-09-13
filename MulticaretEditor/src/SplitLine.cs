using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

namespace MulticaretEditor
{
	public class SplitLine : Control
	{
		public SplitLine()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			TabStop = false;
		}

		private Scheme scheme = new Scheme();
		public Scheme Scheme
		{
			get { return scheme; }
			set
			{
				if (scheme != value)
				{
					scheme = value;
					Invalidate();
				}
			}
		}

		public new void Invalidate()
		{
			if (InvokeRequired)
				BeginInvoke(new MethodInvoker(Invalidate));
			else
				base.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			g.FillRectangle(scheme.tabsBgBrush, 0, 0, Width, Height);
			g.DrawLine(scheme.tabsLinePen, Width - 1, 0, Width - 1, Height);

			base.OnPaint(e);
		}
	}
}
