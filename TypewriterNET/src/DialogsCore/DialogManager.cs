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
	private MainForm mainForm;
	private FrameList frames;
	private KeyMap keyMap;
	private KeyMap doNothingKeyMap;
	private Settings settings;

	public DialogManager(MainForm mainForm, Settings settings, KeyMap keyMap, KeyMap doNothingKeyMap)
	{
		this.mainForm = mainForm;
		this.settings = settings;
		this.keyMap = keyMap;
		this.doNothingKeyMap = doNothingKeyMap;
		frames = mainForm.frames;
		keyMap.AddItem(new KeyItem(Keys.Alt | Keys.X, null, new KeyAction("&View\\Open/close command dialog", DoInputCommand, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null, new KeyAction("F&ind\\Find...", DoFind, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.H, null, new KeyAction("F&ind\\Replace...", DoReplace, null, false)));
	}

	private InfoDialog infoDialog;

	public void ShowInfo(string name, string text)
	{
		if (infoDialog == null)
		{
			infoDialog = new InfoDialog("Command", keyMap, doNothingKeyMap);
			AddBottomNest(infoDialog);
			infoDialog.NeedClose += OnInfoNeedClose;
		}
		infoDialog.Name = name;
		infoDialog.InitText(text);
	}

	public void HideInfo()
	{
		OnInfoNeedClose();
	}

	private void OnInfoNeedClose()
	{
		if (infoDialog != null)
		{
			frames.Remove(infoDialog.Nest);
			infoDialog = null;
		}
	}

	private void AddBottomNest(ADialog dialog)
	{
		Nest nest = frames.AddParentNode();
		nest.AFrame = dialog;
		nest.hDivided = false;
		nest.left = false;
		nest.isPercents = false;
		nest.size = dialog.Height;
		nest.AFrame.UpdateSettings(settings, UpdatePhase.Raw);
		nest.AFrame.UpdateSettings(settings, UpdatePhase.Parsed);
		dialog.Focus();
	}

	private CommandDialog commandDialog;

	private bool DoInputCommand(Controller controller)
	{
		if (commandDialog == null)
		{
			HideInfo();
			commandDialog = new CommandDialog("Command", keyMap, doNothingKeyMap);
			AddBottomNest(commandDialog);
			commandDialog.NeedClose += OnCommandNeedClose;
		}
		else
		{
			OnCommandNeedClose();
		}
		return true;
	}

	private void RemoveDialog(ADialog dialog)
	{
		dialog.DoBeforeClose();
		frames.Remove(dialog.Nest);
	}

	private void OnCommandNeedClose()
	{
		RemoveDialog(commandDialog);
		commandDialog = null;
	}

	private FindDialog findDialog;
	private FindDialog.Data findDialogData = new FindDialog.Data();

	private bool DoFind(Controller controller)
	{
		if (findDialog == null)
		{
			HideInfo();
			findDialog = new FindDialog(findDialogData, "Find", keyMap, doNothingKeyMap);
			AddBottomNest(findDialog);
			findDialog.NeedClose += OnFindNeedClose;
		}
		else
		{
			RemoveDialog(findDialog);
			findDialog = null;
		}
		return true;
	}

	private void OnFindNeedClose()
	{
		RemoveDialog(findDialog);
		findDialog = null;
	}

	private ReplaceDialog replaceDialog;

	private bool DoReplace(Controller controller)
	{
		if (replaceDialog == null)
		{
			HideInfo();
			replaceDialog = new ReplaceDialog("Replace", keyMap, doNothingKeyMap);
			AddBottomNest(replaceDialog);
			replaceDialog.NeedClose += OnReplaceClose;
		}
		else
		{
			OnReplaceClose();
		}
		return true;
	}

	private void OnReplaceClose()
	{
		RemoveDialog(replaceDialog);
		replaceDialog = null;
	}
}
