//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.Instructions
{
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
}