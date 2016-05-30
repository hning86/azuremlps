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
    public sealed class AmlClient
    {
        public const string Version = "0.2.4";

        const string studioApiBaseURL = @"https://{0}studioapi.azureml.net/api/";

        const string manageApiBaseUrl = @"https://{0}management.azureml.net/";

        public string WorkspaceApi { get; private set; }

        public string WebServiceApi { get; private set; }

        public string UploadApi { get; private set; }

        public string UploadBlobApi { get; private set; }

        public string ApiName { get; set; } = "dotnetsdk";

        public ResourceClient<WorkspaceResource> Client { get; private set; }

        public WorkspaceResource Active { get; private set; }

        public AmlClient(ResourceClient<WorkspaceResource> client, WorkspaceResource resource)
        {
            this.Client = client;
            this.Active = resource;
            UpdateApi();
        }

        HttpClient GetHttp()
        {
            HttpClient hc = new HttpClient();

            // used by O16N API
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.Active.AuthorizationToken.PrimaryToken);

            // used by Studio API
            hc.DefaultRequestHeaders.Add("x-ms-metaanalytics-authorizationtoken", this.Active.AuthorizationToken.PrimaryToken);

            return hc;
        }

        async Task<AmlResult> GetAml(string url)
        {
            var hc = GetHttp();
            var r = await hc.GetAsync(url);
            return new AmlResult(r);
        }

        async Task<AmlResult> PutAml(string url)
        {
            var hc = GetHttp();
            var r = await hc.PutAsync(url, null);
            return new AmlResult(r);
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

        async Task<AmlResult> PutAml(string url, object content)
        {
            var hc = GetHttp();
            var sc = GetContent(content);
            var r = await hc.PutAsync(url, sc);
            return new AmlResult(r);
        }

        async Task<AmlResult> GetAzure(string url)
        {
            var hc = new HttpClient();
            var r = await hc.GetAsync(url);
            return new AmlResult(r);
        }

        async Task<AmlResult> DeleteAml(string url)
        {
            var hc = GetHttp();
            var r = await hc.DeleteAsync(url);
            return new AmlResult(r);
        }

        async Task<AmlResult> PostAml(string url)
        {
            var hc = GetHttp();
            var r = await hc.PostAsync(url, null);
            return new AmlResult(r);
        }

        async Task<AmlResult> PostAml(string url, object content)
        {
            var hc = GetHttp();
            var sc = GetContent(content);
            var r = await hc.PostAsync(url, sc);
            return new AmlResult(r);
        }

        async Task<AmlResult> PostAml(string url, ExperimentRequest er)
        {
            var hc = GetHttp();
            if (er.Etag != null)
                hc.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(er.Etag));

            var s = JsonConvert.SerializeObject(er);
            var sc = new StringContent(s, Encoding.ASCII, "application/json");

            var r = await hc.PostAsync(url, sc);
            return new AmlResult(r);
        }

        async Task<AmlResult> DeleteAml(string url, string etag)
        {
            var hc = GetHttp();
            hc.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag));

            var r = await hc.DeleteAsync(url);
            return new AmlResult(r);
        }


        async Task<AmlResult> PostAml(string url, Stream s)
        {
            var hc = GetHttp();
            var sc = new StreamContent(s);
            var r = await hc.PostAsync(url, sc);
            return new AmlResult(r);
        }

        void UpdateApi()
        {
            string key = string.Empty;
            switch (this.Active.Region.ToLower())
            {
                case "south central us":
                    key = "";
                    break;
                case "west europe":
                    key = "europewest.";
                    break;
                case "southeast asia":
                    key = "asiasoutheast.";
                    break;
                default:
                    throw new Exception("Unsupported location: " + this.Active.Region);
            }

            var studio = string.Format(studioApiBaseURL, key);
            var ws = string.Format(manageApiBaseUrl, key);

            this.WorkspaceApi = studio + "workspaces/" + this.Active.Id + "/";
            this.UploadApi = studio + "resourceuploads/workspaces/" + this.Active.Id + "/";
            this.UploadBlobApi = studio + "blobuploads/workspaces/" + Active.Id + "/";
            this.WebServiceApi = ws + "workspaces/" + this.Active.Id + "/webservices/";

        }

        public async Task<Workspace> GetWorkspace()
        {
            var r = await GetAml(this.WorkspaceApi);
            r.ThrowIfFailed();
            var ws = JsonConvert.DeserializeObject<Workspace>(r.Payload);
            return ws;
        }

        public async Task<string> AddWorkspaceUsers(string emails, string role)
        {
            var content = new
            {
                Role = role,
                Emails = emails
            };

            var r = await PostAml(this.WorkspaceApi + "/invitations", content);
            r.ThrowIfFailed();

            string p = await r.Payload;
            return p;
        }

        public async Task<WorkspaceUser[]> GetWorkspaceUsers()
        {
            var r = await GetAml(this.WorkspaceApi + "/users");
            r.ThrowIfFailed();

            var users =  JsonConvert.DeserializeObject<WorkspaceUser[]>(await r.Payload);
            return users;
        }

        #region Dataset

        public async Task<DataSource[]> GetDataSources()
        {
            var r = await GetAml(this.WorkspaceApi + "/datasources");
            r.ThrowIfFailed();

            var ds = JsonConvert.DeserializeObject<DataSource[]>(await r.Payload);
            return ds;
        }

        public async Task<string> DeleteDataset(string id)
        {
            var r = await DeleteAml(this.WorkspaceApi + "/datasources/family/" + id);
            r.ThrowIfFailed();

            return await r.Payload;
        }

        public async Task<DataSource> DownloadDataset(string id, string filename)
        {
            var r = await GetAml(this.WorkspaceApi + "/datasources/" + id);
            r.ThrowIfFailed();

            var ds = JsonConvert.DeserializeObject<DataSource>(await r.Payload);

            var uri = ds.DownloadLocation.BaseUri + ds.DownloadLocation.Location + ds.DownloadLocation.AccessCredential;
            var dr = await GetAzure(uri);
            dr.ThrowIfFailed();

            using (var s = await dr.GetStream())
            using (var fs = File.Create(filename))
            {
                await s.CopyToAsync(fs);
            }

            return ds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<ResourceUploadResponse> UploadResource(string fileName, DataSourceType type)
        {
            var uri = this.UploadsApi + "?userStorage=true&dataTypeId=" + type;

            using (var s = File.OpenRead(fileName))
            {
                var r = await PostAml(uri, s);
                r.ThrowIfFailed();
                var ur = await r.Payload;
                var response = JsonConvert.DeserializeObject<ResourceUploadResponse>(ur);
                return response;
            }
        }

        public async Task<string> UploadResourceInChunksAsnyc(int numOfBlocks, int blockId, string uploadId, string fileName, DataSourceType type)
        {
            var uri = this.UploadBlobApi 
                + "/?numberOfBlocks=" + numOfBlocks
                + "&blockId=" + blockId
                + "&uploadId=" + uploadId
                + "&dataTypeId=" + type;

            using (var s = File.OpenRead(fileName))
            {
                var r = await PostAml(uri, s);
                r.ThrowIfFailed();
                return await r.Payload;
            }
        }

        public async Task<string> StartDatasetSchemaGen(string dataTypeId, string uploadFileId, string datasetName, string description, string uploadFileName)
        {
            var schemaJob = new
            {
                DataSource = new
                {
                    Name = datasetName,
                    DataTypeId = dataTypeId,
                    Description = description,
                    FamilyId = string.Empty,
                    Owner = "PowerShell",
                    SourceOrigin = "FromResourceUpload"
                },

                UploadId = uploadFileId,
                UploadedFromFileName = Path.GetFileName(uploadFileName),
                ClientPoll = true
            };

            var uri = this.WorkspaceApi + "/datasources";
            var r = await PostAml(uri, schemaJob);
            r.ThrowIfFailed();

            var dataSourceId = (await r.Payload).Replace("\"", "");
            return dataSourceId;
        }

        public async Task<string> GetDatasetSchemaGenStatus(string dataSourceId)
        {
            var r = await GetAml(this.WebServiceApi + "/datasources/" + dataSourceId);
            r.ThrowIfFailed();

            var o = JObject.Parse(await r.Payload);
            var statusToken = o["SchemaStatus"];
            var s = statusToken.Value<string>();
            return s;
        }

        #endregion

        #region Custom Module

        public async Task<string> BeginParseCustomModuleJob(string moduleUploadMetadata)
        {
            var r = await PostAml(this.WorkspaceApi + "/modules/custom", moduleUploadMetadata);
            r.ThrowIfFailed();

            string activityId = (await r.Payload).Replace("\"", "");
            return activityId;            
        }

        public async Task<string> GetCustomModuleBuildJobStatus(string activityGroupId)
        {
            var r = await GetAml(this.WorkspaceApi + "/modules/custom?activityGroupId=" + activityGroupId);
            r.ThrowIfFailed();

            var jobStatus = await r.Payload;
            return jobStatus;
        }

        #endregion

        #region Experiment

        public async Task<Experiment[]> GetExperiments()
        {
            var r = await GetAml(this.WorkspaceApi + "/experiments");
            r.ThrowIfFailed();

            var exps = JsonConvert.DeserializeObject<Experiment[]>(await r.Payload);

            // only display user's own experiments.
            exps = exps.Where(e => e.Category == "user" || string.IsNullOrEmpty(e.Category)).ToArray();
            return exps;
        }

        // Workspace/TrainedModels/ListTrainedModels
        public async Task<TrainedModel[]> GetTrainedModels()
        {
            var r = await GetAml(this.WorkspaceApi + "/trainedmodels");
            r.ThrowIfFailed();

            var models = JsonConvert.DeserializeObject<TrainedModel[]>(await hr.Payload);
            return models;
        }

        public async Task<TrainedModel> GetTrainedModelById(string modelId)
        {
            var r = await GetAml(this.WorkspaceApi + "/trainedmodels/" + modelId);
            r.ThrowIfFailed();

            var model = JsonConvert.DeserializeObject<TrainedModel>(await r.Payload);
            return model;
        }

        public async Task<Experiment> GetExperimentById(string id)
        {
            var r = await GetAml(this.WorkspaceApi + "/experiments/" + id);
            r.ThrowIfFailed();

            var exp = JsonConvert.DeserializeObject<Experiment>(await r.Payload);
            return exp;
        }

        public async Task<string> RunExperiment(string id)
        {
            var e = await GetExperimentById(id);
            var req = new RunExperimentRequest(e);
            var uri = this.WorkspaceApi + "/experiments/" + e.ExperimentId;
            var r = await PostAml(uri, req);
            return await r.Payload;
        }

        public async Task<string> NewExperiment(Experiment e)
        {
            var req = new NewExperimentRequest(e);
            var uri = this.WorkspaceApi + "/experiments/";
            var r = await PostAml(uri, req);
            return await r.Payload;
        }

        public async Task<string> SaveAsExperiment(string id, string name)
        {
            var e = await GetExperimentById(id);
            if (e == null)
                throw new AmlException("experiment id "+id+" not found");
            e.Description = name;
            return await NewExperiment(e);
        }

        public async Task<string> UpdateExperiment(Experiment e)
        {
            if (e.ExperimentId == null || e.Etag == null)
                throw new AmlException("cannot update experiment");
            var uri = this.WorkspaceApi + "/experiments/" + e.ExperimentId;
            var u = new UpdateExperimentRequest(e);
            var r = await PostAml(uri, u);
            return await r.Payload;
        }

        public async Task<string> RemoveExperimentById(string id, bool removeHistory)
        {
            var e = await GetExperimentById(id);
            var uri = this.WorkspaceApi + "/experiments/" + e.ExperimentId + "?deleteAncestors=" + removeHistory;
            var r = await DeleteAml(uri, e.Etag);
            var p = await r.Payload;
            return p;
        }

        public async Task<Packing> Pack(string id)
        {
            var uri = this.WorkspaceApi
                + "/packages?api-version=2.0&experimentid=" + id
                + "/&clearCredentials=true&includeAuthorId=false";
            var r = await PostAml(uri, string.Empty);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var package = JsonConvert.DeserializeObject<Packing>(p);
            return package;
        }

        public async Task<T> GetActivity<T>(T a) where T : Activity
        {
            var uri = this.WorkspaceApi + "/packages?api-version=2.0&" + a.IdField;
            var r = await GetAml(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var response = JsonConvert.DeserializeObject<T>(p);
            return response;
        }

        public async Task<Unpacking> Unpack(Uri package)
        {
            var uri = this.WorkspaceApi + "/packages?api-version=2.0"
                + "&packageUri=" + HttpUtility.UrlEncode(package.AbsoluteUri);
            var r = await PutAml(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var unpacking = JsonConvert.DeserializeObject<Unpacking>(p);
            return unpacking;
        }

        public async Task<Unpacking> UnpackFromGallery(Uri package, Uri gallery, string entityId)
        {
            var uri = this.WorkspaceApi +  "/packages?api-version=2.0"
                + "&packageUri=" + HttpUtility.UrlEncode(package.AbsoluteUri)
                + "&communityUri=" + HttpUtility.UrlEncode(gallery.AbsoluteUri)
                + "&entityId=" + entityId;
            var r = await PutAml(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var unpacking = JsonConvert.DeserializeObject<Unpacking>(p);
            return unpacking;
        }

        #endregion

        #region Web Service

        public async Task<WebService[]> GetWebServices()
        {
            var r = await GetAml(this.WebServiceApi);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var services = JsonConvert.DeserializeObject<WebService[]>(p);
            return services;
        }

        public async Task<WebService> GetWebServiceById(string id)
        {
            var r = await GetAml(this.WebServiceApi + id);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var ws = JsonConvert.DeserializeObject<WebService>(p);
            return ws;
        }

        public async Task<DeployStatus> ExperimentDeployById(string id)
        {
            var uri = this.WorkspaceApi + "/experiments/" + id + "/webservice";
            var r = await PostAml(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var status = JsonConvert.DeserializeObject<DeployStatus>(p);
            return status;
        }

        public async Task<DeployStatus> GetDeploy(string id)
        {
            var uri = this.WorkspaceApi + "/experiments/" + id + "/webservice";
            var r = await GetAml(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var status = JsonConvert.DeserializeObject<DeployStatus>(p);
            return status;
        }

        public async Task<string> RemoveWebServiceById(string id)
        {
            var r = await DeleteAml(WebServiceApi + id);
            r.ThrowIfFailed();
            var p = await r.Payload;
            return p;
        }

        #endregion

        #region Web Service Endpoint

        public async Task<Endpoint[]> GetEndpoints(string id)
        {
            var uri = WebServiceApi + id + "/endpoints";
            var r = await GetAml(uri);
            r.ThrowIfFailed();
            var p = await r.Payload;
            var weps = JsonConvert.DeserializeObject<Endpoint[]>(p);
            return weps;
        }

        public async Task<Endpoint> GetEndpointByName(string id, string name)
        {
            var uri = WebServiceApi + id + "/endpoints/" + name;
            var r = await GetAml(uri);
            r.ThrowIfFailed();

            var p = await r.Payload;
            var e = JsonConvert.DeserializeObject<Endpoint>(p);
            return e;
        }

        public async Task<string> AddEndpoint(AddEndpointRequest req)
        {
            var uri = WebServiceApi + req.WebServiceId + "/endpoints/" + req.EndpointName;
            var r = await PutAml(uri, req);
            
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string body = jss.Serialize(req);
            AmlResult hr = Util.HttpPut(queryUrl, body).Result;
            if (!hr.IsSuccess)
                throw new AmlException(hr);
        }
        public void RefreshWebServiceEndPoint(WorkspaceSetting setting, string webServiceId, string endpointName, bool overwriteResources)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = ManageApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}/refresh", setting.WorkspaceId, webServiceId, endpointName);
            string body = "{\"OverwriteResources\": \"" + overwriteResources.ToString() + "\"}";
            AmlResult hr = Util.HttpPost(query, body).Result;
            if (!hr.IsSuccess)
                throw new AmlException(hr);
        }

        public void PatchWebServiceEndpoint(WorkspaceSetting setting, string webServiceId, string endpointName, dynamic patchReq)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string body = jss.Serialize(patchReq);
            string url = ManageApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, webServiceId, endpointName);
            AmlResult hr = Util.HttpPatch(url, body).Result;
            if (!hr.IsSuccess)
                throw new AmlException(hr);
        }

        public void RemoveWebServiceEndpoint(WorkspaceSetting setting, string webServiceId, string endpointName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = ManageApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, webServiceId, endpointName);
            AmlResult hr = Util.HttpDelete(queryUrl).Result;
            if (!hr.IsSuccess)
                throw new AmlException(hr);
        }
        #endregion

        #region Invoke Web Service Endpoint
        public string InvokeRRS(string PostRequestUrl, string apiKey, string input)
        {
            Util.AuthorizationToken = apiKey;
            AmlResult hr = Util.HttpPost(PostRequestUrl, input).Result;
            if (hr.IsSuccess)
                return hr.Payload;
            else
                throw new AmlException(hr);
        }

        public string SubmitBESJob(string submitJobRequestUrl, string apiKey, string jobConfig)
        {
            Util.AuthorizationToken = apiKey;
            AmlResult hr = Util.HttpPost(submitJobRequestUrl, jobConfig).Result;
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);
            
            string jobId = hr.Payload.Replace("\"", "");
            return jobId;
        }

        public void StartBESJob(string submitJobRequestUrl, string apiKey, string jobId)
        {
            Util.AuthorizationToken = apiKey;
            string startJobApiLocation = submitJobRequestUrl.Replace("jobs?api-version=2.0", "jobs/" + jobId + "/start?api-version=2.0");
            AmlResult hr = Util.HttpPost(startJobApiLocation, string.Empty).Result;
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);

        }

        public string GetBESJobStatus(string submitJobRequestUrl, string apiKey, string jobId, out string results)
        {
            Util.AuthorizationToken = apiKey;
            string getJobStatusApiLocation = submitJobRequestUrl.Replace("jobs?api-version=2.0", "jobs/" + jobId + "?api-version=2.0");
            JavaScriptSerializer jss = new JavaScriptSerializer();
            AmlResult hr = Util.HttpGet(getJobStatusApiLocation).Result;
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);
            dynamic parsed = jss.Deserialize<object>(hr.Payload);
            string jobStatus = parsed["StatusCode"];
            results = hr.Payload;            
            return jobStatus;
        }
        #endregion
    }
}
