using System.IO;
using System.Diagnostics;
using System;
using System.Threading;
using Terminal.Gui;
using System.Collections.Generic;

namespace Z80.ConsoleRunner
{
    class Program
    {
        private static AutoResetEvent uiSignal = new AutoResetEvent(false);
        private static AutoResetEvent instructionSignal = new AutoResetEvent(false);

        private readonly static object cpuState = new object();

        private static bool manuallyStepped = true;

        private static byte[] ram = new byte[64*1024];
        private static Z80.Z80Cpu cpu;

        private static bool CpuRunning = false;

        public static Action running = MainApp;
        static void Main(string[] args)
        {
            cpu = new Z80.Z80Cpu();

            cpu.Reset();

            var stopWatch = Stopwatch.StartNew();
            
            // Setup instructions for CPM version of  ZEXDOC/ZEXALL found at https://retrocomputing.stackexchange.com/questions/9361/test-emulated-8080-cpu-without-an-os

            //ram[0x0005] = 0xc9; // 0xc9 = RET
        
            Console.WriteLine("Setting PC to 0x100");
            cpu.PC = 0x100;

            Console.WriteLine($"Firing up the CPU. Test run started at: {DateTime.Now}\n");

            ThreadStart work = RunCPU;
            Thread thread = new Thread(work);
            thread.Start();


            Console.OutputEncoding = System.Text.Encoding.Default;

            Application.Init();
            while (running != null) {
                running.Invoke ();
            }
            Application.Shutdown ();
        }

