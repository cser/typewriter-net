using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class ViReceiverData
	{
		public readonly int count;
		public readonly char action;
		public readonly List<char> inputChars = new List<char>();
		public bool forcedInput;
		
		public ViReceiverData(char action, int count)
		{
			this.action = action;
			this.count = count;
		}
		
	}
}