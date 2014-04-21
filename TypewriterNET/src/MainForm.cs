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

public class MainForm : Form
{
	private readonly string[] args;
	private readonly Timer settingsTimer;
	private readonly Settings settings;
	private readonly MainFormMenu menu;

	public readonly FrameList frames;

	public MainForm(string[] args)
	{
		this.args = args;

		frames = new FrameList(this);

		ResourceManager manager = new ResourceManager("TypewriterNET", typeof(Program).Assembly);
		Icon = (Icon)manager.GetObject("icon");
		Name = Application.ProductName;
		Text = Name;

		menu = new MainFormMenu(this);
		Menu = menu;

		settingsTimer = new Timer();
		settingsTimer.Interval = 10;
		settingsTimer.Tick += OnSettingsTimer;

		settings = new Settings();
		settings.Changed += OnSettingsChanged;

		Load += OnLoad;
	}

	private void OnSettingsChanged()
	{
		settingsTimer.Start();
	}

	private void OnSettingsTimer(object sender, EventArgs e)
	{
		settingsTimer.Stop();
		ValidateSettings(false);
	}

	private Nest mainNest;
	private Nest consoleNest;
	private Nest leftNest;

	private void OnLoad(object sender, EventArgs e)
	{
		BuildMenu();

		mainNest = AddNest(false, true, true, 70);
		consoleNest = AddNest(false, false, true, 20);
		leftNest = AddNest(true, true, true, 20);

		mainNest.AFrame = new Frame("", keyMap, doNothingKeyMap);
		consoleNest.AFrame = new Frame("", keyMap, doNothingKeyMap);
		leftNest.AFrame = new Frame("", keyMap, doNothingKeyMap);

		mainNest.Frame.AddBuffer(NewFileBuffer());

		ValidateSettings(true);
		SetFocus(null, new KeyMapNode(keyMap, 0));
	}

	public KeyMapNode MenuNode { get { return menu.node; } }

	private MulticaretTextBox focusedTextBox;
	public Controller FocusedController { get { return focusedTextBox != null ? focusedTextBox.Controller : null; } }

	public void SetFocus(MulticaretTextBox textBox, KeyMapNode node)
	{
		focusedTextBox = textBox;
		menu.node = node;
	}

	private void ValidateSettings(bool forced)
	{
		bool needResize = forced;
		if (needResize)
			OnResize(null);
	}

	public void DoResize()
	{
		OnResize(null);
	}

	private Nest AddNest(bool hDivided, bool left, bool isPercents, int percents)
	{
		Nest nest = frames.AddParentNode();
		nest.hDivided = hDivided;
		nest.left = left;
		nest.isPercents = isPercents;
		nest.size = percents;
		return nest;
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		frames.Resize(0, 0, ClientSize);
	}

	private KeyMap keyMap;
	private KeyMap doNothingKeyMap;

	private void BuildMenu()
	{
		keyMap = new KeyMap();
		doNothingKeyMap = new KeyMap();
		
		doNothingKeyMap.AddItem(new KeyItem(Keys.Escape, null, KeyAction.Nothing));
		doNothingKeyMap.AddItem(new KeyItem(Keys.Escape | Keys.Shift, null, KeyAction.Nothing));
		
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, new KeyAction("&File\\New", DoNew, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.O, null, new KeyAction("&File\\Open", DoOpen, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.S, null, new KeyAction("&File\\Save", DoSave, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.S, null, new KeyAction("&File\\Save As", DoSaveAs, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("&File\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Alt | Keys.F4, null, new KeyAction("&File\\Exit", DoExit, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Oemtilde, null, new KeyAction("&View\\Show/hide editor console", DoShowHideConsole, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null, new KeyAction("&View\\Change focus", DoChangeFocus, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null, new KeyAction("F&ind\\Find...", DoFind, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.H, null, new KeyAction("F&ind\\Replace...", DoReplace, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.F2, null, new KeyAction("Prefere&nces\\Edit config", DoOpenUserConfig, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F2, null, new KeyAction("Prefere&nces\\Open base config", DoOpenBaseConfig, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F2, null, new KeyAction("Prefere&nces\\Edit current scheme", DoOpenCurrentScheme, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.F2, null, new KeyAction("Prefere&nces\\Open current scheme all files", DoOpenCurrentScheme, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F3, null, new KeyAction("Prefere&nces\\Open AppDdata folder", DoOpenAppDataFolder, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("Prefere&nces\\New syntax file", DoNewSyntax, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.F1, null, new KeyAction("&?\\About", DoAbout, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Close editor console", DoCloseEditorConsole, null, false)));
	}

	private bool DoNew(Controller controller)
	{
		mainNest.Frame.AddBuffer(NewFileBuffer());
		return true;
	}

	private bool DoOpen(Controller controller)
	{
		OpenFileDialog dialog = new OpenFileDialog();
		if (dialog.ShowDialog() == DialogResult.OK)
			LoadFile(dialog.FileName);
		return true;
	}

	public void LoadFile(string file)
	{
		string fullPath = Path.GetFullPath(file);
		Buffer buffer = NewFileBuffer();
		mainNest.Frame.AddBuffer(buffer);
		buffer.SetFile(fullPath, Path.GetFileName(file));

		if (!File.Exists(buffer.FullPath))
		{
			//EditorConsole.Instance.Write("Missing file: ", Ds.Keyword);
			//EditorConsole.Instance.WriteLine(buffer.FullPath, Ds.Normal);
			//ShowEditorConsole();
			return;
		}
		string text = "";
		try
		{
			text = File.ReadAllText(buffer.FullPath);
		}
		catch (IOException/* e*/)
		{
			//EditorConsole.Instance.WriteLine("-- File loading errors:", Ds.Comment);
			//EditorConsole.Instance.WriteLine(e.Message + "\n" + e.StackTrace);
			//ShowEditorConsole();
		}
		buffer.Controller.InitText(text);
		//int caret = fileQualitiesStorage.Get(buffer.FullPath)["cursor"].Int;
		//buffer.Controller.PutCursor(buffer.Controller.SoftNormalizedPlaceOf(caret), false);
		buffer.Controller.NeedScrollToCaret();
		buffer.fileInfo = new FileInfo(buffer.FullPath);
		buffer.lastWriteTimeUtc = buffer.fileInfo.LastWriteTimeUtc;
	}
	
	private bool DoSave(Controller controller)
	{
		TrySaveFile(frames.GetSelectedBuffer(BufferTag.File));
		return true;
	}

	private bool DoSaveAs(Controller controller)
	{
		Buffer buffer = frames.GetSelectedBuffer(BufferTag.File);
		if (buffer != null)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.FileName = buffer.Name;
			dialog.InitialDirectory = Path.GetDirectoryName(buffer.FullPath);
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				buffer.SetFile(Path.GetFullPath(dialog.FileName), Path.GetFileName(dialog.FileName));
				SaveFile(buffer);
			}
		}
		return true;
	}

