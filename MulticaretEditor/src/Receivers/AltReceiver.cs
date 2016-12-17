using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class AltReceiver : AReceiver
	{
		public override void DoKeyPress(char code)
		{
		}
		
		public override bool DoKeyDown(Keys keysData)
		{
			return false;
		}
	}
}