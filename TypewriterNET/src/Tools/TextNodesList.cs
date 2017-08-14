using System;
using System.Windows.Forms;
using MulticaretEditor;

public class TextNodesList : Buffer
{
	public TextNodesList() : base(null, "Nodes list", SettingsMode.TabList)
	{
		showEncoding = false;
		Controller.isReadonly = true;
		additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&View\\Nodes list\\Close nodes list", DoCloseBuffer, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.OemOpenBrackets, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\Nodes list\\Jump to node", DoOpenTab, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
		}
	}
	
	private bool DoCloseBuffer(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoOpenTab(Controller controller)
	{
		return true;
	}
	
	public void Close()
	{
		if (Frame != null)
		{
			Frame.RemoveBuffer(this);
		}
	}
}