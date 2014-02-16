using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MulticaretEditor.KeyMapping
{
	public class KeyMap
	{
		private readonly RWList<KeyItem> _items;
		public readonly IRList<KeyItem> items;
		
		private Dictionary<Keys, KeyItem> itemByKeys;
		private Dictionary<Keys, RWList<KeyItem>> modeItemsByKeys;
		private Dictionary<char, char> altChars;
		
		private IRList<KeyItem> nullItems = new RWList<KeyItem>();
		
		public KeyMap()
		{
			_items = new RWList<KeyItem>();
			items = _items;
			itemByKeys = new Dictionary<Keys, KeyItem>();
			modeItemsByKeys = new Dictionary<Keys, RWList<KeyItem>>();
			altChars = new Dictionary<char, char>();
		}
		
		public void AddItem(KeyItem item)
		{
			AddItem(item, false);
		}
		
		public void AddItem(KeyItem item, bool asMain)
		{
			if (item.modeKeys != null)
			{
				item = new KeyItem(item.keys | item.modeKeys.Value, item.modeKeys, item.action);
			}
			_items.Add(item);
			if (item.keys != Keys.None)
			{
				KeyItem prevItem;
				itemByKeys.TryGetValue(item.keys, out prevItem);
				if (prevItem != null)
				{
					if (asMain)
					{
						item.next = prevItem;
						itemByKeys[item.keys] = item;
					}
					else
					{
						prevItem.next = item;
					}
				}
				else
				{
					itemByKeys.Add(item.keys, item);
				}
				if (item.modeKeys != null)
				{
					RWList<KeyItem> items;
					modeItemsByKeys.TryGetValue(item.modeKeys.Value, out items);
					if (items == null)
					{
						items = new RWList<KeyItem>();
						modeItemsByKeys.Add(item.modeKeys.Value, items);
					}
					items.Add(item);
				}
			}
		}
		
		public KeyItem GetItem(Keys keys)
		{
			KeyItem item;
			itemByKeys.TryGetValue(keys, out item);
			return item;
		}
		
		public IRList<KeyItem> GetModeItems(Keys keys)
		{
			RWList<KeyItem> items;
			modeItemsByKeys.TryGetValue(keys, out items);
			return items != null ? items : nullItems;
		}
		
		public KeyMap SetDefault()
		{
			AddItem(new KeyItem(Keys.Home, null, KeyAction.Home));
			AddItem(new KeyItem(Keys.Home | Keys.Shift, null, KeyAction.HomeWithSelection));
			AddItem(new KeyItem(Keys.End, null, KeyAction.End));
			AddItem(new KeyItem(Keys.End | Keys.Shift, null, KeyAction.EndWithSelection));
			AddItem(new KeyItem(Keys.A | Keys.Control, null, KeyAction.SelectAll));
			AddItem(new KeyItem(Keys.C | Keys.Control, null, KeyAction.Copy));
			AddItem(new KeyItem(Keys.V | Keys.Control, null, KeyAction.Paste));
			AddItem(new KeyItem(Keys.X | Keys.Control, null, KeyAction.Cut));
			AddItem(new KeyItem(Keys.Escape, null, KeyAction.ClearMinorSelections));
			AddItem(new KeyItem(Keys.Shift | Keys.Escape, null, KeyAction.ClearFirstMinorSelections));
			AddItem(new KeyItem(Keys.Delete, null, KeyAction.Delete));
			
			AddItem(new KeyItem(Keys.Left, null, KeyAction.Left));
			AddItem(new KeyItem(Keys.Left | Keys.Shift, null, KeyAction.LeftWithSelection));
			AddItem(new KeyItem(Keys.Left | Keys.Control, null, KeyAction.LeftWord));
			AddItem(new KeyItem(Keys.Left | Keys.Control | Keys.Shift, null, KeyAction.LeftWordWithSelection));
			
			AddItem(new KeyItem(Keys.Right, null, KeyAction.Right));
			AddItem(new KeyItem(Keys.Right | Keys.Shift, null, KeyAction.RightWithSelection));
			AddItem(new KeyItem(Keys.Right | Keys.Control, null, KeyAction.RightWord));
			AddItem(new KeyItem(Keys.Right | Keys.Control | Keys.Shift, null, KeyAction.RightWordWithSelection));
			
			AddItem(new KeyItem(Keys.Up, null, KeyAction.Up));
			AddItem(new KeyItem(Keys.Up | Keys.Shift, null, KeyAction.UpWithSelection));
			
			AddItem(new KeyItem(Keys.Down, null, KeyAction.Down));
			AddItem(new KeyItem(Keys.Down | Keys.Shift, null, KeyAction.DownWithSelection));
			
			AddItem(new KeyItem(Keys.Up | Keys.Alt | Keys.Shift, null, KeyAction.PutCursorUp));
			AddItem(new KeyItem(Keys.Down | Keys.Alt | Keys.Shift, null, KeyAction.PutCursorDown));
			
			AddItem(new KeyItem(Keys.Control | Keys.D, null, KeyAction.SelectNextText));
			
			AddItem(new KeyItem(Keys.Tab, null, KeyAction.ShiftRight));
			AddItem(new KeyItem(Keys.Tab | Keys.Shift, null, KeyAction.ShiftLeft));
			
			AddItem(new KeyItem(Keys.Control | Keys.Back, null, KeyAction.RemoveWordLeft));
			AddItem(new KeyItem(Keys.Control | Keys.Delete, null, KeyAction.RemoveWordRight));
			
			AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.Up, null, KeyAction.MoveLineUp));
			AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.Down, null, KeyAction.MoveLineDown));
			
			AddItem(new KeyItem(Keys.Control | Keys.Z, null, KeyAction.Undo));
			AddItem(new KeyItem(Keys.Control | Keys.Y, null, KeyAction.Redo));
			AddItem(new KeyItem(Keys.T, Keys.Control, KeyAction.SwitchBranch));
			
			AddItem(new KeyItem(Keys.PageUp, null, KeyAction.PageUp));
			AddItem(new KeyItem(Keys.PageDown, null, KeyAction.PageDown));
			AddItem(new KeyItem(Keys.PageUp | Keys.Shift, null, KeyAction.PageUpWithSelection));
			AddItem(new KeyItem(Keys.PageDown | Keys.Shift, null, KeyAction.PageDownWithSelection));
			
			AddItem(new KeyItem(Keys.Control | Keys.Up, null, KeyAction.ScrollUp));
			AddItem(new KeyItem(Keys.Control | Keys.Down, null, KeyAction.ScrollDown));
			
			return this;
		}
		
		public bool GetAltChar(char source, out char result)
		{
			return altChars.TryGetValue(source, out result);
		}
		
		public void SetAltChars(string source, string result)
		{
			altChars.Clear();
			if (source != null && result != null)
			{
				int count = Math.Min(source.Length, result.Length);
				for (int i = 0; i < count; i++)
				{
					altChars[source[i]] = result[i];
				}
			}
		}
		
		public const string DefaultAltCharsSource = "1234567890-=\\" + "qwertyuiop[]" + "asdl;'" + "zcnm,/" + "!$+QWERTYUIOPASDL:\"" + "ZCN<";
		public const string DefaultAltCharsResult = "¡²³¤€¼½¾‘’¥×¬" +  "äåé®þüúíóö«»" + "áßðø¶´" + "æ©ñµç¿" + "¹£÷ÄÅÉ ÞÜÚÍÓÖÁ§ÐØ°¨" +  "Æ¢ÑÇ";
		
		public KeyMap SetDefaultAltChars()
		{
			SetAltChars(DefaultAltCharsSource, DefaultAltCharsResult);
			return this;
		}
	}
}
