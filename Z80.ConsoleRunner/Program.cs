//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
﻿using System.IO;
using System.Diagnostics;
using System;
using System.Threading;
using Terminal.Gui;
using System.Collections.Generic;
using Attribute = Terminal.Gui.Attribute;
using Z80.Instructions;
using System.Linq;

namespace Z80.ConsoleRunner
{
    class Program
    {
        private static AutoResetEvent uiSignal = new AutoResetEvent(false);
        private static AutoResetEvent instructionSignal = new AutoResetEvent(false);

        private static bool manuallyStepped = true;

        private static byte[] ram = new byte[64*1024];
        private static Z80.Z80Cpu cpu;

        private static bool CpuRunning = false;
        private static bool breakPointHit = false;
        private static List<(int, string)> executedInstructions = new List<(int, string)>();

        public static Action running = MainApp;

        private static ListView programListingListView;
        private static ListView stackListView;
        private static ListView recentlyExecutedListView;
        private static Label clockSpeedValueLabel;
        private static Label addressValueLabel;
        private static Label dataValueLabel;
        private static Label accumulatorValueLabel;
        private static Label signFlagLabel;
        private static Label zeroFlagLabel;
        private static Label undocumentedFlag1Label;
        private static Label halfCarryFlagLabel;
        private static Label undocumentedFlag2Label;
        private static Label parityOverflowFlagLabel;
        private static Label negativeFlagLabel;
        private static Label carryFlagLabel;
        private static Label bRegValueLabel;
        private static Label cRegValueLabel;
        private static Label dRegValueLabel;
        private static Label eRegValueLabel;
        private static Label hRegValueLabel;
        private static Label lRegValueLabel;
        private static Label ixRegValueLabel;
        private static Label iyRegValueLabel;
        private static Label spRegValueLabel;
        private static Label pcRegValueLabel;

