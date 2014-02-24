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

		private bool opened;
		public bool Opened { get { return opened; } }

		public MulticaretTextBox TextBox { get { return parent.TextBox; } }
		public string searchOldText = "";
		
		private KeyMap keyMap;

		public SearchFrame()
		{
			keyMap = new KeyMap();
	        keyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Close search", DoCloseSearch, null, false)));
		}

		public void AddTo(SearchableFrame parent)
		{
			if (opened)
				Remove();
			opened = true;

			this.parent = parent;
			panel = new SearchPanel(this);
			parent.AddSearchPanel(panel);
			parent.TextBox.KeyMap.AddAfter(keyMap);
			panel.Focus();
		}

	    public bool Remove()
	    {
			opened = false;

	    	if (panel != null)
	    	{
	    		panel.DoOnClose();
	    		parent.RemoveSearchPanel(panel);
	    		panel.Dispose();
	    		panel = null;
				parent.TextBox.KeyMap.RemoveAfter(keyMap);
	    		parent.TextBox.Focus();
	    		return true;
	    	}
	    	return false;
	    }

		private bool DoCloseSearch(Controller controller)
		{
			return Remove();
		}
	}
}
