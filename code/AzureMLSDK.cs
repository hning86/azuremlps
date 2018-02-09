using AzureML.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Text.RegularExpressions;

namespace AzureML
{
    public class ManagementSDK
    {
        public const string Version = "0.3.4";        
        private string _studioApiBaseURL = @"https://{0}studioapi.azureml{1}/api/";
        private string _webServiceApiBaseUrl = @"https://{0}management.azureml{1}/";

        private string _azMgmtApiBaseUrl = @"https://management.core.windows.net/{0}/cloudservices/amlsdk/resources/machinelearning/~/workspaces/";
        private string httpResponsePayload = string.Empty;

        public string StudioApi = "https://studioapi.azureml.net/api/";
        public string WebServiceApi = "https://management.azureml.net/";
        public string GraphLayoutApi = "http://daglayoutservice20160320092532.azurewebsites.net/api/";
        //public string GraphLayoutApi = "http://localhost:53107/api/";
        protected ManagementUtil Util { get; private set; }
        private string _sdkName = "dotnetsdk_" + Version;
        public ManagementSDK()
        {
            Util = new ManagementUtil(_sdkName);
        }

        internal ManagementSDK(string sdkName) : this()
        {
            _sdkName = sdkName;
        }

        #region Private helpers
        private void ValidateWorkspaceSetting(WorkspaceSetting setting)
        {
            if (setting.Location == null || setting.Location == string.Empty)
                throw new ArgumentException("No Location specified.");
            if (setting.WorkspaceId == null || setting.WorkspaceId == string.Empty)
                throw new ArgumentException("No Workspace Id specified.");
            if (setting.AuthorizationToken == null || setting.AuthorizationToken == string.Empty)
                throw new ArgumentException("No Authorization Token specified.");
            SetApiUrl(setting.Location);
        }

        private void SetApiUrl(string location)
        {
            string key = string.Empty;
            switch (location.ToLower())
            {
                case "south central us":
                    key = "";
                    SetAPIEndpoints(key, ".net");
                    break;
                case "west europe":
                    key = "europewest.";
                    SetAPIEndpoints(key, ".net");
                    break;
                case "southeast asia":
                    key = "asiasoutheast.";
                    SetAPIEndpoints(key, ".net");
                    break;
                case "japan east":
                    key = "japaneast.";
                    SetAPIEndpoints(key, ".net");
                    break;
                case "germany central":
                    key = "germanycentral.";
                    SetAPIEndpoints(key, ".de");
                    break;
                case "west central us":
                    key = "uswestcentral.";
                    SetAPIEndpoints(key, ".net");
                    break;
                case "integration test":
                    key = "";
                    SetAPIEndpoints(key, "-int.net");
                    break;
                default:
                    throw new Exception("Unsupported location: " + location);
            }                                 
        }

        private void SetAPIEndpoints(string key, string postfix)
        {
            StudioApi = string.Format(_studioApiBaseURL, key, postfix);
            WebServiceApi = string.Format(_webServiceApiBaseUrl, key, postfix);
        }

        private string GetExperimentGraphFromJson(string rawJson)
        {            
            dynamic parsed = Util.Deserialize<object>(rawJson);
            string graph = Util.Serialize(parsed["Graph"]);
            return graph;
        }
        private string GetExperimentWebServiceFromJson(string rawJson)
        {         
            dynamic parsed = Util.Deserialize<object>(rawJson);
            string webService = Util.Serialize(parsed["WebService"]);
            return webService;
        }

        private string CreateSubmitExperimentRequest(Experiment exp, string rawJson, bool runExperiment, string newName, bool createNewCopy)
        {
            string graph = GetExperimentGraphFromJson(rawJson);
            string webService = GetExperimentWebServiceFromJson(rawJson);
            string req = "{" + string.Format("\"Description\":\"{0}\", \"Summary\":\"{1}\", \"IsDraft\":" + (runExperiment ? "false" : "true") +
                ", \"ParentExperimentId\":\"{2}\", \"DisableNodeUpdate\":false, \"Category\":\"user\", \"ExperimentGraph\":{3}, \"WebService\":{4}",
                            string.IsNullOrEmpty(newName)? exp.Description : newName, exp.Summary, createNewCopy ? null : exp.ParentExperimentId, graph, webService) + "}";
            return req;
        }

        private HttpWebRequest GetRdfeHttpRequest(string managementCertThumbprint, string reqUrl, string method)
        {
            HttpWebRequest httpReq = (HttpWebRequest)HttpWebRequest.Create(reqUrl);
            httpReq.Method = method;
            httpReq.ContentType = "application/json";
            httpReq.Headers.Add("x-ms-version", "2014-10-01");            
            X509Certificate2 mgmtCert = GetStoreCertificate(managementCertThumbprint);
            httpReq.ClientCertificates.Add(mgmtCert);
            return httpReq;
        }

        private static X509Certificate2 GetStoreCertificate(string thumbprint)
        {
            List<StoreLocation> locations = new List<StoreLocation>  {
                StoreLocation.CurrentUser,
                StoreLocation.LocalMachine
            };

            foreach (var location in locations)
            {
                X509Store store = new X509Store("My", location);
                try
                {
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection certificates = store.Certificates.Find(
                      X509FindType.FindByThumbprint, thumbprint, false);
                    if (certificates.Count == 1)
                    {
                        return certificates[0];
                    }
                }
                finally
                {
                    store.Close();
                }
            }
            throw new ArgumentException(string.Format(
              "A Certificate with Thumbprint '{0}' could not be located.",
              thumbprint));
        }

