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
	public class TabBar<T> : Control
	{
		public static string DefaultStringOf(T value)
		{
			return value + "";
		}

		public event Setter CloseClick;
		public event Setter<T> TabDoubleClick;

		private Timer arrowTimer;
		private StringFormat stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
		private readonly StringOfDelegate<T> stringOf;
		private readonly Point[] tempPoints;

		public TabBar(SwitchList<T> list, StringOfDelegate<T> stringOf)
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			this.stringOf = stringOf;
			TabStop = false;

			tempPoints = new Point[3];
			SetFont(FontFamily.GenericMonospace, 10.25f);

			arrowTimer = new Timer();
			arrowTimer.Interval = 150;
			arrowTimer.Tick += OnArrowTick;

			List = list;
		}

		private void OnSelectedChange()
		{
			needScrollToSelected = true;
			Invalidate();
		}

		private Getter<T, string> text2Of;
		public Getter<T, string> Text2Of
		{
			get { return text2Of; }
			set { text2Of = value; }
		}

		private SwitchList<T> list;
		public SwitchList<T> List
		{
			get { return list; }
			set
			{
				if (list != value)
				{
					if (list != null)
					{
						list.SelectedChange -= OnSelectedChange;
					}
					list = value;
					if (list != null)
					{
						list.SelectedChange += OnSelectedChange;
					}
				}
			}
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

		private string text2;
		public string Text2
		{
			get { return text2; }
			set
			{
				if (text2 != value)
				{
					text2 = value;
					Invalidate();
				}
			}
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

		public new void Invalidate()
		{
			if (InvokeRequired)
				BeginInvoke(new MethodInvoker(Invalidate));
			else
				base.Invalidate();
		}

		private PredictableList<Rectangle> rects = new PredictableList<Rectangle>();
		private Rectangle closeRect;
		private Rectangle? leftRect;
		private Rectangle? rightRect;
		private int leftIndent;
		private int rightIndent;
		private int offsetIndex;
		private bool needScrollToSelected;

		private void ScrollToSelectedIfNeed()
		{
			if (!needScrollToSelected)
				return;
			needScrollToSelected = false;
			int selectedIndex = list != null ? list.IndexOf(list.Selected) : -1;
			if (selectedIndex != -1)
			{
				if (offsetIndex > selectedIndex)
				{
					offsetIndex = selectedIndex;
				}
				else
				{
					for (int i = offsetIndex; i < list.Count; i++)
					{
						offsetIndex = i;
						if (rects.buffer[selectedIndex].Right + GetOffsetX(i) < Width - rightIndent)
							break;
					}
				}
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			int width = Width;
			int x = charWidth;
			int indent = charWidth / 2;

			Brush bgBrush = _selected ? scheme.tabsSelectedBgBrush : scheme.tabsBgBrush;
			Brush tabsFgBrush = _selected ? scheme.tabsSelectedFgBrush : scheme.tabsFgBrush;
			Pen tabsFgPen = _selected ? scheme.tabsSelectedFgPen : scheme.tabsFgPen;
			Pen linePen = scheme.tabsLinePen;

			g.FillRectangle(bgBrush, 0, 0, width - charWidth, charHeight - 1);
			g.DrawLine(linePen, 0, charHeight - 1, width, charHeight - 1);

			leftIndent = charWidth;
			if (text != null)
			{
				for (int j = 0; j < text.Length; j++)
				{
					g.DrawString(
						text[j] + "", font, tabsFgBrush,
						10 - charWidth / 3 + j * charWidth, 0, stringFormat);
				}
				leftIndent += (charWidth + 1) * text.Length;
			}

			rects.Clear();
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					T value = list[i];
					string tabText = stringOf(value);
					Rectangle rect = new Rectangle(x - indent, 0, tabText.Length * charWidth + indent * 2, charHeight);
					x += (tabText.Length + 1) * charWidth;
					rects.Add(rect);
				}
			}
			string text2;
			if (list != null && list.Selected != null && text2Of != null)
				text2 = text2Of(list.Selected);
			else
				text2 = this.text2;
			rightIndent = charHeight + (text2 != null ? text2.Length * charWidth : 0);
			if (x > width - leftIndent - rightIndent)
			{
				rightIndent += charHeight * 3 / 2;
				leftRect = new Rectangle(width - rightIndent, 0, charWidth * 3 / 2, charHeight);
				rightRect = new Rectangle(width - rightIndent + charWidth * 3 / 2, 0, charWidth * 3 / 2, charHeight);
				ScrollToSelectedIfNeed();
				if (offsetIndex < 0)
					offsetIndex = 0;
				else if (offsetIndex > rects.count - 1)
					offsetIndex = rects.count - 1;
				if (offsetIndex > 0)
				{
					for (int i = offsetIndex; i-- > 0;)
					{
						if (rects.buffer[rects.count - 1].Right + GetOffsetX(i) > width - rightIndent - 1)
							break;
						offsetIndex = i;
					}
				}
			}
			else
			{
				leftRect = null;
				rightRect = null;
				offsetIndex = 0;
			}
			if (list != null)
			{
				int offsetX = GetOffsetX(offsetIndex);
				for (int i = Math.Max(0, offsetIndex); i < list.Count; i++)
				{
					T value = list[i];
					string tabText = stringOf(value);
					bool selected = object.Equals(list.Selected, value);
					Rectangle rect = rects.buffer[i];
					rect.X += offsetX;
					if (rect.X > width)
						break;

					if (selected)
					{
						g.FillRectangle(scheme.bgBrush, rect);
						g.DrawRectangle(linePen, rect);
					}
					else
					{
						g.FillRectangle(scheme.lineNumberBackground, rect);
						g.DrawRectangle(linePen, rect.X, rect.Y, rect.Width, rect.Height - 1);
					}
					for (int j = 0; j < tabText.Length; j++)
					{
						g.DrawString(
							tabText[j] + "", font, selected ? scheme.fgBrush : scheme.lineNumberForeground,
							rect.X - charWidth / 3 + j * charWidth + charWidth / 2, 0, stringFormat);
					}
					rects.Add(rect);
				}
			}

			g.FillRectangle(bgBrush, width - rightIndent, 0, rightIndent, charHeight - 1);
			g.DrawLine(linePen, width - rightIndent, charHeight - 1, width, charHeight - 1);

			int closeWidth = charHeight * 12 / 10;
			closeRect = new Rectangle(width - closeWidth, 0, closeWidth, charHeight);
			{
				int tx = closeRect.X + closeRect.Width / 2;
				int ty = charHeight / 2;
				int td = charHeight / 5;
				g.DrawLine(tabsFgPen, tx - td, ty - td, tx + td, ty + td);
				g.DrawLine(tabsFgPen, tx + td, ty - td, tx - td, ty + td);
				g.DrawLine(tabsFgPen, tx - td + 1, ty - td, tx + td + 1, ty + td);
				g.DrawLine(tabsFgPen, tx + td + 1, ty - td, tx - td + 1, ty + td);
			}

			if (leftRect != null)
			{
				int tx = leftRect.Value.X + leftRect.Value.Width / 2;
				int ty = charHeight / 2;
				int td = charHeight / 6;
				tempPoints[0] = new Point(tx - td, ty);
				tempPoints[1] = new Point(tx + td, ty - td * 2);
				tempPoints[2] = new Point(tx + td, ty + td * 2);
				g.FillPolygon(tabsFgBrush, tempPoints);
			}
			if (rightRect != null)
			{
				int tx = rightRect.Value.X + rightRect.Value.Width / 2;
				int ty = charHeight / 2;
				int td = charHeight / 6;
				tempPoints[0] = new Point(tx + td, ty);
				tempPoints[1] = new Point(tx - td, ty - td * 2);
				tempPoints[2] = new Point(tx - td, ty + td * 2);
				g.FillPolygon(tabsFgBrush, tempPoints);
			}

			if (text2 != null)
			{
				int left = width - text2.Length * charWidth - charHeight * 3 / 2;
				for (int j = 0; j < text2.Length; j++)
				{
					g.DrawString(
						text2[j] + "", font, tabsFgBrush,
						left + charWidth * 2 / 3 + j * charWidth, 0, stringFormat);
				}
			}

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
			if (leftRect != null && leftRect.Value.Contains(location))
			{
				offsetIndex--;
				arrowTickDelta = -1;
				arrowTimer.Start();
				Invalidate();
			}
			if (rightRect != null && rightRect.Value.Contains(location))
			{
				offsetIndex++;
				arrowTickDelta = 1;
				arrowTimer.Start();
				Invalidate();
			}
			if (location.X < Width - rightIndent && list != null)
			{
				location.X -= GetOffsetX(offsetIndex);
				for (int i = 0; i < rects.count; i++)
				{
					if (rects.buffer[i].Contains(location))
					{
						if (i < list.Count)
							list.Selected = list[i];
						return;
					}
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			arrowTimer.Stop();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			arrowTimer.Stop();
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			Point location = e.Location;
			if (location.X < Width - rightIndent)
			{
				location.X -= GetOffsetX(offsetIndex);
				for (int i = 0; i < rects.count; i++)
				{
					if (rects.buffer[i].Contains(location))
					{
						if (i < list.Count)
						{
							if (TabDoubleClick != null)
								TabDoubleClick(list[i]);
						}
						return;
					}
				}
			}
		}

		private int GetOffsetX(int index)
		{
			return (index >= 0 && index < rects.count ? -rects.buffer[index].X : 0) + leftIndent;
		}

		private int arrowTickDelta;

		private void OnArrowTick(object senter, EventArgs e)
		{
			offsetIndex += arrowTickDelta;
			Invalidate();
		}

		private bool _selected;
		public bool Selected
		{
			get { return _selected; }
			set
			{
				if (_selected != value)
				{
					_selected = value;
					Invalidate();
				}
			}
		}
	}
}
