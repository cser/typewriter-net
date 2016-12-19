using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class Receiver
	{
		private readonly Controller controller;
		private readonly LineArray lines;
		
		private AReceiver state;
		
		public bool altMode;
		
		public Receiver(Controller controller)
		{
			this.controller = controller;
			this.lines = controller.Lines;
			
			SetState(new InputReceiver());
		}
		
		public void SetState(AReceiver state)
		{
			if (this.state != state)
			{
				this.state = state;
				this.state.Init(controller, this);
				altMode = this.state.AltMode;
			}
		}
		
		public void DoKeyPress(char code)
		{
			state.DoKeyPress(code);
		}
		
		public bool DoKeyDown(Keys keysData)
		{
			return state.DoKeyDown(keysData);
		}
	}
}
