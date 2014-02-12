using System;

namespace MulticaretEditor
{
	public class CutOff
	{
		public readonly int iChar;
		public readonly int left;
		public readonly int sizeX;
		
		public CutOff(int iChar, int left, int sizeX)
		{
			this.iChar = iChar;
			this.left = left;
			this.sizeX = sizeX;
		}
		
		override public string ToString()
		{
			return "(" + iChar + "/" + left + ")";
		}
	}
}
