using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MulticaretEditor;
using MulticaretEditor.Highlighting;

public class SchemeManager
{	
	private XmlLoader xmlLoader;

	public SchemeManager(XmlLoader xmlLoader)
	{
		this.xmlLoader = xmlLoader;
	}

	private AppPath GetSchemePath(string schemeName)
	{
		return new AppPath(Path.Combine(AppPath.Schemes, schemeName + ".xml"));
	}
	
	private List<AppPath> GetSchemePaths(string schemeName)
	{
		List<AppPath> paths = new List<AppPath>();
		foreach (string schemeNameI in ParseSchemeName(schemeName))
		{
			paths.Add(GetSchemePath(schemeNameI));
		}
		return paths;
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
	    
	public Scheme LoadScheme(string schemeName)
	{
		Scheme scheme = new Scheme();
		List<XmlDocument> xmls = new List<XmlDocument>();
		foreach (AppPath schemePath in GetSchemePaths(schemeName))
		{
			XmlDocument xml = xmlLoader.Load(schemePath.GetExisted(), true);
			if (xml != null)
				xmls.Add(xml);
		}
		scheme.ParseXml(xmls);
		return scheme;
	}
}
