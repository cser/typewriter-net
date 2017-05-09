using System;

namespace MulticaretEditor
{
	public struct SimpleRange
	{
		public int index;
		public int count;
		
		public SimpleRange(int index, int count)
		{
			this.index = index;
			this.count = count;
		}
		
		public static int CompareLeftToRight(SimpleRange a, SimpleRange b)
		{
			return a.index == b.index ? a.count - b.count : a.index - b.index;
		}
		
		public override string ToString()
		{
			return "(" + index + ", " + count + ")";
		}
	}
}
