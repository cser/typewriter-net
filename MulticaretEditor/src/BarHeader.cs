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
	public class BarHeader : Control
	{
		public event Setter CloseClick;
		
		private StringFormat stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
		
		public BarHeader()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			
			TabStop = false;
			
			SetFont(FontFamily.GenericMonospace, 10.25f);
		}
		
		private Font font;
		private Font boldFont;
		private int charWidth;
		private int charHeight;

		public void SetFont(FontFamily family, float emSize)
		{
			font = new Font(family, emSize);
			boldFont = new Font(family, emSize, FontStyle.Bold);
			
			SizeF size = GetCharSize(font, 'M');
			charWidth = (int)Math.Round(size.Width * 1f) - 1;
			charHeight = (int)Math.Round(size.Height * 1f) + 1;
			Height = charHeight;
			
			Invalidate();
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

		private static SizeF GetCharSize(Font font, char c)
		{
			Size sz2 = TextRenderer.MeasureText("<" + c.ToString() + ">", font);
			Size sz3 = TextRenderer.MeasureText("<>", font);
			return new SizeF(sz2.Width - sz3.Width + 1, font.Height);
		}

		private string text;
		public override string Text
		{
			get { return text; }
			set
			{
				if (text != value)
				{
					text = value;
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

		private Rectangle closeRect;

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			int width = Width;
			int x = charWidth;
			int indent = charWidth / 2;
			
			int rightIndent = charHeight;
			g.FillRectangle(scheme.tabsBgBrush, 0, 0, width - rightIndent, charHeight - 1);
			if (text != null)
			{
				for (int j = 0; j < text.Length; j++)
				{
					g.DrawString(
						text[j] + "", font, scheme.fgBrush,
						10 - charWidth / 3 + j * charWidth + charWidth / 2, 0, stringFormat);
				}
			}
			
			g.FillRectangle(scheme.tabsBgBrush, width - rightIndent, 0, rightIndent, charHeight - 1);
			
			closeRect = new Rectangle(width - charHeight, 0, charHeight, charHeight);
			g.DrawString("Х", font, scheme.tabsFgBrush, closeRect.X - charWidth / 3 + charWidth / 2, -1, stringFormat);
			g.DrawRectangle(scheme.lineNumberFgPen, 0, 0, width - 1, charHeight - 1);
			
			base.OnPaint(e);
		}
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Point location = e.Location;
			if (closeRect.Contains(location))
			{
				if (CloseClick != null)
					CloseClick();
			}
		}
		
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
		}
		
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
		}
	}
}
