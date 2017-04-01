using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MulticaretEditor.KeyMapping
{
	public class NamedAction
	{
		private static KeysConverter keysConverter;
		
		public static NamedAction OfKeyItem(KeyItem item)
		{
			string name = "[NONE]";
			string hint = null;
			Getter<Controller, bool> doOnDown = null;
			KeyAction action = item.action;
			if (action != null)
			{
				if (action.name != null)
				{
					name = action.name;
					int index = name.IndexOf('\\');
					if (index != -1)
					{
						name = name.Substring(index + 1);
					}
					name = name.ToUpperInvariant();
				}
				if (item.keys != Keys.None)
				{
					if (keysConverter == null)
					{
						keysConverter = new KeysConverter();
					}
					hint = item.keys == Keys.Alt ? "Alt" : keysConverter.ConvertToString(item.keys);
				}
				doOnDown = action.doOnDown;
			}
			return new NamedAction(name, hint, doOnDown);
		}
		
		public readonly string name;
		public readonly string hint;
		public readonly Getter<Controller, bool> doOnDown;

		public NamedAction(string name, string hint, Getter<Controller, bool> doOnDown)
		{
			this.name = name;
			this.hint = hint;
			this.doOnDown = doOnDown;
		}
		
		public bool Execute(Controller controller)
		{
			if (doOnDown != null)
				return doOnDown(controller);
			return false;
		}
		
		public override string ToString()
		{
			return name;
		}
		
		public static string HintOf(NamedAction action)
		{
			return action.hint;
		}
	}
}
