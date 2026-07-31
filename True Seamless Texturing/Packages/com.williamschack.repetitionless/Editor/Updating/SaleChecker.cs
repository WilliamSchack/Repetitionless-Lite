#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Repetitionless.Editor.Updating
{
    internal static class SaleChecker
    {
        private enum ESalePlatform
        {
            All,
            Unity,
            Itch
        }

        [Serializable]
        private struct SaleInfo
        {
            public ESalePlatform Platform;
            public int PercentOff;
            public DateTime StartDate;
            public DateTime EndDate;
        }

        [Serializable]
        private struct SaleInfoJson
        {
            public int platform;
            public int percentOff;
            public string startDate;
            public string endDate;
        }

        [Serializable]
        private struct SaleInfoJsonArray
        {
            public SaleInfoJson[] items;
        }

        private const string SALES_FILE_URL = "https://data.wilschack.dev/repetitionless/sales.json";
        private const string CACHE_FILE_PATH = Constants.LIBRARY_PATH + "/sales.json";
        private const string DATE_TIME_FORMAT = "dd-MM-yyyy";

        private static List<SaleInfo> _sales = null;

        private static HttpClient _client = new HttpClient();

        private static FileInfo GetCacheFileInfo()
        {
            FileInfo prefsFileInfo = new FileInfo(CACHE_FILE_PATH);
            if (!prefsFileInfo.Exists)
                CreateCache();

            return prefsFileInfo;
        }

        private static void CreateCache()
        {
            FileInfo prefsFileInfo = new FileInfo(CACHE_FILE_PATH);
            if (prefsFileInfo.Exists) return;

            string parentDir = prefsFileInfo.DirectoryName;
            if (!Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);

            string contents = "[]";
            File.WriteAllText(prefsFileInfo.FullName, contents);
        }

        public static void FetchSalesAndUpdateCache()
        {
            // Get sales data
            HttpRequestMessage request = new HttpRequestMessage() {
                RequestUri = new Uri(SALES_FILE_URL),
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
                return;
            }

            HttpResponseMessage response = getTask.Result;
            Task<string> getContentTask = response.Content.ReadAsStringAsync();
            getContentTask.Wait();

            Debug.Log(getContentTask.Result);

            // Save to cache
            FileInfo cacheFileInfo = GetCacheFileInfo();
            File.WriteAllText(cacheFileInfo.FullName, getContentTask.Result);

            ReadCache();
        }

        public static void ReadCache(bool force = false)
        {
            // Only read the cache if required
            if (_sales != null && !force)
                return;

            if (_sales == null)
                _sales = new List<SaleInfo>();
            else
                _sales.Clear();

            FileInfo cacheFileInfo = GetCacheFileInfo();
            string json = File.ReadAllText(cacheFileInfo.FullName);

            // JSONUtility cannot read root json arrays, put the array in an object
            string wrappedJson = $"{{\"items\":{json}}}";
            SaleInfoJsonArray responseJson = JsonUtility.FromJson<SaleInfoJsonArray>(wrappedJson);

            foreach(SaleInfoJson saleJson in responseJson.items) {
                SaleInfo sale;
                sale.Platform = (ESalePlatform)saleJson.platform;
                sale.PercentOff = saleJson.percentOff;
                sale.StartDate = DateTime.ParseExact(saleJson.startDate, DATE_TIME_FORMAT, null);
                sale.EndDate = DateTime.ParseExact(saleJson.endDate, DATE_TIME_FORMAT, null);

                Debug.Log(sale.Platform);
                Debug.Log(sale.PercentOff);
                Debug.Log(sale.StartDate);
                Debug.Log(sale.EndDate);

                _sales.Append(sale);
            }
        }

        // Returns 0 ifno sale is active
        public static int ActiveSalePercent()
        {
            return 0;
        }
    }
}
#endif