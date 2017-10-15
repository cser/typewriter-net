using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using MulticaretEditor;

public class Settings
{
	public const string DefaultScheme = "npp";
	
	public readonly Properties.Bool wordWrap = new Properties.Bool("wordWrap", false);
	public readonly Properties.Bool showLineNumbers = new Properties.Bool("showLineNumbers", true);
	public readonly Properties.Bool highlightCurrentLine = new Properties.Bool("highlightCurrentLine", true);
	public readonly Properties.String lineBreak = new Properties.String("lineBreak", "\r\n", true, "").SetVariants("\r\n", "\n", "\r");
	public readonly Properties.IntList tabSize = new Properties.IntList("tabSize", 4).SetMinMax(0, 128);
	public readonly Properties.BoolList spacesInsteadTabs = new Properties.BoolList("spacesInsteadTabs", false);
	public readonly Properties.BoolList autoindent = new Properties.BoolList("autoindent", false);
	public readonly Properties.Int maxTabsCount = new Properties.Int("maxTabsCount", 10).SetMinMax(1, int.MaxValue);
	public readonly Properties.Float lineNumberFontSize = new Properties.Float("lineNumberFontSize", 0).SetMinMax(0, 100).SetPrecision(2);
	public readonly Properties.Float fontSize = new Properties.Float("fontSize", 10.25f).SetMinMax(4, 100).SetPrecision(2);
	public readonly Properties.Font font = new Properties.Font("font", FontFamily.GenericMonospace);
	public readonly Properties.String scheme = new Properties.String("scheme", DefaultScheme, false, "").SetLoadVariants(SchemeManager.GetAllSchemeNames);
	public readonly Properties.Int scrollingIndent = new Properties.Int("scrollingIndent", 3).SetMinMax(0, int.MaxValue);
	public readonly Properties.Int scrollingStep = new Properties.Int("scrollingStep", 3).SetMinMax(1, int.MaxValue);
	public readonly Properties.String altCharsSource = new Properties.String("altCharsSource", "", false, "Chars to input with right Alt");
	public readonly Properties.String altCharsResult = new Properties.String("altCharsResult", "", false, "Output chars with right Alt");
	public readonly Properties.Bool showColorAtCursor = new Properties.Bool("showColorAtCursor", false);
	public readonly Properties.Bool rememberOpenedFiles = new Properties.Bool("rememberOpenedFiles", false);
	public readonly Properties.Int maxFileQualitiesCount = new Properties.Int("maxFileQualitiesCount", 200).SetMinMax(0, int.MaxValue);//May be don't used
	public readonly Properties.Bool alwaysOnTop = new Properties.Bool("alwaysOnTop", false);
	public readonly Properties.Int connectionTimeout = new Properties.Int("connectionTimeout", 1000).SetMinMax(1, int.MaxValue);
	public readonly Properties.RegexList shellRegexList = new Properties.RegexList("shellRegex");
	public readonly Properties.Bool miniMap = new Properties.Bool("miniMap", false);
	public readonly Properties.Float miniMapScale = new Properties.Float("miniMapScale", .3f).SetMinMax(.1f, 10f);
	public readonly Properties.Bool printMargin = new Properties.Bool("printMargin", false);
	public readonly Properties.Int printMarginSize = new Properties.Int("printMarginSize", 80).SetMinMax(1, int.MaxValue);
	public readonly Properties.Bool markWord = new Properties.Bool("markWord", true);
	public readonly Properties.Bool markBracket = new Properties.Bool("markBracket", true);
	public readonly Properties.Bool rememberCurrentDir = new Properties.Bool("rememberCurrentDir", false, Properties.Constraints.NotForLocal);
	public readonly Properties.String findInFilesDir = new Properties.String("findInFilesDir", "", false, "");
	public readonly Properties.String findInFilesIgnoreDir = new Properties.String("findInFilesIgnoreDir", "", false, "");
	public readonly Properties.String findInFilesFilter = new Properties.String("findInFilesFilter", "*.*", false, "");
	public readonly Properties.String hideInFileTree = new Properties.String("hideInFileTree", "", false, "");
	public readonly Properties.String renamePostfixed = new Properties.String("renamePostfixed", "", false, "");
	public readonly Properties.Bool checkContentBeforeReloading = new Properties.Bool("checkContentBeforeReloading", false);
	public readonly Properties.EncodingProperty defaultEncoding = new Properties.EncodingProperty("defaultEncoding", new EncodingPair(Encoding.UTF8, false));
	public readonly Properties.EncodingProperty shellEncoding = new Properties.EncodingProperty("shellEncoding", new EncodingPair(Encoding.UTF8, false));
	public readonly Properties.EncodingProperty httpEncoding = new Properties.EncodingProperty("httpEncoding", new EncodingPair(Encoding.UTF8, false));
	public readonly Properties.Bool showEncoding = new Properties.Bool("showEncoding", false);

