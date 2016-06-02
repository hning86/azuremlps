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
    public sealed class WorkspaceStorageClient
    {
        public AzureClient AzureClient { get; private set; }

        public WorkspaceStorageClient(AzureClient client)
        {
            this.AzureClient = client;
        }

        public const string WorkspacesApi = "workspaces/";

        public async Task<Workspace[]> Get()
        {
            var req = this.AzureClient.Request(WorkspacesApi);

            using (var wr = (HttpWebResponse) await req.GetResponseAsync())
            using (var sr = new StreamReader(wr.GetResponseStream()))
            {
                var s = await sr.ReadToEndAsync();
                var resources = JsonConvert.DeserializeObject<Workspace[]>(s);
                return resources;
            }
        }

        public async Task<Workspace> Get(string id)
        {
            var req = this.AzureClient.Request(WorkspacesApi + id);

            using (var resp = await req.GetResponseAsync())
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var s = await sr.ReadToEndAsync();
                var resource = JsonConvert.DeserializeObject<Workspace>(s);
                return resource;
            }
        }

        public async Task<string> Add(AddWorkspaceRequest add)
        {
            var guid = Guid.NewGuid().ToString("N");
            var req = this.AzureClient.ResourceRequest(WorkspacesApi + guid, HttpMethod.Put);
            var payload = JsonConvert.SerializeObject(add);

            var buffer = Encoding.ASCII.GetBytes(payload);
            req.ContentLength = buffer.Length;

            var stream = await req.GetRequestStreamAsync();
            await stream.WriteAsync(buffer, 0, buffer.Length);

            using (var resp = await req.GetResponseAsync())
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var result = await sr.ReadToEndAsync();
                var p = JObject.Parse(result);
                var id = p.GetValue("Id").Value<string>();
                return id;
            }
        }

        public async Task<string> Remove(string id)
        {
            var req = this.AzureClient.ResourceRequest(WorkspacesApi + id, HttpMethod.Delete);

            using (var resp = await req.GetResponseAsync())
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var s = await sr.ReadToEndAsync();
                return s;
            }
        }
    }
}
