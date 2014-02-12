using System;

namespace MulticaretEditor
{
	public struct LineIndex
	{
		public LineIndex(int iLine, int iSubline)
		{
			this.iLine = iLine;
			this.iSubline = iSubline;
		}
		
		public int iLine;
		public int iSubline;
		
		override public string ToString()
		{
			return "(" + iLine + "/" + iSubline + ")";
		}
	}
}
