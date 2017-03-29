using System;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using Pcre;

namespace MulticaretEditor
{
	public class Highlighter
	{
		public const int MaxStylesCount = 124;

		public string type;

		private Rules.Context[] contexts;
		private Dictionary<string, StyleData> styleDataOf;
		private StyleData defaultStyleData;
		private Dictionary<string, string[]> textListOf;
		private Dictionary<string, Rules.Context> contextOf;
		private List<StyleData> customStyleDatas;

		public Highlighter(Raw raw)
		{
			styleDataOf = new Dictionary<string, StyleData>();
			textListOf = new Dictionary<string, string[]>();
			customStyleDatas = new List<StyleData>();
			foreach (Raw.RawList listI in raw.lists)
			{
				textListOf[listI.name.ToLowerInvariant()] = listI.items.ToArray();
			}
			{
				bool first = true;
				foreach (Raw.ItemData itemDataI in raw.itemDatas)
				{
					StyleData data = new StyleData();
					data.ds = Ds.GetByName(itemDataI.defStyleNum);
					data.name = itemDataI.name;

					data.color = HighlighterUtil.ParseColor(itemDataI.color);
					data.italic = GetVariantBool(itemDataI.italic);
					data.bold = GetVariantBool(itemDataI.bold);
					data.underline = GetVariantBool(itemDataI.underline);
					data.strikeout = GetVariantBool(itemDataI.strikeout);
					if (data.color == null && data.italic == null && data.bold == null && data.underline == null && data.strikeout == null)
					{
						data.index = data.ds.index;
					}
					else
					{
						data.index = (short)(Ds.all.Count + customStyleDatas.Count);
						customStyleDatas.Add(data);
					}

					styleDataOf[data.name.ToLowerInvariant()] = data;
					if (first)
					{
						first = false;
						defaultStyleData = data;
					}
				}
			}

			List<Rules.Context> contextList = new List<Rules.Context>();
			contextOf = new Dictionary<string, Rules.Context>();
			foreach (Raw.Context contextI in raw.contexts)
			{
				string name = contextI.name;
				Rules.Context context = new Rules.Context();
				context.name = name;
				contextOf[name.ToLowerInvariant()] = context;
				contextList.Add(context);
			}
			List<Rules.RegExpr> regExprRules = new List<Rules.RegExpr>();
			foreach (Raw.Context contextI in raw.contexts)
			{
				Rules.Context context = contextOf[contextI.name.ToLowerInvariant()];
				{
					StyleData styleData = null;
					string attribute = contextI.attribute;
					if (attribute != null)
						styleDataOf.TryGetValue(attribute.ToLowerInvariant(), out styleData);
					if (styleData == null)
						styleData = defaultStyleData;
					context.attribute = styleData;
				}
				StyleData currentAttribute = context.attribute;
				context.lineEndContext = GetSwitchInfo(contextI.lineEndContext);
				context.fallthrough = GetBool(contextI.fallthrough);
				context.fallthroughContext = GetSwitchInfo(contextI.fallthroughContext);

				List<Rules.Rule> contextRules = new List<Rules.Rule>();
				foreach (Raw.Rule ruleI in contextI.rules)
				{
					Rules.Rule rule = ParseRule(ruleI, regExprRules, context.attribute);
					if (rule != null)
						contextRules.Add(rule);
				}
				context.childs = contextRules.ToArray();
			}
			awakePositions = new int[regExprRules.Count];
			for (int i = 0; i < regExprRules.Count; i++)
			{
				Rules.RegExpr rule = regExprRules[i];
				rule.awakePositions = awakePositions;
				rule.awakeIndex = i;
			}
			contexts = contextList.ToArray();
			styleDataOf = null;
			contextOf = null;
			textListOf = null;
		}

		private bool GetBool(string value, bool altValue)
		{
			return !string.IsNullOrEmpty(value) ? value == "1" || value.ToLowerInvariant() == "true" : altValue;
		}

		private bool GetBool(string value)
		{
			return !string.IsNullOrEmpty(value) ? value == "1" || value.ToLowerInvariant() == "true" : false;
		}

		private bool? GetVariantBool(string value)
		{
			if (string.IsNullOrEmpty(value))
				return null;
			return value == "1" || value == "true";
		}

		private int GetInt(string value, int defaultValue)
		{
			int result;
			return value != null && int.TryParse(value, out result) ? result : defaultValue;
		}

