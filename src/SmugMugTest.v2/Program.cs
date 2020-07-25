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
   using SmugMug.NET.Utility;
   using SmugMug.v2;
   using SmugMug.v2.Authentication;
   using SmugMug.v2.Authentication.Tokens;
   using SmugMug.v2.Types;
   using SmugMugShared;
   using SmugMugShared.Extensions;
   using SmugMugTest.v2;

   class Program
   {
      private static OAuthToken _oauthToken;
      private static ImageUploader _uploader;

      static void Main(string[] args)
      {
         _oauthToken = ConsoleAuthentication.GetOAuthTokenFromProvider(new FileTokenProvider());

         _uploader = new ImageUploader(_oauthToken);

         Upload2020Archives();

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

      private static void Upload2020Archives()
      {
         var Archives2020 = Directory.GetDirectories(@"\\vmwarehost\KHPArchive\KHP_ARCHIVE", "2020*", SearchOption.TopDirectoryOnly);

         foreach (var archiveFolder in Archives2020)
         {
            var folderName = new DirectoryInfo(archiveFolder).Name;
            var parts = folderName.Split('_');
            var customerId = parts.Last();

            if (string.Equals(customerId, "Test", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(customerId, "Haine", StringComparison.InvariantCultureIgnoreCase))
            {
               continue;
            }

            var customerPassword = new IniFile($@"\\khpserver\CustomerData\{customerId}\{customerId}.ini").IniReadValue("Contact", "Password");

            var shootId = parts.First();
            var incudeOriginals = true;
            if (customerId == "Haine" && shootId.Substring(0, 2) == "20")
            {
               shootId = shootId.Substring(0, 4);
               incudeOriginals = false;
            }

            var video = Directory.GetFiles($@"{ archiveFolder}\ForCustomer", "*.mp4").Where(f => f.ToLower().Contains("facebook") == false).ToList();
            var original = incudeOriginals ? Directory.GetFiles($@"{archiveFolder}\ForCustomer\Original", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false).ToList() : new List<string>();
            var colour = Directory.GetFiles($@"{archiveFolder}\ForCustomer\Edits\colour", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false).ToList();
            var sepia = Directory.GetFiles($@"{archiveFolder}\ForCustomer\Edits\sepia", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false).ToList();
            var bandW = Directory.GetFiles($@"{archiveFolder}\ForCustomer\Edits\BandW", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false).ToList();

            ProcessImages(customerId, customerPassword, shootId, original, video, colour, sepia, bandW);
         }
      }

      private static void UploadHaineArchives()
      {
         Dictionary<string, (List<string> Colour, List<string> Sepia, List<string> BandW)> images = new Dictionary<string, (List<string> Colour, List<string> Sepia, List<string> BandW)>();

         var haineArchives = Directory.GetDirectories(@"\\vmwarehost\KHPArchive\KHP_ARCHIVE", "*_Haine", SearchOption.TopDirectoryOnly);

         foreach (var archiveFolder in haineArchives)
         {
            var year = new DirectoryInfo(archiveFolder).Name.Substring(0, 4);

            if (!images.ContainsKey(year)) images.Add(year, (new List<string>(), new List<string>(), new List<string>()));

            var editsFolder = $@"{archiveFolder}\ForCustomer\Edits";
            if (Directory.Exists(editsFolder))
            {
               images[year].Colour.AddRange(Directory.GetFiles($@"{editsFolder}\colour", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false));
               images[year].Sepia.AddRange(Directory.GetFiles($@"{editsFolder}\sepia", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false));
               images[year].BandW.AddRange(Directory.GetFiles($@"{editsFolder}\BandW", "*.jpg").Where(f => f.ToLower().Contains("facebook") == false));
            }
         }

         foreach (var year in images)
         {
            if (year.Value.Colour.Count + year.Value.Sepia.Count + year.Value.BandW.Count > 0)
            {
               ProcessImages("Haine", "Ha1ne99", year.Key, null, null, year.Value.Colour, year.Value.Sepia, year.Value.BandW);
            }
         }
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

         var customersFolder = _uploader.GetRoot().GetChildrenAsync(type: TypeEnum.Folder).Result.Single(f => f.Name == "Customers");
         var customerFolder = _uploader.GetSubNode(customersFolder, customerName, TypeEnum.Folder, customerPassword);
         var shootFolder = _uploader.GetSubNode(customerFolder, shootName, TypeEnum.Folder);
         var editsFolder = _uploader.GetSubNode(shootFolder, "Edits", TypeEnum.Folder);

         _uploader.Upload(shootFolder, "Original", originals);
         _uploader.Upload(shootFolder, "Videos", videos);
         _uploader.Upload(editsFolder, "Colour", colours);
         _uploader.Upload(editsFolder, "Sepia", sepias);
         _uploader.Upload(editsFolder, "BandW", bandWs);

      }
   }
}