	public readonly Properties.Command f5Command = new Properties.Command("f5Command");
	public readonly Properties.Command f6Command = new Properties.Command("f6Command");
	public readonly Properties.Command f7Command = new Properties.Command("f7Command");
	public readonly Properties.Command f8Command = new Properties.Command("f8Command");
	public readonly Properties.Command f9Command = new Properties.Command("f9Command");
	public readonly Properties.Command f11Command = new Properties.Command("f11Command");
	public readonly Properties.Command f12Command = new Properties.Command("f12Command");
	public readonly Properties.Command shiftF5Command = new Properties.Command("shiftF5Command");
	public readonly Properties.Command shiftF6Command = new Properties.Command("shiftF6Command");
	public readonly Properties.Command shiftF7Command = new Properties.Command("shiftF7Command");
	public readonly Properties.Command shiftF8Command = new Properties.Command("shiftF8Command");
	public readonly Properties.Command shiftF9Command = new Properties.Command("shiftF9Command");
	public readonly Properties.Command shiftF11Command = new Properties.Command("shiftF11Command");
	public readonly Properties.Command shiftF12Command = new Properties.Command("shiftF12Command");
	public readonly Properties.Command ctrlSpaceCommand = new Properties.Command("ctrlSpaceCommand");
	public readonly Properties.Command ctrlShiftSpaceCommand = new Properties.Command("ctrlShiftSpaceCommand");
	public readonly Properties.Command afterSaveCommand = new Properties.Command("afterSaveCommand");
	public readonly Properties.PathProperty omnisharpSln = new Properties.PathProperty("omnisharpSln", "", "path to sln or src");
	public readonly Properties.Int omnisharpPort = new Properties.Int("omnisharpPort", 2000);
	public readonly Properties.Bool omnisharpConsole = new Properties.Bool("omnisharpConsole", false);
	public readonly Properties.Int fileIncrementalSearchTimeout = new Properties.Int("fileIncrementalSearchTimeout", 8);
	public readonly Properties.Bool hideMenu = new Properties.Bool("hideMenu", false);
	public readonly Properties.Bool fullScreenOnMaximized = new Properties.Bool("fullScreenOnMaximized", false);
	public readonly Properties.String viMapSource = new Properties.String("viMapSource", "", false, "");
	public readonly Properties.String viMapResult = new Properties.String("viMapResult", "", false, "");
	public readonly Properties.Bool startWithViMode = new Properties.Bool("startWithViMode", false);
	public readonly Properties.Bool viEsc = new Properties.Bool("viEsc", false);
	public readonly Properties.Bool viAltOem = new Properties.Bool("viAlt[", false);
	public readonly Properties.String ignoreSnippets = new Properties.String("ignoreSnippets", "", false, "names without extension,\n  separated by ';'");
	public readonly Properties.String forcedSnippets = new Properties.String("forcedSnippets", "", false, "names without extension,\n  separated by ';'");
	public readonly Properties.CommandList command = new Properties.CommandList("command");
	public readonly Properties.Bool showLineBreaks = new Properties.Bool("showLineBreaks", false);
	public readonly Properties.Bool showSpaceCharacters = new Properties.Bool("showSpaceCharacters", false);
	public readonly Properties.Command syntax = new Properties.Command("syntax")
		.SetDesc("override syntax by filters");
	
	private static string GetBuildinParsers()
	{
		StringBuilder builder = new StringBuilder();
		foreach (TextNodeParser parser in TextNodesList.buildinParsers)
		{
			if (builder.Length > 0)
			{
				builder.Append("\n");
			}
			builder.Append("  ");
			builder.Append(parser.name);
		}
		return builder.ToString();
	}
	
	public readonly Properties.Command getTextNodes = new Properties.Command("getTextNodes").SetDesc(
		"your_script[:<coloring_syntax>]\nPress `Ctrl+L` or `,g` in vi-mode\nScript must receive document text\nby stdin and put JSON to stdout:\n\n" +
		"{\n" +
		"  \"line\":Number,\n" + 
		"  \"col\":Number,\n" + 
		"  \"name\":String,\n" + 
		"  \"childs\":[{\"line\":...}, {...]\n" + 
		"}\n\n" +
		"Instead external script you\ncan use buildin parsers:\n" + GetBuildinParsers());
	public readonly Properties.String snipsAuthor = new Properties.String("snipsAuthor", "No name", false, "replace `g:snips_author`");
	public readonly Properties.Int opacity = new Properties.Int("opacity", 100).SetMinMax(1, 100);

