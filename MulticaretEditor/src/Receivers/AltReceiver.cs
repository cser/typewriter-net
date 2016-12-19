using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class AltReceiver : AReceiver
	{
		public override bool AltMode { get { return true; } }
		
		public override void DoKeyPress(Controller controller, char code)
		{
			switch (code)
			{
				case 'i':
					receiver.SetState(new InputReceiver());
					break;
			}
		}
		
		public override bool DoKeyDown(Controller controller, Keys keysData)
		{
			return false;
		}
	}
}