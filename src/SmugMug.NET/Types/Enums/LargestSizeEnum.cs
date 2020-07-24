// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace SmugMug.v2.Types
{
    public enum LargestSizeEnum
    {
        Medium = 1,
        Large = 2,
        XLarge = 3,
        X2Large = 4,
        X3Large = 5,
        X4Large = 6,
        X5Large = 7,
        [EnumMember(Value = "4K")]
        _4K = 8,
        [EnumMember(Value = "5K")]
        _5K = 9,
        Original = 10
    }
}