	private void TrySaveFile(Buffer buffer)
	{
		if (buffer == null)
			return;
		if (!string.IsNullOrEmpty(buffer.FullPath) && !buffer.needSaveAs)
		{
			SaveFile(buffer);
			return;
		}
		SaveFileDialog dialog = new SaveFileDialog();
		dialog.FileName = buffer.Name;
		if (!string.IsNullOrEmpty(buffer.FullPath))
			dialog.InitialDirectory = Path.GetDirectoryName(buffer.FullPath);
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			buffer.SetFile(Path.GetFullPath(dialog.FileName), Path.GetFileName(dialog.FileName));
			SaveFile(buffer);
		}
	}

	public void SaveFile(Buffer buffer)
	{
		try
		{
			File.WriteAllText(buffer.FullPath, buffer.Controller.Lines.GetText());
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		buffer.Controller.history.MarkAsSaved();
		buffer.fileInfo = new FileInfo(buffer.FullPath);
		buffer.lastWriteTimeUtc = buffer.fileInfo.LastWriteTimeUtc;
		buffer.needSaveAs = false;
		//tabBar.Invalidate();
		
		/*if (AppPath.ConfigPath.HasPath(buffer.FullPath))
		{
			ReloadConfig();
		}
		else if (GetActiveSchemePaths(config).IndexOf(buffer.FullPath) != -1)
		{
			ReloadScheme();
		}
		
		UpdateHighlighter(buffer);*/
	}

	private bool DoExit(Controller controller)
	{
		return true;
	}

	private bool DoShowHideConsole(Controller controller)
	{
		return true;
	}

	private bool DoChangeFocus(Controller controller)
	{
		Frame frame = frames.GetChildFrame(frames.GetFocusedFrame());
		if (frame == null)
			frame = frames.GetFirstFrame();
		if (frame != null)
			frame.Focus();
		return true;
	}

	private FindDialog findDialog;
	private ReplaceDialog replaceDialog;

	private bool DoFind(Controller controller)
	{
		if (findDialog == null)
		{
			findDialog = new FindDialog("Find", keyMap, doNothingKeyMap);
			Nest nest = frames.AddParentNode();
			nest.AFrame = findDialog;
			nest.hDivided = false;
			nest.left = false;
			nest.isPercents = false;
			nest.size = findDialog.Height;
			findDialog.Focus();
		}
		else
		{
			frames.Remove(findDialog.Nest);
			findDialog = null;
		}
		OnResize(null);
		return true;
	}

	private bool DoReplace(Controller controller)
	{
		if (replaceDialog == null)
		{
			replaceDialog = new ReplaceDialog("Replace", keyMap, doNothingKeyMap);
			Nest nest = frames.AddParentNode();
			nest.AFrame = replaceDialog;
			nest.hDivided = false;
			nest.left = false;
			nest.isPercents = false;
			nest.size = replaceDialog.Height;
		}
		else
		{
			frames.Remove(replaceDialog.Nest);
			replaceDialog = null;
		}
		OnResize(null);
		return true;
	}

	private bool DoOpenUserConfig(Controller controller)
	{
		return true;
	}

	private bool DoOpenBaseConfig(Controller controller)
	{
		return true;
	}

	private bool DoOpenCurrentScheme(Controller controller)
	{
		return true;
	}

	private bool DoOpenAppDataFolder(Controller controller)
	{
		return true;
	}

	private bool DoNewSyntax(Controller controller)
	{
		return true;
	}

	private bool DoAbout(Controller controller)
	{
		return true;
	}

	private bool DoCloseEditorConsole(Controller controller)
	{
		return true;
	}

	private Buffer NewFileBuffer()
	{
		Buffer buffer = new Buffer(null, "Untitled.txt");
		buffer.tags = BufferTag.File;
		buffer.needSaveAs = true;
		buffer.onRemove = OnFileBufferRemove;
		return buffer;
	}

	private bool OnFileBufferRemove(Buffer buffer)
	{
		if (buffer != null)
		{
			if (buffer.Changed)
			{
				DialogResult result = MessageBox.Show("Do you want to save the current changes in\n" + buffer.Name + "?", Name, MessageBoxButtons.YesNoCancel);
				switch (result)
				{
					case DialogResult.Yes:
						TrySaveFile(buffer);
						return true;
					case DialogResult.No:
						return true;
					case DialogResult.Cancel:
						return false;
				}
			}
			else
			{
				return true;
			}
		}
		return false;
	}
}
