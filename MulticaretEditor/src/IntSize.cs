using System;

namespace MulticaretEditor
{
	public struct IntSize
	{
		public int x;
		public int y;
		
		public IntSize(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
		
		override public string ToString()
		{
			return "(" + x + ", " + y + ")";
		}
	}
}
