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
		
		dropDown = new AutocompleteMenu(textBox.Scheme, textBox.FontFamily, textBox.FontSize, textBox.ScrollingIndent);
		UpdateItems();
		dropDown.Show(textBox, point);
		
		textBox.KeyMap.AddBefore(keyMap);
		textBox.AfterKeyPress += OnKeyPress;
	}
	
	private Variant selectedVariant;
	private readonly List<Variant> filteredVariants = new List<Variant>();
	
	private void UpdateItems()
	{
		string word = textBox.Controller.Lines.GetText(startCaret, caret - startCaret).ToLower();
		List<ToolStripItem> items = new List<ToolStripItem>();
		filteredVariants.Clear();
		selectedVariant = null;
		for (int i = 0; i < variants.Count; i++)
		{
			Variant variant = variants[i];
			if (variant.CompletionText == null || variant.DisplayText == null ||
				!string.IsNullOrEmpty(word) && !variant.CompletionText.ToLower().Contains(word))
				continue;
			if (selectedVariant == null)
				selectedVariant = variant;
			filteredVariants.Add(variant);
		}
		dropDown.SetVariants(filteredVariants);
		dropDown.SetSelectedVariant(selectedVariant);
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
		if (dropDown == null || filteredVariants.Count == 0)
			return true;
		if (selectedVariant == null || !filteredVariants.Contains(selectedVariant))
			selectedVariant = filteredVariants[filteredVariants.Count - 1];
		int index = filteredVariants.IndexOf(selectedVariant);
		selectedVariant = filteredVariants[(index + filteredVariants.Count - 1) % filteredVariants.Count];
		dropDown.SetSelectedVariant(selectedVariant);
		return true;
	}
	
	private bool DoMoveDown(Controller controller)
	{
		if (dropDown == null || filteredVariants.Count == 0)
			return true;
		if (selectedVariant == null || !filteredVariants.Contains(selectedVariant))
			selectedVariant = filteredVariants[0];
		int index = filteredVariants.IndexOf(selectedVariant);
		selectedVariant = filteredVariants[(index + 1) % filteredVariants.Count];
		dropDown.SetSelectedVariant(selectedVariant);
		return true;
	}
	
	private bool DoClose(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoComplete(Controller controller)
	{
		string completionText = selectedVariant != null ? selectedVariant.CompletionText : null;
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