using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public struct ViChar
	{
		public const int ControlIndex = 0x10000;
		
		public char c;
		public bool control;
		
		public ViChar(char c, bool control)
		{
			this.c = c;
			this.control = control;
		}
		
		public int Index { get { return (int)c + (control ? ControlIndex : 0); } }
		
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
