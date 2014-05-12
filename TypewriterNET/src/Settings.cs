using System;
using System.Collections.Generic;
using System.Drawing;
using MulticaretEditor;

public class Settings
{
	private bool wordWrap;
	public bool WordWrap
	{
		get { return wordWrap; }
		set { wordWrap = value; }
	}
	
	private bool showLineNumbers = true;
	public bool ShowLineNumbers
	{
		get { return showLineNumbers; }
		set { showLineNumbers = value; }
	}
	
	private bool showLineBreaks;
	public bool ShowLineBreaks
	{
		get { return showLineBreaks; }
		set { showLineBreaks = value; }
	}
	
	private bool highlightCurrentLine = true;
	public bool HighlightCurrentLine
	{
		get { return highlightCurrentLine; }
		set { highlightCurrentLine = value; }
	}
	
	private string lineBreak;
	public string LineBreak
	{
		get { return lineBreak == "\n" || lineBreak == "\r" ? lineBreak : "\r\n"; }
		set { lineBreak = value; }
	}
	
	public readonly Properties.Int tabSize = new Properties.Int("tabSize", 4).SetMinMax(0, 128);
	
	private int maxTabsCount;
	public int MaxTabsCount
	{
		get { return maxTabsCount > 0 ? maxTabsCount : int.MaxValue; }
		set { maxTabsCount = value; }
	}
	
	private float fontSize;
	public float FontSize
	{
		get { return fontSize > 0 ? fontSize : 10.25f; }
		set { fontSize = value; }
	}
	
	private FontFamily fontFamily;
	public FontFamily FontFamily
	{
		get { return fontFamily != null ? fontFamily : FontFamily.GenericMonospace; }
		set { fontFamily = value; }
	}
	
	private string scheme;
	public string Scheme
	{
		get { return scheme; }
		set { scheme = value; }
	}
	
	private int scrollingIndent;
	public int ScrollingIndent
	{
		get { return scrollingIndent; }
		set { scrollingIndent = value; }
	}
	
	private string altCharsSource;
	public string AltCharsSource
	{
		get { return altCharsSource; }
		set { altCharsSource = value; }
	}
	
	private string altCharsResult;
	public string AltCharsResult
	{
		get { return altCharsResult; }
		set { altCharsResult = value; }
	}
	
	private bool showColorAtCursor;
	public bool ShowColorAtCursor
	{
		get { return showColorAtCursor; }
		set { showColorAtCursor = value; }
	}
	
	private bool rememberOpenedFiles;
	public bool RememberOpenedFiles
	{
		get { return rememberOpenedFiles; }
		set { rememberOpenedFiles = value; }
	}

	private int maxFileQualitiesCount;
	public int MaxFileQualitiesCount
	{
		get { return maxFileQualitiesCount; }
		set { maxFileQualitiesCount = value; }
	}

	private Setter onChange;

	public Settings(Setter onChange)
	{
		this.onChange = onChange;
		Add(tabSize);
	}

	public void DispatchChange()
	{
		if (onChange != null)
			onChange();
	}

	private Dictionary<string, Properties.Property> propertyByName = new Dictionary<string, Properties.Property>();

	private void Add(Properties.Property property)
	{
		propertyByName[property.name] = property;
	}

	private Properties.Property emptyProperty = new Properties.Property("");

	public Properties.Property this[string name]
	{
		get
		{
			Properties.Property property;
			propertyByName.TryGetValue(name, out property);
			return property ?? emptyProperty;
		}
	}
}
