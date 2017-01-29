using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public struct ViChar
	{
		public char c;
		public bool control;
		
		public ViChar(char c, bool control)
		{
			this.c = c;
			this.control = control;
		}
		
		override public string ToString()
		{
			if (c == '\0')
				return control ? "<Ctrl-\\0>" : "\\0";
			return control ? "<Ctrl-" + c + ">" : c + "";
		}
	}
}
