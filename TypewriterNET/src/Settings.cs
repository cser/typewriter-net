using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using MulticaretEditor;
using MulticaretEditor.Highlighting;

public class Settings
{
	public readonly Properties.Bool wordWrap = new Properties.Bool("wordWrap", false);
	public readonly Properties.Bool showLineNumbers = new Properties.Bool("showLineNumbers", true);
	public readonly Properties.Bool showLineBreaks = new Properties.Bool("showLineBreaks", false);
	public readonly Properties.Bool showSpaceCharacters = new Properties.Bool("showSpaceCharacters", false);
	public readonly Properties.Bool highlightCurrentLine = new Properties.Bool("highlightCurrentLine", true);
	public readonly Properties.String lineBreak = new Properties.String("lineBreak", "\r\n", true).SetVariants("\r\n", "\n", "\r");
	public readonly Properties.Int tabSize = new Properties.Int("tabSize", 4).SetMinMax(0, 128);
	public readonly Properties.Bool spacesInsteadTabs = new Properties.Bool("spacesInsteadTabs", false);
	public readonly Properties.Int maxTabsCount = new Properties.Int("maxTabsCount", 10).SetMinMax(1, int.MaxValue);
	public readonly Properties.Float fontSize = new Properties.Float("fontSize", 10.25f).SetMinMax(4, 100).SetPrecision(2);
	public readonly Properties.Font font = new Properties.Font("font", FontFamily.GenericMonospace);
	public readonly Properties.String scheme = new Properties.String("scheme", "npp", false).SetLoadVariants(SchemeManager.GetAllSchemeNames);
	public readonly Properties.Int scrollingIndent = new Properties.Int("scrollingIndent", 3).SetMinMax(0, int.MaxValue);
	public readonly Properties.String altCharsSource = new Properties.String("altCharsSource", "", false);
	public readonly Properties.String altCharsResult = new Properties.String("altCharsResult", "", false);
	public readonly Properties.Bool showColorAtCursor = new Properties.Bool("showColorAtCursor", false);
	public readonly Properties.Bool rememberOpenedFiles = new Properties.Bool("rememberOpenedFiles", false);
	public readonly Properties.Int maxFileQualitiesCount = new Properties.Int("maxFileQualitiesCount", 1000).SetMinMax(0, int.MaxValue);
	public readonly Properties.Bool alwaysOnTop = new Properties.Bool("alwaysOnTop", false);
	public readonly Properties.Int connectionTimeout = new Properties.Int("connectionTimeout", 1000).SetMinMax(1, int.MaxValue);
	public readonly Properties.RegexList shellRegexList = new Properties.RegexList("shellRegex");
	public readonly Properties.Bool miniMap = new Properties.Bool("miniMap", false);
	public readonly Properties.Float miniMapScale = new Properties.Float("miniMapScale", .3f).SetMinMax(.1f, 10f);
	public readonly Properties.Bool printMargin = new Properties.Bool("printMargin", false);
	public readonly Properties.Int printMarginSize = new Properties.Int("printMarginSize", 80).SetMinMax(1, int.MaxValue);
	public readonly Properties.Bool markWord = new Properties.Bool("markWord", true);
	public readonly Properties.Bool markBracket = new Properties.Bool("markBracket", true);
	public readonly Properties.Bool rememberCurrentDir = new Properties.Bool("rememberCurrentDir", false);
	public readonly Properties.String findInFilesDir = new Properties.String("findInFilesDir", "", false);
	public readonly Properties.String findInFilesFilter = new Properties.String("findInFilesFilter", "*.*", false);
	public readonly Properties.EncodingProperty defaultEncoding = new Properties.EncodingProperty("defaultEncoding", new EncodingPair(Encoding.UTF8, false));
	public readonly Properties.EncodingProperty shellEncoding = new Properties.EncodingProperty("shellEncoding", new EncodingPair(Encoding.UTF8, false));
	public readonly Properties.EncodingProperty httpEncoding = new Properties.EncodingProperty("httpEncoding", new EncodingPair(Encoding.UTF8, false));
	public readonly Properties.Bool showEncoding = new Properties.Bool("showEncoding", false);

