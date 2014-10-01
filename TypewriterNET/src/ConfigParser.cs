using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using MulticaretEditor.Highlighting;

public class ConfigParser
{
	private Settings settings;

	public ConfigParser(Settings settings)
	{
		this.settings = settings;
	}
	
	public void Reset()
	{
		settings.Reset();
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
							if (settings[name] != null)
							{
								string error = settings[name].SetText(value);
								if (!string.IsNullOrEmpty(error))
									errors.AppendLine(error);
							}
							else
							{
								errors.AppendLine("Unknown name=" + name);
							}
						}
					}
				}
			}
		}
	}

	public void PostParse(StringBuilder errors)
	{
		string error;
		settings.defaultEncodingPair = EncodingPair.ParseEncoding(settings.defaultEncoding.Value, out error);
		if (!string.IsNullOrEmpty(error))
			errors.Append("defaultEncoding error: " + error);
		settings.shellEncodingPair = EncodingPair.ParseEncoding(settings.shellEncoding.Value, out error);
		if (!string.IsNullOrEmpty(error))
			errors.Append("shellEncoding error: " + error);
	}
}