        private static Stopwatch stopwatch = Stopwatch.StartNew();
        private static CycleCountObservation lastCycleObservation = new CycleCountObservation();
        public struct CycleCountObservation {
            public double ElapsedMilliseconds { get; set; }
            public long Count { get; set; }
        }
        static void Main(string[] args)
        {
            cpu = new Z80.Z80Cpu();

            cpu.Reset();

            var stopWatch = Stopwatch.StartNew();
            
            // Setup instructions for CPM version of  ZEXDOC/ZEXALL found at https://retrocomputing.stackexchange.com/questions/9361/test-emulated-8080-cpu-without-an-os

            //ram[0x0005] = 0xc9; // 0xc9 = RET
        
            //Console.WriteLine("Setting PC to 0x100");
            //cpu.PC = 0x100;

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
            programListingListView = new ListView() {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            programListingFrame.Add(programListingListView);
            
            var cpuStatusFrame = new FrameView("CPU Status") {
                X = Pos.Percent(50),
                Y = 0,
                Width = Dim.Percent(50),
                Height = Dim.Percent(50)
            };
            var clockSpeedLabel = new Label("Clock Speed: ") {
                X = 2,
                Y = 2
            };
            cpuStatusFrame.Add(clockSpeedLabel);

            clockSpeedValueLabel = new Label("Manually Stepped") 
            {
                X = Pos.Right(clockSpeedLabel),
                Y = 2,
                Width = Dim.Fill()
            };
            cpuStatusFrame.Add(clockSpeedValueLabel);

            var addressLabel = new Label("Address ") {
                X = 2,
                Y = 4
            };
            cpuStatusFrame.Add(addressLabel);

            addressValueLabel = new Label("0x0000") {
                X = Pos.Right(addressLabel),
                Y = 4
            };
            cpuStatusFrame.Add(addressValueLabel);

            var dataLabel = new Label("Data ") {
                X = 2,
                Y = 6
            };
            cpuStatusFrame.Add(dataLabel);

            dataValueLabel = new Label("0x00") {
                X = Pos.Right(dataLabel),
                Y = 6
            };
            cpuStatusFrame.Add(dataValueLabel);

            var registersFrame = BuildRegistersFrame();

            var stackFrame = new FrameView("Stack") {
                X = Pos.Right(registersFrame),
                Y = Pos.Percent(50),
                Width = Dim.Percent(25),
                Height = Dim.Fill()
            };
            stackListView = new ListView() {
                Width = Dim.Fill(),
                Height = Dim.Percent(50)
            };
            stackFrame.Add(stackListView);

            recentlyExecutedListView = new ListView() {
                Y = Pos.Bottom(stackListView),
                Width = Dim.Fill(),
                Height = Dim.Percent(50),
            };
            stackFrame.Add(recentlyExecutedListView);
            
            win.Add(programListingFrame);
            win.Add(cpuStatusFrame);	
            win.Add(registersFrame);
            win.Add(stackFrame);

            var menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Load file", "", () => {
                        var dialog = new OpenDialog("Load file", "Loads the chosen file into the address space") {
                            CanChooseDirectories = false,
                            AllowsMultipleSelection = false
                        };
                        Application.Run(dialog);
                        var chosenFilePath = dialog.FilePath;
                        LoadBinaryIntoRam(0, chosenFilePath.ToString());
                        UpdateCpuUi();
                    }),
                    new MenuItem ("_Quit", "", () => { 
                        running = null; 
                        top.Running = false; 
                        CpuRunning = false; })
                }),
                new MenuBarItem ("_Actions", new MenuItem [] {
                    new MenuItem ("_Reset CPU", "", () => {
                        cpu.Reset();
                    }),
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
                    ToggleRunPaused();
                }),
                new StatusItem(Key.F10, "~F10~ Step", () => {
                    uiSignal.Set();
                    instructionSignal.WaitOne();  
                    UpdateCpuUi();
                })
            });

            top.Add(win, menu, statusBar);

            UpdateCpuUi();
            Application.Run();
            if (runningProgramTimer != null) {
                Application.MainLoop.RemoveTimeout(runningProgramTimer);
            }
            uiSignal.Set();
        }

        static FrameView BuildRegistersFrame() {
             var registersFrame = new FrameView("Registers") {
                X = Pos.Percent(50),
                Y = Pos.Percent(50),
                Width = Dim.Percent(25),
                Height = Dim.Fill()
            };

            var accumulatorLabel = new Label("Acc: ") {
                X = 2,
                Y = 1
            };
            registersFrame.Add(accumulatorLabel);

            accumulatorValueLabel = new Label("0x00") {
                X = Pos.Right(accumulatorLabel),
                Y = 1,
                Width = 6
            };
            registersFrame.Add(accumulatorValueLabel);

            var flagsLabel = new Label("Flags: ")
            {
                X = Pos.Right(accumulatorValueLabel),
                Y = 1
            };
            registersFrame.Add(flagsLabel);

            signFlagLabel = new Label("S ") {
                X = Pos.Right(flagsLabel),
                Y = 1
            };
            registersFrame.Add(signFlagLabel);

            zeroFlagLabel = new Label("Z ") {
                X = Pos.Right(signFlagLabel),
                Y = 1
            };
            registersFrame.Add(zeroFlagLabel);

            undocumentedFlag1Label = new Label("X ") {
                X = Pos.Right(zeroFlagLabel),
                Y = 1
            };
            registersFrame.Add(undocumentedFlag1Label);

            halfCarryFlagLabel = new Label("H ") {
                X = Pos.Right(undocumentedFlag1Label),
                Y = 1
            };
            registersFrame.Add(halfCarryFlagLabel);

            undocumentedFlag2Label = new Label("X ") {
                X = Pos.Right(halfCarryFlagLabel),
                Y = 1
            };
            registersFrame.Add(undocumentedFlag2Label);

            parityOverflowFlagLabel = new Label("P/V ") {
                X = Pos.Right(undocumentedFlag2Label),
                Y = 1
            };
            registersFrame.Add(parityOverflowFlagLabel);

            negativeFlagLabel = new Label("N ") {
                X = Pos.Right(parityOverflowFlagLabel),
                Y = 1
            };
            registersFrame.Add(negativeFlagLabel);

            carryFlagLabel = new Label("C") {
                X = Pos.Right(negativeFlagLabel),
                Y = 1
            };
            registersFrame.Add(carryFlagLabel);

            var bRegLabel = new Label("B: ") {
                X = 2,
                Y = 3
            };
            registersFrame.Add(bRegLabel);

            bRegValueLabel = new Label("0x00") {
                X = Pos.Right(bRegLabel),
                Y = 3,
                Width = 6
            };
            registersFrame.Add(bRegValueLabel);

            var cRegLabel = new Label("C: ") {
                X = Pos.Right(bRegValueLabel),
                Y = 3
            };
            registersFrame.Add(cRegLabel);

            cRegValueLabel = new Label("0x00") {
                X = Pos.Right(cRegLabel),
                Y = 3
            };
            registersFrame.Add(cRegValueLabel);


            var dRegLabel = new Label("D: ") {
                X = 2,
                Y = 5
            };
            registersFrame.Add(dRegLabel);

            dRegValueLabel = new Label("0x00") {
                X = Pos.Right(dRegLabel),
                Y = 5,
                Width = 6
            };
            registersFrame.Add(dRegValueLabel);

            var eRegLabel = new Label("E: ") {
                X = Pos.Right(dRegValueLabel),
                Y = 5
            };
            registersFrame.Add(eRegLabel);

            eRegValueLabel = new Label("0x00") {
                X = Pos.Right(eRegLabel),
                Y = 5
            };
            registersFrame.Add(eRegValueLabel);


            var hRegLabel = new Label("H: ") {
                X = 2,
                Y = 7
            };
            registersFrame.Add(hRegLabel);

            hRegValueLabel = new Label("0x00") {
                X = Pos.Right(hRegLabel),
                Y = 7,
                Width = 6
            };
            registersFrame.Add(hRegValueLabel);

            var lRegLabel = new Label("L: ") {
                X = Pos.Right(hRegValueLabel),
                Y = 7
            };
            registersFrame.Add(lRegLabel);

            lRegValueLabel = new Label("0x00") {
                X = Pos.Right(lRegLabel),
                Y = 7
            };
            registersFrame.Add(lRegValueLabel);


            var ixRegLabel = new Label("IX: ") {
                X = 2,
                Y = 9
            };
            registersFrame.Add(ixRegLabel);

            ixRegValueLabel = new Label("0x0000") {
                X = Pos.Right(ixRegLabel),
                Y = 9
            };
            registersFrame.Add(ixRegValueLabel);


            var iyRegLabel = new Label("IY: ") {
                X = 2,
                Y = 11
            };
            registersFrame.Add(iyRegLabel);

            iyRegValueLabel = new Label("0x0000") {
                X = Pos.Right(iyRegLabel),
                Y = 11
            };
            registersFrame.Add(iyRegValueLabel);


            var spRegLabel = new Label("SP: ") {
                X = 2,
                Y = 13
            };
            registersFrame.Add(spRegLabel);

            spRegValueLabel = new Label("0x0000") {
                X = Pos.Right(spRegLabel),
                Y = 13
            };
            registersFrame.Add(spRegValueLabel);

            var pcRegLabel = new Label("PC: ") {
                X = 2,
                Y = 15
            };
            registersFrame.Add(pcRegLabel);

            pcRegValueLabel = new Label("0x0000") {
                X = Pos.Right(pcRegLabel),
                Y = 15
            };
            registersFrame.Add(pcRegValueLabel);
            return registersFrame;
        }

        static object runningProgramTimer;

        static void ToggleRunPaused() {
            if (runningProgramTimer != null) {
                Application.MainLoop.RemoveTimeout(runningProgramTimer);
                runningProgramTimer = null;
                manuallyStepped = true;
                instructionSignal.WaitOne();
                UpdateCpuUi();
            } else {
                manuallyStepped = false;
                uiSignal.Set();
                runningProgramTimer = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(50), (loop) => {
                    if (!Application.MainLoop.EventsPending()) {
                        if (CpuRunning) {
                            UpdateCpuUi();
                        } else {
                            uiSignal.Set();
                        }
                    }
                    if (breakPointHit) {
                        Application.MainLoop.RemoveTimeout(runningProgramTimer);
                        breakPointHit = false;
                        manuallyStepped = true;
                    }
                    return true; 
                });
            }
        }

        private static void RunCPU() {
            CpuRunning = true;
            var lastSp = cpu.SP;
            while (CpuRunning) {
                if (manuallyStepped || breakPointHit) {
                    instructionSignal.Set();
                    uiSignal.WaitOne(); // Wait until the UI has signalled CPU execution can continue
                }
                
                lock(cpu.CpuStateLock) {
                    do {
                        cpu.Clock();
                        if (cpu.MREQ && cpu.RD) {
                            var data = ram[cpu.Address];
                            cpu.Data = data;
                        }
                        if (cpu.MREQ && cpu.WR) {
                            ram[cpu.Address] = cpu.Data;
                        }
                        // if (cpu.PC == 0x0005 && cpu.NewInstruction) {
                        //     if (cpu.C == 9) {
                        //         var address = WideRegister.DE.GetValue(cpu);
                        //         char chr;
                        //         while ((chr = (char)ram[address++]) != '$') {
                        //             Console.Write(chr);
                        //         }
                        //     } else if (cpu.C == 2) {
                        //         Console.Write((char)cpu.E);
                        //     } else {
                        //         //Console.WriteLine("Unexpected C value during print");
                        //     }
                        // }
                    } while(!cpu.NewInstruction && !breakPointHit);

                    var nextInstruction = InstructionFor(cpu.PC);
                    executedInstructions.Add((cpu.PC, nextInstruction.instruction?.Mnemonic ?? ""));
                }
            }
            instructionSignal.Set();
        }
        
        static string BuildInstructionDescription(IInstruction instruction) {
            var description = string.Empty;

            if (instruction != null) {
                description = instruction.Mnemonic;
            }

            return description;
        }

        static void UpdateCpuUi() {
            var listing = new List<string>();
            long tCycleCount = 0;
            Z80Flags currentFlags;
            CycleCountObservation currentObservation;
            var stackValues = new List<string>();
            var skipNextDisasm = false;
            List<string> instructionsForUi;

            lock(cpu.CpuStateLock) {
                var pc = cpu.PC;
                var startPc = (ushort)(pc-20);
                for (int i = 0; i < 50; i++) {
                    var currentPc = (ushort)(startPc+i);
                    var description = "";

                    if (skipNextDisasm) {
                        skipNextDisasm = false;
                    } else {
                        var instruction = InstructionFor(currentPc);
                        if (instruction.byteCount == 2) {
                            skipNextDisasm = true;
                        }
                        description = instruction.instruction?.Mnemonic ?? "";
                    }

                    listing.Add($" 0x{currentPc:x4}:    0x{ram[currentPc]:x2}    {description}");
                }
                tCycleCount = cpu.TotalTCycles;
                currentObservation = new CycleCountObservation {
                    ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
                    Count = cpu.TotalTCycles
                };

                addressValueLabel.Text = $"0x{cpu.Address:x4}";
                dataValueLabel.Text = $"0x{cpu.Data:x2}";

                currentFlags = cpu.Flags;
                
                accumulatorValueLabel.Text = $"0x{cpu.A:x2}";

                bRegValueLabel.Text = $"0x{cpu.B:x2}";
                cRegValueLabel.Text = $"0x{cpu.C:x2}";

                dRegValueLabel.Text = $"0x{cpu.D:x2}";
                eRegValueLabel.Text = $"0x{cpu.E:x2}";

                hRegValueLabel.Text = $"0x{cpu.H:x2}";
                lRegValueLabel.Text = $"0x{cpu.L:x2}";

                ixRegValueLabel.Text = $"0x{cpu.IX:x4}";
                iyRegValueLabel.Text = $"0x{cpu.IY:x4}";
                spRegValueLabel.Text = $"0x{cpu.SP:x4}";
                pcRegValueLabel.Text = $"0x{cpu.PC:x4}";
                
                for (int i = 0; i < 10; i++) {
                    stackValues.Add($"0x{cpu.SP+i:x4} 0x{ram[cpu.SP+i]:x2}");
                }
                
                instructionsForUi = executedInstructions
                    .AsEnumerable()
                    .Reverse()
                    .Take(20)
                    .Select(x => $"{x.Item1:x4}: {x.Item2}")
                    .ToList();
            }

            stackListView.SetSource(stackValues);

            recentlyExecutedListView.SetSource(instructionsForUi);

            var timeDelta = currentObservation.ElapsedMilliseconds-lastCycleObservation.ElapsedMilliseconds;
            var cycleCountDelta = currentObservation.Count - lastCycleObservation.Count;

            var frequency = (cycleCountDelta/timeDelta)/1_000; // Time delta is already ms so divide by 1,000 instead of 1,000,000 to get MHz

            lastCycleObservation = currentObservation;
            programListingListView.SetSource(listing);
            programListingListView.SelectedItem = 20;

            if (manuallyStepped) {
                clockSpeedValueLabel.Text = $"Manually Stepped";
            } else {
                clockSpeedValueLabel.Text = $"{frequency:0.00}MHz";
            }

            // Flags
            var flagSetColorScheme = new ColorScheme() {
                Normal = Attribute.Make(Color.BrightRed, Color.Blue),
            };

            var defaultColorScheme = accumulatorValueLabel.ColorScheme; // HACK
            signFlagLabel.ColorScheme = defaultColorScheme;
            zeroFlagLabel.ColorScheme = defaultColorScheme;
            halfCarryFlagLabel.ColorScheme = defaultColorScheme;
            parityOverflowFlagLabel.ColorScheme = defaultColorScheme;
            negativeFlagLabel.ColorScheme = defaultColorScheme;
            carryFlagLabel.ColorScheme = defaultColorScheme;

            if (currentFlags.HasFlag(Z80Flags.Sign_S)) {
                signFlagLabel.ColorScheme = flagSetColorScheme;
            }
            if (currentFlags.HasFlag(Z80Flags.Zero_Z)) {
                zeroFlagLabel.ColorScheme = flagSetColorScheme;
            }
            if (currentFlags.HasFlag(Z80Flags.HalfCarry_H)) {
                halfCarryFlagLabel.ColorScheme = flagSetColorScheme;
            }
            if (currentFlags.HasFlag(Z80Flags.ParityOverflow_PV)) {
                parityOverflowFlagLabel.ColorScheme = flagSetColorScheme;
            }
            if (currentFlags.HasFlag(Z80Flags.AddSubtract_N)) {
                negativeFlagLabel.ColorScheme = flagSetColorScheme;
            }
            if (currentFlags.HasFlag(Z80Flags.Carry_C)) {
                carryFlagLabel.ColorScheme = flagSetColorScheme;
            }
        }

        static (IInstruction instruction, int byteCount) InstructionFor(int addr) {
            int opcode = ram[addr];
            var byteCount = 1;
            if (opcode == 0xcb || opcode == 0xdd || opcode == 0xed || opcode == 0xfd) {
                // Pickup next byte as this is a prefixed instruction
                opcode = opcode << 8 | ram[addr + 1];
                byteCount = 2;
            }

            return (cpu.instructions[opcode], byteCount);
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
