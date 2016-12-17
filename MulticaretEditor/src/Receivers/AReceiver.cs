using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class AReceiver
	{
		protected Controller controller;
		protected LineArray lines;
		protected Receiver receiver;
		
		public void Init(Controller controller, Receiver receiver)
		{
			this.controller = controller;
			this.lines = controller.Lines;
			this.receiver = receiver;
		}
		
		public virtual void DoKeyPress(char code)
		{
		}
		
		public virtual bool DoKeyDown(Keys keysData)
		{
			return false;
		}
	}
}