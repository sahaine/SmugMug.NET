// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SmugMug.v2.Authentication.Tokens;
using SmugMugShared;
using System;
using System.Diagnostics;

namespace SmugMug.v2.Authentication
{
   public class ConsoleAuthentication
   {
      /// <summary>
      /// Use the ITokenProvider to retrieved stored credentials. If they are not available, authorize with SmugMug using the console.
      /// </summary>
      public static OAuthToken GetOAuthTokenFromProvider(ITokenProvider provider, string apiKey = null, string secret = null)
      {
         OAuthToken oauthToken = default(OAuthToken);
         if (!provider.TryGetCredentials(out oauthToken))
         {
            // Do we have the secret/apikey?
            if (string.IsNullOrWhiteSpace(apiKey))
            {
               Console.WriteLine("Please enter your API Key and press [Enter]:");
               apiKey =  Console.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
               Console.WriteLine("Please enter your Application Secret and press [Enter]:");
               secret = Console.ReadLine();
            }

            oauthToken = SmugMugAuthorize.AuthorizeSmugMug(apiKey, secret, AuthenticationOptions.FullAccess);
            provider.SaveCredentials(oauthToken);
         }

#if DEBUG
            Debug.WriteLine(string.Format("Using APIKey={0}", oauthToken.ApiKey));
            Debug.WriteLine(string.Format("Using AppSecret={0}", oauthToken.Secret));
            Debug.WriteLine(string.Format("Using token={0}", oauthToken.Token));
            Debug.WriteLine(string.Format("Using tokenSecret={0}", oauthToken.TokenSecret));
#endif

         return oauthToken;
      }

      public static OAuthToken GetOAuthTokenFromFileProvider(string apiKey = null, string secret = null)
      {
         return GetOAuthTokenFromProvider(new FileTokenProvider(), apiKey, secret);
      }
   }
}
