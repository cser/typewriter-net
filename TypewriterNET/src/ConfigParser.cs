﻿using System;
using System.Text;
using System.Xml;

public class ConfigParser
{
	private readonly Settings settings;

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
			bool wasUnknownName = false;
			foreach (XmlNode node in root.ChildNodes)
			{
				XmlElement element = node as XmlElement;
				if (element != null && element.Name == "item")
				{
					string name = element.GetAttribute("name");
					string keyName = Properties.NameOfName(name);
					Properties.Property property = settings[keyName];
					if (property != null)
					{
						if (element.HasAttribute("value"))
						{
							string value = element.GetAttribute("value");
							if (isLocal && (property.constraints & Properties.Constraints.NotForLocal) != 0)
							{
								errors.Append("Disallowed in local config: name=" + keyName + " (to prevent unexpected behaviour on directory switching)");
							}
							else
							{
								property.initedByConfig = true;
								string error = property.SetText(value, Properties.SubvalueOfName(name));
								if (!string.IsNullOrEmpty(error))
									errors.AppendLine(error);
							}
						}
						else
						{
							if (property != null)
							{
								if (property.AllowTemp)
								{
									property.initedByConfig = false;
								}
								else
								{
									errors.Append("Saving name=" + keyName + " in temp isn't allowed, need value");
								}
							}
						}
					}
					else
					{
						errors.AppendLine("Unknown name=" + keyName);
						wasUnknownName = true;
					}
				}
			}
			if (wasUnknownName)
			{
				errors.AppendLine("(if no errors before upgrade, please, remove this names from configs by F2 and Ctrl+F2)");
			}
		}
	}
}
