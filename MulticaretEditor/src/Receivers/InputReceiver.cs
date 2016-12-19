using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class InputReceiver : AReceiver
	{
		public override void DoKeyPress(Controller controller, char code)
		{
			switch (code)
			{
				case '\b':
					if (controller.Lines.AllSelectionsEmpty)
					{
						controller.Backspace();
					}
					else
					{
						controller.EraseSelection();
					}
					break;
				case '\r':
					controller.InsertLineBreak();
					break;
				default:
					controller.InsertText(code + "");
					break;
			}
		}
		
		public override bool DoKeyDown(Controller controller, Keys keysData)
		{
			if (((keysData & Keys.Control) == Keys.Control) &&
				((keysData & Keys.OemOpenBrackets) == Keys.OemOpenBrackets))
			{
				receiver.SetState(new AltReceiver());
				return true;
			}
			return false;
		}
	}
}