        static void MainApp()
        {            
            // int lastInstructionCount = 0;
            // while(true) {
            //     Thread.Sleep(10000);
            //     int currentInstructionCount = cpu.TotalTCycles;
            //     var calculatedMhz = (currentInstructionCount - lastInstructionCount) / 10_000_000.0;
            //     Console.WriteLine($"Running at {calculatedMhz}MHz");
            //     lastInstructionCount = currentInstructionCount;
            // }

            var top = Application.Top;
            var tframe = top.Frame;

            var win = new Window ("Debugger"){
                X = 0,
                Y = 1,
                Width = Dim.Fill (),
                Height = Dim.Fill () - 1
            };
            var programListingFrame = new FrameView("Program Listing") {
                X = 0,
                Y = 0,
                Width = Dim.Percent(50),
                Height = Dim.Fill()
            };
            var programListingListView = new ListView() {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            programListingListView.SetSource(GenerateProgramListing());
            programListingFrame.Add(programListingListView);
            
            var cpuStatusFrame = new FrameView("CPU Status") {
                X = Pos.Percent(50),
                Y = 0,
                Width = Dim.Percent(50),
                Height = Dim.Percent(50)
            };
            cpuStatusFrame.Add(new Label("Clock Speed") {
                X = 2,
                Y = 2
            });
            cpuStatusFrame.Add(new Label("Address") {
                X = 2,
                Y = 4
            });
            cpuStatusFrame.Add(new Label("Data") {
                X = 2,
                Y = 6
            });

            var registersFrame = new FrameView("Registers") {
                X = Pos.Percent(50),
                Y = Pos.Percent(50),
                Width = Dim.Percent(50),
                Height = Dim.Fill()
            };

            win.Add(programListingFrame);
            win.Add(cpuStatusFrame);	
            win.Add(registersFrame);

            var menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Load file", "", () => {
                        var dialog = new OpenDialog("Load file", "Loads the chosen file into the address space") {
                            CanChooseDirectories = false,
                            AllowsMultipleSelection = false
                        };
                        Application.Run(dialog);
                        var chosenFilePath = dialog.FilePath;
                        LoadBinaryIntoRam(0x100, chosenFilePath.ToString());
                        programListingListView.SetSource(GenerateProgramListing());
                    }),
                    new MenuItem ("_Quit", "", () => { 
                        running = null; 
                        top.Running = false; 
                        CpuRunning = false; })
                }),
                new MenuBarItem ("_Edit", new MenuItem [] {
                    new MenuItem ("_Copy", "", null),
                    new MenuItem ("C_ut", "", null),
                    new MenuItem ("_Paste", "", null)
                })
            });

            var statusBar = new StatusBar (new StatusItem [] {
                new StatusItem(Key.F1, "~F1~ Help", null),// () => Help()),
                new StatusItem(Key.F2, "~F2~ Edit RAM", () => { 
                    running = EditRam; 
                    Application.RequestStop ();
                    CpuRunning = false;
                }),
                new StatusItem(Key.F5, "~F5~ Run/Pause", () => {
                    ToggleRunPaused(programListingListView);
                }),
                new StatusItem(Key.F10, "~F10~ Step", () => {
                    uiSignal.Set();
                    instructionSignal.WaitOne();  
                    programListingListView.SetSource(GenerateProgramListing());
                })
            });

            top.Add(win, menu, statusBar);


            Application.Run ();
            if (runLoopIdle != null) {
                Application.MainLoop.RemoveIdle(runLoopIdle);
            }
            uiSignal.Set();
        }

        static Func<bool> runLoopIdle;

        static void ToggleRunPaused(ListView programListingListView) {
            if (runLoopIdle != null) {
                Application.MainLoop.RemoveIdle(runLoopIdle);
                runLoopIdle = null;
                //uiSignal.Reset();
                //instructionSignal.WaitOne();
                programListingListView.SetSource(GenerateProgramListing());
                manuallyStepped = true;
            } else {
                manuallyStepped = false;
                uiSignal.Set();
                runLoopIdle = Application.MainLoop.AddIdle(() => {
                    if (!Application.MainLoop.EventsPending()) {
                        if (CpuRunning) {
                            //instructionSignal.WaitOne();
                            programListingListView.SetSource(GenerateProgramListing());
                            //uiSignal.Set();
                        } else {
                            uiSignal.Set();
                        }
                    }
                    return true; 
                });
            }
        }

        private static void RunCPU() {
            CpuRunning = true;
            while (CpuRunning) {
                if (manuallyStepped) {
                    instructionSignal.Set();
                    uiSignal.WaitOne(); // Wait until the UI has signalled CPU execution can continue
                }
                lock(cpuState) {
                    do {
                        cpu.Clock();
                        if (cpu.MREQ && cpu.RD) {
                            var data = ram[cpu.Address];
                            //Console.WriteLine($"Reading data: {data:X2} from address: {cpu.Address:X4}");
                            cpu.Data = data;
                        }
                        if (cpu.MREQ && cpu.WR) {
                            //Console.WriteLine($"Setting data at address: {cpu.Address:X4} to {cpu.Data:X2}");
                            ram[cpu.Address] = cpu.Data;
                        }
                        if (cpu.PC == 0x0005 && cpu.NewInstruction) {
                            if (cpu.C == 9) {
                                var address = WideRegister.DE.GetValue(cpu);
                                char chr;
                                while ((chr = (char)ram[address++]) != '$') {
                                    Console.Write(chr);
                                }
                            } else if (cpu.C == 2) {
                                Console.Write((char)cpu.E);
                            } else {
                                //Console.WriteLine("Unexpected C value during print");
                            }
                        }
                    } while(!cpu.NewInstruction);
                }
            }
            instructionSignal.Set();
        }
        
        static List<string> GenerateProgramListing() {
            var listing = new List<string>();

            lock(cpuState) {
                var pc = cpu.PC;
                for (int i = pc; i < pc + 50; i++) {
                    if (i < 0x10000) {
                        listing.Add($" 0x{i:x4}:    0x{ram[i]:x2}    {InstructionDescriptionFor(i)}");
                    }
                }
            }

            return listing;
        }

        static string InstructionDescriptionFor(int addr) {
            int opcode = ram[addr];
            if (opcode == 0xcb || opcode == 0xdd || opcode == 0xed || opcode == 0xfd) {
                // Pickup next byte as this is a prefixed instruction
                opcode = opcode << 8 | ram[addr + 1];
            }

            var instruction = cpu.instructions[ram[addr]];
            return instruction?.Mnemonic ?? "";
        }

        public static void EditRam() {
            var ntop = Application.Top;
            var menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Close", "", () => { running = MainApp; Application.RequestStop(); }, null, null),
                }),
            });
            ntop.Add(menu);

            var win = new Window("RAM Editor") {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            ntop.Add (win);

            var source = new MemoryStream(ram);
            var hex = new HexView(source) {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            win.Add(hex);
            Application.Run(ntop);
            hex.ApplyEdits();
        }

        private static void LoadBinaryIntoRam(int startOffset, string filename) {
            var offset = startOffset;

            //Console.WriteLine($"Loading {filename} into RAM starting at 0x{startOffset:X4}");
            using (var fs = File.OpenRead(filename)) {
                while (offset - startOffset < fs.Length) {
                    byte membyte = (byte)fs.ReadByte();
                    ram[offset++] = membyte;
                }
                //Console.WriteLine($"Wrote {offset - startOffset} bytes to RAM");
            }
        }

        private static void PrintCpuState(Z80Cpu cpu) {
            var registers = (Register[])Enum.GetValues(typeof(Register));
            
            //Console.WriteLine($"Total T cycles: {cpu.TotalTCycles}");
            PrintFlags(cpu);
            foreach(var register in registers) {
                if (register != Register.Flags) {
                    PrintRegister(cpu, register);
                }
            }
        }

        private static void PrintFlags(Z80Cpu cpu) {
            Console.Write("Flags: ");
            PrintFlag(cpu, Z80Flags.Sign_S, "S");
            PrintFlag(cpu, Z80Flags.Zero_Z, "Z");
            PrintFlag(cpu, Z80Flags.HalfCarry_H, "H");
            PrintFlag(cpu, Z80Flags.ParityOverflow_PV, "P/V");
            PrintFlag(cpu, Z80Flags.AddSubtract_N, "N");
            PrintFlag(cpu, Z80Flags.Carry_C, "C");
            Console.WriteLine();
        }

        private static void PrintFlag(Z80Cpu cpu, Z80Flags flag, string character) {
            var initialColour = Console.ForegroundColor;
            var colour = (cpu.Flags & flag) == flag ? initialColour : ConsoleColor.Red;
            Console.ForegroundColor = colour;
            Console.Write(character + ' ');
            Console.ForegroundColor = initialColour;
        }
        private static void PrintRegister(Z80Cpu cpu, Register register) {
            byte value;

            switch (register) {
                case Register.A:
                    value = cpu.A;
                    break;
                case Register.D:
                    value = cpu.D;
                    break;
                default:
                    value = 0x0; // TODO
                    break;
            }

            Console.WriteLine($"{register}: {value:X2}");
        }
    }
}
