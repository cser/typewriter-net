using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public struct REToken
	{
		public readonly char type;
		public readonly char c;
		
		public REToken(char type, char c)
		{
			this.type = type;
			this.c = c;
		}
		
		public override string ToString()
		{
			return (type == '\0' ? "\\0" : "" + type) + (c == '\0' ? "\\0" : "" + c);
		}
	}
}