	private Setter onChange;

	public Settings(Setter onChange)
	{
		this.onChange = onChange;
		Add(wordWrap);
		Add(showLineNumbers);
		Add(highlightCurrentLine);
		Add(lineBreak);
		Add(tabSize);
		Add(spacesInsteadTabs);
		Add(autoindent);
		Add(maxTabsCount);
		Add(lineNumberFontSize);
		Add(fontSize);
		Add(font);
		Add(scheme);
		Add(scrollingIndent);
		Add(scrollingStep);
		Add(altCharsSource);
		Add(altCharsResult);
		Add(showColorAtCursor);
		Add(rememberOpenedFiles);
		Add(maxFileQualitiesCount);
		Add(alwaysOnTop);
		Add(connectionTimeout);
		Add(shellRegexList);
		Add(miniMap);
		Add(miniMapScale);
		Add(printMargin);
		Add(printMarginSize);
		Add(markWord);
		Add(markBracket);
		Add(rememberCurrentDir);
		Add(findInFilesDir);
		Add(findInFilesIgnoreDir);
		Add(findInFilesFilter);
		Add(hideInFileTree);
		Add(renamePostfixed);
		Add(f5Command);
		Add(f6Command);
		Add(f7Command);
		Add(f8Command);
		Add(f9Command);
		Add(f11Command);
		Add(f12Command);
		Add(shiftF5Command);
		Add(shiftF6Command);
		Add(shiftF7Command);
		Add(shiftF8Command);
		Add(shiftF9Command);
		Add(shiftF11Command);
		Add(shiftF12Command);
		Add(ctrlSpaceCommand);
		Add(ctrlShiftSpaceCommand);
		Add(afterSaveCommand);
		Add(defaultEncoding);
		Add(shellEncoding);
		Add(httpEncoding);
		Add(showEncoding);
		Add(omnisharpSln);
		Add(omnisharpPort);
		Add(omnisharpConsole);
		Add(checkContentBeforeReloading);
		Add(fileIncrementalSearchTimeout);
		Add(hideMenu);
		Add(fullScreenOnMaximized);
		Add(viMapSource);
		Add(viMapResult);
		Add(viEsc);
		Add(viAltOem);
		Add(startWithViMode);
		Add(ignoreSnippets);
		Add(forcedSnippets);
		Add(command);
		Add(showLineBreaks);
		Add(showSpaceCharacters);
		Add(getTextNodes);
		Add(snipsAuthor);
		Add(opacity);
		Add(syntax);
	}

	public void DispatchChange()
	{
		if (onChange != null)
			onChange();
	}

	private Dictionary<string, Properties.Property> propertyByName = new Dictionary<string, Properties.Property>();

	private RWList<Properties.Property> properties = new RWList<Properties.Property>();
	
	public IRList<Properties.Property> GetProperties()
	{
		return properties;
	}

	private void Add(Properties.Property property)
	{
		propertyByName[property.name] = property;
		properties.Add(property);
	}

	public Properties.Property this[string name]
	{
		get
		{
			Properties.Property property;
			propertyByName.TryGetValue(name, out property);
			return property;
		}
	}

