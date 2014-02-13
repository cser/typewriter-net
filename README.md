Typewriter.NET
==============

![preview_1](https://raw2.github.com/cser/typewriter-net/master/TypewriterNET/previews/preview_1.png "Typewriter.NET with npp color scheme")

Sources
-------

1. Text drawing idea and parts of code got from **FastColoredTextBox**

	Copyright (C) Pavel Torgashov, 2011-2013.<br/>
	Email: pavel\_torgashov@mail.ru.<br/>
	http://www.codeproject.com/Articles/161871/Fast-Colored-TextBox-for-syntax-highlighting<br/>
	https://github.com/PavelTorgashov/FastColoredTextBox

	This is not fork of **FastColoredTextBox** because I have not mastered to improve it.
	Honestly, I was trying to introduce several cursors. But it was so difficult for me

2. Idea of text highlighting got from post about **Qutepart/Enki**: http://habrahabr.ru/post/188144/<br/>
Editor uses **Kate** highlighting files (because there are already 200 languages for free)

Building
--------

Requires **NET Framework 2.0** (necessarily) and **NSIS** (optional for installer building)

To build and install:

1. Install **NSIS** from http://nsis.sourceforge.net/Download
2. Add **NSIS** directory (for example <code>C:\Program Files (x86)\NSIS</code>) to PATH
3. Execute **MSBuild** in repository folder:
	<code>C:\Windows\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe</code><br/>
	(Of cause, you can add **MSBuild** to PATH too)
4. Run TypewriterNET\typewriter-net-installer.exe

To build and run without installation:

1. Execute in repository folder:
	<code>C:\Windows\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe /target:tw</code>
