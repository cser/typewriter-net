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

namespace TypewriterNET
{
	public class MainForm : Form
	{
		private TableLayoutPanel table;
	    private MulticaretTextBox textBox;
	    private MainMenu mainMenu;
	    private TabInfoList fileList;
	    private TabBar<TabInfo> tabBar;
	    private List<KeyAction> actions;
	    private MainContext context;
	    private ConsoleListController consoleListController;
	    private Config config;
	    private EditorHighlighterSet highlightingSet;
	    private string[] args;
		private SyntaxFilesScanner syntaxFilesScanner;
	
	    public MainForm(string[] args)
	    {
	    	this.args = args;
	    	
	    	AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
	        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	        ClientSize = new System.Drawing.Size(700, 480);
	        ImeMode = System.Windows.Forms.ImeMode.Hiragana;
	        
	    	ResourceManager manager = new ResourceManager("TypewriterNET", typeof(Program).Assembly);
	    	Icon = (Icon)manager.GetObject("icon");
	    	Name = Application.ProductName;
	        Text = Name;
	        
	        SuspendLayout();
	        
	        context = new MainContext();
	    	
	    	table = new TableLayoutPanel();
	    	table.Dock = DockStyle.Fill;
	    	table.BorderStyle = BorderStyle.None;
	    	table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
	        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
	        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
	    	Controls.Add(table);
	    	
	    	fileList = new TabInfoList();
	    	
	    	tabBar = new TabBar<TabInfo>(fileList, TabInfo.TabStringOf);
			tabBar.Dock = DockStyle.Top;
			tabBar.Margin = new Padding();
			tabBar.CloseClick += OnTabCloseClick;
			tabBar.TabDoubleClick += OnTabDoubleClick;
			table.Controls.Add(tabBar, 0, 0);
	    	
	        textBox = new MulticaretTextBox();
	        textBox.Dock = DockStyle.Fill;
	        textBox.Margin = new Padding();
	        textBox.Height = 20;
			textBox.AllowDrop = true;
			textBox.DragEnter += OnDragEnter;
			textBox.DragDrop += OnDragDrop;
	        table.Controls.Add(textBox, 0, 1);
	        
	        BuildMenu();
	        
	        consoleListController = new ConsoleListController(table, context);
	        context.consoleListController = consoleListController;
	        context.textBox = textBox;
	        highlightingSet = new EditorHighlighterSet(context);
	        
	        ResumeLayout(false);
	        PerformLayout();

	        
			Load += OnLoad;
	    }
	    
	    private void OnLoad(object sender, EventArgs e)
	    {
	    	config = new Config();
			syntaxFilesScanner = new SyntaxFilesScanner(new string[]{ Path.Combine(AppPath.appDataDir, AppPath.Syntax), Path.Combine(AppPath.startupDir, AppPath.Syntax) });
			syntaxFilesScanner.Rescan();
	    	highlightingSet.UpdateParameters(syntaxFilesScanner);
	        ReloadConfig();

			fileList.SelectedChange += OnTabSelected;
			FormClosing += OnFormClosing;
			Activated += OnActivated;
			
	    	LoadState();
			if (args.Length == 1)
			{
				LoadFile(args[0]);
			}
			else if (fileList.Count == 0)
			{
				TabInfo info = NewFile();
				info.first = true;
			}
	    }
	    
