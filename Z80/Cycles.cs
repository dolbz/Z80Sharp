using System;
namespace Z80
{
    public interface IMachineCycle : IClockable 
    {
    }

    public class M1Cycle : IMachineCycle {
        private readonly Z80Cpu _cpu;

        public bool IsComplete => RemainingTCycles == 0;
        private int RemainingTCycles { get; set; } = 4;

        public M1Cycle(Z80Cpu cpu) {
            _cpu = cpu;
        }

        public void Reset() {
            RemainingTCycles = 4;
        }

        public void Clock() {
            switch (RemainingTCycles) {
                case 4:
                    _cpu.Address = _cpu.PC++;
                    _cpu.RD = true;
                    _cpu.MREQ = true;
                    break;
                case 3:
                    break;
                case 2:
                    _cpu.Opcode |= _cpu.Data;
                    _cpu.RD = false;
                    _cpu.MREQ = false;
                    break;
                default:
                    // Case 2 and 1 no behaviour needed if RFRSH isn't implemented
                    break;
            }
            
            //if (!_cpu.WAIT) {
            RemainingTCycles--;
            //}
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
            switch (RemainingTCycles) {
                case 3:
                    _cpu.Address = Address;
                    _cpu.Data = DataToWrite;
                    _cpu.MREQ = true;
                    break;
                case 2:
                    _cpu.WR = true;
                    break;
                case 1:
                    _cpu.WR = false;
                    _cpu.MREQ = false;
                    break;
            }

            //if (!_cpu.WAIT) {
            RemainingTCycles--;
            //}
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
            switch (RemainingTCycles) {
                case 3:
                    _cpu.Address = Address ?? _cpu.PC++;
                    _cpu.RD = true;
                    _cpu.MREQ = true;
                    break;
                case 2:
                    break;
                case 1:
                    LatchedData = _cpu.Data;
                    _cpu.RD = false;
                    _cpu.MREQ = false;
                    break;
            }

            //if (!_cpu.WAIT) {
            RemainingTCycles--;
            //}
        }
    }
}
