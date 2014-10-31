using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;

public class MenuItemIncrementalSearch : IncrementalSearchBase
{
	public MenuItemIncrementalSearch(MulticaretTextBox textBox)
		: base("Search in menu", "Menu item incremental search")
	{
		this.textBox = textBox;
	}

	public class Item
	{
		public string text;
		public string name;
		public string keys;
		public KeyAction action;
	}

	private List<Item> items = new List<Item>();
	private MulticaretTextBox textBox;

	override protected bool Prebuild()
	{
		textBoxToFocus = textBox;
		if (textBox == null)
			return false;
		KeyMapNode node = textBox.KeyMap;
		
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
		items.Clear();
		Dictionary<string, Menu> itemByPath = new Dictionary<string, Menu>();
		KeysConverter keysConverter = new KeysConverter();
		int length = 0;
		foreach (KeyAction action in actions)
		{
			if (action.name == "-" || action.name.EndsWith("\\-"))
				continue;
			Item item = new Item();
			item.name = action.name.Replace("&", "");
			List<KeyItem> keys;
			keysByAction.TryGetValue(action, out keys);
			if (keys != null && keys.Count > 0)
				item.keys = MainFormMenu.GetShortcutText(action, keys, keysConverter).Replace("\t", "");
			else
				item.keys = "";
			item.action = action;
			items.Add(item);
			if (item.name.Length + item.keys.Length + 2 > length)
				length = item.name.Length + item.keys.Length + 2;
		}
		foreach (Item item in items)
		{
			if (!string.IsNullOrEmpty(item.keys))
				item.text = item.name + new string(' ', length - item.name.Length - item.keys.Length) + item.keys;
			else
				item.text = item.name;
		}
		return true;
	}

	private List<Item> filteredItems = new List<Item>();
	private string compareText;

	override protected string GetVariantsText(string text)
	{
		compareText = text;
		filteredItems.Clear();
		foreach (Item item in items)
		{
			if (item.name.Contains(text))
				filteredItems.Add(item);
		}
		filteredItems.Sort(CompareItems);
		StringBuilder builder = new StringBuilder();
		bool first = true;
		foreach (Item item in filteredItems)
		{
			if (!first)
				builder.AppendLine();
			first = false;
			builder.Append(item.text);
		}
		return builder.ToString();
	}

	private int CompareItems(Item item0, Item item1)
	{
		int index0 = item0.name.LastIndexOf(compareText);
		int index1 = item1.name.LastIndexOf(compareText);
		int separatorCriterion0 = index0 == item0.name.LastIndexOf("\\") + 1 ? 1 : 0;
		int separatorCriterion1 = index1 == item1.name.LastIndexOf("\\") + 1 ? 1 : 0;
		if (separatorCriterion0 != separatorCriterion1)
			return separatorCriterion0 - separatorCriterion1;
		int offset0 = item0.name.Length - index0;
		int offset1 = item1.name.Length - index1;
		if (offset0 != offset1)
			return offset1 - offset0;
		return item1.name.Length - item0.name.Length;
	}

	override protected void Execute(int line, string lineText)
	{
		if (line >= filteredItems.Count)
			return;
		Item item = filteredItems[line];
		KeyAction action = item.action;
		if (textBox == null || textBox.Controller == null)
			return;
		Controller controller = textBox.Controller;
		if (action.doOnModeChange != null)
			action.doOnModeChange(controller, true);
		action.doOnDown(controller);
		if (action.doOnModeChange != null)
			action.doOnModeChange(controller, false);
		DispatchNeedClose();
	}
}
