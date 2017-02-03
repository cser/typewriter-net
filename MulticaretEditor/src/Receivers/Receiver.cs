using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class Receiver
	{
		private readonly Controller controller;
		private readonly LineArray lines;
		
		private Context context;
		private AReceiver state;
		
		public Dictionary<char, char> viMap;
		
		public bool viMode;
		
		public Receiver(Controller controller, bool viMode)
		{
			this.controller = controller;
			this.lines = controller.Lines;
			
			context = new Context(this);
			ProcessSetViMode(viMode);
		}
		
		public void SetViMode(bool value)
		{
			if (viMode != value)
			{
				ProcessSetViMode(value);
			}
		}
		
		private void ProcessSetViMode(bool value)
		{
			if (value)
			{
				context.SetState(new ViReceiver());
			}
			else
			{
				context.SetState(new InputReceiver());
			}
		}
		
		public class Context
		{
			private Receiver receiver;
			
			public Context(Receiver receiver)
			{
				this.receiver = receiver;
			}
			
			public void SetState(AReceiver state)
			{
				if (receiver.state != state)
				{
					receiver.state = state;
					receiver.state.Init(receiver.controller, this);
					receiver.state.DoOn();
					receiver.viMode = receiver.state.AltMode;
				}
			}
			
			public char GetMapped(char c)
			{
				if (receiver.viMap != null)
				{
					char result;
					if (receiver.viMap.TryGetValue(c, out result))
						return result;
				}
				return c;
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
