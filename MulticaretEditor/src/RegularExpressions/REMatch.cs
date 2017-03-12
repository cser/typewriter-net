using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public struct REMatch
	{
		public readonly int index;
		public readonly int length;
		
		public REMatch(int index, int length)
		{
			this.index = index;
			this.length = length;
		}
		
		public override string ToString()
		{
			return "(index=" + index + " length=" + length + ")";
		}
	}
}