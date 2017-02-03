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
		
		public bool IsChar(char c)
		{
			return this.c == c && !control;
		}
		
		override public string ToString()
		{
			if (c == '\0')
				return control ? "<C-\\0>" : "\\0";
			return control ? "<C-" + c + ">" : c + "";
		}
	}
}
