using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MulticaretEditor.KeyMapping
{
	public class NamedAction
	{
		public readonly string name;
		public readonly Getter<Controller, bool> doOnDown;

		public NamedAction(string name, Getter<Controller, bool> doOnDown)
		{
			this.name = name;
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
	}
}
