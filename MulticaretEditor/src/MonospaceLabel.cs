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

namespace MulticaretEditor
{
	public class MonospaceLabel : Control
	{
		private StringFormat stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
		private SolidBrush bgBrush;
		private SolidBrush textBrush;

		public MonospaceLabel()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			TabStop = false;
			bgBrush = new SolidBrush(BackColor);
			textBrush = new SolidBrush(Color.Black);
			
			SetFont(FontFamily.GenericMonospace, 10.25f);
			Text = "";
		}

		private List<List<char>> lines = new List<List<char>>();
		
		private string text = "";
		public override string Text
		{
			get { return text; }
			set
			{
				if (text == value + "")
					return;
				text = value + "";
				lines.Clear();
				List<char> line = new List<char>();
				for (int i = 0; i < text.Length; i++)
				{
					char c = text[i];
					if (c == '\r')
					{
						if (i + 1 < text.Length && text[i + 1] == '\n')
							i++;
						lines.Add(line);
						line = new List<char>();
					}
					else if (c == '\n')
					{
						lines.Add(line);
						line = new List<char>();
					}
					else
					{
						line.Add(c);
					}
				}
				lines.Add(line);

				int sizeX = 0;
				int sizeY = lines.Count;
				for (int i = 0; i < lines.Count; i++)
				{
					int pos = 0;
					int count = line.Count;
					for (int j = 0; j < count; j++)
					{
						char c = line[j];
						if (c == '\t')
							pos = ((pos + tabSize) / tabSize) * tabSize;
						else
							pos++;
					}
					if (sizeX < pos)
						sizeX = pos;
				}
				Size = new Size(sizeX * charWidth, sizeY * charHeight);
			}
		}
		
		private int tabSize = 4;
		public int TabSize
		{
			get { return tabSize; }
			set
			{
				tabSize = value;
				Invalidate();
			}
		}

		public Color TextColor
		{
			get { return textBrush.Color; }
			set
			{
				textBrush = new SolidBrush(value);
				Invalidate();
			}
		}

		private Font font;
		
		private int charWidth;
		public int CharWidth { get { return charWidth; } }
		
		private int charHeight;
		public int CharHeight { get { return charHeight; } }
		
		private void SetFont(FontFamily family, float emSize)
		{
			fontFamily = family;
			fontSize = emSize;
			font = new Font(family, emSize);
			
			SizeF size = GetCharSize(font, 'M');
			charWidth = (int)Math.Round(size.Width * 1f) - 1;
			charHeight = (int)Math.Round(size.Height * 1f) + 1;
			
			Invalidate();
		}
		
		private float fontSize;
		public float FontSize
		{
			get { return fontSize; }
			set
			{
				if (fontSize != value)
					SetFont(fontFamily, value);
			}
		}
		
		private FontFamily fontFamily;
		public FontFamily FontFamily
		{
			get { return fontFamily; }
			set
			{
				if (fontFamily != value)
					SetFont(value, fontSize);
			}
		}
		
		private static SizeF GetCharSize(Font font, char c)
		{
			Size sz2 = TextRenderer.MeasureText("<" + c.ToString() + ">", font);
			Size sz3 = TextRenderer.MeasureText("<>", font);
			return new SizeF(sz2.Width - sz3.Width + 1, font.Height);
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
			e.Graphics.SmoothingMode = SmoothingMode.None;
			e.Graphics.Clear(BackColor);
			for (int i = 0, count = lines.Count; i < count; i++)
			{
				DrawLineChars(e.Graphics, new Point(0, i * charHeight), lines[i]);
			}
			base.OnPaint(e);
		}
		
		private void DrawLineChars(Graphics g, Point position, List<char> line)
		{
			int count = line.Count;
			float y = position.Y;
			float x = position.X - charWidth / 3;
			int pos = 0;
			for (int i = 0; i < count; i++)
			{
				char c = line[i];
				g.DrawString(c.ToString(), font, textBrush, x + charWidth * pos, y, stringFormat);
				if (c == '\t')
					pos = ((pos + tabSize) / tabSize) * tabSize;
				else
					pos++;
			}
		}
	}	
}
