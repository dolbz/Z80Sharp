using Z80.AddressingModes;

namespace Z80.Instructions {
    internal class CALL : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private PUSH _pushInstruction;
        private IReadAddressedOperand<ushort> _reader;
        private IAddressMode<ushort> _addressMode;
        private JumpCondition _jumpCondition;
        private InternalCycle _internalCycle;
        public string Mnemonic => "CALL";
        

        public bool IsComplete { get; private set; } = false;

        public CALL(Z80Cpu cpu, IAddressMode<ushort> addressMode, JumpCondition jumpCondition) {
            _cpu = cpu;
            _addressMode = addressMode;
            _internalCycle = new InternalCycle(1);
            _jumpCondition = jumpCondition;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete) {
                _addressMode.Clock();
                if (_addressMode.IsComplete) {
                    _reader = _addressMode.Reader;
                }
                return;
            }
            if (!_reader.IsComplete) {
                _reader.Clock();
                SetupPushOrComplete();
                return;
            }
            if (!_internalCycle.IsComplete) {
                _internalCycle.Clock();
                return;
            }
            if (!_pushInstruction.IsComplete) {
                _pushInstruction.Clock();
                if (_pushInstruction.IsComplete){
                    _cpu.PC = _reader.AddressedValue;
                    IsComplete = true;
                }
                return;
            }
        }

        public void Reset()
        {
            IsComplete = false;
            _addressMode.Reset();
            _reader = null;
            _pushInstruction = null;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete) {
                _reader = _addressMode.Reader;
                SetupPushOrComplete();
            }
        }

        private void SetupPushOrComplete() {
            if (_reader.IsComplete) {
                if (_jumpCondition.ShouldJump(_cpu)) {
                    _pushInstruction = new PUSH(_cpu, WideRegister.PC, additionalM1TCycles: 0);
                    _pushInstruction.StartExecution();
                } else {
                    IsComplete = true;
                }
            }
        }
    }
}