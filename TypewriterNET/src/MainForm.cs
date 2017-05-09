using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Resources;
using System.Xml;
using System.Net;
using MulticaretEditor;

public class MainForm : Form
{
	private const string UntitledTxt = "Untitled.txt";

	private readonly string[] args;

	private readonly Settings settings;
	public Settings Settings { get { return settings; } }

	private readonly ConfigParser configParser;

	private readonly MainFormMenu menu;
	private readonly Timer validationTimer;
	private FormWindowState windowState;
	private bool needUpdateBorderStyle;
	private bool ignoreBorderStyleChanging;

	public readonly FrameList frames;
	public readonly Commander commander;

	private ConcreteHighlighterSet highlightingSet;
	public ConcreteHighlighterSet HighlightingSet { get { return highlightingSet; } }
	
	private SyntaxFilesScanner syntaxFilesScanner;
	public SyntaxFilesScanner SyntaxFilesScanner { get { return syntaxFilesScanner; } }
	
	private SnippetFilesScanner snippetFilesScanner;
	public SnippetFilesScanner SnippetFilesScanner { get { return snippetFilesScanner; } }

	public MainForm(string[] args)
	{
		this.args = args;

		windowState = WindowState;
		MulticaretTextBox.initMacrosExecutor = new MacrosExecutor(GetFocusedTextBox);

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

    public const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, Int32 nCmdShow);

    public static void RestoreWindow(IntPtr handle)
    {
        if (IsIconic(handle))
            ShowWindow(handle, SW_RESTORE);
    }

	public void UpdateTitle()
	{
		Buffer buffer = LastBuffer;
		string name = buffer != null ? buffer.FullPath : null;
		Text = Application.ProductName + (string.IsNullOrEmpty(name) ? "" : " - " + name);
	}
	
	public void UpdateAfterFileRenamed()
	{
		frames.UpdateSettings(settings, UpdatePhase.FileSaved);
		UpdateTitle();
	}

	public Buffer LastBuffer
	{
		get { return lastFrame != null && lastFrame.Nest != null ? lastFrame.SelectedBuffer : null; }
	}

	private void OnValidationTimerTick(object sender, EventArgs e)
	{
		if (frames.NeedResize)
		{
			frames.NeedResize = false;
			DoResize();
		}
		if (needUpdateBorderStyle)
		{
			needUpdateBorderStyle = false;
			if (settings.fullScreenOnMaximized.Value)
			{
				if (WindowState == FormWindowState.Maximized)
				{
					ignoreBorderStyleChanging = true;
					WindowState = FormWindowState.Normal;
					FormBorderStyle = FormBorderStyle.None;
					WindowState = FormWindowState.Maximized;
					ignoreBorderStyleChanging = false;
				}
				else
				{
					if (FormBorderStyle != FormBorderStyle.Sizable)
					{
						FormBorderStyle = FormBorderStyle.Sizable;
					}
				}
			}
			else
			{
				if (FormBorderStyle != FormBorderStyle.Sizable)
				{
					FormBorderStyle = FormBorderStyle.Sizable;
				}
			}
		}
	}

	private Nest mainNest;
	public Nest MainNest { get { return mainNest; } }

	private Nest mainNest2;
	public Nest MainNest2 { get { return mainNest2; } }

	private Nest consoleNest;
	public Nest ConsoleNest { get { return consoleNest; } }

	private Nest leftNest;
	public Nest LeftNest { get { return leftNest; } }

	private FileDragger fileDragger;
	private TempSettings tempSettings;
	private string tempFilePostfix;
	private SchemeManager schemeManager;

	private XmlLoader xmlLoader;
	public XmlLoader XmlLoader { get { return xmlLoader; } }

	private DialogManager dialogs;
	public DialogManager Dialogs { get { return dialogs; } }

	private Log log;
	public Log Log { get { return log; } }
	
	private Nest GetMainNest()
	{
		return lastFrame != null && lastFrame.Nest == mainNest2 ? mainNest2 : mainNest;
	}

	public void ProcessViShortcut(Controller controller, string shortcut)
	{
		if (dialogs != null)
		{
			dialogs.DoOnViShortcut(controller, shortcut);
		}
	}

	private FileTree fileTree;

	private void OnLoad(object sender, EventArgs e)
	{
		List<FileArg> filesToLoad;
		int lineNumber;
		string configFilePostfix;
		ApplyArgs(args, out filesToLoad, out lineNumber, out tempFilePostfix, out configFilePostfix);

		string appDataPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypewriterNET");
		if (!Directory.Exists(appDataPath))
			Directory.CreateDirectory(appDataPath);
		AppPath.Init(Application.StartupPath, appDataPath, configFilePostfix);

		BuildMenu();

		tempSettings = new TempSettings(this, settings);
		commander.Init(this, settings, tempSettings);
		dialogs = new DialogManager(this, tempSettings);

		mainNest = AddNest(false, true, true, tempSettings.GetInt("mainNest.size", 70));
		mainNest.buffers = new BufferList();
		new Frame().Create(mainNest);

		mainNest2 = AddNest(true, false, true, tempSettings.GetInt("mainNest2.size", 50));
		mainNest2.buffers = new BufferList();

		consoleNest = AddNest(false, false, true, tempSettings.GetInt("consoleNest.size", 20));
		consoleNest.buffers = new BufferList();
		leftNest = AddNest(true, true, false, tempSettings.GetInt("leftNest.size", 120));

		log = new Log(this, consoleNest);
		xmlLoader = new XmlLoader(this);

		schemeManager = new SchemeManager(xmlLoader);
		syntaxFilesScanner = new SyntaxFilesScanner(new string[] {
			Path.Combine(AppPath.AppDataDir, AppPath.Syntax),
			Path.Combine(AppPath.StartupDir, AppPath.Syntax) });
		snippetFilesScanner = new SnippetFilesScanner(new string[] {
			Path.Combine(AppPath.AppDataDir, AppPath.Snippets),
			Path.Combine(AppPath.StartupDir, AppPath.Snippets) });
		highlightingSet = new ConcreteHighlighterSet(xmlLoader, log, this);

		sharpManager = new SharpManager(this);
		syntaxFilesScanner.Rescan();
		highlightingSet.UpdateParameters(syntaxFilesScanner);
		frames.UpdateSettings(settings, UpdatePhase.HighlighterChange);
		
		fileTree = new FileTree(this);

		leftNest.buffers = new BufferList();

		SetFocus(null, new KeyMapNode(keyMap, 0), null);

		//was ApplySettings();
		
		ReloadConfig();
		fileDragger = new FileDragger(this);

		tempSettings.Load(tempFilePostfix);
		InitStartSettings();
		allowApply = true;
		ApplySettings();
		frames.UpdateSettings(settings, UpdatePhase.TempSettingsLoaded);

        openFileLine = lineNumber;
		foreach (FileArg fileArg in filesToLoad)
		{
			Buffer buffer = LoadFile(fileArg.file, fileArg.httpServer);
			if (openFileLine != 0)
			{
			    Place place = new Place(0, openFileLine - 1);
                buffer.Controller.PutCursor(place, false);
                buffer.Controller.NeedScrollToCaret();
			}
		}
		openFileLine = 0;

		FormClosing += OnFormClosing;
		mainNest.buffers.AllRemoved += OpenEmptyIfNeed;
		OpenEmptyIfNeed();

		UpdateTitle();

		Activated += OnActivated;

        InitMessageReceiving();
	}
	
	private void InitStartSettings()
	{
		if (focusedTextBox != null)
			focusedTextBox.SetViMode(settings.startWithViMode.Value);
	}

    private void InitMessageReceiving()
    {
        NativeMethods.CHANGEFILTERSTRUCT changeFilter = new NativeMethods.CHANGEFILTERSTRUCT();
        changeFilter.size = (uint)Marshal.SizeOf(changeFilter);
        changeFilter.info = 0;
        if (!NativeMethods.ChangeWindowMessageFilterEx(
        	this.Handle, NativeMethods.WM_COPYDATA, NativeMethods.ChangeWindowMessageFilterExAction.Allow, ref changeFilter))
        {
            int error = Marshal.GetLastWin32Error();
            MessageBox.Show(String.Format("The error {0} occurred.", error));
        }
    }

	public struct FileArg
	{
		public string file;
		public string httpServer;

