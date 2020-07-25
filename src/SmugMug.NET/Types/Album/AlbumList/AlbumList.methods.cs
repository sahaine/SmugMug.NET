// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using SmugMug.v2.Authentication;

namespace SmugMug.v2.Types
{
    public partial class AlbumListEntity : SmugMugEntity
    {
        public AlbumListEntity()
        {
            //Empty constructor to enable deserialization
        }

        public AlbumListEntity(OAuthToken oauthToken) : base(oauthToken)
        {
        }
    }
}
