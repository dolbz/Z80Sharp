
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

    public struct IndexedRead : IReadAddressedOperand<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemReadCycle _offsetReadCycle;
        private readonly InternalCycle _internalCycle;
        private readonly MemReadCycle _targetReadCycle;

        public byte AddressedValue { get; private set; }

        public bool IsComplete => _targetReadCycle.IsComplete;

        public IndexedRead(Z80Cpu cpu, Register register)
        {
            if (register != Register.IX && register != Register.IY)
            {
                throw new InvalidOperationException("Invald index register specified");
            }
            AddressedValue = 0;
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
                    _targetReadCycle.Address = _offsetReadCycle.LatchedData;
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

    public struct ExtendedPointerOperand : IReadAddressedOperand<byte>
    {
        private readonly ExtendedOperand _extendedOperand;
        private readonly MemReadCycle _readCycle;

        public byte AddressedValue { get; private set; }

        public bool IsComplete => _readCycle.IsComplete;

        public ExtendedPointerOperand(Z80Cpu cpu) {
            _extendedOperand = new ExtendedOperand(cpu);
            _readCycle = new MemReadCycle(cpu);
            AddressedValue = 0x0;
        }

        public void Clock()
        {
            if (!_extendedOperand.IsComplete) {
                _extendedOperand.Clock();
                if (_extendedOperand.IsComplete) {
                    _readCycle.Address = _extendedOperand.AddressedValue;
                }
            }
            if (!_readCycle.IsComplete) {
                _readCycle.Clock();
                if (_readCycle.IsComplete) {
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

    public struct ExtendedOperand : IReadAddressedOperand<ushort>
    {
        private readonly MemReadCycle _readCycle;
        private readonly MemReadCycle _readCycle2;

        public ushort AddressedValue { get; private set; }

        public bool IsComplete => _readCycle2.IsComplete;

        public ExtendedOperand(Z80Cpu cpu)
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
            if (!_readCycle2.IsComplete) {
                _readCycle2.Clock();
                if (_readCycle2.IsComplete) {
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
            get => GetRegisterValue();
            set => SetRegisterValue(value);
        }

        public bool IsComplete => true;
        public bool WriteReady => true;

        public RegAddrMode8Bit(Z80Cpu processor, Register register)
        {
            _processor = processor;
            _register = register;
        }

        private byte GetRegisterValue()
        {
            switch (_register)
            {
                case Register.A:
                    return _processor.A;
                case Register.D:
                    return _processor.D;
                default:
                    throw new InvalidOperationException($"Invalid register value: {_register}");
            }
        }

        private void SetRegisterValue(byte value)
        {
            switch (_register)
            {
                case Register.A:
                    _processor.A = value;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid register value: {_register}");
            }
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