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

            //_uploader.GetSubNode(_uploader.CustomersFolder, "test", SmugMug.v2.Types.TypeEnum.Folder, false, "Pa55w0rd");

            //ProcessImages("Wood", "2020-07-26");
            UploadFromArchives("2018*", false, true);
            UploadFromArchives("2019*", true, true);

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

      private static void UploadFromArchives(string search, bool incudeOriginals, bool emailCustomer)
      {
         var emailBody = File.ReadAllText(@"\\khpserver\Documents\EMailTempates\NewSmugMug.html");

         var archiveFolders = Directory.GetDirectories(@"\\vmwarehost\KHPArchive\KHP_ARCHIVE", search, SearchOption.TopDirectoryOnly);

         Console.WriteLine($"Search {search} resulted in {archiveFolders.Length} folders"); 

         foreach (var archiveFolder in archiveFolders)
         {
            try
            {
               var smugMugUploadCompleteFile = $@"{archiveFolder}\SmugMugUpload.Complete";
               var folderName = new DirectoryInfo(archiveFolder).Name;

               if (File.Exists(smugMugUploadCompleteFile))
               {
                  ConsolePrinter.Write(ConsoleColor.Yellow, $"{folderName} Has already been uploaded");
                  continue;
               }
               
               var parts = folderName.Split('_');
               var customerId = parts.Last();

               if (string.Equals(customerId, "Test", StringComparison.InvariantCultureIgnoreCase) ||
                   string.Equals(customerId, "Haine", StringComparison.InvariantCultureIgnoreCase))
               {
                  continue;
               }

               var iniFileName = $@"\\khpserver\CustomerData\{customerId}\{customerId}.ini";

               if (!File.Exists(iniFileName ) )
               {
                  File.WriteAllText(iniFileName, string.Empty);
               }

               var customerIniFile = new IniFile(iniFileName);
               var customerPassword = customerIniFile.IniReadValue("Contact", "Password");
               var customerEmailAddress = customerIniFile.IniReadValue("Contact", "EMailAddress");
               var customerSmugMugUrl = customerIniFile.IniReadValue("Account", "SmugMugUrl");

               var shootId = parts.First();
               if (customerId == "Haine" && shootId.Substring(0, 2) == "20")
               {
                  shootId = shootId.Substring(0, 4);
                  incudeOriginals = false;
               }
              
               var original = incudeOriginals ? GetFiles($@"{archiveFolder}\ForCustomer\Original", "*.jpg") : new List<string>();
               
               var colour = GetFiles($@"{archiveFolder}\ForCustomer\Edits\colour", "*.jpg");
               var sepia = GetFiles($@"{archiveFolder}\ForCustomer\Edits\sepia", "*.jpg");
               var bandW = GetFiles($@"{archiveFolder}\ForCustomer\Edits\BandW", "*.jpg");

               var video = GetFiles($@"{ archiveFolder}\ForCustomer", "*.mp4");

               var newCustomerSmugMugUrl = _uploader.ProcessImages(customerId, customerPassword, shootId, original, video, colour, sepia, bandW);

               File.WriteAllText(smugMugUploadCompleteFile, string.Empty); 

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

                  if (emailCustomer &&
                     !string.IsNullOrEmpty(customerEmailAddress) &&
                     string.IsNullOrEmpty(customerIniFile.IniReadValue("SmugMug", "EmailSent")))
                  {
                     try
                     {
                        Console.WriteLine($"Emailing {customerEmailAddress} for accourn {customerId}");
                        Email(emailBody.Replace("SMUGMUGURLADDRESS", customerSmugMugUrl), customerEmailAddress, customerId);
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
               Debug.WriteLine($"{customerEmailAddress} {customerSmugMugUrl} {customerPassword}");
            }
            catch (Exception e)
            {
               ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
            }
         }
      }

      private static List<string> GetFiles(string path, string pattern)
      {
         if (!Directory.Exists(path))
         {
            ConsolePrinter.Write(ConsoleColor.Yellow, $"Path does note exists - {path}");
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
