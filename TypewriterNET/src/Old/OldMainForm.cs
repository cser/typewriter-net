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
using TypewriterNET.Frames;

namespace TypewriterNET
{
	public class OldMainForm : Form, ISearchableFrame, IMainContext
	{
		private TableLayoutPanel table;
	    private MulticaretTextBox textBox;
	    private OldMainFormMenu mainMenu;
	    private TabInfoList fileList;
	    private TabBar<TabInfo> tabBar;
	    private ConsoleListController consoleListController;
	    private Config config;
	    private EditorHighlighterSet highlightingSet;
	    private string[] args;
		private SyntaxFilesScanner syntaxFilesScanner;
		private TabBar<string> consoleBar;
	
	    public OldMainForm(string[] args)
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
			textBox.GotFocus += OnGotFocus;
	        table.Controls.Add(textBox, 0, 1);

			consoleBar = new TabBar<string>(new SwitchList<string>(), TabBar<string>.DefaultStringOf);
			consoleBar.Margin = new Padding();
			consoleBar.Dock = DockStyle.Bottom;
			consoleBar.Text = "Some console";
			consoleBar.List.Add("Editor console");
			consoleBar.List.Add("Console 1");
			consoleBar.List.Add("Console 2");
			consoleBar.List.Add("Console 3");
			consoleBar.List.Add("Console 4");
			consoleBar.List.Add("Console 5");
			table.Controls.Add(consoleBar, 0, 2);

			searchFrame = new SearchFrame(this);
			mainMenu = new OldMainFormMenu(fileList);
			Menu = mainMenu;
	        
	        BuildMenu();
			SetMenuItems(textBox.KeyMap);
	        
	        consoleListController = new ConsoleListController(table, this);
	        highlightingSet = new EditorHighlighterSet(this);
	        
	        ResumeLayout(false);
	        PerformLayout();
	        
			Load += OnLoad;
	    }

		private void OnGotFocus(object sender, EventArgs e)
		{
			mainMenu.node = textBox.KeyMap;
		}

		public MulticaretTextBox TextBox { get { return textBox; } }

		public void AddSearchPanel(Control control)
		{
	        table.Controls.Add(control, 0, 2);
		}

		public void RemoveSearchPanel(Control control)
		{
	        table.Controls.Remove(control);
		}

		//----------------------------------------------------------------------
		// IMainContext
		//----------------------------------------------------------------------

		public void SetMenuItems(KeyMapNode node)
		{
			SuspendLayout();
			mainMenu.node = node;
			ResumeLayout();
		}

		public XmlDocument LoadXml(string file, StringBuilder errors)
	    {
	    	if (!File.Exists(file))
	    	{
	    		errors.AppendLine("Missing file: " + file);
	    		return null;
	    	}
			return PrivateLoadXml(file, errors);
	    }

		public XmlDocument LoadXmlIgnoreMissing(string file, StringBuilder errors)
		{
			if (!File.Exists(file))
	    		return null;
			return PrivateLoadXml(file, errors);
		}

		private XmlDocument PrivateLoadXml(string file, StringBuilder errors)
		{
	    	try
	    	{
	    		XmlDocument xml = new XmlDocument();
	    		xml.Load(file);
	    		return xml;
	    	}
	    	catch (Exception e)
	    	{
	    		errors.AppendLine("Error: " + e.Message);
	    		return null;
	    	}
		}
		
		public void ShowEditorConsole()
	    {
	    	consoleListController.Show(SetFocusToTextBox);
	    	consoleListController.AddConsole(EditorConsole.Instance);
	    	consoleListController.SelectedConsole = EditorConsole.Instance;
	    }
		
		public void SetFocusToTextBox()
	    {
	    	textBox.Focus();
	    }

		//----------------------------------------------------------------------
		//
		//----------------------------------------------------------------------
	    
