//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.Instructions.InterruptHandlers {
    // TODO
    public class Mode0Handler : IInstruction
    {
        public string Mnemonic => "N/A";

        public bool IsComplete => throw new System.NotImplementedException();

        public void Clock()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        public void StartExecution()
        {
            throw new System.NotImplementedException();
        }
    }
}