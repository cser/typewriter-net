using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace MulticaretEditor
{
	public class MatchesRenderer
	{
		public MatchesRenderer()
		{
		}
		
		public Graphics g;
		public Brush brush;
		public int offsetX;
		public int offsetY;
		public int charWidth;
		public int charHeight;
		public int lineInterval;
		public bool start;
		
		public void AddLine(int ix, int iy, int sizeX)
		{
			g.FillRectangle(
				brush,
				offsetX + ix * charWidth,
				offsetY + iy * charHeight + lineInterval / 2 + (start ? 1 : 0),
				sizeX * charWidth - 1,
				charHeight + (start ? -1 : 0));
			start = false;
		}
	}
}