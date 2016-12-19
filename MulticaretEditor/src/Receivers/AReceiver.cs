using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class AReceiver
	{
		public virtual bool AltMode { get { return false; } }
		
		protected Controller controller;
		protected LineArray lines;
		protected Receiver.Context context;
		
		public void Init(Controller controller, Receiver.Context context)
		{
			this.controller = controller;
			this.lines = controller.Lines;
			this.context = context;
		}
		
		public virtual void DoOn()
		{
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