#if UNITY_EDITOR
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Repetitionless.Editor.Updating
{
    internal static class UpdateChecker
    {
        private static HttpClient _client = new HttpClient();
        private static string _versionCache = "";

        public static string GetLatestVersion(bool forceRequest = false)
        {
            if (_versionCache != "" && !forceRequest)
                return _versionCache;

            HttpRequestMessage request = new HttpRequestMessage() {
                RequestUri = new Uri(Constants.GITHUB_TAGS_URL),
                Method = HttpMethod.Get
            };

            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Mozilla", "5.0"));

            Task<HttpResponseMessage> getTask;
            try {
                getTask = _client.SendAsync(request);
                getTask.Wait();
            } catch (Exception e) {
                Debug.LogException(e);
                return "";
            }

            HttpResponseMessage response = getTask.Result;
            Task<string> getContentTask = response.Content.ReadAsStringAsync();
            getContentTask.Wait();

            // Use dynamic json
            dynamic responseData = JArray.Parse(getContentTask.Result);
            
            // Latest tag will always be first in the array
            _versionCache = responseData[0]["name"];
            return _versionCache;
        }

        public static bool UpdateAvailable(string version)
        {
            return version != GetLatestVersion();
        }
    }
}
#endif