using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class InputReceiver : AReceiver
	{
		private ViReceiverData viData;
		
		public InputReceiver(ViReceiverData viData)
		{
			this.viData = viData;
		}
		
		public override void DoKeyPress(char code, out string viShortcut)
		{
			viShortcut = null;
			ProcessInputChar(code);
			if (viData != null)
			{
				viData.inputChars.Add(code);
			}
		}
		
		public override bool DoKeyDown(Keys keysData)
		{
			if (((keysData & Keys.Control) == Keys.Control) &&
				((keysData & Keys.OemOpenBrackets) == Keys.OemOpenBrackets))
			{
				ViReceiverData viData = this.viData;
				this.viData = null;
				context.SetState(new ViReceiver(viData));
				return true;
			}
			return false;
		}
		
		public override void ResetViInput()
		{
			viData = null;
		}
	}
}