		private char CharOf(string text)
		{
			return text == null && text.Length == 0 ? '\0' : text[0];
		}

		private Rules.Rule ParseRule(Raw.Rule rawRule, List<Rules.RegExpr> regExprRules, StyleData parentStyleData)
		{
			Rules.Rule commonRule = null;
			if (rawRule.type == "keyword")
			{
				string[] list;
				textListOf.TryGetValue(rawRule.String.ToLowerInvariant(), out list);
				if (list != null)
				{
					if (GetBool(rawRule.general.keywordsCasesensitive, true))
					{
						commonRule = new Rules.KeywordCasesensitive(
							list,
							rawRule.general.keywordsWeakDeliminator ?? "",
							rawRule.general.keywordsAdditionalDeliminator ?? "");
					}
					else
					{
						commonRule = new Rules.Keyword(
							list,
							rawRule.general.keywordsWeakDeliminator ?? "",
							rawRule.general.keywordsAdditionalDeliminator ?? "");
					}
				}
			}
			else if (rawRule.type == "DetectChar")
			{
				Rules.DetectChar rule = new Rules.DetectChar();
				rule.char0 = CharOf(rawRule.char0);
				commonRule = rule;
			}
			else if (rawRule.type == "Detect2Chars")
			{
				Rules.Detect2Chars rule = new Rules.Detect2Chars();
				rule.char0 = CharOf(rawRule.char0);
				rule.char1 = CharOf(rawRule.char1);
				commonRule = rule;
			}
			else if (rawRule.type == "AnyChar")
			{
				Rules.AnyChar rule = new Rules.AnyChar();
				rule.chars = rawRule.String;
				commonRule = rule;
			}
			else if (rawRule.type == "StringDetect")
			{
				Rules.StringDetect rule = new Rules.StringDetect();
				rule.insensitive = GetBool(rawRule.insensitive);
				rule.text = rawRule.String;
				commonRule = rule;
			}
			else if (rawRule.type == "RegExpr")
			{
				Rules.RegExpr rule = new Rules.RegExpr();
				string regex = rawRule.String;
				PcreOptions options = PcreOptions.NO_AUTO_CAPTURE;
				if (GetBool(rawRule.insensitive))
				{
					options |= PcreOptions.CASELESS;
				}
				if (GetBool(rawRule.minimal))
				{
					options |= PcreOptions.UNGREEDY;
				}
				rule.regex = new PcreRegex(
					HighlighterUtil.FixRegexUnicodeChars(regex), options
				);
				commonRule = rule;
				regExprRules.Add(rule);
			}
			else if (rawRule.type == "Int")
			{
				commonRule = new Rules.Int();
			}
			else if (rawRule.type == "Float")
			{
				commonRule = new Rules.Float();
			}
			else if (rawRule.type == "HlCOct")
			{
				commonRule = new Rules.HlCOct();
			}
			else if (rawRule.type == "HlCHex")
			{
				commonRule = new Rules.HlCHex();
			}
			else if (rawRule.type == "RangeDetect")
			{
				Rules.RangeDetect rule = new Rules.RangeDetect();
				rule.char0 = CharOf(rawRule.char0);
				rule.char1 = CharOf(rawRule.char1);
				commonRule = rule;
			}
			else if (rawRule.type == "DetectSpaces")
			{
				commonRule = new Rules.DetectSpaces();
			}
			else if (rawRule.type == "DetectIdentifier")
			{
				commonRule = new Rules.DetectIdentifier();
			}
			else if (rawRule.type == "HlCStringChar")
			{
				commonRule = new Rules.HlCStringChar();
			}
			else if (rawRule.type == "HlCChar")
			{
				commonRule = new Rules.HlCChar();
			}
			else if (rawRule.type == "LineContinue")
			{
				commonRule = new Rules.LineContinue();
			}
			if (commonRule != null)
			{
				{
					StyleData styleData = null;
					string attribute = rawRule.attribute;
					if (attribute != null)
						styleDataOf.TryGetValue(attribute.ToLowerInvariant(), out styleData);
					if (styleData == null)
						styleData = parentStyleData ?? defaultStyleData;
					commonRule.attribute = styleData;
				}
				commonRule.lookAhead = GetBool(rawRule.lookAhead);
				int column = GetInt(rawRule.column, -1);
				if (GetBool(rawRule.firstNonSpace))
				{
					commonRule.column = 0;
				}
				else
				{
					commonRule.column = column;
				}
				commonRule.context = GetSwitchInfo(!string.IsNullOrEmpty(rawRule.context) ? rawRule.context : "#stay");
				List<Rules.Rule> childs = null;
				foreach (Raw.Rule childI in rawRule.childs)
				{
					Rules.Rule child = ParseRule(childI, regExprRules, commonRule.attribute);
					if (child != null)
					{
						if (childs == null)
							childs = new List<Rules.Rule>();
						childs.Add(child);
					}
				}
				if (childs != null)
					commonRule.childs = childs.ToArray();
			}
			return commonRule;
		}

