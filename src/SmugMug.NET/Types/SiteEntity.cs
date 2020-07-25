﻿// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SmugMug.v2.Authentication;
using System.Threading.Tasks;

namespace SmugMug.v2.Types
{
    public class SiteEntity : SmugMugEntity
    {
        public SiteEntity(OAuthToken token) : base(token)
        {            
        }

        public async Task<UserEntity> GetAuthenticatedUserAsync()
        {
            // !authuser 
            string requestUri = string.Format("{0}!authuser", SmugMug.v2.Constants.Addresses.SmugMugApi);

            return await RetrieveEntityAsync<UserEntity>(requestUri);
        }

        public async Task<CatalogVendorEntity[]> GetVendorsAsync()
        {
            // /catalog!vendors 
            string requestUri = string.Format("{0}/catalog!vendors", SmugMug.v2.Constants.Addresses.SmugMugApi);

            return await RetrieveEntityArrayAsync<CatalogVendorEntity>(requestUri);
        }

        public async Task<UserEntity[]> SearchForUserAsync(string query)
        {
            // api/v2/user!search?q=
            string requestUri = string.Format("{0}/user!search?q={1}", SmugMug.v2.Constants.Addresses.SmugMugApi, query);

            return await RetrieveEntityArrayAsync<UserEntity>(requestUri);
        }
    }
}
