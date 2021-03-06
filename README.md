Z80# - A .NET 5 Z80 emulator
============================

Z80# is a Z80 emulator written in C# and targeting .NET 5. It was written with the primary purpose of supporting [CPC#](https://github.com/dolbz/CPCSharp), an Amstrad CPC464 emulator.

Currently supported:
* Cycle accurate implementation of all documented instructions
* Non-maskable interrupts
* Interrupt mode 1
* The undocumented 0xdd and 0xfd prefixed instructions

Not yet available:
* BUSRQ
* Interrupt modes 0 and 2
* Remaining undocumented instructions/behaviours

Note:
The emulator doesn't pass all [ZEXDOC](http://mdfs.net/Software/Z80/Exerciser/ "The ZEXALL/ZEXDOC Z80 instruction exercisers") tests and won't pass ZEXALL as the undocumented flags haven't been implemented yet. In practice I haven't seen unexpected behaviour when running software on CPC# but eventually I want both ZEXALL and ZEXDOC to pass in full.