	public readonly Properties.String f5Command = new Properties.String("f5Command", "", false);
	public readonly Properties.String f6Command = new Properties.String("f6Command", "", false);
	public readonly Properties.String f7Command = new Properties.String("f7Command", "", false);
	public readonly Properties.String f8Command = new Properties.String("f8Command", "", false);
	public readonly Properties.String f9Command = new Properties.String("f9Command", "", false);
	public readonly Properties.String f11Command = new Properties.String("f11Command", "", false);
	public readonly Properties.String f12Command = new Properties.String("f12Command", "", false);

	private Setter onChange;

	public Settings(Setter onChange)
	{
		this.onChange = onChange;
		Add(wordWrap);
		Add(showLineNumbers);
		Add(showLineBreaks);
		Add(showSpaceCharacters);
		Add(highlightCurrentLine);
		Add(lineBreak);
		Add(tabSize);
		Add(spacesInsteadTabs);
		Add(maxTabsCount);
		Add(fontSize);
		Add(font);
		Add(scheme);
		Add(scrollingIndent);
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
		Add(findInFilesFilter);
		Add(f5Command);
		Add(f6Command);
		Add(f7Command);
		Add(f8Command);
		Add(f9Command);
		Add(f11Command);
		Add(f12Command);
		Add(defaultEncoding);
		Add(shellEncoding);
		Add(httpEncoding);
		Add(showEncoding);
	}

	public void DispatchChange()
	{
		if (onChange != null)
			onChange();
	}

	private Dictionary<string, Properties.Property> propertyByName = new Dictionary<string, Properties.Property>();

	private RWList<Properties.Property> properties = new RWList<Properties.Property>();

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
		TextTable table = new TextTable().SetMaxColWidth(30);
		Properties.AddHeadTo(table);
		table.AddLine();
		bool first = true;
		foreach (Properties.Property property in properties)
		{
			if (!first)
				table.NewRow();
			first = false;
			property.GetHelpText(table);
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

	public void ApplyParameters(MulticaretTextBox textBox, SettingsMode settingsMode)
	{
		textBox.WordWrap = settingsMode != SettingsMode.FileTree && wordWrap.Value;
		textBox.ShowLineNumbers = showLineNumbers.Value;
		textBox.ShowLineBreaks = showLineBreaks.Value;
		textBox.ShowSpaceCharacters = showSpaceCharacters.Value;
		textBox.HighlightCurrentLine = highlightCurrentLine.Value;
		textBox.TabSize = tabSize.Value;
		textBox.SpacesInsteadTabs = spacesInsteadTabs.Value;
		textBox.LineBreak = lineBreak.Value;
		textBox.FontFamily = font.Value;
		textBox.FontSize = fontSize.Value;
		textBox.ScrollingIndent = scrollingIndent.Value;
		textBox.ShowColorAtCursor = showColorAtCursor.Value;
		textBox.KeyMap.main.SetAltChars(altCharsSource.Value, altCharsResult.Value);
		textBox.Map = settingsMode != SettingsMode.FileTree && miniMap.Value;
		textBox.MapScale = miniMapScale.Value;
		textBox.PrintMargin = settingsMode == SettingsMode.Normal && printMargin.Value;
		textBox.PrintMarginSize = printMarginSize.Value;
		textBox.MarkWord = markWord.Value;
		textBox.MarkBracket = markBracket.Value;
	}

	public void ApplySimpleParameters(MulticaretTextBox textBox)
	{
		textBox.WordWrap = wordWrap.Value;
		textBox.ShowLineNumbers = false;
		textBox.ShowLineBreaks = showLineBreaks.Value;
		textBox.ShowSpaceCharacters = showSpaceCharacters.Value;
		textBox.HighlightCurrentLine = false;
		textBox.TabSize = tabSize.Value;
		textBox.SpacesInsteadTabs = spacesInsteadTabs.Value;
		textBox.LineBreak = lineBreak.Value;
		textBox.FontFamily = font.Value;
		textBox.FontSize = fontSize.Value;
		textBox.ScrollingIndent = scrollingIndent.Value;
		textBox.ShowColorAtCursor = showColorAtCursor.Value;
		textBox.KeyMap.main.SetAltChars(altCharsSource.Value, altCharsResult.Value);
	}

	public void ApplyToLabel(MonospaceLabel label)
	{
		label.TabSize = tabSize.Value;
		label.FontFamily = font.Value;
		label.FontSize = fontSize.Value;
	}

	public void ApplySchemeToLabel(MonospaceLabel label)
	{
		label.BackColor = parsedScheme.tabsBgColor;
		label.TextColor = parsedScheme.fgColor;
	}
}
