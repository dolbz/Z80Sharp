using System;
using Z80.AddressingModes;

namespace Z80.Instructions
{
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

    internal class ExchangeStack : IInstruction
    {
        private Z80Cpu _cpu;
        private IReadAddressedOperand<ushort> _read;
        private IWriteAddressedOperand<ushort> _write;

        private WideRegister _targetRegister;
        private int _additionalReadCycles = 1;
        private int _additionalWriteCycles = 2;

        public string Mnemonic => "EX";

        public bool IsComplete => _additionalWriteCycles <= 0;

        public ExchangeStack(Z80Cpu cpu, WideRegister exchangeRegister)
        {
            _cpu = cpu;
            _targetRegister = exchangeRegister;
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
            _read = null;
            _write = null;
        }

        public void StartExecution()
        {
            var addrMode = new RegIndirectWide(_cpu, WideRegister.SP, false);
            _read = addrMode.Reader;
            _write = addrMode.Writer;
        }
    }
}