	public string GetHelpText()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("# Settings properties");
		builder.AppendLine();
		builder.AppendLine("- First col legend: C - loads from config on start, T - from temp settings, [EMPTY] - only from config");
		builder.AppendLine("- Store here in config:           xml: <item name=\"name\" value=\"value\"/>");
		builder.AppendLine("- Make it store in temp settings: xml: <item name=\"name\"/>");
		builder.AppendLine("- [:<filter>] using example:      xml: <item name=\"name:*.cs;*.txt\" value=\"value for cs/txt file\"/>");
		builder.AppendLine("- Set property by command dialog: name value (autocomplete supported by `Tab` or `Ctrl+Space`)");
		builder.AppendLine();
		TextTable table = new TextTable().SetMaxColWidth(33);
		Properties.AddHeadTo(table);
		table.AddLine();
		bool first = true;
		Properties.Property prev = null;
		foreach (Properties.Property property in properties)
		{
			if (!first)
				table.NewRow();
			first = false;
			if (prev != null && prev.Type != property.Type)
			{
				if (prev.GetHelpTypeText(table))
					table.NewRow();
			}
			property.GetHelpText(this, table);
			prev = property;
		}
		builder.Append(table);
		builder.AppendLine();
		builder.Append(EncodingPair.GetEncodingsText());
		return builder.ToString();
	}

	public void Reset()
	{
		foreach (Properties.Property property in properties)
		{
			property.Reset();
			property.initedByConfig = false;
		}
	}

	private bool parsed = false;
	public bool Parsed
	{
		get { return parsed; }
		set { parsed = value; }
	}

	private Scheme parsedScheme;
	public Scheme ParsedScheme
	{
		get { return parsedScheme; }
		set { parsedScheme = value; }
	}

	public void ApplyParameters(MulticaretTextBox textBox, SettingsMode settingsMode, Buffer buffer)
	{
		textBox.WordWrap = settingsMode != SettingsMode.FileTree && settingsMode != SettingsMode.Help && wordWrap.Value;
		textBox.ShowLineNumbers = showLineNumbers.Value && settingsMode != SettingsMode.FileTree;
		textBox.ShowLineBreaks = showLineBreaks.Value;
		textBox.ShowSpaceCharacters = showSpaceCharacters.Value;
		textBox.HighlightCurrentLine = highlightCurrentLine.Value;
		textBox.TabSize = tabSize.GetValue(buffer);
		textBox.SpacesInsteadTabs = spacesInsteadTabs.GetValue(buffer);
		textBox.Autoindent = autoindent.GetValue(buffer);
		textBox.LineBreak = lineBreak.Value;
		textBox.FontFamily = font.Value;
		textBox.SetFontSize(fontSize.Value, lineNumberFontSize.Value);
		textBox.ScrollingIndent = scrollingIndent.Value;
		textBox.ScrollingStep = scrollingStep.Value;
		textBox.ShowColorAtCursor = showColorAtCursor.Value;
		textBox.KeyMap.main.SetAltChars(altCharsSource.Value, altCharsResult.Value);
		textBox.SetViMap(viMapSource.Value, viMapResult.Value);
		textBox.Map = settingsMode != SettingsMode.FileTree && miniMap.Value;
		textBox.MapScale = miniMapScale.Value;
		textBox.PrintMargin = settingsMode == SettingsMode.Normal && printMargin.Value;
		textBox.PrintMarginSize = printMarginSize.Value;
		textBox.MarkWord = markWord.Value;
		textBox.MarkBracket = markBracket.Value;
	}
	
	public void ApplySimpleParameters(MulticaretTextBox textBox, Buffer buffer)
	{
		ApplySimpleParameters(textBox, buffer, true);
	}

	public void ApplySimpleParameters(MulticaretTextBox textBox, Buffer buffer, bool changeFont)
	{
		textBox.WordWrap = wordWrap.Value;
		textBox.ShowLineNumbers = false;
		textBox.ShowLineBreaks = showLineBreaks.Value;
		textBox.ShowSpaceCharacters = showSpaceCharacters.Value;
		textBox.HighlightCurrentLine = false;
		textBox.TabSize = tabSize.GetValue(buffer);
		textBox.SpacesInsteadTabs = spacesInsteadTabs.GetValue(buffer);
		textBox.Autoindent = autoindent.GetValue(buffer);
		textBox.LineBreak = lineBreak.Value;
		if (changeFont)
		{
			textBox.FontFamily = font.Value;
			textBox.SetFontSize(fontSize.Value, lineNumberFontSize.Value);
		}
		textBox.ScrollingIndent = scrollingIndent.Value;
		textBox.ScrollingStep = scrollingStep.Value;
		textBox.ShowColorAtCursor = showColorAtCursor.Value;
		textBox.KeyMap.main.SetAltChars(altCharsSource.Value, altCharsResult.Value);
	}
	
	public void ApplyOnlyFileParameters(MulticaretTextBox textBox, Buffer buffer)
	{
	    textBox.SpacesInsteadTabs = spacesInsteadTabs.GetValue(buffer);
	}

	public void ApplyToLabel(MonospaceLabel label)
	{
		label.TabSize = tabSize.GetValue(null);
		label.FontFamily = font.Value;
		label.FontSize = fontSize.Value;
	}

	public void ApplySchemeToLabel(MonospaceLabel label)
	{
		label.BackColor = parsedScheme.tabsBg.color;
		label.TextColor = parsedScheme.tabsFg.color;
	}
	
	public void ParametersFromTemp(Dictionary<string, SValue> settingsData)
	{
		for (int i = 0; i < properties.Count; i++)
		{
			Properties.Property property = properties[i];
			if (property.AllowTemp && !property.initedByConfig)
			{
				SValue value = settingsData.ContainsKey(property.name) ? settingsData[property.name] : SValue.None;
				property.SetTemp(value);
			}
		}
	}
	
	public void ParametersToTemp(Dictionary<string, SValue> settingsData)
	{
		settingsData.Clear();
		for (int i = 0; i < properties.Count; i++)
		{
			Properties.Property property = properties[i];
			if (property.AllowTemp)
			{
				settingsData[property.name] = property.GetTemp();
			}
		}
	}
}
