using System.Collections.Generic;

namespace MulticaretEditor
{
	public interface IViStoreSelector
	{
		void ViStoreSelections();
		
		void ViStoreMementos(SelectionMemento[] mementos);
	}
}