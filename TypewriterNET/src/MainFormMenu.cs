using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

namespace TypewriterNET
{
	public class MainFormMenu : MainMenu
	{
		private MainForm mainForm;
		private List<string> names;

		public KeyMapNode node;

		public MainFormMenu(MainForm mainForm)
		{
			this.mainForm = mainForm;

			names = new List<string>();
			AddRootItem("&File", false);
			AddRootItem("&Edit", false);
			AddRootItem("F&ind", false);
			AddRootItem("&View", false);
			AddRootItem("Prefere&nces", false);
			AddRootItem("&Other", true);
			AddRootItem("&?", false);
		}

		private void AddRootItem(string name, bool isOther)
		{
			if (!isOther)
				names.Add(name);
			MenuItems.Add(new DynamicMenuItem(this, name, isOther));
		}

		public class DynamicMenuItem : MenuItem
		{
			private MainFormMenu menu;
			private string name;
			private bool isOther;

			public DynamicMenuItem(MainFormMenu menu, string name, bool isOther) : base(name)
			{
				this.name = name;
				this.menu = menu;
				this.isOther = isOther;

				MenuItems.Add(new MenuItem(" "));
				Popup += OnPopup;
			}

			private void OnPopup(object sender, EventArgs e)
			{
				MenuItems.Clear();
				menu.BuildItems(this, name, isOther);
				if (MenuItems.Count == 0)
				{
					MenuItem item = new MenuItem("[Empty]");
					item.Enabled = false;
					MenuItems.Add(item);
				}
			}
		}

		private void BuildItems(MenuItem root, string rootName, bool isOther)
		{
			if (node == null)
				return;
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
	        	string itemName = GetMenuItemName(action.name);
	        	List<KeyItem> keys;
	        	keysByAction.TryGetValue(action, out keys);
	        	if (keys != null)
	        	{
	        		bool first = true;
	        		foreach (KeyItem keyItem in keys)
	        		{
						if (keyItem.keys == Keys.None)
							continue;
						itemName += first ? "\t" : "/";
	        			first = false;
	        			if (action.doOnModeChange != null)
	        				itemName += "[";
	        			itemName += keysConverter.ConvertToString(keyItem.keys);
	        			if (action.doOnModeChange != null)
	        				itemName += "]";
	        		}
	        	}
				bool filtered;
				if (isOther)
				{
					filtered = true;
					foreach (string name in names)
					{
						if (action.name.StartsWith(name + "\\"))
						{
							filtered = false;
							break;
						}
					}
				}
				else
				{
					filtered = action.name.StartsWith(rootName + "\\");
				}
				if (filtered)
				{
					MenuItem item = new MenuItem(itemName, new MenuItemActionDelegate(mainForm, action).OnClick);
					GetMenuItemParent(root, rootName, action.name, itemByPath).MenuItems.Add(item);
				}
	        }
		}

	    private Menu GetMenuItemParent(MenuItem root, string rootName, string path, Dictionary<string, Menu> itemByPath)
	    {
	    	string parentPath = GetMenuItemParentPath(path);
	    	if (parentPath == rootName || string.IsNullOrEmpty(parentPath))
	    		return root;
	    	Menu parent;
	    	itemByPath.TryGetValue(parentPath, out parent);
	    	if (parent != null)
	    		return parent;
    		MenuItem item = new MenuItem(GetMenuItemName(parentPath));
    		itemByPath[parentPath] = item;
    		GetMenuItemParent(root, rootName, parentPath, itemByPath).MenuItems.Add(item);
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
			private MainForm mainForm;
	    	private KeyAction action;
	    	
	    	public MenuItemActionDelegate(MainForm mainForm, KeyAction action)
	    	{
				this.mainForm = mainForm;
	    		this.action = action;
	    	}
	    	
	    	public void OnClick(object sender, EventArgs e)
	    	{
	    		Controller controller = mainForm.FocusedController;
				if (action.doOnModeChange != null)
					action.doOnModeChange(controller, true);
				action.doOnDown(controller);
				if (action.doOnModeChange != null)
					action.doOnModeChange(controller, false);
	    	}
	    }
	}
}