        public string UpdateNodesPositions(string jsonGraph, StudioGraph graph)
        {            
            dynamic experimentDag = Util.Deserialize<object>(jsonGraph);
            List<string> regularNodes = ExtractNodesFromXml(experimentDag["Graph"]["SerializedClientData"]);                         
            List<string> webServiceNodes = ExtractNodesFromXml(experimentDag["WebService"]["SerializedClientData"]);            

            StringBuilder newPositions = new StringBuilder();            
            if (regularNodes.Count > 0)
            {
                foreach (var node in graph.Nodes.Where(n => regularNodes.Contains(n.Id)))
                    newPositions.Append("<NodePosition Node='" + node.Id + "' Position='" + node.CenterX + "," + node.CenterY + "," + node.Width + "," + node.Height + "'/>");
                string oldPositions = Regex.Match(experimentDag["Graph"]["SerializedClientData"].ToString(), "<NodePositions>(.*)</NodePositions>").Groups[1].Value;
                jsonGraph = jsonGraph.Replace(oldPositions, newPositions.ToString());
            }
            
            if (webServiceNodes.Count > 0)
            {
                newPositions.Clear();
                foreach (var node in graph.Nodes.Where(n => webServiceNodes.Contains(n.Id)))
                    newPositions.Append("<NodePosition Node='" + node.Id + "' Position='" + node.CenterX + "," + node.CenterY + "," + node.Width + "," + node.Height + "'/>");
                string oldPositions = Regex.Match(experimentDag["WebService"]["SerializedClientData"].ToString(), "<NodePositions>(.*)</NodePositions>").Groups[1].Value;
                jsonGraph = jsonGraph.Replace(oldPositions, newPositions.ToString());
            }

            return jsonGraph;
        }     

        #endregion

        #region Workspace
        public WorkspaceRdfe[] GetWorkspacesFromRdfe(string managementCertThumbprint, string azureSubscriptionId)
        {
            string reqUrl = string.Format(_azMgmtApiBaseUrl, azureSubscriptionId);
            HttpWebRequest httpReq = GetRdfeHttpRequest(managementCertThumbprint, reqUrl, "GET");            

            HttpWebResponse wr = (HttpWebResponse)httpReq.GetResponse();            
            StreamReader sr = new StreamReader(wr.GetResponseStream());
            string result = sr.ReadToEnd();
            wr.Close();
            sr.Close();            
            WorkspaceRdfe[] workspaces = Util.Deserialize<WorkspaceRdfe[]>(result);
            return workspaces;
        }

        public async Task<string> CreateWorkspace(string managementCertThumbprint, string azureSubscriptionId, string workspaceName, string location, string storageAccountName, string storageAccountKey, string ownerEmail, string source)
        {        
            // initial workspace is a made-up but valid guid.
            string reqUrl = string.Format(_azMgmtApiBaseUrl + "/e582920d010646acbb0ec3183dc2243a", azureSubscriptionId);

            HttpWebRequest httpReq = GetRdfeHttpRequest(managementCertThumbprint, reqUrl, "PUT");            
            
            string payload = Util.Serialize(new
                {
                    Name = workspaceName,
                    Region = location,
                    StorageAccountName = storageAccountName,
                    StorageAccountKey = storageAccountKey,
                    OwnerId = ownerEmail,
                    ImmediateActivation = true,
                    Source = source
                });
            httpReq.ContentLength = payload.Length;
            Stream stream = httpReq.GetRequestStream();
            byte[] buffer = Encoding.UTF8.GetBytes(payload);
            stream.Write(buffer, 0, buffer.Length);

            WebResponse resp = await httpReq.GetResponseAsync();                       
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string result = sr.ReadToEnd();
                        
            dynamic d = Util.Deserialize<object>(result);
            return d["Id"];
        }

        public WorkspaceRdfe GetCreateWorkspaceStatus(string managementCertThumbprint, string azureSubscriptionId, string workspaceId, string region)
        {
            string reqUrl = string.Format(_azMgmtApiBaseUrl + "/{1}?Region={2}", azureSubscriptionId, workspaceId, HttpUtility.HtmlEncode(region));
            HttpWebRequest httpReq = GetRdfeHttpRequest(managementCertThumbprint, reqUrl, "GET");
            
            WebResponse resp = httpReq.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string result = sr.ReadToEnd();                        
            WorkspaceRdfe ws = Util.Deserialize<WorkspaceRdfe>(result);
            return ws;
        }
             

        public void RemoveWorkspace(string managementCertThumbprint, string azureSubscriptionId, string workspaceId, string region)
        {
            string reqUrl = string.Format(_azMgmtApiBaseUrl + "{1}?Region={2}", azureSubscriptionId, workspaceId, HttpUtility.HtmlEncode(region));
            HttpWebRequest httpReq = GetRdfeHttpRequest(managementCertThumbprint, reqUrl, "DELETE");
            
            WebResponse resp = httpReq.GetResponse();
            long len = resp.ContentLength;
            byte[] buffer = new byte[len];
            resp.GetResponseStream().Read(buffer, 0, (int)len);
            string result = UnicodeEncoding.ASCII.GetString(buffer);                        
        }

        public Workspace GetWorkspaceFromAmlRP(WorkspaceSetting setting)
        {
            return GetWorkspaceFromAmlRPAsync(setting).GetAwaiter().GetResult();
        }

        public async Task<Workspace> GetWorkspaceFromAmlRPAsync(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}", setting.WorkspaceId);
            var hr = await Util.HttpGet<Workspace>(setting.AuthorizationToken, queryUrl).ConfigureAwait(false);
            return hr.DeserializedPayload;
        }

        public void AddWorkspaceUsers(WorkspaceSetting setting, string emails, string role)
        {
            AddWorkspaceUsersAsync(setting, emails, role).GetAwaiter().GetResult();
        }

