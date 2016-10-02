using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

public class AutocompleteMode
{
	private AutocompleteMenu dropDown;
	private MulticaretTextBox textBox;
	private Buffer buffer;
	
	private KeyMap keyMap;
	
	public AutocompleteMode(MulticaretTextBox textBox, Buffer buffer)
	{
		this.textBox = textBox;
		this.buffer = buffer;
		
		keyMap = new KeyMap();
		keyMap.AddItem(new KeyItem(Keys.Up, null, new KeyAction("&View\\Autocomplete\\MoveUp", DoMoveUp, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Down, null, new KeyAction("&View\\Autocomplete\\MoveDown", DoMoveDown, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Autocomplete\\Close", DoClose, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("&View\\Autocomplete\\Complete", DoComplete, null, false)));
	}

	private List<Variant> variants;	
	private bool opened;
	private int startCaret;
	private int caret;
	
	public void Show(List<Variant> variants, string leftWord)
	{
		if (opened)
			return;
		opened = true;
		
		this.variants = variants;
		Place place = textBox.Controller.Lines.PlaceOf(textBox.Controller.LastSelection.caret - leftWord.Length);
		startCaret = textBox.Controller.LastSelection.caret - leftWord.Length;
		caret = textBox.Controller.LastSelection.caret;
		
		Point point = textBox.ScreenCoordsOfPlace(place);
		point.Y += textBox.CharHeight;
		
		dropDown = new AutocompleteMenu(textBox.Scheme, textBox.FontFamily, textBox.FontSize);
		UpdateItems();
		dropDown.Show(textBox, point);
		
		textBox.KeyMap.AddBefore(keyMap);
		textBox.AfterKeyPress += OnKeyPress;
	}
	
	private string completionText;
	
	private void UpdateItems()
	{
		string word = textBox.Controller.Lines.GetText(startCaret, caret - startCaret).ToLower();
		List<ToolStripItem> items = new List<ToolStripItem>();
		List<Variant> variants = new List<Variant>();
		completionText = null;
		for (int i = 0; i < this.variants.Count; i++)
		{
			Variant variant = this.variants[i];
			if (variant.CompletionText == null || variant.DisplayText == null ||
				!string.IsNullOrEmpty(word) && !variant.CompletionText.ToLower().Contains(word))
				continue;
			if (completionText == null)
				completionText = variant.CompletionText;
			variants.Add(variant);
		}
		dropDown.SetVariants(variants);
	}
	
	public void Close()
	{
		if (!opened)
			return;
		opened = false;
		
		textBox.AfterKeyPress -= OnKeyPress;
		textBox.KeyMap.RemoveBefore(keyMap);
		dropDown.Close();
	}
	
	private bool DoMoveUp(Controller controller)
	{
		return true;
	}
	
	private bool DoMoveDown(Controller controller)
	{
		return true;
	}
	
	private bool DoClose(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoComplete(Controller controller)
	{
		if (completionText != null)
		{
			int count = caret - startCaret;
			if (count < 0 || count > completionText.Length)
			{
				Close();
				return true;
			}
			textBox.Controller.InsertText(completionText.Substring(count));
		}
		return true;
	}
	
	private void OnKeyPress()
	{
		if (!opened)
			return;
		if (caret != textBox.Controller.LastSelection.caret)
		{
			caret = textBox.Controller.LastSelection.caret;
			if (caret < startCaret)
			{
				Close();
				return;
			}
			UpdateItems();
		}
	}
}