using AzureML.Contract;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AzureML
{
    public class ManagementUtil
    {
        private static HttpClient _httpClient;
        private static JavaScriptSerializer _javaScriptSerializer;
        private string _sdkName { get; set; }

        internal ManagementUtil(string sdkName)
        {
            _sdkName = sdkName;

            if (_javaScriptSerializer == null)
                _javaScriptSerializer = new JavaScriptSerializer();
        }

        private HttpClient GetHttpClient(string authKey, bool withAuthHeader = true)
        {
            if (_httpClient == null)
                _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Clear();
            // use a special header to track usage.
            _httpClient.DefaultRequestHeaders.Add("x-aml-sdk", _sdkName);

            if (withAuthHeader)
            {
                // used by O16N API
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authKey);
                // used by Studio API
                _httpClient.DefaultRequestHeaders.Add("x-ms-metaanalytics-authorizationtoken", authKey);
            }
            return _httpClient;
        }

        internal async Task<HttpResult> HttpPost(string authKey, string url, string jsonBody, bool withAuthHeader = true)
        {
            if (jsonBody == null)
                jsonBody = string.Empty;
            var hc = GetHttpClient(authKey, withAuthHeader);
            using (StringContent sc = new StringContent(jsonBody, Encoding.ASCII, "application/json"))
            using (HttpResponseMessage resp = await hc.PostAsync(url, sc).ConfigureAwait(false))
            {
                HttpResult hr = await CreateHttpResult(resp).ConfigureAwait(false);
                EnsureSuccessStatusCode(resp, hr);
                return hr;
            }
        }

        internal async Task<HttpResult<T>> HttpPost<T>(string authKey, string url, string jsonBody, bool withAuthHeader = true)
        {
            if (jsonBody == null)
                jsonBody = string.Empty;

            var hc = GetHttpClient(authKey, withAuthHeader);
            using (StringContent sc = new StringContent(jsonBody, Encoding.ASCII, "application/json"))
            using (HttpResponseMessage resp = await hc.PostAsync(url, sc).ConfigureAwait(false))
            {
                HttpResult<T> hr = await CreateHttpResult<T>(resp).ConfigureAwait(false);
                EnsureSuccessStatusCode(resp, hr);
                return hr;
            }
        }

        internal async Task<HttpResult> HttpPatch(string authKey, string url, string jsonBody)
        {
            if (jsonBody == null)
                jsonBody = string.Empty;
            HttpClient hc = GetHttpClient(authKey);
            using (StringContent sc = new StringContent(jsonBody, Encoding.ASCII, "application/json"))
            using (HttpResponseMessage resp = await hc.PatchAsJsonAsync(url, jsonBody).ConfigureAwait(false))
            {
                HttpResult hr = await CreateHttpResult(resp).ConfigureAwait(false);
                EnsureSuccessStatusCode(resp, hr);
                return hr;
            }
        }

        internal async Task<HttpResult> HttpPostFile(string authKey, string url, string filePath)
        {
            HttpClient hc = GetHttpClient(authKey);
            using (StreamContent sc = new StreamContent(File.OpenRead(filePath)))
            using (HttpResponseMessage resp = await hc.PostAsync(url, sc).ConfigureAwait(false))
            {
                HttpResult hr = await CreateHttpResult(resp).ConfigureAwait(false);
                EnsureSuccessStatusCode(resp, hr);
                return hr;
            }
        }

        internal async Task<HttpResult> CreateHttpResult(HttpResponseMessage hrm)
        {
            HttpResult hr = new HttpResult
            {
                StatusCode = (int)hrm.StatusCode,
                Payload = await hrm.Content.ReadAsStringAsync().ConfigureAwait(false),
                PayloadStream = await hrm.Content.ReadAsStreamAsync().ConfigureAwait(false),
                IsSuccess = hrm.IsSuccessStatusCode,
                ReasonPhrase = hrm.ReasonPhrase
            };
            return hr;
        }

        internal async Task<HttpResult<T>> CreateHttpResult<T>(HttpResponseMessage hrm)
        {
            HttpResult<T> hr = new HttpResult<T>
            {
                StatusCode = (int)hrm.StatusCode,
                Payload = await hrm.Content.ReadAsStringAsync().ConfigureAwait(false),
                PayloadStream = await hrm.Content.ReadAsStreamAsync().ConfigureAwait(false),
                IsSuccess = hrm.IsSuccessStatusCode,
                ReasonPhrase = hrm.ReasonPhrase
            };

            hr.DeserializedPayload = _javaScriptSerializer.Deserialize<T>(hr.Payload);

            return hr;
        }

        internal async Task<HttpResult> HttpDelete(string authKey, string url)
        {
            HttpClient hc = GetHttpClient(authKey);
            using (HttpResponseMessage resp = await hc.DeleteAsync(url).ConfigureAwait(false))
            {
                HttpResult hr = await CreateHttpResult(resp).ConfigureAwait(false);
                EnsureSuccessStatusCode(resp, hr);
                return hr;
            }
        }

        internal async Task<HttpResult> HttpPut(string authKey, string url, string body)
        {
            HttpClient hc = GetHttpClient(authKey);
            using (var content = new StringContent(body, Encoding.ASCII, "application/json"))
            using (HttpResponseMessage resp = await hc.PutAsync(url, content).ConfigureAwait(false))
            {
                HttpResult hr = await CreateHttpResult(resp).ConfigureAwait(false);
                EnsureSuccessStatusCode(resp, hr);
                return hr;
            }
        }

        internal async Task<HttpResult<T>> HttpPut<T>(string authKey, string url, string body)
        {
            HttpClient hc = GetHttpClient(authKey);
            using (var content = new StringContent(body, Encoding.ASCII, "application/json"))
            using (HttpResponseMessage resp = await hc.PutAsync(url, content).ConfigureAwait(false))
            {
                HttpResult<T> hr = await CreateHttpResult<T>(resp).ConfigureAwait(false);
                EnsureSuccessStatusCode(resp, hr);
                return hr;
            }
        }

        internal async Task<HttpResult> HttpGet(string authKey, string url, bool withAuthHeader = true)
        {
            var hc = GetHttpClient(authKey, withAuthHeader);
            using (HttpResponseMessage resp = await hc.GetAsync(url).ConfigureAwait(false))
            {
                HttpResult hr = await CreateHttpResult(resp).ConfigureAwait(false);
                EnsureSuccessStatusCode(resp, hr);
                return hr;
            }
        }

        private static void EnsureSuccessStatusCode(HttpResponseMessage resp, HttpResult hr)
        {
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
        }

        internal async Task<HttpResult<T>> HttpGet<T>(string authKey, string url, bool withAuthHeader = true)
        {
            var hc = GetHttpClient(authKey, withAuthHeader);
            HttpResponseMessage resp = await hc.GetAsync(url).ConfigureAwait(false);
            HttpResult<T> hr = await CreateHttpResult<T>(resp).ConfigureAwait(false);
            EnsureSuccessStatusCode(resp, hr);
            return hr;
        }

        internal string Serialize(object body)
        {
            return _javaScriptSerializer.Serialize(body);
        }

        internal T Deserialize<T>(string json)
        {
            return _javaScriptSerializer.Deserialize<T>(json);
        }
    }

    public static class ExtensionMethods
    {
        public static Task<HttpResponseMessage> PatchAsJsonAsync(this HttpClient client, string requestUri, string jsonBody)
        {
            using (StringContent sc = new StringContent(jsonBody, Encoding.ASCII, "application/json"))
            using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = sc })
                return client.SendAsync(request);
        }
    }
}
