using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViReceiverData : AReceiver
	{
		public readonly int count;
		public readonly List<char> inputChars = new List<char>();
		
		public ViReceiverData(int count)
		{
			this.count = count;
		}
		
	}
}