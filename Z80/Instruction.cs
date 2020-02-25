using System.Diagnostics;
namespace Z80
{
    internal interface IInstruction : IClockable
    {
        string Mnemonic { get; }

        // Called on the final clock cycle of the instruction fetch (M1) cycle.
        // If the instruction can be completed without additional cycles it
        // performs the behaviour and IsComplete returns true so that the next
        // fetch cycle can be started immediately. Otherwise the IInstruction object
        // receives future Clock() invocations until IsComplete returns true 
        void StartExecution();
    }

    internal class NOP : IInstruction
    {

        public string Mnemonic => "NOP";

        public bool IsComplete => true;

        public void StartExecution()
        {
            // No operation
        }
        public void Clock()
        {
            // Do no operation
        }

        public void Reset()
        {
            // Nothing to do
        }
    }

    internal class LD_8Bit : LD_Generic<byte>
    {
        public LD_8Bit(Z80Cpu cpu, IWriteAddressedOperand<byte> destination, IReadAddressedOperand<byte> source, int additionalM1TCycles = 0)
            : base(cpu, destination, source, additionalM1TCycles)
        {
        }
    }

    internal class LD_16Bit : LD_Generic<ushort>
    {
        public LD_16Bit(Z80Cpu cpu, IWriteAddressedOperand<ushort> destination, IReadAddressedOperand<ushort> source, int additionalM1TCycles = 0)
            : base(cpu, destination, source, additionalM1TCycles)
        {
        }
    }
    internal abstract class LD_Generic<T> : IInstruction
    {
        public bool IsComplete => _remainingM1Cycles <= 0 && _destination.IsComplete && _source.IsComplete;
        public string Mnemonic { get; }

        protected Z80Cpu _cpu;
        protected IWriteAddressedOperand<T> _destination;
        private IReadAddressedOperand<T> _source;

        private readonly int _additionalM1TCycles;
        private int _remainingM1Cycles;

        public LD_Generic(Z80Cpu cpu, IWriteAddressedOperand<T> destination, IReadAddressedOperand<T> source, int additionalM1TCycles)
        {
            _cpu = cpu;
            Mnemonic = "LD";
            _additionalM1TCycles = additionalM1TCycles;
            _remainingM1Cycles = additionalM1TCycles;
            _destination = destination;
            _source = source;
        }

        public virtual void StartExecution()
        {
            if (_source.IsComplete)
            {
                _destination.AddressedValue = _source.AddressedValue;
            }
        }

        public void Reset()
        {
            _source.Reset();
            _destination.Reset();
            _remainingM1Cycles = _additionalM1TCycles;
        }

        public virtual void Clock()
        {
            if (_remainingM1Cycles-- <= 0)
            {
                // Destination is checked first as its operand comes first if there are 
                // additional bytes to the instruction.
                if (!_destination.WriteReady)
                {
                    _destination.Clock();
                    return;
                }
                if (!_source.IsComplete)
                {
                    _source.Clock();

                    if (_source.IsComplete)
                    {
                        _destination.AddressedValue = _source.AddressedValue;
                    }
                    return;
                }

                if (!_destination.IsComplete)
                {
                    _destination.Clock();
                }
            }
        }
    }

    internal class PUSH : LD_Generic<ushort>
    {
        public PUSH(Z80Cpu cpu, WideRegister register) : base(cpu, new RegIndirectWideWrite(cpu, WideRegister.SP), new RegAddrMode16Bit(cpu, register), additionalM1TCycles: 1)
        {

        }

        public override void Clock()
        {
            base.Clock();

            if (_destination.IsComplete)
            {
                _cpu.SP -= 2;
            }
        }
    }

    internal class POP : LD_Generic<ushort>
    {
        public POP(Z80Cpu cpu, WideRegister register) : base(cpu, new RegAddrMode16Bit(cpu, register), new RegIndirectWideRead(cpu, WideRegister.SP), additionalM1TCycles: 0)
        {

        }

        public override void Clock()
        {
            base.Clock();

            if (_destination.IsComplete)
            {
                _cpu.SP += 2;
            }
        }
    }
}