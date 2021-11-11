PS2LS
=====

A toolkit for inspecting Planetside 2 files. It should work on other Forgelight files with the same structure, but they're not a priority.
This project was at one point worked on in #REside on irc.planetside-universe.com.

Planetside Universe has since shut down. If you can find an an archive, let me know. I'd like to see what was discussed there.

For now, contact me on discord NatCracken#1770.


Building
=====
Project was created with Visual Studio 2012. I am working on this iteration in VS 2019

Dependant on OpenTK 3.3.2 and OpenTK.GLControl 3.1.0, Zlib Portable 1.11.0, and Pfim 0.10.1. (NuGet)


Using
=====
.pack2 files do not contain the names of assets anymore. Thus this program requires a NameList. You can build one with my fork of dbg-pack here: https://github.com/NatCracken/dbg-pack

Once a NameList in the right format has been provided, this program will work as just as it did with the old .pack1 files


Credits
=====

Colin Basnett - Original Author of ps2ls

https://github.com/RoyAwesome - Currently Maintains ps2ls

https://github.com/brhumphe - DBG-Pack

https://github.com/RhettVX - Forgelight-Toolbox
