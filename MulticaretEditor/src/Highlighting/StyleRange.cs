using System;
using System.Drawing;

namespace MulticaretEditor.Highlighting
{
	public class StyleRange
	{		
		public readonly short style;
		public readonly int start;
		public readonly int count;

		public StyleRange(int start, int count, short style)
		{
			this.start = start;
			this.count = count;
			this.style = style;
		}
	}
}
