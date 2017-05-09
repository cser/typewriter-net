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
