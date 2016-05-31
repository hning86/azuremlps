using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace AzureMachineLearning
{
    public class ResourceClient<T>
    {
        public AzureClient AzureClient { get; private set; }

        public string Suffix { get; private set; }

        public ResourceClient(AzureClient client, string suffix)
        {
            this.AzureClient = client;
            this.Suffix = suffix;
        }

        public async Task<T[]> Get()
        {
            var req = this.AzureClient.ResourceRequest(this.Suffix);

            using (var wr = (HttpWebResponse) await req.GetResponseAsync())
            using (var sr = new StreamReader(wr.GetResponseStream()))
            {
                var s = await sr.ReadToEndAsync();
                var resources = JsonConvert.DeserializeObject<T[]>(s);
                return resources;
            }
        }

        public async Task<string> Create(string name, string location, string storageAccountName, string storageAccountKey, string ownerEmail, string source)
        {
            // initial workspace is a made-up but valid guid
            var guid = Guid.NewGuid().ToString("N");
            var req = this.AzureClient.ResourceRequest("resources/" + guid, HttpMethod.Put);

            var payload = JsonConvert.SerializeObject(new
            {
                Name = name,
                Location = location,
                StorageAccountName = storageAccountName,
                StorageAccountKey = storageAccountKey,
                OwnerId = ownerEmail,
                ImmediateActivation = true,
                Source = source
            });

            req.ContentLength = payload.Length;
            var stream = req.GetRequestStream();
            byte[] buffer = Encoding.UTF8.GetBytes(payload);
            stream.Write(buffer, 0, buffer.Length);

            using (var resp = await req.GetResponseAsync())
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var result = sr.ReadToEnd();

                var o = JObject.Parse(result);
                var idToken = o["Id"];
                var s = JsonConvert.SerializeObject(idToken);
                return s;
            }
        }

        public async Task<T> Get(string id)
        {
            var req = this.AzureClient.ResourceRequest(this.Suffix + id);

            using (var resp = await req.GetResponseAsync())
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var s = await sr.ReadToEndAsync();
                var resource = JsonConvert.DeserializeObject<T>(s);
                return resource;
            }
        }

        public async Task<string> Remove(string id)
        {
            var req = this.AzureClient.ResourceRequest(this.Suffix + id, HttpMethod.Delete);

            using (var resp = await req.GetResponseAsync())
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var s = await sr.ReadToEndAsync();
                return s;
            }
        }
    }
}
