using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace AzureMachineLearning
{
    public interface IEntityTag
    {
        string Etag { get; }
    }

    public sealed class AmlClient
    {
        AuthorizationToken token;

        public AmlClient(AuthorizationToken token)
        {
            this.token = token;
        }

        HttpClient GetHttp()
        {
            HttpClient hc = new HttpClient();

            // used by O16N API
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.PrimaryToken);

            // used by Studio API
            hc.DefaultRequestHeaders.Add("x-ms-metaanalytics-authorizationtoken", token.PrimaryToken);

            return hc;
        }

        public async Task<AmlResult> Get(Uri url)
        {
            var hc = GetHttp();
            var r = await hc.GetAsync(url);
            return new AmlResult(r);
        }

        public Task<AmlResult> Put(Uri url)
        {
            return Put(url, null);
        }

        static StringContent GetContent(object content)
        {
            // if passed anything but null or string, just serialize it
            string s = string.Empty;
            if (content != null)
            {
                s = content as string;

                if (s == null)
                    s = JsonConvert.SerializeObject(content);
            }

            var sc = new StringContent(s, Encoding.ASCII, "application/json");
            return sc;
        }

        public async Task<AmlResult> Put(Uri url, object content)
        {
            var hc = GetHttp();
            var sc = GetContent(content);
            var r = await hc.PutAsync(url, sc);
            return new AmlResult(r);
        }

        //public async Task<AmlResult> GetAzure(Uri url)
        //{
        //    var hc = new HttpClient();
        //    var r = await hc.GetAsync(url);
        //    return new AmlResult(r);
        //}

        public async Task<AmlResult> Delete(Uri url)
        {
            var hc = GetHttp();
            var r = await hc.DeleteAsync(url);
            return new AmlResult(r);
        }

        public Task<AmlResult> Post(Uri url)
        {
            return Post(url, null);
        }

        static void UpdateIfEtag(HttpClient hc, object o)
        {
            if (o == null)
                return;

            var e = o as IEntityTag;
            if (e == null)
                return;

            hc.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(e.Etag));
        }

        public async Task<AmlResult> Post(Uri url, object content)
        {
            var hc = GetHttp();
            UpdateIfEtag(hc, content);
            var sc = GetContent(content);
            var r = await hc.PostAsync(url, sc);
            return new AmlResult(r);
        }

        public async Task<AmlResult> Delete(Uri url, IEntityTag e)
        {
            var hc = GetHttp();
            hc.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(e.Etag));

            var r = await hc.DeleteAsync(url);
            return new AmlResult(r);
        }

        public async Task<AmlResult> Post(Uri url, Stream s)
        {
            var hc = GetHttp();
            var sc = new StreamContent(s);
            var r = await hc.PostAsync(url, sc);
            return new AmlResult(r);
        }

        public async Task<AmlResult> Patch(Uri url, object o)
        {
            var m = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            m.Content = GetContent(o);

            var hc = GetHttp();
            var r = await hc.SendAsync(m);
            return new AmlResult(r);
        }
    }
}
