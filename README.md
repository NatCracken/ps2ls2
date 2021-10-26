PS2LS
=====

A toolkit for inspecting Planetside 2 files. It should work on other Forgelight files with the same structure, but for now I am not interested in supporting them.

This project was at one point worked on in #REside on irc.planetside-universe.com.

Planetside Universe has since shut down. If you can find an an archive, let me know. I'd like to see what was discussed there.

For now, contact me on discord NatCracken#1770.


Building
=====
Project was created with Visual Studio 2012. I am working on this fork in VS 2019

You need OpenTK that targets .NET Framework 4.x: https://opentk.net/

I am using OpenTK 3.3.2 and OpenTK.GLControl 3.1.0. They can be easilly installed through VS's NuGet manager.


Using
=====
.pack2 files do not contain the names of assets anymore. Thus this program requires a NameList. You can build one with my fork of dbg-pack here: https://github.com/NatCracken/dbg-pack

Once a NameList has been provided, this program will work as just as it did with the old .pack1 files


Credits
=====

Colin Basnett - Original Author
