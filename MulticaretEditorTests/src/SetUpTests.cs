using System;
using NUnit.Framework;
using MulticaretEditor;
using System.Windows.Forms;

[SetUpFixture]
public class SetUpTests
{
    [SetUp]
    public void Setup()
    {
        ClipboardExecuter.Reset(true);
    }
}