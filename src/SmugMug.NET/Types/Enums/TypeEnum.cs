// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System.Runtime.Serialization;

namespace SmugMug.v2.Types
{
    public enum TypeEnum
    {
        NotSet,
        Album,
        Page,
        Folder,
        All,
        [EnumMember(Value = "System Album")]
        SystemAlbum,
        [EnumMember(Value = "System Page")]
        SystemPage
    }
}
