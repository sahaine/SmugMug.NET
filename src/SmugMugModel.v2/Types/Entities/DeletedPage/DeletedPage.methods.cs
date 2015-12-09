// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace SmugMug.v2.Types
{
    public partial class DeletedPageEntity : SmugMugEntity
    {

        public UserEntity user___ ()
        {
            // /user/(*) 
            return default(UserEntity); 
        }

        public RecoverDeletedPageEntity deletedpage____recover ()
        {
            // /deleted/page/(*)!recover 
            return default(RecoverDeletedPageEntity); 
        }

    }
}