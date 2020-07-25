// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SmugMug.v2.Authentication;

namespace SmugMug.v2.Types
{
    public partial class FolderEntity : SmugMugEntity
    {
        public FolderEntity()
        {
            //Empty constructor to enable deserialization
        }

        public FolderEntity(OAuthToken oauthToken) : base(oauthToken)
        {
        }

        protected override IEnumerable<string> GetPatchPropertiesName()
        {
            return PatchParameters;
        }

        protected override IEnumerable<string> GetPostPropertiesName()
        {
            return PostParameters;
        }

        private static readonly List<string> PatchParameters = new List<string>(){ "Name","UrlName","AutoRename","SecurityType","SortMethod","SortDirection","Description","Keywords","Password","PasswordHint","Privacy","SmugSearchable","WorldSearchable","HighlightImageUri" };

        private static readonly List<string> PostParameters = new List<string>(){ "Name", "UrlName", "AutoRename", "SecurityType", "SortMethod", "SortDirection", "Description", "Keywords", "Password", "PasswordHint", "Privacy", "SmugSearchable", "WorldSearchable", "HighlightImageUri" };


        private async Task<AlbumListEntity> folderuser____albumlist (string param1)
        {
            // /folder/user/(*)!albumlist 
            string requestUri = string.Format("{0}/folder/user/{1}!albumlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1);

            return await RetrieveEntityAsync<AlbumListEntity>(requestUri); 
        }

        private async Task<AlbumEntity[]> folderuser____albums (string param1)
        {
            // /folder/user/(*)!albums 
            string requestUri = string.Format("{0}/folder/user/{1}!albums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1);

            return await RetrieveEntityArrayAsync<AlbumEntity>(requestUri); 
        }

        private async Task<FolderListEntity> folderuser____folderlist (string param1)
        {
            // /folder/user/(*)!folderlist 
            string requestUri = string.Format("{0}/folder/user/{1}!folderlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1);

            return await RetrieveEntityAsync<FolderListEntity>(requestUri); 
        }

        private async Task<ImageEntity> folderuser____highlightimage (string param1)
        {
            // /folder/user/(*)!highlightimage 
            string requestUri = string.Format("{0}/folder/user/{1}!highlightimage", SmugMug.v2.Constants.Addresses.SmugMugApi, param1);

            return await RetrieveEntityAsync<ImageEntity>(requestUri); 
        }

        private async Task<AlbumListEntity> folderuser_______albumlist (string param1, string param2)
        {
            // /folder/user/(*)/(*)!albumlist 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!albumlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<AlbumListEntity>(requestUri); 
        }

        private async Task<AlbumEntity[]> folderuser_______albums (string param1, string param2)
        {
            // /folder/user/(*)/(*)!albums 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!albums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<AlbumEntity>(requestUri); 
        }

        private async Task<FolderListEntity> folderuser_______folderlist (string param1, string param2)
        {
            // /folder/user/(*)/(*)!folderlist 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!folderlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<FolderListEntity>(requestUri); 
        }

        private async Task<FolderEntity[]> folderuser_______folders (string param1, string param2)
        {
            // /folder/user/(*)/(*)!folders 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!folders", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<FolderEntity>(requestUri); 
        }

        private async Task<GrantEntity[]> folderuser_______grants (string param1, string param2)
        {
            // /folder/user/(*)/(*)!grants 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!grants", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<GrantEntity>(requestUri); 
        }

        private async Task<ImageEntity> folderuser_______highlightimage (string param1, string param2)
        {
            // /folder/user/(*)/(*)!highlightimage 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!highlightimage", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<ImageEntity>(requestUri); 
        }

        private async Task folderuser_______movealbums (string param1, string param2)
        {
            // /folder/user/(*)/(*)!movealbums 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!movealbums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser_______movefolders (string param1, string param2)
        {
            // /folder/user/(*)/(*)!movefolders 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!movefolders", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser_______movepages (string param1, string param2)
        {
            // /folder/user/(*)/(*)!movepages 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!movepages", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task<PageEntity[]> folderuser_______pages (string param1, string param2)
        {
            // /folder/user/(*)/(*)!pages 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!pages", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<PageEntity>(requestUri); 
        }

        private async Task<FolderEntity> folderuser_______parent (string param1, string param2)
        {
            // /folder/user/(*)/(*)!parent 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!parent", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<FolderEntity>(requestUri); 
        }

        private async Task<FolderEntity[]> folderuser_______parents (string param1, string param2)
        {
            // /folder/user/(*)/(*)!parents 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!parents", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<FolderEntity>(requestUri); 
        }

        private async Task<SizeEntity> folderuser_______size (string param1, string param2)
        {
            // /folder/user/(*)/(*)!size 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!size", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<SizeEntity>(requestUri); 
        }

        private async Task folderuser_______sortalbums (string param1, string param2)
        {
            // /folder/user/(*)/(*)!sortalbums 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!sortalbums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser_______sortfolders (string param1, string param2)
        {
            // /folder/user/(*)/(*)!sortfolders 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!sortfolders", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser_______sortpages (string param1, string param2)
        {
            // /folder/user/(*)/(*)!sortpages 
            string requestUri = string.Format("{0}/folder/user/{1}/{2}!sortpages", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser___albumName____albumfromalbumtemplate (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!albumfromalbumtemplate 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!albumfromalbumtemplate", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task<AlbumListEntity> folderuser___albumName____albumlist (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!albumlist 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!albumlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<AlbumListEntity>(requestUri); 
        }

        private async Task<AlbumEntity[]> folderuser___albumName____albums (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!albums 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!albums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<AlbumEntity>(requestUri); 
        }

        private async Task<FolderListEntity> folderuser___albumName____folderlist (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!folderlist 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!folderlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<FolderListEntity>(requestUri); 
        }

        private async Task<FolderEntity[]> folderuser___albumName____folders (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!folders 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!folders", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<FolderEntity>(requestUri); 
        }

        private async Task<GrantEntity[]> folderuser___albumName____grants (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!grants 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!grants", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<GrantEntity>(requestUri); 
        }

        private async Task<ImageEntity> folderuser___albumName____highlightimage (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!highlightimage 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!highlightimage", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<ImageEntity>(requestUri); 
        }

        private async Task folderuser___albumName____movealbums (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!movealbums 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!movealbums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser___albumName____movefolders (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!movefolders 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!movefolders", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser___albumName____movepages (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!movepages 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!movepages", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task<PageEntity[]> folderuser___albumName____pages (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!pages 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!pages", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<PageEntity>(requestUri); 
        }

        private async Task<FolderEntity> folderuser___albumName____parent (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!parent 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!parent", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<FolderEntity>(requestUri); 
        }

        private async Task<SizeEntity> folderuser___albumName____size (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!size 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!size", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<SizeEntity>(requestUri); 
        }

        private async Task folderuser___albumName____sortalbums (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!sortalbums 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!sortalbums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser___albumName____sortfolders (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!sortfolders 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!sortfolders", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task folderuser___albumName____sortpages (string param1, string param2)
        {
            // /folder/user/(*)/albumName/(*)!sortpages 
            string requestUri = string.Format("{0}/folder/user/{1}/albumName/{2}!sortpages", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            await GetRequestAsync(requestUri); 
        }

        private async Task<AlbumListEntity> folderuser___Family____albumlist (string param1, string param2)
        {
            // /folder/user/(*)/Family/(*)!albumlist 
            string requestUri = string.Format("{0}/folder/user/{1}/Family/{2}!albumlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<AlbumListEntity>(requestUri); 
        }

        private async Task<AlbumEntity[]> folderuser___Family____albums (string param1, string param2)
        {
            // /folder/user/(*)/Family/(*)!albums 
            string requestUri = string.Format("{0}/folder/user/{1}/Family/{2}!albums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<AlbumEntity>(requestUri); 
        }

        private async Task<FolderListEntity> folderuser___Family____folderlist (string param1, string param2)
        {
            // /folder/user/(*)/Family/(*)!folderlist 
            string requestUri = string.Format("{0}/folder/user/{1}/Family/{2}!folderlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<FolderListEntity>(requestUri); 
        }

        private async Task<FolderEntity[]> folderuser___Family____folders (string param1, string param2)
        {
            // /folder/user/(*)/Family/(*)!folders 
            string requestUri = string.Format("{0}/folder/user/{1}/Family/{2}!folders", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<FolderEntity>(requestUri); 
        }

        private async Task<ImageEntity> folderuser___Family____highlightimage (string param1, string param2)
        {
            // /folder/user/(*)/Family/(*)!highlightimage 
            string requestUri = string.Format("{0}/folder/user/{1}/Family/{2}!highlightimage", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<ImageEntity>(requestUri); 
        }

        private async Task<PageEntity[]> folderuser___Family____pages (string param1, string param2)
        {
            // /folder/user/(*)/Family/(*)!pages 
            string requestUri = string.Format("{0}/folder/user/{1}/Family/{2}!pages", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<PageEntity>(requestUri); 
        }

        private async Task<FolderEntity> folderuser___Family____parent (string param1, string param2)
        {
            // /folder/user/(*)/Family/(*)!parent 
            string requestUri = string.Format("{0}/folder/user/{1}/Family/{2}!parent", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<FolderEntity>(requestUri); 
        }

        private async Task<SizeEntity> folderuser___Family____size (string param1, string param2)
        {
            // /folder/user/(*)/Family/(*)!size 
            string requestUri = string.Format("{0}/folder/user/{1}/Family/{2}!size", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<SizeEntity>(requestUri); 
        }

        private async Task<AlbumListEntity> folderuser___SmugMug____albumlist (string param1, string param2)
        {
            // /folder/user/(*)/SmugMug/(*)!albumlist 
            string requestUri = string.Format("{0}/folder/user/{1}/SmugMug/{2}!albumlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<AlbumListEntity>(requestUri); 
        }

        private async Task<AlbumEntity[]> folderuser___SmugMug____albums (string param1, string param2)
        {
            // /folder/user/(*)/SmugMug/(*)!albums 
            string requestUri = string.Format("{0}/folder/user/{1}/SmugMug/{2}!albums", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<AlbumEntity>(requestUri); 
        }

        private async Task<FolderListEntity> folderuser___SmugMug____folderlist (string param1, string param2)
        {
            // /folder/user/(*)/SmugMug/(*)!folderlist 
            string requestUri = string.Format("{0}/folder/user/{1}/SmugMug/{2}!folderlist", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<FolderListEntity>(requestUri); 
        }

        private async Task<FolderEntity[]> folderuser___SmugMug____folders (string param1, string param2)
        {
            // /folder/user/(*)/SmugMug/(*)!folders 
            string requestUri = string.Format("{0}/folder/user/{1}/SmugMug/{2}!folders", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<FolderEntity>(requestUri); 
        }

        private async Task<ImageEntity> folderuser___SmugMug____highlightimage (string param1, string param2)
        {
            // /folder/user/(*)/SmugMug/(*)!highlightimage 
            string requestUri = string.Format("{0}/folder/user/{1}/SmugMug/{2}!highlightimage", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<ImageEntity>(requestUri); 
        }

        private async Task<PageEntity[]> folderuser___SmugMug____pages (string param1, string param2)
        {
            // /folder/user/(*)/SmugMug/(*)!pages 
            string requestUri = string.Format("{0}/folder/user/{1}/SmugMug/{2}!pages", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityArrayAsync<PageEntity>(requestUri); 
        }

        private async Task<FolderEntity> folderuser___SmugMug____parent (string param1, string param2)
        {
            // /folder/user/(*)/SmugMug/(*)!parent 
            string requestUri = string.Format("{0}/folder/user/{1}/SmugMug/{2}!parent", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<FolderEntity>(requestUri); 
        }

        private async Task<SizeEntity> folderuser___SmugMug____size (string param1, string param2)
        {
            // /folder/user/(*)/SmugMug/(*)!size 
            string requestUri = string.Format("{0}/folder/user/{1}/SmugMug/{2}!size", SmugMug.v2.Constants.Addresses.SmugMugApi, param1,param2);

            return await RetrieveEntityAsync<SizeEntity>(requestUri); 
        }
    }
}