        public async Task AddWorkspaceUsersAsync(WorkspaceSetting setting, string emails, string role)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/invitations", setting.WorkspaceId);
            string body = "{Role: \"" + role + "\", Emails:\"" + emails + "\"}";
            HttpResult hr = await Util.HttpPost(queryUrl, body).ConfigureAwait(false);
        }

        public WorkspaceUser[] GetWorkspaceUsers(WorkspaceSetting setting)
        {
            return GetWorkspaceUsersAsync(setting).GetAwaiter().GetResult();
        }

        public async Task<WorkspaceUser[]> GetWorkspaceUsersAsync(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/users", setting.WorkspaceId);
            HttpResult<WorkspaceUserInternal[]> hr = await Util.HttpGet<WorkspaceUserInternal[]>(setting.AuthorizationToken, queryUrl).ConfigureAwait(false);
            return hr.DeserializedPayload.Select(u => new WorkspaceUser(u)).ToArray();
        }
        #endregion

        #region Dataset
        public Dataset[] GetDataset(WorkspaceSetting setting)
        {
            return GetDatasetAsync(setting).GetAwaiter().GetResult();
        }

        public async Task<Dataset[]> GetDatasetAsync(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + string.Format("workspaces/{0}/datasources", setting.WorkspaceId);
            var hr = await Util.HttpGet<Dataset[]>(setting.AuthorizationToken, query).ConfigureAwait(false);
            return hr.DeserializedPayload;
        }

        public void DeleteDataset(WorkspaceSetting setting, string datasetFamilyId)
        {
            DeleteDatasetAsync(setting, datasetFamilyId).GetAwaiter().GetResult();
        }

        public Task DeleteDatasetAsync(WorkspaceSetting setting, string datasetFamilyId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string url = StudioApi + string.Format("workspaces/{0}/datasources/family/{1}", setting.WorkspaceId, datasetFamilyId);
            return Util.HttpDelete(url);
        }

        public async Task DownloadDatasetAsync(WorkspaceSetting setting, string datasetId, string filename)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string url = StudioApi + string.Format("workspaces/{0}/datasources/{1}", setting.WorkspaceId, datasetId);
            HttpResult<Dataset> hr = await Util.HttpGet<Dataset>(setting.AuthorizationToken, url).ConfigureAwait(false);
            Dataset ds = hr.DeserializedPayload;
            string downloadUrl = ds.DownloadLocation.BaseUri + ds.DownloadLocation.Location + ds.DownloadLocation.AccessCredential;
            await DownloadFileAsync(downloadUrl, filename).ConfigureAwait(false);
        }

        public async Task DownloadFileAsync(string url, string filename)
        {                              
            HttpResult hr = await Util.HttpGet(null, url, false).ConfigureAwait(false);
            if (File.Exists(filename))
                throw new Exception(filename + " alread exists.");
            using (FileStream fs = File.Create(filename))
            {
                hr.PayloadStream.Seek(0, SeekOrigin.Begin);
                await hr.PayloadStream.CopyToAsync(fs).ConfigureAwait(false);
            }
        }

        public async Task<string> UploadResourceAsync(WorkspaceSetting setting, string fileFormat, string fileName = "")
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + string.Format("resourceuploads/workspaces/{0}/?userStorage=true&dataTypeId={1}", setting.WorkspaceId, fileFormat);
            HttpResult hr = await Util.HttpPostFile(query, fileName).ConfigureAwait(false);
            return hr.Payload;
        }

        public string UploadResource(WorkspaceSetting setting, string fileFormat, string fileName = "")
        {
            return UploadResourceAsync(setting, fileFormat, fileName).GetAwaiter().GetResult();
        }

        public async Task<string> UploadResourceInChunksAsnyc(WorkspaceSetting setting, int numOfBlocks, int blockId, string uploadId, string fileName, string fileFormat)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + string.Format("blobuploads/workspaces/{0}/?numberOfBlocks={1}&blockId={2}&uploadId={3}&dataTypeId={4}",
                setting.WorkspaceId, numOfBlocks, blockId, uploadId, fileFormat);
            HttpResult hr = await Util.HttpPostFile(query, fileName).ConfigureAwait(false);
            return hr.Payload;
        }

        public string StartDatasetSchemaGen(WorkspaceSetting setting, string dataTypeId, string uploadFileId, string datasetName, string description, string uploadFileName)
        {
            return StartDatasetSchemaGenAsync(setting, dataTypeId, uploadFileId, datasetName, description, uploadFileName).GetAwaiter().GetResult();
        }

        public async Task<string> StartDatasetSchemaGenAsync(WorkspaceSetting setting, string dataTypeId, string uploadFileId, string datasetName, string description, string uploadFileName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;            
            dynamic schemaJob = new
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
            string query = StudioApi + string.Format("workspaces/{0}/datasources", setting.WorkspaceId);
            var body = Util.Serialize(schemaJob);
            HttpResult hr = await Util.HttpPost(query, body).ConfigureAwait(false);
            string dataSourceId = hr.Payload.Replace("\"", "");
            return dataSourceId;
        }


        public string GetDatasetSchemaGenStatus(WorkspaceSetting setting, string dataSourceId)
        {
            return GetDatasetSchemaGenStatusAsync(setting, dataSourceId).GetAwaiter().GetResult();
        }

        public async Task<string> GetDatasetSchemaGenStatusAsync(WorkspaceSetting setting, string dataSourceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + string.Format("workspaces/{0}/datasources/{1}", setting.WorkspaceId, dataSourceId);
            HttpResult hr = await Util.HttpGet(query).ConfigureAwait(false);
            dynamic parsed = Util.Deserialize<object>(hr.Payload);
            string schemaJobStatus = parsed["SchemaStatus"];
            return schemaJobStatus;
        }
        #endregion

        #region Custom Module
        public string BeginParseCustomModuleJob(WorkspaceSetting setting, string moduleUploadMetadata)
        {
            return BeginParseCustomModuleJobAsync(setting, moduleUploadMetadata).GetAwaiter().GetResult();
        }

        public async Task<string> BeginParseCustomModuleJobAsync(WorkspaceSetting setting, string moduleUploadMetadata)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + string.Format("workspaces/{0}/modules/custom", setting.WorkspaceId);
            HttpResult hr = await Util.HttpPost(query, moduleUploadMetadata).ConfigureAwait(false);
            string activityId = hr.Payload.Replace("\"", "");
            return activityId;
        }

        public string GetCustomModuleBuildJobStatus(WorkspaceSetting setting, string activityGroupId)
        {
            return GetCustomModuleBuildJobStatusAsync(setting, activityGroupId).GetAwaiter().GetResult();
        }

        public async Task<string> GetCustomModuleBuildJobStatusAsync(WorkspaceSetting setting, string activityGroupId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + string.Format("workspaces/{0}/modules/custom?activityGroupId={1}", setting.WorkspaceId, activityGroupId);
            HttpResult hr = await Util.HttpGet(query).ConfigureAwait(false);
            string jobStatus = hr.Payload;
            return jobStatus;
        }

        public Module[] GetModules(WorkspaceSetting setting)
        {
            return GetModulesAsync(setting).GetAwaiter().GetResult();
        }

        public async Task<Module[]> GetModulesAsync(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            string query = StudioApi + string.Format("workspaces/{0}/modules", setting.WorkspaceId);
            HttpResult<Module[]> hr = await Util.HttpGet<Module[]>(setting.AuthorizationToken, query).ConfigureAwait(false);
            return hr.DeserializedPayload;
        }
        #endregion

        #region Experiment
        public Experiment[] GetExperiments(WorkspaceSetting setting)
        {
            return GetExperimentsAsync(setting).GetAwaiter().GetResult();
        }

        public async Task<Experiment[]> GetExperimentsAsync(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments", setting.WorkspaceId);
            HttpResult<Experiment[]> hr = await Util.HttpGet<Experiment[]>(setting.AuthorizationToken, queryUrl).ConfigureAwait(false);
            // only display user's own experiments.
            return hr.DeserializedPayload.Where(e => e.Category == "user" || string.IsNullOrEmpty(e.Category)).ToArray();
        }

        public Experiment GetExperimentById(WorkspaceSetting setting, string experimentId, out string rawJson)
        {
            var result = GetExperimentByIdAsync(setting, experimentId).GetAwaiter().GetResult();
            rawJson = result.Item2;
            return result.Item1;
        }

        public async Task<Tuple<Experiment, string>> GetExperimentByIdAsync(WorkspaceSetting setting, string experimentId)
        {
            ValidateWorkspaceSetting(setting);
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}", setting.WorkspaceId, experimentId);
            HttpResult<Experiment> hr = await Util.HttpGet<Experiment>(setting.AuthorizationToken, queryUrl).ConfigureAwait(false);
            return Tuple.Create(hr.DeserializedPayload, hr.Payload);
        }

        public void RunExperiment(WorkspaceSetting setting, Experiment exp, string rawJson)
        {
            SubmitExperiment(setting, exp, rawJson, string.Empty, false, true);
        }
        public void SaveExperiment(WorkspaceSetting setting, Experiment exp, string rawJson)
        {
            SubmitExperiment(setting, exp, rawJson, string.Empty, false, false);
        }
        public void SaveExperimentAs(WorkspaceSetting setting, Experiment exp, string rawJson, string newName)
        {
            SubmitExperiment(setting, exp, rawJson, newName, true, false);
        }

        public Task RunExperimentAsync(WorkspaceSetting setting, Experiment exp, string rawJson)
        {
            return SubmitExperimentAsync(setting, exp, rawJson, string.Empty, false, true);
        }
        public Task SaveExperimentAsync(WorkspaceSetting setting, Experiment exp, string rawJson)
        {
            return SubmitExperimentAsync(setting, exp, rawJson, string.Empty, false, false);
        }
        public Task SaveExperimentAsAsync(WorkspaceSetting setting, Experiment exp, string rawJson, string newName)
        {
            return SubmitExperimentAsync(setting, exp, rawJson, newName, true, false);
        }

        private void SubmitExperiment(WorkspaceSetting setting, Experiment exp, string rawJson, string newName, bool createNewCopy, bool run)
        {
            SubmitExperimentAsync(setting, exp, rawJson, newName, createNewCopy, run).GetAwaiter().GetResult();
        }

        private async Task SubmitExperimentAsync(WorkspaceSetting setting, Experiment exp, string rawJson, string newName, bool createNewCopy, bool run)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string body = CreateSubmitExperimentRequest(exp, rawJson, run, newName, createNewCopy);
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}", setting.WorkspaceId, createNewCopy ? string.Empty : exp.ExperimentId);
            HttpResult hr = await Util.HttpPost(queryUrl, body).ConfigureAwait(false);
        }

        public void RemoveExperimentById(WorkspaceSetting setting, string experimentId)
        {
            RemoveExperimentByIdAsync(setting, experimentId).GetAwaiter().GetResult();
        }

        public async Task RemoveExperimentByIdAsync(WorkspaceSetting setting, string experimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}?deleteAncestors=true", setting.WorkspaceId, experimentId);
            HttpResult hr = await Util.HttpDelete(queryUrl).ConfigureAwait(false);
        }

        public PackingServiceActivity PackExperiment(WorkspaceSetting setting, string experimentId)
        {
            return PackExperimentAsync(setting, experimentId).GetAwaiter().GetResult();
        }

        public async Task<PackingServiceActivity> PackExperimentAsync(WorkspaceSetting setting, string experimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/packages?api-version=2.0&experimentid={1}/&clearCredentials=true&includeAuthorId=false", setting.WorkspaceId, experimentId);
            //Console.WriteLine("Packing: POST " + queryUrl);
            HttpResult<PackingServiceActivity> hr = await Util.HttpPost<PackingServiceActivity>(queryUrl, string.Empty).ConfigureAwait(false);
            return hr.DeserializedPayload;
        }

        public PackingServiceActivity GetActivityStatus(WorkspaceSetting setting, string activityId, bool isPacking)
        {
            return GetActivityStatusAsync(setting, activityId, isPacking).GetAwaiter().GetResult();
        }

        public async Task<PackingServiceActivity> GetActivityStatusAsync(WorkspaceSetting setting, string activityId, bool isPacking)
        {
            ValidateWorkspaceSetting(setting);
            string queryUrl = StudioApi + string.Format("workspaces/{0}/packages?{1}ActivityId={2}", setting.WorkspaceId, (isPacking ? "package" : "unpack"), activityId);
            //Console.WriteLine("Getting activity: GET " + queryUrl);
            HttpResult<PackingServiceActivity> hr = await Util.HttpGet<PackingServiceActivity>(setting.AuthorizationToken, queryUrl, true).ConfigureAwait(false);
            return hr.DeserializedPayload;
        }


        public PackingServiceActivity UnpackExperiment(WorkspaceSetting setting, string packedLocation, string sourceRegion)
        {
            return UnpackExperimentAsync(setting, packedLocation, sourceRegion).GetAwaiter().GetResult();
        }

        public async Task<PackingServiceActivity> UnpackExperimentAsync(WorkspaceSetting setting, string packedLocation, string sourceRegion)
        {
            ValidateWorkspaceSetting(setting);
            string queryUrl = StudioApi + string.Format("workspaces/{0}/packages?api-version=2.0&packageUri={1}{2}", setting.WorkspaceId, HttpUtility.UrlEncode(packedLocation), "&region=" + sourceRegion.Replace(" ", string.Empty));
            //Console.WriteLine("Unpacking: PUT " + queryUrl);
            HttpResult<PackingServiceActivity> hr = await Util.HttpPut<PackingServiceActivity>(setting.AuthorizationToken, queryUrl, string.Empty).ConfigureAwait(false);
            return hr.DeserializedPayload;
        }
        
        // Note this API is NOT officially supported. It might break in the future and we won't support it if/when it happens.
        public PackingServiceActivity UnpackExperimentFromGallery(WorkspaceSetting setting, string packageUri, string galleryUrl, string entityId)
        {
            return UnpackExperimentFromGalleryAsync(setting, packageUri, galleryUrl, entityId).GetAwaiter().GetResult();
        }

        public async Task<PackingServiceActivity> UnpackExperimentFromGalleryAsync(WorkspaceSetting setting, string packageUri, string galleryUrl, string entityId)
        {
            ValidateWorkspaceSetting(setting);
            string queryUrl = StudioApi + string.Format("workspaces/{0}/packages?api-version=2.0&packageUri={1}&communityUri={2}&entityId={3}", setting.WorkspaceId, HttpUtility.UrlEncode(packageUri), HttpUtility.UrlEncode(galleryUrl), entityId);
            //Console.WriteLine("Upacking from Gallery: PUT " + queryUrl);
            HttpResult<PackingServiceActivity> hr = await Util.HttpPut<PackingServiceActivity>(setting.AuthorizationToken, queryUrl, string.Empty).ConfigureAwait(false);
            return hr.DeserializedPayload;
        }

        public string ExportAmlWebServiceDefinitionFromExperiment(WorkspaceSetting setting, string experimentId)
        {
            return ExportAmlWebServiceDefinitionFromExperimentAsync(setting, experimentId).GetAwaiter().GetResult();
        }

        public async Task<string> ExportAmlWebServiceDefinitionFromExperimentAsync(WorkspaceSetting setting, string experimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}/webservicedefinition", setting.WorkspaceId, experimentId);
            HttpResult hr = await Util.HttpGet(queryUrl).ConfigureAwait(false);
            if (hr.IsSuccess)
                return hr.Payload;
            throw new AmlRestApiException(hr);
        }

        public string AutoLayoutGraph(string jsonGraph)
        {
            return AutoLayoutGraphAsync(jsonGraph).GetAwaiter().GetResult();
        }

        public async Task<string> AutoLayoutGraphAsync(string jsonGraph)
        {
            StudioGraph sg = CreateStudioGraph(Util.Deserialize<object>(jsonGraph));
            HttpResult hr = await Util.HttpPost(GraphLayoutApi + "AutoLayout", Util.Serialize(sg)).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                sg = Util.Deserialize<StudioGraph>(hr.Payload);
                string serializedGraph = Util.Serialize(sg);
                jsonGraph = UpdateNodesPositions(jsonGraph, sg);
                return jsonGraph;
            }
            throw new AmlRestApiException(hr);
        }

        private List<string> ExtractNodesFromXml(string xml)
        {
            List<string> nodes = new List<string>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xml);
            foreach (XmlNode node in xDoc.SelectSingleNode("//NodePositions"))
            {
                string nodeId = node.Attributes["Node"].Value;
                nodes.Add(nodeId);
            }
            return nodes;
        }

        private void InsertNodesIntoGraph(dynamic dag, StudioGraph graph, string section)
        {
            string nodePositions = dag[section]["SerializedClientData"];
            List<string> nodes = ExtractNodesFromXml(nodePositions);
            foreach (var nodeId in nodes)
                graph.Nodes.Add(new StudioGraphNode
                {
                    Id = nodeId,
                    Width = 300,
                    Height = 100,
                    UserData = nodeId
                });
        }

        private StudioGraph CreateStudioGraph(dynamic dag)
        {            
            StudioGraph graph = new StudioGraph();            
            InsertNodesIntoGraph(dag, graph, "Graph");            
            InsertNodesIntoGraph(dag, graph, "WebService");
            // dataset nodes are treated differently because they don't show in the EdgesInternal section.
            Dictionary<string, string> datasetNodes = new Dictionary<string, string>();
            foreach (var moduleNode in dag["Graph"]["ModuleNodes"])
            {
                string nodeId = moduleNode["Id"];
                foreach (var inputPort in moduleNode["InputPortsInternal"])
                    if (inputPort["DataSourceId"] != null && !datasetNodes.Keys.Contains(nodeId)) // this is a dataset node
                        datasetNodes.Add(nodeId, inputPort["DataSourceId"].ToString());
            }

            // normal edges
            foreach (dynamic edge in dag["Graph"]["EdgesInternal"])
            {
                string sourceOutputPort = edge["SourceOutputPortId"].ToString();
                string destInputPort = edge["DestinationInputPortId"].ToString();
                string sourceNode = (sourceOutputPort.Split(':')[0]);
                string destNode = (destInputPort.Split(':')[0]);
                graph.Edges.Add(new StudioGraphEdge
                {
                    DestinationNode = graph.Nodes.Single(n => n.Id == destNode),
                    SourceNode = graph.Nodes.Single(n => n.Id == sourceNode)
                });
            }

            // dataset edges
            foreach (string nodeId in datasetNodes.Keys)
                graph.Edges.Add(new StudioGraphEdge {
                    DestinationNode = graph.Nodes.Single(n => n.Id == nodeId),
                    SourceNode = graph.Nodes.Single(n => n.Id == datasetNodes[nodeId])
                    }
                );

            if (dag["WebService"] != null)
            {
                // web service input edges
                if (dag["WebService"]["Inputs"] != null)
                    foreach (var webSvcInput in dag["WebService"]["Inputs"])
                    {
                        if (webSvcInput["PortId"] != null)
                        {
                            string webSvcModuleId = webSvcInput["Id"].ToString();
                            string connectedModuleId = webSvcInput["PortId"].ToString().Split(':')[0];
                            graph.Edges.Add(new StudioGraphEdge
                            {
                                DestinationNode = graph.Nodes.Single(n => n.Id == connectedModuleId),
                                SourceNode = graph.Nodes.Single(n => n.Id == webSvcModuleId)
                            });                            
                        }
                    }

                // web service output edges
                if (dag["WebService"]["Outputs"] != null)
                    foreach (var webSvcOutput in dag["WebService"]["Outputs"])
                    {
                        if (webSvcOutput["PortId"] != null)
                        {
                            string webSvcModuleId = webSvcOutput["Id"].ToString();
                            string connectedModuleId = webSvcOutput["PortId"].ToString().Split(':')[0];
                            graph.Edges.Add(new StudioGraphEdge
                            {
                                DestinationNode = graph.Nodes.Single(n => n.Id == webSvcModuleId),
                                SourceNode = graph.Nodes.Single(n => n.Id == connectedModuleId)
                            });
                        }
                    }
            }            
            return graph;
        }

        
        #endregion

        #region User Assets
        public UserAsset[] GetTrainedModels(WorkspaceSetting setting)
        {
            return GetTrainedModelsAsync(setting).GetAwaiter().GetResult();
        }

        public async Task<UserAsset[]> GetTrainedModelsAsync(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/trainedmodels", setting.WorkspaceId);
            HttpResult hr = await Util.HttpGet(queryUrl).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                UserAsset[] tms = Util.Deserialize<UserAsset[]>(hr.Payload);
                return tms;
            }
            throw new AmlRestApiException(hr);
        }

        public UserAsset[] GetTransforms(WorkspaceSetting setting)
        {
            return GetTransformsAsync(setting).GetAwaiter().GetResult();
        }

        public async Task<UserAsset[]> GetTransformsAsync(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/transformmodules", setting.WorkspaceId);
            HttpResult hr = await Util.HttpGet(queryUrl).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                UserAsset[] tms = Util.Deserialize<UserAsset[]>(hr.Payload);
                return tms;
            }
            throw new AmlRestApiException(hr);
        }

        public void PromoteUserAsset(WorkspaceSetting setting, string experimentId, string nodeId, string nodeOutputName, string assetName, string assetDescription, UserAssetType assetType, string familyId)
        {
            PromoteUserAssetAsync(setting, experimentId, nodeId, nodeOutputName, assetName, assetDescription, assetType, familyId).GetAwaiter().GetResult();
        }

        public async Task PromoteUserAssetAsync(WorkspaceSetting setting, string experimentId, string nodeId, string nodeOutputName, string assetName, string assetDescription, UserAssetType assetType, string familyId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;

            string postPayloadInJson = string.Empty;

            switch (assetType)
            {
                case UserAssetType.Transform:
                    var transformPayload = new
                    {
                        ExperimentId = experimentId,
                        ModuleNodeId = nodeId,
                        OutputName = nodeOutputName,
                        Transform = new
                        {
                            Name = assetName,
                            DataTypeId = "iTransformDotNet",
                            Description = assetDescription,
                            SourceOrigin = "FromOutputPromotion",
                            FamilyId = familyId
                        }
                    };
                    postPayloadInJson = Util.Serialize(transformPayload);
                    break;
                case UserAssetType.TrainedModel:
                    var trainedModelPayload = new
                    {
                        ExperimentId = experimentId,
                        ModuleNodeId = nodeId,
                        OutputName = nodeOutputName,
                        TrainedModel = new
                        {
                            Name = assetName,
                            DataTypeId = "iLearnerDotNet",
                            Description = assetDescription,
                            SourceOrigin = "FromOutputPromotion",
                            FamilyId = familyId
                        }
                    };
                    postPayloadInJson = Util.Serialize(trainedModelPayload);
                    break;
                case UserAssetType.Dataset:
                    var datasetPayload = new
                    {
                        ExperimentId = experimentId,
                        ModuleNodeId = nodeId,
                        OutputName = nodeOutputName,
                        DataSource = new
                        {
                            Name = assetName,
                            DataTypeId = "Dataset",
                            Description = assetDescription,
                            SourceOrigin = "FromOutputPromotion",
                            FamilyId = familyId
                        }
                    };
                    postPayloadInJson = Util.Serialize(datasetPayload);
                    break;
            }

            string queryUrl = StudioApi + string.Format("workspaces/{0}/{1}", setting.WorkspaceId, assetType == UserAssetType.Transform ? "transformmodules" : (assetType == UserAssetType.TrainedModel ? "trainedmodels" : "datasources"));
            HttpResult hr = await Util.HttpPost(queryUrl, postPayloadInJson).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                return;
            }
            throw new AmlRestApiException(hr);
        }
        #endregion

        #region Web Service

        public WebService[] GetWebServicesInWorkspace(WorkspaceSetting setting)
        {
            return GetWebServicesInWorkspaceAsync(setting).GetAwaiter().GetResult();
        }

        public async Task<WebService[]> GetWebServicesInWorkspaceAsync(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices", setting.WorkspaceId);
            HttpResult hr = await Util.HttpGet(queryUrl).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                WebService[] wss = Util.Deserialize<WebService[]>(hr.Payload);
                return wss;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public WebService GetWebServicesById(WorkspaceSetting setting, string webServiceId)
        {
            return GetWebServicesByIdAsync(setting, webServiceId).GetAwaiter().GetResult();
        }

        public async Task<WebService> GetWebServicesByIdAsync(WorkspaceSetting setting, string webServiceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}", setting.WorkspaceId, webServiceId);
            HttpResult hr = await Util.HttpGet(queryUrl).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                WebService ws = Util.Deserialize<WebService>(hr.Payload);
                return ws;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public WebServiceCreationStatus DeployWebServiceFromPredictiveExperiment(WorkspaceSetting setting, string predictiveExperimentId, bool updateExistingWebServiceDefaultEndpoint)
        {
            return DeployWebServiceFromPredictiveExperimentAsync(setting, predictiveExperimentId, updateExistingWebServiceDefaultEndpoint).GetAwaiter().GetResult();
        }

        public async Task<WebServiceCreationStatus> DeployWebServiceFromPredictiveExperimentAsync(WorkspaceSetting setting, string predictiveExperimentId, bool updateExistingWebServiceDefaultEndpoint)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}/webservice?generateNewPortNames=false{2}", setting.WorkspaceId, predictiveExperimentId, updateExistingWebServiceDefaultEndpoint ? "&updateExistingWebService=true" : "");
            HttpResult hr = await Util.HttpPost(queryUrl, string.Empty).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                WebServiceCreationStatus status = Util.Deserialize<WebServiceCreationStatus>(hr.Payload);
                return status;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public WebServiceCreationStatus GetWebServiceCreationStatus(WorkspaceSetting setting, string activityId)
        {
            return GetWebServiceCreationStatusAsync(setting, activityId).GetAwaiter().GetResult();
        }

        public async Task<WebServiceCreationStatus> GetWebServiceCreationStatusAsync(WorkspaceSetting setting, string activityId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}/webservice", setting.WorkspaceId, activityId);
            HttpResult hr = await Util.HttpGet(queryUrl).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                WebServiceCreationStatus status = Util.Deserialize<WebServiceCreationStatus>(hr.Payload);
                return status;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public void RemoveWebServiceById(WorkspaceSetting setting, string webServiceId)
        {
            RemoveWebServiceByIdAsync(setting, webServiceId).GetAwaiter().GetResult();
        }

        public async Task RemoveWebServiceByIdAsync(WorkspaceSetting setting, string webServiceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}", setting.WorkspaceId, webServiceId);
            HttpResult hr = await Util.HttpDelete(queryUrl).ConfigureAwait(false);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }
        #endregion

        #region Web Service Endpoint
        public WebServiceEndPoint[] GetWebServiceEndpoints(WorkspaceSetting setting, string webServiceId)
        {
            return GetWebServiceEndpointsAsync(setting, webServiceId).GetAwaiter().GetResult();
        }

        public async Task<WebServiceEndPoint[]> GetWebServiceEndpointsAsync(WorkspaceSetting setting, string webServiceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints", setting.WorkspaceId, webServiceId);
            HttpResult hr = await Util.HttpGet(queryUrl).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                WebServiceEndPoint[] weps = Util.Deserialize<WebServiceEndPoint[]>(hr.Payload);
                return weps;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public WebServiceEndPoint GetWebServiceEndpointByName(WorkspaceSetting setting, string webServiceId, string epName)
        {
            return GetWebServiceEndpointByNameAsync(setting, webServiceId, epName).GetAwaiter().GetResult();
        }

        public async Task<WebServiceEndPoint> GetWebServiceEndpointByNameAsync(WorkspaceSetting setting, string webServiceId, string epName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, webServiceId, epName);
            HttpResult hr = await Util.HttpGet(queryUrl).ConfigureAwait(false);
            if (hr.IsSuccess)
            {
                WebServiceEndPoint ep = Util.Deserialize<WebServiceEndPoint>(hr.Payload);
                return ep;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public void AddWebServiceEndpoint(WorkspaceSetting setting, AddWebServiceEndpointRequest req)
        {
            AddWebServiceEndpointAsync(setting, req).GetAwaiter().GetResult();
        }

        public async Task AddWebServiceEndpointAsync(WorkspaceSetting setting, AddWebServiceEndpointRequest req)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, req.WebServiceId, req.EndpointName);
            string body = Util.Serialize(req);
            HttpResult hr = await Util.HttpPut(queryUrl, body).ConfigureAwait(false);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public bool RefreshWebServiceEndPoint(WorkspaceSetting setting, string webServiceId, string endpointName, bool overwriteResources)
        {
            return RefreshWebServiceEndPointAsync(setting, webServiceId, endpointName, overwriteResources).GetAwaiter().GetResult();
        }

        public async Task<bool> RefreshWebServiceEndPointAsync(WorkspaceSetting setting, string webServiceId, string endpointName, bool overwriteResources)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}/refresh", setting.WorkspaceId, webServiceId, endpointName);
            string body = "{\"OverwriteResources\": \"" + overwriteResources.ToString() + "\"}";
            HttpResult hr = await Util.HttpPost(query, body).ConfigureAwait(false);
            if (hr.StatusCode == 304) // no change detected so no update happened.
                return false;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            return true;
        }

        public void PatchWebServiceEndpoint(WorkspaceSetting setting, string webServiceId, string endpointName, dynamic patchReq)
        {
            PatchWebServiceEndpointAsync(setting, webServiceId, endpointName, patchReq).GetAwaiter().GetResult();
        }

        public async Task PatchWebServiceEndpointAsync(WorkspaceSetting setting, string webServiceId, string endpointName, dynamic patchReq)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string body = Util.Serialize(patchReq);
            string url = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, webServiceId, endpointName);
            HttpResult hr = await Util.HttpPatch(url, body).ConfigureAwait(false);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public void RemoveWebServiceEndpoint(WorkspaceSetting setting, string webServiceId, string endpointName)
        {
            RemoveWebServiceEndpointAsync(setting, webServiceId, endpointName).GetAwaiter().GetResult();
        }

        public async Task RemoveWebServiceEndpointAsync(WorkspaceSetting setting, string webServiceId, string endpointName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, webServiceId, endpointName);
            HttpResult hr = await Util.HttpDelete(queryUrl).ConfigureAwait(false);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }
        #endregion

        #region Invoke Web Service Endpoint
        public string InvokeRRS(string postRequestUrl, string apiKey, string input)
        {
            return InvokeRRSAsync(postRequestUrl, apiKey, input).GetAwaiter().GetResult();
        }

        public async Task<string> InvokeRRSAsync(string postRequestUrl, string apiKey, string input)
        {
            Util.AuthorizationToken = apiKey;
            HttpResult hr = await Util.HttpPost(postRequestUrl, input).ConfigureAwait(false);
            if (hr.IsSuccess)
                return hr.Payload;
            else
                throw new AmlRestApiException(hr);
        }

        public string SubmitBESJob(string submitJobRequestUrl, string apiKey, string jobConfig)
        {
            return SubmitBESJobAsync(submitJobRequestUrl, apiKey, jobConfig).GetAwaiter().GetResult();
        }

        public async Task<string> SubmitBESJobAsync(string submitJobRequestUrl, string apiKey, string jobConfig)
        {
            Util.AuthorizationToken = apiKey;
            HttpResult hr = await Util.HttpPost(submitJobRequestUrl, jobConfig).ConfigureAwait(false);
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);

            string jobId = hr.Payload.Replace("\"", "");
            return jobId;
        }

        public void StartBESJob(string submitJobRequestUrl, string apiKey, string jobId)
        {
            StartBESJobAsync(submitJobRequestUrl, apiKey, jobId).GetAwaiter().GetResult();
        }

        public async Task StartBESJobAsync(string submitJobRequestUrl, string apiKey, string jobId)
        {
            Util.AuthorizationToken = apiKey;
            string startJobApiLocation = submitJobRequestUrl.Replace("jobs?api-version=2.0", "jobs/" + jobId + "/start?api-version=2.0");
            HttpResult hr = await Util.HttpPost(startJobApiLocation, string.Empty).ConfigureAwait(false);
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);
        }

        public string GetBESJobStatus(string submitJobRequestUrl, string apiKey, string jobId, out string results)
        {
            var result = GetBESJobStatusAsync(submitJobRequestUrl, apiKey, jobId).GetAwaiter().GetResult();
            results = result.Item2;
            return result.Item1;
        }

        public async Task<Tuple<string, string>> GetBESJobStatusAsync(string submitJobRequestUrl, string apiKey, string jobId)
        {
            Util.AuthorizationToken = apiKey;
            string getJobStatusApiLocation = submitJobRequestUrl.Replace("jobs?api-version=2.0", "jobs/" + jobId + "?api-version=2.0");
            HttpResult hr = await Util.HttpGet(getJobStatusApiLocation).ConfigureAwait(false);
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);
            dynamic parsed = Util.Deserialize<object>(hr.Payload);
            string jobStatus = parsed["StatusCode"];
            var results = hr.Payload;
            return Tuple.Create(jobStatus, results);
        }
        #endregion
    }
}
