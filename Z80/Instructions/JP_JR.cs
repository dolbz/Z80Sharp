using System;
using Z80.AddressingModes;

namespace Z80.Instructions {
    public enum JumpCondition {
        Unconditional = 0,
        Carry = 1,
        NonCarry = 2,
        Zero = 3,
        NonZero = 4,
        ParityEven = 5,
        ParityOdd = 6,
        SignNeg = 7,
        SignPos = 8,
        RegBNotZero = 9
    }

    public static class JumpConditionExtensions {
        public static bool ShouldJump(this JumpCondition jumpCondition, Z80Cpu cpu) {
            var flags = cpu.Flags;

            switch(jumpCondition) {
                case JumpCondition.Unconditional:
                    return true;
                case JumpCondition.Carry:
                    return flags.HasFlag(Z80Flags.Carry_C);
                case JumpCondition.NonCarry:
                    return !flags.HasFlag(Z80Flags.Carry_C);
                case JumpCondition.Zero:
                    return flags.HasFlag(Z80Flags.Zero_Z);
                case JumpCondition.NonZero:
                    return !flags.HasFlag(Z80Flags.Zero_Z);
                case JumpCondition.ParityEven:
                    return flags.HasFlag(Z80Flags.ParityOverflow_PV);
                case JumpCondition.ParityOdd:
                    return !flags.HasFlag(Z80Flags.ParityOverflow_PV);
                case JumpCondition.SignNeg:
                    return flags.HasFlag(Z80Flags.Sign_S);
                case JumpCondition.SignPos:
                    return !flags.HasFlag(Z80Flags.Sign_S);
                case JumpCondition.RegBNotZero:
                    return --cpu.B != 0;
            }
            throw new InvalidOperationException("Invalid condition specified for jump");
        }
    }

    public class Jump : IInstruction
    {
        private Z80Cpu _cpu;
        private IAddressMode<ushort> _addressMode;
        private JumpCondition _condition;
        private IReadAddressedOperand<ushort> _reader;
        private readonly InternalCycle _internalCycle;
        private readonly int _additionalM1TCycles;
        private int _remainingM1Cycles;
        private bool _requiresConditionalInternalCycle;

        public string Mnemonic { get; private set; }

        public bool IsComplete { get; private set; }

        public Jump(Z80Cpu cpu, IAddressMode<ushort> addressMode, JumpCondition condition, bool requiresConditionalInternalCycle = false, int additionalM1TCycles = 0, string mnemomic = "JP") {
            _cpu = cpu;
            _addressMode = addressMode;
            _condition = condition;
            _requiresConditionalInternalCycle = requiresConditionalInternalCycle;
            _internalCycle = new InternalCycle(5);
            _additionalM1TCycles = additionalM1TCycles;
            _remainingM1Cycles = additionalM1TCycles;
            Mnemonic = mnemomic;
        }

        public void Clock()
        {
            if (_remainingM1Cycles-- <= 0)
            {
                if (!_addressMode.IsComplete) {
                    _addressMode.Clock();
                    if (_addressMode.IsComplete) {
                        _reader = _addressMode.Reader;
                        JumpIfRequired();
                    }
                    return;
                }
                if (!_reader.IsComplete) {
                    _reader.Clock();
                    JumpIfRequired();
                    return;
                }

                if (!_internalCycle.IsComplete) {
                    _internalCycle.Clock();
                    if (_internalCycle.IsComplete) {
                        _cpu.PC = _reader.AddressedValue;
                        IsComplete = true;
                    }
                }
            }
        }

        public void Reset()
        {
            _addressMode.Reset();
            _reader = null;
            _internalCycle.Reset();
            IsComplete = false;
            _remainingM1Cycles = _additionalM1TCycles;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete) {
                _reader = _addressMode.Reader;
                JumpIfRequired();
            }
        }

        private void JumpIfRequired() {
            if (_reader.IsComplete && !_requiresConditionalInternalCycle && _condition.ShouldJump(_cpu)) {
                _cpu.PC = _reader.AddressedValue;
                IsComplete = true;
            } else if (_reader.IsComplete && _requiresConditionalInternalCycle && !_condition.ShouldJump(_cpu)) {
                IsComplete = true;
            } else if (_reader.IsComplete && !_requiresConditionalInternalCycle) {
                IsComplete = true;
            }
        }
    }
}