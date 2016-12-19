using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class AltReceiver : AReceiver
	{
		public override bool AltMode { get { return true; } }
		
		public override void DoKeyPress(char code)
		{
			switch (code)
			{
				case 'i':
					receiver.SetState(new InputReceiver());
					break;
			}
		}
		
		public override bool DoKeyDown(Keys keysData)
		{
			return false;
		}
	}
}