		public static void ParseSwitch(string text, out int pops, out string contextName)
		{
			contextName = null;
			pops = 0;
			if (text == null)
				return;

			int index = 0;
			while (text.IndexOf("#pop", index) == index)
			{
				index += 4;
				pops++;
			}
			if (index > 0 && index < text.Length && text[index] == '!')
			{
				contextName = text.Substring(index + 1);
			}
			else if (text.Length > 0 && text[0] != '#')
			{
				contextName = text;
			}
		}

		private Rules.SwitchInfo GetSwitchInfo(string text)
		{
			string contextName;
			int pops;
			ParseSwitch(text, out pops, out contextName);
			Rules.SwitchInfo info = new Rules.SwitchInfo();
			info.pops = pops;
			if (contextName != null)
				contextOf.TryGetValue(contextName.ToLowerInvariant(), out info.next);
			else
				info.next = null;
			return info;
		}

		private PredictableList<Rules.Context> stack;

		private static bool AreEquals(PredictableList<Rules.Context> a, Rules.Context[] b)
		{
			if (b == null || a.count != b.Length)
				return false;
			for (int i = 0; i < b.Length; i++)
			{
				if (a.buffer[i] != b[i])
					return false;
			}
			return true;
		}

		private static bool AreEquals(Rules.Context[] a, Rules.Context[] b)
		{
			if (a == null || b == null)
				return a == b;
			if (a.Length != b.Length)
				return false;
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
					return false;
			}
			return true;
		}

		private Random random = new Random();

		private bool lastParsingChanged = true;
		public bool LastParsingChanged { get { return lastParsingChanged; } }

		public bool Parse(LineArray lines)
		{
			return Parse(lines, 20);
		}

		private int[] awakePositions;
		
		private Stopwatch _debugStopwatch;

