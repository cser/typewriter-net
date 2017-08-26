using System;

namespace MulticaretEditor
{
	public struct TabSettings
	{
		public bool useSpaces;
		public int size;
		
		public TabSettings(bool useSpaces, int size)
		{
			this.useSpaces = useSpaces;
			this.size = size;
		}
		
		public string Tab
		{
			get
			{
				if (useSpaces)
				{
					return size < 8 ? _tabs[size - 1] : new string(' ', size);
				}
				return "\t";
			}
		}
		
		public string ShiftedSpacesOfSize(int spacesSize, int shift)
		{
			int count = Math.Max(0, spacesSize / size + shift);
			return useSpaces ? new string(' ', size * count) : new string('\t', count);
		}
		
		private static string[] _tabs = new string[]
		{
			" ",
			"  ",
			"   ",
			"    ",
			"     ",
			"      ",
			"       ",
			"        "
		};
	}
}
