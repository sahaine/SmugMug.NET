namespace SmugMugTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.ModelBinding;
    using Newtonsoft.Json;
    using SmugMug.v2;
    using SmugMug.v2.Authentication;
    using SmugMug.v2.Authentication.Tokens;
    using SmugMug.v2.Types;
    using SmugMugShared;
    using SmugMugShared.Extensions;

    class Program
    {
        private static OAuthToken _oauthToken;

        static void Main(string[] args)
        {
            _oauthToken = ConsoleAuthentication.GetOAuthTokenFromProvider(new FileTokenProvider());

            Debug.Assert(!_oauthToken.Equals(OAuthToken.Invalid));

            Dictionary<string, (List<string> Colour, List<string> Sepia, List<string> BandW)> images = new Dictionary<string, (List<string> Colour, List<string> Sepia, List<string> BandW)>();
                                
            var haineArchives = Directory.GetDirectories(@"\\vmwarehost\KHPArchive\KHP_ARCHIVE", "*_Haine", SearchOption.TopDirectoryOnly);

            foreach (var archiveFolder in haineArchives)
            {
                var year = new DirectoryInfo(archiveFolder).Name.Substring(0, 4);

                if (!images.ContainsKey(year)) images.Add(year, (new List<string>(), new List<string>(), new List<string>()));

                var editsFolder = $@"{archiveFolder}\ForCustomer\Edits";
                if (Directory.Exists(editsFolder))
                {
                    images[year].Colour.AddRange(Directory.GetFiles($@"{editsFolder}\colour", "*.jpg").Where(f=> f.ToLower().Contains("facebook") == false));
                    images[year].Sepia.AddRange(Directory.GetFiles($@"{editsFolder}\sepia", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false));
                    images[year].BandW.AddRange(Directory.GetFiles($@"{editsFolder}\BandW", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false));
                }
            }

            foreach(var year in images)
            {
                if (year.Value.Colour.Count + year.Value.Sepia.Count + year.Value.BandW.Count > 0)
                {
                    ProcessImages("Haine", "Ha1ne99", year.Key, null, null, year.Value.Colour , year.Value.Sepia, year.Value.BandW);
                }
            }

            //var fileList = Directory.GetFiles(@"E:\SampleImages", "*.jpg").ToList();
            //if (!_oauthToken.Equals(OAuthToken.Invalid))
            //{
            //    ProcessImages("Haine", "Ha1ne99", "2020-07-24", fileList, fileList, fileList, fileList, fileList);
            //}

#if DEBUG
            Console.WriteLine("Complete");
            Console.ReadKey();
#endif 
        }

        private static void ProcessImages(
            string customerName,
            string customerPassword,
            string shootName,
            List<string> originals,
            List<string> videos,
            List<string> colours,
            List<string> sepias,
            List<string> bandWs)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                ConsolePrinter.Write(ConsoleColor.Red, $"Customer Name not specfied!");
                return;
            }

            if (string.IsNullOrWhiteSpace(customerPassword))
            {
                ConsolePrinter.Write(ConsoleColor.Red, $"Customer Password not specfied!");
                return;
            }

            if (string.IsNullOrWhiteSpace(shootName))
            {
                ConsolePrinter.Write(ConsoleColor.Red, $"Shoot Name not specfied!");
                return;
            }

            var site = new SiteEntity(_oauthToken);
            var user = site.GetAuthenticatedUserAsync().Result;
            var albums = user.GetAllAlbumsAsync().Result;
            var root = user.GetRootNodeAsync().Result;
            var customersFolder = root.GetChildrenAsync(type: TypeEnum.Folder).Result.Single(f => f.Name == "Customers");

            var customerFolder = GetSubNode(customersFolder, customerName, TypeEnum.Folder, customerPassword);
            var shootFolder = GetSubNode(customerFolder, shootName, TypeEnum.Folder);

            Upload(shootFolder, "Originals", originals);
            Upload(shootFolder, "Videos", videos);

            var editsFolder = GetSubNode(shootFolder, "Edits", TypeEnum.Folder);
            Upload(editsFolder, "Colour", colours);
            Upload(editsFolder, "Sepia", sepias);
            Upload(editsFolder, "BandW", bandWs);


        }

        private static NodeEntity GetSubNode(NodeEntity parentFolder, string subFolderName, TypeEnum nodeType, string subFolderPassword = null)
        {
            var subFolder = parentFolder.GetChildrenAsync(type: nodeType).Result?.FirstOrDefault(f => f.Name == subFolderName);

            if (subFolder != null)
            {
                ConsolePrinter.Write(ConsoleColor.Green, $"Loaded {nodeType} : {subFolderName}");
                return subFolder;
            }

            ConsolePrinter.Write(ConsoleColor.DarkYellow, $"Creating {nodeType} : {subFolderName}");

            //we need to create it
            subFolder = new NodeEntity(_oauthToken)
            {
                Type = nodeType,
                Description = subFolderName,
                Name = subFolderName,
                UrlName = subFolderName,
                Keywords = new[] { subFolderName },
                Parent = parentFolder,
                Privacy = string.IsNullOrWhiteSpace(subFolderPassword) ? PrivacyEnum.Public : PrivacyEnum.Unlisted,
                SecurityType = string.IsNullOrWhiteSpace(subFolderPassword) ? SecurityTypeEnum.None : SecurityTypeEnum.Password,
                Password = string.IsNullOrWhiteSpace(subFolderPassword) ? null : subFolderPassword,
                PasswordHint = string.IsNullOrWhiteSpace(subFolderPassword) ? null : "It's on your sales reciept"
            };

            subFolder.CreateAsync(parentFolder).Wait();

            return subFolder;

        }

        private static void Upload(NodeEntity parentFolder, string albumName, List<string> files)
        {
            if (files?.Any() != true)
            {
                return;
            }

            ConsolePrinter.Write( ConsoleColor.White, $"Uplaoding {files.Count()} images to {albumName}.");

            var albumNode = GetSubNode(parentFolder, albumName, TypeEnum.Album);

            try
            {
                OAuth.OAuthMessageHandler oAuthhandler = new OAuth.OAuthMessageHandler(
                   _oauthToken.ApiKey,
                   _oauthToken.Secret,
                   _oauthToken.Token,
                   _oauthToken.TokenSecret);

                using (var client = new HttpClient(oAuthhandler))
                //using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var albumUri = albumNode.Uris.Single(uri => uri.Key == "Album").Value.Uri;
                    var album = AlbumEntity.RetrieveEntityAsync<AlbumEntity>(_oauthToken, $"{Constants.Addresses.SmugMugApi}{albumUri}").Result;
                    var images = album.GetImagesAsync().Result;

                    AddUploadHeaders(client, albumUri);

                    foreach (var file in files)
                    {
                        var name = Path.GetFileNameWithoutExtension(file);

                        if (images != null && images.Any(i => i.FileName == name))
                        {
                            ConsolePrinter.Write(ConsoleColor.Yellow , $"Skipping {name} it's already there!");
                            continue;
                        }

                        var fileItem = new FileInfo(file);
                        
                        int timeout = ((int)fileItem.Length / 1024) * 1000;

                        ConsolePrinter.Write(ConsoleColor.Cyan, $"Uploading {fileItem.Name}");

                        var md5CheckSum = GetCheckSum(file);

                        using (var fileContent = new StreamContent(fileItem.OpenRead(), 1024 * 32))
                        {
                            AddUploadHeaders(fileContent.Headers, fileItem, name, md5CheckSum);
                            var resp = client.PostAsync(Constants.Addresses.SmugMugUpload, fileContent);
                            if (resp.Wait(timeout))
                                ParseReponse(resp.Result);
                            else
                                throw new TimeoutException("Timed Out");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
            }
        }

        private static void ParseReponse(HttpResponseMessage result)
        {
            ConsolePrinter.Write(ConsoleColor.Green, result.StatusCode.ToString());

            var resp = result.Content.ReadAsStringAsync();
            resp.Wait();

            var response = JsonConvert.DeserializeObject<UploadReponse>(Uri.UnescapeDataString(resp.Result));

            if (response.stat != "ok")
            {
                Debug.WriteLine(resp.Result);
                throw new ApplicationException($"{response.message} : {response.code}");
            }
        }

        private static string GetCheckSum(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private static void AddUploadHeaders(HttpClient client, string albumUri)
        {
            client.DefaultRequestHeaders.Add("User-Agent", "KHainePhotography_PhotoStudioManager_v1.0");

            client.DefaultRequestHeaders.Add("X-Smug-AlbumUri", albumUri);
            client.DefaultRequestHeaders.Add("X-Smug-ResponseType", "JSON");
            client.DefaultRequestHeaders.Add("X-Smug-Version", "v2");

            client.DefaultRequestHeaders.Add("X-Smug-Latitude", "53.457920");
            client.DefaultRequestHeaders.Add("X-Smug-Longitude", "-1.464252");
            client.DefaultRequestHeaders.Add("X-Smug-Altitude", "86");

            client.DefaultRequestHeaders.Add("X-Smug-Pretty", "true");
            client.DefaultRequestHeaders.Add("X-Smug-Hidden", "false");
        }

        private static void AddUploadHeaders(HttpContentHeaders headers, FileInfo file, string name, string md5Checksum)
        {
            headers.Add("Content-MD5", md5Checksum);
            headers.Add("Content-Length", file.Length.ToString());
            headers.ContentType = MediaTypeHeaderValue.Parse(MimeMapping.GetMimeMapping(file.Name));

            headers.Add("X-Smug-FileName", name);
            headers.Add("X-Smug-Title", name);
            headers.Add("X-Smug-Caption", name);
        }

        public class UploadReponse
        {
            public string stat { get; set; }
            public string method { get; set; }

            public string code { get; set; }

            public string message { get; set; }
            public ImageUploadData Image { get; set; }

            public class ImageUploadData
            {
                public string ImageUri { get; set; }
                public string AlbumImageUri { get; set; }
                public string StatusImageReplaceUri { get; set; }
                public string URL { get; set; }
            }
        }
    }
}
