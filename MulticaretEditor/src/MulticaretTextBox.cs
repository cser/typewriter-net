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
	public class MulticaretTextBox : Control
	{
		public event Setter FocusedChange;

		private LineArray lines;
		private Controller controller;
		private StringFormat stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
		private StringFormat rightAlignFormat= new StringFormat(StringFormatFlags.DirectionRightToLeft);
		private int lineInterval = 0;
		private Timer cursorTimer;
		private Timer keyTimer;
		private Timer highlightingTimer;
		private bool isCursorTick = true;
		private KeyMapNode keyMap = new KeyMapNode(new KeyMap().SetDefault(), 0);
		private TextStyle[] styles;
		private readonly Brush bgBrush;

		public MulticaretTextBox()
		{
			bgBrush = new SolidBrush(BackColor);

			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			ImeMode = ImeMode.Off;
			Cursor = Cursors.IBeam;

			cursorTimer = new Timer();
			cursorTimer.Interval = 400;
			cursorTimer.Tick += OnCursorTick;

			keyTimer = new Timer();
			keyTimer.Interval = 50;
			keyTimer.Tick += OnKeyTick;

			highlightingTimer = new Timer();
			highlightingTimer.Interval = 50;
			highlightingTimer.Tick += OnHighlightingTick;

			SetFont(FontFamily.GenericMonospace, 10.25f);
			cursorTimer.Start();
			keyTimer.Start();
			highlightingTimer.Start();

			styles = Highlighter.GetDefaultStyles(scheme);

			InitScrollBars();
			Controller = new Controller(new LineArray());
		}

		public override string Text
		{
			get { return lines.GetText(); }
			set
			{
				lines.SetText(value);
				Invalidate();
			}
		}

		public Controller Controller
		{
			get { return controller; }
			set
			{
				if (controller != value)
				{
					controller = value;
					if (controller != null)
					{
						lines = controller.Lines;
						lines.wordWrap = wordWrap;
						lines.lineBreak = lineBreak;
						lines.SetTabSize(tabSize);
						lines.scroller.scrollingIndent = scrollingIndent;
						InitScrollByLines();
					}
					else
					{
						lines = null;
					}
					Invalidate();
				}
			}
		}

		private bool wordWrap = false;
		public bool WordWrap
		{
			get { return wordWrap; }
			set
			{
				wordWrap = value;
				if (lines != null && lines.wordWrap != wordWrap)
				{
					lines.wordWrap = wordWrap;
					Invalidate();
				}
			}
		}

		private string lineBreak = "\r\n";
		public string LineBreak
		{
			get { return lineBreak; }
			set
			{
				lineBreak = value;
				if (lines != null)
					lines.lineBreak = lineBreak;
			}
		}

		private bool showLineBreaks = false;
		public bool ShowLineBreaks
		{
			get { return showLineBreaks; }
			set
			{
				if (showLineBreaks != value)
				{
					showLineBreaks = value;
					Invalidate();
				}
			}
		}

		private int tabSize = 4;
		public int TabSize
		{
			get { return tabSize; }
			set
			{
				tabSize = value;
				if (lines != null)
					lines.SetTabSize(tabSize);
			}
		}

		private Scheme scheme = new Scheme();
		public Scheme Scheme
		{
			get { return scheme; }
			set
			{
				scheme = value;
				styles = highlighter != null ? highlighter.GetStyles(scheme) : Highlighter.GetDefaultStyles(scheme);
				BackColor = scheme.bgColor;
			}
		}

		private Highlighter highlighter;
		public Highlighter Highlighter
		{
			get { return highlighter; }
			set
			{
				highlighter = value;
				styles = highlighter != null ? highlighter.GetStyles(scheme) : Highlighter.GetDefaultStyles(scheme);
			}
		}

		private bool showColorAtCursor = false;
		public bool ShowColorAtCursor
		{
			get { return showColorAtCursor; }
			set
			{
				if (showColorAtCursor != value)
				{
					showColorAtCursor = value;
					Invalidate();
				}
			}
		}

		private Font font;
		private Font[] fonts = new Font[16];

		private int charWidth;
		public int CharWidth { get { return charWidth; } }

		private int charHeight;
		public int CharHeight { get { return charHeight; } }

		private void SetFont(FontFamily family, float emSize)
		{
			fontFamily = family;
			fontSize = emSize;

			fonts[TextStyle.NoneMask] = new Font(family, emSize);

			fonts[TextStyle.ItalicMask] = new Font(family, emSize, FontStyle.Italic);
			fonts[TextStyle.BoldMask] = new Font(family, emSize, FontStyle.Bold);
			fonts[TextStyle.UnderlineMask] = new Font(family, emSize, FontStyle.Underline);
			fonts[TextStyle.StrikeoutMask] = new Font(family, emSize, FontStyle.Strikeout);

			fonts[TextStyle.ItalicMask | TextStyle.BoldMask] = new Font(family, emSize, FontStyle.Italic | FontStyle.Bold);
			fonts[TextStyle.ItalicMask | TextStyle.UnderlineMask] = new Font(family, emSize, FontStyle.Italic | FontStyle.Underline);
			fonts[TextStyle.ItalicMask | TextStyle.StrikeoutMask] = new Font(family, emSize, FontStyle.Italic | FontStyle.Strikeout);

			fonts[TextStyle.BoldMask | TextStyle.UnderlineMask] = new Font(family, emSize, FontStyle.Bold | FontStyle.Underline);
			fonts[TextStyle.BoldMask | TextStyle.StrikeoutMask] = new Font(family, emSize, FontStyle.Bold | FontStyle.Strikeout);

			fonts[TextStyle.StrikeoutMask | TextStyle.UnderlineMask] = new Font(family, emSize, FontStyle.Strikeout | FontStyle.Underline);

			fonts[TextStyle.ItalicMask | TextStyle.BoldMask | TextStyle.UnderlineMask] =
				new Font(family, emSize, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline);
			fonts[TextStyle.ItalicMask | TextStyle.BoldMask | TextStyle.StrikeoutMask] =
				new Font(family, emSize, FontStyle.Italic | FontStyle.Bold | FontStyle.Strikeout);
			fonts[TextStyle.ItalicMask | TextStyle.UnderlineMask | TextStyle.StrikeoutMask] =
				new Font(family, emSize, FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
			fonts[TextStyle.BoldMask | TextStyle.UnderlineMask | TextStyle.StrikeoutMask] =
				new Font(family, emSize, FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);

			fonts[TextStyle.ItalicMask | TextStyle.BoldMask | TextStyle.UnderlineMask | TextStyle.StrikeoutMask] =
				new Font(family, emSize, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);

			font = fonts[TextStyle.NoneMask];

			SizeF size = GetCharSize(fonts[0], 'M');
			charWidth = (int)Math.Round(size.Width * 1f) - 1;
			charHeight = lineInterval + (int)Math.Round(size.Height * 1f) + 1;

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

		private int scrollingIndent = 2;
		public int ScrollingIndent
		{
			get { return scrollingIndent; }
			set
			{
				scrollingIndent = value;
				if (lines != null)
					lines.scroller.scrollingIndent = scrollingIndent;
			}
		}

		private bool showLineNumbers = true;
		public bool ShowLineNumbers
		{
			get { return showLineNumbers; }
			set
			{
				if (showLineNumbers != value)
				{
					showLineNumbers = value;
					Invalidate();
				}
			}
		}

		private bool highlightCurrentLine = true;
		public bool HighlightCurrentLine
		{
			get { return highlightCurrentLine; }
			set
			{
				if (highlightCurrentLine != value)
				{
					highlightCurrentLine = value;
					Invalidate();
				}
			}
		}

		private float mapScale = .3f;
		public float MapScale
		{
			get { return mapScale; }
			set
			{
				if (Math.Abs(mapScale - value) > .00001f)
				{
					mapScale = value;
					Invalidate();
				}
			}
		}

		private bool showMap = false;
		public bool ShowMap
		{
			get { return showMap; }
			set
			{
				if (showMap != value)
				{
					showMap = value;
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

        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int SB_ENDSCROLL = 0x8;

        protected override void WndProc(ref Message m)
        {
			if ((m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL) &&
				m.WParam.ToInt32() != SB_ENDSCROLL)
                Invalidate();
            base.WndProc(ref m);
        }

		public new void Invalidate()
		{
			if (InvokeRequired)
				BeginInvoke(new MethodInvoker(Invalidate));
			else
				base.Invalidate();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			UnblinkCursor();
			base.OnGotFocus(e);
			if (FocusedChange != null)
				FocusedChange();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			if (FocusedChange != null)
				FocusedChange();
		}

		private PredictableList<LineNumberInfo> lineNumberInfos = new PredictableList<LineNumberInfo>();
		private int mouseAreaRight;

		protected override void OnPaint(PaintEventArgs e)
		{
			#if debug
            Stopwatch sw = Stopwatch.StartNew();
            #endif

            if (lines == null)
				return;

            UpdateScrollOnPaint();

            int leftIndent = GetLeftIndent();
			int clientWidth = lines.scroller.textAreaWidth;
			int clientHeight = lines.scroller.textAreaHeight;
			int valueX = lines.scroller.scrollX.value;
			int valueY = lines.scroller.scrollY.value;

			e.Graphics.SmoothingMode = SmoothingMode.None;
			e.Graphics.Clear(scheme.bgColor);

			DrawText(e.Graphics, valueX, valueY, leftIndent, clientWidth, clientHeight);
			e.Graphics.FillRectangle(scheme.lineNumberBackground, 0, 0, leftIndent, clientHeight);
			if (showLineNumbers)
			{
				for (int i = 0; i < lineNumberInfos.count; i++)
				{
					LineNumberInfo info = lineNumberInfos.buffer[i];
					e.Graphics.DrawString(
						(info.iLine + 1) + "", font, scheme.lineNumberForeground, new RectangleF(0, info.y, leftIndent, charHeight), rightAlignFormat);
				}
			}

			mouseAreaRight = leftIndent + clientWidth;
			if (showMap)
			{
				e.Graphics.FillRectangle(scheme.bgBrush, leftIndent + clientWidth, 0, leftIndent + clientWidth + (int)(clientWidth / mapScale) + 1, clientHeight);
				e.Graphics.ScaleTransform(mapScale, mapScale);
				int offsetX = (int)((clientWidth + leftIndent) / mapScale);
				int mapValueY = GetMapValueY();
				mapRectangle = new RectangleF(clientWidth + leftIndent, (valueY - mapValueY) * mapScale, clientWidth * mapScale, clientHeight * mapScale);
				e.Graphics.FillRectangle(scheme.lineBgBrush, offsetX, valueY - mapValueY, clientWidth, clientHeight);
				DrawText(e.Graphics, 0, mapValueY, offsetX, clientWidth, (int)(clientHeight / mapScale));
				e.Graphics.ScaleTransform(1, 1);
			}

			if (lines.scroller.scrollX.visible && lines.scroller.scrollY.visible)
				e.Graphics.FillRectangle(bgBrush, ClientRectangle.Width - scrollBarBreadth, clientHeight, scrollBarBreadth, scrollBarBreadth);

			base.OnPaint(e);

			#if debug
            Console.WriteLine("OnPaint: " + sw.ElapsedMilliseconds + "ms");
			#endif
		}

		private RectangleF mapRectangle;
		private bool mapMouseDown;
		private int mapPageMouseOffset;

		private void DoMapMouseDown(MouseEventArgs e)
		{
			if (mapRectangle.Contains(e.Location))
			if (e.Button == MouseButtons.Left)
			{
				mapMouseDown = true;
				mapPageMouseOffset = (int)(e.Location.Y - mapRectangle.Y);
			}
		}

		private void DoMapMouseUp(MouseEventArgs e)
		{
			mapMouseDown = false;
		}

		private void DoMapMouseMove(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && mapMouseDown)
			{
				lines.scroller.ScrollValue(lines.scroller.scrollX.value, GetMapPageDraggingValueY(e.Location.Y));
				Invalidate();
			}
		}

		private int GetMapValueY()
		{
			int valueY = lines.scroller.scrollY.value;
			int maxValueY = lines.scroller.scrollY.contentSize - lines.scroller.scrollY.areaSize;
			if (maxValueY <= 0)
				return 0;
			int maxMapValueY = lines.scroller.scrollY.contentSize - (int)(lines.scroller.scrollY.areaSize / mapScale);
			if (maxMapValueY <= 0)
				return 0;
			if (valueY < 0)
				valueY = 0;
			else if (valueY > maxValueY)
				valueY = maxValueY;
			return valueY * maxMapValueY / maxValueY;
		}

		private int GetMapPageDraggingValueY(int mouseY)
		{
			int maxValueY = lines.scroller.scrollY.contentSize - lines.scroller.scrollY.areaSize;
			if (maxValueY <= 0)
				return 0;
			float ratio = (mouseY - mapPageMouseOffset) /
				(float)(Math.Min(lines.scroller.scrollY.areaSize, lines.scroller.scrollY.contentSize * mapScale) - lines.scroller.scrollY.areaSize * mapScale);
			int valueY = (int)(ratio * maxValueY);
			if (valueY < 0)
				valueY = 0;
			else if (valueY > maxValueY)
				valueY = maxValueY;
			return valueY;
		}

		private void DrawText(Graphics g, int valueX, int valueY, int leftIndent, int clientWidth, int clientHeight)
		{
			int offsetX = -valueX + leftIndent;
			int offsetY = -valueY;
			int linesCount = lines.LinesCount;
			LineIndex lineMin;
			LineIndex lineMax;
			int wwILineMin;
			int wwILineMax;
			if (lines.wordWrap)
			{
				wwILineMin = Math.Max(0, valueY / charHeight - 1);
				wwILineMax = Math.Min(lines.scroller.textSizeY, (valueY + clientHeight) / charHeight);
				lineMin = lines.wwValidator.GetLineIndexOfWW(wwILineMin);
				lineMax = lines.wwValidator.GetLineIndexOfWW(wwILineMax);
				if (lineMax.iLine < linesCount - 1)
				{
					lineMax.iLine++;
					lineMax.iSubline = 0;
				}
			}
			else
			{
				wwILineMin = Math.Max(0, valueY / charHeight - 1);
				wwILineMax = Math.Min(linesCount - 1, (valueY + clientHeight) / charHeight);
				lineMin = new LineIndex(wwILineMin, 0);
				lineMax = new LineIndex(wwILineMax, 0);
			}
			int minPos = Math.Max(0, valueX / charWidth - 1);
			int maxPos = Math.Min(lines.scroller.textSizeX, (valueX + clientWidth) / charWidth + 1);
			int start = lines.IndexOf(new Place(0, lineMin.iLine));
			int end = lines.IndexOf(new Place(lines[lineMax.iLine].chars.Count, lineMax.iLine));
			if (lines.wordWrap)
			{
				DrawSelections_WordWrap(leftIndent, start, end, g, lineMin, lineMax, offsetX, offsetY, clientWidth, clientHeight);
			}
			else
			{
				DrawSelections_Fixed(leftIndent, start, end, g, lineMin.iLine, lineMax.iLine, offsetX, offsetY, clientWidth, clientHeight);
			}
			lineNumberInfos.Clear();
			if (lines.wordWrap)
			{
				LineIterator iterator = lines.GetLineRange(lineMin.iLine, lineMax.iLine - lineMin.iLine + 1);
				if (iterator.MoveNext())
				{
					int y = offsetY + (wwILineMin - lineMin.iSubline) * charHeight;
					do
					{
						DrawLineChars(g, new Point(offsetX, y), iterator.current, minPos, maxPos);
						lineNumberInfos.Add(new LineNumberInfo(iterator.Index, y));
						y += (iterator.current.cutOffs.count + 1) * charHeight;
					}
					while (iterator.MoveNext());
				}
			}
			else
			{
				LineIterator iterator = lines.GetLineRange(lineMin.iLine, lineMax.iLine - lineMin.iLine + 1);
				if (iterator.MoveNext())
				{
					int y = offsetY + iterator.Index * charHeight;
					do
					{
						DrawLineChars(g, new Point(offsetX, y), iterator.current, minPos, maxPos);
						lineNumberInfos.Add(new LineNumberInfo(iterator.Index, y));
						y += charHeight;
					}
					while (iterator.MoveNext());
				}
			}
			{
				int selectionsCount = lines.selections.Count;
				for (int i = selectionsCount; i-- > 0;)
				{
					Selection selection = lines.selections[i];
					if (selection.Right < start || selection.Left > end)
						continue;
					Place caret = lines.PlaceOf(selection.caret);
					Line line = lines[caret.iLine];
					int x;
					int y;
					if (lines.wordWrap)
					{
						int wwILine = lines.wwValidator.GetWWILine(caret.iLine);
						Pos innerPos = line.WWPosOfIndex(caret.iChar);
						x = offsetX + innerPos.ix * charWidth;
						y = offsetY + (wwILine + innerPos.iy) * charHeight;
					}
					else
					{
						x = offsetX + line.PosOfIndex(caret.iChar) * charWidth;
						y = offsetY + caret.iLine * charHeight;
					}

					if (showColorAtCursor)
					{
						int offset;
						Color color;
						if (HighlighterUtil.GetRGBForHighlight(line.chars, caret.iChar, out offset, out color))
						{
							using (Pen pen = new Pen(color, 2))
								g.DrawLine(pen, x + offset * CharWidth, y + charHeight - 1, x + (offset + 6) * CharWidth, y + charHeight - 1);
						}
					}

					if (isCursorTick && Focused)
						g.DrawLine(i == selectionsCount - 1 ? scheme.mainCaretPen : scheme.caretPen, x, y, x, y + charHeight);
				}
			}
		}

		private PredictableList<DrawingLine> selectionRects = new PredictableList<DrawingLine>();

		private void DrawSelections_Fixed(
			int leftIndent,
			int start, int end, Graphics g, int iLineMin, int iLineMax, int offsetX, int offsetY, int clientWidth, int clientHeight)
		{
			if (lines.LastSelection.caret >= start && lines.LastSelection.caret <= end && highlightCurrentLine)
			{
				Place caret = lines.PlaceOf(lines.LastSelection.caret);
				g.FillRectangle(scheme.lineBgBrush, leftIndent, offsetY + caret.iLine * charHeight, clientWidth, charHeight);
			}
			foreach (Selection selection in lines.selections)
			{
				if (selection.Right < start || selection.Left > end || selection.Count == 0)
					continue;

				selectionRects.Clear();

				Place left = lines.PlaceOf(selection.Left);
				Line leftLine = lines[left.iLine];
				if (left.iChar + selection.Count <= leftLine.chars.Count)
				{
					int pos0 = leftLine.PosOfIndex(left.iChar);
					int pos1 = leftLine.PosOfIndex(left.iChar + selection.Count);
					selectionRects.Add(new DrawingLine(pos0, left.iLine, pos1 - pos0));
				}
				else
				{
					Place right = lines.PlaceOf(selection.Right);
					if (left.iLine >= iLineMin)
					{
						int pos0 = leftLine.PosOfIndex(left.iChar);
						selectionRects.Add(new DrawingLine(pos0, left.iLine, leftLine.Size - pos0));
					}
					if (right.iLine <= iLineMax)
					{
						Line line = lines[right.iLine];
						int pos1 = line.PosOfIndex(right.iChar);
						selectionRects.Add(new DrawingLine(0, right.iLine, pos1));
					}
					int i0 = left.iLine + 1;
					int i1 = right.iLine;
					if (i1 > i0)
					{
						if (i0 < iLineMin)
							i0 = iLineMin;
						if (i1 > iLineMax + 1)
							i1 = iLineMax + 1;
						for (int i = i0; i < i1; i++)
						{
							selectionRects.Add(new DrawingLine(0, i, lines[i].Size));
						}
					}
				}

				DrawSelection(g, selectionRects, offsetX, offsetY);
				selectionRects.Clear();
			}
		}

		private void DrawSelections_WordWrap(
			int leftIndent,
			int start, int end, Graphics g, LineIndex iLineMin, LineIndex iLineMax, int offsetX, int offsetY, int clientWidth, int clientHeight)
		{
			if (lines.LastSelection.caret >= start && lines.LastSelection.caret <= end && highlightCurrentLine)
			{
				Place caret = lines.PlaceOf(lines.LastSelection.caret);
				Line line = lines[caret.iLine];
				int wwILine = lines.wwValidator.GetWWILine(caret.iLine);
				g.FillRectangle(scheme.lineBgBrush, leftIndent, offsetY + wwILine * charHeight, clientWidth, charHeight * (line.cutOffs.count + 1));
			}
			foreach (Selection selection in lines.selections)
			{
				if (selection.Right < start || selection.Left > end || selection.Count == 0)
					continue;

				selectionRects.Clear();

				Place left = lines.PlaceOf(selection.Left);
				Line leftLine = lines[left.iLine];
				int leftILine = lines.wwValidator.GetWWILine(left.iLine);
				if (left.iChar + selection.Count <= leftLine.chars.Count)
				{
					Pos pos0 = leftLine.WWPosOfIndex(left.iChar);
					Pos pos1 = leftLine.WWPosOfIndex(left.iChar + selection.Count);
					if (pos0.iy == pos1.iy)
					{
						selectionRects.Add(new DrawingLine(pos0.ix, leftILine + pos0.iy, pos1.ix - pos0.ix));
					}
					else
					{
						selectionRects.Add(new DrawingLine(pos0.ix, leftILine + pos0.iy, leftLine.GetSublineSize(pos0.iy) - pos0.ix));
						int sublineLeft;
						for (int iy = pos0.iy + 1; iy < pos1.iy; iy++)
						{
							sublineLeft = leftLine.GetSublineLeft(iy);
							selectionRects.Add(new DrawingLine(sublineLeft, leftILine + iy, leftLine.GetSublineSize(iy) - sublineLeft));
						}
						sublineLeft = leftLine.GetSublineLeft(pos1.iy);
						selectionRects.Add(new DrawingLine(sublineLeft, leftILine + pos1.iy, pos1.ix - sublineLeft));
					}
				}
				else
				{
					if (left.iLine >= iLineMin.iLine)
					{
						int sublineLeft;
						Pos pos0 = leftLine.WWPosOfIndex(left.iChar);
						selectionRects.Add(new DrawingLine(pos0.ix, leftILine + pos0.iy, leftLine.GetSublineSize(0) - pos0.ix));
						for (int iy = pos0.iy + 1; iy <= leftLine.cutOffs.count; iy++)
						{
							sublineLeft = leftLine.GetSublineLeft(iy);
							selectionRects.Add(new DrawingLine(sublineLeft, leftILine + iy, leftLine.GetSublineSize(iy) - sublineLeft));
						}
					}
					Place right = lines.PlaceOf(selection.Right);
					int rightILine = lines.wwValidator.GetWWILine(right.iLine);
					if (right.iLine <= iLineMax.iLine)
					{
						int sublineLeft;
						Line line = lines[right.iLine];
						Pos pos1 = line.WWPosOfIndex(right.iChar);
						for (int iy = 0; iy < pos1.iy; iy++)
						{
							sublineLeft = line.GetSublineLeft(iy);
							selectionRects.Add(new DrawingLine(sublineLeft, rightILine + iy, line.GetSublineSize(iy) - sublineLeft));
						}
						sublineLeft = line.GetSublineLeft(pos1.iy);
						selectionRects.Add(new DrawingLine(sublineLeft, rightILine + pos1.iy, pos1.ix - sublineLeft));
					}
					int i0 = left.iLine + 1;
					int i1 = right.iLine;
					if (i1 > i0)
					{
						if (i0 < iLineMin.iLine)
							i0 = iLineMin.iLine;
						if (i1 > iLineMax.iLine + 1)
							i1 = iLineMax.iLine + 1;
						int wwILine = lines.wwValidator.GetWWILine(i0);
						for (int i = i0; i < i1; i++)
						{
							Line line = lines[i];
							for (int iy = 0; iy <= line.cutOffs.count; iy++)
							{
								int sublineLeft = line.GetSublineLeft(iy);
								selectionRects.Add(new DrawingLine(sublineLeft, wwILine + iy, line.GetSublineSize(iy) - sublineLeft));
							}
							wwILine += line.cutOffs.count + 1;
						}
					}
				}

				DrawSelection(g, selectionRects, offsetX, offsetY);
				selectionRects.Clear();
			}
		}

		private void DrawSelection(Graphics g, PredictableList<DrawingLine> rects, int offsetX, int offsetY)
		{
			Pen selectionPen = scheme.selectionPen;
			for (int i = 0; i < rects.count; i++)
			{
				DrawingLine rectangle = rects.buffer[i];
				g.DrawRectangle(
					selectionPen,
					offsetX + rectangle.ix * charWidth,
					offsetY + rectangle.iy * charHeight + lineInterval / 2,
					rectangle.sizeX * charWidth,
					charHeight);
			}
			Brush selectionBrush = scheme.selectionBrush;
			for (int i = 0; i < rects.count; i++)
			{
				DrawingLine rectangle = rects.buffer[i];
				g.FillRectangle(
					selectionBrush,
					offsetX + rectangle.ix * charWidth,
					offsetY + rectangle.iy * charHeight + lineInterval / 2,
					rectangle.sizeX * charWidth,
					charHeight);
			}
		}

		private int GetLeftIndent()
		{
			int result = 0;
			if (showLineNumbers)
			{
				int digit = lines.LinesCount;
				int i = 0;
				for (; digit > 0; i++)
				{
					digit /= 10;
				}
				result = (i + 1) * charWidth;
			}
			return result;
		}

		private void DrawLineChars(Graphics g, Point position, Line line, int minPos, int maxPos)
		{
			int size = line.Size;
			int count = line.chars.Count;
			int start = Math.Min(minPos, size);
			int end = Math.Min(maxPos, size);
			int tabSize = lines.tabSize;
			float y = position.Y + lineInterval / 2;
			float x = position.X - charWidth / 3;

			if (lines.wordWrap)
			{
				for (int iCutOff = 0; iCutOff <= line.cutOffs.count; iCutOff++)
				{
					int pos = 0;
					int i0 = 0;
					if (iCutOff > 0)
					{
						CutOff cutOff = line.cutOffs.buffer[iCutOff - 1];
						pos = cutOff.left;
						i0 = cutOff.iChar;
					}
					int i1 = iCutOff < line.cutOffs.count ? line.cutOffs.buffer[iCutOff].iChar : line.chars.Count;
					for (int i = i0; i < i1; i++)
					{
						Char c = line.chars[i];
						if (showLineBreaks && c.c == '\r')
						{
							g.DrawString("▇", font, scheme.fgBrush, x + charWidth * pos, y, stringFormat);
							g.DrawString("r", font, scheme.bgBrush, x + charWidth * pos, y, stringFormat);
						}
						else if (showLineBreaks && c.c == '\n')
						{
							g.DrawString("▇", font, scheme.fgBrush, x + charWidth * pos, y, stringFormat);
							g.DrawString("n", font, scheme.bgBrush, x + charWidth * pos, y, stringFormat);
						}
						else
						{
							TextStyle style = styles[c.style];
							g.DrawString(c.c.ToString(), fonts[style.fontStyle], style.brush, x + charWidth * pos, y, stringFormat);
						}
						if (c.c == '\t')
						{
							pos = ((pos + tabSize) / tabSize) * tabSize;
						}
						else
						{
							pos++;
						}
					}
					y += charHeight;
				}
			}
			else
			{
				int pos = 0;
				for (int i = 0; i < count; i++)
				{
					if (pos > maxPos)
						break;
					Char c = line.chars[i];
					if (pos >= minPos)
					{
						if (showLineBreaks && c.c == '\r')
						{
							g.DrawString("▇", font, scheme.fgBrush, x + charWidth * pos, y, stringFormat);
							g.DrawString("r", font, scheme.bgBrush, x + charWidth * pos, y, stringFormat);
						}
						else if (showLineBreaks && c.c == '\n')
						{
							g.DrawString("▇", font, scheme.fgBrush, x + charWidth * pos, y, stringFormat);
							g.DrawString("n", font, scheme.bgBrush, x + charWidth * pos, y, stringFormat);
						}
						else
						{
							TextStyle style = styles[c.style];
							g.DrawString(c.c.ToString(), fonts[style.fontStyle], style.brush, x + charWidth * pos, y, stringFormat);
						}
					}
					if (c.c == '\t')
					{
						pos = ((pos + tabSize) / tabSize) * tabSize;
					}
					else
					{
						pos++;
					}
				}
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
        {
			e.Graphics.FillRectangle(scheme.bgBrush, ClientRectangle);
        }

		private void OnCursorTick(object sender, EventArgs e)
		{
			isCursorTick = !isCursorTick;
			Invalidate();
		}

		private Keys modePressedKeys = Keys.None;

		private void OnKeyTick(object sender, EventArgs e)
		{
			if (Focused)
			{
				if (modePressedKeys != Control.ModifierKeys)
				{
					if (modePressedKeys != Keys.None)
						keyMap.Enumerate<bool>(ProcessKeyTick, false);
					modePressedKeys = Control.ModifierKeys;
					if (modePressedKeys != Keys.None)
						keyMap.Enumerate<bool>(ProcessKeyTick, true);
				}
			}
		}

		private bool ProcessKeyTick(KeyMap keyMap, bool mode)
		{
			if (keyMap != null)
			{
				IRList<KeyItem> items = keyMap.GetModeItems(modePressedKeys);
				for (int i = 0; i < items.Count; i++)
				{
					KeyItem item = items[i];
					if (item.action != null && item.action.doOnModeChange != null)
						item.action.doOnModeChange(controller, mode);
				}
			}
			return false;
		}

		private void OnHighlightingTick(object sender, EventArgs e)
		{
			if (highlighter != null && highlighter.Parse(lines))
				Invalidate();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				cursorTimer.Dispose();
			}
		}

		private void UnblinkCursor()
		{
			cursorTimer.Stop();
			cursorTimer.Start();
			isCursorTick = true;
			Invalidate();
		}

		//---------------------------------
		// Keys
		//---------------------------------

		protected override bool IsInputKey(Keys keyData)
		{
			return true;
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			if (!Focused)
				return false;

			char altChar;
			if (!actionProcessed && (Control.ModifierKeys & Keys.Alt) != 0 && keyMap.main.GetAltChar(charCode, out altChar))
			{
				controller.InsertText(altChar + "");
				actionProcessed = false;
				return true;
			}
			return false;
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			char code = e.KeyChar;
			if (Focused && !actionProcessed)
			{
				switch (code)
				{
					case '\b':
						if (lines.AllSelectionsEmpty)
						{
							controller.Backspace();
						}
						else
						{
							controller.EraseSelection();
						}
						break;
					case '\r':
						controller.InsertLineBreak();
						break;
					default:
						controller.InsertText(code + "");
						break;
				}
				if (highlighter != null && !highlighter.LastParsingChanged)
					highlighter.Parse(lines, 100);
				UnblinkCursor();
				ScrollIfNeedToCaret();
			}
			actionProcessed = false;
			base.OnKeyPress(e);
		}

		private bool actionProcessed = false;

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!Focused)
				return;

			actionProcessed = false;
			keyMap.Enumerate<Keys>(ProcessKeyDown, e.KeyData);
		}

		private bool ProcessKeyDown(KeyMap keyMap, Keys keyData)
		{
			if (!actionProcessed)
			{
				KeyItem keyItem = keyMap.GetItem(keyData);
				while (keyItem != null)
				{
					KeyAction action = keyItem.action;
					if (action == null)
						break;
					if (keyItem.modeKeys == null || (keyItem.modeKeys.Value & modePressedKeys) == keyItem.modeKeys.Value)
					if (action.doOnDown(controller))
					{
						actionProcessed = true;
						UnblinkCursor();
						if (action.needScroll)
							ScrollIfNeedToCaret();
						return true;
					}
					keyItem = keyItem.next;
				}
			}
			return false;
		}

		private bool ProcessDoubleClick(KeyMap keyMap, bool _)
		{
			if (!actionProcessed)
			{
				for (KeyItem keyItem = keyMap.GetDoubleClickItem(); keyItem != null; keyItem = keyItem.next)
				{
					KeyAction action = keyItem.action;
					if (action != null)
					if (keyItem.modeKeys == null && modePressedKeys == Keys.None ||
						keyItem.modeKeys != null && (keyItem.modeKeys.Value & modePressedKeys) == keyItem.modeKeys.Value)
					if (action.doOnDown(controller))
					{
						actionProcessed = true;
						UnblinkCursor();
						if (action.needScroll)
							ScrollIfNeedToCaret();
						return true;
					}
				}
			}
			return false;
		}

		private bool isMouseDown = false;
		private int mouseDownIndex = 0;
		private Point lastMouseDownLocation;
		private long lastMouseDownTicks;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Location.X > mouseAreaRight)
			{
				if (showMap)
					DoMapMouseDown(e);
				return;
			}
			Focus();
			if (mouseDownIndex == 1 &&
				(DateTime.Now.Ticks - lastMouseDownTicks) / 10000 < 500 &&
				(Math.Abs(lastMouseDownLocation.X - e.Location.X) + Math.Abs(lastMouseDownLocation.Y - e.Location.Y) < charWidth / 2))
				mouseDownIndex = 2;
			else
				mouseDownIndex = 1;
			lastMouseDownLocation = e.Location;
			lastMouseDownTicks = DateTime.Now.Ticks;

			if (mouseDownIndex == 1)
			{
				if (e.Button == MouseButtons.Left)
				isMouseDown = true;
				if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
				{
					if (Control.ModifierKeys == Keys.Control)
					{
						controller.PutNewCursor(GetMousePlace(e.Location));
					}
					else
					{
						controller.ClearMinorSelections();
						controller.PutCursor(GetMousePlace(e.Location), false);
					}
					UnblinkCursor();
				}
			}
			else if (mouseDownIndex == 2)
			{
				mouseDownIndex = 0;
				actionProcessed = false;
				if (!keyMap.Enumerate<bool>(ProcessDoubleClick, false))
					controller.SelectWordAtPlace(GetMousePlace(e.Location), (Control.ModifierKeys & Keys.Control) != 0);
				Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (showMap)
				DoMapMouseUp(e);
			if (e.Button == MouseButtons.Left)
				isMouseDown = false;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (showMap)
				DoMapMouseMove(e);
			if (e.Button == MouseButtons.Left && mouseDownIndex == 1 && isMouseDown)
			{
				controller.PutCursor(GetMousePlace(e.Location), true);
				UnblinkCursor();
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			int delta = (int)Math.Round((float)e.Delta / 120f) * GetControlPanelWheelScrollLinesValue();
			if ((Control.ModifierKeys & Keys.Shift) != 0)
			{
				hScrollBar.Value = CommonHelper.Clamp(hScrollBar.Value - delta * charWidth, 0, lines.scroller.scrollX.contentSize - lines.scroller.scrollX.areaSize);
			}
			else
			{
				vScrollBar.Value = CommonHelper.Clamp(vScrollBar.Value - delta * charHeight, 0, lines.scroller.scrollY.contentSize - lines.scroller.scrollY.areaSize);
			}
			if (isMouseDown)
			{
				UpdateScrollOnPaint();
				controller.PutCursor(GetMousePlace(e.Location), true);
			}
			UnblinkCursor();
		}

		private Pos GetMousePos(Point location)
		{
			return new Pos(
				(location.X + charWidth / 3 + lines.scroller.scrollX.value - GetLeftIndent()) / charWidth,
				(location.Y + lines.scroller.scrollY.value) / charHeight);
		}

		private Place GetMousePlace(Point location)
		{
			Pos pos = GetMousePos(location);
			return lines.UniversalPlaceOf(pos);
		}

		private static int GetControlPanelWheelScrollLinesValue()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false))
                {
                    return Convert.ToInt32(key.GetValue("WheelScrollLines"));
                }
            }
            catch
            {
                return 3;
            }
        }

		//----------------------------------------------------------
		// Scrolling
		//----------------------------------------------------------

		public KeyMapNode KeyMap { get { return keyMap; } }

		public void MoveToCaret()
		{
			ScrollIfNeedToCaret();
			Invalidate();
		}

		private void ScrollIfNeedToCaret()
		{
			lines.scroller.needScrollToCaret = true;
		}

		private ScrollBar hScrollBar;
		private ScrollBar vScrollBar;
		private int scrollBarBreadth;

		private void InitScrollBars()
		{
			SuspendLayout();

			hScrollBar = new HScrollBar();
			hScrollBar.Cursor = Cursors.Default;
			hScrollBar.SmallChange = charWidth;
			Controls.Add(hScrollBar);

			vScrollBar = new VScrollBar();
			vScrollBar.Cursor = Cursors.Default;
			vScrollBar.SmallChange = charHeight;
			vScrollBar.Scroll += OnVScroll;
			Controls.Add(vScrollBar);

			scrollBarBreadth = vScrollBar.Width;
			AlignScrollBars();

			ResumeLayout(false);
		}

		private void InitScrollByLines()
		{
			ScrollBarData scrollX = lines.scroller.scrollX;
			ScrollBarData scrollY = lines.scroller.scrollY;
			scrollX.ApplyParamsTo(hScrollBar);
			scrollY.ApplyParamsTo(vScrollBar);
			hScrollBar.Value = scrollX.ClampValue(scrollX.value);
			vScrollBar.Value = scrollY.ClampValue(scrollY.value);
		}

		private void OnVScroll(object target, ScrollEventArgs args)
		{
			if (args.Type == ScrollEventType.EndScroll)
				lines.scroller.needVScrollFix = true;
		}

		private void AlignScrollBars()
		{
			int clientWidth = ClientRectangle.Width;
			int clientHeight = ClientRectangle.Height;
			hScrollBar.Top = clientHeight - scrollBarBreadth;
			vScrollBar.Left = clientWidth - scrollBarBreadth;
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			AlignScrollBars();
		}

		public void UpdateScrollOnPaint()
		{
			int leftIndent = GetLeftIndent();
			ScrollOnPaintInfo info = new ScrollOnPaintInfo();
			info.leftIndent = leftIndent;
			if (showMap)
				info.width = leftIndent + (int)((ClientRectangle.Width - leftIndent) / (mapScale + 1));
			else
				info.width = ClientRectangle.Width;
			info.height = ClientRectangle.Height;
			info.charSize = new IntSize(charWidth, charHeight);
			info.scrollBarBreadth = scrollBarBreadth;

			int valueX = hScrollBar.Value;
			int valueY = vScrollBar.Value;
			lines.scroller.UpdateScrollOnPaint(info, ref valueX, ref valueY);

			lines.scroller.scrollX.ApplyParamsTo(hScrollBar);
			lines.scroller.scrollY.ApplyParamsTo(vScrollBar);
			hScrollBar.Width = ClientRectangle.Width - (lines.scroller.scrollY.visible ? scrollBarBreadth : 0);
			vScrollBar.Height = lines.scroller.textAreaHeight;
			hScrollBar.Value = valueX;
			vScrollBar.Value = valueY;
		}

		public int GetScrollSizeY()
		{
			ScrollOnPaintInfo info = new ScrollOnPaintInfo();
			info.width = ClientRectangle.Width;
			info.height = ClientRectangle.Height;
			info.leftIndent = GetLeftIndent();
			info.charSize = new IntSize(charWidth, charHeight);
			info.scrollBarBreadth = scrollBarBreadth;

			int valueX = hScrollBar.Value;
			int valueY = vScrollBar.Value;
			lines.scroller.UpdateScrollOnPaint(info, ref valueX, ref valueY);
			return lines.wwSizeY;
		}
	}
}
