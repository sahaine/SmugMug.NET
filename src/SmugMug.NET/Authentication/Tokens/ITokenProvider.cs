﻿// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace SmugMug.v2.Authentication.Tokens
{
   public interface ITokenProvider
   {
      bool TryGetCredentials(out OAuthToken token);
      bool SaveCredentials(OAuthToken token);
   }
}
