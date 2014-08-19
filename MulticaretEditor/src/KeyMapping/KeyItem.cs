using System;
using System.Windows.Forms;

namespace MulticaretEditor.KeyMapping
{
	public class KeyItem
	{
		public readonly Keys keys;
		public readonly Keys? modeKeys;
		public readonly KeyAction action;
		
		public KeyItem next;
		public bool doubleClick;
		
		public KeyItem(Keys keys, Keys? modeKeys, KeyAction action)
		{
			this.keys = keys;
			this.modeKeys = modeKeys;
			this.action = action;
			
			next = null;
		}

		public KeyItem SetDoubleClick(bool value)
		{
			doubleClick = value;
			return this;
		}
	}
}
