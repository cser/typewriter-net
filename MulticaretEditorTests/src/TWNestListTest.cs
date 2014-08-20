using System;
using MulticaretEditor.Highlighting;
using NUnit.Framework;

[TestFixture]
public class TWNestListTest
{
	[Test]
	public void Test001()
	{
		NestList list = new NestList(null);
		Assert.AreEqual(null, list.Head);

		Nest nest = list.AddParent();
		Assert.AreEqual(nest, list.Head);
		Assert.AreEqual(list, nest.Owner);
		Assert.AreEqual(null, nest.Parent);
		Assert.AreEqual(null, nest.Child);
	}

	[Test]
	public void Test002()
	{
		NestList list = new NestList(null);
		Assert.AreEqual(null, list.Head);

		Nest nest0 = list.AddParent();
		Nest nest1 = list.AddParent();
		Assert.AreEqual(nest1, list.Head);
		Assert.AreEqual(nest0, list.Head.Child);
		Assert.AreEqual(null, nest1.Parent);
		Assert.AreEqual(nest1, nest0.Parent);
		Assert.AreEqual(nest0, nest1.Child);
		Assert.AreEqual(null, nest0.Child);
	}
}
