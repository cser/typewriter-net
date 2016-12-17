using System;

namespace MulticaretEditor
{
	public struct Range
	{
		public int iLine0;
		public int iLine1;
		public int start;
		
		public Range(int iLine0, int iLine1, int start)
		{
			this.iLine0 = iLine0;
			this.iLine1 = iLine1;
			this.start = start;
		}
		
		override public string ToString()
		{
			return start + ":(" + iLine0 + ", " + iLine1 + ")";
		}
	}
}
