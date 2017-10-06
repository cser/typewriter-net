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
	private DialogOwner<SnippetIncrementalSearch> snippetIncrementalSearch;
	private DialogOwner<EncodingIncrementalSearch> saveEncodingIncrementalSearch;
	private DialogOwner<EncodingIncrementalSearch> loadEncodingIncrementalSearch;
	private DialogOwner<SchemeIncrementalSearch> schemeIncrementalSearch;
	private DialogOwner<CommandDialog> command;
	private CommandDialog.Data commandData = new CommandDialog.Data();
	private DialogOwner<FindDialog> find;
	private DialogOwner<ViFindDialog> viFind;
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
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.OemSemicolon, null,
			new KeyAction("&View\\Open/close command dialog (no history)", DoInputCommandNoHistory, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.OemMinus, null,
			new KeyAction("&View\\Set syntax…", DoSetSyntax, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Oemplus, null,
			new KeyAction("&View\\Set save encoding…", DoSetSaveEncoding, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.OemPipe, null,
			new KeyAction("&View\\Reload with encoding…", DoReloadWithEncoding, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.C, null,
			new KeyAction("&View\\Preview color scheme…", DoSchemeIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null,
			new KeyAction("F&ind\\Find…", DoFind, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.F, null,
			new KeyAction("F&ind\\Find in files…", DoFindInFiles, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.H, null,
			new KeyAction("F&ind\\Replace…", DoReplace, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.G, null,
			new KeyAction("F&ind\\Go to line…", DoGoToLine, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("F&ind\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.P, null,
			new KeyAction("F&ind\\File incremental search…", DoFileIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.O, null,
			new KeyAction("F&ind\\Recently incremental search…", DoRecentlyIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.G, null,
			new KeyAction("F&ind\\Recently dirs incremental search…", DoRecentlyDirsIncrementalSearch, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.P, null,
			new KeyAction("F&ind\\Menu item incremental search…", DoMenuItemIncrementalSearch, null, false)));
		KeyAction escape = new KeyAction("F&ind\\Close dialogs", DoCloseDialogs, null, false);
		keyMap.AddItem(new KeyItem(Keys.Escape, null, escape));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.OemOpenBrackets, null, escape));

		info = new DialogOwner<InfoDialog>(this);
		fileIncrementalSearch = new DialogOwner<FileIncrementalSearch>(this);
		recentlyIncrementalSearch = new DialogOwner<RecentlyIncrementalSearch>(this);
		recentlyDirsIncrementalSearch = new DialogOwner<RecentlyDirsIncrementalSearch>(this);
		menuItemIncrementalSearch = new DialogOwner<MenuItemIncrementalSearch>(this);
		syntaxIncrementalSearch = new DialogOwner<SyntaxIncrementalSearch>(this);
		snippetIncrementalSearch = new DialogOwner<SnippetIncrementalSearch>(this);
		saveEncodingIncrementalSearch = new DialogOwner<EncodingIncrementalSearch>(this);
		loadEncodingIncrementalSearch = new DialogOwner<EncodingIncrementalSearch>(this);
		schemeIncrementalSearch = new DialogOwner<SchemeIncrementalSearch>(this);
		command = new DialogOwner<CommandDialog>(this);
		find = new DialogOwner<FindDialog>(this);
		viFind = new DialogOwner<ViFindDialog>(this);
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
			ShowInputCommand(null, false);
		return true;
	}
	
	private bool DoInputCommandNoHistory(Controller controller)
	{
		if (command.SwitchOpen())
			ShowInputCommand(null, true);
		return true;
	}
	
	public void ShowInputCommand(string text, bool ignoreHistory)
	{
		command.Open(new CommandDialog(commandData, "Command", text, ignoreHistory), true);
	}

	private bool DoFind(Controller controller)
	{
		if (find.SwitchOpen())
			find.Open(new FindDialog(findData, tempSettings.FindParams, DoFindText,
				DoSelectAllFound, DoSelectNextFound, DoUnselectPrevText, "Find", false), true);
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
			replace.Open(new ReplaceDialog(replaceData, tempSettings.FindParams, DoFindText,
				DoSelectAllFound, DoSelectNextFound, DoUnselectPrevText, "Replace"), true);
		return true;
	}

	private bool DoFindText(string text)
	{
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			lastController.DialogsExtension.FindNext(
				text, tempSettings.FindParams.regex, tempSettings.FindParams.ignoreCase);
			ProcessAfterControllerExtension(lastController.DialogsExtension);
		}
		return true;
	}
	
	private bool DoSelectAllFound(string text)
	{
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			bool result = lastController.DialogsExtension.SelectAllFound(
				text, tempSettings.FindParams.regex, tempSettings.FindParams.ignoreCase);
			ProcessAfterControllerExtension(lastController.DialogsExtension);
			return result;
		}
		return true;
	}
	
	private bool DoSelectNextFound(string text)
	{
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			lastController.DialogsExtension.SelectNextFound(
				text, tempSettings.FindParams.regex, tempSettings.FindParams.ignoreCase);
			ProcessAfterControllerExtension(lastController.DialogsExtension);
		}
		return true;
	}
	
	private void ProcessAfterControllerExtension(ControllerDialogsExtension extension)
	{
		if (extension.NeedMoveToCaret)
		{
			mainForm.LastFrame.TextBox.MoveToCaret();
		}
		if (extension.NeedShowError != null)
		{
			ShowInfo("FindInFiles", "Error: " + extension.NeedShowError);
		}
	}
	
	private bool DoUnselectPrevText()
	{
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			if (lastController != null)
			{
				lastController.UnselectPrevText();
				mainForm.LastFrame.TextBox.MoveToCaret();
			}
		}
		return true;
	}

	private bool DoFindInFilesDialog(string text, string filter)
	{
		findInFiles.Close(true);
		string errors = new FindInFiles(mainForm).Execute(
			text,
			tempSettings.FindParams,
			mainForm.Settings.findInFilesDir.Value,
			filter,
			mainForm.Settings.findInFilesIgnoreDir.Value);
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
				goToLineData, null, DoGoToLine, null, null, null,
				"Go to line" +
				(place != null ?
					" (current line: " + (place.Value.iLine + 1) + ", char: " + (place.Value.iChar + 1) + ")" : ""),
				true
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
			if (iLine < 0)
			{
				iLine = 0;
			}
			else if (iLine >= lastController.Lines.LinesCount)
			{
				iLine = lastController.Lines.LinesCount - 1;
			}
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
			fileIncrementalSearch.Open(new FileIncrementalSearch(tempSettings, findInFilesData), false);
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
			schemeIncrementalSearch.Open(new SchemeIncrementalSearch(tempSettings, mainForm.Settings), false);
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
			input.Open(new FindDialog(inputData, null, doInput, null, null, null, title, true), true);
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
	
	public void OpenSnippetsSearch()
	{
		if (snippetIncrementalSearch.Dialog == null)
			snippetIncrementalSearch.Open(new SnippetIncrementalSearch(tempSettings), false);
	}
	
	private bool ViDoFindText(string text, Pattern pattern, bool isBackward)
	{
		viFind.Close(true);
		if (mainForm.LastFrame != null)
		{
			mainForm.LastFrame.TextBox.ViFind(pattern, isBackward);
			mainForm.LastFrame.TextBox.Controller.ViAddHistoryPosition(true);
		}
		return true;
	}
	
	public bool DoOnViShortcut(Controller controller, string shortcut)
	{
		if (shortcut == "/" || shortcut == "?")
		{
			if (viFind.SwitchOpen())
				viFind.Open(new ViFindDialog(findData, tempSettings.FindParams, ViDoFindText, shortcut == "?"), true);
			return true;
		}
		if (shortcut == "C/" || shortcut == "C?")
		{
			if (viFind.SwitchOpen())
				viFind.Open(new ViFindDialog(findData, new FindParams(), ViDoFindText, shortcut == "C?"), true);
			return true;
		}
		if (shortcut == ":")
		{
			return DoInputCommand(controller);
		}
		return false;
	}
}
