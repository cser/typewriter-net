using System;

namespace MulticaretEditor.Highlighting
{
	public struct Syntax
	{
		public string name;
		public string filename;
		
		public Syntax(string name, string filename)
		{
			this.name = name;
			this.filename = filename;
		}
	}
}
