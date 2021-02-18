using System;

namespace Z80
{
    public enum Register
    {
        A,
        Flags,
        B,
        C,
        D,
        E,
        H,
        L,
        I,
        R,
        
        // Used by undocumented instructions that act on the high and low bytes of IX and IY
        IXh,
        IXl,
        IYh,
        IYl
    }

    public enum WideRegister
    {
        None = 0,
        AF,
        AF_,
        BC,
        BC_,
        DE,
        DE_,
        HL,
        HL_,
        SP,
        IX,
        IY,
        PC
    }

    public static class RegisterExtensions
    {
        public static void SetValueOnProcessor(this Register register, Z80Cpu cpu, byte value)
        {
            switch (register)
            {
                case Register.A:
                    cpu.A = value;
                    break;
                case Register.B:
                    cpu.B = value;
                    break;
                case Register.C:
                    cpu.C = value;
                    break;
                case Register.D:
                    cpu.D = value;
                    break;
                case Register.E:
                    cpu.E = value;
                    break;
                case Register.H:
                    cpu.H = value;
                    break;
                case Register.I:
                    cpu.I = value;
                    break;
                case Register.L:
                    cpu.L = value;
                    break;
                case Register.R:
                    cpu.R = value;
                    break;
                case Register.IXh:
                    cpu.IX = (ushort)((cpu.IX & 0xff) | (value << 8));
                    break;
                case Register.IXl:
                    cpu.IX = (ushort)((cpu.IX & 0xff00) | value);
                    break;
                case Register.IYh:
                    cpu.IY = (ushort)((cpu.IY & 0xff) | (value << 8));
                    break;
                case Register.IYl:
                    cpu.IY = (ushort)((cpu.IY & 0xff00) | value);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid register value: {register}");
            }
        }

        public static byte GetValue(this Register register, Z80Cpu cpu)
        {
            switch (register)
            {
                case Register.A:
                    return cpu.A;
                case Register.B:
                    return cpu.B;
                case Register.C:
                    return cpu.C;
                case Register.D:
                    return cpu.D;
                case Register.E:
                    return cpu.E;
                case Register.H:
                    return cpu.H;
                case Register.I:
                    return cpu.I;
                case Register.L:
                    return cpu.L;
                case Register.R:
                    return cpu.R;
                case Register.IXh:
                    return (byte)(cpu.IX >> 8);
                case Register.IXl:
                    return (byte)(cpu.IX & 0xff);
                case Register.IYh:
                    return (byte)(cpu.IY >> 8);
                case Register.IYl:
                    return (byte)(cpu.IY & 0xff);
                default:
                    throw new InvalidOperationException($"Invalid register value: {register}");
            }
        }

        public static ushort GetValue(this WideRegister register, Z80Cpu cpu)
        {
            switch (register)
            {
                case WideRegister.AF:
                    return (ushort)(cpu.A << 8 | (byte)cpu.Flags);
                case WideRegister.BC:
                    return (ushort)(cpu.B << 8 | cpu.C);
                case WideRegister.DE:
                    return (ushort)(cpu.D << 8 | cpu.E);
                case WideRegister.HL:
                    return (ushort)(cpu.H << 8 | cpu.L);
                case WideRegister.AF_:
                    return cpu.AF_;
                case WideRegister.BC_:
                    return cpu.BC_;
                case WideRegister.DE_:
                    return cpu.DE_;
                case WideRegister.HL_:
                    return cpu.HL_;
                case WideRegister.SP:
                    return cpu.SP;
                case WideRegister.IX:
                    return cpu.IX;
                case WideRegister.IY:
                    return cpu.IY;
                case WideRegister.PC:
                    return cpu.PC;
                default:
                    throw new InvalidOperationException($"Invalid register value: {register}");
            }
        }

        public static void SetValueOnProcessor(this WideRegister register, Z80Cpu cpu, ushort value)
        {
            switch (register)
            {
                case WideRegister.AF:
                    cpu.A = (byte)((value & 0xff00) >> 8);
                    cpu.Flags = (Z80Flags)(value & 0xff);
                    break;
                case WideRegister.BC:
                    cpu.B = (byte)((value & 0xff00) >> 8);
                    cpu.C = (byte)(value & 0xff);
                    break;
                case WideRegister.DE:
                    cpu.D = (byte)((value & 0xff00) >> 8);
                    cpu.E = (byte)(value & 0xff);
                    break;
                case WideRegister.HL:
                    cpu.H = (byte)((value & 0xff00) >> 8);
                    cpu.L = (byte)(value & 0xff);
                    break;
                case WideRegister.SP:
                    cpu.SP = value;
                    break;
                case WideRegister.IX:
                    cpu.IX = value;
                    break;
                case WideRegister.IY:
                    cpu.IY = value;
                    break;
                case WideRegister.AF_:
                    cpu.AF_ = value;
                    break;
                case WideRegister.BC_:
                    cpu.BC_ = value;
                    break;
                case WideRegister.DE_:
                    cpu.DE_ = value;
                    break;
                case WideRegister.HL_:
                    cpu.HL_ = value;
                    break;
                case WideRegister.PC:
                    cpu.PC = value;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid register value: {register}");
            }
        }
    }
}