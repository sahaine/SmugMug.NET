// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SmugMug.v2.Authentication;
using SmugMug.v2.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmugMug.v2.Types
{
   public partial class SmugMugEntity
   {

      private string _uri;
      private string _uriDescription;
      private string _nodeId;

      public SmugMugEntity()
      {
      }

      public SmugMugEntity(OAuthToken token)
      {
         Token = token;
      }

      public OAuthToken Token { get; set; }

      public Dictionary<string, UriDescriptor> Uris { get; set; }

      public string Uri
      {
         get
         {
            return _uri;
         }
         set
         {
            if (_uri != value)
            {
               value = value.Replace("api/v2/api/v2", "api/v2");
               NotifyPropertyValueChanged("Uri", oldValue: _uri, newValue: value);
               _uri = value;
            }
         }
      }

      public string UriDescription
      {
         get
         {
            return _uriDescription;
         }
         set
         {
            if (_uriDescription != value)
            {
               NotifyPropertyValueChanged("UriDescription", oldValue: _uriDescription, newValue: value);
               _uriDescription = value;
            }
         }
      }

      public SmugMugEntity Parent { get; set; }

      public virtual string EntityId { get { return string.Empty; } }

      public virtual string PatchUri { get { return Uri; } }

      public async Task SaveAsync()
      {
         // We get the modified properties and post them to the objects's uri
         var patchPropertiesWithValues = GetPropertiesValue(GetPatchPropertiesName());
         if (patchPropertiesWithValues?.Any() == true)
         {
            await PatchRequestAsync(Constants.Addresses.SmugMug + AppendSuffixToUrl(this.PatchUri), JsonHelpers.GetPayloadAsJson(patchPropertiesWithValues));
         }
      }

      protected async Task<TResult> CreateAsync<TResult>(string uri)
      {
         var postPropertiesWithValues = GetPropertiesValue(GetPostPropertiesName());
         return await PostRequestAsync<TResult>(uri, JsonHelpers.GetPayloadAsJson(postPropertiesWithValues));
      }

      protected string AppendSuffixToUrl(string url)
      {
         // we are going to use the value on the Uri and use that, if it exists.
         if (string.IsNullOrEmpty(Uri))
            return url;

         // if the Uri string is not empty, has more than 2 characters and we have a suffix (ie. image-1), use that.
         if (Uri.Length > 2 && Uri[Uri.Length - 2] == '-')
         {
            return $"{url}-{Uri[Uri.Length - 1]}";
         }

         if (url.Length > 2 && url[url.Length - 2] == '-')
            return url;

         //return $"{url}-0";
         return url;
      }

      public string NodeId
      {
         get
         {
            if (_nodeId != null)
            {
               return _nodeId;
            }

            // get it from the Node Uri.
            UriDescriptor val;
            if (Uris.TryGetValue("Node", out val))
            {
               var posLastSlash = val.Uri.LastIndexOf("/");
               if (posLastSlash >= 0)
                  _nodeId = val.Uri.Substring(posLastSlash + 1);
               else
                  _nodeId = string.Empty;
            }
            return _nodeId;
         }
         set
         {
            _nodeId = value;
         }
      }
   }
}
