using System;
using MulticaretEditor;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class LineTest
	{
		private LineArray lines;
		private Line line;

		private void Init(string text)
		{
			lines = new LineArray(200);
			lines.tabSize = 4;
			lines.SetText(text);
			line = lines[0];
		}

		[Test]
		public void PosOf()
		{
			Init("123");

			Assert.AreEqual(0, line.PosOfIndex(0));
			Assert.AreEqual(1, line.PosOfIndex(1));
			Assert.AreEqual(2, line.PosOfIndex(2));
			Assert.AreEqual(3, line.PosOfIndex(3));
			Assert.AreEqual(3, line.PosOfIndex(4));
		}

		[Test]
		public void GetFirstIntegerTabs()
		{
			string text;
			int tabsCount;

			Init("\t123");
			line.GetFirstIntegerTabs(out text, out tabsCount);
			Assert.AreEqual("\t/1", text + "/" + tabsCount);

			Init("123");
			line.GetFirstIntegerTabs(out text, out tabsCount);
			Assert.AreEqual("/0", text + "/" + tabsCount);

			Init("    123");
			line.GetFirstIntegerTabs(out text, out tabsCount);
			Assert.AreEqual("    /1", text + "/" + tabsCount);

			Init("    \t123");
			line.GetFirstIntegerTabs(out text, out tabsCount);
			Assert.AreEqual("    \t/2", text + "/" + tabsCount);

			Init("    \t 123");
			line.GetFirstIntegerTabs(out text, out tabsCount);
			Assert.AreEqual("    \t/2", text + "/" + tabsCount);

			Init("    \t   123");
			line.GetFirstIntegerTabs(out text, out tabsCount);
			Assert.AreEqual("    \t/2", text + "/" + tabsCount);

			Init("    \t    123");
			line.GetFirstIntegerTabs(out text, out tabsCount);
			Assert.AreEqual("    \t    /3", text + "/" + tabsCount);
		}

		[Test]
		public void GetFirstSpaceSize()
		{
			int iChar;

			Init(" word word word");
			Assert.AreEqual("1/1", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("  word word word");
			Assert.AreEqual("2/2", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("   word  word  word");
			Assert.AreEqual("3/3", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("    word  word  word");
			Assert.AreEqual("4/4", line.GetFirstSpaceSize(out iChar) + "/" + iChar);

			Init("\tword word word");
			Assert.AreEqual("4/1", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("\t\tword\tword\tword");
			Assert.AreEqual("8/2", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("\t\t\tword  word  word");
			Assert.AreEqual("12/3", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("\t\t\t word  word  word");
			Assert.AreEqual("13/4", line.GetFirstSpaceSize(out iChar) + "/" + iChar);

			Init(" \t\t\t word  word  word");
			Assert.AreEqual("13/5", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("  \t\t\t word  word  word");
			Assert.AreEqual("13/6", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("   \t\t\t word  word  word");
			Assert.AreEqual("13/7", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("    \t\t\t word  word  word");
			Assert.AreEqual("17/8", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("    \t \t\t word  word  word");
			Assert.AreEqual("17/9", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("    \t   \t\t word  word  word");
			Assert.AreEqual("17/11", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
			Init("    \t    \t\t word  word  word");
			Assert.AreEqual("21/12", line.GetFirstSpaceSize(out iChar) + "/" + iChar);
		}

		[Test]
		public void CalcCutOffs0()
		{
			//              1         2
			//    012345678901234567890123
			Init("word word word word word");
			line.CalcCutOffs(10);
			Assert.AreEqual("[(10/0), (20/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
			line.CalcCutOffs(9);
			Assert.AreEqual("[(5/0), (10/0), (15/0)]", ListUtil.ToString(line.cutOffs.ToArray()));

			Init("word word word word word ");
			line.CalcCutOffs(9);
			Assert.AreEqual("[(5/0), (10/0), (15/0), (20/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
		}

		[Test]
		public void CalcCutOffs1()
		{
			//              1         2
			//    0123456789012345678901234567
			Init("    word word word word word");
			line.CalcCutOffs(20);
			Assert.AreEqual("[(19/4)]", ListUtil.ToString(line.cutOffs.ToArray()));

			//               1         2
			//     0123456789012345678901234
			Init("\tword word word word word");
			line.CalcCutOffs(20);
			Assert.AreEqual("[(16/4)]", ListUtil.ToString(line.cutOffs.ToArray()));
		}

		[Test]
		public void CalcCutOffs2()
		{
			//              1         2         3
			//    0123456789012345678901234567890123
			Init("01234567890123456 0123456789012345");
			line.CalcCutOffs(20);
			Assert.AreEqual("[(18/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
			line.CalcCutOffs(18);
			Assert.AreEqual("[(18/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
			line.CalcCutOffs(17);
			Assert.AreEqual("[(17/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
		}

		[Test]
		public void CalcCutOffs3()
		{
			//              1         2         3
			//    01234567890123456789012345678901234567
			Init("    01234567890123456 0123456789012345");
			line.CalcCutOffs(21);
			Assert.AreEqual("[(21/4)]", ListUtil.ToString(line.cutOffs.ToArray()));
			//              1         2         3
			//    01234567890123456789012345678901234567
			//        0123456789012|
			//        3456         |
			//    0123456789012345 |
			line.CalcCutOffs(17);
			Assert.AreEqual("[(17/4), (22/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
			//              1         2         3
			//    01234567890123456789012345678901234567
			//        012345678901|
			//        23456       |
			//    0123456789012345|
			line.CalcCutOffs(16);
			Assert.AreEqual("[(16/4), (22/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
		}

		[Test]
		public void CalcCutOffs4()
		{
			//              1         2         3
			//    01234567890123456789012345678901234567
			Init("    01234567890123456 0123456789012345");
			//              1         2         3
			//    01234567890123456789012345678901234567
			//        01234567890|
			//        123456     |
			//    012345678901234|
			//    5              |
			line.CalcCutOffs(15);
			Assert.AreEqual("[(15/4), (22/0), (37/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
			//              1         2         3
			//    01234567890123456789012345678901234567
			//        0123456789|
			//        0123456   |
			//    01234567890123|
			//    45            |
			line.CalcCutOffs(14);
			Assert.AreEqual("[(14/4), (22/0), (36/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
			//              1         2         3
			//    01234567890123456789012345678901234567
			//        0123456|
			//    7890123456 |
			//    01234567890|
			//    14523      |
			line.CalcCutOffs(11);
			Assert.AreEqual("[(11/0), (22/0), (33/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
		}

		[Test]
		public void CalcCutOffs5()
		{
			//              1         2         3
			//    01234567890123456789012345678901234567
			Init("    01234567890123456 0123456789012345");
			//              1         2         3
			//    01234567890123456789012345678901234567
			//        012345|
			//    6789012345|
			//    6         |
			//    0123456789|
			//    014523    |
			line.CalcCutOffs(10);
			Assert.AreEqual("[(10/0), (20/0), (22/0), (32/0)]", ListUtil.ToString(line.cutOffs.ToArray()));
		}

		[Test]
		public void CalcCutOffs_LineSize0()
		{
			//              1         2
			//    012345678901234567890123
			Init("word word word word word");
			line.CalcCutOffs(10);
			Assert.AreEqual("[(10/0):10, (20/0):10]:4", StringOfCutOffs(line));
			line.CalcCutOffs(9);
			Assert.AreEqual("[(5/0):5, (10/0):5, (15/0):5]:9", StringOfCutOffs(line));

			Init("word word word word word ");
			line.CalcCutOffs(9);
			Assert.AreEqual("[(5/0):5, (10/0):5, (15/0):5, (20/0):5]:5", StringOfCutOffs(line));
		}

		[Test]
		public void CalcCutOffs_LineSize1()
		{
			//              1         2
			//    0123456789012345678901234567
			Init("    word word word word word");
			line.CalcCutOffs(20);
			//     word word word
			//     word word
			Assert.AreEqual("[(19/4):19]:13", StringOfCutOffs(line));

			//               1         2
			//     0123456789012345678901234
			Init("\tword word word word word");
			line.CalcCutOffs(20);
			//     word word word
			//     word word
			Assert.AreEqual("[(16/4):19]:13", StringOfCutOffs(line));
		}

		[Test]
		public void CalcCutOffs_LineSize2()
		{
			//              1         2         3
			//    01234567890123456789012345678901234567
			Init("    01234567890123456 0123456789012345");
			//              1         2         3
			//    01234567890123456789012345678901234567
			//        012345|
			//    6789012345|
			//    6         |
			//    0123456789|
			//    014523    |
			line.CalcCutOffs(10);
			Assert.AreEqual("[(10/0):10, (20/0):10, (22/0):2, (32/0):10]:6", StringOfCutOffs(line));
		}

		private string StringOfCutOffs(Line line)
		{
			return ListUtil.ToString<CutOff>(line.cutOffs.ToArray(), StringOfCutOff) + ":" + line.lastSublineSizeX;
		}

		private string StringOfCutOff(MulticaretEditor.CutOff cutOff)
		{
			return cutOff + ":" + cutOff.sizeX;
		}
		
		[Test]
		public void IndexOfPos()
		{
			Init("\tabcd");
			Assert.AreEqual(1, line.IndexOfPos(4));
			Init("");
			Assert.AreEqual(0, line.IndexOfPos(0));
			Assert.AreEqual(0, line.IndexOfPos(1));
			Assert.AreEqual(0, line.IndexOfPos(-1));
		}
		
		[Test]
		public void NormalCount()
		{
			Init("\tabcd");
			Assert.AreEqual(5, line.NormalCount);
			Init("\tabcd\n");
			Assert.AreEqual(5, line.NormalCount);
			Init("");
			Assert.AreEqual(0, line.NormalCount);
		}
	}
}
