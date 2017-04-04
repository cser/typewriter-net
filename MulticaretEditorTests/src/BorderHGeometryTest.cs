using System;
using NUnit.Framework;
using MulticaretEditor;

namespace UnitTests
{
	[TestFixture]
	public class BorderHGeometryTest
	{
		private BorderHGeometry geometry;
		
		[SetUp]
		public void SetUp()
		{
			geometry = new BorderHGeometry();
			geometry.Begin();
		}
		
		private void Assert0(bool expectedExists, int expectedX0, int expectedX1)
		{
			Assert.AreEqual(
				expectedExists + " (x0=" + expectedX0 + " x1=" + expectedX1 + ")",
				geometry.top0Exists + " (x0=" + geometry.top0X0 + " x1=" + geometry.top0X1 + ")");
		}
		
		private void Assert1(bool expectedExists, int expectedX0, int expectedX1)
		{
			Assert.AreEqual(
				expectedExists + " (x0=" + expectedX0 + " x1=" + expectedX1 + ")",
				geometry.top1Exists + " (x0=" + geometry.top1X0 + " x1=" + geometry.top1X1 + ")");
		}
		
		private void AssertX01(int expectedX0, int expectedX1)
		{
			Assert.AreEqual(
				"[" + expectedX0 + ", " + expectedX1 + "]",
				"[" + geometry.x0 + ", " + geometry.x1 + "]");
		}
		
		[Test]
		public void OneLine()
		{
			Assert.AreEqual(false, geometry.isEnd);
			geometry.AddLine(2, 4);
			Assert0(true, 2, 6);
			Assert.AreEqual(false, geometry.top1Exists);
			AssertX01(2, 6);
			Assert.AreEqual(false, geometry.isEnd);
			
			geometry.End();
			Assert0(true, 2, 6);
			Assert.AreEqual(false, geometry.top1Exists);
			Assert.AreEqual(true, geometry.isEnd);
		}
		
		[Test]
		public void TwoLines1()
		{
			geometry.AddLine(2, 4);
			Assert0(true, 2, 6);
			Assert.AreEqual(false, geometry.top1Exists);
			AssertX01(2, 6);
			
			geometry.AddLine(1, 3);
			Assert0(true, 1, 2);
			Assert1(true, 4, 6);
			AssertX01(1, 4);
			
			geometry.End();
			Assert0(true, 1, 4);
			Assert.AreEqual(false, geometry.top1Exists);
		}
		
		[Test]
		public void TwoLines2()
		{
			geometry.AddLine(2, 4);
			Assert0(true, 2, 6);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.AddLine(5, 3);
			Assert0(true, 2, 5);
			Assert1(true, 6, 8);
			
			geometry.End();
			Assert0(true, 5, 8);
			Assert.AreEqual(false, geometry.top1Exists);
		}
		
		[Test]
		public void TwoLines3()
		{
			geometry.AddLine(2, 4);
			Assert0(true, 2, 6);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.AddLine(1, 7);
			Assert0(true, 1, 2);
			Assert1(true, 6, 8);
			
			geometry.End();
			Assert0(true, 1, 8);
			Assert.AreEqual(false, geometry.top1Exists);
		}
		
		[Test]
		public void TwoLines4()
		{
			geometry.AddLine(2, 4);
			Assert0(true, 2, 6);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.AddLine(3, 2);
			Assert0(true, 2, 3);
			Assert1(true, 5, 6);
			
			geometry.End();
			Assert0(true, 3, 5);
			Assert.AreEqual(false, geometry.top1Exists);
		}
		
		[Test]
		public void TwoLines5()
		{
			geometry.AddLine(4, 3);
			Assert0(true, 4, 7);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.AddLine(1, 2);
			Assert0(true, 4, 7);
			Assert1(true, 1, 3);
			
			geometry.End();
			Assert0(true, 1, 3);
			Assert.AreEqual(false, geometry.top1Exists);
		}
		
		[Test]
		public void TwoLines6()
		{
			geometry.AddLine(4, 3);
			Assert0(true, 4, 7);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.AddLine(9, 2);
			Assert0(true, 4, 7);
			Assert1(true, 9, 11);
			
			geometry.End();
			Assert0(true, 9, 11);
			Assert.AreEqual(false, geometry.top1Exists);
		}
		
		[Test]
		public void TwoLines_Equals()
		{
			geometry.AddLine(4, 3);
			Assert0(true, 4, 7);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.AddLine(4, 3);
			Assert.AreEqual(false, geometry.top0Exists);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.End();
			Assert0(true, 4, 7);
			Assert.AreEqual(false, geometry.top1Exists);
		}
		
		[Test]
		public void TwoLines_EqualsLeft()
		{
			geometry.AddLine(4, 3);
			Assert0(true, 4, 7);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.AddLine(4, 2);
			Assert.AreEqual(false, geometry.top0Exists);
			Assert1(true, 6, 7);
			
			geometry.End();
			Assert0(true, 4, 6);
			Assert.AreEqual(false, geometry.top1Exists);
		}
		
		[Test]
		public void TwoLines_EqualsRight()
		{
			geometry.AddLine(4, 3);
			Assert0(true, 4, 7);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.AddLine(5, 2);
			Assert0(true, 4, 5);
			Assert.AreEqual(false, geometry.top1Exists);
			
			geometry.End();
			Assert0(true, 5, 7);
			Assert.AreEqual(false, geometry.top1Exists);
		}
	}
}
