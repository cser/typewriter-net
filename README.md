Typewriter.NET
==============

Current state (may be another branch):
[![Build status](https://ci.appveyor.com/api/projects/status/ft623rt0w9ewe0f8?svg=true)](https://ci.appveyor.com/project/cser/typewriter-net)
[![Build status](http://flauschig.ch/batch.php?type=tests&account=cser&slug=typewriter-net)](https://ci.appveyor.com/project/cser/typewriter-net)

Last stable installer for download:
[![Build status](https://ci.appveyor.com/api/projects/status/ft623rt0w9ewe0f8/branch/master?svg=true)](https://ci.appveyor.com/project/cser/typewriter-net/branch/master/artifacts)
[![Build status](http://flauschig.ch/batch.php?type=tests&account=cser&slug=typewriter-net&branch=master)](https://ci.appveyor.com/project/cser/typewriter-net/branch/master/artifacts)

![preview_1](https://raw.githubusercontent.com/cser/typewriter-net/master/TypewriterNET/previews/preview_1.png "Typewriter.NET with npp color scheme")

Features
--------
Described in [Wiki](https://github.com/cser/typewriter-net/wiki)

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

3. Encoding detection
	TextFileEncodingDetector.cs got without changes from<br/>
	https://gist.github.com/TaoK/945127

4. Info that using for file opening in same application instance
    http://www.codeproject.com/Tips/1017834/How-to-Send-Data-from-One-Process-to-Another-in-Cs

5. OmniSharp server
	https://github.com/OmniSharp/omnisharp-server

6. JSON parser
	https://github.com/libla/TinyJSON

7. Custom scroll bar (modified)
	http://www.codeproject.com/Articles/41869/Custom-Drawn-Scrollbar

8. Regex for search by char[] instead of string got with changes from mono
	https://github.com/mono/mono/tree/master/mcs/class/referencesource/System/regex/system/text/regularexpressions

9. jsx syntax file
	https://github.com/brunocodutra/kate-jsx

10. Snippets (supported particulary)
	https://github.com/honza/vim-snippets

11. How cut/copy and paste files
	https://www.codeproject.com/Articles/14059/Drag-and-drop-cut-copy-and-paste-files-with-Window

Installation without building
-----------------------------

Download <code>typewriter-net-installer-XX.exe</code> at the root of the repository and run it<br/>
Requires **NET Framework 2.0**

Or you can get [latest stable installer](https://ci.appveyor.com/project/cser/typewriter-net/branch/master/artifacts)

Building
--------

Requires **NET Framework 2.0** (necessarily) and **NSIS** (optional for installer building)

To build and install:

1. Install **NSIS** from http://nsis.sourceforge.net/Download
2. Add **NSIS** directory (for example <code>C:\Program Files (x86)\NSIS</code>) to PATH
3. Execute **MSBuild** in repository folder:<br/>
	<code>C:\Windows\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe /target:run-installer</code><br/>
	(Of cause, you can add **MSBuild** to PATH too)

To build and run without installation:

1. Execute in repository folder:<br/>
	<code>C:\Windows\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe /target:tw</code>
