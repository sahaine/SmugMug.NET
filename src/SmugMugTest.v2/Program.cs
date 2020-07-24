using SmugMug.v2.Authentication;
using SmugMug.v2.Authentication.Tokens;
using SmugMug.v2.Types;
using System;
using System.Diagnostics;
using System.Linq;

namespace SmugMugTest
{
    class Program
    {
        private static OAuthToken s_oauthToken;

        static void Main(string[] args)
        {
            s_oauthToken = ConsoleAuthentication.GetOAuthTokenFromProvider(new FileTokenProvider());
            Debug.Assert(!s_oauthToken.Equals(OAuthToken.Invalid));

            SiteEntity site = new SiteEntity(s_oauthToken);
            var user = site.GetAuthenticatedUserAsync().Result;

           
            System.Console.WriteLine($"{user.Name} {user.TotalAccountSize}");

            var newAbum = new AlbumEntity(s_oauthToken)
            {
                Description = "StevesTest",
                Name = "StevesTest",
                UrlName = "StevesTest",
                Keywords = "SteveTest",

                SecurityType = SecurityTypeEnum.Password,
                Privacy = PrivacyEnum.Unlisted,
                PasswordHint = "Its on your Invoice",
                Password = "StevesTest",

                AllowDownloads = true,
                LargestSize = LargestSizeEnum.Original, 
                EXIF = true,
                
            };
    
            newAbum.CreateAsync(user.NickName, "Customers" ).Wait();

            var albums = user.GetAllAlbumsAsync().Result;

            Array.ForEach(albums, a => Console.WriteLine(a.Name));



            var image = albums.FirstOrDefault()?.GetImagesAsync().Result.FirstOrDefault();

            Console.WriteLine(image.Title);

            image.Caption = "test";

            image.SaveAsync().Wait();
        }
    }
}
