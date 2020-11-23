namespace Z80.Instructions.InterruptHandlers {
    public class Mode1Handler : IInstruction
    {
        public string Mnemonic => "N/A";

        private PUSH _pcPush;
        private RST _restartInstruction;

        public bool IsComplete => _restartInstruction.IsComplete;

        public Mode1Handler(Z80Cpu cpu) {
            _pcPush = new PUSH(cpu, WideRegister.PC);
            _pcPush.StartExecution();
            _restartInstruction = new RST(cpu, 0x38);
            _restartInstruction.StartExecution();
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