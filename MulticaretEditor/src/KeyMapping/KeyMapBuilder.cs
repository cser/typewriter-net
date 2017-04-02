using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MulticaretEditor
{
	public class KeyMapBuilder
	{
		public readonly KeyMap map;
		public readonly SwitchList<NamedAction> list;
		
		public KeyMapBuilder(KeyMap map, SwitchList<NamedAction> list)
		{
			this.map = map;
			this.list = list;
		}
		
		public void Add(Keys keys, Keys? modeKeys, KeyAction action)
		{
			KeyItem item = new KeyItem(keys, modeKeys, action);
			map.AddItem(item);
		}
		
		public void AddInList(Keys keys, Keys? modeKeys, KeyAction action)
		{
			KeyItem item = new KeyItem(keys, modeKeys, action);
			map.AddItem(item);
			list.Add(NamedAction.OfKeyItem(item));
		}
	}
}
