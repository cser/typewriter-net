using System;
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
using MulticaretEditor.Highlighting;
using MulticaretEditor.KeyMapping;

public class DialogManager
{
	public class DialogOwner<T> where T : ADialog
	{
		private DialogManager manager;

		public DialogOwner(DialogManager manager)
		{
			this.manager = manager;
		}

		private T dialog;
		public T Dialog { get { return dialog; } }

		public void Open(T dialog)
		{
			if (dialog != null)
				Close();
			this.dialog = dialog;
			AddBottomNest(dialog);
			dialog.NeedClose += OnNeedClose;
		}

		public void Close()
		{
			if (dialog != null)
			{
				manager.frames.Remove(dialog.Nest);
				dialog = null;
			}
		}

		private void OnNeedClose()
		{
			Close();
		}

		private void AddBottomNest(ADialog dialog)
		{
			Nest nest = manager.frames.AddParentNode();
			nest.AFrame = dialog;
			nest.hDivided = false;
			nest.left = false;
			nest.isPercents = false;
			nest.size = dialog.Height;
			nest.AFrame.UpdateSettings(manager.settings, UpdatePhase.Raw);
			nest.AFrame.UpdateSettings(manager.settings, UpdatePhase.Parsed);
			dialog.Focus();
		}

		private void RemoveDialog(ADialog dialog)
		{
			dialog.DoBeforeClose();
			manager.frames.Remove(dialog.Nest);
		}
	}

	private MainForm mainForm;
	private FrameList frames;
	private KeyMap keyMap;
	private KeyMap doNothingKeyMap;
	private Settings settings;

	private DialogOwner<InfoDialog> info;
	private DialogOwner<CommandDialog> command;
	private DialogOwner<FindDialog> find;
	private FindDialog.Data findDialogData = new FindDialog.Data();
	private DialogOwner<FindDialog> findInFiles;
	private FindDialog.Data findInFilesDialogData = new FindDialog.Data();
	private DialogOwner<ReplaceDialog> replace;
	private ReplaceDialog.Data replaceDialogData = new ReplaceDialog.Data();

	public DialogManager(MainForm mainForm, Settings settings, KeyMap keyMap, KeyMap doNothingKeyMap)
	{
		this.mainForm = mainForm;
		this.settings = settings;
		this.keyMap = keyMap;
		this.doNothingKeyMap = doNothingKeyMap;
		frames = mainForm.frames;

		keyMap.AddItem(new KeyItem(Keys.Alt | Keys.X, null, new KeyAction("&View\\Open/close command dialog", DoInputCommand, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null, new KeyAction("F&ind\\Find...", DoFind, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.F, null, new KeyAction("F&ind\\Find in Files...", DoFindInFiles, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.H, null, new KeyAction("F&ind\\Replace...", DoReplace, null, false)));

		info = new DialogOwner<InfoDialog>(this);
		command = new DialogOwner<CommandDialog>(this);
		find = new DialogOwner<FindDialog>(this);
		findInFiles = new DialogOwner<FindDialog>(this);
		replace = new DialogOwner<ReplaceDialog>(this);
	}

	public void ShowInfo(string name, string text)
	{
		if (info.Dialog == null)
			info.Open(new InfoDialog("Command", keyMap, doNothingKeyMap));
		info.Dialog.Name = name;
		info.Dialog.InitText(text);
	}

	public void HideInfo()
	{
		info.Close();
	}

	private bool DoInputCommand(Controller controller)
	{
		if (command.Dialog == null)
		{
			HideInfo();
			command.Open(new CommandDialog("Command", keyMap, doNothingKeyMap));
		}
		else
		{
			command.Close();
		}
		return true;
	}

	private bool DoFind(Controller controller)
	{
		if (find.Dialog == null)
		{
			HideInfo();
			find.Open(new FindDialog(findDialogData, DoFindText, "Find", keyMap, doNothingKeyMap));
		}
		else
		{
			find.Close();
		}
		return true;
	}

	private bool DoFindInFiles(Controller controller)
	{
		if (findInFiles.Dialog == null)
		{
			HideInfo();
			findInFiles.Open(new FindDialog(findInFilesDialogData, DoFindInFilesDialog, "Find in Files", keyMap, doNothingKeyMap));
		}
		else
		{
			findInFiles.Close();
		}
		return true;
	}

	private bool DoReplace(Controller controller)
	{
		if (replace.Dialog == null)
		{
			HideInfo();
			replace.Open(new ReplaceDialog(replaceDialogData, "Replace", keyMap, doNothingKeyMap));
		}
		else
		{
			replace.Close();
		}
		return true;
	}

	private bool DoFindText(string text)
	{
		if (mainForm.LastFrame != null)
		{
			Controller lastController = mainForm.LastFrame.Controller;
			int index = lastController.Lines.IndexOf(text, lastController.Lines.LastSelection.Right);
			if (index == -1)
				index = lastController.Lines.IndexOf(text, 0);
			if (index != -1)
			{
				lastController.PutCursor(lastController.Lines.PlaceOf(index), false);
				lastController.PutCursor(lastController.Lines.PlaceOf(index + text.Length), true);
				mainForm.LastFrame.TextBox.MoveToCaret();
			}
		}
		return true;
	}

	private bool DoFindInFilesDialog(string text)
	{
		findInFiles.Close();
		Buffer buffer = new Buffer(null, "Find results");
		buffer.Controller.isReadonly = true;
		mainForm.ShowBuffer(mainForm.ConsoleNest, buffer);
		if (mainForm.ConsoleNest.Frame != null)
			mainForm.ConsoleNest.Frame.Focus();
		return true;
	}
}
