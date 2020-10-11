namespace Z80.Instructions {
    internal class RET : POP {
        private readonly JumpCondition _condition;
        private readonly int _additionalM1Cycles;
        private int _remainingM1Cycles;
        public override string Mnemonic => "RET";

        private bool _isComplete = false;
        public override bool IsComplete => _isComplete;
        public RET(Z80Cpu cpu, JumpCondition condition) : base(cpu, WideRegister.PC) {
            _remainingM1Cycles = _additionalM1Cycles = condition == JumpCondition.Unconditional ? 0 : 1;
            _condition = condition;
        }

        public override void Clock()
        {
            if (_condition.ShouldJump(_cpu)) {
                if (_remainingM1Cycles-- <= 0)
                {
                    base.Clock();
                    if (base.IsComplete) {
                        _isComplete = true;
                    }
                } 
            }
            else {
                _isComplete = true;
            }
        }

        public override void Reset() {
            _isComplete = false;
            _remainingM1Cycles = _additionalM1Cycles;
            base.Reset();
        }
    }
}