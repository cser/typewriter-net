namespace CustomScrollBar
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	
	/// <summary>
	/// The scrollbar renderer class.
	/// </summary>
	internal static class ScrollBarExRenderer
	{
		/// <summary>
		/// Draws the background.
		/// </summary>
		/// <param name="g">The <see cref="Graphics"/> used to paint.</param>
		/// <param name="rect">The rectangle in which to paint.</param>
		/// <param name="orientation">The <see cref="ScrollBarOrientation"/>.</param>
		public static void DrawBackground(Graphics g, Rectangle rect, bool isHorizontal)
		{
			if (g == null)
			{
				throw new ArgumentNullException("g");
			}
		
			if (rect.IsEmpty || g.IsVisibleClipEmpty
			|| !g.VisibleClipBounds.IntersectsWith(rect))
			{
				return;
			}
			
			using (SolidBrush brush = new SolidBrush(Color.Gray))
			{
				g.FillRectangle(brush, rect);
			}
		}
		
		/// <summary>
		/// Draws the channel ( or track ).
		/// </summary>
		/// <param name="g">The <see cref="Graphics"/> used to paint.</param>
		/// <param name="rect">The rectangle in which to paint.</param>
		/// <param name="state">The scrollbar state.</param>
		/// <param name="orientation">The <see cref="ScrollBarOrientation"/>.</param>
		public static void DrawTrack(
			Graphics g,
			Rectangle rect,
			ScrollBarState state,
			bool isHorizontal)
		{
			if (g == null)
			{
				throw new ArgumentNullException("g");
			}
			
			if (rect.Width <= 0 || rect.Height <= 0
			|| state != ScrollBarState.Pressed || g.IsVisibleClipEmpty
			|| !g.VisibleClipBounds.IntersectsWith(rect))
			{
				return;
			}
			
			using (SolidBrush brush = new SolidBrush(Color.Gray))
			{
				g.FillRectangle(brush, rect);
			}
		}
		
		/// <summary>
		/// Draws the thumb.
		/// </summary>
		/// <param name="g">The <see cref="Graphics"/> used to paint.</param>
		/// <param name="rect">The rectangle in which to paint.</param>
		/// <param name="state">The <see cref="ScrollBarState"/> of the thumb.</param>
		/// <param name="orientation">The <see cref="ScrollBarOrientation"/>.</param>
		public static void DrawThumb(
			Graphics g,
			Rectangle rect,
			ScrollBarState state,
			bool isHorizontal)
		{
			if (g == null)
			{
				throw new ArgumentNullException("g");
			}
		
			if (rect.IsEmpty || g.IsVisibleClipEmpty
			|| !g.VisibleClipBounds.IntersectsWith(rect)
			|| state == ScrollBarState.Disabled)
			{
				return;
			}
			
			using (SolidBrush brush = new SolidBrush(Color.Silver))
			{
				g.FillRectangle(brush, rect);
			}
		}
		
		/// <summary>
		/// Draws an arrow button.
		/// </summary>
		/// <param name="g">The <see cref="Graphics"/> used to paint.</param>
		/// <param name="rect">The rectangle in which to paint.</param>
		/// <param name="state">The <see cref="ScrollBarArrowButtonState"/> of the arrow button.</param>
		/// <param name="arrowUp">true for an up arrow, false otherwise.</param>
		/// <param name="orientation">The <see cref="ScrollBarOrientation"/>.</param>
		public static void DrawArrowButton(
			Graphics g,
			Rectangle rect,
			ScrollBarArrowButtonState state,
			bool arrowUp,
			bool isHorizontal)
		{
			if (g == null)
			{
				throw new ArgumentNullException("g");
			}
			
			if (rect.IsEmpty || g.IsVisibleClipEmpty
			|| !g.VisibleClipBounds.IntersectsWith(rect))
			{
				return;
			}
			
			if (isHorizontal)
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
			using (Pen pen = new Pen(Color.Black))
			{
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
		}

		private static int a0;
		private static int a1;
		private static int a2;
		private static int a3;
		private static int lineX;
		private static int lineY;
		
		private static void DrawLine(Graphics g, Pen pen, int x0, int y0, int x1, int y1)
		{
			int xx0 = x0 * a0 + y0 * a1;
			int yy0 = x0 * a2 + y0 * a3;
			int xx1 = x1 * a0 + y1 * a1;
			int yy1 = x1 * a2 + y1 * a3;
			g.DrawLine(pen, lineX + xx0, lineY + yy0, lineX + xx1, lineY + yy1);
		}
	}
}
