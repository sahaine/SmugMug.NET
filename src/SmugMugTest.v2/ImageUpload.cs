using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmugMug.v2.Authentication;
using SmugMug.v2.Types;
using SmugMugModel.oAuth;

namespace SmugMugModel
{
    public class UploadEventArgs : EventArgs
    {
        public float PercentComplete { get; internal set; }
        /// <summary>
        /// The filename of the image (or video).
        /// This header overrides whatever is set as the filename in the PUT endpoint.
        /// </summary>
        public string FileName { get; set; }
    }

    public class ImageUpload
    {
        #region Properties


        public NodeEntity Album { get; }

        private int chunkSize = 1024 * 32; //the default chunksize for reading from the file
        #endregion

        public event EventHandler<UploadEventArgs> UploadCompleted;
        public event EventHandler<UploadEventArgs> UploadStarted;
        public event EventHandler<UploadEventArgs> UploadProgress;

        /// <summary>
        /// Converts the MD5 sum from a byte array to a string
        /// </summary>
        /// <param name="arr">The byte array</param>
        /// <returns></returns>
        private static string GetStringFromHash(byte[] arr)
        {
            StringBuilder s = new StringBuilder();
            foreach (byte item in arr)
            {
                var first = item >> 4;
                var second = (item & 0x0F);

                s.AppendFormat("{0:X}{1:X}", first, second);
            }

            return s.ToString().ToLower();
        }

        /// <summary>
        /// This is the constructor for the ImageUpload object. It is private so you have to use the CreateUploader method
        /// </summary>
        /// <param name="SessionID">The session ID</param>
        /// <param name="AlbumID">The album ID</param>
        internal ImageUpload(NodeEntity Album)
        {
            this.Album = Album;
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="fileName">The filename we want to upload</param>
        /// <returns>An Image object that describes the image we uploaded</returns>
        public void UploadImage(string fileName)
        {
            var byteArr = File.ReadAllBytes(fileName);
            var MD5Sum = GetStringFromHash(System.Security.Cryptography.MD5.Create().ComputeHash(byteArr));

            UploadImage(fileName, MD5Sum);
        }

        public void UploadImage(string filename, string MD5Checksum)
        {
            UploadImageAsync(filename, MD5Checksum, CancellationToken.None).Wait();
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="filename">The filename we want to upload</param>
        /// <param name="MD5Checksum">The MD5 checksum of a file</param>
        /// <returns>An Image object that describes the image we uploaded</returns>
        public async Task UploadImageAsync(string fileName, CancellationToken token)
        {
            var byteArr = File.ReadAllBytes(fileName);
            var MD5Sum = GetStringFromHash(System.Security.Cryptography.MD5.Create().ComputeHash(byteArr));

            await UploadImageAsync(fileName, MD5Sum, token);
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="filename">The filename we want to upload</param>
        /// <param name="MD5Checksum">The MD5 checksum of a file</param>
        /// <returns>An Image object that describes the image we uploaded</returns>
        public async Task UploadImageAsync(string filename, string MD5Checksum, CancellationToken token)
        {
            const string url = "https://upload.smugmug.com/";

            try
            {
                var file = new FileInfo(filename);
                var myWebRequest = (HttpWebRequest)WebRequest.Create(url);

                myWebRequest.Method = WebRequestMethods.Http.Post;
                myWebRequest.ContentLength = file.Length;
                myWebRequest.UserAgent = "KHainePhotography PhotoStudioManager v1.0";

                switch (file.Extension.ToLower())
                {
                    case "mp4":
                        myWebRequest.ContentType = "video/mp4";
                        break;

                    case "jpg":
                        myWebRequest.ContentType = "image/jpeg";
                        break;

                    default:
                        myWebRequest.ContentType = "binary/octet-stream";
                        break;
                }

                // Add the authorization header
                myWebRequest.Headers.Add("Authorization", oAuthUtility.GetAuthorizationHeader(Album.Token, myWebRequest.RequestUri.AbsoluteUri));

                myWebRequest.Headers.Add("Content-MD5", MD5Checksum);

                myWebRequest.Headers.Add("X-Smug-ResponseType", "JSON");
                myWebRequest.Headers.Add("X-Smug-Version", "v2");

                myWebRequest.Headers.Add("X-Smug-AlbumUri", Album.Uri);
                myWebRequest.Headers.Add("X-Smug-FileName", file.Name);
                myWebRequest.Headers.Add("X-Smug-Title", Path.GetFileNameWithoutExtension(file.FullName));
                myWebRequest.Headers.Add("X-Smug-Caption", Path.GetFileNameWithoutExtension(file.FullName));

                myWebRequest.Headers.Add("X-Smug-Latitude", "53.457920");
                myWebRequest.Headers.Add("X-Smug-Longitude", "-1.464252");
                myWebRequest.Headers.Add("X-Smug-Altitude", "86");

                myWebRequest.Headers.Add("X-Smug-Pretty", "true");

                //we start reading from the file...

                //we have some elements to set
                //- request time out (compute this for 10 kb/sec speed)
                //- the chunk size to use when uploading (how much data to report after)
                UploadStarted?.Invoke(this, new UploadEventArgs() { FileName = file.FullName, PercentComplete = 0 });

                int timeOut = ((int)file.Length / 1024) * 1000;
                myWebRequest.Timeout = timeOut;
                myWebRequest.ReadWriteTimeout = timeOut;
                myWebRequest.AllowWriteStreamBuffering = true;

                long howMuchRead = 0;
                byte[] buffer = new byte[chunkSize];
                int readSoFar = 0;

                using (FileStream sr = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    using (var stream = await myWebRequest.GetRequestStreamAsync())
                    {
                        while (howMuchRead < file.Length)
                        {
                            //we try to read a chunk from the file
                            readSoFar = sr.Read(buffer, 0, chunkSize);
                            howMuchRead += readSoFar;

                            //we now write those files to the web.
                            await stream.WriteAsync(buffer, 0, readSoFar, token);

                            UploadProgress?.Invoke(this, new UploadEventArgs() { FileName = file.FullName, PercentComplete = (float)howMuchRead / (float)file.Length });
                        }
                    }
                }

                var resp = await myWebRequest.GetResponseAsync();
                string rez = string.Empty;
                using (StreamReader ns = new StreamReader(resp.GetResponseStream()))
                {
                    rez = await ns.ReadToEndAsync();
                }

                UploadCompleted?.Invoke(this, new UploadEventArgs() { FileName = file.FullName, PercentComplete = (float)howMuchRead / (float)file.Length });

                //we deserialize the image
                var response = JsonConvert.DeserializeObject<UploadReponse>(System.Uri.UnescapeDataString(rez));

                if (response.stat != "ok")
                    throw new ApplicationException(response.stat);

            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
                throw;
            }
        }

        public class UploadReponse
        {
            public string stat { get; set; }
            public string method { get; set; }
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
