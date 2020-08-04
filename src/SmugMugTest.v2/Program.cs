﻿namespace SmugMugTest
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Net;
   using System.Net.Mail;
   using System.Net.Security;
   using SmugMug.NET.Utility;
   using SmugMug.v2;
   using SmugMug.v2.Authentication;
   using SmugMug.v2.Authentication.Tokens;
   using SmugMugShared.Extensions;
   using SmugMugTest.v2;

   class Program
   {
      private static OAuthToken _oauthToken;
      private static ImageUploader _uploader;

      static void Main(string[] args)
      {
         Console.CursorVisible = false;
         try
         {

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, error) =>
            {
               Debug.WriteLine(cert.Subject);
               return cert.Subject == "CN=smugmug.com" || cert.Subject == "CN=khainephotography.co.uk";  
            };

            _oauthToken = ConsoleAuthentication.GetOAuthTokenFromProvider(new FileTokenProvider());

            _uploader = new ImageUploader(_oauthToken);

            //ProcessImages("Wood", "2020-07-26");
            Upload2020Archives();

         }
         catch (Exception e)
         {
            ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
         }
         finally
         {
#if DEBUG
            ConsolePrinter.Write(ConsoleColor.Cyan, "Complete");
            Console.ReadKey();
#endif
         }

      }

      private static void ProcessImages(string customerId, string shootId)
      {
         const string customerDataPath = @"\\khpserver\CustomerData";
         const string websitePath = @"\\khpserver\WebSites\KHainePhotography.co.uk";

         var shootData = new CustomerShootData(customerId, shootId, customerDataPath, websitePath);

         _uploader.ProcessImages(customerId, shootData.CustomerData.UsersPassword, shootId, shootData.Originals(), shootData.Videos(), shootData.ColourEdits(), shootData.SepiaEdits(), shootData.BandWEdits());
      }

      private static void Upload2020Archives()
      {
         var Archives2020 = Directory.GetDirectories(@"\\vmwarehost\KHPArchive\KHP_ARCHIVE", "2020*", SearchOption.TopDirectoryOnly);

         foreach (var archiveFolder in Archives2020.Reverse())
         {
            try
            {
               var folderName = new DirectoryInfo(archiveFolder).Name;
               var parts = folderName.Split('_');
               var customerId = parts.Last();

               if (string.Equals(customerId, "Test", StringComparison.InvariantCultureIgnoreCase) ||
                   string.Equals(customerId, "Haine", StringComparison.InvariantCultureIgnoreCase))
               {
                  continue;
               }

               var customerIniFile = new IniFile($@"\\khpserver\CustomerData\{customerId}\{customerId}.ini");
               var customerPassword = customerIniFile.IniReadValue("Contact", "Password");
               var customerEmailAddress = customerIniFile.IniReadValue("Contact", "EMailAddress");
               var customerSmugMugUrl = customerIniFile.IniReadValue("Account", "SmugMugUrl");

               //Debug.WriteLine($"{customerId} {customerEmailAddress} {customerPassword}");
               //Email(
               //   File.ReadAllText(@"\\khpserver\Documents\EMailTempates\NewSmugMug.html").Replace("SMUGMUGURLADDRESS", customerSmugMugUrl), 
               //   customerEmailAddress,
               //   customerId);

               continue;

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

               var newCustomerSmugMugUrl = _uploader.ProcessImages(customerId, customerPassword, shootId, original, video, colour, sepia, bandW);

               if (newCustomerSmugMugUrl != null)
               {
                  if (newCustomerSmugMugUrl != customerSmugMugUrl)
                  {
                     try
                     {
                        customerSmugMugUrl = newCustomerSmugMugUrl;
                        customerIniFile.IniWriteValue("Account", "SmugMugUrl", customerSmugMugUrl);
                     }
                     catch (Exception e)
                     {
                        ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
                     }
                  }
               }
               else
               {
                  ConsolePrinter.Write(ConsoleColor.Yellow, "No SmugMug Url returned :-( Not so SmugMug....");
               }
               Debug.WriteLine($"{customerEmailAddress} {customerSmugMugUrl} {customerPassword}");
            }
            catch (Exception e)
            {
               ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
            }
         }
      }

      public static void Email(string htmlString,  string customersAddress, string customerId)
      {
         try
         {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("Admin@KHainePhotography.co.uk");
            message.To.Add(new MailAddress(customersAddress));
            message.Bcc.Add(new MailAddress("Karen@KhainePhotography.co.uk"));
            message.Subject = $"KHainePhotographay Digital Downloads Now Availalble - {customerId}";
            message.IsBodyHtml = true; //to make message body as html  
            message.Body = htmlString;
            smtp.Port = 25;
            smtp.Host = "KHPServer"; //for gmail host  
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("Admin@KHainePhotography.co.uk", "Access12!");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
         }
         catch (Exception e) 
         {
            ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
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
               _uploader.ProcessImages("Haine", "Ha1ne99", year.Key, null, null, year.Value.Colour, year.Value.Sepia, year.Value.BandW);
            }
         }
      }
   }

   public class CustomerData
   {
      readonly IniFile _customerDataIniFile;

      public CustomerData(string customerId, string CustomerDataPath, string WebSitePath)
      {
         CustomerId = customerId;
         Path = new DirectoryInfo(CustomerDataPath + "\\" + customerId);
         if (!Path.Exists)
         {
            Path.Create();
         }

         WebPath = new DirectoryInfo(WebSitePath + "CustomerGallery\\" + CustomerId);
         if (!WebPath.Exists)
         {
            WebPath.Create();
         }

         var oldFileName = CustomerDataPath + "\\" + customerId + ".ini";
         var newFileName = CustomerDataPath + "\\" + customerId + "\\" + customerId + ".ini";

         if (!File.Exists(newFileName) && File.Exists(oldFileName))
            File.Move(oldFileName, newFileName);

         _customerDataIniFile = new IniFile(newFileName);
      }

      public DirectoryInfo Path { get; }

      public DirectoryInfo WebPath { get; }

      public string CustomerId { get; }

      public string UsersPassword
      {
         get { return _customerDataIniFile.IniReadValue("Contact", "Password"); }
         set { _customerDataIniFile.IniWriteValue("Contact", "Password", value); }
      }

   }

   public class CustomerShootData
   {
      public CustomerShootData(string customerId, string shootName, string customerDataPath, string websitePath)
         : this(new CustomerData(customerId, customerDataPath, websitePath), shootName, customerDataPath, websitePath)
      {
      }

      public CustomerShootData(CustomerData customerData, string shootName, string customerDataPath, string websitePath)
      {
         ShootName = shootName;
         CustomerData = customerData;

         ShootCustomerDataPath = new DirectoryInfo($@"{customerDataPath}\{customerData.CustomerId}\{shootName}");
         ShootWebPath = new DirectoryInfo($@"{websitePath}\CustomerGallery\{customerData.CustomerId}\{shootName}");
         ForCustomerPath = new DirectoryInfo($@"{ShootCustomerDataPath}\ForCustomer");
         OriginalsPath = new DirectoryInfo($@"{ForCustomerPath}\Original");
         EditsPath = new DirectoryInfo($@"{ForCustomerPath}\Edits");
      }

      public List<string> BandWEdits()
      {
         return new DirectoryInfo($@"{EditsPath}\BandW").GetFiles("*.jpg").Select(f => f.FullName).ToList();
      }
      public List<string> SepiaEdits()
      {
         return new DirectoryInfo($@"{EditsPath}\Sepia").GetFiles("*.jpg").Select(f => f.FullName).ToList();
      }
      public List<string> ColourEdits()
      {
         return new DirectoryInfo($@"{EditsPath}\Colour").GetFiles("*.jpg").Select(f => f.FullName).ToList();
      }
      public List<string> Videos()
      {
         return ForCustomerPath.GetFiles("*.mp4").Select(f => f.FullName).ToList();
      }
      public List<string> Originals()
      {
         return OriginalsPath.GetFiles("*.jpg").Select(f => f.FullName).ToList();
      }

      public CustomerData CustomerData { get; }
      public DirectoryInfo ShootWebPath { get; }
      public DirectoryInfo ShootCustomerDataPath { get; }
      private DirectoryInfo ForCustomerPath { get; }
      private DirectoryInfo EditsPath { get; }
      private DirectoryInfo OriginalsPath { get; }

      public string ShootName { get; }

   }
}
