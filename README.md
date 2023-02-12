PS2LS
=====

A toolkit for inspecting Planetside 2 files. It should work on other Forgelight files with the same structure, but they're not a priority.

I can be contacted on discord NatCracken#1770.

Building
=====
Project was created with Visual Studio 2012. I am working on this iteration in VS 2019

Dependant on OpenTK 3.3.2 and OpenTK.GLControl 3.1.0, Zlib Portable 1.11.0, and Pfim 0.10.1. (NuGet)


Using
=====
.pack2 files do not contain the names of assets anymore. Thus you will first have to build a namelist using the Build Namelist button in the Asset Browser

Once completed, point ps2ls2 to the namelist using the Add Namelist button and add .pack2 files using the Add Packs button.

Credits
=====
Colin Basnett - Original Author of ps2ls

https://github.com/RoyAwesome - Currently Maintains ps2ls

https://github.com/brhumphe - DBG-Pack

https://github.com/RhettVX - Forgelight-Toolbox
