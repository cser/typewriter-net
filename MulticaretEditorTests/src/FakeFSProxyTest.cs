using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class FakeFSProxyTest
	{
		private FakeFSProxy fs;
		
		[SetUp]
		public void SetUp()
		{
			fs = new FakeFSProxy();
		}
		
		private void AssertFS(string expected)
		{
			string[] lines = expected.Trim().Replace("\r\n", "\n").Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Trim();
			}
			Assert.AreEqual(string.Join("\n", lines), fs.ToString());
		}
		
		[Test]
		public void SimpleToString()
		{
			fs.Add(new FakeFSProxy.FakeDir("c:")
				.Add(new FakeFSProxy.FakeDir("root")
					.Add(new FakeFSProxy.FakeFile("Test.cs", 1))
					.Add(new FakeFSProxy.FakeFile("Test2.cs", 2))
				)
			);
			AssertFS(@"c:
				-root
				--Test.cs{1}
				--Test2.cs{2}
			");
		}
	}
}
