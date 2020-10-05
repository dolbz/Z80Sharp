using System;
using Z80.AddressingModes;

namespace Z80.Instructions
{
    public class RotateLeft : RotateShiftBase
    {
        public RotateLeft(Z80Cpu cpu, IAddressMode<byte> addressMode, bool circular = false, bool is8080Compatible = false) : base(cpu, addressMode, circular, is8080Compatible)
        {
        }

        protected override int CarryBitCheckValue => 0x80;
        protected override int CarryBitAddValue => 0x01;
        protected override bool IsLeftShift => true;
    }

    public class RotateRight : RotateShiftBase
    {
        public RotateRight(Z80Cpu cpu, IAddressMode<byte> addressMode, bool circular = false, bool is8080Compatible = false) : base(cpu, addressMode, circular, is8080Compatible)
        {
        }

        protected override int CarryBitCheckValue => 0x01;
        protected override int CarryBitAddValue => 0x80;
        protected override bool IsLeftShift => false;
    }

    public class ShiftLeftArithmetic : RotateShiftBase
    {
        public ShiftLeftArithmetic(Z80Cpu cpu, IAddressMode<byte> addressMode) : base(cpu, addressMode)
        {
        }

        protected override int CarryBitCheckValue => 0x80;
        protected override int CarryBitAddValue => 0;
        protected override bool IsLeftShift => true;
        protected override bool ReplicateMsbOnShift => false;
    }

    public class ShiftRightArithmetic : RotateShiftBase
    {
        public ShiftRightArithmetic(Z80Cpu cpu, IAddressMode<byte> addressMode) : base(cpu, addressMode)
        {
        }

        protected override int CarryBitCheckValue => 0x01;
        protected override int CarryBitAddValue => 0;
        protected override bool IsLeftShift => false;
        protected override bool ReplicateMsbOnShift => true;
    }

    public class ShiftRightLogical : RotateShiftBase
    {
        public ShiftRightLogical(Z80Cpu cpu, IAddressMode<byte> addressMode) : base(cpu, addressMode)
        {
        }

        protected override int CarryBitCheckValue => 0x01;
        protected override int CarryBitAddValue => 0;
        protected override bool IsLeftShift => false;
        protected override bool ReplicateMsbOnShift => false;
    }

    public abstract class RotateShiftBase : IInstruction
    {
        private Z80Cpu _cpu;
        private IAddressMode<byte> _addressMode;
        private bool _is8080Compatible;
        private bool _circular;

        private IReadAddressedOperand<byte> _reader;
        private IWriteAddressedOperand<byte> _writer;

        protected abstract int CarryBitCheckValue { get; }

        protected abstract int CarryBitAddValue { get; }

        protected abstract bool IsLeftShift { get; }

        protected virtual bool ReplicateMsbOnShift => false;

        public string Mnemonic => "RL" + (_circular ? "C" : string.Empty) + (_is8080Compatible ? "A" : string.Empty); // TODO shifts and RR

        public bool IsComplete => _writer?.IsComplete ?? false;

        public RotateShiftBase(Z80Cpu cpu, IAddressMode<byte> addressMode, bool circular = false, bool is8080Compatible = false) 
        {
            _cpu = cpu;
            _addressMode = addressMode;
            _circular = circular;
            _is8080Compatible = is8080Compatible;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete) {
                _addressMode.Clock();
                if (_addressMode.IsComplete) {
                    _reader = _addressMode.Reader;
                    _writer = _addressMode.Writer;
                }
                return;
            }
            if (!_reader.IsComplete) {
                _reader.Clock();
                if (_reader.IsComplete) {
                    DoRotate();
                }
                return;
            }
            if (!_writer.IsComplete) {
                _writer.Clock();
            }
        }

