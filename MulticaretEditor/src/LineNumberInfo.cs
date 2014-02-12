using System;

namespace MulticaretEditor
{
	public struct LineNumberInfo
	{
		public int iLine;
		public int y;
		
		public LineNumberInfo(int iLine, int y)
		{
			this.iLine = iLine;
			this.y = y;
		}
	}
}
