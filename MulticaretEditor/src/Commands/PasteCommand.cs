using System;

namespace MulticaretEditor.Commands
{
	public class PasteCommand : Command
	{
		public PasteCommand() : base(CommandType.Paste)
		{
		}
		
		private string[] pasted;
		private string[] deleted;
		private SelectionMemento[] mementos;
		
		override public bool Init()
		{
			lines.JoinSelections();
			string text = ClipboardExecuter.GetFromClipboard();
			if (string.IsNullOrEmpty(text))
				return false;
			
			LineSubdivider subdivider = new LineSubdivider(text);
			if (subdivider.GetLinesCount() != selections.Count)
			{
				pasted = new string[selections.Count];
				for (int i = 0; i < pasted.Length; i++)
				{
					pasted[i] = text;
				}
			}
			else
			{
				pasted = subdivider.GetLines();
				for (int i = 0; i < pasted.Length; i++)
				{
					pasted[i] = LineSubdivider.GetWithoutEndRN(pasted[i]);
				}
			}
			mementos = GetSelectionMementos();
			return true;
		}
		
		override public void Redo()
		{
			System.Console.WriteLine("Redo() {");
			System.Console.WriteLine("REDO:" + lines.CheckConsistency());
			deleted = new string[mementos.Length];
			int offset = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				SelectionMemento memento = mementos[i];
				memento.anchor += offset;
				memento.caret += offset;
				string deletedI = lines.GetText(memento.Left, memento.Count);
				string pastedI = pasted[i];
				System.Console.WriteLine("REDO[" + i + "]#1:" + lines.CheckConsistency());
				lines.RemoveText(memento.Left, memento.Count);
				System.Console.WriteLine("REDO[" + i + "]#2:" + lines.CheckConsistency());
				lines.InsertText(memento.Left, pastedI);
				System.Console.WriteLine("REDO[" + i + "]#3:" + lines.GetDebugText());
				System.Console.WriteLine("REDO[" + i + "]#4:" + lines.CheckConsistency());
				memento.caret += pastedI.Length;
				memento.anchor += pastedI.Length;
				offset += -memento.Count + pastedI.Length;
				mementos[i] = memento;
				deleted[i] = deletedI;
			}
			System.Console.WriteLine("REDO#1");
			SetSelectionMementos(mementos);
			foreach (Selection selection in selections)
			{
				selection.anchor = selection.Left;
				selection.caret = selection.anchor;
				Place place = lines.PlaceOf(selection.caret);
				lines.SetPreferredPos(selection, place);
			}
			System.Console.WriteLine("REDO#2");
			lines.JoinSelections();
			System.Console.WriteLine("}");
		}
		
		override public void Undo()
		{
			int offset = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				SelectionMemento memento = mementos[i];
				memento.anchor += offset;
				memento.caret += offset;
				string deletedI = deleted[i];
				string pastedI = pasted[i];
				memento.anchor -= pastedI.Length;
				memento.caret -= pastedI.Length;
				lines.RemoveText(memento.Left, pastedI.Length);
				lines.InsertText(memento.Left, deletedI);
				offset += deletedI.Length - pastedI.Length;
				Place place = lines.PlaceOf(memento.caret);
				memento.preferredPos = lines[place.iLine].PosOfIndex(place.iChar);
				mementos[i] = memento;
			}
			deleted = null;
			SetSelectionMementos(mementos);
		}
	}
}
