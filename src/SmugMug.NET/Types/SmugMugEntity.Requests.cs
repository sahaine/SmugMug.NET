﻿// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SmugMug.v2.Authentication;
using SmugMug.v2.Helpers;
using SmugMug.v2.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SmugMug.v2.Types
{
   public partial class SmugMugEntity
   {
      public static async Task<TResult> RetrieveEntityAsync<TResult>(OAuthToken oauthToken, string requestUri)
          where TResult : SmugMugEntity
      {
         var result = await DeserializeRequestAsync<TResult>(oauthToken, requestUri);
         if (result.Item1 == null)
            return null;

         result.Item1.Token = oauthToken;

         return result.Item1;
      }

      public static async Task<TResult[]> RetrieveEntityArrayAsync<TResult>(OAuthToken oauthToken, string requestUri)
          where TResult : SmugMugEntity
      {
         var result = await DeserializeRequestAsync<TResult[]>(oauthToken, requestUri);
         if (result.Item1 == null)
            return null;

         // if we don't have paging, return what we already have.
         if (result.Item2 == null)
            return result.Item1;

         //let's page.
         List<TResult> accumulator = new List<TResult>(result.Item1);
         var pages = result.Item2;
         while (accumulator.Count < pages.Total)
         {
            result = await DeserializeRequestAsync<TResult[]>(oauthToken, $"{Constants.Addresses.SmugMug}{pages.NextPage}");
            pages = result.Item2;

            accumulator.AddRange(result.Item1);
         }

         return accumulator.ToArray();
      }

      protected async Task<TResult[]> RetrieveEntityArrayAsync<TResult>(string requestUri)
          where TResult : SmugMugEntity
      {
         TResult[] result = await RetrieveEntityArrayAsync<TResult>(this.Token, requestUri);

         if (result == null)
            return null;

         // Let each object have the token
         foreach (var item in result)
         {
            item.Token = this.Token;
            item.Parent = this;
         }
         return result;
      }

      protected async Task<TResult> RetrieveEntityAsync<TResult>(string requestUri)
          where TResult : SmugMugEntity
      {
         TResult result = await RetrieveEntityAsync<TResult>(this.Token, requestUri);
         if (result == null)
            return null;

         result.Token = this.Token;
         result.Parent = this;
         return result;
      }

      private static async Task<Tuple<TResult, Pages>> DeserializeRequestAsync<TResult>(OAuthToken oauthToken, string requestUri)
      {
         using (var httpClient = HttpClientHelpers.CreateHttpClient(oauthToken))
         using (var response = await httpClient.GetAsync(requestUri))
         using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            return ParseResponse<TResult>(response, streamReader);
      }

      private static Tuple<TResult, Pages> ParseResponse<TResult>(HttpResponseMessage response, StreamReader streamReader)
      {
         using (var jsonTextReader = new JsonTextReader(streamReader))
         {
            CheckResponse<TResult>(response, null);

            JObject jsonObject = JObject.Load(jsonTextReader);
            JToken objectResponse = JsonHelpers.GetDataOrDefault(jsonObject);

            if (objectResponse == null)
               return Tuple.Create(default(TResult), default(Pages));

            JToken pagesResponse = JsonHelpers.GetPagesResultOrDefault(jsonObject);

            //TODO: Implement paging
            TResult result = DeserializeObject<TResult>(objectResponse);
            Pages pagedResponse = pagesResponse != null ? DeserializeObject<Pages>(pagesResponse) : null;

            return Tuple.Create(result, pagedResponse);
         }
      }

      private static TResult DeserializeObject<TResult>(JToken objectResponse)
      {
         using (var reader = objectResponse.CreateReader())
         {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters = new List<JsonConverter>();
            settings.Converters.Add(new StringEnumConverter());
            settings.ContractResolver = new PrivateMemberContractResolver();
            JsonSerializer jser = JsonSerializer.Create(settings);

            return jser.Deserialize<TResult>(reader);
         }
      }

      internal async Task GetRequestAsync(string requestUri)
      {
         using (var httpClient = HttpClientHelpers.CreateHttpClient(Token))
         using (var response = await httpClient.GetAsync(requestUri))
         using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            CheckResponse<Object>(response, streamReader);

      }

      internal async Task<TResult> PostRequestAsync<TResult>(string requestUri, string content)
      {
         using (var httpClient = HttpClientHelpers.CreateHttpClient(Token))
         using (var httpContent = new StringContent(content))
         using (var response = await httpClient.PostAsync(requestUri, httpContent))
         using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            return CheckResponse<TResult>(response, streamReader);

      }

      internal async Task PatchRequestAsync(string requestUri, string content)
      {
         using (var httpClient = HttpClientHelpers.CreateHttpClient(Token))
         using (var httpContent = new StringContent(content))
         using (var message = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri))
         {
            message.Content = httpContent;
            using (var response = await httpClient.SendAsync(message))
            using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
               CheckResponse<object>(response, streamReader);
         }
      }

      private static TResult CheckResponse<TResult>(HttpResponseMessage response, StreamReader responseBody)
      {
         if (response.IsSuccessStatusCode)
         {
            if (responseBody != null)
            {
               var data = ParseResponse<TResult>(response, responseBody);
               return data.Item1;
            }
            else
               return default;
         }
         else
         {
            var fileName = Path.GetTempFileName() + ".htm";
            File.WriteAllText(fileName, responseBody.ReadToEnd());
            Process.Start(fileName);
            throw new HttpRequestException($"{response.StatusCode}:{response.ReasonPhrase}");
         }
      }
   }
}
