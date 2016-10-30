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
	private bool rawView;
	
	private KeyMap keyMap;
	
	public AutocompleteMode(MulticaretTextBox textBox, bool rawView)
	{
		this.textBox = textBox;
		this.rawView = rawView;
		
		keyMap = new KeyMap();
		keyMap.AddItem(new KeyItem(Keys.Up, null, new KeyAction("&View\\Autocomplete\\MoveUp", DoMoveUp, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Down, null, new KeyAction("&View\\Autocomplete\\MoveDown", DoMoveDown, null, false)));
		keyMap.AddItem(new KeyItem(Keys.PageUp, null, new KeyAction("&View\\Autocomplete\\MovePageUp", DoMovePageUp, null, false)));
		keyMap.AddItem(new KeyItem(Keys.PageDown, null, new KeyAction("&View\\Autocomplete\\MovePageDown", DoMovePageDown, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Home, null, new KeyAction("&View\\Autocomplete\\MoveToFirst", DoMoveToFirst, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.End, null, new KeyAction("&View\\Autocomplete\\MoveToLast", DoMoveToLast, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Autocomplete\\Close", DoClose, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("&View\\Autocomplete\\Complete", DoComplete, null, false)));
	}
	
	public class Handler
	{
		private AutocompleteMode mode;
		
		public Handler(AutocompleteMode mode)
		{
			this.mode = mode;
		}
		
		public MulticaretTextBox TextBox { get { return mode.textBox; } }
		public bool RawView { get { return mode.rawView; } }
		
		public void CheckPosition()
		{
			if (mode.dropDown != null)
			{
				Place place = mode.textBox.Controller.Lines.PlaceOf(mode.textBox.Controller.LastSelection.caret);
				if (place.iLine != mode.startPlace.iLine && (Math.Abs(place.iLine - mode.startPlace.iLine) > 1 || place.iChar > 0) ||
				    place.iChar < mode.startPlace.iChar)
				{
					mode.Close();
					return;
				}
				Point point = mode.textBox.ScreenCoordsOfPlace(mode.startPlace);
				if (point.Y < -mode.textBox.CharHeight || point.Y > mode.textBox.Height)
				{
					mode.Close();
					return;
				}
				point.Y += mode.textBox.CharHeight;
				mode.dropDown.SetScreenPosition(mode.textBox.PointToScreen(point));
				mode.dropDown.UpdateScreenPosition();
			}
		}
		
		public void ProcessSelect(Variant variant)
		{
			if (mode.dropDown != null && variant != null)
			{
				mode.selectedVariant = variant;
				mode.dropDown.SetSelectedVariant(mode.selectedVariant);
			}
		}
		
		public void ProcessComplete()
		{
			mode.DoComplete(null);
		}
	}

	private List<Variant> variants;	
	private bool opened;
	private int startCaret;
	private int caret;
	private Place startPlace;
	
	public void Show(List<Variant> variants, string leftWord)
	{
		if (opened)
			return;
		opened = true;
		
		this.variants = variants;
		startPlace = textBox.Controller.Lines.PlaceOf(textBox.Controller.LastSelection.caret - leftWord.Length);
		startCaret = textBox.Controller.LastSelection.caret - leftWord.Length;
		caret = textBox.Controller.LastSelection.caret;
		Point point = textBox.ScreenCoordsOfPlace(startPlace);
		point.Y += textBox.CharHeight;
		Point screenPoint = textBox.PointToScreen(point);
		
		dropDown = new AutocompleteMenu(new Handler(this));
		dropDown.SetScreenPosition(screenPoint);
		dropDown.Show(textBox, screenPoint);
		UpdateItems();
		
		textBox.KeyMap.AddBefore(keyMap);
		textBox.FocusedChange += OnFocusedChange;
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
		textBox.FocusedChange -= OnFocusedChange;
		textBox.KeyMap.RemoveBefore(keyMap);
		dropDown.Close();
	}
	
	private bool DoMoveUp(Controller controller)
	{
		return ProcessMove(controller, true);
	}
	
	private bool DoMoveDown(Controller controller)
	{
		return ProcessMove(controller, false);
	}
	
	private bool ProcessMove(Controller controller, bool isUp)
	{
		if (dropDown == null || filteredVariants.Count == 0)
		{
			Close();
			return false;
		}
		if (selectedVariant == null || !filteredVariants.Contains(selectedVariant))
			selectedVariant = filteredVariants[filteredVariants.Count - 1];
		int index = filteredVariants.IndexOf(selectedVariant);
		int next = isUp ? index + filteredVariants.Count - 1 : index + 1;
		selectedVariant = filteredVariants[next % filteredVariants.Count];
		dropDown.SetSelectedVariant(selectedVariant);
		return true;
	}
	
	private bool DoMovePageUp(Controller controller)
	{
		return ProcessMovePage(controller, -dropDown.maxLinesCount);
	}
	
	private bool DoMovePageDown(Controller controller)
	{
		return ProcessMovePage(controller, dropDown.maxLinesCount);
	}
	
	private bool DoMoveToFirst(Controller controller)
	{
		return ProcessMovePage(controller, -filteredVariants.Count);
	}
	
	private bool DoMoveToLast(Controller controller)
	{
		return ProcessMovePage(controller, filteredVariants.Count);
	}
	
	private bool ProcessMovePage(Controller controller, int offset)
	{
		if (dropDown == null || filteredVariants.Count == 0 || dropDown == null)
		{
			Close();
			return false;
		}
		if (selectedVariant == null || !filteredVariants.Contains(selectedVariant))
			selectedVariant = filteredVariants[filteredVariants.Count - 1];
		int index = filteredVariants.IndexOf(selectedVariant);
		int next = index + offset;
		if (next < 0)
		{
			next = 0;
		}
		else if (next >= filteredVariants.Count)
		{
			next = filteredVariants.Count - 1;
		}
		selectedVariant = filteredVariants[next];
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
			        Close();
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
			if (caret > startCaret)
			{
				string text = textBox.Controller.Lines.GetText(startCaret, caret - startCaret);
				for (int i = 0; i < text.Length; i++)
				{
					char c = text[i];
					if (c != '_' && !char.IsLetterOrDigit(c))
					{
						Close();
						return;
					}
				}
			}
			UpdateItems();
		}
	}
	
	private void OnFocusedChange()
	{
		if (!dropDown.Focused && !textBox.Focused)
		{
			Close();
		}
	}
}