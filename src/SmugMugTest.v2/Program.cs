namespace SmugMugTest
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Net;
   using System.Net.Mail;

   using PhotoStudioManager.Common;
   using SmugMug.NET.Utility;

   using SmugMug.v2.Authentication;
   using SmugMug.v2.Authentication.Tokens;
   using SmugMugShared.Extensions;

   class Program
   {
      private static OAuthToken _oauthToken;
      private static ImageUploader _uploader;

      private static string EmailBody = File.ReadAllText(@"\\khpserver\Documents\EMailTempates\NewSmugMug.html");

      static void Main(string[] args)
      {
         Console.CursorVisible = false;

         ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, error) =>
         {
            Debug.WriteLine(cert.Subject);
            return cert.Subject == "CN=smugmug.com" || cert.Subject == "CN=khainephotography.co.uk";
         };

         try
         {
            _oauthToken = ConsoleAuthentication.GetOAuthTokenFromProvider(new FileTokenProvider());
            _uploader = new ImageUploader(_oauthToken);

            UploadFromArchives("2016*", false, true, true);
            UploadFromArchives("2017*", false, true, true);
            UploadFromArchives("2018*", false, true, true);
            UploadFromArchives("2019*", true, true, true);
            UploadFromArchives("2020*", true, true, true);

            //ProcessImages("Wood", "2020-07-26");
            //UploadArchiveFolder(@"\\VMWareHost\b$\KHPArchiveToProcess\2017-02-26_Sharp", false, true, true);
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

      private static string ProcessImages(string customerId, string shootId)
      {
         const string customerDataPath = @"\\khpserver\CustomerData";
         const string websitePath = @"\\khpserver\WebSites\KHainePhotography.co.uk";

         var shootData = new CustomerShootData(customerId, shootId, customerDataPath, websitePath);

         return _uploader.ProcessImages(customerId, shootData.CustomerData.UsersPassword, shootId, shootData.Originals(), shootData.Videos(), shootData.ColourEdits(), shootData.SepiaEdits(), shootData.BandWEdits());
      }

      private static void UploadFromArchives(string search, bool incudeOriginals, bool emailCustomer, bool useUploadedCheck)
      {
         var archiveLocations = new[] { @"\\vmwarehost\KHPArchive\KHP_ARCHIVE",
                                        @"\\VMWareHost\b$\KHPArchiveToProcess"};

         var archiveFolders = archiveLocations.Where(Directory.Exists)
                       .SelectMany(f => Directory.GetDirectories(f, search, SearchOption.TopDirectoryOnly))
                       .Select(f => new DirectoryInfo(f))
                       .Where(f => ShouldProcess(f, useUploadedCheck))
                       .OrderBy(f => f.Name)
                       .ToList();

         Console.WriteLine($"Search {search} resulted in {archiveFolders.Count} folders");

         foreach (var archiveFolder in archiveFolders)
         {
            UploadArchiveFolder(archiveFolder, incudeOriginals, emailCustomer, useUploadedCheck);
         }
      }

      private static bool ShouldProcess(DirectoryInfo archiveFolder, bool useUploadedCheck)
      {
         var folderName = archiveFolder.Name;

         var parts = folderName.Split('_');
         var customerId = parts.Last();

         if (string.Equals(customerId, "Test", StringComparison.InvariantCultureIgnoreCase) ||
             string.Equals(customerId, "GoApe", StringComparison.InvariantCultureIgnoreCase))
         {
            return false;
         }

         if (!Directory.Exists($@"{archiveFolder}\ForCustomer\Edits"))
         {
            ConsolePrinter.Write(ConsoleColor.Yellow, $"{folderName} has no edits folder");
            return false;
         }

         var iniFileName = $@"\\khpserver\CustomerData\{customerId}\{customerId}.ini";
         if (!File.Exists(iniFileName))
         {
            if (!Directory.Exists($@"\\khpserver\CustomerData\{customerId}"))
            {
               Directory.CreateDirectory($@"\\khpserver\CustomerData\{customerId}");
            }

            File.WriteAllText(iniFileName, string.Empty);
         }

         var smugMugUploadCompleteFile = $@"{archiveFolder}\SmugMugUpload.Complete";
         var customerIniFile = new IniFile(iniFileName);

         if (useUploadedCheck && File.Exists(smugMugUploadCompleteFile) && !string.IsNullOrEmpty(customerIniFile.IniReadValue("Account", "SmugMugUrl")))
         {
            ConsolePrinter.Write(ConsoleColor.Yellow, $"{folderName} has already been uploaded");
            return false;
         }

         return true;
      }

      private static void UploadArchiveFolder(DirectoryInfo archiveFolder, bool incudeOriginals, bool emailCustomer, bool useUploadedCheck)
      {
         try
         {
            var folderName = archiveFolder.Name;

            var parts = folderName.Split('_');
            var customerId = parts.Last();
            var iniFileName = $@"\\khpserver\CustomerData\{customerId}\{customerId}.ini";
            var customerIniFile = new IniFile(iniFileName);

            var customerPassword = customerIniFile.IniReadValue("Contact", "Password");
            var customerEmailAddress = customerIniFile.IniReadValue("Contact", "EMailAddress");
            var customerSmugMugUrl = customerIniFile.IniReadValue("Account", "SmugMugUrl");

            var shootId = parts.First();
            if (customerId == "Haine")
            {
               shootId = shootId.Substring(0, 4);
               incudeOriginals = false;
            }

            var original = incudeOriginals ? GetFiles($@"{archiveFolder.FullName}\ForCustomer\Original", "*.jpg") : new List<string>();

            var colour = GetFiles($@"{archiveFolder.FullName}\ForCustomer\Edits\colour", "*.jpg");
            var sepia = GetFiles($@"{archiveFolder.FullName}\ForCustomer\Edits\sepia", "*.jpg");
            var bandW = GetFiles($@"{archiveFolder.FullName}\ForCustomer\Edits\BandW", "*.jpg");
            var video = GetFiles($@"{archiveFolder.FullName}\ForCustomer", "*.mp4");

            var newCustomerSmugMugUrl = _uploader.ProcessImages(customerId, customerPassword, shootId, original, video, colour, sepia, bandW);

            if (!string.IsNullOrEmpty(newCustomerSmugMugUrl))
            {
               var smugMugUploadCompleteFile = $@"{archiveFolder.FullName}\SmugMugUpload.Complete";
               File.WriteAllText(smugMugUploadCompleteFile, string.Empty);

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

               if (emailCustomer &&
                  !string.IsNullOrEmpty(customerEmailAddress) &&
                  string.IsNullOrEmpty(customerIniFile.IniReadValue("SmugMug", "EmailSent")))
               {
                  try
                  {
                     Console.WriteLine($"Emailing {customerEmailAddress} for account {customerId}");
                     Email(EmailBody.Replace("SMUGMUGURLADDRESS", customerSmugMugUrl), customerEmailAddress, customerId);
                     customerIniFile.IniWriteValue("SmugMug", "EmailSent", "true");
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
         }
         catch (Exception e)
         {
            ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
         }
      }

      private static List<string> GetFiles(string path, string pattern)
      {
         if (!Directory.Exists(path))
         {
            ConsolePrinter.Write(ConsoleColor.Yellow, $"Path does not exists - {path}");
            return new List<string>();
         }

         return Directory.GetFiles(path, pattern).Where(f => !f.ToLower().Contains("facebook")).ToList();
      }

      public static void Email(string htmlString, string customersAddress, string customerId)
      {
         MailMessage message = new MailMessage();
         SmtpClient smtp = new SmtpClient();
         message.From = new MailAddress("Admin@KHainePhotography.co.uk");
         message.To.Add(new MailAddress(customersAddress));
         message.Bcc.Add(new MailAddress("Karen@KhainePhotography.co.uk"));
         message.Subject = $"KHainePhotographay Digital Downloads Now Availalble - {customerId}";
         message.IsBodyHtml = true;
         message.Body = htmlString;
         smtp.Port = 25;
         smtp.Host = "KHPServer";
         smtp.EnableSsl = true;
         smtp.UseDefaultCredentials = false;
         smtp.Credentials = new NetworkCredential("Admin@KHainePhotography.co.uk", "Access12!");
         smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
         smtp.Send(message);
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
