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
					if (place.iChar > 0)
					{
						selection.anchor--;
						selection.caret--;
					}
				}
			}
		}
		
		public override void DoKeyPress(char code)
		{
			code = context.GetMapped(code);
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
	
	/*
	NORMAL
	COMMAND
	VISUAL
	LINE_VISUAL
	
	10w 
	10{move{WORD}}
	10W
	10{move{LONG_WORD}}
	10.
	10{replay}
	10i
	switch{INPUT}, wait_switch_out, (10-1){replay}
	i
	switch{INPUT}, wait_switch_out
	a
	move{RIGHT}, switch{INPUT}, wait_switch_out
	10dw
	10{delete{move{WORD}}}
	10diw
	10{delete{object{WORD}}}
	10v
	10{select{move{RIGHT}}}
	v
	switch{VISUAL}
	
	COMMAND
	10w
	Do(10, Move(WORD, false))
	10dw
	Do(10, And(Move(WORD, true), Delete()))
	10diw
	Do(10, And(Select(WORD), Delete()))
	10W
	Do(10, Move(BIG_WORD, false))
	10fa
	Do(10, Move(Find('a'), false))
	%
	Move(BRACKET, false)
	di%
	And(Select(BRACKET_INSIDE, true), Delete())
	da%
	And(Select(BRACKET_OUTSIDE, true), Delete())
	*/
}