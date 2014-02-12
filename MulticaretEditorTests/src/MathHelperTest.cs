using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class MathHelperTest
	{
		[Test]
		public void Clamp_Normal()
		{
			Assert.AreEqual(1, MathHelper.Clamp(-1, 1, 10));
			Assert.AreEqual(1, MathHelper.Clamp(0, 1, 10));
			Assert.AreEqual(1, MathHelper.Clamp(1, 1, 10));
			Assert.AreEqual(2, MathHelper.Clamp(2, 1, 10));
			Assert.AreEqual(5, MathHelper.Clamp(5, 1, 10));
			Assert.AreEqual(9, MathHelper.Clamp(9, 1, 10));
			Assert.AreEqual(10, MathHelper.Clamp(10, 1, 10));
			Assert.AreEqual(10, MathHelper.Clamp(11, 1, 10));
			Assert.AreEqual(10, MathHelper.Clamp(12, 1, 10));
		}
		
		[Test]
		public void Clamp_Constraints()
		{
			Assert.AreEqual(0, MathHelper.Clamp(-2, 0, -1));
			Assert.AreEqual(0, MathHelper.Clamp(-1, 0, -1));
			Assert.AreEqual(0, MathHelper.Clamp(0, 0, -1));
			Assert.AreEqual(0, MathHelper.Clamp(0, 0, -1));
			
			Assert.AreEqual(2, MathHelper.Clamp(0, 2, 0));
			Assert.AreEqual(2, MathHelper.Clamp(1, 2, 0));
			Assert.AreEqual(2, MathHelper.Clamp(2, 2, 0));
			Assert.AreEqual(2, MathHelper.Clamp(3, 2, 0));
		}
	}
}
