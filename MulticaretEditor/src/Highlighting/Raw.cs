using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MulticaretEditor
{
	public class Raw
	{
		public readonly List<RawList> lists = new List<RawList>();
		public readonly List<Context> contexts = new List<Context>();
		public readonly List<ItemData> itemDatas = new List<ItemData>();
		public readonly General general = new General();
		
		override public string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("<language>");
			
			builder.Append("<highlighting>");
			for (int i = 0; i < lists.Count; i++)
			{
				builder.Append(lists[i].ToString());
			}
			builder.Append("<contexts>");
			for (int i = 0; i < contexts.Count; i++)
			{
				builder.Append(contexts[i].ToString());
			}
			builder.Append("</contexts>");
			builder.Append("<itemDatas>");
			for (int i = 0; i < itemDatas.Count; i++)
			{
				builder.Append(itemDatas[i].ToString());
			}
			builder.Append("</itemDatas>");
			builder.Append("</highlighting>");
			
			builder.Append(general.ToString());
			
			builder.Append("</language>");
			return builder.ToString();
		}
		
		public class General
		{
			public string keywordsCasesensitive;
			public string keywordsWeakDeliminator;
			public string keywordsAdditionalDeliminator;
			
			override public string ToString()
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("<general>");
				builder.Append("<keywords");
				AppendAttr(builder, "casesensitive", keywordsCasesensitive);
				AppendAttr(builder, "weakDeliminator", keywordsWeakDeliminator);
				AppendAttr(builder, "additionalDeliminator", keywordsAdditionalDeliminator);
				builder.Append("/>");
				builder.Append("</general>");
				return builder.ToString();
			}
		}
		
		public class RawList
		{
			public string name;
			public List<string> items = new List<string>();
			
			public override string ToString()
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("<list");
				AppendAttr(builder, "name", name);
				builder.Append(">");
				for (int i = 0; i < items.Count; i++)
				{
					builder.Append("<item>" + items[i] + "</item>");
				}
				builder.Append("</list>");
				return builder.ToString();
			}
		}
		
		public class Context
		{
			public string name;
			public string attribute;
			public string lineEndContext;
			public string fallthrough;
			public string fallthroughContext;
			public readonly List<Rule> rules = new List<Raw.Rule>();
			
			override public string ToString()
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("<context");
				AppendAttr(builder, "name", name);
				AppendAttr(builder, "attribute", attribute);
				AppendAttr(builder, "lineEndContext", lineEndContext);
				AppendAttr(builder, "fallthrough", fallthrough);
				AppendAttr(builder, "fallthroughContext", fallthroughContext);
				builder.Append(">");
				for (int i = 0; i < rules.Count; i++)
				{
					builder.Append(rules[i].ToString());
				}
				builder.Append("</context>");
				return builder.ToString();
			}
		}
		
		public class Rule
		{
			public readonly string type;
			public readonly General general;
			
			public Rule(string type, General general)
			{
				this.type = type;
				this.general = general;
			}
			
			public string attribute;
			public string context;
			public string lineEndContext;
			public string String;
			public string char0;
			public string char1;
			public string insensitive;
			public string minimal;
			public string lookAhead;
			public string column;
			public string firstNonSpace;
			public readonly List<Rule> childs = new List<Raw.Rule>();
			
			override public string ToString()
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("<" + type);
				AppendAttr(builder, "attribute", attribute);
				AppendAttr(builder, "context", context);
				AppendAttr(builder, "String", String);
				AppendAttr(builder, "char", char0);
				AppendAttr(builder, "char1", char1);
				AppendAttr(builder, "lineEndContext", lineEndContext);
				AppendAttr(builder, "insensitive", lineEndContext);
				AppendAttr(builder, "minimal", minimal);
				AppendAttr(builder, "lookAhead", lookAhead);
				AppendAttr(builder, "column", column);
				AppendAttr(builder, "firstNonSpace", firstNonSpace);
				if (childs.Count > 0)
				{
					builder.Append(">");
					for (int i = 0; i < childs.Count; i++)
					{
						builder.Append(childs[i].ToString());
					}
					builder.Append("</" + type + ">");
				}
				else
				{
					builder.Append("/>");
				}
				return builder.ToString();
			}
		}
		
		public class ItemData
		{
			public string name;
			public string defStyleNum;
			
			public string color;
			public string italic;
			public string bold;
			public string underline;
			public string strikeout;
			
			override public string ToString()
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("<itemData");
				AppendAttr(builder, "name", name);
				AppendAttr(builder, "defStyleNum", defStyleNum);
				AppendAttr(builder, "color", color);
				AppendAttr(builder, "italic", italic);
				AppendAttr(builder, "bold", bold);
				AppendAttr(builder, "underline", underline);
				AppendAttr(builder, "strikeout", strikeout);
				builder.Append("/>");
				return builder.ToString();
			}
		}
		
		public static void AppendAttr(StringBuilder builder, string name, string value)
		{
			if (!string.IsNullOrEmpty(value))
				builder.Append(" " + name + "='" + value + "'");
		}
		
		public static Raw Parse(XmlDocument xml)
		{
			if (xml == null)
				return null;
			Raw raw = new Raw();
			foreach (XmlNode nodeI in xml.ChildNodes)
			{
				XmlElement elementI = nodeI as XmlElement;
				if (elementI != null && elementI.Name == "language")
				{
					foreach (XmlNode nodeJ in elementI.ChildNodes)
					{
						XmlElement elementJ = nodeJ as XmlElement;
						if (elementJ != null)
						{
							if (elementJ.Name == "highlighting")
							{
								foreach (XmlNode nodeK in elementJ.ChildNodes)
								{
									XmlElement elementK = nodeK as XmlElement;
									if (elementK != null)
									{
										if (elementK.Name == "list")
										{
											RawList list = new RawList();
											list.name = elementK.GetAttribute("name");
											foreach (XmlNode nodeL in elementK.ChildNodes)
											{
												XmlElement elementL = nodeL as XmlElement;
												if (elementL != null && elementL.Name == "item")
												{
													string innerText = elementL.InnerText.Trim();
													if (!string.IsNullOrEmpty(innerText))
													{
														list.items.Add(innerText);
													}
												}
											}
											raw.lists.Add(list);
										}
										else if (elementK.Name == "contexts")
										{
											foreach (XmlNode nodeL in elementK.ChildNodes)
											{
												XmlElement elementL = nodeL as XmlElement;
												if (elementL != null && elementL.Name == "context")
												{
													Context context = new Context();
													context.name = elementL.GetAttribute("name");
													context.attribute = elementL.GetAttribute("attribute");
													context.lineEndContext = elementL.GetAttribute("lineEndContext");
													context.fallthrough = elementL.GetAttribute("fallthrough");
													context.fallthroughContext = elementL.GetAttribute("fallthroughContext");
													ParseRules(context.rules, elementL, raw.general);
													raw.contexts.Add(context);
												}
											}
										}
										else if (elementK.Name == "itemDatas")
										{
											foreach (XmlNode nodeL in elementK.ChildNodes)
											{
												XmlElement elementL = nodeL as XmlElement;
												if (elementL != null && elementL.Name == "itemData")
												{
													ItemData itemData = new ItemData();
													itemData.name = elementL.GetAttribute("name");
													itemData.defStyleNum = elementL.GetAttribute("defStyleNum");
													itemData.color = elementL.GetAttribute("color");
													itemData.italic = elementL.GetAttribute("italic");
													itemData.bold = elementL.GetAttribute("bold");
													itemData.underline = elementL.GetAttribute("underline");
													itemData.strikeout = elementL.GetAttribute("strikeout");
													raw.itemDatas.Add(itemData);
												}
											}
										}
									}
								}
							}
							else if (elementJ.Name == "general")
							{
								foreach (XmlNode nodeK in elementJ.ChildNodes)
								{
									XmlElement elementK = nodeK as XmlElement;
									if (elementK != null)
									{
										if (elementK.Name == "keywords")
										{
											raw.general.keywordsCasesensitive = elementK.GetAttribute("casesensitive");
											raw.general.keywordsWeakDeliminator = elementK.GetAttribute("weakDeliminator");
											raw.general.keywordsAdditionalDeliminator = elementK.GetAttribute("additionalDeliminator");
										}
									}
								}
							}
						}
					}
				}
			}
			return raw;
		}
		
		private static void ParseRules(List<Rule> rules, XmlElement element, General general)
		{
			foreach (XmlNode nodeI in element.ChildNodes)
			{
				XmlElement elementI = nodeI as XmlElement;
				if (elementI != null)
				{
					Rule rule = new Rule(elementI.Name, general);
					rule.attribute = elementI.GetAttribute("attribute");
					rule.context = elementI.GetAttribute("context");
					rule.lineEndContext = elementI.GetAttribute("lineEndContext");
					rule.String = elementI.GetAttribute("String");
					rule.char0 = elementI.GetAttribute("char");
					rule.char1 = elementI.GetAttribute("char1");
					rule.insensitive = elementI.GetAttribute("insensitive");
					rule.minimal = elementI.GetAttribute("minimal");
					rule.lookAhead = elementI.GetAttribute("lookAhead");
					rule.column = elementI.GetAttribute("column");
					rule.firstNonSpace = elementI.GetAttribute("firstNonSpace");
					rules.Add(rule);
					if (elementI.ChildNodes.Count > 0)
						ParseRules(rule.childs, elementI, general);
				}
			}
		}
		
		public static void PrefixContexts(Raw raw, string prefix)
		{
			if (raw == null)
				return;
			foreach (RawList list in raw.lists)
			{
				list.name = prefix + list.name;
			}
			foreach (Context context in raw.contexts)
			{
				context.name = prefix + context.name;
				context.lineEndContext = PrefixContextName(context.lineEndContext, prefix);
				context.fallthroughContext = PrefixContextName(context.fallthroughContext, prefix);
				if (!string.IsNullOrEmpty(context.attribute))
					context.attribute = prefix + context.attribute;
				PrefixRules(context.rules, prefix);
			}
			foreach (ItemData itemData in raw.itemDatas)
			{
				itemData.name = prefix + itemData.name;
			}
		}
		
		private static void PrefixRules(List<Rule> rules, string prefix)
		{
			foreach (Rule rule in rules)
			{
				if (rule.type == "keyword")
					rule.String = prefix + rule.String;
				rule.context = PrefixContextName(rule.context, prefix);
				rule.lineEndContext = PrefixContextName(rule.lineEndContext, prefix);
				if (!string.IsNullOrEmpty(rule.attribute))
					rule.attribute = prefix + rule.attribute;
				if (rule.childs.Count > 0)
					PrefixRules(rule.childs, prefix);
			}
		}
		
		private static string PrefixContextName(string name, string prefix)
		{
			if (!string.IsNullOrEmpty(name))
			{
				if (name[0] != '#')
				{
					name = prefix + name;
				}
				else
				{
					int index = name.IndexOf('!');
					if (index != -1)
						name = name.Substring(0, index + 1) + prefix + name.Substring(index + 1);
				}
			}
			return name;
		}
		
		public class Collector
		{
			public Collector(HighlighterSet highlighterSet)
			{
				this.highlighterSet = highlighterSet;
			}
			
			public readonly Dictionary<string, Context> contextByName = new Dictionary<string, Context>();
			public readonly Dictionary<string, ItemData> itemDataByName = new Dictionary<string, ItemData>();
			public readonly Dictionary<string, RawList> listByName = new Dictionary<string, RawList>();
			public readonly Dictionary<string, bool> externalContextAdded = new Dictionary<string, bool>();
			public readonly Dictionary<Context, bool> inlined = new Dictionary<Context, bool>();
			public readonly List<Context> additionContexts = new List<Context>();
			public readonly List<ItemData> additionItemDatas = new List<ItemData>();
			public readonly List<RawList> additionLists = new List<RawList>();
			public readonly HighlighterSet highlighterSet;
		}
		
		public static void InlineIncludeRules(Raw raw, HighlighterSet highlighterSet)
		{
			if (raw == null)
				return;
			Collector collector = new Collector(highlighterSet);
			foreach (RawList list in raw.lists)
			{
				if (list.name != null)
					collector.listByName[list.name.ToLowerInvariant()] = list;
			}
			foreach (Context context in raw.contexts)
			{
				if (context.name != null)
					collector.contextByName[context.name.ToLowerInvariant()] = context;
			}
			foreach (ItemData itemData in raw.itemDatas)
			{
				if (itemData.name != null)
					collector.itemDataByName[itemData.name.ToLowerInvariant()] = itemData;
			}
			
			foreach (Context context in raw.contexts)
			{
				AddExternalContext(collector, ref context.lineEndContext);
				AddExternalContext(collector, ref context.fallthroughContext);
				AddRulesExternalContext(collector, context.rules);
			}
			
			foreach (Context context in raw.contexts)
			{
				RecursiveInlineIncludeRules(context, collector);
			}

			raw.lists.AddRange(collector.additionLists);
			raw.contexts.AddRange(collector.additionContexts);			
			raw.itemDatas.AddRange(collector.additionItemDatas);
		}
		
		private static void RecursiveInlineIncludeRules(Context context, Collector collector)
		{
			if (collector.inlined.ContainsKey(context))
				return;
			collector.inlined[context] = true;
			for (int i = context.rules.Count; i-- > 0;)
			{
				Rule rule = context.rules[i];
				if (rule.type == "IncludeRules" && !string.IsNullOrEmpty(rule.context))
				{
					Context contextToInline;
					collector.contextByName.TryGetValue(rule.context.ToLowerInvariant(), out contextToInline);
					if (contextToInline != null)
					{
						RecursiveInlineIncludeRules(contextToInline, collector);
						context.rules.RemoveAt(i);
						context.rules.InsertRange(i, contextToInline.rules);
					}
				}
			}
		}
		
		private static void AddExternalContext(Collector collector, ref string name)
		{
			if (!string.IsNullOrEmpty(name) && name.Length > 2 && name[0] == '#' && name[1] == '#')
			{
				string key = name.Substring(2).ToLowerInvariant();
				if (!collector.externalContextAdded.ContainsKey(key))
				{
					collector.externalContextAdded[key] = true;
					
					Raw externalRaw = collector.highlighterSet.GetRaw(key);
					if (externalRaw != null && externalRaw.contexts.Count > 0)
					{
						name = externalRaw.contexts[0].name;
						foreach (RawList list in externalRaw.lists)
						{
							string nameI = list.name.ToLowerInvariant();
							if (!collector.listByName.ContainsKey(nameI))
							{
								collector.listByName[nameI] = list;
								collector.additionLists.Add(list);
							}
						}
						foreach (Context context in externalRaw.contexts)
						{
							string nameI = context.name.ToLowerInvariant();
							if (!collector.contextByName.ContainsKey(nameI))
							{
								collector.contextByName[nameI] = context;
								collector.inlined[context] = true;
								collector.additionContexts.Add(context);
							}
						}
						foreach (ItemData itemData in externalRaw.itemDatas)
						{
							string nameI = itemData.name.ToLowerInvariant();
							if (!collector.itemDataByName.ContainsKey(nameI))
							{
								collector.itemDataByName[nameI] = itemData;
								collector.additionItemDatas.Add(itemData);
							}
						}
					}
				}
			}
		}
		
		private static void AddRulesExternalContext(Collector collector, List<Rule> rules)
		{
			foreach (Rule rule in rules)
			{
				AddExternalContext(collector, ref rule.context);
				if (rule.childs.Count > 0)
				{
					AddRulesExternalContext(collector, rule.childs);
				}
			}
		}
	}
}