	    private void BuildMenu()
	    {
	    	KeyMap keyMap = new KeyMap();
	    	KeyMap doNothingKeyMap = new KeyMap();
	    	context.keyMap = keyMap;
	    	textBox.parentKeyMaps.Add(keyMap);
	    	textBox.parentKeyMaps.Add(doNothingKeyMap);
	    	
	    	actions = new List<KeyAction>();
	    	
	    	doNothingKeyMap.AddItem(new KeyItem(Keys.Escape, null, KeyAction.Nothing));
	        
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, AddAction("&File\\New", DoNew, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.O, null, AddAction("&File\\Open", DoOpen, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.S, null, AddAction("&File\\Save", DoSave, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.S, null, AddAction("&File\\Save As", DoSaveAs, null, false)));
	        AddAction("&File\\-", null, null, false);
	        keyMap.AddItem(new KeyItem(Keys.Alt | Keys.F4, null, AddAction("&File\\Exit", DoExit, null, false)));
	        
	        foreach (KeyAction action in KeyAction.Actions)
	        {
	        	actions.Add(action);
	        }
	       	
	        keyMap.AddItem(new KeyItem(Keys.Tab, Keys.Control, AddAction("&View\\Switch tab", DoTabDown, DoTabModeChange, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.W, null, AddAction("&View\\Close tab", DoCloseTab, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.Oemtilde, null, AddAction("&View\\Show/hide editor console", DoShowHideConsole, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null, AddAction("F&ind\\Find...", DoFind, null, false)));
	        
	        keyMap.AddItem(new KeyItem(Keys.F2, null, AddAction("Prefere&nces\\Edit config", DoOpenUserConfig, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Shift | Keys.F2, null, AddAction("Prefere&nces\\Open base config", DoOpenBaseConfig, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.F2, null, AddAction("Prefere&nces\\Edit current scheme", DoOpenCurrentScheme, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.F2, null, AddAction("Prefere&nces\\Open current scheme all files", DoOpenCurrentScheme, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.F3, null, AddAction("Prefere&nces\\Open AppDdata folder", DoOpenAppDataFolder, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.None, null, AddAction("Prefere&nces\\New syntax file", DoNewSyntax, null, false)));
	        
	        keyMap.AddItem(new KeyItem(Keys.F1, null, AddAction("&?\\About", DoAbout, null, false)));
	        
	        keyMap.AddItem(new KeyItem(Keys.Escape, null, AddAction("&View\\Close search", DoCloseSearch, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Escape, null, AddAction("&View\\Close editor console", DoCloseEditorConsole, null, false)));
	        
	        mainMenu = new MainMenu();
	    	Menu = mainMenu;
	        Dictionary<KeyAction, List<KeyItem>> keysByAction = new Dictionary<KeyAction, List<KeyItem>>();
			List<KeyItem> keyItems = new List<KeyItem>(textBox.KeyMap.items);
			foreach (KeyMap keyMapI in textBox.parentKeyMaps)
			{
				keyItems.AddRange(keyMapI.items);
			}
	        foreach (KeyItem keyItem in keyItems)
	        {
	        	List<KeyItem> list;
	        	keysByAction.TryGetValue(keyItem.action, out list);
	        	if (list == null)
	        	{
	        		list = new List<KeyItem>();
	        		keysByAction[keyItem.action] = list;
	        	}
	        	list.Add(keyItem);
	        }
	        Dictionary<string, Menu> itemByPath = new Dictionary<string, Menu>();
	        KeysConverter keysConverter = new KeysConverter();
	        foreach (KeyAction action in actions)
	        {
	        	string name = GetMenuItemName(action.name);
	        	List<KeyItem> keys;
	        	keysByAction.TryGetValue(action, out keys);
	        	if (keys != null && keys.Count > 0)
	        	{
	        		name += "\t";
	        		bool first = true;
	        		foreach (KeyItem keyItem in keys)
	        		{
	        			if (!first)
	        				name += "/";
	        			first = false;
	        			if (action.doOnModeChange != null)
	        				name += "[";
	        			name += keysConverter.ConvertToString(keyItem.keys);
	        			if (action.doOnModeChange != null)
	        				name += "]";
	        		}
	        	}
	        	MenuItem item = new MenuItem(name, new MenuItemActionDelegate(action, fileList).OnClick);
	        	GetMenuItemParent(action.name, itemByPath).MenuItems.Add(item);
	        }
	    }

	    private AppPath GetSchemePath(string schemeName)
	    {
			return new AppPath(Path.Combine(AppPath.Schemes, schemeName + ".xml"));
	    }
	    
	    private List<AppPath> GetSchemePaths(Config config)
	    {
	    	List<AppPath> paths = new List<AppPath>();
	    	foreach (string schemeName in ParseSchemeName(config.Scheme))
	    	{
	    		paths.Add(GetSchemePath(schemeName));
	    	}
	    	return paths;
	    }

	    private List<string> GetActiveSchemePaths(Config config)
	    {
	    	List<string> paths = new List<string>();
	    	foreach (string schemeName in ParseSchemeName(config.Scheme))
	    	{
	    		paths.Add(GetSchemePath(schemeName).GetExisted());
	    	}
	    	return paths;
	    }
	    
	    private void ReloadConfig()
	    {
	    	config.Reset();
	    	
	    	StringBuilder errors = new StringBuilder();
			foreach (string pathI in AppPath.configPath.paths)
			{
				XmlDocument configXml = context.LoadXmlIgnoreMissing(pathI, errors);
				if (configXml != null)
					config.Parse(configXml, errors);
			}
			if (errors.Length > 0)
			{
				EditorConsole.Instance.WriteLine("-- Reload config errors:", Ds.Comment);
				EditorConsole.Instance.Write(errors.ToString());
				context.ShowEditorConsole();
			}
			
			textBox.WordWrap = config.WordWrap;
			textBox.ShowLineNumbers = config.ShowLineNumbers;
			textBox.ShowLineBreaks = config.ShowLineBreaks;
			textBox.HighlightCurrentLine = config.HighlightCurrentLine;
			textBox.TabSize = config.TabSize;
			textBox.LineBreak = config.LineBreak;
			textBox.FontFamily = config.FontFamily;
			textBox.FontSize = config.FontSize;
			textBox.ScrollingIndent = config.ScrollingIndent;
			textBox.ShowColorAtCursor = config.ShowColorAtCursor;
			textBox.KeyMap.SetAltChars(config.AltCharsSource, config.AltCharsResult);
			
			tabBar.SetFont(config.FontFamily, config.FontSize);
			
			consoleListController.UpdateParameters(config);
			
			RemoveSuperfluousTabs();
			ReloadScheme();
			
			foreach (TabInfo infoI in fileList)
	    	{
				UpdateHighlighter(infoI);
	    	}
	    }
	    
	    private static List<string> ParseSchemeName(string schemeName)
	    {
	    	List<string> list = new List<string>();
	    	if (!string.IsNullOrEmpty(schemeName))
	    	{
		    	int startIndex = 0;
		    	while (true)
		    	{
		    		startIndex = schemeName.IndexOf('-', startIndex);
		    		if (startIndex == -1)
		    		{
		    			list.Add(schemeName);
		    			break;
		    		}
		    		list.Add(schemeName.Substring(0, startIndex));
		    		startIndex++;
		    	}
	    	}
	    	return list;
	    }
	    
	    private void ReloadScheme()
	    {
	    	Scheme scheme = new Scheme();
	    	List<XmlDocument> xmls = new List<XmlDocument>();
	    	StringBuilder errors = new StringBuilder();
	    	foreach (AppPath schemePath in GetSchemePaths(config))
	    	{
	    		XmlDocument xml = context.LoadXml(schemePath.GetExisted(), errors);
	    		if (xml != null)
	    			xmls.Add(xml);
	    	}
	    	scheme.ParseXml(xmls);
			if (errors.Length > 0)
			{
				EditorConsole.Instance.WriteLine("-- Scheme loading errors:", Ds.Comment);
				EditorConsole.Instance.Write(errors.ToString());
				context.ShowEditorConsole();
			}
			
			textBox.Scheme = scheme;
			tabBar.Scheme = scheme;
			consoleListController.UpdateScheme(scheme);
	    }
	    
	    private bool activationInProcess = false;
	    
	    private void OnActivated(object sender, EventArgs e)
	    {
	    	if (activationInProcess)
	    		return;
	    	activationInProcess = true;
	    	
	    	foreach (TabInfo info in fileList)
	    	{
	    		if (info.fileInfo != null)
	    		{
	    			info.fileInfo.Refresh();
	    			if (info.lastWriteTimeUtc != info.fileInfo.LastWriteTimeUtc)
	    			{
	    				DialogResult result = MessageBox.Show("File was changed. Reload it?", Name, MessageBoxButtons.YesNo);
	    				if (result == DialogResult.Yes)
	    					LoadFile(info);
	    			}
	    		}
	    	}
	    	
	    	activationInProcess = false;
	    }
	    
	    private Menu GetMenuItemParent(string path, Dictionary<string, Menu> itemByPath)
	    {
	    	string parentPath = GetMenuItemParentPath(path);
	    	if (string.IsNullOrEmpty(parentPath))
	    		return mainMenu;
	    	Menu parent;
	    	itemByPath.TryGetValue(parentPath, out parent);
	    	if (parent != null)
	    		return parent;
    		MenuItem item = new MenuItem(GetMenuItemName(parentPath));
    		itemByPath[parentPath] = item;
    		GetMenuItemParent(parentPath, itemByPath).MenuItems.Add(item);
    		return item;
	    }
	    
	    private static string GetMenuItemName(string path)
	    {
	    	int index = path.LastIndexOf("\\");
	    	if (index == -1)
	    		return path;
	    	return path.Substring(index + 1);
	    }
	    
	    private static string GetMenuItemParentPath(string path)
	    {
	    	int index = path.LastIndexOf("\\");
	    	if (index == -1)
	    		return "";
	    	return path.Substring(0, index);
	    }
	    
	    private KeyAction AddAction(string name, Getter<Controller, bool> doOnDown, Setter<Controller, bool> doOnModeChange, bool needScroll)
		{
			KeyAction action = new KeyAction(name, doOnDown, doOnModeChange, needScroll);
			actions.Add(action);
			return action;
		}
	    
	    private void OnFormClosing(object sender, FormClosingEventArgs e)
	    {
	    	SaveState();
	    	foreach (TabInfo info in new List<TabInfo>(fileList))
	    	{
	    		if (!CloseTab(info))
	    		{
	    			e.Cancel = true;
	    			return;
	    		}
	    	}
	    }
	    
	    private bool DoTabDown(Controller controller)
	    {
	    	fileList.Down();
	    	return true;
	    }
	    
	    private void DoTabModeChange(Controller controller, bool mode)
	    {
	    	if (mode)
	    		fileList.ModeOn();
	    	else
	    		fileList.ModeOff();
	    }
	    
	    private bool DoCloseTab(Controller controller)
	    {
	    	CloseTab(fileList.Selected);
	    	return true;
	    }
	    
	    private void OnTabCloseClick()
	    {
	    	CloseTab(fileList.Selected);
	    }
	    
	    private void OnTabDoubleClick(TabInfo info)
	    {
	    	CloseTab(info);
	    }
	    
	    private bool DoNew(Controller controller)
	    {
	    	NewFile();
	    	return true;
	    }
	    
	    private bool DoOpen(Controller controller)
	    {
	    	OpenFileDialog dialog = new OpenFileDialog();
	    	if (dialog.ShowDialog() == DialogResult.OK)
	    	{
	    		LoadFile(dialog.FileName);
	    	}
	    	return true;
	    }
	    
	    private bool DoSave(Controller controller)
	    {
	    	TrySaveFile(fileList.Selected);
	    	return true;
	    }
	    
	    private bool DoSaveAs(Controller controller)
	    {
	    	TabInfo info = fileList.Selected;
	    	if (info != null)
	    	{
		    	SaveFileDialog dialog = new SaveFileDialog();
		    	dialog.FileName = info.Name;
		    	dialog.InitialDirectory = Path.GetDirectoryName(info.FullPath);
		    	if (dialog.ShowDialog() == DialogResult.OK)
		    	{
		    		info.SetFile(Path.GetFullPath(dialog.FileName), Path.GetFileName(dialog.FileName));
		    		SaveFile(info);
		    	}
	    	}
	    	return true;
	    }
	    
	    private bool DoExit(Controller controller)
	    {
	    	Close();
	    	return true;
	    }
	    
	    private bool DoAbout(Controller controller)
	    {
	    	foreach (TabInfo infoI in fileList)
	    	{
	    		if (infoI.Tag == "About")
	    		{
	    			if (fileList.Selected == infoI)
	    			{
	    				fileList.Remove(infoI);
	    			}
	    			else
	    			{
	    				fileList.Selected = infoI;
	    			}
	    			return true;
	    		}
	    	}
	    	string text = "# About\n" +
	    		"\n" +
	    		Application.ProductName + "\n" +
	    		"Build " + Application.ProductVersion;
	    	TabInfo info = OpenTab(null, "About.twh");
	    	info.Tag = "About";
	    	info.Controller.InitText(text);
	    	return true;
	    }

		private void CreateAppDataFolders()
		{
			if (!Directory.Exists(AppPath.syntaxDir.appDataPath))
				Directory.CreateDirectory(AppPath.syntaxDir.appDataPath);
			if (!Directory.Exists(AppPath.schemesDir.appDataPath))
				Directory.CreateDirectory(AppPath.schemesDir.appDataPath);
		}
	    
	    private bool DoOpenUserConfig(Controller controller)
	    {
			if (!File.Exists(AppPath.configPath.appDataPath))
				File.Copy(AppPath.configTemplatePath, AppPath.configPath.appDataPath);
			LoadFile(AppPath.configPath.appDataPath);
	    	return true;
	    }

	    private bool DoOpenBaseConfig(Controller controller)
	    {
			LoadFile(AppPath.configPath.startupPath);
	    	return true;
	    }
	    
	    private bool DoOpenCurrentScheme(Controller controller)
	    {
	    	List<AppPath> paths = GetSchemePaths(config);
	    	if (paths.Count > 0)
	    	{
				AppPath last = paths[paths.Count - 1];
				if (!File.Exists(last.appDataPath) && File.Exists(last.startupPath))
				{
					string dir = Path.GetDirectoryName(last.appDataPath);
					CreateAppDataFolders();
					File.Copy(last.startupPath, last.appDataPath);
				}
				LoadFile(last.appDataPath);
	    	}
	    	return true;
	    }
		
	    private bool DoOpenCurrentSchemeAllFiles(Controller controller)
	    {
	    	List<string> paths = GetActiveSchemePaths(config);
	    	if (paths.Count > 0)
	    	{
				foreach (string path in paths)
				{
					LoadFile(path);
				}
	    	}
	    	return true;
	    }

	    private bool DoNewSyntax(Controller controller)
	    {
			CreateAppDataFolders();
	    	string templatePath = Path.Combine(AppPath.syntaxDir.startupPath, "syntax.template");
	    	string filePath = Path.Combine(AppPath.syntaxDir.appDataPath, "new-syntax.xml");
	    	TabInfo info = OpenTab(templatePath, Path.GetFileName(templatePath));
	    	LoadFile(info);
	    	info.SetFile(filePath, Path.GetFileName(filePath));
	    	info.needSaveAs = true;
	    	UpdateHighlighter(info);
	    	return true;
	    }

		private bool DoOpenAppDataFolder(Controller controller)
		{
			CreateAppDataFolders();
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = AppPath.appDataDir;
			process.Start();
			return true;
		}
	    
	    private bool DoShowHideConsole(Controller controller)
	    {
	    	if (consoleListController.Visible)
	    	{
	    		if (consoleListController.SelectedConsole == EditorConsole.Instance)
	    		{
	    			consoleListController.RemoveConsole(EditorConsole.Instance);
	    			consoleListController.Hide();
	    		}
	    		else
	    		{
	    			consoleListController.AddConsole(EditorConsole.Instance);
	    			consoleListController.SelectedConsole = EditorConsole.Instance;
	    		}
	    	}
	    	else
	    	{
	    		consoleListController.Show(context.SetFocusToTextBox);
    			consoleListController.AddConsole(EditorConsole.Instance);
    			consoleListController.SelectedConsole = EditorConsole.Instance;
	    	}
	    	return true;
	    }
	    
	    public bool DoCloseEditorConsole(Controller controller)
	    {
	    	if (consoleListController.Visible)
	    	{
	    		consoleListController.Hide();
	    		return true;
	    	}
	    	return false;
	    }
	    
	    public void LoadFile(string file)
	    {
	    	string fullPath = Path.GetFullPath(file);
	    	TabInfo info = fileList.GetByFullPath(fullPath);
	    	if (info != null)
	    	{
	    		fileList.Selected = info;
	    		return;
	    	}
	    	
	    	info = OpenTab(fullPath, Path.GetFileName(file));
	    	LoadFile(info);
	    	if (fileList.Count == 2)
	    	{
	    		TabInfo infoToRemove = fileList[0];
	    		if (infoToRemove.first)
	    		{
		    		Controller controller = infoToRemove.Controller;
		    		if (controller.Lines.charsCount == 0 && !controller.history.CanUndo && !controller.history.CanRedo && controller.history.tags.Count <= 1)
		    			CloseTab(infoToRemove);
	    		}
	    	}
	    }
		
	    public void LoadFile(TabInfo info)
	    {
	    	if (!File.Exists(info.FullPath))
	    	{
	    		EditorConsole.Instance.Write("Missing file: ", Ds.Keyword);
	    		EditorConsole.Instance.WriteLine(info.FullPath, Ds.Normal);
				context.ShowEditorConsole();
				return;
	    	}
	    	string text = "";
	    	try
	    	{
	    		text = File.ReadAllText(info.FullPath);
	    	}
	    	catch (IOException e)
	    	{
				EditorConsole.Instance.WriteLine("-- File loading errors:", Ds.Comment);
				EditorConsole.Instance.WriteLine(e.Message + "\n" + e.StackTrace);
				context.ShowEditorConsole();
	    	}
	    	info.Controller.InitText(text);
	    	info.fileInfo = new FileInfo(info.FullPath);
	    	info.lastWriteTimeUtc = info.fileInfo.LastWriteTimeUtc;
	    }
	    
	    public void SaveFile(TabInfo info)
	    {
	    	File.WriteAllText(info.FullPath, info.Controller.Lines.GetText());
	    	info.Controller.history.MarkAsSaved();
	    	info.fileInfo = new FileInfo(info.FullPath);
	    	info.lastWriteTimeUtc = info.fileInfo.LastWriteTimeUtc;
	    	info.needSaveAs = false;
	    	tabBar.Invalidate();
	    	
	    	if (AppPath.configPath.HasPath(info.FullPath))
	    	{
	    		ReloadConfig();
	    	}
	    	else if (GetActiveSchemePaths(config).IndexOf(info.FullPath) != -1)
	    	{
	    		ReloadScheme();
	    	}
	    	
	    	UpdateHighlighter(info);
	    }
	    
	    private void OnTabSelected()
	    {
	    	TabInfo info = fileList.Selected;
	    	if (info == null)
	    	{
	    		NewFile();
	    		info = fileList.Selected;
	    		if (info != null)
	    			info.first = true;
	    		return;
	    	}
	    	textBox.Controller = info.Controller;
	    	UpdateHighlighter(info);
	    	Name = !string.IsNullOrEmpty(info.FullPath) ? info.FullPath + " - " + Application.ProductName : Application.ProductName;
	    	Text = Name;
	    }
	    
	    private void UpdateHighlighter(TabInfo info)
	    {
	    	string syntax = syntaxFilesScanner.GetSyntaxByFile(info.Name);
	    	string extension = info.Name.ToLowerInvariant();
			textBox.Highlighter = syntax != null ? highlightingSet.GetHighlighter(syntax) : null;
	    }
	    
	    private void OnChangedChange()
	    {
	    	tabBar.Invalidate();
	    }
	    
	    private TabInfo NewFile()
	    {
	    	TabInfo info = OpenTab(null, "Untitled.txt");
	    	return info;
	    }
	    
	    private TabInfo OpenTab(string fullPath, string name)
	    {
	    	TabInfo info = new TabInfo(fullPath, name);
	    	info.Controller.history.ChangedChange += OnChangedChange;
	    	fileList.Add(info);
	    	RemoveSuperfluousTabs();
	    	return info;
	    }
	    
	    private bool CloseTab(TabInfo info)
	    {
	    	if (info != null)
	    	{
	    		if (info.Controller.history.Changed)
	    		{
	    			DialogResult result = MessageBox.Show("Do you want to save the current changes in\n" + info.Name + "?", Name, MessageBoxButtons.YesNoCancel);
	    			switch (result)
	    			{
	    				case DialogResult.Yes:
	    					TrySaveFile(info);
	    					info.Controller.history.ChangedChange -= OnChangedChange;
			    			fileList.Remove(info);
	    					return true;
	    				case DialogResult.No:
	    					info.Controller.history.ChangedChange -= OnChangedChange;
			    			fileList.Remove(info);
	    					return true;
	    				case DialogResult.Cancel:
	    					return false;
	    			}
	    		}
	    		else
	    		{
	    			info.Controller.history.ChangedChange -= OnChangedChange;
			    	fileList.Remove(info);
			    	return true;
	    		}
	    	}
	    	return false;
	    }
	    
	    private void TrySaveFile(TabInfo info)
	    {
	    	if (info == null)
	    		return;
	    	if (!string.IsNullOrEmpty(info.FullPath) && !info.needSaveAs)
	    	{
	    		SaveFile(info);
	    		return;
	    	}
	    	SaveFileDialog dialog = new SaveFileDialog();
	    	dialog.FileName = info.Name;
	    	if (!string.IsNullOrEmpty(info.FullPath))
	    		dialog.InitialDirectory = Path.GetDirectoryName(info.FullPath);
	    	if (dialog.ShowDialog() == DialogResult.OK)
	    	{
	    		info.SetFile(Path.GetFullPath(dialog.FileName), Path.GetFileName(dialog.FileName));
	    		SaveFile(info);
	    	}
	    }
	    
	    private void RemoveSuperfluousTabs()
	    {
	    	if (fileList.Count > config.MaxTabsCount)
	    	{
	    		for (int i = fileList.Count - config.MaxTabsCount; i-- > 0;)
	    		{
	    			CloseTab(fileList.Oldest);
	    		}
	    	}
	    }
	    
	    public class MenuItemActionDelegate
	    {
	    	private KeyAction action;
	    	private SwitchList<TabInfo> fileList;
	    	
	    	public MenuItemActionDelegate(KeyAction action, SwitchList<TabInfo> fileList)
	    	{
	    		this.action = action;
	    		this.fileList = fileList;
	    	}
	    	
	    	public void OnClick(object sender, EventArgs e)
	    	{
	    		TabInfo info = fileList.Selected;
	    		if (info != null)
	    		{
	    			if (action.doOnModeChange != null)
	    				action.doOnModeChange(info.Controller, true);
	    			action.doOnDown(info.Controller);
	    			if (action.doOnModeChange != null)
	    				action.doOnModeChange(info.Controller, false);
	    		}
	    	}
	    }
	    
	    //-----------------------------------------------------
	    // Drag & Drop
	    //-----------------------------------------------------
	    
	    private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
        }
		
		private void OnDragDrop(object sender, DragEventArgs e)
        {
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
				if (files != null)
				{
					foreach (string fileI in files)
					{
						FileAttributes attributes = File.GetAttributes(fileI);
						if ((attributes & FileAttributes.Directory) > 0)
						{
							foreach (string fileJ in Directory.GetFiles(fileI, "*.*", SearchOption.AllDirectories))
							{
								LoadFile(fileJ);
							}
						}
						else
						{
							LoadFile(fileI);
						}
					}
				}
			}
        }
		
		//-----------------------------------------------------
	    // Search
	    //-----------------------------------------------------
	    
	    private SearchPanel searchPanel;
	    
	    private bool DoFind(Controller controller)
	    {
	    	if (searchPanel == null)
	    	{
	    		searchPanel = new SearchPanel(context);
	    		table.Controls.Add(searchPanel, 0, 2);
	    	}
	    	searchPanel.Focus();
	    	return true;
	    }
	    
	    private bool DoCloseSearch(Controller controller)
	    {
	    	if (searchPanel != null)
	    	{
	    		searchPanel.DoOnClose();
	    		table.Controls.Remove(searchPanel);
	    		searchPanel.Dispose();
	    		searchPanel = null;
	    		textBox.Focus();
	    		return true;
	    	}
	    	return false;
	    }
	    
	    //----------------------------
	    // State
	    //----------------------------
	    
	    private static string GetStatePath()
	    {
	    	return Path.Combine(Path.GetTempPath(), "typewriter-state.bin");
	    }
	    
	    private void LoadState()
	    {
	    	SValue state = SValue.None;
	    	string file = GetStatePath();
	    	if (File.Exists(file))
	    		state = SValue.Unserialize(File.ReadAllBytes(file));
	    	
	    	Size = new Size(state["width"].GetInt(700), state["height"].GetInt(480));
	    	Location = new Point(state["x"].Int, state["y"].Int);
	    	WindowState = state["maximized"].GetBool(false) ? FormWindowState.Maximized : FormWindowState.Normal;
	    	if (config.RememberOpenedFiles)
	    	{
		    	foreach (SValue valueI in state["openedTabs"].List)
		    	{
		    		string fullPath = valueI["fullPath"].String;
		    		if (fullPath != "" && File.Exists(fullPath))
		    			LoadFile(fullPath);
		    	}
	    	}
	    }
	    
	    private void SaveState()
	    {
	    	SValue state = SValue.NewHash();
	    	if (WindowState == FormWindowState.Maximized)
	    	{
		    	state["width"] = SValue.NewInt(RestoreBounds.Width);
		    	state["height"] = SValue.NewInt(RestoreBounds.Height);
		    	state["x"] = SValue.NewInt(RestoreBounds.X);
	    		state["y"] = SValue.NewInt(RestoreBounds.Y);
	    	}
	    	else
	    	{
	    		state["width"] = SValue.NewInt(Width);
		    	state["height"] = SValue.NewInt(Height);
		    	state["x"] = SValue.NewInt(Location.X);
	    		state["y"] = SValue.NewInt(Location.Y);
	    	}
	    	state["maximized"] = SValue.NewBool(WindowState == FormWindowState.Maximized);
	    	if (config.RememberOpenedFiles)
	    	{
		    	SValue openedTabs = state.SetNewList("openedTabs");
		    	foreach (TabInfo tabInfoI in fileList)
		    	{
		    		openedTabs.Add(SValue.NewHash().With("fullPath", SValue.NewString(tabInfoI.FullPath)));
		    	}
	    	}
	    	
	    	File.WriteAllBytes(GetStatePath(), SValue.Serialize(state));
	    }
	}
}
