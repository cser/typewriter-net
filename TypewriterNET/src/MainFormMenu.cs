using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

namespace TypewriterNET
{
	public class MainFormMenu : MainMenu
	{
		public MainFormMenu()
		{
		}

		public void SetItems(KeyMapNode node, TabInfoList fileList)
		{
			MenuItems.Clear();
	    	List<KeyAction> actions = new List<KeyAction>();
	    	Dictionary<KeyAction, bool> actionSet = new Dictionary<KeyAction, bool>();
	        Dictionary<KeyAction, List<KeyItem>> keysByAction = new Dictionary<KeyAction, List<KeyItem>>();
			List<KeyItem> keyItems = new List<KeyItem>();
			foreach (KeyMap keyMapI in node.ToList())
			{
				keyItems.AddRange(keyMapI.items);
				foreach (KeyItem keyItem in keyMapI.items)
				{
					if (keyItem.action != KeyAction.Nothing && !actionSet.ContainsKey(keyItem.action))
					{
						actionSet.Add(keyItem.action, true);
						actions.Add(keyItem.action);
					}
				}
			}
	        foreach (KeyItem keyItem in keyItems)
	        {
	        	List<KeyItem> list;
	        	keysByAction.TryGetValue(keyItem.action, out list);
	        	if (list == null)
	        	{
	        		list = new List<KeyItem>();
	        		keysByAction[keyItem.action] = list;
	        	}
	        	list.Add(keyItem);
	        }
	        Dictionary<string, Menu> itemByPath = new Dictionary<string, Menu>();
	        KeysConverter keysConverter = new KeysConverter();
	        foreach (KeyAction action in actions)
	        {
	        	string name = GetMenuItemName(action.name);
	        	List<KeyItem> keys;
	        	keysByAction.TryGetValue(action, out keys);
	        	if (keys != null)
	        	{
	        		bool first = true;
	        		foreach (KeyItem keyItem in keys)
	        		{
						if (keyItem.keys == Keys.None)
							continue;
						name += first ? "\t" : "/";
	        			first = false;
	        			if (action.doOnModeChange != null)
	        				name += "[";
	        			name += keysConverter.ConvertToString(keyItem.keys);
	        			if (action.doOnModeChange != null)
	        				name += "]";
	        		}
	        	}
	        	MenuItem item = new MenuItem(name, new MenuItemActionDelegate(action, fileList).OnClick);
	        	GetMenuItemParent(action.name, itemByPath).MenuItems.Add(item);
	        }
		}

	    private Menu GetMenuItemParent(string path, Dictionary<string, Menu> itemByPath)
	    {
	    	string parentPath = GetMenuItemParentPath(path);
	    	if (string.IsNullOrEmpty(parentPath))
	    		return this;
	    	Menu parent;
	    	itemByPath.TryGetValue(parentPath, out parent);
	    	if (parent != null)
	    		return parent;
    		MenuItem item = new MenuItem(GetMenuItemName(parentPath));
    		itemByPath[parentPath] = item;
    		GetMenuItemParent(parentPath, itemByPath).MenuItems.Add(item);
    		return item;
	    }

	    private static string GetMenuItemParentPath(string path)
	    {
	    	int index = path.LastIndexOf("\\");
	    	if (index == -1)
	    		return "";
	    	return path.Substring(0, index);
	    }

	    private static string GetMenuItemName(string path)
	    {
	    	int index = path.LastIndexOf("\\");
	    	if (index == -1)
	    		return path;
	    	return path.Substring(index + 1);
	    }

	    public class MenuItemActionDelegate
	    {
	    	private KeyAction action;
	    	private SwitchList<TabInfo> fileList;
	    	
	    	public MenuItemActionDelegate(KeyAction action, SwitchList<TabInfo> fileList)
	    	{
	    		this.action = action;
	    		this.fileList = fileList;
	    	}
	    	
	    	public void OnClick(object sender, EventArgs e)
	    	{
	    		TabInfo info = fileList.Selected;
	    		if (info != null)
	    		{
	    			if (action.doOnModeChange != null)
	    				action.doOnModeChange(info.Controller, true);
	    			action.doOnDown(info.Controller);
	    			if (action.doOnModeChange != null)
	    				action.doOnModeChange(info.Controller, false);
	    		}
	    	}
	    }
	}
}
