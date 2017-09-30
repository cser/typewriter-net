using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;

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
	
	public void Parse(XmlDocument document, StringBuilder errors, bool isLocal)
	{
		bool wasUnknownName = false;
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
					if (element.Name == "item" && element.HasAttribute("value"))
					{
						string value = element.GetAttribute("value");
						string name = element.GetAttribute("name");
						string keyName = Properties.NameOfName(name);
						Properties.Property property = settings[keyName];
						if (property != null)
						{
							if (isLocal && (property.constraints & Properties.Constraints.NotForLocal) != 0)
							{
								errors.Append("Disallowed in local config: name=" + keyName + " (to prevent unexpected behaviour on directory switching)");
							}
							else
							{
								string error = property.SetText(value, Properties.SubvalueOfName(name));
								if (!string.IsNullOrEmpty(error))
									errors.AppendLine(error);
							}
						}
						else
						{
							errors.AppendLine("Unknown name=" + keyName);
							wasUnknownName = true;
						}
					}
				}
			}
		}
		if (wasUnknownName)
		{
			errors.AppendLine("(if no errors before upgrade, please, remove this names from configs by F2 and Ctrl+F2)");
		}
	}
}
