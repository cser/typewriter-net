using System;

namespace MulticaretEditor
{
	public class InsertIndentedBeforeCommand : Command
	{
		public InsertIndentedBeforeCommand() : base(CommandType.InsertIndentedBefore)
		{
		}

		private EraseSelectionCommand eraseCommand;
		private InsertTextCommand insertText;

		override public bool Init()
		{
			eraseCommand = new EraseSelectionCommand();
			eraseCommand.lines = lines;
			eraseCommand.selections = selections;
			eraseCommand.Init();
			return true;
		}

		override public void Redo()
		{
			eraseCommand.Redo();
			int lastILine = -1;
			string[] texts = new string[selections.Count];
			for (int i = 0; i < selections.Count; ++i)
			{
				Selection selection = selections[i];
				texts[i] = "";
				Place place = lines.PlaceOf(selection.caret);
				if (place.iLine == lastILine)
				{
					continue;
				}
				lastILine = place.iLine;
				if (place.iLine + 1 < lines.LinesCount)
				{
					Line line = lines[place.iLine];
					if (line.NormalCount > 0 && line.IsOnlySpaces())
					{
						Line nextLine = lines[place.iLine + 1];
						int iChar;
						int nextSpacesSize = nextLine.GetFirstSpaceSize(out iChar);
						if (nextSpacesSize > 0 && nextSpacesSize % lines.tabSize == 0)
						{
							int spacesSize = line.GetFirstSpaceSize(out iChar);
							char nextPrevNotSpace = nextLine.GetLastNotSpace();
							if (nextPrevNotSpace == '}' && nextSpacesSize == spacesSize)
							{
								texts[i] = lines.spacesInsteadTabs ? new string(' ', lines.tabSize) : "\t";
							}
						}
					}
				}					
			}
			insertText = new InsertTextCommand(null, texts, true);
			insertText.lines = lines;
			insertText.selections = selections;
			insertText.Init();
			insertText.Redo();
		}

		override public void Undo()
		{
			insertText.Undo();
			insertText = null;
			eraseCommand.Undo();
		}
	}
}