		public FileArg(string file, string httpServer)
		{
			this.file = file;
			this.httpServer = httpServer;
		}
	}
	
	private int openFileLine = 0;

	private void ApplyArgs(string[] args, out List<FileArg> filesToLoad, out int lineNumber, out string tempFilePostfix, out string configFilePostfix)
	{
		string currentDirectory = Directory.GetCurrentDirectory();
		lineNumber = 0;
		filesToLoad = new List<FileArg>();
		tempFilePostfix = null;
		configFilePostfix = null;
		for (int i = 0; true;)
		{
			if (i < args.Length && args[i] == "-connect")
			{
				i++;
				if (i + 1 < args.Length && !args[i].StartsWith("-") && !args[i + 1].StartsWith("-"))
				{
					filesToLoad.Add(new FileArg(args[i], args[i + 1]));
					i += 2;
				}
				else
				{
					Console.Error.WriteLine("-connect requires fileName and httpServer");
					break;
				}
			}
			else if (i < args.Length && !args[i].StartsWith("-"))
			{
				filesToLoad.Add(new FileArg(Path.Combine(currentDirectory, args[i]), null));
				i++;
			}
			else if (i < args.Length && args[i] == "-temp")
			{
				i++;
				if (i < args.Length && !args[i].StartsWith("-"))
				{
					tempFilePostfix = args[i];
					i++;
				}
				else
				{
					Console.Error.WriteLine("-temp requires tempFilePostfix");
					break;
				}
			}
			else if (i < args.Length && args[i] == "-config")
			{
				i++;
				if (i < args.Length && !args[i].StartsWith("-"))
				{
					configFilePostfix = args[i];
					i++;
				}
				else
				{
					Console.Error.WriteLine("-config requires configFilePostfix");
					break;
				}
			}
			else if (i < args.Length && args[i] == "-help")
			{
			    i++;
                WriteHelp();
			}
			else if (i < args.Length && args[i].StartsWith("-line="))
			{
				if (int.TryParse(args[i].Substring("-line=".Length), out lineNumber))
				{
					i++;
				}
				else
				{
					Console.Error.WriteLine("-line requires number");
					break;
				}
			}
			else
			{
				if (i < args.Length)
                {
					Console.Error.WriteLine("Unexpected parameter: " + args[i]);
                    WriteHelp();
                }
				break;
			}
		}
	}
	
	private string GetExeHelp()
	{
	    return "<fileName>\n" +
            "-connect <fictiveFileName> <httpServer>\n" +
            "-temp <tempFilePostfix> - use different temp settings\n" +
            "-config <tempFilePostfix> - use different config\n" +
            "-help\n" + 
            "-line=<line>";
	}

    private void WriteHelp()
    {
        Console.Write("Options: " + GetExeHelp());
    }

	public bool SetCurrentDirectory(string path, out string error)
	{
		error = null;
		if (string.IsNullOrEmpty(path))
			return false;
		string oldDir = Directory.GetCurrentDirectory();
		if (path.ToLowerInvariant() == oldDir.ToLowerInvariant())
			return false;
		try
		{
			Directory.SetCurrentDirectory(path);
		}
		catch (Exception e)
		{
			error = e.Message;
			return false;
		}
		frames.UpdateSettings(settings, UpdatePhase.ChangeCurrentDirectory);
		if (hasCurrentConfig || File.Exists(AppPath.ConfigPath.GetCurrentPath()))
			ReloadConfig();
		if (tempSettings != null)
			tempSettings.AddDirectory(oldDir);
		return true;
	}

	private bool activationInProcess = false;

	private void OnActivated(object sender, EventArgs e)
	{
		CheckFilesChanges();
	}
	
	public void CheckFilesChanges()
	{
		if (activationInProcess)
			return;
		activationInProcess = true;

		foreach (Buffer buffer in frames.GetBuffers(BufferTag.File))
		{
			CheckFileChange(buffer);
		}

		activationInProcess = false;
	}
	
	private void CheckFileChange(Buffer buffer)
	{
		if (buffer.fileInfo != null)
		{
			buffer.fileInfo.Refresh();
			if (buffer.lastWriteTimeUtc != buffer.fileInfo.LastWriteTimeUtc)
			{
				if (settings.checkContentBeforeReloading.Value && IsFileEqualToBuffer(buffer))
				{
					buffer.Controller.history.MarkAsSaved();
				}
				else
				{
					DialogResult result = MessageBox.Show(
						CommonHelper.GetShortText(buffer.FullPath, 60) + "\n" +
						"______________________________________________________________________________\n" +
						"File was changed, reload it?",
						Name, MessageBoxButtons.YesNo);
					if (result == DialogResult.Yes)
					{
						ReloadFile(buffer);
					}
					else
					{
						buffer.lastWriteTimeUtc = buffer.fileInfo.LastWriteTimeUtc;
						buffer.Controller.history.MarkAsFullyUnsaved();
					}
				}
			}
		}
	}
	
	private bool IsFileEqualToBuffer(Buffer buffer)
	{
		if (buffer.FullPath == null)
		{
			return false;
		}
		byte[] bytes = null;
		try
		{
			bytes = File.ReadAllBytes(buffer.FullPath);
		}
		catch (IOException)
		{
			bool needDelete = false;
			string tempFile = Path.Combine(Path.GetTempPath(), buffer.Name);
			try
			{
				File.Copy(buffer.FullPath, tempFile, true);
				needDelete = true;
				bytes = File.ReadAllBytes(tempFile);
			}
			catch
			{
				return false;
			}
			finally
			{
				if (needDelete)
				{
					try
					{
						File.Delete(tempFile);
					}
					catch
					{
					}
				}
			}
		}
		catch (Exception)
		{
			return false;
		}
		string fileText = null;
		try
		{
			fileText = buffer.encodingPair.GetString(bytes, buffer.encodingPair.CorrectBomLength(bytes));
		}
		catch
		{
		}
		if (fileText == null)
			return false;
		return buffer.Controller.Lines.GetText() == fileText;
	}

	private void OpenEmptyIfNeed()
	{
		if (mainNest.Frame == null)
			new Frame().Create(mainNest);
		if (mainNest.buffers.list.Count == 0)
		{
			Buffer buffer = NewFileBuffer(true);
			mainNest.Frame.AddBuffer(buffer);
		}
	}
	
	private void RemoveEmptyIfNeed()
	{
		RemoveEmptyIfNeed(mainNest);
	}

	private void RemoveEmptyIfNeed(Nest nest)
	{
		Buffer buffer = null;
		int otherEmpty = 0;
		for (int i = nest.buffers.list.Count; i-- > 0;)
		{
			Buffer bufferI = nest.buffers.list[i];
			if ((bufferI.tags & BufferTag.File) == BufferTag.File &&
				bufferI.IsEmpty && !bufferI.HasHistory && bufferI.Name == UntitledTxt)
			{
				if (buffer == null && (bufferI.tags & BufferTag.Placeholder) == BufferTag.Placeholder)
				{
					buffer = bufferI;
				}
				else
				{
					++otherEmpty;
				}
			}
		}
		if (buffer != null && otherEmpty == 0)
		{
			nest.buffers.list.Remove(buffer);
		}
		
		CloseOldBuffers();
	}

