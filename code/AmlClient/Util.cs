using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureMachineLearning
{
    internal class Util
    {
        internal AuthorizationToken Token { get; private set; }

        public Util(AuthorizationToken token)
        {
            this.Token = token;
        }

        public HttpClient GetAuthenticatedHttpClient()
        {
            HttpClient hc = new HttpClient();
            // used by O16N API
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.Token.PrimaryToken);
            // used by Studio API
            hc.DefaultRequestHeaders.Add("x-ms-metaanalytics-authorizationtoken", this.Token.PrimaryToken);
            return hc;
        }

        public async Task<AmlResult> HttpPost(string url, string jsonBody)
        {
            if (jsonBody == null)
                jsonBody = string.Empty;

            var hc = GetAuthenticatedHttpClient();
            var sc = new StringContent(jsonBody, Encoding.ASCII, "application/json");
            var resp = await hc.PostAsync(url, sc);

            var hr = await CreateHttpResult(resp);
            return hr;
        }

        public async Task<AmlResult> HttpPatch(string url, string jsonBody)
        {
            if (jsonBody == null)
                jsonBody = string.Empty;

            var hc = GetAuthenticatedHttpClient();
            var sc = new StringContent(jsonBody, Encoding.ASCII, "application/json");
            var resp = await hc.PatchAsJsonAsync(url, jsonBody);

            var hr = await CreateHttpResult(resp);
            return hr;
        }

        public async Task<AmlResult> HttpPostFile(string url, string filePath)
        {
            var hc = GetAuthenticatedHttpClient();
            var sc = new StreamContent(File.OpenRead(filePath));
            var resp = await hc.PostAsync(url, sc);
            var hr = await CreateHttpResult(resp);
            return hr;
        }

        public async Task<AmlResult> HttpDelete(string url)
        {
            var hc = GetAuthenticatedHttpClient();
            var resp = await hc.DeleteAsync(url);
            var hr = await CreateHttpResult(resp);
            return hr;
        }

        public async Task<AmlResult> HttpPut(string url, string body)
        {
            var hc = GetAuthenticatedHttpClient();
            var resp = await hc.PutAsync(url, new StringContent(body, Encoding.ASCII, "application/json"));
            var hr = await CreateHttpResult(resp);
            return hr;
        }

        public async Task<AmlResult> HttpGet(string url)
        {
            return await HttpGet(url, true);
        }

        public async Task<AmlResult> HttpGet(string url, bool withAuthHeader)
        {
            var hc = new HttpClient();
            var resp = await hc.GetAsync(url);
            var hr = await CreateHttpResult(resp);
            return hr;
        }
    }

    public static class ExtensionMethods { 
        public static Task<HttpResponseMessage> PatchAsJsonAsync(this HttpClient client, string requestUri, string jsonBody)
        {
            StringContent sc = new StringContent(jsonBody, Encoding.ASCII, "application/json");            
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = sc };
            return client.SendAsync(request);
        }
    }
}
