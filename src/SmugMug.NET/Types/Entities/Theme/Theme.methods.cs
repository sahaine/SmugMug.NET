// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SmugMug.v2.Authentication;

namespace SmugMug.v2.Types
{
    public partial class ThemeEntity : SmugMugEntity
    {
        public ThemeEntity()
        {
            //Empty constructor to enable deserialization
        }

        public ThemeEntity(OAuthToken oauthToken) : base(oauthToken)
        {
        }

        protected override IEnumerable<string> GetPatchPropertiesName()
        {
            return PatchParameters;
        }

        private static readonly List<string> PatchParameters = new List<string>(){ "Name","BaseColor","Stretchy","Theme" };
    }
}
