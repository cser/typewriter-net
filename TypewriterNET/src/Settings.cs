using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MulticaretEditor;
using MulticaretEditor.Highlighting;

public class Settings
{
	public readonly Properties.Bool wordWrap = new Properties.Bool("wordWrap", false);
	public readonly Properties.Bool showLineNumbers = new Properties.Bool("showLineNumbers", true);
	public readonly Properties.Bool showLineBreaks = new Properties.Bool("showLineBreaks", false);
	public readonly Properties.Bool highlightCurrentLine = new Properties.Bool("highlightCurrentLine", true);
	public readonly Properties.String lineBreak = new Properties.String("lineBreak", "\r\n", true).SetVariants("\r\n", "\n", "\r");
	public readonly Properties.Int tabSize = new Properties.Int("tabSize", 4).SetMinMax(0, 128);
	public readonly Properties.Int maxTabsCount = new Properties.Int("maxTabsCount", 10).SetMinMax(1, int.MaxValue);
	public readonly Properties.Float fontSize = new Properties.Float("fontSize", 10.25f).SetMinMax(4, 100).SetPrecision(2);
	public readonly Properties.Font font = new Properties.Font("font", FontFamily.GenericMonospace);
	public readonly Properties.String scheme = new Properties.String("scheme", "npp", false);
	public readonly Properties.Int scrollingIndent = new Properties.Int("scrollingIndent", 3).SetMinMax(0, int.MaxValue);
	public readonly Properties.String altCharsSource = new Properties.String("altCharsSource", "", false);
	public readonly Properties.String altCharsResult = new Properties.String("altCharsResult", "", false);
	public readonly Properties.Bool showColorAtCursor = new Properties.Bool("showColorAtCursor", false);
	public readonly Properties.Bool rememberOpenedFiles = new Properties.Bool("rememberOpenedFiles", false);
	public readonly Properties.Int maxFileQualitiesCount = new Properties.Int("maxFileQualitiesCount", 1000).SetMinMax(0, int.MaxValue);

	private Setter onChange;

	public Settings(Setter onChange)
	{
		this.onChange = onChange;
		Add(wordWrap);
		Add(showLineNumbers);
		Add(showLineBreaks);
		Add(highlightCurrentLine);
		Add(lineBreak);
		Add(tabSize);
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
		return builder.ToString();
	}

	public void Reset()
	{
		foreach (Properties.Property property in properties)
		{
			property.Reset();
		}
	}

	private Scheme parsedScheme;
	public Scheme ParsedScheme
	{
		get { return parsedScheme; }
		set { parsedScheme = value; }
	}

	public void ApplyParameters(MulticaretTextBox textBox)
	{
		textBox.WordWrap = wordWrap.Value;
		textBox.ShowLineNumbers = showLineNumbers.Value;
		textBox.ShowLineBreaks = showLineBreaks.Value;
		textBox.HighlightCurrentLine = highlightCurrentLine.Value;
		textBox.TabSize = tabSize.Value;
		textBox.LineBreak = lineBreak.Value;
		textBox.FontFamily = font.Value;
		textBox.FontSize = fontSize.Value;
		textBox.ScrollingIndent = scrollingIndent.Value;
		textBox.ShowColorAtCursor = showColorAtCursor.Value;
		textBox.KeyMap.main.SetAltChars(altCharsSource.Value, altCharsResult.Value);
	}

	public void ApplySimpleParameters(MulticaretTextBox textBox)
	{
		textBox.WordWrap = wordWrap.Value;
		textBox.ShowLineNumbers = false;
		textBox.ShowLineBreaks = showLineBreaks.Value;
		textBox.HighlightCurrentLine = false;
		textBox.TabSize = tabSize.Value;
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
