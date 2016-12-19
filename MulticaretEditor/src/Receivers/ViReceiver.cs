using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViReceiver : AReceiver
	{
		public override bool AltMode { get { return true; } }
		
		public override void DoOn()
		{
			for (int i = 0; i < lines.selections.Count; i++)
			{
				Selection selection = lines.selections[i];
				if (selection.Empty)
				{
					Place place = lines.PlaceOf(selection.caret);
					Line line = lines[place.iLine];
					int count = line.NormalCount;
					if (place.iChar >= count)
					{
						selection.anchor -= place.iChar - count + 1;
						selection.caret -= place.iChar - count + 1;
					}
				}
			}
		}
		
		public override void DoKeyPress(char code)
		{
			switch (code)
			{
				case 'i':
					context.SetState(new InputReceiver());
					break;
				case 'h':
					controller.MoveLeft(false);
					break;
				case 'l':
					controller.MoveRight(false);
					break;
				case 'j':
					controller.MoveDown(false);
					break;
				case 'k':
					controller.MoveUp(false);
					break;
			}
		}
		
		public override bool DoKeyDown(Keys keysData)
		{
			switch (keysData)
			{
				case Keys.Left:
					controller.MoveLeft(false);
					return true;
				case Keys.Right:
					controller.MoveRight(false);
					return true;
				case Keys.Down:
					controller.MoveDown(false);
					return true;
				case Keys.Up:
					controller.MoveUp(false);
					return true;
			}
			return false;
		}
	}
}