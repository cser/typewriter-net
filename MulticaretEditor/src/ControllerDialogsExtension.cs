using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class ControllerDialogsExtension
	{
		private readonly Controller controller;
		
		public ControllerDialogsExtension(Controller controller)
		{
			this.controller = controller;
		}
		
		private bool needMoveToCaret;
		public bool NeedMoveToCaret { get { return needMoveToCaret; } }
		
		private string needShowError;
		public string NeedShowError { get { return needShowError; } }
		
		private void ResetOutput()
		{
			needMoveToCaret = false;
			needShowError = null;
		}
		
		public void FindNext(string text, bool isRegex, bool isIgnoreCase)
		{
			ResetOutput();
			int index;
			int length;
			if (isRegex)
			{
				string error;
				Regex regex = ParseRegex(text, out error);
				if (regex == null || error != null)
				{
					needShowError = error;
					return;
				}
				Match match = regex.Match(controller.Lines.GetText(), controller.Lines.LastSelection.Right);
				index = -1;
				length = text.Length;
				if (match.Success)
				{
					index = match.Index;
					length = match.Length;
				}
				else
				{
					match = regex.Match(controller.Lines.GetText(), 0);
					if (match.Success)
					{
						index = match.Index;
						length = match.Length;
					}
				}
			}
			else
			{
				length = text.Length;
				CompareInfo ci = isIgnoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
				index = ci != null ?
					ci.IndexOf(controller.Lines.GetText(), text, controller.Lines.LastSelection.Right, CompareOptions.IgnoreCase) :
					controller.Lines.IndexOf(text, controller.Lines.LastSelection.Right);
				if (index == -1)
					index = ci != null ?
						ci.IndexOf(controller.Lines.GetText(), text, 0, CompareOptions.IgnoreCase) :
						controller.Lines.IndexOf(text, 0);
			}
			if (index != -1)
			{
				controller.PutCursorForcedly(controller.Lines.PlaceOf(index), false);
				controller.PutCursorForcedly(controller.Lines.PlaceOf(index + length), true);
				needMoveToCaret = true;
			}
		}
		
		public void SelectNextFound(string text, bool isRegex, bool isIgnoreCase)
		{
			ResetOutput();
			int index;
			int length;
			if (isRegex)
			{
				string error;
				Regex regex = ParseRegex(text, out error);
				if (regex == null || error != null)
				{
					needShowError = error;
					return;
				}
				Match match = regex.Match(controller.Lines.GetText(), controller.Lines.LastSelection.Right);
				index = -1;
				length = text.Length;
				if (match.Success)
				{
					index = match.Index;
					length = match.Length;
				}
				else
				{
					match = regex.Match(controller.Lines.GetText(), 0);
					if (match.Success)
					{
						index = match.Index;
						length = match.Length;
					}
				}
			}
			else
			{
				length = text.Length;
				CompareInfo ci = isIgnoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
				index = ci != null ?
					ci.IndexOf(controller.Lines.GetText(), text, controller.Lines.LastSelection.Right, CompareOptions.IgnoreCase) :
					controller.Lines.IndexOf(text, controller.Lines.LastSelection.Right);
				if (index == -1)
					index = ci != null ?
						ci.IndexOf(controller.Lines.GetText(), text, 0, CompareOptions.IgnoreCase) :
						controller.Lines.IndexOf(text, 0);
			}
			if (index != -1)
			{
				if (controller.Lines.LastSelection.Right != index)
				{
					controller.PutNewCursor(controller.Lines.PlaceOf(index));
					controller.PutCursor(controller.Lines.PlaceOf(index + length), true);
				}
				else
				{
					controller.PutNewCursor(controller.Lines.PlaceOf(index + length));
					controller.PutCursor(controller.Lines.PlaceOf(index), true);
					Selection lastSelection = controller.Lines.selections[controller.Lines.selections.Count - 1];
					if (lastSelection.anchor == index + length && lastSelection.caret == index)
					{
						int temp = lastSelection.anchor;
						lastSelection.anchor = lastSelection.caret;
						lastSelection.caret = temp;
					}
				}
				needMoveToCaret = true;
			}
		}
		
		public bool SelectAllFound(string text, bool isRegex, bool isIgnoreCase)
		{
			return PrivateSelectAllFound(text, isRegex, isIgnoreCase, null);
		}
		
		public bool PrivateSelectAllFound(string text, bool isRegex, bool isIgnoreCase,
			List<SimpleRange> outRanges)
		{
			ResetOutput();
			
			bool result = true;
			string all = controller.Lines.GetText();
			int minIndex = 0;
			int maxIndex = all.Length;
			
			Regex regex = null;
			if (isRegex)
			{
				string error;
				regex = ParseRegex(text, out error);
				if (regex == null || error != null)
				{
					needShowError = error;
					return true;
				}
			}
			
			if (!controller.LastSelection.Empty && controller.SelectionsCount == 1)
			{
				string selectionText = all.Substring(controller.LastSelection.Left, controller.LastSelection.Count);
				bool useArea = false;
				if (isRegex)
				{
					Match match = regex.Match(selectionText);
					useArea = !match.Success || match.Length != selectionText.Length;
				}
				else
				{
					useArea = text != selectionText;
				}
				if (useArea)
				{
					result = false;
					minIndex = controller.LastSelection.Left;
					maxIndex = controller.LastSelection.Right;
				}
			}
			
			LineArray lines = controller.Lines;
			List<Selection> selections = lines.selections;
			int start = minIndex;
			bool found = false;
			bool first = true;
			while (true)
			{
				int index;
				int length;
				if (isRegex)
				{
					Match match = regex.Match(all, start);
					index = -1;
					length = text.Length;
					if (match.Success)
					{
						index = match.Index;
						length = match.Length;
					}
				}
				else
				{
					length = text.Length;
					CompareInfo ci = isIgnoreCase ? CultureInfo.InvariantCulture.CompareInfo : null;
					index = ci != null ?
						ci.IndexOf(all, text, start, CompareOptions.IgnoreCase) :
						all.IndexOf(text, start);
				}
				if (index == -1 || index + length > maxIndex)
				{
					break;
				}
				if (outRanges == null)
				{
					if (first)
					{
						first = false;
						controller.ClearMinorSelections();
					}
					else
					{
						selections.Add(new Selection());
					}
					Selection selection = selections[selections.Count - 1];
					selection.anchor = index;
					selection.caret = index + length;
					lines.SetPreferredPos(selection, lines.PlaceOf(selection.caret));
				}
				else
				{
					outRanges.Add(new SimpleRange(index, length));
				}
				start = index + length;
			}
			needMoveToCaret = found;
			return result;
		}
		
		public void Replace(string text, string newText, bool isRegex, bool isIgnoreCase, bool isEscape)
		{
			if (isEscape)
				newText = Regex.Unescape(newText);
			if (!controller.Lines.AllSelectionsEmpty)
			{
				controller.RemoveEmptyOrMinorSelections();
				controller.InsertText(newText);
			}
			FindNext(text, isRegex, isIgnoreCase);
		}
		
		public void ReplaceAll(string text, string newText, bool isRegex, bool isIgnoreCase, bool isEscape)
		{
			if (isEscape)
				newText = Regex.Unescape(newText);
			List<SimpleRange> ranges = new List<SimpleRange>();
			PrivateSelectAllFound(text, isRegex, isIgnoreCase, ranges);
			if (ranges.Count > 0)
			{
				controller.ReplaceText(ranges.ToArray(), newText);
			}
		}
		
		public static Regex ParseRegex(string regexText, out string error)
		{
			Regex regex = null;
			RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.Multiline;
			string rawRegex;
			if (regexText.Length > 2 && regexText[0] == '/' && regexText.LastIndexOf("/") > 1)
			{
				int lastIndex = regexText.LastIndexOf("/");
				string optionsText = regexText.Substring(lastIndex + 1);
				rawRegex = regexText.Substring(1, lastIndex - 1);
				for (int i = 0; i < optionsText.Length; i++)
				{
					char c = optionsText[i];
					if (c == 'i')
						options |= RegexOptions.IgnoreCase;
					else if (c == 's')
						options &= ~RegexOptions.Multiline;
					else if (c == 'e')
						options |= RegexOptions.ExplicitCapture;
					else
					{
						error = "Unsupported regex option: " + c;
						return null;
					}
				}
			}
			else
			{
				rawRegex = regexText;
			}
			try
			{
				regex = new Regex(rawRegex, options);
			}
			catch (Exception e)
			{
				error = "Incorrect regex: " + regexText + " - " + e.Message;
				return null;
			}
			error = null;
			return regex;
		}
	}
}