using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViReceiverData
	{
		public readonly int count;
		public readonly char action;
		public readonly List<char> inputChars = new List<char>();
		
		public ViReceiverData(char action, int count)
		{
			this.action = action;
			this.count = count;
		}
		
	}
}