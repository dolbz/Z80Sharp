namespace Z80.Instructions.InterruptHandlers {
    public class NMIHandler : IInstruction
    {
        public string Mnemonic => "N/A";

        private PUSH _pcPush;
        private RST _restartInstruction;

        public bool IsComplete => _restartInstruction.IsComplete;

        public NMIHandler(Z80Cpu cpu) {
            _pcPush = new PUSH(cpu, WideRegister.PC);
            _restartInstruction = new RST(cpu, 0x66);
        }

        public void Clock()
        {
            if (!_pcPush.IsComplete) {
                _pcPush.Clock();
                return;
            }
            if (!_restartInstruction.IsComplete) {
                _restartInstruction.Clock();
            }
        }

        public void Reset()
        {
            _pcPush.Reset();
            _restartInstruction.Reset();            
        }

        public void StartExecution()
        {
        }
    }
}