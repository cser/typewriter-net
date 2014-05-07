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
using TypewriterNET;
using TypewriterNET.Frames;

public class DialogManager
{
	private MainForm mainForm;
	private FrameList frames;
	private KeyMap keyMap;
	private KeyMap doNothingKeyMap;

	public DialogManager(MainForm mainForm, KeyMap keyMap, KeyMap doNothingKeyMap)
	{
		this.mainForm = mainForm;
		this.keyMap = keyMap;
		this.doNothingKeyMap = doNothingKeyMap;
		frames = mainForm.frames;
		keyMap.AddItem(new KeyItem(Keys.Alt | Keys.X, null, new KeyAction("&View\\Open/close command dialog", DoInputCommand, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null, new KeyAction("F&ind\\Find...", DoFind, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.H, null, new KeyAction("F&ind\\Replace...", DoReplace, null, false)));
	}

	private void AddBottomNest(ADialog dialog)
	{
		Nest nest = frames.AddParentNode();
		nest.AFrame = dialog;
		nest.hDivided = false;
		nest.left = false;
		nest.isPercents = false;
		nest.size = dialog.Height;
		dialog.Focus();
	}

	private CommandDialog commandDialog;

	public bool DoInputCommand(Controller controller)
	{
		if (commandDialog == null)
		{
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

	private void OnCommandNeedClose()
	{
		frames.Remove(commandDialog.Nest);
		commandDialog = null;
	}

	private FindDialog findDialog;

	private bool DoFind(Controller controller)
	{
		if (findDialog == null)
		{
			findDialog = new FindDialog("Find", keyMap, doNothingKeyMap);
			AddBottomNest(findDialog);
			findDialog.NeedClose += OnFindNeedClose;
		}
		else
		{
			frames.Remove(findDialog.Nest);
			findDialog = null;
		}
		return true;
	}

	private void OnFindNeedClose()
	{
		frames.Remove(findDialog.Nest);
		findDialog = null;
	}

	private ReplaceDialog replaceDialog;

	private bool DoReplace(Controller controller)
	{
		if (replaceDialog == null)
		{
			replaceDialog = new ReplaceDialog("Replace", keyMap, doNothingKeyMap);
			AddBottomNest(findDialog);
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
		frames.Remove(replaceDialog.Nest);
		replaceDialog = null;
	}
}
