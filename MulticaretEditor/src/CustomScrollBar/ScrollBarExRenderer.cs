namespace CustomScrollBar
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using MulticaretEditor;
	
	internal static class ScrollBarExRenderer
	{
		public static void DrawBackground(Graphics g, Scheme scheme, Rectangle rect, bool isHorizontal)
		{
			if (rect.IsEmpty || g.IsVisibleClipEmpty || !g.VisibleClipBounds.IntersectsWith(rect))
			{
				return;
			}
			g.FillRectangle(scheme.scrollBgBrush, rect);
		}
		
		public static void DrawThumb(Graphics g, Scheme scheme, Rectangle rect, ScrollBarState state, bool isHorizontal)
		{
			if (rect.IsEmpty || g.IsVisibleClipEmpty || !g.VisibleClipBounds.IntersectsWith(rect) ||
				state == ScrollBarState.Disabled)
			{
				return;
			}			
			if (state == ScrollBarState.Hot || state == ScrollBarState.Pressed)
			{
				g.FillRectangle(scheme.scrollThumbHoverBrush, rect);
			}
			else
			{
				g.FillRectangle(scheme.scrollThumbBrush, rect);
			}
		}
		
		public static void DrawArrowButton(Graphics g, Scheme scheme, Rectangle rect, ScrollBarState state, bool arrowUp, bool isVertical)
		{
			if (rect.IsEmpty || g.IsVisibleClipEmpty || !g.VisibleClipBounds.IntersectsWith(rect))
			{
				return;
			}
			offset = state == ScrollBarState.Pressed ? 1 : 0;
			if (isVertical)
			{
				if (arrowUp)
				{
					a0 = 1;
					a1 = 0;
					a2 = 0;
					a3 = 1;
				}
				else
				{
					a0 = -1;
					a1 = 0;
					a2 = 0;
					a3 = -1;
				}
			}
			else
			{
				if (arrowUp)
				{
					a0 = 0;
					a1 = 1;
					a2 = 1;
					a3 = 0;
				}
				else
				{
					a0 = 0;
					a1 = -1;
					a2 = -1;
					a3 = 0;
				}
			}
			Pen pen = state == ScrollBarState.Hot || state == ScrollBarState.Pressed ?
				scheme.scrollArrowHoverPen : scheme.scrollArrowPen;
			lineX = rect.X + rect.Width / 2;
			lineY = rect.Y + rect.Height / 2;
			int size = 16;
			int td = size / 4;
			int td2 = td * 4 / 6;
			DrawLine(g, pen, -td, td2, 0, -td2);
			DrawLine(g, pen, 0, -td2, td, td2);
			DrawLine(g, pen, -td + 1, td2, 0, -td2 + 1);
			DrawLine(g, pen, 0, -td2 + 1, td - 1, td2);
			DrawLine(g, pen, -td + 1, td2 + 1, 0, -td2 + 2);
			DrawLine(g, pen, 0, -td2 + 2, td - 1, td2 + 1);
		}

		private static int offset;
		private static int a0;
		private static int a1;
		private static int a2;
		private static int a3;
		private static int lineX;
		private static int lineY;
		
		private static void DrawLine(Graphics g, Pen pen, int x0, int y0, int x1, int y1)
		{
			int offsetY = -1;
			y0 += offsetY;
			y1 += offsetY;
			int xx0 = x0 * a0 + y0 * a1;
			int yy0 = x0 * a2 + y0 * a3;
			int xx1 = x1 * a0 + y1 * a1;
			int yy1 = x1 * a2 + y1 * a3;
			g.DrawLine(pen, lineX + xx0 + offset, lineY + yy0 + offset, lineX + xx1 + offset, lineY + yy1 + offset);
		}
	}
}