		public bool Parse(LineArray lines, int maxMilliseconds)
		{
			if (_debugStopwatch == null)
			{
				_debugStopwatch = new Stopwatch();
				_debugStopwatch.Start();
			}
			DateTime startTime = DateTime.Now;
			int changesBeforeTimeCheck = 0;
			bool timeElapsed = false;
			bool changed = false;
			stack = new PredictableList<Rules.Context>(8);
			stack.Add(contexts[0]);
			Rules.Context[] state = stack.ToArray();
			bool needSetStack = false;
			bool lastLineChanged = false;
			for (int i = 0; i < lines.blocksCount; i++)
			{
				LineBlock block = lines.blocks[i];
				if (timeElapsed && lastLineChanged)
				{
					while (block.count == 0)
					{
						i++;
						if (i >= lines.blocksCount)
						{
							block = null;
							break;
						}
						block = lines.blocks[i];
					}
					if (block != null)
					{
						block.valid &= ~LineBlock.ColorValid;
						Line line = block.array[0];
						line.startState = state;
						line.endState = null;
					}
					stack = null;
					return changed;
				}
				{
					bool noChangesInBlock = (block.valid & LineBlock.ColorValid) != 0 && !lastLineChanged;
					if (noChangesInBlock && block.count > 0)
					{
						Rules.Context[] nextState = block.array[block.count - 1].endState;
						if (nextState == null)
						{
							noChangesInBlock = false;
						}
						else
						{
							state = nextState;
						}
					}
					if (noChangesInBlock)
					{
						needSetStack = true;
						continue;
					}
				}
				block.valid |= LineBlock.ColorValid;
				for (int j = 0; j < block.count; j++)
				{
					Line line = block.array[j];
					if (line.endState != null && AreEquals(line.startState, state))
					{
						state = line.endState;
						needSetStack = true;
						lastLineChanged = false;
						continue;
					}
					if (needSetStack)
					{
						needSetStack = false;
						stack.Resize(state.Length);
						Array.Copy(state, stack.buffer, stack.count);
					}
					line.startState = state;
					string text = line.Text;
					Array.Clear(awakePositions, 0, awakePositions.Length);
					int position = 0;
					int count = line.charsCount;
					while (position < count)
					{
						Rules.Context context = stack.count > 0 ? stack.Peek() : contexts[0];
						bool ruleMatched = false;
						for (int ri = 0; ri < context.childs.Length; ri++)
						{
							Rules.Rule rule = context.childs[ri];
							int nextPosition;
							if ((rule.column == -1 || position == rule.column) &&
								rule.Match(text, position, out nextPosition))
							{
								if (!rule.lookAhead)
								{
									for (; position < nextPosition; position++)
									{
										line.styles[position] = rule.attribute.index;
									}
								}
								if (rule.childs != null && position < count)
								{
									foreach (Rules.Rule childRule in rule.childs)
									{
										int childNextPosition;
										if (childRule.Match(text, nextPosition, out childNextPosition))
										{
											for (; position < childNextPosition; position++)
											{
												line.styles[position] = childRule.attribute.index;
											}
											Switch(rule.context);
										}
									}
								}
								Switch(rule.context);
								ruleMatched = true;
								break;
							}
						}
						if (!ruleMatched)
						{
							if (context.fallthrough)
							{
								Switch(context.fallthroughContext);
							}
							else
							{
								line.styles[position] = context.attribute.index;
								position++;
							}
						}
					}
					while (stack.count > 0)
					{
						Rules.Context contextI = stack.Peek();
						if (contextI.lineEndContext.next == null && contextI.lineEndContext.pops == 0)
							break;
						Switch(contextI.lineEndContext);
					}
					if (AreEquals(stack, state))
					{
						line.endState = state;
					}
					else
					{
						state = stack.ToArray();
						line.endState = state;
					}
					changesBeforeTimeCheck++;
					lastLineChanged = true;
					changed = true;
				}
				if (changesBeforeTimeCheck > 50)
				{
					changesBeforeTimeCheck = 0;
					DateTime nextTime = DateTime.Now;
					timeElapsed = (nextTime - startTime).TotalMilliseconds > maxMilliseconds;
					if (timeElapsed)
						startTime = nextTime;
				}
			}
			stack = null;
			lastParsingChanged = changed;
			if (!changed && lines.ranges != null)
			{
				foreach (StyleRange range in lines.ranges)
				{
					lines.SetStyleRange(range);
				}
			}
			if (!changed)
			{
				_debugStopwatch.Stop();
				Console.WriteLine("TIME: " + (_debugStopwatch.Elapsed.TotalMilliseconds / 1000).ToString("0.00"));
			}
			return changed;
		}

		private void Switch(Rules.SwitchInfo info)
		{
			for (int i = 0; i < info.pops; i++)
			{
				stack.Pop();
			}
			if (info.next != null)
				stack.Add(info.next);
		}

		public TextStyle[] GetStyles(Scheme scheme)
		{
			TextStyle[] styles = new TextStyle[MaxStylesCount];
			for (int i = 0; i < Ds.all.Count; i++)
			{
				TextStyle style = scheme[Ds.all[i]].Clone();
				styles[i] = style;
			}
			for (int i = 0; i < customStyleDatas.Count && i < MaxStylesCount; i++)
			{
				StyleData data = customStyleDatas[i];
				TextStyle style = scheme[data.ds].Clone();
				if (data.color != null)
					style.brush.Color = data.color.Value;
				if (data.italic != null)
					style.Italic = data.italic.Value;
				if (data.bold != null)
					style.Bold = data.bold.Value;
				if (data.underline != null)
					style.Underline = data.underline.Value;
				if (data.strikeout != null)
					style.Strikeout = data.strikeout.Value;
				styles[Ds.all.Count + i] = style;
			}
			for (int i = Ds.all.Count + customStyleDatas.Count; i < MaxStylesCount; i++)
			{
				styles[i] = new TextStyle();
			}
			return styles;
		}

		public static TextStyle[] GetDefaultStyles(Scheme scheme)
		{
			TextStyle[] styles = new TextStyle[MaxStylesCount];
			for (int i = 0; i < Ds.all.Count; i++)
			{
				TextStyle style = scheme[Ds.all[i]].Clone();
				styles[i] = style;
			}
			for (int i = Ds.all.Count; i < MaxStylesCount; i++)
			{
				styles[i] = new TextStyle();
			}
			return styles;
		}
	}
}
