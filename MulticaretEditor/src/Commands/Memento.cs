using System;

namespace MulticaretEditor.Commands
{
	public struct Memento
	{
		public string text;
		public int count;
		
		public Memento(string text, int count)
		{
			this.text = text;
			this.count = count;
		}
	}
}
