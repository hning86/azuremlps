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

        internal string AuthorizationToken { get; set; }
        private string _sdkName { get; set; }

        internal ManagementUtil(string sdkName)
        {
            _sdkName = sdkName;

            if (_javaScriptSerializer == null)
                _javaScriptSerializer = new JavaScriptSerializer();
        }

        internal HttpClient GetAuthenticatedHttpClient()
        {
            return GetAuthenticatedHttpClient(AuthorizationToken);
        }

        internal HttpClient GetAuthenticatedHttpClient(string authCode)
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }

            _httpClient.DefaultRequestHeaders.Clear();

            // used by O16N API
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCode);
            // used by Studio API
            _httpClient.DefaultRequestHeaders.Add("x-ms-metaanalytics-authorizationtoken", authCode);
            // use a special header to track usage.
            _httpClient.DefaultRequestHeaders.Add("x-aml-sdk", _sdkName);

            return _httpClient;
        }

        internal async Task<HttpResult> HttpPost(string url, string jsonBody)
        {
            if (jsonBody == null)
                jsonBody = string.Empty;
            HttpClient hc = GetAuthenticatedHttpClient();
            using (StringContent sc = new StringContent(jsonBody, Encoding.ASCII, "application/json"))
            using (HttpResponseMessage resp = await hc.PostAsync(url, sc))
            {
                HttpResult hr = await CreateHttpResult(resp);
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
                return hr;
            }
        }

        internal async Task<HttpResult<T>> HttpPost<T>(string url, string jsonBody)
        {
            if (jsonBody == null)
                jsonBody = string.Empty;
            HttpClient hc = GetAuthenticatedHttpClient();
            StringContent sc = new StringContent(jsonBody, Encoding.ASCII, "application/json");
            HttpResponseMessage resp = await hc.PostAsync(url, sc);
            HttpResult<T> hr = await CreateHttpResult<T>(resp);
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
            return hr;
        }

        internal async Task<HttpResult> HttpPatch(string url, string jsonBody)
        {
            if (jsonBody == null)
                jsonBody = string.Empty;
            HttpClient hc = GetAuthenticatedHttpClient();
            using (StringContent sc = new StringContent(jsonBody, Encoding.ASCII, "application/json"))
            using (HttpResponseMessage resp = await hc.PatchAsJsonAsync(url, jsonBody))
            {
                HttpResult hr = await CreateHttpResult(resp);
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
                return hr;
            }
        }

        internal async Task<HttpResult> HttpPostFile(string url, string filePath)
        {
            HttpClient hc = GetAuthenticatedHttpClient();
            using (StreamContent sc = new StreamContent(File.OpenRead(filePath)))
            using (HttpResponseMessage resp = await hc.PostAsync(url, sc))
            {
                HttpResult hr = await CreateHttpResult(resp);
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
                return hr;
            }
        }

        internal async Task<HttpResult> CreateHttpResult(HttpResponseMessage hrm)
        {
            HttpResult hr = new HttpResult
            {
                StatusCode = (int)hrm.StatusCode,
                Payload = await hrm.Content.ReadAsStringAsync(),
                PayloadStream = await hrm.Content.ReadAsStreamAsync(),
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
                Payload = await hrm.Content.ReadAsStringAsync(),
                PayloadStream = await hrm.Content.ReadAsStreamAsync(),
                IsSuccess = hrm.IsSuccessStatusCode,
                ReasonPhrase = hrm.ReasonPhrase
            };

            hr.DeserializedPayload = _javaScriptSerializer.Deserialize<T>(hr.Payload);

            return hr;
        }

        internal async Task<HttpResult> HttpDelete(string url)
        {
            HttpClient hc = GetAuthenticatedHttpClient();
            using (HttpResponseMessage resp = await hc.DeleteAsync(url))
            {
                HttpResult hr = await CreateHttpResult(resp);
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
                return hr;
            }
        }
        internal Task<HttpResult> HttpPut(string url, string body)
        {
            return HttpPut(AuthorizationToken, url, body);
        }

        internal async Task<HttpResult> HttpPut(string authCode, string url, string body)
        {
            HttpClient hc = GetAuthenticatedHttpClient();
            if (authCode != string.Empty)
                hc = GetAuthenticatedHttpClient(authCode);
            using (var content = new StringContent(body, Encoding.ASCII, "application/json"))
            using (HttpResponseMessage resp = await hc.PutAsync(url, content))
            {
                HttpResult hr = await CreateHttpResult(resp);
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
                return hr;
            }
        }

        internal Task<HttpResult> HttpGet(string url)
        {
            return HttpGet(AuthorizationToken, url, true);
        }

        internal Task<HttpResult> HttpGet(string url, bool withAuthHeader)
        {
            return HttpGet(AuthorizationToken, url, withAuthHeader);
        }

        internal async Task<HttpResult> HttpGet(string authCode, string url, bool withAutHeader = true)
        {
            HttpClient hc = new HttpClient();
            if (withAutHeader)
                hc = GetAuthenticatedHttpClient(authCode);
            using (HttpResponseMessage resp = await hc.GetAsync(url))
            {
                HttpResult hr = await CreateHttpResult(resp);
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
                return hr;
            }
        }

        internal async Task<HttpResult<T>> HttpGet<T>(string authCode, string url, bool withAutHeader = true)
        {
            HttpClient hc = new HttpClient();
            if (withAutHeader)
                hc = GetAuthenticatedHttpClient(authCode);
            HttpResponseMessage resp = await hc.GetAsync(url);
            HttpResult<T> hr = await CreateHttpResult<T>(resp);
            if (!resp.IsSuccessStatusCode)
                throw new AmlRestApiException(hr);
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
