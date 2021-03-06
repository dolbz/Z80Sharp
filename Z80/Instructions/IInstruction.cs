//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.Instructions
{
    public interface IInstruction : IClockable
    {
        string Mnemonic { get; }

        // Called on the final clock cycle of the instruction fetch (M1) cycle.
        // If the instruction can be completed without additional cycles it
        // performs the behaviour and IsComplete returns true so that the next
        // fetch cycle can be started immediately. Otherwise the IInstruction object
        // receives future Clock() invocations until IsComplete returns true 
        void StartExecution();
    }
}