        public void Reset()
        {
            _addressMode.Reset();
            _reader = null;
            _writer = null;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete) {
                _reader = _addressMode.Reader;
                _writer = _addressMode.Writer;
                if (_reader.IsComplete) {
                    DoRotate();
                }
            }
        }

        private void DoRotate() 
        {
            var value = _reader.AddressedValue;

            var carry = (value & CarryBitCheckValue) == CarryBitCheckValue;

            var rotated = 0;
            if (IsLeftShift) 
            {
                rotated = _writer.AddressedValue = (byte)(value << 1);
            } 
            else 
            {
                var msb = value & 0x80;
                if (!ReplicateMsbOnShift) 
                {
                    msb = 0;
                }

                rotated = _writer.AddressedValue = (byte)((value >> 1) + msb);
            }

            if ((_circular && carry) || (!_circular && _cpu.Flags.HasFlag(Z80Flags.Carry_C))) {
                rotated += CarryBitAddValue;
            }

            var finalValue = (byte)rotated;
            _writer.AddressedValue = finalValue;

            Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
            Z80Flags.Carry_C.SetOrReset(_cpu, carry);

            if (!_is8080Compatible) 
            {
                Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, finalValue.IsEvenParity());
                Z80Flags.Sign_S.SetOrReset(_cpu, finalValue >= 0x80);
                Z80Flags.Zero_Z.SetOrReset(_cpu, finalValue == 0);
            }
        }
    }

    public class RotateIndexed : IInstruction
    {
        private Z80Cpu _cpu;
        private Indexed _addressMode;
        private MemoryByteReader _instructionTypeReader;
        
        private IInstruction _underlyingInstruction;

        public RotateIndexed(Z80Cpu cpu, WideRegister register) {
            _cpu = cpu;
            _addressMode = new Indexed(cpu, register);
            _instructionTypeReader = new MemoryByteReader(cpu);
        }

        public string Mnemonic => _underlyingInstruction.Mnemonic;

        public bool IsComplete => _underlyingInstruction?.IsComplete ?? false;

        public void Clock()
        {
            if (!_addressMode.IsComplete) 
            {
                _addressMode.Clock();
                return;
            }
            if (!_instructionTypeReader.IsComplete) 
            {
                _instructionTypeReader.Clock();
                if (_instructionTypeReader.IsComplete)
                {
                    _underlyingInstruction = GetInstructionFromByte(_instructionTypeReader.AddressedValue);
                }
                return;
            }
            if (!_underlyingInstruction.IsComplete) 
            {
                _underlyingInstruction.Clock();
            }
        }

        private IInstruction GetInstructionFromByte(byte value) 
        {
            var pointer = _addressMode;
            IInstruction instruction = null;

            switch (value) {
                case 0x06:
                    instruction = new RotateLeft(_cpu, pointer, circular: true); // RLC
                    break;
                case 0x0e:
                    instruction = new RotateRight(_cpu, pointer, circular: true); // RRC
                    break;
                case 0x16:
                    instruction =  new RotateLeft(_cpu, pointer); // RL
                    break;
                case 0x1e:
                    instruction = new RotateRight(_cpu, pointer); // RR
                    break;
                case 0x26:
                    instruction =  new ShiftLeftArithmetic(_cpu, pointer); // SLA
                    break;
                case 0x2e:
                    instruction =  new ShiftRightArithmetic(_cpu, pointer); // SRA
                    break;
                case 0x3e:
                    instruction =  new ShiftRightLogical(_cpu, pointer); // SRL
                    break;
                default:
                    throw new InvalidOperationException("Unexpected opcode");
            }

            instruction.StartExecution();
            return instruction;
        }

        public void Reset()
        {
            _addressMode.Reset();
            _instructionTypeReader.Reset();
            _underlyingInstruction = null;
        }

        public void StartExecution()
        {
        }
    }

    public class RotateDigit : IInstruction
    {
        private Z80Cpu _cpu;
        private RegIndirect _addressMode;
        private InternalCycle _internalCycle;
        private IWriteAddressedOperand<byte> _writer;
        private IReadAddressedOperand<byte> _reader;

        private bool _isLeftShift;

        public string Mnemonic => "R" + (_isLeftShift ? "L" : "R") + "D";

        public bool IsComplete => _writer != null && _writer.IsComplete;

        public RotateDigit(Z80Cpu cpu, bool isLeftShift) {
            _cpu = cpu;
            _addressMode = new RegIndirect(_cpu, WideRegister.HL);
            _internalCycle = new InternalCycle(4);
            _isLeftShift = isLeftShift;
        }

        public void Clock()
        {
            if (!_reader.IsComplete) {
                _reader.Clock();
                return;
            }
            if (!_internalCycle.IsComplete) {
                _internalCycle.Clock();
                if (_internalCycle.IsComplete) {
                    var originalAccBits = _cpu.A & 0x0f;
                    var newAccBits = 0;
                    var shiftedValue = 0;
                    if (_isLeftShift) {
                        newAccBits = (_reader.AddressedValue & 0xf0) >> 4;
                        shiftedValue = _reader.AddressedValue << 4;
                    } else {
                        newAccBits = _reader.AddressedValue & 0x0f;
                        originalAccBits = originalAccBits << 4;
                        shiftedValue = _reader.AddressedValue >> 4;
                    }
                    
                    var newPointedValue = (shiftedValue & 0xff) | originalAccBits;
                    _writer = _addressMode.Writer;
                    _writer.AddressedValue = (byte)newPointedValue;
                    _cpu.A = (byte)((_cpu.A & 0xf0) | newAccBits);

                    // Set the flags
                    Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);
                    Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);

                    Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, _cpu.A.IsEvenParity());
                    Z80Flags.Sign_S.SetOrReset(_cpu, _cpu.A >= 0x80);
                    Z80Flags.Zero_Z.SetOrReset(_cpu, _cpu.A == 0);
                }
                return;
            }
            if (!_writer.IsComplete) {
                _writer.Clock();
            }
        }

        public void Reset()
        {
            _reader = _addressMode.Reader;;
            _internalCycle.Reset();
            _writer = null;
        }

        public void StartExecution()
        {
        }
    }
}