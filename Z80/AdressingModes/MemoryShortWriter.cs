//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.AddressingModes
{
    public class MemoryShortWriter : IWriteAddressedOperand<ushort> 
    {
        private readonly MemWriteCycle _byte1Writer;
        private readonly MemWriteCycle _byte2Writer;

        public bool _decreasingAddress;

        public MemoryShortWriter(Z80Cpu cpu, ushort address, bool decreasingAddress) {
            _decreasingAddress = decreasingAddress;

            _byte1Writer = new MemWriteCycle(cpu);
            _byte1Writer.Address = _decreasingAddress ? --address : address++;
            _byte2Writer = new MemWriteCycle(cpu);
            _byte2Writer.Address = _decreasingAddress ? --address : address;
        }

        public ushort AddressedValue  { 
            set 
            { 
                var msb = (byte)(value >> 8);
                var lsb = (byte)(value & 0xff);
                _byte1Writer.DataToWrite = _decreasingAddress ? msb : lsb;
                _byte2Writer.DataToWrite =  _decreasingAddress ? lsb : msb;
            }
        }

        public bool IsComplete => _byte2Writer.IsComplete;

        public void Clock()
        {
            if (!_byte1Writer.IsComplete) {
                _byte1Writer.Clock();
                return;
            }
            if (!_byte2Writer.IsComplete) {
                _byte2Writer.Clock();
                return;
            }
        }

        public void Reset()
        {
            _byte1Writer.Reset();
            _byte2Writer.Reset();
        }
    }
}