using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class InputReceiver : AReceiver
	{
		public override ViMode ViMode { get { return ViMode.Insert; } }
		
		private ViReceiverData viData;
		private readonly bool alwaysInputMode;
		
		public InputReceiver(ViReceiverData viData, bool alwaysInputMode)
		{
			this.viData = viData;
			this.alwaysInputMode = alwaysInputMode;
		}
		
		public override bool DoKeyDown(Keys keysData, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			scrollToCursor = true;
			if (!alwaysInputMode &&
				((keysData & Keys.Control) == Keys.Control) &&
				((keysData & Keys.OemOpenBrackets) == Keys.OemOpenBrackets))
			{
				ViReceiverData viData = this.viData;
				this.viData = null;
				context.SetState(new ViReceiver(viData, true, false));
				return true;
			}
			return false;
		}
		
		public override void DoKeyPress(char code, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			ProcessInputChar(code);
			scrollToCursor = true;
			if (viData != null)
			{
				viData.inputChars.Add(code);
			}
		}
		
		public override void ResetViInput()
		{
			viData = null;
		}
	}
}