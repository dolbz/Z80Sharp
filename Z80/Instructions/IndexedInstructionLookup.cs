using System;
using Z80.AddressingModes;

namespace Z80.Instructions {
    public class IndexedInstructionLookup : IInstruction
    {
        private Z80Cpu _cpu;
        private Indexed _addressMode;
        private MemoryByteReader _instructionTypeReader;
        
        private IInstruction _underlyingInstruction;

        public IndexedInstructionLookup(Z80Cpu cpu, WideRegister register) {
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
                    instruction = new RotateLeft(_cpu, pointer); // RL
                    break;
                case 0x1e:
                    instruction = new RotateRight(_cpu, pointer); // RR
                    break;
                case 0x26:
                    instruction = new ShiftLeftArithmetic(_cpu, pointer); // SLA
                    break;
                case 0x2e:
                    instruction = new ShiftRightArithmetic(_cpu, pointer); // SRA
                    break;
                case 0x3e:
                    instruction = new ShiftRightLogical(_cpu, pointer); // SRL
                    break;
                case 0x46:
                case 0x4e:
                case 0x56:
                case 0x5e:
                case 0x66:
                case 0x6e:
                case 0x76:
                case 0x7e:
                    instruction = new BitTest(_cpu, pointer, (value % 0x46) / 8); // BIT
                    break;
                case 0x86:
                case 0x8e:
                case 0x96:
                case 0x9e:
                case 0xa6:
                case 0xae:
                case 0xb6:
                case 0xbe:
                    instruction = new SetOrResetBit(_cpu, pointer, (value % 0x86) / 8, false); // RST
                    break;
                case 0xc6:
                case 0xce:
                case 0xd6:
                case 0xde:
                case 0xe6:
                case 0xee:
                case 0xf6:
                case 0xfe:
                    instruction = new SetOrResetBit(_cpu, pointer, (value % 0xc6) / 8, true); // SET
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


}