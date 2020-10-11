using System;
using NUnit.Framework;

namespace Z80.Tests
{
    public abstract class CpuRunTestBase {
        protected Z80Cpu _cpu = new Z80Cpu();
        protected byte[] _ram;

        [SetUp]
        public void Setup()
        {
            _cpu.Reset();
            _ram = new byte[ushort.MaxValue + 1]; // +1 to account for zeroeth element
        }

        protected void RunUntil(int pc)
        {
            while (_cpu.PC != pc || !_cpu.NewInstruction)
            {
                if (_cpu.MREQ && _cpu.RD)
                {
                    var data = _ram[_cpu.Address];
                    _cpu.Data = data;
                }
                if (_cpu.MREQ && _cpu.WR)
                {
                    _ram[_cpu.Address] = _cpu.Data;
                }
                _cpu.Clock();
                if (_cpu.TotalTCycles > 1000) {
                    throw new InvalidOperationException("Unexpected long runtime during unit test");
                }
            }
        }
    }
}