	    private void OnLoad(object sender, EventArgs e)
	    {
			string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypewriterNET");
			if (!Directory.Exists(appDataPath))
				Directory.CreateDirectory(appDataPath);
			OldAppPath.Init(Application.StartupPath, appDataPath);

	    	config = new Config();
			syntaxFilesScanner = new SyntaxFilesScanner(new string[]{ Path.Combine(OldAppPath.AppDataDir, OldAppPath.Syntax), Path.Combine(OldAppPath.StartupDir, OldAppPath.Syntax) });
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
	    	textBox.KeyMap.AddAfter(keyMap);
	    	textBox.KeyMap.AddAfter(doNothingKeyMap, -1);
	    	
	    	doNothingKeyMap.AddItem(new KeyItem(Keys.Escape, null, KeyAction.Nothing));
	    	doNothingKeyMap.AddItem(new KeyItem(Keys.Escape | Keys.Shift, null, KeyAction.Nothing));
	        
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, new KeyAction("&File\\New", DoNew, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.O, null, new KeyAction("&File\\Open", DoOpen, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.S, null, new KeyAction("&File\\Save", DoSave, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.Shift | Keys.S, null, new KeyAction("&File\\Save As", DoSaveAs, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.None, null, new KeyAction("&File\\-", null, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Alt | Keys.F4, null, new KeyAction("&File\\Exit", DoExit, null, false)));
	        
	        keyMap.AddItem(new KeyItem(Keys.Tab, Keys.Control, new KeyAction("&View\\Switch tab", DoTabDown, DoTabModeChange, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.W, null, new KeyAction("&View\\Close tab", DoCloseTab, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.Oemtilde, null, new KeyAction("&View\\Show/hide editor console", DoShowHideConsole, null, false)));
	        keyMap.AddItem(new KeyItem(Keys.Control | Keys.D1, null, new KeyAction("&View\\Change focus", DoChangeFocus, null, false)));
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

	    private OldAppPath GetSchemePath(string schemeName)
	    {
			return new OldAppPath(Path.Combine(OldAppPath.Schemes, schemeName + ".xml"));
	    }
	    
	    private List<OldAppPath> GetSchemePaths(Config config)
	    {
	    	List<OldAppPath> paths = new List<OldAppPath>();
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
			foreach (string pathI in OldAppPath.ConfigPath.paths)
			{
				XmlDocument configXml = LoadXmlIgnoreMissing(pathI, errors);
				if (configXml != null)
					config.Parse(configXml, errors);
			}
			if (errors.Length > 0)
			{
				EditorConsole.Instance.WriteLine("-- Reload config errors:", Ds.Comment);
				EditorConsole.Instance.Write(errors.ToString());
				ShowEditorConsole();
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
			textBox.KeyMap.main.SetAltChars(config.AltCharsSource, config.AltCharsResult);
			
			tabBar.SetFont(config.FontFamily, config.FontSize);
			consoleBar.SetFont(config.FontFamily, config.FontSize);
			
			consoleListController.UpdateParameters(config);
			fileQualitiesStorage.MaxCount = config.MaxFileQualitiesCount;
			
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
	    	foreach (OldAppPath schemePath in GetSchemePaths(config))
	    	{
	    		XmlDocument xml = LoadXml(schemePath.GetExisted(), errors);
	    		if (xml != null)
	    			xmls.Add(xml);
	    	}
	    	scheme.ParseXml(xmls);
			if (errors.Length > 0)
			{
				EditorConsole.Instance.WriteLine("-- Scheme loading errors:", Ds.Comment);
				EditorConsole.Instance.Write(errors.ToString());
				ShowEditorConsole();
			}
			
			BackColor = scheme.bgColor;
			textBox.Scheme = scheme;
			tabBar.Scheme = scheme;
			consoleBar.Scheme = scheme;
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
			if (!Directory.Exists(OldAppPath.SyntaxDir.appDataPath))
				Directory.CreateDirectory(OldAppPath.SyntaxDir.appDataPath);
			if (!Directory.Exists(OldAppPath.SchemesDir.appDataPath))
				Directory.CreateDirectory(OldAppPath.SchemesDir.appDataPath);
		}
	    
	    private bool DoOpenUserConfig(Controller controller)
	    {
			if (!File.Exists(OldAppPath.ConfigPath.appDataPath))
				File.Copy(OldAppPath.ConfigTemplatePath, OldAppPath.ConfigPath.appDataPath);
			LoadFile(OldAppPath.ConfigPath.appDataPath);
	    	return true;
	    }

	    private bool DoOpenBaseConfig(Controller controller)
	    {
			LoadFile(OldAppPath.ConfigPath.startupPath);
	    	return true;
	    }
	    
	    private bool DoOpenCurrentScheme(Controller controller)
	    {
	    	List<OldAppPath> paths = GetSchemePaths(config);
	    	if (paths.Count > 0)
	    	{
				OldAppPath last = paths[paths.Count - 1];
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
	    	string templatePath = Path.Combine(OldAppPath.SyntaxDir.startupPath, "syntax.template");
	    	string filePath = Path.Combine(OldAppPath.SyntaxDir.appDataPath, "new-syntax.xml");
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
			process.StartInfo.FileName = OldAppPath.AppDataDir;
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
	    		consoleListController.Show(SetFocusToTextBox);
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
				ShowEditorConsole();
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
				ShowEditorConsole();
	    	}
	    	info.Controller.InitText(text);
			int caret = fileQualitiesStorage.Get(info.FullPath)["cursor"].Int;
			info.Controller.PutCursor(info.Controller.SoftNormalizedPlaceOf(caret), false);
			info.Controller.NeedScrollToCaret();
	    	info.fileInfo = new FileInfo(info.FullPath);
	    	info.lastWriteTimeUtc = info.fileInfo.LastWriteTimeUtc;
	    }
	    
	    public void SaveFile(TabInfo info)
	    {
			try
			{
				File.WriteAllText(info.FullPath, info.Controller.Lines.GetText());
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
	    	info.Controller.history.MarkAsSaved();
	    	info.fileInfo = new FileInfo(info.FullPath);
	    	info.lastWriteTimeUtc = info.fileInfo.LastWriteTimeUtc;
	    	info.needSaveAs = false;
	    	tabBar.Invalidate();
	    	
	    	if (OldAppPath.ConfigPath.HasPath(info.FullPath))
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
							DestroyTab(info);
	    					return true;
	    				case DialogResult.No:
							DestroyTab(info);
	    					return true;
	    				case DialogResult.Cancel:
	    					return false;
	    			}
	    		}
	    		else
	    		{
					DestroyTab(info);
			    	return true;
	    		}
	    	}
	    	return false;
	    }

		private void DestroyTab(TabInfo info)
		{
			info.Controller.history.ChangedChange -= OnChangedChange;
			StorageQualities(info);
			fileList.Remove(info);
		}

		private void StorageQualities(TabInfo info)
		{
			fileQualitiesStorage.Set(info.FullPath).With("cursor", SValue.NewInt(info.Controller.Lines.LastSelection.caret));
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
		
	    private SearchFrame searchFrame;
	    
	    private bool DoFind(Controller controller)
	    {
			ISearchableFrame parent = this;
			if (consoleListController.TextBox.Focused)
				parent = consoleListController;
			if (!searchFrame.Opened || searchFrame.Parent != parent)
				searchFrame.AddTo(parent);
			else
				searchFrame.Remove();
	    	return true;
	    }

		private bool DoChangeFocus(Controller controller)
		{
			if (!textBox.Focused)
			{
				textBox.Focus();
				return true;
			}
			else if (consoleListController.Visible && !consoleListController.TextBox.Focused)
			{
				consoleListController.TextBox.Focus();
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

		private FileQualitiesStorage fileQualitiesStorage = new FileQualitiesStorage();
	    
	    private void LoadState()
	    {
	    	SValue state = SValue.None;
	    	string file = GetStatePath();
	    	if (File.Exists(file))
	    		state = SValue.Unserialize(File.ReadAllBytes(file));
	    	
	    	Size = new Size(state["width"].GetInt(700), state["height"].GetInt(480));
	    	Location = new Point(state["x"].Int, state["y"].Int);
	    	WindowState = state["maximized"].GetBool(false) ? FormWindowState.Maximized : FormWindowState.Normal;
			consoleListController.AreaHeight = state["consoleAreaHeight"].GetInt(100);
			fileQualitiesStorage.Unserialize(state["fileQualitiesStorage"]);
	    	if (config.RememberOpenedFiles)
	    	{
		    	foreach (SValue valueI in state["openedTabs"].List)
		    	{
		    		string fullPath = valueI["fullPath"].String;
		    		if (fullPath != "" && File.Exists(fullPath))
		    			LoadFile(fullPath);
		    	}
				TabInfo selectedTab = fileList.GetByFullPath(state["selectedTab"]["fullPath"].String);
				if (selectedTab != null)
					fileList.Selected = selectedTab;
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
			state["consoleAreaHeight"] = SValue.NewInt(consoleListController.AreaHeight);
	    	if (config.RememberOpenedFiles)
	    	{
		    	SValue openedTabs = state.SetNewList("openedTabs");
		    	foreach (TabInfo tabInfoI in fileList)
		    	{
					SValue valueI = SValue.NewHash().With("fullPath", SValue.NewString(tabInfoI.FullPath));
		    		openedTabs.Add(valueI);
					if (tabInfoI == fileList.Selected)
						state["selectedTab"] = valueI;
		    	}
	    	}
	    	foreach (TabInfo info in new List<TabInfo>(fileList))
			{
				StorageQualities(info);
			}
			state["fileQualitiesStorage"] = fileQualitiesStorage.Serialize();
	    	
	    	File.WriteAllBytes(GetStatePath(), SValue.Serialize(state));
	    }
	}
}
