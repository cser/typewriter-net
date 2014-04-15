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

	public MainForm(string[] args)
	{
		this.args = args;

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

	private Dictionary<string, Frame> frames = new Dictionary<string, Frame>();

	private void AddBuffer(string frameName, Buffer buffer)
	{
		Frame frame;
		frames.TryGetValue(frameName, out frame);
		if (frame == null)
		{
			frame = new Frame("", keyMap, doNothingKeyMap);
			frames[frameName] = frame;
			AddFrame(frame, false, true, true, 50);
		}
		frame.AddBuffer(buffer);
	}

	private void OnLoad(object sender, EventArgs e)
	{
		BuildMenu();
		{
			FindDialog dialog = new FindDialog("Find", keyMap, doNothingKeyMap);
			nests = new Nest(dialog, nests);
			nests.hDivided = false;
			nests.left = false;
			nests.isPercents = false;
			nests.size = dialog.Height;
			nests.Init(this);
			Controls.Add(dialog);
		}
		{
			ReplaceDialog dialog = new ReplaceDialog("Replace", keyMap, doNothingKeyMap);
			nests = new Nest(dialog, nests);
			nests.hDivided = false;
			nests.left = false;
			nests.isPercents = false;
			nests.size = dialog.Height;
			nests.Init(this);
			Controls.Add(dialog);
		}
		AddBuffer("main", new Buffer("aaaa", "aaaa"));
		AddBuffer("main", new Buffer("bbbb", "bbbb"));
		AddBuffer("left", new Buffer("bbbb", "bbbb"));
		ValidateSettings(true);
		MenuNode = new KeyMapNode(keyMap, 0);
	}

	public KeyMapNode MenuNode
	{
		get { return menu.node; }
		set { menu.node = value; }
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

	private Nest nests;
	public Nest Nests { get { return nests; } }

	private void AddFrame(Frame frame, bool hDivided, bool left, bool isPercents, int percents)
	{
		nests = new Nest(frame, nests);
		nests.hDivided = hDivided;
		nests.left = left;
		nests.isPercents = isPercents;
		nests.size = percents;
		nests.Init(this);
		Controls.Add(frame);
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		Size size = ClientSize;
		if (nests != null)
		{
			nests.Update();
			nests.Resize(0, 0, size.Width, size.Height);
		}
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
		
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.W, null, new KeyAction("&View\\Close tab", DoCloseTab, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Oemtilde, null, new KeyAction("&View\\Show/hide editor console", DoShowHideConsole, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null, new KeyAction("&View\\Change focus", DoChangeFocus, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null, new KeyAction("F&ind\\Find...", DoFind, null, false)));
		
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
		return true;
	}

	private bool DoOpen(Controller controller)
	{
		return true;
	}

	private bool DoSave(Controller controller)
	{
		TrySaveFile(Nest.GetSelectedBuffer(nests));
		return true;
	}

	private bool DoSaveAs(Controller controller)
	{
		Buffer buffer = Nest.GetSelectedBuffer(nests);
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

	private bool DoCloseTab(Controller controller)
	{
		return true;
	}

	private bool DoShowHideConsole(Controller controller)
	{
		return true;
	}

	private bool DoChangeFocus(Controller controller)
	{
		Frame selectedFrame = Nest.GetFocusedFrame(nests);
		Frame frame = Nest.GetNextFrame(selectedFrame);
		if (frame == null)
			frame = Nest.GetFirstFrame(nests);
		if (frame != null)
			frame.Focus();
		return true;
	}

	private bool DoFind(Controller controller)
	{
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
}
