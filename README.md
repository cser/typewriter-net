Typewriter.NET
==============

Sources
-------

- Text drawing idea and parts of code got from FastColoredTextBox

Copyright (C) Pavel Torgashov, 2011-2013.
Email: pavel\_torgashov@mail.ru.
http://www.codeproject.com/Articles/161871/Fast-Colored-TextBox-for-syntax-highlighting
https://github.com/PavelTorgashov/FastColoredTextBox

This is not fork of FastColoredTextBox because I have not mastered to improve it.
Honestly, I was trying to introduce several cursors. But it was so difficult for me

- Idea of text highlighting got from post about Qutepart/Enki: http://habrahabr.ru/post/188144/
Editor uses Kate highlighting files (because there are already 200 languages for free)

Building
--------

[preview_1]([[https://github.com/cser/typewriter-net/tree/master/TypewriterNET/previews/preview_1.png]] "Typewriter.NET")

Requires NET Framework 2.0 and NSIS(for installer building)

To build product:

1. Add NSIS directory to PATH
2. Execute in repository folder:
	C:\Windows\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe 
3. Run TypewriterNET\typewriter-net-installer.exe

To build and run without installation:

1. Execute in repository folder:
	C:\Windows\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe /target:tw
