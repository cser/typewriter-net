using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

namespace TypewriterNET.Frames
{
	public class SearchFrame
	{
		private SearchableFrame parent;
		private SearchPanel panel;

		public MulticaretTextBox TextBox { get { return parent.TextBox; } }
		public string searchOldText = "";

		public void AddTo(SearchableFrame parent)
		{
			this.parent = parent;
			panel = new SearchPanel(this);
			parent.AddSearchPanel(panel);
			panel.Focus();

	        parent.TextBox.KeyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Close search", DoCloseSearch, null, false)));
		}

	    public bool DoCloseSearch(Controller controller)
	    {
	    	if (panel != null)
	    	{
	    		panel.DoOnClose();
	    		parent.RemoveSearchPanel(panel);
	    		panel.Dispose();
	    		panel = null;
	    		parent.TextBox.Focus();
	    		return true;
	    	}
	    	return false;
	    }
	}
}
