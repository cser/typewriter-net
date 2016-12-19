using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class AReceiver
	{
		public virtual bool AltMode { get { return false; } }
		
		protected Receiver receiver;
		
		public void Init(Receiver receiver)
		{
			this.receiver = receiver;
		}
		
		public virtual void DoKeyPress(Controller controller, char code)
		{
		}
		
		public virtual bool DoKeyDown(Controller controller, Keys keysData)
		{
			return false;
		}
	}
}