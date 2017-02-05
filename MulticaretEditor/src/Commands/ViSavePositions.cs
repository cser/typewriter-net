using System;
using System.Text;

namespace MulticaretEditor
{
	public class ViSavePositions : Command
	{
		public ViSavePositions() : base(CommandType.Copy)
		{
		}
		
		private SelectionMemento[] mementos;
		
		override public bool Init()
		{
			mementos = GetSelectionMementos();
			return true;
		}
		
		override public void Redo()
		{
			SetSelectionMementos(mementos);
		}
		
		override public void Undo()
		{
			SetSelectionMementos(mementos);
		}
	}
}
