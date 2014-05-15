using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using MulticaretEditor.Highlighting;

namespace TypewriterNET
{
	public class Config
	{
		public Config()
		{
		}
		
		private bool wordWrap;
		public bool WordWrap { get { return wordWrap; } }
		
		private bool showLineNumbers = true;
		public bool ShowLineNumbers { get { return showLineNumbers; } }
		
		private bool showLineBreaks;
		public bool ShowLineBreaks { get { return showLineBreaks; } }
		
		private bool highlightCurrentLine = true;
		public bool HighlightCurrentLine { get { return highlightCurrentLine; } }
		
		private string lineBreak;
		public string LineBreak { get { return lineBreak; } }
		
		private int tabSize;
		public int TabSize { get { return tabSize; } }
		
		private int maxTabsCount;
		public int MaxTabsCount { get { return maxTabsCount; } }
		
		private float fontSize;
		public float FontSize { get { return fontSize; } }
		
		private FontFamily fontFamily;
		public FontFamily FontFamily { get { return fontFamily; } }
		
		private string scheme;
		public string Scheme { get { return scheme; } }
		
		private int scrollingIndent;
		public int ScrollingIndent { get { return scrollingIndent; } }
		
		private string altCharsSource;
		public string AltCharsSource { get { return altCharsSource; } }
		
		private string altCharsResult;
		public string AltCharsResult { get { return altCharsResult; } }
		
		private bool showColorAtCursor;
		public bool ShowColorAtCursor { get { return showColorAtCursor; } }
		
		private bool rememberOpenedFiles;
		public bool RememberOpenedFiles { get { return rememberOpenedFiles; } }

		private int maxFileQualitiesCount;
		public int MaxFileQualitiesCount { get { return maxFileQualitiesCount; } }
		
		public void Reset()
		{
			wordWrap = false;
			showLineNumbers = true;
			showLineBreaks = false;
			highlightCurrentLine = true;
			showColorAtCursor = false;
			rememberOpenedFiles = true;
			lineBreak = "\r\n";
			tabSize = 4;
			maxTabsCount = 10;
			fontSize = 10.25f;
			fontFamily = FontFamily.GenericMonospace;
			scheme = "npp";
			scrollingIndent = 2;
			altCharsSource = "";
			altCharsResult = "";
		}
		
		public void Parse(XmlDocument document, StringBuilder errors)
		{
			XmlNode root = null;
			foreach (XmlNode node in document.ChildNodes)
			{
				if (node is XmlElement && node.Name == "config")
				{
					root = node;
					break;
				}

			}
			if (root != null)
			{
				foreach (XmlNode node in root.ChildNodes)
				{
					XmlElement element = node as XmlElement;
					if (element != null)
					{
						if (element.Name == "item")
						{
							string value = element.GetAttribute("value");
							if (!string.IsNullOrEmpty(value))
							{
								string name = element.GetAttribute("name");
								switch (name)
								{
									case "wordWrap":
										wordWrap = value == "true";
										break;
									case "showLineNumbers":
										showLineNumbers = value == "true";
										break;
									case "showLineBreaks":
										showLineBreaks = value == "true";
										break;
									case "highlightCurrentLine":
										highlightCurrentLine = value == "true";
										break;
									case "showColorAtCursor":
										showColorAtCursor = value == "true";
										break;
									case "lineBreak":
										if (value == "\\r")
											lineBreak = "\r";
										else if (value == "\\n")
											lineBreak = "\n";
										else if (value == "\\r\\n")
											lineBreak = "\r\n";
										else
											errors.AppendLine("Incorrect lineBreak=" + value);
										break;
									case "tabSize":
										if (TryParseInt(name, value, ref tabSize, errors))
											ClampInt(name, ref tabSize, 1, int.MaxValue, errors);
										break;
									case "maxTabsCount":
										if (TryParseInt(name, value, ref maxTabsCount, errors))
											ClampInt(name, ref maxTabsCount, 1, int.MaxValue, errors);
										break;
									case "fontSize":
										if (TryParseFloat(name, value, ref fontSize, errors))
											ClampFloat(name, ref fontSize, 4, int.MaxValue, errors);
										break;
									case "fontFamily":
										if (IsFamilyInstalled(value))
											fontFamily = new FontFamily(value);
										else
											errors.AppendLine("Missing fontFamily=" + value);
										break;
									case "scheme":
										scheme = value;
										break;
									case "scrollingIndent":
										if (TryParseInt(name, value, ref scrollingIndent, errors))
											ClampInt(name, ref scrollingIndent, 0, int.MaxValue, errors);
										break;
									case "rememberOpenedFiles":
										rememberOpenedFiles = value == "true";
										break;
									case "maxFileQualitiesCount":
										if (TryParseInt(name, value, ref maxFileQualitiesCount, errors))
											ClampInt(name, ref maxFileQualitiesCount, 1, int.MaxValue, errors);
										break;
									default:
										errors.AppendLine("Unknown name=" + name);
										break;
								}
							}
						}
						else if (element.Name == "altChars")
						{
							string source = element.GetAttribute("source");
							string result = element.GetAttribute("result");
							if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(result))
							{
								int length = Math.Min(source.Length, result.Length);
								altCharsSource += source.Substring(0, length);
								altCharsResult += result.Substring(0, length);
							}
						}
					}
				}
			}
		}
		
		private bool IsFamilyInstalled(string fontFamily)
		{
			InstalledFontCollection installed = new InstalledFontCollection();
			foreach (FontFamily familyI in installed.Families)
			{
				if (familyI.Name == fontFamily)
					return true;
			}
			return false;
		}
		
		private bool TryParseInt(string name, string value, ref int intValue, StringBuilder errors)
		{
			int temp;
			if (Int32.TryParse(value, out temp))
			{
				intValue = temp;
				return true;
			}
			errors.AppendLine("Can't parse " + name + "=" + value);
			return false;
		}
		
		private void ClampInt(string name, ref int intValue, int min, int max, StringBuilder errors)
		{
			if (intValue < min)
			{
				errors.AppendLine("Fails: (" + name + "=" + intValue + ") >= " + min);
				intValue = min;
			}
			else if (intValue >= max)
			{
				errors.AppendLine("Fails: (" + name + "=" + intValue + ") <= " + max);
				intValue = max;				
			}
		}
		
		private bool TryParseFloat(string name, string value, ref float floatValue, StringBuilder errors)
		{
			float temp;
			if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out temp))
			{
				floatValue = temp;
				return true;
			}
			errors.AppendLine("Can't parse " + name + "=" + value);
			return false;
		}
		
		private void ClampFloat(string name, ref float floatValue, float min, float max, StringBuilder errors)
		{
			if (floatValue < min)
			{
				errors.AppendLine("Fails: (" + name + "=" + floatValue + ") >= " + min);
				floatValue = min;
			}
			else if (floatValue >= max)
			{
				errors.AppendLine("Fails: (" + name + "=" + floatValue + ") <= " + max);
				floatValue = max;
			}
		}
	}
}
