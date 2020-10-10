using Z80.AddressingModes;

namespace Z80.Instructions {
    internal class CALL : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private PUSH _pushInstruction;
        private MemoryShortReader _extendedReader;
        private JumpCondition _jumpCondition;
        private InternalCycle _internalCycle;
        public string Mnemonic => "CALL";
        

        public bool IsComplete { get; private set; } = false;

        public CALL(Z80Cpu cpu, JumpCondition jumpCondition) {
            _cpu = cpu;
            _extendedReader = new MemoryShortReader(_cpu);
            _internalCycle = new InternalCycle(1);
            _jumpCondition = jumpCondition;
        }

        public void Clock()
        {
            if (!_extendedReader.IsComplete) {
                _extendedReader.Clock();
                if (_extendedReader.IsComplete) {
                    if (_jumpCondition.ShouldJump(_cpu)) {
                        _pushInstruction = new PUSH(_cpu, WideRegister.PC, additionalM1TCycles: 0);
                        _pushInstruction.StartExecution();
                    } else {
                        IsComplete = true;
                    }
                }
                return;
            }
            if (!_internalCycle.IsComplete) {
                _internalCycle.Clock();
                return;
            }
            if (!_pushInstruction.IsComplete) {
                _pushInstruction.Clock();
                if (_pushInstruction.IsComplete){
                    _cpu.PC = _extendedReader.AddressedValue;
                    IsComplete = true;
                }
                return;
            }
        }

        public void Reset()
        {
            IsComplete = false;
            _extendedReader.Reset();
            _pushInstruction = null;
        }

        public void StartExecution()
        {
        }
    }
}