using System;

namespace MulticaretEditor
{
	public class Scroller
	{
		private readonly LineArray lines;

		public Scroller(LineArray lines)
		{
			this.lines = lines;
		}
		
		public int scrollingIndent = 2;
		
		public int textAreaWidth;
		public int textAreaHeight;
		public int textSizeX;
		public int textSizeY;
		
		public int? oldWWLine;
		public LineIndex? oldFirstLine;
		public bool needScrollToCaret;
		public bool needVScrollFix = false;
		
		public readonly ScrollBarData scrollX = new ScrollBarData();
		public readonly ScrollBarData scrollY = new ScrollBarData();
		
		private void ScrollToCaret(ref int valueX, ref int valueY, IntSize charSize)
		{
			if (!needScrollToCaret)
				return;

			needScrollToCaret = false;
			int indentX = charSize.x * scrollingIndent;
			int indentY = charSize.y * scrollingIndent;
			if (indentX > (textAreaWidth - charSize.x) / 2)
				indentX = ((textAreaWidth - charSize.x) / (2 * charSize.x)) * charSize.x;
			if (indentY > (textAreaHeight - charSize.y) / 2)
				indentY = ((textAreaHeight - charSize.y) / (2 * charSize.y)) * charSize.y;
			
			Pos pos = lines.UniversalPosOf(lines.PlaceOf(lines.LastSelection.caret));
			int x = pos.ix * charSize.x;
			int y = pos.iy * charSize.y;			
			bool changed = false;
			if (valueX > x - indentX)
			{
				valueX = x - indentX;
				changed = true;
			}
			else if (valueX < x - textAreaWidth + indentX)
			{
				valueX = x - textAreaWidth + indentX;
				changed = true;
			}
			if (valueY > y - indentY)
			{
				valueY = y - indentY;
				changed = true;
			}
			else if (valueY < y - textAreaHeight + indentY + charSize.y)
			{
				valueY = y - textAreaHeight + indentY + charSize.y;
				changed = true;
			}
			
			if (changed)
			{
				valueX = CommonHelper.Clamp(valueX, 0, scrollX.contentSize - scrollX.areaSize);
				valueY = CommonHelper.Clamp(valueY, 0, scrollY.contentSize - scrollY.areaSize);
			}
		}
		
		public void UpdateScrollOnPaint(ScrollOnPaintInfo info, ref int valueX, ref int valueY)
		{
			int width = info.width;
			int height = info.height;
			IntSize charSize = info.charSize;
			CheckScrollPage(ref valueX, ref valueY, charSize);
			CheckScroll(ref valueX, ref valueY, charSize);
			if (lines.wordWrap)
			{
				int vScrollBarValue = FixScrollY(valueY, charSize);
				bool needFixByOldLine = vScrollBarValue == FixScrollY(valueY, charSize);
				
				IntSize oldSize = new IntSize(lines.wwSizeX, lines.wwSizeY);
				bool showVScrollBar = scrollY.visible;
				if (showVScrollBar)
				{
					textAreaWidth = width - info.leftIndent - info.scrollBarBreadth;
					lines.wwValidator.Validate(textAreaWidth / charSize.x);
					if (lines.wwSizeY * charSize.y < height)
					{
						textAreaWidth = width - info.leftIndent;
						lines.wwValidator.Validate(textAreaWidth / charSize.x);
						showVScrollBar = false;
					}
				}
				else
				{
					textAreaWidth = width - info.leftIndent;
					lines.wwValidator.Validate(textAreaWidth / charSize.x);
					if (lines.wwSizeY * charSize.y > height)
					{
						textAreaWidth = width - info.leftIndent - info.scrollBarBreadth;
						lines.wwValidator.Validate(textAreaWidth / charSize.x);
						showVScrollBar = true;
					}
				}
				textAreaHeight = height;
				
				scrollX.contentSize = lines.wwSizeX * charSize.x;
				scrollX.areaSize = textAreaWidth;
				scrollX.visible = false;
				
				scrollY.contentSize = lines.wwSizeY * charSize.y + charSize.y / 2;
				scrollY.areaSize = textAreaHeight;
				scrollY.visible = showVScrollBar;
				
				textSizeX = lines.wwSizeX;
				textSizeY = lines.wwSizeY;
				
				if (oldWWLine != null &&
					oldFirstLine != null &&
					!object.Equals(new IntSize(lines.wwSizeX, lines.wwSizeY), oldSize) && needFixByOldLine)
				{
					int wwLine = lines.wwValidator.GetWWILine(oldFirstLine.Value.iLine) + oldFirstLine.Value.iSubline;
					int delta = wwLine - oldWWLine.Value;
					if (delta != 0)
						valueY = CommonHelper.Clamp(vScrollBarValue + delta * charSize.y, 0, scrollY.contentSize - scrollY.areaSize);
				}
			}
			else
			{
				IntSize size = lines.Size;
				int textWidth = size.x * charSize.x;
				int textHeight = size.y * charSize.y;
				bool showVScrollBar = textHeight > height;
				textAreaWidth = showVScrollBar ? width - info.leftIndent - info.scrollBarBreadth : width - info.leftIndent;
				bool showHScrollBar = textWidth > textAreaWidth;
				textAreaHeight = showHScrollBar ? height - info.scrollBarBreadth : height;
				if (!showVScrollBar && showHScrollBar && textHeight > height - info.scrollBarBreadth)
				{
					showVScrollBar = true;
					textAreaHeight = height - info.scrollBarBreadth;
				}
				
				scrollX.contentSize = size.x * charSize.x;
				scrollX.areaSize = textAreaWidth;
				scrollX.visible = showHScrollBar;
				
				scrollY.contentSize = size.y * charSize.y + charSize.y / 2;
				scrollY.areaSize = textAreaHeight;
				scrollY.visible = showVScrollBar;

				textSizeX = size.x;
				textSizeY = size.y;
			}
			if (needVScrollFix)
			{
				needVScrollFix = false;
				valueY = FixScrollY(valueY, charSize);
			}
			if (lines.wordWrap)
			{
				oldWWLine = Math.Max(0, valueY / charSize.y);
				oldFirstLine = lines.wwValidator.GetLineIndexOfWW(oldWWLine.Value);
			}
			
			ScrollToCaret(ref valueX, ref valueY, charSize);
			valueX = scrollX.ClampValue(valueX);
			valueY = scrollY.ClampValue(valueY);
			scrollX.value = valueX;
			scrollY.value = FixScrollY(valueY, charSize);
		}
		
