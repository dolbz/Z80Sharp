using System;
using System.Diagnostics;
namespace Z80
{
    internal interface IInstruction : IClockable
    {
        string Mnemonic { get; }

        // Called on the final clock cycle of the instruction fetch (M1) cycle.
        // If the instruction can be completed without additional cycles it
        // performs the behaviour and IsComplete returns true so that the next
        // fetch cycle can be started immediately. Otherwise the IInstruction object
        // receives future Clock() invocations until IsComplete returns true 
        void StartExecution();
    }

    internal class NOP : IInstruction
    {

        public string Mnemonic => "NOP";

        public bool IsComplete => true;

        public void StartExecution()
        {
            // No operation
        }
        public void Clock()
        {
            // Do no operation
        }

        public void Reset()
        {
            // Nothing to do
        }
    }

    internal class LD_8Bit : LD_Generic<byte>
    {
        public LD_8Bit(Z80Cpu cpu, IWriteAddressedOperand<byte> destination, IReadAddressedOperand<byte> source, int additionalM1TCycles = 0)
            : base(cpu, destination, source, additionalM1TCycles)
        {
        }
    }

    internal class LD_16Bit : LD_Generic<ushort>
    {
        public LD_16Bit(Z80Cpu cpu, IWriteAddressedOperand<ushort> destination, IReadAddressedOperand<ushort> source, int additionalM1TCycles = 0)
            : base(cpu, destination, source, additionalM1TCycles)
        {
        }
    }
    internal abstract class LD_Generic<T> : IInstruction
    {
        public bool IsComplete => _remainingM1Cycles <= 0 && _destination.IsComplete && _source.IsComplete;
        public string Mnemonic { get; }

        protected Z80Cpu _cpu;
        protected IWriteAddressedOperand<T> _destination;
        private IReadAddressedOperand<T> _source;

        private readonly int _additionalM1TCycles;
        private int _remainingM1Cycles;

        public LD_Generic(Z80Cpu cpu, IWriteAddressedOperand<T> destination, IReadAddressedOperand<T> source, int additionalM1TCycles)
        {
            _cpu = cpu;
            Mnemonic = "LD";
            _additionalM1TCycles = additionalM1TCycles;
            _remainingM1Cycles = additionalM1TCycles;
            _destination = destination;
            _source = source;
        }

        public virtual void StartExecution()
        {
            if (_source.IsComplete)
            {
                _destination.AddressedValue = _source.AddressedValue;
            }
        }

        public void Reset()
        {
            _source.Reset();
            _destination.Reset();
            _remainingM1Cycles = _additionalM1TCycles;
        }

        public virtual void Clock()
        {
            if (_remainingM1Cycles-- <= 0)
            {
                // Destination is checked first as its operand comes first if there are 
                // additional bytes to the instruction.
                if (!_destination.WriteReady)
                {
                    _destination.Clock();
                    return;
                }
                if (!_source.IsComplete)
                {
                    _source.Clock();

                    if (_source.IsComplete)
                    {
                        _destination.AddressedValue = _source.AddressedValue;
                    }
                    return;
                }

                if (!_destination.IsComplete)
                {
                    _destination.Clock();
                }
            }
        }
    }

    internal class PUSH : LD_Generic<ushort>
    {
        public PUSH(Z80Cpu cpu, WideRegister register) : base(cpu, new RegIndirectWideWrite(cpu, WideRegister.SP, true), new RegAddrMode16Bit(cpu, register), additionalM1TCycles: 1)
        {

        }

        public override void Clock()
        {
            base.Clock();

            if (_destination.IsComplete)
            {
                _cpu.SP -= 2;
            }
        }
    }

    internal class POP : LD_Generic<ushort>
    {
        public POP(Z80Cpu cpu, WideRegister register) : base(cpu, new RegAddrMode16Bit(cpu, register), new RegIndirectWideRead(cpu, WideRegister.SP), additionalM1TCycles: 0)
        {

        }

        public override void Clock()
        {
            base.Clock();

            if (_destination.IsComplete)
            {
                _cpu.SP += 2;
            }
        }
    }

    internal class ExchangeStack : IInstruction
    {
        private Z80Cpu _cpu;
        private RegIndirectWideRead _read;
        private RegIndirectWideWrite _write;

        private WideRegister _targetRegister;
        private int _additionalReadCycles = 1;
        private int _additionalWriteCycles = 2;

        public string Mnemonic => "EX";

        public bool IsComplete => _additionalWriteCycles <= 0;

        public ExchangeStack(Z80Cpu cpu, WideRegister exchangeRegister)
        {
            _cpu = cpu;
            _targetRegister = exchangeRegister;
            _read = new RegIndirectWideRead(cpu, WideRegister.SP);
            _write = new RegIndirectWideWrite(cpu, WideRegister.SP, false);
        }

