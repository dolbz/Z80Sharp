//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80
{
    public interface IClockable
    {
        void Clock();

        void Reset();

        bool IsComplete { get; }
    }
}