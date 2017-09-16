using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Win32;
using MulticaretEditor;

public class IncrementalSearchBase : ADialog
{
	private SwitchList<string> list;
	private TabBar<string> tabBar;
	private MulticaretTextBox variantsTextBox;
	private MulticaretTextBox textBox;
	private MulticaretTextBox filterTextBox;

	protected readonly TempSettings tempSettings;
	private string name;
	private string submenu;
	private readonly FindInFilesDialog.Data findInFilesData;
	private string lastFilter;

	public IncrementalSearchBase(TempSettings tempSettings, string name, string submenu, FindInFilesDialog.Data findInFilesData)
	{
		this.tempSettings = tempSettings;
		this.name = name;
		this.submenu = submenu;
		this.findInFilesData = findInFilesData;
	}

	override protected void DoCreate()
	{
		list = new SwitchList<string>();
		tabBar = new TabBar<string>(list, TabBar<string>.DefaultStringOf);
		tabBar.Text = name;
		tabBar.CloseClick += OnCloseClick;
		Controls.Add(tabBar);

		KeyMap textKeyMap = new KeyMap();
		KeyMap variantsKeyMap = new KeyMap();
		KeyMap beforeKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("F&ind\\" + submenu + "\\Close", DoClose, null, false);
			textKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
			variantsKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
		}
		{
			KeyAction actionUp = new KeyAction("F&ind\\" + submenu + "\\Select up", DoUp, null, false);
			KeyAction actionDown = new KeyAction("F&ind\\" + submenu + "\\Select down", DoDown, null, false);
			textKeyMap.AddItem(new KeyItem(Keys.Up, null, actionUp));
			textKeyMap.AddItem(new KeyItem(Keys.Down, null, actionDown));
			textKeyMap.AddItem(new KeyItem(Keys.Control | Keys.K, null, actionUp));
			textKeyMap.AddItem(new KeyItem(Keys.Control | Keys.J, null, actionDown));
			textKeyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null,
				new KeyAction("&View\\Vi normal mode", DoNormalMode, null, false)));
			if (findInFilesData != null)
			{
				textKeyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null,
					new KeyAction("F&ind\\Temp filter", DoSwitchToTempFilter, null, false)));
			}
		}
		{
			beforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.Home, null,
				new KeyAction("F&ind\\" + submenu + "\\Select document start", DoDocumentStart, null, false)));
			beforeKeyMap.AddItem(new KeyItem(Keys.Control | Keys.End, null,
				new KeyAction("F&ind\\" + submenu + "\\Select document end", DoDocumentEnd, null, false)));
			beforeKeyMap.AddItem(new KeyItem(Keys.PageUp, null,
				new KeyAction("F&ind\\" + submenu + "\\Select page up", DoPageUp, null, false)));
			beforeKeyMap.AddItem(new KeyItem(Keys.PageDown, null,
				new KeyAction("F&ind\\" + submenu + "\\Select page down", DoPageDown, null, false)));
		}
		{
			KeyAction action = new KeyAction("F&ind\\" + submenu + "\\Next field", DoNextField, null, false);
			textKeyMap.AddItem(new KeyItem(Keys.Tab, null, action));
			variantsKeyMap.AddItem(new KeyItem(Keys.Tab, null, action));
		}
		{
			KeyAction action = new KeyAction("F&ind\\" + submenu + "\\Open", DoExecute, null, false);
			textKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			textKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
			variantsKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			variantsKeyMap.AddItem(new KeyItem(Keys.None, null, action).SetDoubleClick(true));
		}
		
		variantsTextBox = new MulticaretTextBox();
		variantsTextBox.KeyMap.AddAfter(KeyMap);
		variantsTextBox.KeyMap.AddAfter(variantsKeyMap, 1);
		variantsTextBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		variantsTextBox.FocusedChange += OnTextBoxFocusedChange;
		variantsTextBox.Controller.isReadonly = true;
		variantsTextBox.AfterClick += OnVariantsTextBoxClick;
		variantsTextBox.AfterKeyPress += OnVariantsTextBoxClick;
		Controls.Add(variantsTextBox);

		textBox = new MulticaretTextBox(true);
		textBox.KeyMap.AddBefore(beforeKeyMap);
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(textKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		textBox.TextChange += OnTextBoxTextChange;
		Controls.Add(textBox);

		SetTextBoxParameters();

		tabBar.RightHint = tempSettings.FindParams != null ?
			tempSettings.FindParams.GetIgnoreCaseIndicationHint() : "";
		tabBar.MouseDown += OnTabBarMouseDown;
		
		if (findInFilesData != null)
		{
			KeyMap filterKeyMap = new KeyMap();
			{
				KeyAction action = new KeyAction("F&ind\\Switch to input field", DoSwitchToInputField, null, false);
				filterKeyMap.AddItem(new KeyItem(Keys.Control | Keys.E, null, action));
				filterKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
			}
			filterKeyMap.AddItem(new KeyItem(Keys.Control | Keys.F, null,
				new KeyAction("&View\\Vi normal mode", DoNormalMode, null, false)));
			if (findInFilesData.filterHistory != null)
			{
				KeyAction prevAction = new KeyAction("F&ind\\Previous filter", DoFilterPrevPattern, null, false);
				KeyAction nextAction = new KeyAction("F&ind\\Next filter", DoFilterNextPattern, null, false);
				filterKeyMap.AddItem(new KeyItem(Keys.Up, null, prevAction));
				filterKeyMap.AddItem(new KeyItem(Keys.Down, null, nextAction));
				filterKeyMap.AddItem(new KeyItem(Keys.Control | Keys.P, null, prevAction));
				filterKeyMap.AddItem(new KeyItem(Keys.Control | Keys.N, null, nextAction));
			}
			filterTextBox = new MulticaretTextBox(true);
			filterTextBox.FontFamily = FontFamily.GenericMonospace;
			filterTextBox.FontSize = 10.25f;
			filterTextBox.ShowLineNumbers = false;
			filterTextBox.HighlightCurrentLine = false;
			filterTextBox.KeyMap.AddAfter(KeyMap);
			filterTextBox.KeyMap.AddAfter(filterKeyMap, 1);
			filterTextBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
			filterTextBox.FocusedChange += OnTextBoxFocusedChange;
			filterTextBox.Visible = false;
			filterTextBox.Text = findInFilesData.currentFilter.value;
			filterTextBox.TextChange += OnFilterTextChange;
			tabBar.Controls.Add(filterTextBox);
		}
		
		InitResizing(tabBar, null);
		Height = MinSize.Height;

		Name = GetSubname();
		if (findInFilesData != null)
		{
			lastFilter = GetFilterText();
		}
		if (!Prebuild())
		{
			preventOpen = true;
			return;
		}
		UpdateVariantsText();
		UpdateFindParams();
	}

	private void OnCloseClick()
	{
		DispatchNeedClose();
	}

	override protected void DoDestroy()
	{
	}

	new public string Name
	{
		get { return list.Count > 0 ? list[0] : ""; }
		set
		{
			list.Clear();
			if (!string.IsNullOrEmpty(value))
				list.Add(value);
		}
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height + textBox.CharHeight); } }

	override public void Focus()
	{
		textBox.Focus();
	}

	private void OnTabBarMouseDown(object sender, EventArgs e)
	{
		textBox.Focus();
	}

	private void OnTextBoxFocusedChange()
	{
		if (Destroyed)
			return;
		if (findInFilesData != null)
		{
			tabBar.Selected = textBox.Focused || filterTextBox.Focused;
			if (textBox.Focused)
			{
				filterTextBox.Visible = false;
				Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
				string filter = GetFilterText();
				if (lastFilter != filter)
				{
					lastFilter = filter;
					Name = GetSubname();
					Prebuild();
					UpdateVariantsText();
					UpdateFindParams();
				}
			}
			else if (filterTextBox.Focused)
			{
				Nest.MainForm.SetFocus(filterTextBox, filterTextBox.KeyMap, null);
			}
			UpdateFindParams();
			if (filterTextBox.Focused)
			{
				int position = filterTextBox.Text.Length;
				filterTextBox.Controller.ClearMinorSelections();
				filterTextBox.Controller.LastSelection.anchor = position;
				filterTextBox.Controller.LastSelection.caret = position;
				filterTextBox.Controller.NeedScrollToCaret();
			}
		}
		else
		{
			tabBar.Selected = textBox.Focused;
			if (textBox.Focused)
				Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
		}
	}

	private void OnTextBoxTextChange()
	{
		UpdateVariantsText();
		UpdateSelectionChange();
	}

	private string pattern = "";
	private CompareInfo ci;
	
	protected string Pattern { get { return pattern; } }
	
	virtual protected int StartVariantIndex { get { return -1; } }
	
	protected int GetIndex(string text)
	{
		return ci != null ?
			ci.IndexOf(text, pattern, CompareOptions.IgnoreCase) :
			text.IndexOf(pattern);
	}
	
	protected int GetLastIndex(string text)
	{
		return ci != null ?
			ci.LastIndexOf(text, pattern, CompareOptions.IgnoreCase) :
			text.LastIndexOf(pattern);
	}
	
	private bool ignoreCase;
	
	private void UpdateVariantsText()
	{
		ignoreCase = tempSettings.FindParams.ignoreCase;
		pattern = textBox.Text;
		ci = ignoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
		InitVariantsText(GetVariantsText());
	}

	override public bool Focused { get { return textBox.Focused || variantsTextBox.Focused; } }

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		int width = Width < 50 ? MainForm.Width - 20 : Width;
		tabBar.Size = new Size(width, tabBarHeight);
		variantsTextBox.Location = new Point(0, tabBarHeight);
		variantsTextBox.Size = new Size(width, Height - tabBarHeight - variantsTextBox.CharHeight - 2);
		variantsTextBox.Controller.NeedScrollToCaret();
		textBox.Location = new Point(0, Height - variantsTextBox.CharHeight);
		textBox.Size = new Size(width, variantsTextBox.CharHeight + 1);
		if (findInFilesData != null)
		{
			int size = 20;
			filterTextBox.Location = new Point(Width - 7 * filterTextBox.CharWidth - size * filterTextBox.CharWidth, 2);
			filterTextBox.Size = new Size(size * filterTextBox.CharWidth, filterTextBox.CharHeight + 1);
		}
	}

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw)
		{
			settings.ApplySimpleParameters(variantsTextBox, null);
			settings.ApplySimpleParameters(textBox, null);
			textBox.SetViMap(settings.viMapSource.Value, settings.viMapResult.Value);
			SetTextBoxParameters();
			if (findInFilesData != null)
			{
				settings.ApplySimpleParameters(filterTextBox, null, false);
				filterTextBox.SetViMap(settings.viMapSource.Value, settings.viMapResult.Value);
			}
		}
		else if (phase == UpdatePhase.Parsed)
		{
			BackColor = settings.ParsedScheme.tabsBg.color;
			variantsTextBox.Scheme = settings.ParsedScheme;
			textBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
			if (findInFilesData != null)
			{
				filterTextBox.Scheme = settings.ParsedScheme;
				UpdateFilterText();
			}
		}
		else if (phase == UpdatePhase.FindParams)
		{
			UpdateFindParams();
			if (ignoreCase != tempSettings.FindParams.ignoreCase)
				UpdateVariantsText();
		}
	}

	private void UpdateFindParams()
	{
		string text = tempSettings.FindParams != null ? tempSettings.FindParams.GetIgnoreCaseIndicationText() : "";
		if (findInFilesData != null)
		{
			text = (string.IsNullOrEmpty(filterTextBox.Text) ? " filter  " : "[filter] ") + text;
		}
		tabBar.Text2 = text;
	}

	private void SetTextBoxParameters()
	{
		variantsTextBox.ShowLineNumbers = false;
		variantsTextBox.HighlightCurrentLine = true;
		variantsTextBox.WordWrap = true;

		textBox.ShowLineNumbers = false;
		textBox.HighlightCurrentLine = false;
	}

	private bool DoClose(Controller controller)
	{
		DispatchNeedClose();
		return true;
	}

	private bool DoNextField(Controller controller)
	{
		if (controller == textBox.Controller && variantsTextBox.Controller.Lines.charsCount != 0)
			variantsTextBox.Focus();
		else
			textBox.Focus();
		return true;
	}

	private void InitVariantsText(string text)
	{
		variantsTextBox.Controller.InitText(text);
		variantsTextBox.Controller.ClearMinorSelections();
		Selection selection = variantsTextBox.Controller.LastSelection;
		int index = variantsTextBox.Controller.Lines.LinesCount - 1;
		if (string.IsNullOrEmpty(pattern))
		{
			int startIndex = StartVariantIndex;
			if (startIndex != -1)
			{
				index = startIndex;
			}
		}
		Place place = new Place(0, index);
		selection.anchor = selection.caret = variantsTextBox.Controller.Lines.IndexOf(place);
		variantsTextBox.Invalidate();
		Nest.size = tabBar.Height + variantsTextBox.CharHeight * (
			!string.IsNullOrEmpty(text) && variantsTextBox.Controller != null ?
				variantsTextBox.GetScrollSizeY() + 2 : 1
		) + 4;
		variantsTextBox.Controller.NeedScrollToCaret();
		SetNeedResize();
	}

	private bool DoUp(Controller controller)
	{
		variantsTextBox.Controller.MoveUp(false);
		variantsTextBox.Controller.NeedScrollToCaret();
		variantsTextBox.Invalidate();
		UpdateSelectionChange();
		return true;
	}

	private bool DoDown(Controller controller)
	{
		variantsTextBox.Controller.MoveDown(false);
		variantsTextBox.Controller.NeedScrollToCaret();
		variantsTextBox.Invalidate();
		UpdateSelectionChange();
		return true;
	}

	private bool DoDocumentStart(Controller controller)
	{
		variantsTextBox.Controller.DocumentStart(false);
		variantsTextBox.Controller.NeedScrollToCaret();
		variantsTextBox.Invalidate();
		UpdateSelectionChange();
		return true;
	}

	private bool DoDocumentEnd(Controller controller)
	{
		variantsTextBox.Controller.DocumentEnd(false);
		variantsTextBox.Controller.NeedScrollToCaret();
		variantsTextBox.Invalidate();
		UpdateSelectionChange();
		return true;
	}

	private bool DoPageUp(Controller controller)
	{
		variantsTextBox.Controller.ScrollPage(true, false);
		variantsTextBox.Controller.NeedScrollToCaret();
		variantsTextBox.Invalidate();
		UpdateSelectionChange();
		return true;
	}
	
	private bool DoPageDown(Controller controller)
	{
		variantsTextBox.Controller.ScrollPage(false, false);
		variantsTextBox.Controller.NeedScrollToCaret();
		variantsTextBox.Invalidate();
		UpdateSelectionChange();
		return true;
	}

	private bool DoExecute(Controller controller)
	{
		Place place = variantsTextBox.Controller.Lines.PlaceOf(variantsTextBox.Controller.LastSelection.caret);
		string lineText = variantsTextBox.Controller.Lines[place.iLine].Text.Trim();
		Execute(place.iLine, lineText);
		return true;
	}
	
	private void OnVariantsTextBoxClick()
	{
		UpdateSelectionChange();
	}
	
	private void UpdateSelectionChange()
	{
		if (MainForm == null)
			return;
		Place place = variantsTextBox.Controller.Lines.PlaceOf(variantsTextBox.Controller.LastSelection.caret);
		string lineText = variantsTextBox.Controller.Lines[place.iLine].Text.Trim();
		DoOnSelectionChange(place.iLine, lineText);
	}
	
	virtual protected string GetSubname()
	{
		return null;
	}

	virtual protected bool Prebuild()
	{
		return true;
	}

	virtual protected string GetVariantsText()
	{
		return "";
	}
	
	virtual protected void DoOnSelectionChange(int line, string lineText)
	{
	}

	virtual protected void Execute(int line, string lineText)
	{
	}
	
	private bool DoNormalMode(Controller controller)
	{
		textBox.SetViMode(true);
		textBox.Controller.ViFixPositions(false);
		if (findInFilesData != null)
		{
			filterTextBox.SetViMode(true);
			filterTextBox.Controller.ViFixPositions(false);
		}
		return true;
	}
	
	private bool DoSwitchToTempFilter(Controller controller)
	{
		filterTextBox.Visible = true;
		filterTextBox.Focus();
		return true;
	}
	
	private bool DoFilterPrevPattern(Controller controller)
	{
		return GetFilterHistoryPattern(true);
	}
	
	private bool DoFilterNextPattern(Controller controller)
	{
		return GetFilterHistoryPattern(false);
	}
	
	private bool GetFilterHistoryPattern(bool isPrev)
	{
		string text = filterTextBox.Text;
		string newText = findInFilesData.filterHistory.GetOrEmpty(text, isPrev);
		if (newText != text)
		{
			filterTextBox.Text = newText;
			filterTextBox.Controller.ClearMinorSelections();
			filterTextBox.Controller.LastSelection.anchor = filterTextBox.Controller.LastSelection.caret = newText.Length;
			UpdateFilterText();
		}
		return true;
	}
	
	private void OnFilterTextChange()
	{
		UpdateFilterText();
	}
	
	private void UpdateFilterText()
	{
		findInFilesData.currentFilter.value = filterTextBox.Text;
	}
	
	protected string GetFilterText()
	{
		return !string.IsNullOrEmpty(filterTextBox.Text) ? filterTextBox.Text : MainForm.Settings.findInFilesFilter.Value;
	}
	
	protected string GetFilterDesc()
	{
		string filterText = GetFilterText();
		return string.IsNullOrEmpty(filterTextBox.Text) ? filterText : "[" + filterText + "]";
	}
	
	private bool DoSwitchToInputField(Controller controller)
	{
		filterTextBox.Visible = false;
		textBox.Focus();
		return true;
	}
}
