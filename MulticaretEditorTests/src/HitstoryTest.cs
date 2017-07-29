using System;
using System.Collections.Generic;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class HistoryTest
	{
		public class TestCommand : Command
		{
			private HistoryTest test;
			
			public readonly string name;
			
			public TestCommand(CommandType type, HistoryTest test, string name) : base(type)
			{
				this.test = test;
				this.name = name;
			}
			
			override public void Redo()
			{
				test.log += name + ":redo;";
			}
			
			override public void Undo()
			{
				test.log += name + ":undo;";
			}
			
			override public string ToString()
			{
				return name;
			}
		}
		
		private int _commandType;
		
		private TestCommand NewCommand(string name)
		{
			_commandType++;
			return new TestCommand((CommandType)_commandType, this, name);
		}
		
		private string log;
		private History history;
		
		private void AssertLog(string expected)
		{
			Assert.AreEqual(expected, log);
			log = "";
		}
		
		private void Init()
		{
			log = "";
			history = new History();
		}
		
		private void AssertCan(bool expectedUndo, bool expectedRedo)
		{
			Assert.True(
				expectedUndo == history.CanUndo && expectedRedo == history.CanRedo,
				string.Format("(CanUndo, CanRedo) expected:({0}, {1}), got:({2}, {3})", expectedUndo, expectedRedo, history.CanUndo, history.CanRedo)
			);
		}
		
		private class TagsAssertion
		{
			private History history;
			
			public TagsAssertion(History history)
			{
				this.history = history;
			}
			
			public TagsAssertion Ids(params int[] ids)
			{
				bool equals = false;
				if (ids.Length == history.tags.Count)
				{
					equals = true;
					for (int i = 0; i < ids.Length; i++)
					{
						if (ids[i] != history.tags[i].id)
						{
							equals = false;
							break;
						}
					}
				}
				List<int> gotIds = new List<int>();
				foreach (CommandTag tagI in history.tags)
				{
					gotIds.Add(tagI.id);
				}
				Assert.True(equals, "Explected: " + ListUtil.ToString(ids) + ", got: " + ListUtil.ToString(gotIds));
				return this;
			}
			
			public TagsAssertion HeadId(int id)
			{
				Assert.NotNull(history.Head, "Head can't be null");
				Assert.True(history.tags.Contains(history.Head), "Tags mast contain head");
				Assert.AreEqual(id, history.Head.id, "Head id");
				return this;
			}
		}
		
		private TagsAssertion AssertTags()
		{
			return new TagsAssertion(history);
		}
		
		[Test]
		public void UndoRedo_Line()
		{
			Init();
			
			AssertCan(false, false);
			
			history.ExecuteInited(NewCommand("0"));
			AssertCan(true, false);
			history.ExecuteInited(NewCommand("1"));
			AssertLog("0:redo;1:redo;");
			AssertCan(true, false);
			
			history.Undo();
			AssertLog("1:undo;");
			AssertCan(true, true);
			
			history.Undo();
			AssertLog("0:undo;");
			AssertCan(false, true);
			
			history.Redo();
			AssertLog("0:redo;");
			AssertCan(true, true);
			
			history.Redo();
			AssertLog("1:redo;");
			AssertCan(true, false);
		}
		
		[Test]
		public void UndoRedo_Line_SuperfluousCallsDoNothing()
		{
			Init();
			
			AssertCan(false, false);
			history.Undo();
			AssertLog("");
			history.Redo();
			AssertLog("");
			AssertCan(false, false);
			
			history.ExecuteInited(NewCommand("0"));
			history.ExecuteInited(NewCommand("1"));
			AssertLog("0:redo;1:redo;");
			AssertCan(true, false);
			
			history.Redo();
			AssertLog("");
			
			history.Undo();
			history.Undo();
			AssertLog("1:undo;0:undo;");
			AssertCan(false, true);
			
			history.Undo();
			AssertLog("");
			
			history.Redo();
			history.Redo();
			AssertLog("0:redo;1:redo;");
			AssertCan(true, false);
			
			history.Redo();
			AssertLog("");
		}
		
		[Test]
		public void ExecuteAfterUndo_NewTagCreation()
		{
			Init();
			
			Assert.AreEqual(1, history.tags.Count);
			AssertCan(false, false);
			
			history.ExecuteInited(NewCommand("0"));
			history.ExecuteInited(NewCommand("1"));
			history.ExecuteInited(NewCommand("2"));
			history.ExecuteInited(NewCommand("3"));
			AssertLog("0:redo;1:redo;2:redo;3:redo;");
			AssertCan(true, false);
			
			history.Undo();
			history.Undo();
			AssertLog("3:undo;2:undo;");
			AssertCan(true, true);
			
			Assert.AreEqual(1, history.tags.Count);
			Assert.AreEqual(0, history.tags[0].id);
			Assert.AreEqual(history.Head, history.tags[0]);
			
			history.ExecuteInited(NewCommand("10"));
			Assert.AreEqual(2, history.tags.Count);
			Assert.AreEqual(0, history.tags[0].id);
			Assert.AreEqual(1, history.tags[1].id);
			Assert.AreEqual(history.Head, history.tags[1]);
			AssertLog("10:redo;");
			history.ExecuteInited(NewCommand("11"));
			history.ExecuteInited(NewCommand("12"));
			AssertLog("11:redo;12:redo;");
			AssertCan(true, false);
			
			Assert.AreEqual(2, history.tags.Count);
			
			history.Undo();
			history.Undo();
			history.Undo();
			history.Undo();
			AssertLog("12:undo;11:undo;10:undo;1:undo;");
			AssertCan(true, true);
			
			Assert.AreEqual(2, history.tags.Count);
			Assert.AreEqual(history.Head, history.tags[1]);
		}
		
		[Test]
		public void Checkout()
		{
			Init();
			
			history.ExecuteInited(NewCommand("0"));
			history.ExecuteInited(NewCommand("1"));
			history.ExecuteInited(NewCommand("2"));
			history.ExecuteInited(NewCommand("3"));
			history.Undo();
			history.Undo();
			history.ExecuteInited(NewCommand("10"));
			history.ExecuteInited(NewCommand("11"));
			history.ExecuteInited(NewCommand("12"));
			AssertLog("0:redo;1:redo;2:redo;3:redo;3:undo;2:undo;10:redo;11:redo;12:redo;");
			AssertTags().Ids(0, 1).HeadId(1);
			
			AssertCan(true, false);
			history.Checkout(history.tags[0]);
			AssertLog("12:undo;11:undo;10:undo;");
			AssertCan(true, true);
			AssertTags().Ids(0, 1).HeadId(0);
			
			history.Redo();
			
			AssertLog("2:redo;");
			AssertTags().Ids(0, 1).HeadId(0);
			history.Checkout(history.tags[1]);
			AssertLog("2:undo;10:redo;11:redo;12:redo;");
		}
		
		[Test]
		public void Checkout_ToRoot()
		{
			Init();
			
			history.ExecuteInited(NewCommand("00"));
			history.ExecuteInited(NewCommand("01"));
			AssertTags().Ids(0).HeadId(0);
			
			history.Undo();
			history.Undo();
			history.ExecuteInited(NewCommand("10"));
			history.ExecuteInited(NewCommand("11"));
			history.Undo();
			history.Undo();
			AssertLog("00:redo;01:redo;01:undo;00:undo;10:redo;11:redo;11:undo;10:undo;");
			AssertTags().Ids(0, 1).HeadId(1);
			
			history.Checkout(history.tags[0]);
			AssertLog("");
			
			history.Redo();
			history.Redo();
			AssertLog("00:redo;01:redo;");
			history.Checkout(history.tags[1]);
			AssertLog("01:undo;00:undo;");
			AssertTags().Ids(0, 1).HeadId(1);
			
			history.Redo();
			AssertLog("10:redo;");
			history.Checkout(history.tags[0]);
			AssertTags().Ids(0, 1).HeadId(0);
			AssertLog("10:undo;00:redo;01:redo;");
		}
		
		[Test]
		public void Checkout_ToHead()
		{
			Init();
			
			history.ExecuteInited(NewCommand("0"));
			history.ExecuteInited(NewCommand("1"));
			history.ExecuteInited(NewCommand("2"));
			history.ExecuteInited(NewCommand("3"));
			history.Undo();
			history.Undo();
			history.ExecuteInited(NewCommand("10"));
			history.ExecuteInited(NewCommand("11"));
			history.ExecuteInited(NewCommand("12"));
			AssertLog("0:redo;1:redo;2:redo;3:redo;3:undo;2:undo;10:redo;11:redo;12:redo;");
			AssertTags().Ids(0, 1).HeadId(1);
			
			history.Checkout(history.Head);
			AssertLog("");
			AssertTags().Ids(0, 1).HeadId(1);
		}
		
		[Test]
		public void Checkout_IncorrectHead()
		{
			Init();
			
			history.ExecuteInited(NewCommand("0"));
			history.ExecuteInited(NewCommand("1"));
			AssertLog("0:redo;1:redo;");
			AssertTags().Ids(0).HeadId(0);
			
			history.Checkout(new CommandTag(2));
			
			AssertLog("");
			AssertTags().Ids(0).HeadId(0);
		}
		
		[Test]
		public void UndosCountConstraints()
		{
			Init();
	
			history.MaxUndosCount = 3;
			Assert.AreEqual(0, history.UndosCount);
			history.ExecuteInited(NewCommand("0"));
			Assert.AreEqual(1, history.UndosCount);
			history.ExecuteInited(NewCommand("1"));
			Assert.AreEqual(2, history.UndosCount);
			history.Undo();
			Assert.AreEqual(1, history.UndosCount);
			history.Undo();
			Assert.AreEqual(0, history.UndosCount);
			
			history.ExecuteInited(NewCommand("11"));
			Assert.AreEqual(1, history.UndosCount);
			history.ExecuteInited(NewCommand("12"));
			Assert.AreEqual(2, history.UndosCount);
			history.ExecuteInited(NewCommand("13"));
			Assert.AreEqual(3, history.UndosCount);
			
			history.ExecuteInited(NewCommand("14"));
			Assert.AreEqual(3, history.UndosCount);
			history.ExecuteInited(NewCommand("15"));
			Assert.AreEqual(3, history.UndosCount);
			
			history.Undo();
			Assert.AreEqual(2, history.UndosCount);
			history.Redo();
			Assert.AreEqual(3, history.UndosCount);
		}
		
		[Test]
		public void ChooseBranchWithHead_OnRemoveOldUnods()
		{
			Init();
			
			history.MaxUndosCount = 3;
			history.ExecuteInited(NewCommand("0"));
			history.ExecuteInited(NewCommand("1"));
			history.ExecuteInited(NewCommand("2"));
			history.Undo();
			history.Undo();
			AssertLog("0:redo;1:redo;2:redo;2:undo;1:undo;");
			
			history.ExecuteInited(NewCommand("10"));
			history.ExecuteInited(NewCommand("11"));
			Assert.AreEqual(3, history.UndosCount);
			history.ExecuteInited(NewCommand("12"));
			history.ExecuteInited(NewCommand("13"));
			Assert.AreEqual(3, history.UndosCount);
			AssertLog("10:redo;11:redo;12:redo;13:redo;");
			
			history.Undo();
			history.Undo();
			history.Undo();
			Assert.AreEqual(0, history.UndosCount);
			AssertLog("13:undo;12:undo;11:undo;");
			history.Undo();
			AssertLog("");
		}
		
		[Test]
		public void RemoveTagsThatWasOnRemovedBranches()
		{
			Init();
			
			history.MaxUndosCount = 4;
			history.ExecuteInited(NewCommand("1"));
			history.ExecuteInited(NewCommand("2"));
			history.ExecuteInited(NewCommand("3"));
			history.ExecuteInited(NewCommand("4"));
			history.Undo();
			history.Undo();
			history.ExecuteInited(NewCommand("21"));
			history.ExecuteInited(NewCommand("22"));
			history.Undo();
			history.ExecuteInited(NewCommand("211"));
			AssertLog("1:redo;2:redo;3:redo;4:redo;4:undo;3:undo;21:redo;22:redo;22:undo;211:redo;");
			AssertTags().Ids(0, 1, 2).HeadId(2);
			
			history.Checkout(history.tags[0]);
			AssertLog("211:undo;21:undo;");
			history.Redo();
			Assert.AreEqual(3, history.UndosCount);
			AssertTags().Ids(0, 1, 2).HeadId(0);
			history.ExecuteInited(NewCommand("31"));
			Assert.AreEqual(4, history.UndosCount);
			AssertTags().Ids(0, 1, 2, 3).HeadId(3);
			history.ExecuteInited(NewCommand("32"));
			Assert.AreEqual(4, history.UndosCount);
			AssertTags().Ids(0, 1, 2, 3).HeadId(3);
			AssertLog("3:redo;31:redo;32:redo;");
	
			history.ExecuteInited(NewCommand("33"));
			Assert.AreEqual(4, history.UndosCount);
			AssertLog("33:redo;");
			AssertTags().Ids(0, 1, 2, 3).HeadId(3);
			
			history.ExecuteInited(NewCommand("34"));
			Assert.AreEqual(4, history.UndosCount);
			AssertLog("34:redo;");
			AssertTags().Ids(0, 3).HeadId(3);
			
			history.Undo();
			history.Undo();
			history.Undo();
			history.Undo();
			AssertLog("34:undo;33:undo;32:undo;31:undo;");
			history.Undo();
			AssertLog("");
			
			// 0-th branch saved
			history.Redo();
			AssertLog("31:redo;");
			AssertTags().HeadId(3);
			history.Checkout(history.tags[0]);
			AssertLog("31:undo;");
			history.Redo();
			AssertLog("4:redo;");
		}
		
		[Test]
		public void RemoveOldOnExecute_IfChangeMaxUndosCount()
		{
			Init();
			
			history.MaxUndosCount = 5;
			
			history.ExecuteInited(NewCommand("0"));
			history.ExecuteInited(NewCommand("1"));
			history.ExecuteInited(NewCommand("2"));
			history.ExecuteInited(NewCommand("3"));
			history.ExecuteInited(NewCommand("4"));
			AssertLog("0:redo;1:redo;2:redo;3:redo;4:redo;");
			Assert.AreEqual(5, history.UndosCount);
			history.ExecuteInited(NewCommand("5"));
			AssertLog("5:redo;");
			Assert.AreEqual(5, history.UndosCount);
			
			history.MaxUndosCount = 6;
			history.ExecuteInited(NewCommand("6"));
			AssertLog("6:redo;");
			Assert.AreEqual(6, history.UndosCount);
			history.ExecuteInited(NewCommand("7"));
			AssertLog("7:redo;");
			Assert.AreEqual(6, history.UndosCount);
			
			history.MaxUndosCount = 5;
			history.ExecuteInited(NewCommand("8"));
			AssertLog("8:redo;");
			Assert.AreEqual(5, history.UndosCount);
			
			history.MaxUndosCount = 3;
			history.ExecuteInited(NewCommand("9"));
			AssertLog("9:redo;");
			Assert.AreEqual(3, history.UndosCount);
			
			history.Undo();
			history.Undo();
			history.Undo();
			AssertLog("9:undo;8:undo;7:undo;");
			history.Undo();
			AssertLog("");
		}
	}
}