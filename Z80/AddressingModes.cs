
using System;

namespace Z80
{
    public interface IClockable
    {
        void Clock();

        void Reset();

        bool IsComplete { get; }
    }

    public interface IWriteAddressedOperand<T> : IClockable
    {
        T AddressedValue { get; set; }

        bool WriteReady { get; }
    }

    public interface IReadAddressedOperand<T> : IClockable
    {
        T AddressedValue { get; }
    }

    public class RegIndirectWrite : IWriteAddressedOperand<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemWriteCycle _writeCycle;
        private readonly WideRegister _register;

        public byte AddressedValue
        {
            get => _writeCycle.DataToWrite;
            set { _writeCycle.DataToWrite = value; }
        }

        public bool WriteReady => true;

        public bool IsComplete => _writeCycle.IsComplete;

        public RegIndirectWrite(Z80Cpu cpu, WideRegister register)
        {
            _cpu = cpu;
            _writeCycle = new MemWriteCycle(cpu);
            _register = register;
        }

        public void Clock()
        {
            _writeCycle.Clock();
        }

        public void Reset()
        {
            _writeCycle.Reset();
            _writeCycle.Address = _register.GetValue(_cpu);
        }
    }

    public struct RegIndirectRead : IReadAddressedOperand<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemReadCycle _readCycle;
        private readonly WideRegister _register;

        public byte AddressedValue
        {
            get => _readCycle.LatchedData;
        }

        public bool IsComplete => _readCycle.IsComplete;

        public RegIndirectRead(Z80Cpu cpu, WideRegister register)
        {
            _cpu = cpu;
            _readCycle = new MemReadCycle(cpu);
            _register = register;
        }

        public void Clock()
        {
            _readCycle.Clock();
        }

        public void Reset()
        {
            _readCycle.Reset();
            _readCycle.Address = _register.GetValue(_cpu);
        }
    }

    public struct IndexedRead : IReadAddressedOperand<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemReadCycle _offsetReadCycle;
        private readonly InternalCycle _internalCycle;
        private readonly MemReadCycle _targetReadCycle;
        private readonly WideRegister _register;

        public byte AddressedValue { get; private set; }

        public bool IsComplete => _targetReadCycle.IsComplete;

        public IndexedRead(Z80Cpu cpu, WideRegister register)
        {
            if (register != WideRegister.IX && register != WideRegister.IY)
            {
                throw new InvalidOperationException("Invalid index register specified");
            }
            AddressedValue = 0;
            _register = register;
            _cpu = cpu;
            _offsetReadCycle = new MemReadCycle(cpu);
            _internalCycle = new InternalCycle(5);
            _targetReadCycle = new MemReadCycle(cpu);
        }

        public void Reset()
        {
            _offsetReadCycle.Reset();
            _internalCycle.Reset();
            _targetReadCycle.Reset();
        }

        public void Clock()
        {
            if (!_offsetReadCycle.IsComplete)
            {
                _offsetReadCycle.Clock();
                if (_offsetReadCycle.IsComplete)
                {
                    Console.WriteLine("Offset read complete");
                }
                return;
            }

            if (!_internalCycle.IsComplete)
            {
                _internalCycle.Clock();
                if (_internalCycle.IsComplete)
                {
                    _targetReadCycle.Address = (ushort)((_register == WideRegister.IX ? _cpu.IX : _cpu.IY) + (sbyte)_offsetReadCycle.LatchedData);
                    Console.WriteLine("Internal cycle complete");
                }
                return;
            }

            if (!_targetReadCycle.IsComplete)
            {
                _targetReadCycle.Clock();
                if (_targetReadCycle.IsComplete)
                {
                    AddressedValue = _targetReadCycle.LatchedData;
                    Console.WriteLine("Indexed read complete");
                }
            }
        }
    }

    public struct IndexedWrite : IWriteAddressedOperand<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemReadCycle _offsetReadCycle;
        private readonly InternalCycle _internalCycle;
        private readonly MemWriteCycle _targetWriteCycle;
        private readonly WideRegister _register;

        public byte AddressedValue
        {
            get => _targetWriteCycle.DataToWrite;
            set
            {
                _targetWriteCycle.DataToWrite = value;
            }
        }

        public bool WriteReady => _offsetReadCycle.IsComplete;

        public bool IsComplete => _targetWriteCycle.IsComplete;

        public IndexedWrite(Z80Cpu cpu, WideRegister register, int internalCycleLength = 5)
        {
            if (register != WideRegister.IX && register != WideRegister.IY)
            {
                throw new InvalidOperationException("Invald index register specified");
            }
            _register = register;
            _cpu = cpu;
            _offsetReadCycle = new MemReadCycle(cpu);
            _internalCycle = new InternalCycle(internalCycleLength);
            _targetWriteCycle = new MemWriteCycle(cpu);
        }

        public void Reset()
        {
            _offsetReadCycle.Reset();
            _internalCycle.Reset();
            _targetWriteCycle.Reset();
        }

        public void Clock()
        {
            if (!_offsetReadCycle.IsComplete)
            {
                _offsetReadCycle.Clock();
                return;
            }

            if (!_internalCycle.IsComplete)
            {
                _internalCycle.Clock();
                if (_internalCycle.IsComplete)
                {
                    _targetWriteCycle.Address = (ushort)((_register == WideRegister.IX ? _cpu.IX : _cpu.IY) + (sbyte)_offsetReadCycle.LatchedData);
                }
                return;
            }

            if (!_targetWriteCycle.IsComplete)
            {
                _targetWriteCycle.Clock();
            }
        }
    }

    public struct ExtendedPointerWrite : IWriteAddressedOperand<byte>
    {
        private readonly ExtendedReadOperand _extendedOperand;
        private readonly MemWriteCycle _writeCycle;

        public byte AddressedValue
        {
            get => _writeCycle.DataToWrite;
            set
            {
                _writeCycle.DataToWrite = value;
            }
        }

        public bool IsComplete => _writeCycle.IsComplete;

        public bool WriteReady => _extendedOperand.IsComplete;

        public ExtendedPointerWrite(Z80Cpu cpu)
        {
            _extendedOperand = new ExtendedReadOperand(cpu);
            _writeCycle = new MemWriteCycle(cpu);
            AddressedValue = 0x0;
        }

        public void Clock()
        {
            if (!_extendedOperand.IsComplete)
            {
                _extendedOperand.Clock();
                if (_extendedOperand.IsComplete)
                {
                    _writeCycle.Address = _extendedOperand.AddressedValue;
                }
                return;
            }
            if (!_writeCycle.IsComplete)
            {
                _writeCycle.Clock();
            }
        }

        public void Reset()
        {
            _extendedOperand.Reset();
            _writeCycle.Reset();
        }
    }

    public struct ExtendedPointerRead : IReadAddressedOperand<byte>
    {
        private readonly ExtendedReadOperand _extendedOperand;
        private readonly MemReadCycle _readCycle;

        public byte AddressedValue { get; private set; }

        public bool IsComplete => _readCycle.IsComplete;

        public ExtendedPointerRead(Z80Cpu cpu)
        {
            _extendedOperand = new ExtendedReadOperand(cpu);
            _readCycle = new MemReadCycle(cpu);
            AddressedValue = 0x0;
        }

        public void Clock()
        {
            if (!_extendedOperand.IsComplete)
            {
                _extendedOperand.Clock();
                if (_extendedOperand.IsComplete)
                {
                    _readCycle.Address = _extendedOperand.AddressedValue;
                }
                return;
            }
            if (!_readCycle.IsComplete)
            {
                _readCycle.Clock();
                if (_readCycle.IsComplete)
                {
                    AddressedValue = _readCycle.LatchedData;
                }
            }
        }

        public void Reset()
        {
            _extendedOperand.Reset();
            _readCycle.Reset();
        }
    }

    public class ExtendedReadOperand : IReadAddressedOperand<ushort>
    {
        private readonly MemReadCycle _readCycle;
        private readonly MemReadCycle _readCycle2;

        public ushort AddressedValue { get; private set; }

        public bool IsComplete => _readCycle2.IsComplete;

        public ExtendedReadOperand(Z80Cpu cpu)
        {
            _readCycle = new MemReadCycle(cpu);
            _readCycle2 = new MemReadCycle(cpu);
            AddressedValue = 0x0;
        }

        public void Clock()
        {
            if (!_readCycle.IsComplete)
            {
                _readCycle.Clock();
                if (_readCycle.IsComplete)
                {
                    AddressedValue = _readCycle.LatchedData;
                }
                return;
            }
            if (!_readCycle2.IsComplete)
            {
                _readCycle2.Clock();
                if (_readCycle2.IsComplete)
                {
                    AddressedValue |= (ushort)(_readCycle2.LatchedData << 8);
                }
            }
        }

        public void Reset()
        {
            _readCycle.Reset();
            _readCycle2.Reset();
        }
    }

    public struct ImmediateOperand : IReadAddressedOperand<byte>
    {
        private readonly MemReadCycle _readCycle;

        public byte AddressedValue { get; private set; }

        public bool IsComplete => _readCycle.IsComplete;

        public ImmediateOperand(Z80Cpu cpu)
        {
            _readCycle = new MemReadCycle(cpu);
            AddressedValue = 0x0;
        }

        public void Clock()
        {
            if (!_readCycle.IsComplete)
            {
                _readCycle.Clock();
                if (_readCycle.IsComplete)
                {
                    AddressedValue = _readCycle.LatchedData;
                }
            }
        }

        public void Reset()
        {
            _readCycle.Reset();
        }
    }

    public struct RegAddrMode8Bit : IWriteAddressedOperand<byte>, IReadAddressedOperand<byte>
    {
        public readonly Register _register;
        public readonly Z80Cpu _processor;
        public byte AddressedValue
        {
            get => _register.GetValue(_processor);
            set => _register.SetValueOnProcessor(_processor, value);
        }

        public bool IsComplete => true;
        public bool WriteReady => true;

        public RegAddrMode8Bit(Z80Cpu processor, Register register)
        {
            _processor = processor;
            _register = register;
        }

        public void Reset()
        {
            // Nothing to do
        }

        public void Clock()
        {
            throw new InvalidOperationException("This isn't expected to be called as IsComplete is true");
        }
    }
}