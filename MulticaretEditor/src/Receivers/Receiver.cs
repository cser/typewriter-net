using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class Receiver
	{
		private AReceiver state;
		
		public bool altMode;
		
		public Receiver()
		{
			SetState(new InputReceiver());
		}
		
		public void SetState(AReceiver state)
		{
			if (this.state != state)
			{
				this.state = state;
				this.state.Init(this);
				altMode = this.state.AltMode;
			}
		}
		
		public void DoKeyPress(Controller controller, char code)
		{
			state.DoKeyPress(controller, code);
		}
		
		public bool DoKeyDown(Controller controller, Keys keysData)
		{
			return state.DoKeyDown(controller, keysData);
		}
	}
}
