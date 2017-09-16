using System;
using System.Text;

namespace MulticaretEditor
{
	public class SavePositions : Command
	{
		public SavePositions() : base(CommandType.ViSavePositions)
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
			lines.viStoreSelector.ViStoreMementos(mementos);
		}
	}
}
