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
	}
}