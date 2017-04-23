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
	public class TabBar<T> : Control
	{
		public static string DefaultStringOf(T value)
		{
			return value + "";
		}

		public event Setter CloseClick;
		public event Setter<T> TabClick;
		public event Setter<T> TabDoubleClick;
		public event Setter NewTabDoubleClick;

		private Timer arrowTimer;
		private StringFormat stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
		private readonly StringOfDelegate<T> stringOf;
		private readonly StringOfDelegate<T> hintOf;
		private readonly Point[] tempPoints;
		private readonly Point[] tempPoints2;

		public TabBar(SwitchList<T> list, StringOfDelegate<T> stringOf, StringOfDelegate<T> hintOf)
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			this.stringOf = stringOf;
			this.hintOf = hintOf;
			TabStop = false;

			tempPoints = new Point[3];
			tempPoints2 = new Point[5];
			SetFont(FontFamily.GenericMonospace, 10.25f);

			arrowTimer = new Timer();
			arrowTimer.Interval = 150;
			arrowTimer.Tick += OnArrowTick;

			List = list;
		}
		
		public TabBar(SwitchList<T> list, StringOfDelegate<T> stringOf) : this(list, stringOf, null)
		{
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
		
		private bool buttonMode;
		public bool ButtonMode
		{
			get { return buttonMode; }
			set
			{
				if (buttonMode != value)
				{
					buttonMode = value;
					Invalidate();
				}
			}
		}

		private Font font;
		private Font boldFont;
		private int charWidth;
		private int charHeight;

		private void SetFont(FontFamily family, float emSize)
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
			return new SizeF(sz2.Width - sz3.Width + 1, font.Height + 4);
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
		private bool needScrollToSelected = true;

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
		
		protected override void OnResize(EventArgs e)
		{
			needScrollToSelected = true;
			base.OnResize(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			int width = Width;
			int x = charWidth;
			int indent = charWidth;
			int yOffset = 3;

			g.FillRectangle(scheme.tabsBg.brush, 0, 0, width - charWidth, charHeight);

			leftIndent = charWidth;
			if (text != null)
			{
				Brush fg = scheme.tabsFg.brush;
				for (int j = 0; j < text.Length; j++)
				{
					g.DrawString(text[j] + "", font, fg, 10 - charWidth / 3 + j * charWidth, yOffset, stringFormat);
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
					x += tabText.Length * charWidth + indent * 2;
					rects.Add(rect);
				}
			}
			string text2;
			if (list != null && list.Selected != null && text2Of != null)
				text2 = text2Of(list.Selected);
			else
				text2 = this.text2;
			rightIndent = charHeight * 5 / 4 + (text2 != null ? text2.Length * charWidth : 0);
			if (x > width - leftIndent - rightIndent)
			{
				rightIndent += charHeight * 5 / 4;
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
			int selectedX0 = 0;
			int selectedX1 = 0;
			if (list != null)
			{
				int offsetX = GetOffsetX(offsetIndex);
				bool prevSelected = false;
				for (int i = Math.Max(0, offsetIndex - 1); i < list.Count; i++)
				{
					T value = list[i];
					string tabText = stringOf(value);
					bool isCurrent = !buttonMode && object.Equals(list.Selected, value);
					Rectangle rect = rects.buffer[i];
					rect.X += offsetX;
					if (rect.X > width)
						break;

					if (isCurrent)
					{
						g.FillRectangle(scheme.tabsSelectedBg.brush,
							rect.X, rect.Y + 1, rect.Width - 1, rect.Height - 1);
						selectedX0 = rect.X;
						selectedX1 = rect.X + rect.Width;
					}
					if (!isCurrent)
					{
						if (buttonMode)
						{
							g.FillRectangle(scheme.buttonBgBrush,
								rect.X + 1, rect.Y + 2, rect.Width - 2, rect.Height - 4);
						}
						else
						{
							g.FillRectangle(scheme.tabsUnselectedBg.brush,
								rect.X, rect.Y + 1, rect.Width - 1, rect.Height - 2);
						}
					}
					Brush currentFg = isCurrent ? scheme.tabsSelectedFg.brush : scheme.tabsUnselectedFg.brush;
					if (buttonMode)
					{
						currentFg = scheme.buttonFgBrush;
					}
					for (int j = 0; j < tabText.Length; j++)
					{
						int charX = rect.X - charWidth / 3 + j * charWidth + indent;
						if (charX > 0 && charX < width - rightIndent - charWidth * 2)
						{
							g.DrawString(tabText[j] + "", font, currentFg, charX, yOffset, stringFormat);
						}
					}
					rects.Add(rect);
					prevSelected = isCurrent;
				}
			}

			int fictiveIndent = rightIndent - charHeight / 4;
			{
				tempPoints2[0] = new Point(width - fictiveIndent - charHeight / 2, charHeight / 2);
				tempPoints2[1] = new Point(width - fictiveIndent, 0);
				tempPoints2[2] = new Point(width, 0);
				tempPoints2[3] = new Point(width, charHeight);
				tempPoints2[4] = new Point(width - fictiveIndent + (charHeight % 2), charHeight);
				g.FillPolygon(_selected ? scheme.tabsInfoBg.brush : scheme.tabsBg.brush, tempPoints2);
				Pen pen = _selected ? scheme.tabsBg.pen : scheme.tabsInfoBg.pen;
				g.DrawLine(pen,
					new Point(width - fictiveIndent, 0),
					new Point(width - fictiveIndent - charHeight / 2, charHeight / 2));
				g.DrawLine(pen,
					new Point(width - fictiveIndent - charHeight / 2, charHeight / 2),
					new Point(width - fictiveIndent + (charHeight % 2), charHeight));
			}
			
			Brush infoBrush = _selected ? scheme.tabsInfoFg.brush : scheme.tabsFg.brush;
			Pen infoPen = _selected ? scheme.tabsInfoFg.pen : scheme.tabsFg.pen;

			int closeWidth = charHeight * 12 / 10;
			closeRect = new Rectangle(width - closeWidth, 0, closeWidth, charHeight);
			{
				int tx = closeRect.X + closeRect.Width / 2 + 1;
				int ty = charHeight / 2;
				int td = 3;
				g.DrawLine(infoPen, tx - td, ty - td, tx + td + 1, ty + td + 1);
				g.DrawLine(infoPen, tx - td + 1, ty - td, tx + td + 1, ty + td);
				g.DrawLine(infoPen, tx - td + 1, ty - td - 1, tx + td + 2, ty + td);
				g.DrawLine(infoPen, tx + td + 1, ty - td - 1, tx - td, ty + td);
				g.DrawLine(infoPen, tx + td + 1, ty - td, tx - td + 1, ty + td);
				g.DrawLine(infoPen, tx + td + 2, ty - td, tx - td + 1, ty + td + 1);
			}

			if (leftRect != null)
			{
				int tx = leftRect.Value.X + leftRect.Value.Width / 2;
				int ty = charHeight / 2;
				int td = charHeight / 6;
				tempPoints[0] = new Point(tx - td, ty);
				tempPoints[1] = new Point(tx + td, ty - td * 2);
				tempPoints[2] = new Point(tx + td, ty + td * 2);
				g.FillPolygon(infoBrush, tempPoints);
			}
			if (rightRect != null)
			{
				int tx = rightRect.Value.X + rightRect.Value.Width / 2;
				int ty = charHeight / 2;
				int td = charHeight / 6;
				tempPoints[0] = new Point(tx + td, ty);
				tempPoints[1] = new Point(tx - td, ty - td * 2);
				tempPoints[2] = new Point(tx - td, ty + td * 2);
				g.FillPolygon(infoBrush, tempPoints);
			}

			if (text2 != null)
			{
				int left = width - text2.Length * charWidth - charHeight * 3 / 2;
				for (int j = 0; j < text2.Length; j++)
				{
					g.DrawString(
						text2[j] + "", font, infoBrush,
						left + charWidth * 2 / 3 + j * charWidth, yOffset - 2, stringFormat);
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
						{
							if (buttonMode)
							{
								if (TabClick != null)
									TabClick(list[i]);
							}
							else
							{
								list.Selected = list[i];
							}
						}
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
			int x = location.X;
			int y = location.Y;
			if (x < Width - rightIndent)
			{
				x -= GetOffsetX(offsetIndex);
				for (int i = 0; i < rects.count; i++)
				{
					if (rects.buffer[i].Contains(x, y))
					{
						if (i < list.Count)
						{
							if (TabDoubleClick != null)
								TabDoubleClick(list[i]);
							return;
						}
						break;
					}
				}
			}
			if (leftRect != null && leftRect.Value.Contains(location))
			{
				return;
			}
			if (rightRect != null && rightRect.Value.Contains(location))
			{
				return;
			}
			if (closeRect != null && closeRect.Contains(location))
			{
				return;
			}
			if (NewTabDoubleClick != null)
				NewTabDoubleClick();
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
		
		private string rightHint;
		public string RightHint
		{
			get { return rightHint; }
			set { rightHint = value; }
		}
		
		private string showingHint;
		
		private ToolTip toolTip;
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			int locationX;
			string hint = GetHintText(e.Location, out locationX);
			TryShowHint(hint, locationX);
			base.OnMouseMove(e);
		}
		
		private void TryShowHint(string hint, int locationX)
		{
			if (showingHint != hint)
			{
				showingHint = hint;
				if (!string.IsNullOrEmpty(showingHint))
				{
					if (toolTip == null)
					{
						toolTip = new ToolTip();
					}
					toolTip.Show(showingHint, this, locationX, 20);
				}
				else
				{
					if (toolTip != null)
					{
						toolTip.Dispose();
						toolTip = null;
					}
				}
			}
		}
		
		private string GetHintText(Point location, out int locationX)
		{
			locationX = 0;
			int x = location.X;
			int y = location.Y;
			if (x < Width - rightIndent)
			{
				x -= GetOffsetX(offsetIndex);
				for (int i = 0; i < rects.count; i++)
				{
					if (rects.buffer[i].Contains(x, y))
					{
						if (i < list.Count)
						{
							if (hintOf != null)
							{
								string hintText = hintOf(list[i]);
								if (!string.IsNullOrEmpty(hintText))
								{
									locationX = rects.buffer[i].X + GetOffsetX(offsetIndex);
									return hintText;
								}
							}
						}
						break;
					}
				}
				return null;
			}
			if (leftRect != null && leftRect.Value.Contains(location))
			{
				return null;
			}
			if (rightRect != null && rightRect.Value.Contains(location))
			{
				return null;
			}
			if (closeRect != null && closeRect.Contains(location))
			{
				return null;
			}
			locationX = Width - rightIndent;
			return rightHint;
		}
		
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			TryShowHint(null, 0);
		}
	}
}