        public void Clock()
        {
            if (!_read.IsComplete)
            {
                _read.Clock();
                if (_read.IsComplete)
                {
                    _write.AddressedValue = _targetRegister.GetValue(_cpu);
                    _targetRegister.SetValueOnProcessor(_cpu, _read.AddressedValue);
                }
                return;
            }
            if (_additionalReadCycles-- > 0)
            {
                // http://www.baltazarstudios.com/files/xx.html#E3 shows that the cpu read/write signals aren't affected by
                // the extended cycles so we can just do nothing for these cycles
                return;
            }
            if (!_write.IsComplete)
            {
                _write.Clock();
                return;
            }
            _additionalWriteCycles--;
        }

        public void Reset()
        {
            _additionalReadCycles = 1;
            _additionalWriteCycles = 2;
            _read.Reset();
            _write.Reset();
        }

        public void StartExecution()
        {
        }
    }

    internal class Exchange : IInstruction
    {
        private Z80Cpu _cpu;

        private WideRegister[] _registers;

        private WideRegister _exchangeRegister;
        public string Mnemonic => _registers.Length == 1 ? "EX" : "EXX";

        public bool IsComplete => true;

        public Exchange(Z80Cpu cpu, WideRegister[] registers)
        {
            _cpu = cpu;
            _registers = registers;
        }

        public Exchange(Z80Cpu cpu, WideRegister register, WideRegister exchangeRegister = WideRegister.None) : this(cpu, new[] { register })
        {
            _exchangeRegister = exchangeRegister;
        }

        public void Clock()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
        }

        public void StartExecution()
        {
            foreach (var register in _registers)
            {
                var tempStorage = register.GetValue(_cpu);
                var exchangeRegister = GetExchangeRegister(register);
                register.SetValueOnProcessor(_cpu, exchangeRegister.GetValue(_cpu));
                exchangeRegister.SetValueOnProcessor(_cpu, tempStorage);
            }
        }

        private WideRegister GetExchangeRegister(WideRegister register)
        {
            if (_exchangeRegister != WideRegister.None)
            {
                return _exchangeRegister;
            }

            switch (register)
            {
                case WideRegister.AF:
                    return WideRegister.AF_;
                case WideRegister.BC:
                    return WideRegister.BC_;
                case WideRegister.DE:
                    return WideRegister.DE_;
                case WideRegister.HL:
                    return WideRegister.HL_;
                default:
                    throw new InvalidOperationException("Unexpected register value for exchange");
            }
        }
    }

    internal class LoadAndXcrement : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly RegIndirectRead _readCycle;
        private readonly RegIndirectWrite _writeCycle;
        private readonly bool _increment;
        private readonly bool _repeats;

        private int _additionalCycles = 2;
        private int _additionalRepeatCycles = 5;

        public string Mnemonic => (_increment ? "LDI" : "LDD") + (_repeats ? "R" : "");

        public bool IsComplete { get; private set;}

        public LoadAndXcrement(Z80Cpu cpu, bool increment, bool withRepeat = false)
        {
            _cpu = cpu;
            _increment = increment;
            _readCycle = new RegIndirectRead(cpu, WideRegister.HL);
            _writeCycle = new RegIndirectWrite(cpu, WideRegister.DE);
            _repeats = withRepeat;
        }

        public void Clock()
        {
            // Read from HL pointed address
            if (!_readCycle.IsComplete)
            {
                Console.WriteLine("Read cycle");
                _readCycle.Clock();
                if (_readCycle.IsComplete)
                {
                    _writeCycle.AddressedValue = _readCycle.AddressedValue;
                }
                return;
            }
            if (!_writeCycle.IsComplete)
            {
                Console.WriteLine("Write cycle");
                _writeCycle.Clock();
                return;
            }

            var bcValue = WideRegister.BC.GetValue(_cpu);
            if (((!_repeats || bcValue == 1) && --_additionalCycles > 0) || (_repeats && _additionalCycles-- > 0)) // If it's not repeating we need the last cycle of the additional cycles to carry out the instruction
            {
                return;
            }

            if (bcValue != 1 && _repeats && --_additionalRepeatCycles > 0){ // Prefix decrement here so we use the last addtional cycle to actually carry out the instruction
                return;
            }

            var deValue = WideRegister.DE.GetValue(_cpu);
            var hlValue = WideRegister.HL.GetValue(_cpu);

            if (_increment)
            {
                deValue++;
                hlValue++;
            }
            else
            {
                deValue--;
                hlValue--;
            }

            WideRegister.DE.SetValueOnProcessor(_cpu, deValue);
            WideRegister.HL.SetValueOnProcessor(_cpu, hlValue);

            WideRegister.BC.SetValueOnProcessor(_cpu, --bcValue);

            Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, bcValue == 0);
            if (bcValue != 0)
            {
                _cpu.Flags &= ~Z80Flags.ParityOverflow_PV;
                if (_repeats)
                {
                    _cpu.PC -= 2;
                }
            }
            IsComplete = true;
        }

        public void Reset()
        {
            _readCycle.Reset();
            _writeCycle.Reset();
            _additionalCycles = 2;
            _additionalRepeatCycles = 5;
            IsComplete = false;
        }

        public void StartExecution()
        {
        }
    }
}