		private int FixScrollY(int value, IntSize charSize)
		{
			return Convert.ToInt32(Math.Round((float)value / charSize.y) * charSize.y);
		}
		
		private bool scrollPage_Need;
		private int scrollPage_Pages;
		private bool scrollPage_WithSelection;
		
		private void CheckScrollPage(ref int valueX, ref int valueY, IntSize charSize)
		{
			if (scrollPage_Need)
			{
				valueY = FixScrollY(valueY, charSize);
				
				int pageSizeY = (textAreaHeight - charSize.y) / charSize.y;
				
				Selection selection = lines.LastSelection;
				Pos pos = lines.UniversalPosOf(lines.PlaceOf(selection.caret));
				pos.ix = selection.wwPreferredPos;
				pos.iy += pageSizeY * scrollPage_Pages;
				selection.caret = lines.IndexOf(lines.Normalize(lines.UniversalPlaceOf(pos)));
				if (!scrollPage_WithSelection)
					selection.anchor = selection.caret;
				
				valueY += pageSizeY * charSize.y * scrollPage_Pages;
				
				scrollPage_Need = false;
				scrollPage_Pages = 0;
				scrollPage_WithSelection = false;
				
				needScrollToCaret = true;
			}
		}
		
		public void ScrollPage(bool isUp, Controller controller, bool withSelection)
		{
			controller.ClearMinorSelections();
			scrollPage_Need = true;
			scrollPage_Pages += isUp ? -1 : 1;
			scrollPage_WithSelection = withSelection;
		}
		
		private bool scroll_Need;
		private bool scroll_ValueNeed;
		private int scroll_X;
		private int scroll_Y;
		private int scroll_ValueX;
		private int scroll_ValueY;
		
		private void CheckScroll(ref int valueX, ref int valueY, IntSize charSize)
		{
			if (scroll_Need)
			{
				valueX += scroll_X * charSize.y;
				valueY = FixScrollY(valueY, charSize) + scroll_Y * charSize.y;
				
				scroll_Need = false;
				scroll_X = 0;
				scroll_Y = 0;
			}
			if (scroll_ValueNeed)
			{
				valueX = scroll_ValueX;
				valueY = FixScrollY(scroll_ValueY, charSize);
				
				scroll_ValueNeed = false;
				scroll_ValueX = 0;
				scroll_ValueY = 0;
			}
		}
		
		public void ScrollRelative(int x, int y)
		{
			scroll_Need = true;
			scroll_X += x;
			scroll_Y += y;
		}

		public void ScrollValue(int valueX, int valueY)
		{
			scroll_ValueNeed = true;
			scroll_ValueX = valueX;
			scroll_ValueY = valueY;
		}
	}
}