	private bool forbidTempSaving = false;

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
		if (_helpBuffer != null && _helpBuffer.onRemove != null)
			_helpBuffer.onRemove(_helpBuffer);
		if (!forbidTempSaving)
			tempSettings.Save(tempFilePostfix);
		if (sharpManager != null)
			sharpManager.Close();
	}

	public KeyMapNode MenuNode { get { return menu.node; } }

	private MulticaretTextBox focusedTextBox;
	public Controller FocusedController { get { return focusedTextBox != null ? focusedTextBox.Controller : null; } }

	public MulticaretTextBox GetFocusedTextBox()
	{
		return focusedTextBox;
	}

	private Frame lastFrame;
	public Frame LastFrame { get { return lastFrame; } }

	public void SetFocus(MulticaretTextBox textBox, KeyMapNode node, Frame frame)
	{
		focusedTextBox = textBox;
		menu.node = node;
		if (frame != null)
			lastFrame = frame;
		UpdateTitle();
	}
	
	private SharpManager sharpManager;
	public SharpManager SharpManager { get { return sharpManager; } }
	
	private bool allowApply = false;
	
	private void ApplySettings()
	{
		if (!allowApply)
		{
			return;
		}
		string scheme = "";
		if (!string.IsNullOrEmpty(settings.scheme.Value))
		{
			scheme = settings.scheme.Value ?? "";
		}
		else if (!string.IsNullOrEmpty(tempSettings.Scheme))
		{
			scheme = tempSettings.Scheme ?? "";
		}
		tempSettings.Scheme = scheme;
		settings.ParsedScheme = schemeManager.LoadScheme(scheme);
		settings.Parsed = true;
		
		BackColor = settings.ParsedScheme.bgColor;
		TopMost = settings.alwaysOnTop.Value;
		frames.UpdateSettings(settings, UpdatePhase.Raw);
		frames.UpdateSettings(settings, UpdatePhase.Parsed);
		sharpManager.UpdateSettings(settings);
		if (fileTree != null)
		    fileTree.ReloadIfNeedForSettings();
		if (settings.hideMenu.Value)
		{
			if (Menu != null)
				Menu = null;
		}
		else
		{
			if (Menu != menu)
				Menu = menu;
		}
		if (settings.fullScreenOnMaximized.Value)
		{
			if (WindowState == FormWindowState.Maximized)
			{
				if (FormBorderStyle != FormBorderStyle.None)
				{
					ignoreBorderStyleChanging = true;
					WindowState = FormWindowState.Normal;
					FormBorderStyle = FormBorderStyle.None;
					WindowState = FormWindowState.Maximized;
					ignoreBorderStyleChanging = false;
				}
			}
			else
			{
				if (FormBorderStyle != FormBorderStyle.Sizable)
				{
					FormBorderStyle = FormBorderStyle.Sizable;
				}
			}
		}
		else
		{
			if (FormBorderStyle != FormBorderStyle.Sizable)
			{
				FormBorderStyle = FormBorderStyle.Sizable;
			}
		}
		snippetFilesScanner.SetIgnoreFiles(
			settings.ignoreSnippets.Value,
			settings.forcedSnippets.Value);
	}
	
	protected override void OnClientSizeChanged(EventArgs e)
	{
		if (!ignoreBorderStyleChanging)
		{
			if (windowState != WindowState)
			{
				windowState = WindowState;
				needUpdateBorderStyle = true;
			}
		}
		base.OnClientSizeChanged(e);
	}

	public void DoResize()
	{
		frames.Resize(0, 0, ClientSize);
	}

	private Nest AddNest(bool hDivided, bool left, bool isPercents, TempSettingsInt settingsInt)
	{
		Nest nest = frames.AddParentNode();
		nest.hDivided = hDivided;
		nest.left = left;
		nest.isPercents = isPercents;
		nest.size = settingsInt.value;
		nest.settingsSize = settingsInt;
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

		doNothingKeyMap.AddItem(new KeyItem(Keys.Escape, null, KeyAction.Nothing));
		doNothingKeyMap.AddItem(new KeyItem(Keys.Escape | Keys.Shift, null, KeyAction.Nothing));
		doNothingKeyMap.AddItem(new KeyItem(Keys.Control | Keys.J, null, KeyAction.Nothing));
		doNothingKeyMap.AddItem(new KeyItem(Keys.Control | Keys.K, null, KeyAction.Nothing));

		keyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, new KeyAction("&File\\New", DoNew, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.O, null, new KeyAction("&File\\Open", DoOpen, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("&File\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.S, null, new KeyAction("&File\\Save", DoSave, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.S, null,
			new KeyAction("&File\\Save As...", DoSaveAs, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.R, null, new KeyAction("&File\\Reload", DoReload, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("&File\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Alt | Keys.F4, null, new KeyAction("&File\\Exit", DoExit, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.R, null,
			new KeyAction("F&ind\\Switch regex", DoSwitchRegex, null, false)
			.SetGetText(GetFindRegex)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.I, null,
			new KeyAction("F&ind\\Switch ignore case", DoSwitchIgnoreCase, null, false)
			.SetGetText(GetFindIgnoreCase)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.E, null,
			new KeyAction("F&ind\\Switch replace escape sequence", DoSwitchEscape, null, false)
			.SetGetText(GetFindEscape)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("F&ind\\-", null, null, false)));
		
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.V, null,
			new KeyAction("&Edit\\Paste in output (for stack traces)", DoPasteInOutput, null, false)));

		keyMap.AddItem(new KeyItem(Keys.Control | Keys.D1, null,
			new KeyAction("&View\\Open/close log", DoOpenCloseLog, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.D2, null,
			new KeyAction("&View\\Open/close find results", DoOpenCloseFindResults, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.D3, null,
			new KeyAction("&View\\Open/close shell command results", DoOpenCloseShellResults, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Oemtilde, null,
			new KeyAction("&View\\Open/close console panel", DoOpenCloseConsolePanel, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("&View\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Escape, null, 
			new KeyAction("&View\\Close console panel", DoCloseConsolePanel, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null,
			new KeyAction("&View\\Change focus", DoChangeFocus, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Alt | Keys.Right, null,
			new KeyAction("&View\\Move document right", MoveDocumentRight, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Alt | Keys.Left, null,
			new KeyAction("&View\\Move document left", MoveDocumentLeft, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("&View\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.I, null,
			new KeyAction("&View\\File tree\\Open/close file tree", DoOpenCloseFileTree, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.D0, null,
			new KeyAction("&View\\File tree\\Find file in tree", DoFindFileInTree, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Enter, null,
			new KeyAction("&View\\File tree\\Switch maximized/minimized mode", DoSwitchWindowMode, null, false)));

		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F2, null,
			new KeyAction("Prefere&nces\\Edit/create current config", DoEditCreateCurrentConfig, null, false)));
		keyMap.AddItem(new KeyItem(Keys.F2, null,
			new KeyAction("Prefere&nces\\Edit config", DoOpenUserConfig, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F2, null,
			new KeyAction("Prefere&nces\\Open base config", DoOpenBaseConfig, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null,
			new KeyAction("Prefere&nces\\Reset config…", DoResetConfig, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null,
			new KeyAction("Prefere&nces\\Reset temp and close", DoResetTempAndClose, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.F3, null,
			new KeyAction("Prefere&nces\\Edit current scheme", DoEditCurrentScheme, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("Prefere&nces\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.F3, null,
			new KeyAction("Prefere&nces\\Open AppData folder", DoOpenAppDataFolder, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F3, null,
			new KeyAction("Prefere&nces\\Open Startup folder", DoOpenStartupFolder, null, false)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F4, null,
			new KeyAction("Prefere&nces\\Open current folder", DoOpenCurrentFolder, null, false)));
		keyMap.AddItem(new KeyItem(Keys.F4, null,
			new KeyAction("Prefere&nces\\Change current folder", DoChangeCurrentFolder, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("Prefere&nces\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null,
			new KeyAction("Prefere&nces\\New syntax file", DoNewSyntax, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null,
			new KeyAction("Prefere&nces\\Edit current syntax file", DoEditCurrentSyntaxFile, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null,
			new KeyAction("Prefere&nces\\Edit current base syntax file", DoEditCurrentBaseSyntaxFile, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("Prefere&nces\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null,
			new KeyAction("Prefere&nces\\New snippet file", DoNewSnippet, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null,
			new KeyAction("Prefere&nces\\Edit snippet file…", DoEditSnippetFile, null, false)));
		keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("Prefere&nces\\-", null, null, false)));
		keyMap.AddItem(new KeyItem(Keys.F5, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteF5Command, null, false)
			.SetGetText(GetF5CommandText)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F5, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteShiftF5Command, null, false)
			.SetGetText(GetShiftF5CommandText)));
		keyMap.AddItem(new KeyItem(Keys.F6, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteF6Command, null, false)
			.SetGetText(GetF6CommandText)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F6, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteShiftF6Command, null, false)
			.SetGetText(GetShiftF6CommandText)));
		keyMap.AddItem(new KeyItem(Keys.F7, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteF7Command, null, false)
			.SetGetText(GetF7CommandText)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F7, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteShiftF7Command, null, false)
			.SetGetText(GetShiftF7CommandText)));
		keyMap.AddItem(new KeyItem(Keys.F8, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteF8Command, null, false)
			.SetGetText(GetF8CommandText)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F8, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteShiftF8Command, null, false)
			.SetGetText(GetShiftF8CommandText)));
		keyMap.AddItem(new KeyItem(Keys.F9, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteF9Command, null, false)
			.SetGetText(GetF9CommandText)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F9, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteShiftF9Command, null, false)
			.SetGetText(GetShiftF9CommandText)));
		keyMap.AddItem(new KeyItem(Keys.F11, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteF11Command, null, false)
			.SetGetText(GetF11CommandText)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F11, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteShiftF11Command, null, false)
			.SetGetText(GetShiftF11CommandText)));
		keyMap.AddItem(new KeyItem(Keys.F12, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteF12Command, null, false)
			.SetGetText(GetF12CommandText)));
		keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F12, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteShiftF12Command, null, false)
			.SetGetText(GetShiftF12CommandText)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Space, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteCtrlSpaceCommand, null, false)
			.SetGetText(GetCtrlSpaceCommandText)));
		keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.Space, null,
			new KeyAction("Prefere&nces\\Execute command", DoExecuteCtrlShiftSpaceCommand, null, false)
			.SetGetText(GetCtrlShiftSpaceCommandText)));

		keyMap.AddItem(new KeyItem(Keys.F1, null, new KeyAction("&?\\Help", DoHelp, null, false)));
	}
	
	private bool DoPasteInOutput(Controller controller)
	{
		new RunShellCommand(this).ShowInOutput(
			ClipboardExecuter.GetFromClipboard(), settings.shellRegexList.Value, false, null);
		return true;
	}

	private bool DoNew(Controller controller)
	{
		OpenNew();
		return true;
	}
	
	public void OpenNew()
	{
		Nest nest = GetMainNest();
		RemoveEmptyIfNeed(nest);
		nest.Frame.AddBuffer(NewFileBuffer());
	}
	
	public void ProcessTabBarDoubleClick(Nest nest)
	{
		if (nest == mainNest || nest == mainNest2)
		{
			OpenNew();
		}
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
		return LoadFile(file, null);
	}

	public Buffer LoadFile(string file, string httpServer)
	{
		return LoadFile(file, httpServer, null);
	}
	
	public Buffer GetBuffer(string file)
	{
		string name = null;
		string fullPath = null;
		try
		{
			fullPath = Path.GetFullPath(file);
			name = Path.GetFileName(file);
		}
		catch (Exception)
		{
			return null;
		}
		return mainNest.buffers.GetBuffer(fullPath, name) ?? mainNest2.buffers.GetBuffer(fullPath, name);
	}

	public Buffer LoadFile(string file, string httpServer, Nest nest)
	{
		string fullPath = null;
		string name = null;
		try
		{
			fullPath = Path.GetFullPath(file);
			name = Path.GetFileName(file);
		}
		catch (Exception e)
		{
			Log.WriteWarning("Path", e.Message + " (" + file + ")");
			Log.Open();
			return null;
		}
		Buffer buffer = mainNest.buffers.GetBuffer(fullPath, name) ?? mainNest2.buffers.GetBuffer(fullPath, name);
		bool needLoad = false;
		bool isNew = false;
		if (buffer == null)
		{
			buffer = NewFileBuffer();
			buffer.httpServer = httpServer;
			needLoad = true;
			isNew = true;
		}
		buffer.SetFile(fullPath, name);
		if (nest == null)
			nest = buffer.Frame != null ? buffer.Frame.Nest : GetMainNest();
		ShowBuffer(nest, buffer);
		if (needLoad && !ReloadFile(buffer))
		{
			if (isNew && buffer.Frame != null)
				buffer.Frame.RemoveBuffer(buffer);
			return null;
		}
		if (buffer.Frame != null)
			buffer.Frame.UpdateHighlighter();
		RemoveEmptyIfNeed();
		return buffer;
	}

	public Buffer ForcedLoadFile(string file)
	{
		string fullPath = null;
		string name = null;
		try
		{
			fullPath = Path.GetFullPath(file);
			name = Path.GetFileName(file);
		}
		catch (Exception e)
		{
			Log.WriteWarning("Path", e.Message + " (" + file + ")");
			Log.Open();
			return null;
		}
		Buffer buffer = mainNest.buffers.GetBuffer(fullPath, name) ?? mainNest2.buffers.GetBuffer(fullPath, name);
		bool needLoad = false;
		if (buffer == null)
		{
			buffer = NewFileBuffer();
			needLoad = File.Exists(fullPath);
		}
		buffer.SetFile(fullPath, name);
		ShowBuffer(buffer.Frame != null ? buffer.Frame.Nest : mainNest, buffer);
		if (needLoad && !ReloadFile(buffer))
			return null;
		if (buffer.Frame != null)
			buffer.Frame.UpdateHighlighter();
		RemoveEmptyIfNeed();
		return buffer;
	}

	public bool ReloadFile(Buffer buffer)
    {
        return ReloadFile(buffer, false);
    }

	public bool ReloadFile(Buffer buffer, bool last)
	{
		if (buffer.httpServer != null)
		{
			string text = "";
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
					buffer.httpServer + "/" + buffer.Name + "/get");
				request.Timeout = settings.connectionTimeout.Value;
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

				byte[] byteVersion = Encoding.ASCII.GetBytes("NULL");
				request.ContentLength = byteVersion.Length;

				Stream stream = request.GetRequestStream();
				stream.Write(byteVersion, 0, byteVersion.Length);
				stream.Close();

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				buffer.encodingPair = settings.httpEncoding.Value;
				using (StreamReader reader = new StreamReader(
					response.GetResponseStream(), settings.httpEncoding.Value.encoding))
				{
					text = reader.ReadToEnd();
				}
			}
			catch (Exception e)
			{
				Log.WriteError("http", e.ToString());
				Log.Open();
				return false;
			}
			buffer.InitText(text);
			return true;
		}
		if (!File.Exists(buffer.FullPath))
		{
			Log.WriteWarning("Missing file", buffer.FullPath);
			Log.Open();
			return false;
		}
		{
            tempSettings.ApplyQualitiesBeforeLoading(buffer);
			byte[] bytes = null;
			try
			{
				bytes = File.ReadAllBytes(buffer.FullPath);
			}
			catch (IOException e)
			{
				bool needDelete = false;
				string tempFile = Path.Combine(Path.GetTempPath(), buffer.Name);
				try
				{
					File.Copy(buffer.FullPath, tempFile, true);
					needDelete = true;
					bytes = File.ReadAllBytes(tempFile);
				}
				catch
				{
					Log.WriteError("File loading error", e.Message);
					Log.Open();
				}
				finally
				{
					if (needDelete)
					{
						try
						{
							File.Delete(tempFile);
						}
						catch
						{
						}
					}
				}
			}
			string error;
			buffer.InitBytes(bytes, settings.defaultEncoding.Value, out error);
			if (error != null)
			{
				Log.WriteError("File decoding error", error);
				Log.Open();
			}
			buffer.fileInfo = new FileInfo(buffer.FullPath);
			buffer.lastWriteTimeUtc = buffer.fileInfo.LastWriteTimeUtc;
			buffer.needSaveAs = false;
			tempSettings.ApplyQualities(buffer, openFileLine);
            if (last)
            {
                buffer.Controller.DocumentEnd(false);
            }
			return true;
		}
	}

	private bool DoSave(Controller controller)
	{
		TrySaveFile(frames.GetSelectedBuffer(BufferTag.File));
		return true;
	}

	private bool DoReload(Controller controller)
	{
		Buffer buffer = frames.GetSelectedBuffer(BufferTag.File);
		if (buffer != null)
		{
            Selection selection = buffer.Controller.Lines.LastSelection;
            bool last = selection.Empty && selection.caret == buffer.Controller.Lines.charsCount;
			tempSettings.StorageQualities(buffer);

			if (buffer.Changed)
			{
				DialogResult result = MessageBox.Show(
					"File has unsaved changes. Reload it anyway?",
					Name, MessageBoxButtons.YesNo);
				if (result == DialogResult.Yes)
                {
					ReloadFile(buffer, last);
                }
			}
			else
			{
				ReloadFile(buffer, last);
			}
		}
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
				UpdateTitle();
			}
		}
		return true;
	}

	private void TrySaveFile(Buffer buffer)
	{
		if (buffer == null)
			return;
		buffer.settedEncodingPair = buffer.encodingPair;
		if (buffer.httpServer != null)
		{
			string text = "";
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(buffer.httpServer + "/" + buffer.Name + "/push");
				request.Timeout = settings.connectionTimeout.Value;
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

				byte[] byteVersion = buffer.encodingPair.encoding.GetBytes(buffer.Controller.Lines.GetText());
				request.ContentLength = byteVersion.Length;

				Stream stream = request.GetRequestStream();
				stream.Write(byteVersion, 0, byteVersion.Length);
				stream.Close();

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				using (StreamReader reader = new StreamReader(response.GetResponseStream(), buffer.encodingPair.encoding))
				{
					text = reader.ReadToEnd();
				}
			}
			catch (Exception e)
			{
				Log.WriteError("http", e.ToString());
				Log.Open();
				return;
			}
			buffer.Controller.history.MarkAsSaved();
			Log.WriteInfo("Responce", text);
			return;
		}
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
		string text = buffer.Controller.Lines.GetText();
		try
		{
			if (!buffer.encodingPair.bom && buffer.encodingPair.encoding == Encoding.UTF8)
				File.WriteAllText(buffer.FullPath, text);
			else
				File.WriteAllText(buffer.FullPath, text, buffer.encodingPair.encoding);
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
		frames.UpdateSettings(settings, UpdatePhase.FileSaved);
		string fullPath = buffer.FullPath.ToLowerInvariant();
		string syntaxDir = Path.GetDirectoryName(buffer.FullPath).ToLowerInvariant();
		if (fullPath == AppPath.ConfigPath.GetCurrentPath().ToLowerInvariant() ||
			fullPath == AppPath.ConfigPath.startupPath.ToLowerInvariant() ||
			fullPath == AppPath.ConfigPath.appDataPath.ToLowerInvariant())
		{
			ReloadConfig();
		}
		else if (schemeManager.IsActiveSchemePath(GetCurrentScheme(), buffer.FullPath))
		{
			ApplySettings();
		}
		else if (Path.GetExtension(buffer.FullPath).ToLowerInvariant() == ".xml" &&
			(syntaxDir == AppPath.SyntaxDir.appDataPath.ToLowerInvariant() ||
			syntaxDir == AppPath.SyntaxDir.startupPath.ToLowerInvariant()))
		{
			ReloadSyntaxes();
		}
		else if (Path.GetExtension(buffer.FullPath).ToLowerInvariant() == ".snippets")
		{
			string dir = Path.GetDirectoryName(buffer.FullPath).ToLowerInvariant();
			if (dir == AppPath.SnippetsDir.appDataPath.ToLowerInvariant() ||
				dir == AppPath.SnippetsDir.startupPath.ToLowerInvariant())
			{
				snippetFilesScanner.Reset();
			}
		}
		Properties.CommandInfo info = GetCommandInfo(settings.afterSaveCommand.Value, buffer);
		if (info != null && !string.IsNullOrEmpty(info.command))
		{
			commander.Execute(info.command, true, false);
		}
	}
	
	private string GetCurrentScheme()
	{
		string scheme = settings.scheme.Value;
		if (string.IsNullOrEmpty(scheme))
			scheme = tempSettings.Scheme;
		return scheme;
	}

	private bool DoExit(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoSwitchRegex(Controller controller)
	{
		tempSettings.FindParams.regex = !tempSettings.FindParams.regex;
		frames.UpdateSettings(settings, UpdatePhase.FindParams);
		return true;
	}

	private bool DoSwitchIgnoreCase(Controller controller)
	{
		tempSettings.FindParams.ignoreCase = !tempSettings.FindParams.ignoreCase;
		frames.UpdateSettings(settings, UpdatePhase.FindParams);
		return true;
	}

	private bool DoSwitchEscape(Controller controller)
	{
		tempSettings.FindParams.escape = !tempSettings.FindParams.escape;
		frames.UpdateSettings(settings, UpdatePhase.FindParams);
		return true;
	}
	
	private string GetFindRegex()
	{
		return tempSettings.FindParams.regex ? " (on)" : " (off)";
	}
	
	private string GetFindIgnoreCase()
	{
		return tempSettings.FindParams.ignoreCase ? " (on)" : " (off)";
	}
	
	private string GetFindEscape()
	{
		return tempSettings.FindParams.escape ? " (on)" : " (off)";
	}

	private bool DoOpenCloseLog(Controller controller)
	{
		return OpenCloseConsoleBuffer(LogId);
	}

	private bool DoOpenCloseFindResults(Controller controller)
	{
		return OpenCloseConsoleBuffer(FindResultsId);
	}

	private bool DoOpenCloseShellResults(Controller controller)
	{
		return OpenCloseConsoleBuffer(ShellResultsId);
	}

	private bool OpenCloseConsoleBuffer(string id)
	{
		Buffer buffer;
		consoleBuffers.TryGetValue(id, out buffer);
		if (buffer == null)
			return false;
		if (consoleNest.buffers.list.Selected == buffer)
		if (buffer.Frame != null)
		{
			buffer.Frame.Destroy();
			return true;
		}
		consoleBuffers[id] = buffer;
		ShowBuffer(consoleNest, buffer);
		if (consoleNest.Frame != null)
			consoleNest.Frame.Focus();
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

	private bool DoCloseConsolePanel(Controller controller)
	{
		if (consoleNest.AFrame != null)
		{
			consoleNest.AFrame.Destroy();
			return true;
		}
		return false;
	}
	
	public bool FileTreeFocused
	{
	    get
	    {
	        return leftNest.AFrame != null && leftNest.AFrame.Focused && leftNest.buffers.list.Selected == fileTree.Buffer;
	    }
	}
	
	public void FileTreeReload()
	{
	    if (leftNest.AFrame != null && leftNest.buffers.list.Selected == fileTree.Buffer)
		{
			fileTree.Reload();
		}
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

	private bool DoFindFileInTree(Controller controller)
	{
		if (leftNest.AFrame != null && leftNest.buffers.list.Selected == fileTree.Buffer && leftNest.AFrame.Focused)
		{
			leftNest.AFrame.Destroy();
			return true;
		}
		
		Buffer buffer = LastBuffer;
		if (buffer == null || buffer.FullPath == null)
			return false;
		OpenFileTree();
		fileTree.Find(buffer.FullPath);
		leftNest.Frame.Focus();
		return true;
	}

	public bool FileTreeOpened
	{
		get { return fileTree.Buffer.Frame != null; }
	}

	public FileTree FileTree { get { return fileTree; } }

	public void OpenFileTree()
	{
		if (fileTree.Buffer.Frame == null)
		{
			if (leftNest.AFrame == null)
				new Frame().Create(leftNest);
			leftNest.Frame.AddBuffer(fileTree.Buffer);
			leftNest.Frame.Focus();
		}
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
	
	private bool DoSwitchWindowMode(Controller controller)
	{
		if (WindowState == FormWindowState.Normal)
		{
			WindowState = FormWindowState.Maximized;
		}
		else
		{
			WindowState = FormWindowState.Normal;
		}
		return true;
	}

	private bool DoEditCreateCurrentConfig(Controller controller)
	{
		string path = AppPath.ConfigPath.GetCurrentPath();
		string templatePath = Path.Combine(AppPath.TemplatesDir, "current-tw-config.xml");
		if (!File.Exists(path))
		{
			if (!File.Exists(templatePath))
			{
				Log.WriteWarning("Config", "Missing template config at: " + templatePath);
				Log.Open();
				return false;
			}
			Buffer buffer = LoadFile(templatePath);
			buffer.SetFile(path, Path.GetFileName(path));
		}
		else
		{
			LoadFile(path);
		}
		return true;
	}

	private bool DoOpenUserConfig(Controller controller)
	{
		CopyConfigIfNeed();
		LoadFile(AppPath.ConfigPath.appDataPath);
		return true;
	}

	private bool DoOpenBaseConfig(Controller controller)
	{
		if (!File.Exists(AppPath.ConfigPath.startupPath))
		{
			MessageBox.Show("Missing base config", Name, MessageBoxButtons.OK);
			return true;
		}
		LoadFile(AppPath.ConfigPath.startupPath);
		return true;	
	}

	private bool DoResetConfig(Controller controller)
	{
		if (!File.Exists(AppPath.ConfigPath.appDataPath))
		{
			MessageBox.Show("Nothing to reset", Name, MessageBoxButtons.OK);
			return true;
		}
		DialogResult result = MessageBox.Show("Current config will be removed", Name, MessageBoxButtons.OKCancel);
		if (result == DialogResult.OK)
		{
			if (File.Exists(AppPath.ConfigPath.appDataPath))
				File.Delete(AppPath.ConfigPath.appDataPath);
			CopyConfigIfNeed();
			ReloadConfig();

			activationInProcess = true;
			string fullPath = Path.GetFullPath(AppPath.ConfigPath.appDataPath);
			string name = Path.GetFileName(AppPath.ConfigPath.appDataPath);
			Buffer buffer = mainNest.buffers.GetBuffer(fullPath, name) ?? mainNest.buffers.GetBuffer(fullPath, name);
			if (buffer != null && buffer.Frame != null)
				CheckFileChange(buffer);
			activationInProcess = false;
		}
		return true;
	}

	private bool DoResetTempAndClose(Controller controller)
	{
		string path = TempSettings.GetTempSettingsPath(tempFilePostfix, AppPath.StartupDir);
		if (File.Exists(path))
			File.Delete(path);
		forbidTempSaving = true;
		Close();
		return true;
	}

	private bool DoEditCurrentScheme(Controller controller)
	{
		CreateAppDataFolders();
		List<AppPath> paths = schemeManager.GetSchemePaths(GetCurrentScheme());
		if (paths.Count > 0)
		{
			foreach (AppPath path in paths)
			{
				if (File.Exists(path.startupPath))
				{
					if (!File.Exists(path.appDataPath))
						File.Copy(path.startupPath, path.appDataPath);
					LoadFile(path.appDataPath);
				}
				else if (File.Exists(path.appDataPath))
				{
					LoadFile(path.appDataPath);
				}
				else
				{
					Log.WriteWarning("Missing scheme", path.startupPath + "\nand missing in: " + path.appDataPath);
					Log.Open();
				}
			}
		}
		return true;
	}

	private bool DoOpenAppDataFolder(Controller controller)
	{
		CreateAppDataFolders();
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		process.StartInfo.FileName = AppPath.AppDataDir;
		process.Start();
		return true;
	}

	private bool DoOpenStartupFolder(Controller controller)
	{
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		process.StartInfo.FileName = AppPath.StartupDir;
		process.Start();
		return true;
	}

	private bool DoOpenCurrentFolder(Controller controller)
	{
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		process.StartInfo.FileName = Directory.GetCurrentDirectory();
		process.Start();
		return true;
	}

	private bool DoChangeCurrentFolder(Controller controller)
	{
		FolderBrowserDialog dialog = new FolderBrowserDialog();
		dialog.Description = "Current folder selection";
		dialog.SelectedPath = Directory.GetCurrentDirectory();
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			string error;
			if (!SetCurrentDirectory(dialog.SelectedPath, out error))
				MessageBox.Show(error, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
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
			Log.WriteWarning("Missing template", templatePath);
			Log.Open();
			return true;
		}
		buffer.InitText(File.ReadAllText(templatePath));
		return true;
	}
	
	private bool DoNewSnippet(Controller controller)
	{
		CreateAppDataFolders();
		string path = Path.Combine(AppPath.SnippetsDir.appDataPath, "Untitled.snippet");
		OpenNewAsFile(path, "extensions:*.*\r\nsnippet key\r\n\ttext", true);
		return true;
	}
	
	public void OpenNewAsFile(string path, string text, bool unsaved)
	{
		Buffer buffer = NewFileBuffer();
		buffer.SetFile(path, Path.GetFileName(path));
		buffer.InitText(text);
		if (unsaved)
		{
			buffer.Controller.history.MarkAsFullyUnsaved();
		}
		GetMainNest().Frame.AddBuffer(buffer);
	}

	private bool DoEditCurrentSyntaxFile(Controller controller)
	{
		EditCurrentSyntaxFile(false);
		return true;
	}

	private bool DoEditCurrentBaseSyntaxFile(Controller controller)
	{
		EditCurrentSyntaxFile(true);
		return true;
	}

	private void EditCurrentSyntaxFile(bool isBase)
	{
		CreateAppDataFolders();
		if (LastBuffer == null || LastBuffer.Controller.isReadonly || string.IsNullOrEmpty(LastBuffer.FullPath))
		{
			Dialogs.ShowInfo("Error", "No file with path in current frame");
			return;
		}
		Highlighter highlighter = LastBuffer.Frame.TextBox.Highlighter;
		if (highlighter == null || string.IsNullOrEmpty(highlighter.type))
		{
			Dialogs.ShowInfo("Error", "No syntax in current frame");
			return;
		}
		string file = syntaxFilesScanner.GetSyntaxFileByName(highlighter.type);
		if (string.IsNullOrEmpty(file))
		{
			Dialogs.ShowInfo("Error", "No file for syntax");
			return;
		}
		string fileName = Path.GetFileName(file);
		string startupPath = Path.Combine(AppPath.SyntaxDir.startupPath, fileName);
		string appDataPath = Path.Combine(AppPath.SyntaxDir.appDataPath, fileName);
		if (isBase)
		{
			LoadFile(startupPath);
		}
		else
		{
			if (!File.Exists(appDataPath))
				File.Copy(startupPath, appDataPath);
			LoadFile(appDataPath);
		}
	}
	
	private bool DoEditSnippetFile(Controller controller)
	{
		dialogs.OpenSnippetsSearch();
		return true;
	}

	public void CreateAppDataFolders()
	{
		CopyConfigIfNeed();
		if (!Directory.Exists(AppPath.SyntaxDir.appDataPath))
			Directory.CreateDirectory(AppPath.SyntaxDir.appDataPath);
		if (!File.Exists(AppPath.SyntaxDtd.appDataPath) && File.Exists(AppPath.SyntaxDtd.startupPath))
			File.Copy(AppPath.SyntaxDtd.startupPath, AppPath.SyntaxDtd.appDataPath);
		if (!Directory.Exists(AppPath.SchemesDir.appDataPath))
			Directory.CreateDirectory(AppPath.SchemesDir.appDataPath);
		if (!Directory.Exists(AppPath.SnippetsDir.appDataPath))
			Directory.CreateDirectory(AppPath.SnippetsDir.appDataPath);
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
			List<StyleRange> ranges = new List<StyleRange>();
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("# About");
			builder.AppendLine();
			builder.AppendLine(Application.ProductName);
			builder.AppendLine("Build " + Application.ProductVersion);
			builder.AppendLine();
			builder.AppendLine("# Command line options");
			builder.AppendLine();
			builder.AppendLine(GetExeHelp());
			builder.AppendLine();
			builder.AppendLine("# Actions");
			builder.AppendLine();
			builder.AppendLine("All actions are represented in menu.");
			builder.AppendLine("Menu subitems are depended on frame with cursor");
			builder.AppendLine("[] in menu item denotes complex shortcut,");
			builder.AppendLine("i.e. for [Ctrl+Tab]:");
			builder.AppendLine("\tCtrl↓, Tab↓↑, Ctrl↑ - switch back / forward between 2 tabs");
			builder.AppendLine("\tCtrl↓, Tab↓↑, Tab↓↑, Ctrl↑ - switch back / forward between 3 tabs");
			builder.AppendLine();
			builder.AppendLine(commander.GetHelpText());
			builder.AppendLine(settings.GetHelpText());
			builder.AppendLine("# Syntax highlighting styles");
			builder.AppendLine();
			foreach (Ds ds in Ds.all)
			{
				ranges.Add(new StyleRange(builder.Length, ds.name.Length, ds.index));
				builder.AppendLine(ds.name);
			}
			_helpBuffer = new Buffer(null, "Help.twh", SettingsMode.Help);
			_helpBuffer.tags = BufferTag.Other;
			_helpBuffer.onRemove = OnHelpBufferRemove;
			_helpBuffer.Controller.isReadonly = true;
			_helpBuffer.Controller.InitText(builder.ToString());
			_helpBuffer.Controller.Lines.ranges = ranges;
			if (tempSettings.helpPosition < 0)
				tempSettings.helpPosition = 0;
			else if (tempSettings.helpPosition > _helpBuffer.Controller.Lines.charsCount)
				tempSettings.helpPosition = _helpBuffer.Controller.Lines.charsCount;
			ShowBuffer(GetMainNest(), _helpBuffer);
			_helpBuffer.Controller.PutCursor(_helpBuffer.Controller.Lines.PlaceOf(tempSettings.helpPosition), false);
			_helpBuffer.Controller.NeedScrollToCaret();
		}
		else
		{
			_helpBuffer.Frame.RemoveBuffer(_helpBuffer);
			_helpBuffer = null;
		}
	}

	private bool OnHelpBufferRemove(Buffer buffer)
	{
		if (buffer != null)
		{
			tempSettings.helpPosition = buffer.Controller.LastSelection.caret;
		}
		_helpBuffer = null;
		return true;
	}
	
	private Buffer NewFileBuffer()
	{
		return NewFileBuffer(false);
	}

	private Buffer NewFileBuffer(bool placeholder)
	{
		Buffer buffer = new Buffer(null, UntitledTxt, SettingsMode.Normal);
		buffer.tags = BufferTag.File;
		if (placeholder)
		{
			buffer.tags |= BufferTag.Placeholder;
		}
		buffer.needSaveAs = true;
		buffer.onRemove = OnFileBufferRemove;
		buffer.encodingPair = settings.defaultEncoding.Value;
		return buffer;
	}

	public void ShowBuffer(Nest nest, Buffer buffer)
	{
		if (nest.Frame == null)
			new Frame().Create(nest);
		nest.Frame.AddBuffer(buffer);
	}
	
	public void MarkShowed(Buffer buffer)
	{
		if (buffer.FullPath != null)
			tempSettings.MarkLoaded(buffer);
	}

	private Dictionary<string, Buffer> consoleBuffers = new Dictionary<string, Buffer>();

	public const string LogId = "LogId";
	public const string FindResultsId = "FindResultsId";
	public const string ShellResultsId = "ShellResultsId";

	public void ShowConsoleBuffer(string id, Buffer buffer)
	{
		Buffer oldBuffer;
		consoleBuffers.TryGetValue(id, out oldBuffer);
		if (oldBuffer != null)
		{
			if (oldBuffer.Frame != null)
				oldBuffer.Frame.RemoveBuffer(oldBuffer);
			else
				consoleNest.buffers.list.Remove(oldBuffer);
			consoleBuffers.Remove(id);
		}
		RegisterConsoleBuffer(id, buffer);
		if (buffer != null)
		{
			ShowBuffer(consoleNest, buffer);
			if (consoleNest.Frame != null)
				consoleNest.Frame.Focus();
		}
	}

	public void RegisterConsoleBuffer(string id, Buffer buffer)
	{
		if (id != null && buffer != null)
			consoleBuffers[id] = buffer;
		else if (id != null)
			consoleBuffers.Remove(id);
	}

	private bool OnFileBufferRemove(Buffer buffer)
	{
		if (buffer != null)
		{
			tempSettings.StorageQualities(buffer);
			if (buffer.Changed)
			{
				DialogResult result = MessageBox.Show(
					"Do you want to save the current changes in\n" + buffer.Name + "?",
					Name,
					MessageBoxButtons.YesNoCancel);
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

	private void CopyConfigIfNeed()
	{
		if (!File.Exists(AppPath.ConfigPath.appDataPath))
		{
			if (!File.Exists(AppPath.ConfigPath.startupPath))
			{
				Log.WriteWarning("Config", "Missing base config at: " + AppPath.ConfigPath.startupPath);
				Log.Open();
				return;
			}
			File.Copy(AppPath.ConfigPath.startupPath, AppPath.ConfigPath.appDataPath);
			Log.WriteInfo("Config", "Config was created: " + AppPath.ConfigPath.appDataPath);
		}
	}

	private bool hasCurrentConfig = false;

	private void ReloadConfig()
	{
		hasCurrentConfig = false;
		configParser.Reset();
		StringBuilder builder = new StringBuilder();
		foreach (string path in AppPath.ConfigPath.GetBoth())
		{
			if (File.Exists(path))
			{
				XmlDocument xml = xmlLoader.Load(path, false);
				if (xml != null)
					configParser.Parse(xml, builder);
			}
		}
		{
			string path = AppPath.ConfigPath.GetCurrentPath();
			if (path != AppPath.ConfigPath.startupPath && File.Exists(path))
			{
				XmlDocument xml = xmlLoader.Load(path, false);
				if (xml != null)
				{
					configParser.Parse(xml, builder);
					hasCurrentConfig = true;
				}
			}
		}
		if (builder.Length > 0)
		{
			Log.WriteError("Config", builder.ToString());
			Log.Open();
		}
		settings.DispatchChange();
	}

	public void UpdateHighlighter(MulticaretTextBox textBox, string fileName, Buffer buffer)
	{
		Highlighter highlighter = null;
		if (buffer != null && !string.IsNullOrEmpty(buffer.customSyntax))
			highlighter = highlightingSet.GetHighlighter(buffer.customSyntax);
		if (highlighter == null && fileName != null)
		{
			string syntax = syntaxFilesScanner.GetSyntaxByFile(fileName);
			highlighter = syntax != null ? highlightingSet.GetHighlighter(syntax) : null;
		}
		if (textBox.Highlighter != highlighter)
		{
			if (highlighter == null && buffer != null && (buffer.tags & BufferTag.File) != 0 &&
				textBox.Controller != null)
				textBox.Controller.Lines.ResetColor();
			textBox.Highlighter = highlighter;
		}
	}

	public void NavigateTo(string fileName, int position0, int position1)
	{
		Buffer buffer = LoadFile(fileName);
		if (buffer != null)
		{
			buffer.Controller.PutCursor(buffer.Controller.Lines.PlaceOf(position0), false);
			buffer.Controller.PutCursor(buffer.Controller.Lines.PlaceOf(position1), true);
			if (buffer.Frame != null)
			{
				buffer.Frame.Focus();
				buffer.Frame.TextBox.MoveToCaret();
			}
		}
	}

	public void NavigateTo(string fileName, Place place0, Place place1)
	{
		Buffer buffer = LoadFile(fileName);
		if (buffer != null)
		{
			buffer.Controller.PutCursor(place0, false);
			buffer.Controller.PutCursor(place1, true);
			if (buffer.Frame != null)
			{
				buffer.Frame.Focus();
				buffer.Frame.TextBox.MoveToCaret();
			}
		}
	}
	
	public void NavigateTo(string fileName, Place place0, int length)
	{
		Buffer buffer = LoadFile(fileName);
		if (buffer != null)
		{
			int position = buffer.Controller.Lines.IndexOf(place0) + length;
			if (position < 0)
				position = 0;
			if (position > buffer.Controller.Lines.charsCount)
				position = buffer.Controller.Lines.charsCount;
			Place place1 = buffer.Controller.Lines.PlaceOf(position);
			buffer.Controller.PutCursor(place0, false);
			buffer.Controller.PutCursor(place1, true);
			if (buffer.Frame != null)
			{
				buffer.Frame.Focus();
				buffer.Frame.TextBox.MoveToCaret();
			}
		}
	}

	private bool DoExecuteF5Command(Controller controller)
	{
		return ExecuteCommand(settings.f5Command.Value);
	}
	
	private string GetF5CommandText()
	{
		return GetCommandText(settings.f5Command);
	}

	private bool DoExecuteF6Command(Controller controller)
	{
		return ExecuteCommand(settings.f6Command.Value);
	}
	
	private string GetF6CommandText()
	{
		return GetCommandText(settings.f6Command);
	}

	private bool DoExecuteF7Command(Controller controller)
	{
		return ExecuteCommand(settings.f7Command.Value);
	}
	
	private string GetF7CommandText()
	{
		return GetCommandText(settings.f7Command);
	}

	private bool DoExecuteF8Command(Controller controller)
	{
		return ExecuteCommand(settings.f8Command.Value);
	}
	
	private string GetF8CommandText()
	{
		return GetCommandText(settings.f8Command);
	}

	private bool DoExecuteF9Command(Controller controller)
	{
		return ExecuteCommand(settings.f9Command.Value);
	}
	
	private string GetF9CommandText()
	{
		return GetCommandText(settings.f9Command);
	}

	private bool DoExecuteF11Command(Controller controller)
	{
		return ExecuteCommand(settings.f11Command.Value);
	}
	
	private string GetF11CommandText()
	{
		return GetCommandText(settings.f11Command);
	}

	private bool DoExecuteF12Command(Controller controller)
	{
		return ExecuteCommand(settings.f12Command.Value);
	}
	
	private string GetF12CommandText()
	{
		return GetCommandText(settings.f12Command);
	}
	
	private bool DoExecuteShiftF5Command(Controller controller)
	{
		return ExecuteCommand(settings.shiftF5Command.Value);
	}
	
	private string GetShiftF5CommandText()
	{
		return GetCommandText(settings.shiftF5Command);
	}
	
	private bool DoExecuteShiftF6Command(Controller controller)
	{
		return ExecuteCommand(settings.shiftF6Command.Value);
	}
	
	private string GetShiftF6CommandText()
	{
		return GetCommandText(settings.shiftF6Command);
	}
	
	private bool DoExecuteShiftF7Command(Controller controller)
	{
		return ExecuteCommand(settings.shiftF7Command.Value);
	}
	
	private string GetShiftF7CommandText()
	{
		return GetCommandText(settings.shiftF7Command);
	}
	
	private bool DoExecuteShiftF8Command(Controller controller)
	{
		return ExecuteCommand(settings.shiftF8Command.Value);
	}
	
	private string GetShiftF8CommandText()
	{
		return GetCommandText(settings.shiftF8Command);
	}
	
	private bool DoExecuteShiftF9Command(Controller controller)
	{
		return ExecuteCommand(settings.shiftF9Command.Value);
	}
	
	private string GetShiftF9CommandText()
	{
		return GetCommandText(settings.shiftF9Command);
	}
	
	private bool DoExecuteShiftF11Command(Controller controller)
	{
		return ExecuteCommand(settings.shiftF11Command.Value);
	}
	
	private string GetShiftF11CommandText()
	{
		return GetCommandText(settings.shiftF11Command);
	}
	
	private bool DoExecuteShiftF12Command(Controller controller)
	{
		return ExecuteCommand(settings.shiftF12Command.Value);
	}
	
	private string GetShiftF12CommandText()
	{
		return GetCommandText(settings.shiftF12Command);
	}
	
	private bool DoExecuteCtrlSpaceCommand(Controller controller)
	{
		return ExecuteCommand(settings.ctrlSpaceCommand.Value);
	}
	
	private string GetCtrlSpaceCommandText()
	{
		return GetCommandText(settings.ctrlSpaceCommand);
	}
	
	private bool DoExecuteCtrlShiftSpaceCommand(Controller controller)
	{
		return ExecuteCommand(settings.ctrlShiftSpaceCommand.Value);
	}
	
	private string GetCtrlShiftSpaceCommandText()
	{
		return GetCommandText(settings.ctrlShiftSpaceCommand);
	}

	private void ReloadSyntaxes()
	{
		syntaxFilesScanner.Rescan();
		highlightingSet.UpdateParameters(syntaxFilesScanner);
		mainNest.Frame.UpdateHighlighter();
		foreach (Buffer buffer in mainNest.buffers.list)
		{
			buffer.Controller.Lines.ResetHighlighting();
		}
		if (mainNest2.Frame != null)
		{
			mainNest2.Frame.UpdateHighlighter();
			foreach (Buffer buffer in mainNest2.buffers.list)
			{
				buffer.Controller.Lines.ResetHighlighting();
			}
		}
	}
	
	public List<Buffer> GetFileBuffers()
	{
		List<Buffer> buffers = new List<Buffer>();
		foreach (Buffer buffer in mainNest.buffers.list)
		{
			buffers.Add(buffer);
		}
		if (mainNest2.Frame != null)
		{
			foreach (Buffer buffer in mainNest2.buffers.list)
			{
				buffers.Add(buffer);
			}
		}
		return buffers;
	}

	private bool ExecuteCommand(IRList<Properties.CommandInfo> infos)
	{
		Properties.CommandInfo info = GetCommandInfo(infos, LastBuffer);
		if (info != null)
		{
			commander.Execute(info.command, true, false);
		}
		return true;
	}
	
	private string GetCommandText(Properties.Command command)
	{
		Properties.CommandInfo info = GetCommandInfo(command.Value, LastBuffer);
		return info != null ? ": " + CommonHelper.GetShortText(info.command, 40) : "";
	}
	
	private Properties.CommandInfo GetCommandInfo(IRList<Properties.CommandInfo> infos, Buffer buffer)
	{
		string name = buffer != null ? buffer.Name : null;
		Properties.CommandInfo info = null;
		if (name != null)
		{
			for (int i = infos.Count; i-- > 0;)
			{
				Properties.CommandInfo infoI = infos[i];
				if (infoI.filter != null && infoI.filter.Match(name))
				{
					info = infoI;
					break;
				}
			}
		}
		if (info == null)
		{
			for (int i = infos.Count; i-- > 0;)
			{
				Properties.CommandInfo infoI = infos[i];
				if (infoI.filter == null)
				{
					info = infoI;
					break;
				}
			}
		}
		return info;
	}

	private bool MoveDocumentRight(Controller controller)
	{
		if (mainNest2.Frame == null)
			new Frame().Create(mainNest2);
		mainNest2.Frame.AddBuffer(mainNest.buffers.list.Selected);
		mainNest2.Frame.Focus();
		return true;
	}

	private bool MoveDocumentLeft(Controller controller)
	{
		if (mainNest2.Frame != null)
		{
			mainNest.Frame.AddBuffer(mainNest2.buffers.list.Selected);
			mainNest.Frame.Focus();
		}
		return true;
	}

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_COPYDATA)
        {
            // Extract the file name
            NativeMethods.COPYDATASTRUCT copyData = 
            (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure
            (m.LParam, typeof(NativeMethods.COPYDATASTRUCT));
            int dataType = (int)copyData.dwData;
            if (dataType == 2)
            {
                string text = Marshal.PtrToStringAnsi(copyData.lpData);
                string part0 = text;
                string part1 = "";
                int index = text.IndexOf("++");
                if (index != -1)
                {
                    part0 = text.Substring(0, index);
                    part1 = text.Substring(index + 2);
                }
                string[] files = part0.Split('+');
                string[] supportedArgs = part1.Split('+');
                openFileLine = 0;
                if (supportedArgs[0].StartsWith("-line="))
                    int.TryParse(supportedArgs[0].Substring("-line=".Length), out openFileLine);
                foreach (string file in files)
                {
                    Buffer buffer = LoadFile(file);
                    if (openFileLine != 0)
                    {
                        Place place = new Place(0, openFileLine - 1);
                        buffer.Controller.PutCursor(place, false);
                        buffer.Controller.NeedScrollToCaret();
                    }
                }
                openFileLine = 0;
            }
            else
            {
                MessageBox.Show(String.Format("Unrecognized data type = {0}.", 
                dataType), "SendMessageDemo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            base.WndProc(ref m);
        }
    }
    
    private void CloseOldBuffers()
    {
   		CloseOldBuffers(mainNest);
   		CloseOldBuffers(mainNest2);
    }
    
    private void CloseOldBuffers(Nest nest)
    {
    	if (nest.buffers == null)
    		return;
    	
    	int count = nest.buffers.list.Count;
    	int filesCount = 0;
		for (int i = 0; i < count; i++)
		{
			Buffer buffer = nest.buffers.list[i];
			if ((buffer.tags & BufferTag.File) == BufferTag.File)
				filesCount++;
		}
		int countToRemove = filesCount - settings.maxTabsCount.Value;
		if (countToRemove < 0)
			return;
		
		List<Buffer> buffers = new List<Buffer>(nest.buffers.list.History);
		List<Buffer> buffersToRemove = new List<Buffer>();
		for (int i = buffers.Count; i-- > 0;)
		{
			if (countToRemove <= 0)
				break;
			Buffer buffer = buffers[i];
			if ((buffer.tags & BufferTag.File) == BufferTag.File && !buffer.Changed && !buffer.needSaveAs)
			{
				countToRemove--;
				buffersToRemove.Add(buffer);
			}
		}
		foreach (Buffer buffer in buffersToRemove)
		{
			if (buffer.Frame != null)
				buffer.Frame.RemoveBuffer(buffer);
		}
    }
}
