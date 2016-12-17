using System;
using System.Drawing;

namespace MulticaretEditor
{
	public class StyleData
	{		
		public StyleData()
		{
		}

		public short index;
		public Ds ds;
		public string name;
		public Color? color;
		public bool? italic;
		public bool? bold;
		public bool? underline;
		public bool? strikeout;
	}
}
