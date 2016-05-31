using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureMachineLearning
{
    public sealed class ManagementClient
    {

        //this.WebServiceApi = ws + "workspaces/" + this.Active.Id + "/webservices/";

        public WorkspaceClient Workspace { get; private set; }

        public Uri WebServicesApi { get; private set; }
        public Uri DataSourcesApi { get; private set; }
        public AmlClient Aml { get; private set; }

        public ManagementClient(WorkspaceClient workspace)
        {
            this.Workspace = workspace;
            this.Aml = workspace.Aml;

            this.WebServicesApi = new Uri(workspace.Location.Management + "workspaces/" + workspace.Active.Id + "/webservices/");
            this.DataSourcesApi = new Uri(workspace.Location.Management + "workspaces/" + workspace.Active.Id + "/datasources/");
        }

        public async Task<string> GetDatasetSchemaGenStatus(string id)
        {
            var uri = new Uri(DataSourcesApi + id);
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();

            var o = JObject.Parse(await r.Payload);
            var statusToken = o["SchemaStatus"];
            var s = statusToken.Value<string>();
            return s;
        }

        public async Task<WebService[]> GetWebServices()
        {
            var r = await Aml.Get(WebServicesApi);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var services = JsonConvert.DeserializeObject<WebService[]>(p);
            return services;
        }

        public async Task<WebService> GetWebServiceById(string id)
        {
            var uri = new Uri(WebServicesApi + id);
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var ws = JsonConvert.DeserializeObject<WebService>(p);
            return ws;
        }

        public async Task<string> RemoveWebServiceById(string id)
        {
            var uri = new Uri(WebServicesApi + id);
            var r = await Aml.Delete(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            return p;
        }

        #region Endpoint

        public async Task<Endpoint[]> GetEndpoints(string id)
        {
            var uri = new Uri(WebServicesApi + id + "/endpoints");
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<Endpoint[]>();
        }

        public async Task<Endpoint> GetEndpointByName(string id, string name)
        {
            var uri = new Uri(WebServicesApi + id + "/endpoints/" + name);
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<Endpoint>();
        }

        public async Task<string> AddEndpoint(AddEndpointRequest req)
        {
            var uri = new Uri(WebServicesApi + req.WebServiceId + "/endpoints/" + req.EndpointName);
            var r = await Aml.Put(uri, req);
            r.ThrowIfFailed();
            return await r.GetPayload();
        }

        public async Task<string> RefreshEndpoint(string id, string name, bool overwriteResources)
        {
            var uri = new Uri(WebServicesApi + id + "/endpoints/" + name + "/refresh");
            var refreshRequest = new { OverwriteResources = overwriteResources };
            var r = await Aml.Post(uri, refreshRequest);
            r.ThrowIfFailed();
            return await r.GetPayload();
        }

        public async Task<string> PatchEndpoint(string id, string name, object patchReq)
        {
            var uri = new Uri(WebServicesApi + id + "/endpoints/" + name);
            var r = await Aml.Patch(uri, patchReq);
            r.ThrowIfFailed();
            var p = await r.Payload;
            return p;
        }

        public async Task<string> RemoveEndpoint(string id, string name)
        {
            var uri = new Uri(WebServicesApi + id + "/endpoints/" + name);
            var r = await Aml.Delete(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            return p;
        }

        #endregion
    }
}
