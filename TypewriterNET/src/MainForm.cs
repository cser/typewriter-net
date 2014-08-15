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

public class MainForm : Form
{
	private const string UntitledTxt = "Untitled.txt";
	private readonly string[] args;
	
	private readonly Settings settings;
	public Settings Settings { get { return settings; } }

	private readonly ConfigParser configParser;

	private readonly MainFormMenu menu;
	private readonly Timer validationTimer;

	public readonly FrameList frames;
	public readonly Commander commander;

	private ConcreteHighlighterSet highlightingSet;
	private SyntaxFilesScanner syntaxFilesScanner;

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

		settings = new Settings(ApplySettings);
		configParser = new ConfigParser(settings);
		commander = new Commander();

		Load += OnLoad;

		validationTimer = new Timer();
		validationTimer.Interval = 20;
		validationTimer.Tick += OnValidationTimerTick;
		validationTimer.Start();
	}

	private void OnValidationTimerTick(object sender, EventArgs e)
	{
		if (frames.NeedResize)
		{
			frames.NeedResize = false;
			DoResize();
		}
	}

	private Nest mainNest;
	public Nest MainNest { get { return mainNest; } }

	private Nest consoleNest;
	public Nest ConsoleNest { get { return consoleNest; } }

	private Nest leftNest;
	public Nest LeftNest { get { return leftNest; } }

	private FileDragger fileDragger;
	private TempSettings tempSettings;
	private SchemeManager schemeManager;

	private XmlLoader xmlLoader;
	public XmlLoader XmlLoader { get { return xmlLoader; } }
	
	private DialogManager dialogs;
	public DialogManager Dialogs { get { return dialogs; } }

	private Log log;
	public Log Log { get { return log; } }

	private FileTreeProcessor fileTree;

	private void OnLoad(object sender, EventArgs e)
	{
		string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypewriterNET");
		if (!Directory.Exists(appDataPath))
			Directory.CreateDirectory(appDataPath);
		AppPath.Init(Application.StartupPath, appDataPath);

		BuildMenu();

		commander.Init(this, settings);

		mainNest = AddNest(false, true, true, 70);
		mainNest.buffers = new BufferList();
		new Frame().Create(mainNest);

		consoleNest = AddNest(false, false, true, 20);
		consoleNest.buffers = new BufferList();
		leftNest = AddNest(true, true, true, 20);

		log = new Log(this, consoleNest);
		xmlLoader = new XmlLoader(this);

		schemeManager = new SchemeManager(xmlLoader);
		syntaxFilesScanner = new SyntaxFilesScanner(new string[] {
			Path.Combine(AppPath.AppDataDir, AppPath.Syntax),
			Path.Combine(AppPath.StartupDir, AppPath.Syntax) });
		highlightingSet = new ConcreteHighlighterSet(xmlLoader, log);

		syntaxFilesScanner.Rescan();
		highlightingSet.UpdateParameters(syntaxFilesScanner);
		frames.UpdateSettings(settings, UpdatePhase.HighlighterChange);

		fileTree = new FileTreeProcessor(this);
		fileTree.Reload();

		leftNest.buffers = new BufferList();

		SetFocus(null, new KeyMapNode(keyMap, 0), null);

		ApplySettings();
		ReloadConfig();
		fileDragger = new FileDragger(this);

		tempSettings = new TempSettings(this, settings);
		tempSettings.Load();

		if (args.Length == 1)
			LoadFile(args[0]);
		FormClosing += OnFormClosing;
		mainNest.buffers.AllRemoved += OpenEmptyIfNeed;
		OpenEmptyIfNeed();

		Activated += OnActivated;
	}

	private bool activationInProcess = false;
	    
	private void OnActivated(object sender, EventArgs e)
	{
		if (activationInProcess)
			return;
		activationInProcess = true;
		
		foreach (Buffer buffer in frames.GetBuffers(BufferTag.File))
		{
			if (buffer.fileInfo != null)
			{
				buffer.fileInfo.Refresh();
				if (buffer.lastWriteTimeUtc != buffer.fileInfo.LastWriteTimeUtc)
				{
					DialogResult result = MessageBox.Show("File was changed. Reload it?", Name, MessageBoxButtons.YesNo);
					if (result == DialogResult.Yes)
						ReloadFile(buffer);
				}
			}
		}
		
		activationInProcess = false;
	}

	private void OpenEmptyIfNeed()
	{
		if (mainNest.Frame == null)
			new Frame().Create(mainNest);
		if (mainNest.buffers.list.Count == 0)
		{
			Buffer buffer = NewFileBuffer();
			mainNest.Frame.AddBuffer(buffer);
		}
	}

	private void RemoveEmptyIfNeed()
	{
		Buffer buffer = null;
		for (int i = mainNest.buffers.list.Count; i-- > 0;)
		{
			Buffer bufferI = mainNest.buffers.list[i];
			if ((bufferI.tags & BufferTag.File) != 0 && bufferI.IsEmpty && !bufferI.HasHistory &&
				bufferI.Name == UntitledTxt)
			{
				buffer = bufferI;
				break;
			}
		}
		if (buffer != null)
			mainNest.buffers.list.Remove(buffer);
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		foreach (Buffer buffer in frames.GetBuffers(BufferTag.File))
		{
			if (buffer.onRemove != null && !buffer.onRemove(buffer))
			{
				e.Cancel = true;
				break;
			}
		}
		tempSettings.Save();
	}

	public KeyMapNode MenuNode { get { return menu.node; } }

	private MulticaretTextBox focusedTextBox;
	public Controller FocusedController { get { return focusedTextBox != null ? focusedTextBox.Controller : null; } }

	private Frame lastFrame;
	public Frame LastFrame { get { return lastFrame; } }

	public void SetFocus(MulticaretTextBox textBox, KeyMapNode node, Frame frame)
	{
		focusedTextBox = textBox;
		menu.node = node;
		if (frame != null)
			lastFrame = frame;
	}

	private void ApplySettings()
	{
		settings.ParsedScheme = schemeManager.LoadScheme(settings.scheme.Value);
		settings.Parsed = true;
		BackColor = settings.ParsedScheme.bgColor;
		frames.UpdateSettings(settings, UpdatePhase.Raw);
		frames.UpdateSettings(settings, UpdatePhase.Parsed);
	}

	public void DoResize()
	{
		frames.Resize(0, 0, ClientSize);
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
		DoResize();
	}

	private KeyMap keyMap;
	public KeyMap KeyMap { get { return keyMap; } }

	private KeyMap doNothingKeyMap;
	public KeyMap DoNothingKeyMap { get { return doNothingKeyMap; } }

	private void BuildMenu()
	{
		keyMap = new KeyMap();
		doNothingKeyMap = new KeyMap();
		dialogs = new DialogManager(this);
		
		doNothingKeyMap.AddItem(new KeyItem(Keys.Escape, null, KeyAction.Nothing));
		doNothingKeyMap.AddItem(new KeyItem(Keys.Escape | Keys.Shift, null, KeyAction.Nothing));
		
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, new KeyAction("&File\\New", DoNew, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.O, null, new KeyAction("&File\\Open", DoOpen, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.S, null, new KeyAction("&File\\Save", DoSave, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.S, null, new KeyAction("&File\\Save As", DoSaveAs, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("&File\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Alt | Keys.F4, null, new KeyAction("&File\\Exit", DoExit, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Oemtilde, null, new KeyAction("&View\\Open/close log", DoOpenCloseLog, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.G, null, new KeyAction("&View\\Open/close console panel", DoOpenCloseConsolePanel, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null, new KeyAction("&View\\Change focus", DoChangeFocus, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.I, null, new KeyAction("&View\\Open/close file tree", DoOpenCloseFileTree, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.F2, null, new KeyAction("Prefere&nces\\Edit config", DoOpenUserConfig, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F2, null, new KeyAction("Prefere&nces\\Open base config", DoOpenBaseConfig, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F2, null, new KeyAction("Prefere&nces\\Edit current scheme", DoEditCurrentScheme, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F3, null, new KeyAction("Prefere&nces\\Open AppDdata folder", DoOpenAppDataFolder, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("Prefere&nces\\New syntax file", DoNewSyntax, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.F1, null, new KeyAction("&?\\Help", DoHelp, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.Escape, null, new KeyAction("&View\\Close editor console", DoCloseEditorConsole, null, false)));
	}

	private bool DoNew(Controller controller)
	{
		RemoveEmptyIfNeed();
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

	public Buffer LoadFile(string file)
	{
		string fullPath = Path.GetFullPath(file);
		string name = Path.GetFileName(file);
		Buffer buffer = mainNest.buffers.GetBuffer(fullPath, name);
		bool needLoad = false;
		if (buffer == null)
		{
			buffer = NewFileBuffer();
			needLoad = true;
		}
		ShowBuffer(mainNest, buffer);
		buffer.SetFile(fullPath, name);
		if (buffer.Frame != null)
			buffer.Frame.UpdateHighlighter();

		if (needLoad && !ReloadFile(buffer))
			return null;
		RemoveEmptyIfNeed();
		return buffer;
	}

	public Buffer ForcedLoadFile(string file)
	{
		string fullPath = Path.GetFullPath(file);
		string name = Path.GetFileName(file);
		Buffer buffer = mainNest.buffers.GetBuffer(fullPath, name);
		bool needLoad = false;
		if (buffer == null)
		{
			buffer = NewFileBuffer();
			needLoad = File.Exists(fullPath);
		}
		ShowBuffer(mainNest, buffer);
		buffer.SetFile(fullPath, name);
		if (buffer.Frame != null)
			buffer.Frame.UpdateHighlighter();

		if (needLoad && !ReloadFile(buffer))
			return null;
		RemoveEmptyIfNeed();
		return buffer;
	}

	private bool ReloadFile(Buffer buffer)
	{
		if (!File.Exists(buffer.FullPath))
		{
			Log.Write("Missing file: ", Ds.Keyword);
			Log.WriteLine(buffer.FullPath, Ds.Normal);
			Log.Open();
			return false;
		}
		string text = "";
		try
		{
			text = File.ReadAllText(buffer.FullPath);
		}
		catch (IOException e)
		{
			Log.WriteLine("-- File loading errors:", Ds.Comment);
			Log.WriteLine(e.Message + "\n" + e.StackTrace);
			Log.Open();
		}
		buffer.Controller.InitText(text);
		buffer.fileInfo = new FileInfo(buffer.FullPath);
		buffer.lastWriteTimeUtc = buffer.fileInfo.LastWriteTimeUtc;
		buffer.needSaveAs = false;
		tempSettings.ApplyQualities(buffer);
		return true;
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
		if (buffer.FullPath == AppPath.ConfigPath)
		{
			ReloadConfig();
		}
		else if (schemeManager.IsActiveSchemePath(settings.scheme.Value, buffer.FullPath))
		{
			ApplySettings();
		}
	}

	private bool DoExit(Controller controller)
	{
		Close();
		return true;
	}

	private bool DoOpenCloseLog(Controller controller)
	{
		if (Log.Opened)
		{
			Log.Close();
		}
		else
		{
			Log.Open();
			Log.Focus();
		}
		return true;
	}

	private bool DoOpenCloseConsolePanel(Controller controller)
	{
		if (consoleNest.AFrame != null)
		{
			if (consoleNest.AFrame.Focused)
				consoleNest.AFrame.Destroy();
			else
				consoleNest.AFrame.Focus();
		}
		else
		{
			new Frame().Create(consoleNest);
			if (consoleNest.buffers.list.Count == 0)
				Log.Open();
			consoleNest.Frame.Focus();
		}
		return true;
	}

	private bool DoOpenCloseFileTree(Controller controller)
	{
		if (leftNest.AFrame != null && leftNest.buffers.list.Selected == fileTree.Buffer)
		{
			leftNest.AFrame.Destroy();
		}
		else
		{
			fileTree.Reload();
			if (leftNest.AFrame == null)
				new Frame().Create(leftNest);
			leftNest.Frame.AddBuffer(fileTree.Buffer);
			leftNest.Frame.Focus();
		}
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

	private bool DoOpenUserConfig(Controller controller)
	{
		LoadFile(AppPath.ConfigPath);
		return true;
	}

	private bool DoOpenBaseConfig(Controller controller)
	{
		return true;
	}

	private bool DoEditCurrentScheme(Controller controller)
	{
		List<AppPath> paths = schemeManager.GetSchemePaths(settings.scheme.Value);
		if (paths.Count > 0)
		{
			foreach (AppPath path in paths)
			{
				if (!File.Exists(path.appDataPath))
					File.Copy(path.startupPath, path.appDataPath);
				LoadFile(path.appDataPath);
			}
		}
		return true;
	}

	private bool DoOpenAppDataFolder(Controller controller)
	{
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		process.StartInfo.FileName = AppPath.AppDataDir;
		process.Start();
		return true;
	}

	private bool DoNewSyntax(Controller controller)
	{
		CreateAppDataFolders();
		string templatePath = Path.Combine(AppPath.TemplatesDir, "syntax.xml");
		string filePath = Path.Combine(AppPath.SyntaxDir.appDataPath, "new-syntax.xml");
		Buffer buffer = ForcedLoadFile(filePath);
		if (!File.Exists(templatePath))
		{
			Log.Write("Missing template: ", Ds.Error);
			Log.WriteLine(templatePath);
			Log.Open();
			return false;
		}
		buffer.InitText(File.ReadAllText(templatePath));
		return true;
	}

	private void CreateAppDataFolders()
	{
		if (!Directory.Exists(AppPath.SyntaxDir.appDataPath))
			Directory.CreateDirectory(AppPath.SyntaxDir.appDataPath);
		if (!Directory.Exists(AppPath.SchemesDir.appDataPath))
			Directory.CreateDirectory(AppPath.SchemesDir.appDataPath);
	}

	private bool DoHelp(Controller controller)
	{
		ProcessHelp();
		return true;
	}

	private Buffer _helpBuffer;

	public void ProcessHelp()
	{
		if (_helpBuffer == null || _helpBuffer.Frame == null)
		{
			string text = "# About\n" +
				"\n" +
				Application.ProductName + "\n" +
				"Build " + Application.ProductVersion + "\n" +
				"\n" +
				commander.GetHelpText() + "\n" +
				settings.GetHelpText();
			_helpBuffer = new Buffer(null, "Help.twh");
			_helpBuffer.tags = BufferTag.Other;
			_helpBuffer.onRemove = OnHelpBufferRemove;
			_helpBuffer.Controller.isReadonly = true;
			_helpBuffer.Controller.InitText(text);
			ShowBuffer(mainNest, _helpBuffer);
		}
		else
		{
			_helpBuffer.Frame.RemoveBuffer(_helpBuffer);
			_helpBuffer = null;
		}
	}

	private bool OnHelpBufferRemove(Buffer buffer)
	{
		_helpBuffer = null;
		return true;
	}

	private bool DoCloseEditorConsole(Controller controller)
	{
		if (Log.Opened)
		{
			Log.Close();
			return true;
		}
		return false;
	}

	private Buffer NewFileBuffer()
	{
		Buffer buffer = new Buffer(null, UntitledTxt);
		buffer.tags = BufferTag.File;
		buffer.needSaveAs = true;
		buffer.onRemove = OnFileBufferRemove;
		return buffer;
	}

	public void ShowBuffer(Nest nest, Buffer buffer)
	{
		if (nest.Frame == null)
			new Frame().Create(nest);
		nest.Frame.AddBuffer(buffer);
	}

	private bool OnFileBufferRemove(Buffer buffer)
	{
		if (buffer != null)
		{
			tempSettings.StorageQualities(buffer);
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

	private void ReloadConfig()
	{
		configParser.Reset();
		
		StringBuilder errors = new StringBuilder();
		if (!File.Exists(AppPath.ConfigPath))
		{
			if (!File.Exists(AppPath.ConfigTemplatePath))
			{
				Log.WriteLine("Warning: Missing config", Ds.String);
				Log.Open();
			}
			File.Copy(AppPath.ConfigTemplatePath, AppPath.ConfigPath);
			Log.WriteLine("Config was created: " + AppPath.ConfigPath, Ds.Comment);
		}
		XmlDocument xml = xmlLoader.Load(AppPath.ConfigPath, false);
		if (xml != null)
		{
			StringBuilder builder = new StringBuilder();
			configParser.Parse(xml, builder);
			if (builder.Length > 0)
			{
				Log.WriteLine(builder.ToString());
				Log.Open();
			}
			StringWriter sw = new StringWriter();
			XmlTextWriter writer = new XmlTextWriter(sw);
			xml.WriteTo(writer);
		}
		settings.DispatchChange();
	}

	public void UpdateHighlighter(MulticaretTextBox textBox, string fileName)
	{
		if (fileName == null)
		{
			textBox.Highlighter = null;
			return;
		}
		string syntax = syntaxFilesScanner.GetSyntaxByFile(fileName);
		string extension = fileName.ToLowerInvariant();
		textBox.Highlighter = syntax != null ? highlightingSet.GetHighlighter(syntax) : null;
	}

	public void NavigateTo(string fileName, int position0, int position1)
	{
		Buffer buffer = LoadFile(fileName);
		if (buffer != null)
		{
			buffer.Controller.PutCursor(buffer.Controller.Lines.PlaceOf(position0), false);
			buffer.Controller.PutCursor(buffer.Controller.Lines.PlaceOf(position1), true);
			if (buffer.Frame != null)
				buffer.Frame.Focus();
		}
	}
}
