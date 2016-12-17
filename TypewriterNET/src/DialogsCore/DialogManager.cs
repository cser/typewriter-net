using System;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Resources;
using System.Xml;
using MulticaretEditor;

public class DialogManager
{
	public class DialogOwner<T> where T : ADialog
	{
		private readonly DialogManager manager;

		public DialogOwner(DialogManager manager)
		{
			this.manager = manager;
		}

		private T dialog;
		public T Dialog { get { return dialog; } }

		public void Open(T dialog, bool closeOther)
		{
			if (closeOther)
				manager.CloseDialogs();
			this.dialog = dialog;
			manager.closeMethods.Add(Close);
			
			Nest nest = manager.frames.AddParentNode();
			nest.hDivided = false;
			nest.left = false;
			nest.isPercents = false;
			nest.size = 1;
			dialog.Create(nest);
			if (!dialog.preventOpen)
			{
				dialog.Focus();
				dialog.NeedClose += OnNeedClose;
			}
			else
			{
				Close(true);
			}
		}

		public bool SwitchOpen()
		{
			if (dialog == null)
				return true;
			if (!dialog.Focused)
			{
				Close(false);
				return true;
			}
			Close(true);
			return false;
		}

		public bool Close(bool changeFocus)
		{
			if (changeFocus && dialog != null)
			{
				if (dialog.textBoxToFocus != null)
					dialog.textBoxToFocus.Focus();
				else if (manager.mainForm.LastFrame != null)
					manager.mainForm.LastFrame.Focus();
			}
			if (dialog != null)
			{
				manager.closeMethods.Remove(Close);
				dialog.Nest.Destroy();
				dialog = null;
				return true;
			}
			return false;
		}

		private void OnNeedClose()
		{
			Close(true);
		}
	}

	private MainForm mainForm;
	private TempSettings tempSettings;
	private FrameList frames;
	private List<Getter<bool, bool>> closeMethods;

	private DialogOwner<InfoDialog> info;
	private DialogOwner<FileIncrementalSearch> fileIncrementalSearch;
	private DialogOwner<RecentlyIncrementalSearch> recentlyIncrementalSearch;
	private DialogOwner<RecentlyDirsIncrementalSearch> recentlyDirsIncrementalSearch;
	private DialogOwner<MenuItemIncrementalSearch> menuItemIncrementalSearch;
	private DialogOwner<SyntaxIncrementalSearch> syntaxIncrementalSearch;
	private DialogOwner<EncodingIncrementalSearch> saveEncodingIncrementalSearch;
	private DialogOwner<EncodingIncrementalSearch> loadEncodingIncrementalSearch;
	private DialogOwner<SchemeIncrementalSearch> schemeIncrementalSearch;
	private DialogOwner<CommandDialog> command;
	private CommandDialog.Data commandData = new CommandDialog.Data();
	private DialogOwner<FindDialog> find;
	private FindDialog.Data findData;
	private DialogOwner<FindInFilesDialog> findInFiles;
	private FindInFilesDialog.Data findInFilesData;
	private DialogOwner<ReplaceDialog> replace;
	private ReplaceDialog.Data replaceData;
	private DialogOwner<FindDialog> goToLine;
	private FindDialog.Data goToLineData;
	private DialogOwner<FindDialog> input;
	private DialogOwner<RenameDialog> rename;
	private FindDialog.Data inputData;
	private MoveDialog.Data moveData;
	private DialogOwner<MoveDialog> move;

