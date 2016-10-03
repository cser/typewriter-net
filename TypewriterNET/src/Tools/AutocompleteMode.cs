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
		dropDown.SetScreenPosition(textBox.PointToScreen(point));
		dropDown.Show(textBox, point);
		UpdateItems();
		
		textBox.KeyMap.AddBefore(keyMap);
		textBox.AfterKeyPress += OnKeyPress;
	}
	
	private Variant selectedVariant;
	private readonly List<Variant> filteredVariants = new List<Variant>();
	
	private void UpdateItems()
	{
		string word = textBox.Controller.Lines.GetText(startCaret, caret - startCaret);
		string wordIgnoredCase = word.ToLower();
		List<ToolStripItem> items = new List<ToolStripItem>();
		filteredVariants.Clear();
		selectedVariant = null;
		for (int i = 0; i < variants.Count; i++)
		{
			Variant variant = variants[i];
			if (variant.CompletionText == null || variant.DisplayText == null ||
				!string.IsNullOrEmpty(word) && !variant.CompletionText.ToLower().Contains(wordIgnoredCase))
				continue;
			filteredVariants.Add(variant);
		}
		Compare_Word = word;
		Compare_WordIgnoredCase = wordIgnoredCase;
		filteredVariants.Sort(CompareFilteredVariants);
		if (selectedVariant == null && filteredVariants.Count > 0)
			selectedVariant = filteredVariants[0];
		dropDown.SetVariants(filteredVariants);
		dropDown.SetSelectedVariant(selectedVariant);
	}
	
	private string Compare_Word;
	private string Compare_WordIgnoredCase;
	
	private int CompareFilteredVariants(Variant v0, Variant v1)
	{
	    if (string.IsNullOrEmpty(Compare_Word))
	        return string.Compare(v0.CompletionText, v1.CompletionText);
	    string text0 = v0.CompletionText.ToLower();
	    string text1 = v1.CompletionText.ToLower();
	    int index0 = text0.IndexOf(Compare_WordIgnoredCase);
	    int index1 = text1.IndexOf(Compare_WordIgnoredCase);
	    if (index0 == -1 || index1 == -1)
	        return index1 - index0;
	    if (index0 == index1)
	    {
	        index0 = v0.CompletionText.IndexOf(Compare_Word);
            index1 = v1.CompletionText.IndexOf(Compare_Word);
            if (index0 == -1 || index1 == -1)
	            return index1 - index0;
	        if (index0 == index1)
	            string.Compare(v0.CompletionText, v1.CompletionText);
	    }
	    return index0 - index1;
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
		{
			Close();
			return true;
		}
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
		{
			Close();
			return false;
		}
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
				return false;
			}
			if (count > 0)
			{
			    string leftText = textBox.Controller.Lines.GetText(startCaret, count);
			    if (leftText != completionText.Substring(0, count))
			    {
			        textBox.Controller.ClearMinorSelections();
			        textBox.Controller.LastSelection.anchor = textBox.Controller.LastSelection.caret;
			        for (int i = 0; i < count; i++)
			        {
			            textBox.Controller.Backspace();
			        }
			        textBox.Controller.InsertText(completionText);
			        return true;
			    }
			}
			foreach (Selection selection in textBox.Controller.Selections)
			{
			    selection.anchor = selection.caret;
			}
			textBox.Controller.InsertText(completionText.Substring(count));
		}
		Close();
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