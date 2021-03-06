//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
namespace Z80
{
    public interface IMachineCycle
    {
    }

    public class M1Cycle : IMachineCycle {
        private readonly Z80Cpu _cpu;

        public bool IsComplete => _tCycle == 5;
        private int _tCycle = 1;

        private int _interruptWaitCyclesRemaining = -1;

        public M1Cycle(Z80Cpu cpu) {
            _cpu = cpu;
        }

        public void Reset(bool forInterruptAcknowledge = false) {
            _tCycle = 1;
            if (forInterruptAcknowledge) {
                _interruptWaitCyclesRemaining = 2;
            } else {
                _interruptWaitCyclesRemaining = -1;
            }
        }

        public void Clock() {
            var waitThisCycle = false;
            switch (_tCycle) {
                case 1:
                    _cpu.RFRSH = false;
                    _cpu.Address = _cpu.PC;
                    _cpu.RD = true;
                    _cpu.MREQ = true;
                    _cpu.M1 = true;
                    break;
                case 2:
                    waitThisCycle = _cpu.WAIT;
                    break;
                case 3:
                    if (_interruptWaitCyclesRemaining-- > 0)
                    {
                        _cpu.IORQ = true;
                    } 
                    else 
                    {
                        _cpu.IORQ = false;
                        _cpu.Opcode |= _cpu.Data;
                        _cpu.RD = false;
                        _cpu.MREQ = false;
                        _cpu.M1 = false;
                        _cpu.RFRSH = true;
                    }
                    break;
                case 4:
                    // R only increments bits 0-7
                    // If bit 8 has been set via LD R, A it should be preserved 
                    // indefinitely. The following code increments the value and
                    // ensures bit 8 is unchanged afterwards.
                    var workingR = _cpu.R;
                    workingR++;
                    workingR &= 0x7f; // Wipe changes to bit 8
                    _cpu.R = (byte)(workingR | (_cpu.R & 0x80)); // OR with current R value bit 8 only
                    break;
                default:
                    break;
            }
            
            if (!waitThisCycle && (_tCycle != 3 || _interruptWaitCyclesRemaining < 0)) {
                _tCycle++;
            }
        }
    }

    public class InternalCycle : IMachineCycle
    {
        private readonly int _initialTCycleCount;
        private int _remainingTCycles;
        public bool IsComplete => _remainingTCycles == 0;

        public InternalCycle(int tCycleCount) {
            _remainingTCycles = tCycleCount;
            _initialTCycleCount = tCycleCount;
        }

        public void Clock()
        {
            if (_remainingTCycles == 0) {
                throw new InvalidOperationException();
            }
            _remainingTCycles--;
        }

        public void Reset()
        {
            _remainingTCycles = _initialTCycleCount;
        }
    }

    public class MemWriteCycle : IMachineCycle
    {
        private readonly Z80Cpu _cpu;
        private int RemainingTCycles { get; set; } = 3;

        public byte DataToWrite { get; set; }

        public ushort Address { get; set; }

        public bool IsComplete => RemainingTCycles == 0;

        public MemWriteCycle(Z80Cpu cpu) {
            _cpu = cpu;
        }

        public void Reset() {
            RemainingTCycles = 3;
        }

        public void Clock() {
            var waitThisCycle = false;
            switch (RemainingTCycles) {
                case 3:
                    _cpu.Address = Address;
                    _cpu.Data = DataToWrite;
                    _cpu.MREQ = true;
                    break;
                case 2:
                    _cpu.WR = true;
                    waitThisCycle = _cpu.WAIT;
                    break;
                case 1:
                    _cpu.WR = false;
                    _cpu.MREQ = false;
                    break;
            }

            if (!waitThisCycle) {
                RemainingTCycles--;
            }
        }
    }

    public class MemReadCycle : IMachineCycle {
        private readonly Z80Cpu _cpu;
        private int RemainingTCycles { get; set; } = 3;

        public bool IsComplete => RemainingTCycles == 0;
        public byte LatchedData { get; private set; }

        // If address isn't specified the PC is used
        public ushort? Address { get; set; }

        public MemReadCycle(Z80Cpu cpu) {
            _cpu = cpu;
        }

        public void Reset() {
            RemainingTCycles = 3;
        }

        public void Clock() {
            var waitThisCycle = false;

            switch (RemainingTCycles) {
                case 3:
                    _cpu.Address = Address ?? _cpu.PC++;
                    _cpu.RD = true;
                    _cpu.MREQ = true;
                    break;
                case 2:
                    waitThisCycle = _cpu.WAIT;
                    break;
                case 1:
                    LatchedData = _cpu.Data;
                    _cpu.RD = false;
                    _cpu.MREQ = false;
                    break;
            }

            if (!waitThisCycle) {
                RemainingTCycles--;
            }
        }
    }

    public class InputCycle : IMachineCycle {
        private readonly Z80Cpu _cpu;
        private int _tCycle = 1;

        public bool IsComplete => _tCycle == 5;
        public byte LatchedData { get; private set; }
        public ushort Address { get; set; }

        public InputCycle(Z80Cpu cpu) {
            _cpu = cpu;
        }

        public void Reset() {
            _tCycle = 1;
        }

        public void Clock() {
            var waitThisCycle = false;

            switch (_tCycle) {
                case 1:
                    _cpu.Address = Address;
                    break;
                case 2:
                    _cpu.IORQ = true;
                    _cpu.RD = true;
                    break;
                case 3:
                    // Automatically inserted wait
                    waitThisCycle = _cpu.WAIT;
                    break;
                case 4:
                    LatchedData = _cpu.Data;
                    _cpu.RD = false;
                    _cpu.IORQ = false;
                    break;
            }

            if (!waitThisCycle) {
                _tCycle++;
            }
        }
    }

        public class OutputCycle : IMachineCycle {
        private readonly Z80Cpu _cpu;
        private int _tCycle = 1;

        public bool IsComplete => _tCycle == 5;
        public byte DataToOutput { get; set; }
        public ushort Address { get; set; }

        public OutputCycle(Z80Cpu cpu) {
            _cpu = cpu;
        }

        public void Reset() {
            _tCycle = 1;
        }

        public void Clock() {
            var waitThisCycle = false;

            switch (_tCycle) {
                case 1:
                    _cpu.Address = Address;
                    _cpu.Data = DataToOutput;
                    break;
                case 2:
                    _cpu.IORQ = true;
                    _cpu.WR = true;
                    break;
                case 3:
                    // Automatically inserted wait.
                    waitThisCycle = _cpu.WAIT;
                    break;
                case 4:
                    _cpu.WR = false;
                    _cpu.IORQ = false;
                    break;
            }

            if (!waitThisCycle) {
                _tCycle++;
            }
        }
    }
}