	public DialogManager(MainForm mainForm, TempSettings tempSettings)
	{
		this.mainForm = mainForm;
		this.tempSettings = tempSettings;
		frames = mainForm.frames;
		closeMethods = new List<Getter<bool, bool>>();

		KeyMap keyMap = mainForm.KeyMap;
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.OemSemicolon, null,
			new KeyAction("&View\\Open/close command dialog", DoInputCommand, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.OemMinus, null,
			new KeyAction("&View\\Set syntax...", DoSetSyntax, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Oemplus, null,
			new KeyAction("&View\\Set save encoding...", DoSetSaveEncoding, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.OemPipe, null,
			new KeyAction("&View\\Reload with encoding...", DoReloadWithEncoding, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.C, null,
			new KeyAction("&View\\Preview color scheme...", DoSchemeIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null,
			new KeyAction("F&ind\\Find...", DoFind, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.F, null,
			new KeyAction("F&ind\\Find in files...", DoFindInFiles, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.H, null,
			new KeyAction("F&ind\\Replace...", DoReplace, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.G, null,
			new KeyAction("F&ind\\Go to line...", DoGoToLine, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.P, null,
			new KeyAction("F&ind\\File incremental search...", DoFileIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.O, null,
			new KeyAction("F&ind\\Recently incremental search...", DoRecentlyIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.G, null,
			new KeyAction("F&ind\\Recently dirs incremental search...", DoRecentlyDirsIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.P, null,
			new KeyAction("F&ind\\Menu item incremental search...", DoMenuItemIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Escape, null,
			new KeyAction("F&ind\\Close dialogs", DoCloseDialogs, null, false)));

		info = new DialogOwner<InfoDialog>(this);
		fileIncrementalSearch = new DialogOwner<FileIncrementalSearch>(this);
		recentlyIncrementalSearch = new DialogOwner<RecentlyIncrementalSearch>(this);
		recentlyDirsIncrementalSearch = new DialogOwner<RecentlyDirsIncrementalSearch>(this);
		menuItemIncrementalSearch = new DialogOwner<MenuItemIncrementalSearch>(this);
		syntaxIncrementalSearch = new DialogOwner<SyntaxIncrementalSearch>(this);
		saveEncodingIncrementalSearch = new DialogOwner<EncodingIncrementalSearch>(this);
		loadEncodingIncrementalSearch = new DialogOwner<EncodingIncrementalSearch>(this);
		schemeIncrementalSearch = new DialogOwner<SchemeIncrementalSearch>(this);
		command = new DialogOwner<CommandDialog>(this);
		find = new DialogOwner<FindDialog>(this);
		findData = new FindDialog.Data(tempSettings.FindHistory);
		findInFiles = new DialogOwner<FindInFilesDialog>(this);
		findInFilesData = new FindInFilesDialog.Data(tempSettings.FindInFilesHistory, tempSettings.FindInFilesTempFilter, tempSettings.FindInFilesTempCurrentFilter);
		replace = new DialogOwner<ReplaceDialog>(this);
		replaceData = new ReplaceDialog.Data(tempSettings.ReplacePatternHistory, tempSettings.ReplaceHistory);
		goToLine = new DialogOwner<FindDialog>(this);
		goToLineData = new FindDialog.Data(tempSettings.GoToLineHistory);
		input = new DialogOwner<FindDialog>(this);
		inputData = new FindDialog.Data(null);
		rename = new DialogOwner<RenameDialog>(this);
		moveData = new MoveDialog.Data(tempSettings.MoveHistory);
		move = new DialogOwner<MoveDialog>(this);
	}

	public void ShowInfo(string name, string text)
	{
		if (info.Dialog == null)
			info.Open(new InfoDialog(), false);
		info.Dialog.Name = name;
		info.Dialog.InitText(text);
	}
	
	public void HideInfo(string name, string text)
	{
		if (info.Dialog != null && info.Dialog.Name == name && info.Dialog.SettedText == text)
		{
			info.Close(true);
		}
	}

	private bool DoInputCommand(Controller controller)
	{
		if (command.SwitchOpen())
			ShowInputCommand(null);
		return true;
	}
	
	public void ShowInputCommand(string text)
	{
		command.Open(new CommandDialog(commandData, "Command", text), true);
	}

	private bool DoFind(Controller controller)
	{
		if (find.SwitchOpen())
			find.Open(
				new FindDialog(findData, tempSettings.FindParams, DoFindText, DoSelectAllFinded, DoSelectNextFinded, "Find", null), true);
		return true;
	}

	private bool DoFindInFiles(Controller controller)
	{
		if (findInFiles.SwitchOpen())
			findInFiles.Open(
				new FindInFilesDialog(findInFilesData, tempSettings.FindParams, DoFindInFilesDialog, "Find in files"), true);
		return true;
	}

	private bool DoReplace(Controller controller)
	{
		if (replace.SwitchOpen())
			replace.Open(
				new ReplaceDialog(replaceData, tempSettings.FindParams, DoFindText, DoSelectAllFinded, DoSelectNextFinded, "Replace"), true);
		return true;
	}

	private bool DoFindText(string text)
	{
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			int index;
			int length;
			if (tempSettings.FindParams.regex)
			{
				string error;
				Regex regex = ParseRegex(text, out error);
				if (regex == null || error != null)
				{
					ShowInfo("FindInFiles", "Error: " + error);
					return true;
				}
				Match match = regex.Match(lastController.Lines.GetText(), lastController.Lines.LastSelection.Right);
				index = -1;
				length = text.Length;
				if (match.Success)
				{
					index = match.Index;
					length = match.Length;
				}
				else
				{
					match = regex.Match(lastController.Lines.GetText(), 0);
					if (match.Success)
					{
						index = match.Index;
						length = match.Length;
					}
				}
			}
			else
			{
				length = text.Length;
				CompareInfo ci = tempSettings.FindParams.ignoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
				index = ci != null ?
					ci.IndexOf(lastController.Lines.GetText(), text, lastController.Lines.LastSelection.Right, CompareOptions.IgnoreCase) :
					lastController.Lines.IndexOf(text, lastController.Lines.LastSelection.Right);
				if (index == -1)
					index = ci != null ?
						ci.IndexOf(lastController.Lines.GetText(), text, 0, CompareOptions.IgnoreCase) :
						lastController.Lines.IndexOf(text, 0);
			}
			if (index != -1)
			{
				lastController.PutCursor(lastController.Lines.PlaceOf(index), false);
				lastController.PutCursor(lastController.Lines.PlaceOf(index + length), true);
				mainForm.LastFrame.TextBox.MoveToCaret();
			}
		}
		return true;
	}
	
	private bool DoSelectAllFinded(string text)
	{
		bool result = true;
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			string all = lastController.Lines.GetText();
			int minIndex = 0;
			int maxIndex = all.Length;
			
			Regex regex = null;
			if (tempSettings.FindParams.regex)
			{
				string error;
				regex = ParseRegex(text, out error);
				if (regex == null || error != null)
				{
					ShowInfo("Select all finded", "Error: " + error);
					return false;
				}
			}
			
			if (!lastController.LastSelection.Empty && lastController.SelectionsCount == 1)
			{
				string selectionText = all.Substring(lastController.LastSelection.Left, lastController.LastSelection.Count);
				bool useArea = false;
				if (tempSettings.FindParams.regex)
				{
					Match match = regex.Match(selectionText);
					useArea = !match.Success || match.Length != selectionText.Length;
				}
				else
				{
					useArea = text != selectionText;
				}
				if (useArea)
				{
					result = false;
					minIndex = lastController.LastSelection.Left;
					maxIndex = lastController.LastSelection.Right;
				}
			}
			List<Selection> selections = new List<Selection>();

			int start = minIndex;			
			while (true)
			{
				int index;
				int length;
				if (tempSettings.FindParams.regex)
				{
					Match match = regex.Match(all, start);
					index = -1;
					length = text.Length;
					if (match.Success)
					{
						index = match.Index;
						length = match.Length;
					}
				}
				else
				{
					length = text.Length;
					CompareInfo ci = tempSettings.FindParams.ignoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
					index = ci != null ?
						ci.IndexOf(all, text, start, CompareOptions.IgnoreCase) :
						all.IndexOf(text, start);
				}
				if (index == -1 || index + length > maxIndex)
				{
					break;
				}
				Selection selection = new Selection();
				selection.anchor = index;
				selection.caret = index + length;
				selections.Add(selection);
				start = index + length;
			}
			if (selections.Count > 0)
			{
				lastController.ClearMinorSelections();
				
				Selection selection = selections[0];
				lastController.PutCursor(lastController.Lines.PlaceOf(selection.anchor), false);
				lastController.PutCursor(lastController.Lines.PlaceOf(selection.caret), true);
				for (int i = 1; i < selections.Count; i++)
				{
					selection = selections[i];
					lastController.PutNewCursor(lastController.Lines.PlaceOf(selection.anchor));
					lastController.PutCursor(lastController.Lines.PlaceOf(selection.caret), true);
				}
				mainForm.LastFrame.TextBox.MoveToCaret();
			}
		}
		return result;
	}
	
	private bool DoSelectNextFinded(string text)
	{
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			int index;
			int length;
			if (tempSettings.FindParams.regex)
			{
				string error;
				Regex regex = ParseRegex(text, out error);
				if (regex == null || error != null)
				{
					ShowInfo("FindInFiles", "Error: " + error);
					return true;
				}
				Match match = regex.Match(lastController.Lines.GetText(), lastController.Lines.LastSelection.Right);
				index = -1;
				length = text.Length;
				if (match.Success)
				{
					index = match.Index;
					length = match.Length;
				}
				else
				{
					match = regex.Match(lastController.Lines.GetText(), 0);
					if (match.Success)
					{
						index = match.Index;
						length = match.Length;
					}
				}
			}
			else
			{
				length = text.Length;
				CompareInfo ci = tempSettings.FindParams.ignoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
				index = ci != null ?
					ci.IndexOf(lastController.Lines.GetText(), text, lastController.Lines.LastSelection.Right, CompareOptions.IgnoreCase) :
					lastController.Lines.IndexOf(text, lastController.Lines.LastSelection.Right);
				if (index == -1)
					index = ci != null ?
						ci.IndexOf(lastController.Lines.GetText(), text, 0, CompareOptions.IgnoreCase) :
						lastController.Lines.IndexOf(text, 0);
			}
			if (index != -1)
			{
				if (lastController.Lines.LastSelection.Right != index)
				{
					lastController.PutNewCursor(lastController.Lines.PlaceOf(index));
					lastController.PutCursor(lastController.Lines.PlaceOf(index + length), true);
				}
				else
				{
					lastController.PutNewCursor(lastController.Lines.PlaceOf(index + length));
					lastController.PutCursor(lastController.Lines.PlaceOf(index), true);
					Selection lastSelection = lastController.Lines.selections[lastController.Lines.selections.Count - 1];
					if (lastSelection.anchor == index + length && lastSelection.caret == index)
					{
						int temp = lastSelection.anchor;
						lastSelection.anchor = lastSelection.caret;
						lastSelection.caret = temp;
					}
				}
				mainForm.LastFrame.TextBox.MoveToCaret();
			}
		}
		return true;
	}

	private bool DoFindInFilesDialog(string text, string filter)
	{
		findInFiles.Close(true);
		string errors = new FindInFiles(mainForm)
			.Execute(text, tempSettings.FindParams, mainForm.Settings.findInFilesDir.Value, filter);
		if (errors != null)
			ShowInfo("FindInFiles", errors);
		return true;
	}

	private Place? GetLastPlace()
	{
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			return lastController.Lines.PlaceOf(lastController.LastSelection.caret);
		}
		return null;
	}

	private bool DoGoToLine(Controller controller)
	{
		if (goToLine.SwitchOpen())
		{
			Place? place = GetLastPlace();
			if (string.IsNullOrEmpty(goToLineData.oldText) && place != null)
				goToLineData.oldText = place.Value.iLine + "";
			goToLine.Open(new FindDialog(
				goToLineData, null, DoGoToLine, null, null,
				"Go to line" +
				(place != null ?
					" (current line: " + (place.Value.iLine + 1) + ", char: " + (place.Value.iChar + 1) + ")" : ""),
				null
			), true);
		}
		return true;
	}

	private bool DoGoToLine(string text)
	{
		if (string.IsNullOrEmpty(text))
			return true;
		goToLine.Close(false);
		int iLine;
		try
		{
			iLine = int.Parse(text);
		}
		catch (Exception e)
		{
			ShowInfo("Go to line", e.Message);
			return true;
		}
		iLine--;
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			int iChar = lastController.Lines[iLine].GetFirstSpaces();
			lastController.PutCursor(new Place(iChar, iLine), false);
			mainForm.LastFrame.TextBox.MoveToCaret();
			mainForm.LastFrame.Focus();
		}
		return true;
	}

	private bool DoFileIncrementalSearch(Controller controller)
	{
		if (fileIncrementalSearch.SwitchOpen())
			fileIncrementalSearch.Open(new FileIncrementalSearch(tempSettings), false);
		return true;
	}
	
	private bool DoRecentlyIncrementalSearch(Controller controller)
	{
		if (recentlyIncrementalSearch.SwitchOpen())
			recentlyIncrementalSearch.Open(new RecentlyIncrementalSearch(tempSettings), false);
		return true;
	}
	
	private bool DoRecentlyDirsIncrementalSearch(Controller controller)
	{
		if (recentlyDirsIncrementalSearch.SwitchOpen())
			recentlyDirsIncrementalSearch.Open(new RecentlyDirsIncrementalSearch(tempSettings), false);
		return true;
	}

	private bool DoMenuItemIncrementalSearch(Controller controller)
	{
		if (menuItemIncrementalSearch.SwitchOpen())
			menuItemIncrementalSearch.Open(
				new MenuItemIncrementalSearch(tempSettings, mainForm.GetFocusedTextBox()), false);
		return true;
	}
	
	private bool DoSchemeIncrementalSearch(Controller controller)
	{
		if (schemeIncrementalSearch.SwitchOpen())
			schemeIncrementalSearch.Open(new SchemeIncrementalSearch(tempSettings), false);
		return true;
	}

	private bool DoSetSyntax(Controller controller)
	{
		if (syntaxIncrementalSearch.Dialog == null)
			syntaxIncrementalSearch.Open(new SyntaxIncrementalSearch(tempSettings), false);
		return true;
	}
	
	private bool DoSetSaveEncoding(Controller controller)
	{
		if (saveEncodingIncrementalSearch.Dialog == null)
			saveEncodingIncrementalSearch.Open(new EncodingIncrementalSearch(tempSettings, true), false);
		return true;
	}
	
	private bool DoReloadWithEncoding(Controller controller)
	{
		if (loadEncodingIncrementalSearch.Dialog == null)
			loadEncodingIncrementalSearch.Open(new EncodingIncrementalSearch(tempSettings, false), false);
		return true;
	}

	private bool DoCloseDialogs(Controller controller)
	{
		return CloseDialogs();
	}

	private bool CloseDialogs()
	{
		bool result = false;
		foreach (Getter<bool, bool> closeMethod in closeMethods.ToArray())
		{
			if (closeMethod(false))
				result = true;
		}
		closeMethods.Clear();
		return result;
	}

	public static Regex ParseRegex(string regexText, out string error)
	{
		Regex regex = null;
		RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.Multiline;
		string rawRegex;
		if (regexText.Length > 2 && regexText[0] == '/' && regexText.LastIndexOf("/") > 1)
		{
			int lastIndex = regexText.LastIndexOf("/");
			string optionsText = regexText.Substring(lastIndex + 1);
			rawRegex = regexText.Substring(1, lastIndex - 1);
			for (int i = 0; i < optionsText.Length; i++)
			{
				char c = optionsText[i];
				if (c == 'i')
					options |= RegexOptions.IgnoreCase;
				else if (c == 's')
					options &= ~RegexOptions.Multiline;
				else if (c == 'e')
					options |= RegexOptions.ExplicitCapture;
				else
				{
					error = "Unsupported regex option: " + c;
					return null;
				}
			}
		}
		else
		{
			rawRegex = regexText;
		}
		try
		{
			regex = new Regex(rawRegex, options);
		}
		catch (Exception e)
		{
			error = "Incorrect regex: " + regexText + " - " + e.Message;
			return null;
		}
		error = null;
		return regex;
	}

	public bool OpenInput(string title, string text, Getter<string, bool> doInput)
	{
		if (input.SwitchOpen())
		{
			inputData.oldText = text;
			input.Open(new FindDialog(inputData, null, doInput, null, null, title, null), true);
		}
		return true;
	}
	
	public bool OpenRename(string title, string text, List<bool> isDirectory, Getter<string, bool> doInput)
	{
		if (rename.SwitchOpen())
		{
			rename.Open(new RenameDialog(doInput, title, text, isDirectory), true);
		}
		return true;
	}
	
	public bool OpenMove(string title, string text, Getter<string, bool> doInput)
	{
		if (move.SwitchOpen())
		{
			move.Open(new MoveDialog(moveData, doInput, title, text), true);
		}
		return true;
	}

	public void CloseInput()
	{
		input.Close(true);
	}
	
	public void CloseRename()
	{
		rename.Close(true);
	}
}
