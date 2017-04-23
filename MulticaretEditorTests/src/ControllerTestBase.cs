using System;
using System.Text;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	public class ControllerTestBase
	{
		protected LineArray lines;
		protected Controller controller;
		
		virtual protected void Init()
		{
			Init(50);
		}
		
		virtual protected void Init(int blockSize)
		{
			lines = new LineArray(blockSize);
			lines.tabSize = 4;
			controller = new Controller(lines);
		}
		
		protected string GetLineText(int index)
		{
			StringBuilder builder = new StringBuilder();
			int Count = lines[index].charsCount;
			for (int i = 0; i < Count; i++)
			{
				builder.Append(lines[index].chars[i].c);
			}
			return builder.ToString();
		}
		
		protected void AssertText(string text)
		{
			Assert.AreEqual(text, lines.GetText());
			Assert.AreEqual(text.Length, lines.charsCount, "Chars count");
		}
		
		protected class SelectionAssertion
		{
			private LineArray provider;
			private int index;
			private Selection selection;
			private string description;
	
			public SelectionAssertion(LineArray provider, int index, string description)
			{
				this.provider = provider;
				this.index = index;
				this.description = description;
				SetSelectionByIndex();
			}
			
			private void SetSelectionByIndex()
			{
				if (index >= provider.selections.Count)
					Assert.Fail("No selection with index: " + index);
				selection = provider.selections[index];
			}
			
			public SelectionAssertion Next()
			{
				this.index++;
				SetSelectionByIndex();
				return this;
			}
			
			public void NoNext()
			{
				Assert.IsTrue(index >= provider.selections.Count - 1,
					(description != null ? description + ":" : "") + "Mast be no next selection");
			}
			
			public SelectionAssertion Anchor(int iChar, int iLine)
			{
				Assert.AreEqual(new Place(iChar, iLine), provider.PlaceOf(selection.anchor),
					(description != null ? description + ":" : "") + "Anchor");
				return this;
			}
			
			public SelectionAssertion Caret(int iChar, int iLine)
			{
				Assert.AreEqual(new Place(iChar, iLine), provider.PlaceOf(selection.caret),
					(description != null ? description + ":" : "") + "Caret");
				return this;
			}
			
			public SelectionAssertion Both(int iChar, int iLine)
			{
				Assert.AreEqual(new Place(iChar, iLine), provider.PlaceOf(selection.anchor),
					(description != null ? description + ":" : "") + "Anchor");
				Assert.AreEqual(new Place(iChar, iLine), provider.PlaceOf(selection.caret),
					(description != null ? description + ":" : "") + "Caret");
				return this;
			}
			
			public SelectionAssertion AbsoluteAnchorCaret(int anchor, int caret)
			{
				Assert.AreEqual(anchor + ", " + caret, selection.anchor + ", " + selection.caret,
					"Absolute anchor, caret");
				return this;
			}
			
			public SelectionAssertion PreferredPos(int pos)
			{
				Assert.AreEqual(pos, selection.preferredPos,
					(description != null ? description + ":" : "") + "PreferredPos");
				return this;
			}

			public SelectionAssertion ModePreferredPos(int pos)
			{
				Assert.AreEqual(pos, provider.wordWrap ? selection.wwPreferredPos : selection.preferredPos,
					(description != null ? description + ":" : "") + "WWPreferredPos");
				return this;
			}
			
			public SelectionAssertion Count(int count)
			{
				Assert.AreEqual(count, provider.selections.Count,
					(description != null ? description + ":" : "") + "SelectionsCount");
				return this;
			}
			
			public SelectionAssertion HasBoth(int iChar, int iLine)
			{
				Place place = new Place(iChar, iLine);
				foreach (Selection selection in provider.selections)
				{
					if (place == provider.PlaceOf(selection.anchor) && place == provider.PlaceOf(selection.caret))
					{
						Assert.True(true);
						return this;
					}
				}
				Assert.Fail((description != null ? description + ":" : "") + "No both: " + place);
				return this;
			}
		}
		
		protected SelectionAssertion AssertSelection()
		{
			return new SelectionAssertion(lines, 0, null);
		}
		
		protected SelectionAssertion AssertSelection(string description)
		{
			return new SelectionAssertion(lines, 0, description);
		}
		
		protected class SizeAssertion
		{
			private LineArray provider;
			
			public SizeAssertion(LineArray provider)
			{
				this.provider = provider;
			}
			
			public SizeAssertion XY(int x, int y)
			{
				Assert.AreEqual(new IntSize(x, y), provider.Size);
				return this;
			}
			
			public SizeAssertion X(int x)
			{
				Assert.AreEqual(x, provider.Size.x);
				return this;
			}
			
			public SizeAssertion Y(int y)
			{
				Assert.AreEqual(y, provider.Size.y);
				return this;
			}
		}
		
		protected SizeAssertion AssertSize()
		{
			return new SizeAssertion(lines);
		}
		
		protected void PutToViClipboard(string text)
		{
			ClipboardExecuter.PutToRegister('\0', text);
		}
		
		protected void AssertViClipboard(string text)
		{
			Assert.AreEqual(text, ClipboardExecuter.GetFromRegister('\0'));
		}
	}
}
