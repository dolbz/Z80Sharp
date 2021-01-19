using System.Collections.Generic;
using System;
using NUnit.Framework;

namespace Z80.Tests
{
    public abstract class CpuRunTestBase {
        protected Z80Cpu _cpu = new Z80Cpu();
        protected byte[] _ram;

        private Dictionary<ushort, byte> _dataDictionary = new Dictionary<ushort, byte>();

        [SetUp]
        public void Setup()
        {
            _cpu.Reset();
            _ram = new byte[ushort.MaxValue + 1]; // +1 to account for zeroeth element
            _dataDictionary.Clear();
        }

        protected void AddDataAtIoAddress(ushort address, byte data) {
            _dataDictionary.Add(address, data);
        }

        protected byte DataAtIoAddress(ushort address) {
            return _dataDictionary[address];
        }

        protected void RunUntil(int pc)
        {
            while (_cpu.PC != pc || !_cpu.NewInstruction)
            {
                if (_cpu.IORQ && _cpu.RD && !_cpu.INT) {
                    _cpu.Data = _dataDictionary[_cpu.Address];
                }
                if (_cpu.IORQ && _cpu.WR && !_cpu.INT) {
                    if (_dataDictionary.ContainsKey(_cpu.Address)) {
                        _dataDictionary[_cpu.Address] = _cpu.Data;
                    } else {
                        _dataDictionary.Add(_cpu.Address, _cpu.Data);
                    }
                }
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