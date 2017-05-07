using System;

namespace MulticaretEditor.Commands
{
	public class InsertIndentedCket : Command
	{
		public InsertIndentedCket() : base(CommandType.InsertIndentedCket)
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
			foreach (Selection selection in selections)
			{
				Place place = lines.PlaceOf(selection.caret);
				if (place.iLine == lastILine)
				{
					continue;
				}
				lastILine = place.iLine;
				if (place.iLine > 0)
				{
					Line line = lines[place.iLine];
					if (line.NormalCount > 0 && line.IsOnlySpaces())
					{
						Line prevLine = lines[place.iLine - 1];
						int iChar;
						int prevSpacesSize = prevLine.GetFirstSpaceSize(out iChar);
						if (prevSpacesSize > 0 && prevSpacesSize % lines.tabSize == 0)
						{
							int spacesSize = line.GetFirstSpaceSize(out iChar);
							char lastPrevNotSpace = prevLine.GetLastNotSpace();
							if (lastPrevNotSpace != '{' && prevSpacesSize == spacesSize)
							{
								int newPos = spacesSize - lines.tabSize;
								int newIChar = line.IndexOfPos(newPos);
								int newPos2 = line.PosOfIndex(newIChar);
								if (newPos2 == newPos)
								{
									selection.anchor += newIChar - place.iChar;
								}
							}
							else if (lastPrevNotSpace == '{' && prevSpacesSize == spacesSize - lines.tabSize)
							{
								int newPos = prevSpacesSize;
								int newIChar = line.IndexOfPos(newPos);
								int newPos2 = line.PosOfIndex(newIChar);
								if (newPos2 == newPos)
								{
									selection.anchor += newIChar - place.iChar;
								}
							}
						}
					}
				}					
			}
			insertText = new InsertTextCommand("}", null, true);
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
