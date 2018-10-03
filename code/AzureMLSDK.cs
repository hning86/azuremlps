﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;

namespace AzureMLPS
{    
    public class ManagementSDK
    {
        public const string Version = "0.3.8";        
        private JavaScriptSerializer jss;
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
            jss = new JavaScriptSerializer();
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
            dynamic parsed = jss.Deserialize<object>(rawJson);
            string graph = jss.Serialize(parsed["Graph"]);
            return graph;
        }
        private string GetExperimentWebServiceFromJson(string rawJson)
        {         
            dynamic parsed = jss.Deserialize<object>(rawJson);
            string webService = jss.Serialize(parsed["WebService"]);
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
            throw new ArgumentException($"A Certificate with Thumbprint '{thumbprint}' could not be located.");
        }

        public string UpdateNodesPositions(string jsonGraph, StudioGraph graph)
        {            
            dynamic experimentDag = jss.Deserialize<object>(jsonGraph);
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
            WorkspaceRdfe[] workspaces = jss.Deserialize<WorkspaceRdfe[]>(result);
            return workspaces;
        }

        public async Task<string> CreateWorkspace(string managementCertThumbprint, string azureSubscriptionId, string workspaceName, string location, string storageAccountName, string storageAccountKey, string ownerEmail, string source)
        {        
            // initial workspace is a made-up but valid guid.
            string reqUrl = string.Format(_azMgmtApiBaseUrl + "/e582920d010646acbb0ec3183dc2243a", azureSubscriptionId);

            HttpWebRequest httpReq = GetRdfeHttpRequest(managementCertThumbprint, reqUrl, "PUT");            
            
            string payload = jss.Serialize(new
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
                        
            dynamic d = jss.Deserialize<object>(result);
            return d["Id"];
        }

        public WorkspaceRdfe GetCreateWorkspaceStatus(string managementCertThumbprint, string azureSubscriptionId, string workspaceId, string region)
        {
            string reqUrl = string.Format(_azMgmtApiBaseUrl + "/{1}?Region={2}", azureSubscriptionId, workspaceId, HttpUtility.HtmlEncode(region));
            HttpWebRequest httpReq = GetRdfeHttpRequest(managementCertThumbprint, reqUrl, "GET");
            
            WebResponse resp = httpReq.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string result = sr.ReadToEnd();                        
            WorkspaceRdfe ws = jss.Deserialize<WorkspaceRdfe>(result);
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
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + $"workspaces/{setting.WorkspaceId}";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                Workspace ws = jss.Deserialize<Workspace>(hr.Payload);
                return ws;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public void AddWorkspaceUsers(WorkspaceSetting setting, string emails, string role)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + $"workspaces/{setting.WorkspaceId}/invitations";
            string body = "{Role: \"" + role + "\", Emails:\"" + emails + "\"}";
            HttpResult hr = Util.HttpPost(queryUrl, body).Result;
            if (hr.IsSuccess)
            {
                string p = hr.Payload;
                return;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public WorkspaceUser[] GetWorkspaceUsers(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + $"workspaces/{setting.WorkspaceId}/users";            
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {                
                WorkspaceUserInternal[] usersInternal =  jss.Deserialize<WorkspaceUserInternal[]>(hr.Payload);
                List<WorkspaceUser> users = new List<WorkspaceUser>();
                foreach (WorkspaceUserInternal u in usersInternal)
                    users.Add(new WorkspaceUser(u));
                return users.ToArray();
            }
            else
                throw new AmlRestApiException(hr);
        }
        #endregion

        #region Dataset
        public Dataset[] GetDataset(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/datasources";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);            
            Dataset[] datasets = jss.Deserialize<Dataset[]>(hr.Payload);
            return datasets;
        }

        public void DeleteDataset(WorkspaceSetting setting, string datasetFamilyId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string url = StudioApi + $"workspaces/{setting.WorkspaceId}/datasources/family/{datasetFamilyId}";
            HttpResult hr = Util.HttpDelete(url).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public async Task DownloadDatasetAsync(WorkspaceSetting setting, string datasetId, string filename)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string url = StudioApi + $"workspaces/{setting.WorkspaceId}/datasources/{datasetId}";
            HttpResult hr = Util.HttpGet(url).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);            
            Dataset ds = jss.Deserialize<Dataset>(hr.Payload);
            string downloadUrl = ds.DownloadLocation.BaseUri + ds.DownloadLocation.Location + ds.DownloadLocation.AccessCredential;
            await DownloadFileAsync(downloadUrl, filename);
        }

        public async Task DownloadFileAsync(string url, string filename)
        {                              
            HttpResult hr = await Util.HttpGet(url, false);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            if (File.Exists(filename))
                throw new Exception(filename + " alread exists.");
            FileStream fs = File.Create(filename);
            hr.PayloadStream.Seek(0, SeekOrigin.Begin);
            hr.PayloadStream.CopyTo(fs);
            fs.Close();
        }
        public async Task<string> UploadResourceAsnyc(WorkspaceSetting setting, string fileFormat, string fileName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi +
                           $"resourceuploads/workspaces/{setting.WorkspaceId}/?userStorage=true&dataTypeId={fileFormat}";
            HttpResult hr = await Util.HttpPostFile(query, fileName);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            return hr.Payload;
        }

        public string UploadResource(WorkspaceSetting setting, string fileFormat)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi +
                           $"resourceuploads/workspaces/{setting.WorkspaceId}/?userStorage=true&dataTypeId={fileFormat}";
            HttpResult hr = Util.HttpPost(query, string.Empty).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            return hr.Payload;
        }

        public async Task<string> UploadResourceInChunksAsnyc(WorkspaceSetting setting, int numOfBlocks, int blockId, string uploadId, string fileName, string fileFormat)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi +
                           $"blobuploads/workspaces/{setting.WorkspaceId}/?numberOfBlocks={numOfBlocks}&blockId={blockId}&uploadId={uploadId}&dataTypeId={fileFormat}";
            HttpResult hr = await Util.HttpPostFile(query, fileName);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            return hr.Payload;
        }
        public string StartDatasetSchemaGen(WorkspaceSetting setting, string dataTypeId, string uploadFileId, string datasetName, string description, string uploadFileName)
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
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/datasources";
            HttpResult hr = Util.HttpPost(query, jss.Serialize(schemaJob)).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            string dataSourceId = hr.Payload.Replace("\"", "");
            return dataSourceId;
        }


        public string GetDatasetSchemaGenStatus(WorkspaceSetting setting, string dataSourceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;            
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/datasources/{dataSourceId}";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            dynamic parsed = jss.Deserialize<object>(hr.Payload);
            string schemaJobStatus = parsed["SchemaStatus"];
            return schemaJobStatus;
        }
        #endregion

        #region Custom Module
        public string BeginParseCustomModuleJob(WorkspaceSetting setting, string moduleUploadMetadata)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/modules/custom";
            HttpResult hr = Util.HttpPost(query, moduleUploadMetadata).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            string activityId = hr.Payload.Replace("\"", "");
            return activityId;            
        }

        public string GetCustomModuleBuildJobStatus(WorkspaceSetting setting, string activityGroupId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi +
                           $"workspaces/{setting.WorkspaceId}/modules/custom?activityGroupId={activityGroupId}";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            string jobStatus = hr.Payload;
            return jobStatus;
        }

        public Module[] GetModules(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/modules";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);            
            Module[] modules = jss.Deserialize<Module[]>(hr.Payload);
            return modules;
        }
        #endregion

        #region Experiment
        public Experiment[] GetExperiments(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + $"workspaces/{setting.WorkspaceId}/experiments";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                Experiment[] exps = jss.Deserialize<Experiment[]>(hr.Payload);
                // only display user's own experiments.
                exps = exps.Where(e => e.Category == "user" || string.IsNullOrEmpty(e.Category)).ToArray();
                return exps;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public Experiment GetExperimentById(WorkspaceSetting setting, string experimentId, out string rawJson)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            rawJson = string.Empty;
            string queryUrl = StudioApi + $"workspaces/{setting.WorkspaceId}/experiments/{experimentId}";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                rawJson = hr.Payload;
                Experiment exp = jss.Deserialize<Experiment>(hr.Payload);                
                return exp;
            }
            else
                throw new AmlRestApiException(hr);
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

        private void SubmitExperiment(WorkspaceSetting setting, Experiment exp, string rawJson, string newName, bool createNewCopy, bool run)
        {
            ValidateWorkspaceSetting(setting);  
            Util.AuthorizationToken = setting.AuthorizationToken;
            string body = CreateSubmitExperimentRequest(exp, rawJson, run, newName, createNewCopy);
            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/experiments/{(createNewCopy ? string.Empty : exp.ExperimentId)}";
            HttpResult hr = Util.HttpPost(queryUrl, body).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }
        public void RemoveExperimentById(WorkspaceSetting setting, string ExperimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/experiments/{ExperimentId}?deleteAncestors=true";
            HttpResult hr = Util.HttpDelete(queryUrl).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public PackingServiceActivity PackExperiment(WorkspaceSetting setting, string experimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/packages?api-version=2.0&experimentid={experimentId}/&clearCredentials=true&includeAuthorId=false";
            //Console.WriteLine("Packing: POST " + queryUrl);
            HttpResult hr = Util.HttpPost(queryUrl, string.Empty).Result;
            if (hr.IsSuccess)
            {                
                PackingServiceActivity activity = jss.Deserialize<PackingServiceActivity>(hr.Payload);
                return activity;
            }
            throw new AmlRestApiException(hr);
        }

        public PackingServiceActivity GetActivityStatus(WorkspaceSetting setting, string activityId, bool isPacking)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/packages?{(isPacking ? "package" : "unpack")}ActivityId={activityId}";
            //Console.WriteLine("Getting activity: GET " + queryUrl);
            HttpResult hr = Util.HttpGet(queryUrl, true).Result;

            if (hr.IsSuccess)
            {                
                PackingServiceActivity activity = jss.Deserialize<PackingServiceActivity>(hr.Payload);
                return activity;
            }
            else
                throw new AmlRestApiException(hr);
        }


        public PackingServiceActivity UnpackExperiment(WorkspaceSetting setting, string packedLocation, string sourceRegion)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/packages?api-version=2.0&packageUri={HttpUtility.UrlEncode(packedLocation)}{"&region=" + sourceRegion.Replace(" ", string.Empty)}";
            //Console.WriteLine("Unpacking: PUT " + queryUrl);
            HttpResult hr = Util.HttpPut(queryUrl, string.Empty).Result;
            if (hr.IsSuccess)
            {                
                PackingServiceActivity activity = jss.Deserialize<PackingServiceActivity>(hr.Payload);
                return activity;
            }
            throw new AmlRestApiException(hr);
        }

        
        // Note this API is NOT officially supported. It might break in the future and we won't support it if/when it happens.
        public PackingServiceActivity UnpackExperimentFromGallery(WorkspaceSetting setting, string packageUri, string galleryUrl, string entityId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/packages?api-version=2.0&packageUri={HttpUtility.UrlEncode(packageUri)}&communityUri={HttpUtility.UrlEncode(galleryUrl)}&entityId={entityId}";
            //Console.WriteLine("Upacking from Gallery: PUT " + queryUrl);
            HttpResult hr = Util.HttpPut(setting.AuthorizationToken, queryUrl, string.Empty).Result;
            if (hr.IsSuccess)
            {                
                PackingServiceActivity activity = jss.Deserialize<PackingServiceActivity>(hr.Payload);
                return activity;
            }
            throw new AmlRestApiException(hr);
        }

        public string ExportAmlWebServiceDefinitionFromExperiment(WorkspaceSetting setting, string experimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/experiments/{experimentId}/webservicedefinition";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
                return hr.Payload;
            throw new AmlRestApiException(hr);
        }
            
        public string AutoLayoutGraph(string jsonGraph)
        {            
            StudioGraph sg = CreateStudioGraph(jss.Deserialize<object>(jsonGraph));            
            HttpResult hr = Util.HttpPost(GraphLayoutApi + "AutoLayout", jss.Serialize(sg)).Result;
            if (hr.IsSuccess)
            {
                sg = jss.Deserialize<StudioGraph>(hr.Payload);
                string serializedGraph = jss.Serialize(sg);                
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
        public UserAsset[] GetTrainedModels (WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + $"workspaces/{setting.WorkspaceId}/trainedmodels";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                UserAsset[] tms = jss.Deserialize<UserAsset[]>(hr.Payload);
                return tms;
            }
            throw new AmlRestApiException(hr);
        }

        public UserAsset[] GetTransforms(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + $"workspaces/{setting.WorkspaceId}/transformmodules";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                UserAsset[] tms = jss.Deserialize<UserAsset[]>(hr.Payload);
                return tms;
            }
            throw new AmlRestApiException(hr);
        }

        public void PromoteUserAsset(WorkspaceSetting setting, string experimentId, string nodeId, string nodeOutputName, string assetName, string assetDescription, UserAssetType assetType, string familyId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;

            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/{(assetType == UserAssetType.Transform ? "transformmodules" : (assetType == UserAssetType.TrainedModel ? "trainedmodels" : "datasources"))}";
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
                    postPayloadInJson = jss.Serialize(transformPayload);
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
                    postPayloadInJson = jss.Serialize(trainedModelPayload);
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
                    postPayloadInJson = jss.Serialize(datasetPayload);
                    break;
            }
            HttpResult hr = Util.HttpPost(queryUrl, postPayloadInJson).Result;
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
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + $"workspaces/{setting.WorkspaceId}/webservices";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {                
                WebService[] wss = jss.Deserialize<WebService[]>(hr.Payload);
                return wss;                
            }
            else
                throw new AmlRestApiException(hr);
        }
        public WebService GetWebServicesById(WorkspaceSetting setting, string webServiceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + $"workspaces/{setting.WorkspaceId}/webservices/{webServiceId}";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                WebService ws = jss.Deserialize<WebService>(hr.Payload);                
                return ws;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public WebServiceCreationStatus DeployWebServiceFromPredictiveExperiment(WorkspaceSetting setting, string predictiveExperimentId, bool updateExistingWebServiceDefaultEndpoint)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi +
                              $"workspaces/{setting.WorkspaceId}/experiments/{predictiveExperimentId}/webservice?generateNewPortNames=false{(updateExistingWebServiceDefaultEndpoint ? "&updateExistingWebService=true" : "")}";            
            HttpResult hr = Util.HttpPost(queryUrl, string.Empty).Result;
            if (hr.IsSuccess)
            {             
                WebServiceCreationStatus status = jss.Deserialize<WebServiceCreationStatus>(hr.Payload);
                return status;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public WebServiceCreationStatus GetWebServiceCreationStatus(WorkspaceSetting setting, string activityId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + $"workspaces/{setting.WorkspaceId}/experiments/{activityId}/webservice";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {                
                WebServiceCreationStatus status = jss.Deserialize<WebServiceCreationStatus>(hr.Payload);
                return status;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public void RemoveWebServiceById(WorkspaceSetting setting, string webServiceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + $"workspaces/{setting.WorkspaceId}/webservices/{webServiceId}";
            HttpResult hr = Util.HttpDelete(queryUrl).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }
        #endregion

        #region Web Service Endpoint
        public WebServiceEndPoint[] GetWebServiceEndpoints(WorkspaceSetting setting, string webServiceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + $"workspaces/{setting.WorkspaceId}/webservices/{webServiceId}/endpoints";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                WebServiceEndPoint[] weps = jss.Deserialize<WebServiceEndPoint[]>(hr.Payload);                
                return weps;
            }
            else
                throw new AmlRestApiException(hr);
        }
        public WebServiceEndPoint GetWebServiceEndpointByName(WorkspaceSetting setting, string webServiceId, string epName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi +
                              $"workspaces/{setting.WorkspaceId}/webservices/{webServiceId}/endpoints/{epName}";
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                WebServiceEndPoint ep = jss.Deserialize<WebServiceEndPoint>(hr.Payload);                
                return ep;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public void AddWebServiceEndpoint(WorkspaceSetting setting, AddWebServiceEndpointRequest req)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi +
                              $"workspaces/{setting.WorkspaceId}/webservices/{req.WebServiceId}/endpoints/{req.EndpointName}";            
            string body = jss.Serialize(req);
            HttpResult hr = Util.HttpPut(queryUrl, body).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }
        public bool RefreshWebServiceEndPoint(WorkspaceSetting setting, string webServiceId, string endpointName, bool overwriteResources)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = WebServiceApi +
                           $"workspaces/{setting.WorkspaceId}/webservices/{webServiceId}/endpoints/{endpointName}/refresh";
            string body = "{\"OverwriteResources\": \"" + overwriteResources.ToString() + "\"}";
            HttpResult hr = Util.HttpPost(query, body).Result;
            if (hr.StatusCode == 304) // no change detected so no update happened.
                return false;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            return true;
        }

        public void PatchWebServiceEndpoint(WorkspaceSetting setting, string webServiceId, string endpointName, dynamic patchReq)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;            
            string body = jss.Serialize(patchReq);
            string url = WebServiceApi +
                         $"workspaces/{setting.WorkspaceId}/webservices/{webServiceId}/endpoints/{endpointName}";
            HttpResult hr = Util.HttpPatch(url, body).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public void RemoveWebServiceEndpoint(WorkspaceSetting setting, string webServiceId, string endpointName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi +
                              $"workspaces/{setting.WorkspaceId}/webservices/{webServiceId}/endpoints/{endpointName}";
            HttpResult hr = Util.HttpDelete(queryUrl).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }
        #endregion

        #region Invoke Web Service Endpoint
        public string InvokeRRS(string PostRequestUrl, string apiKey, string input)
        {
            Util.AuthorizationToken = apiKey;
            HttpResult hr = Util.HttpPost(PostRequestUrl, input).Result;
            if (hr.IsSuccess)
                return hr.Payload;
            else
                throw new AmlRestApiException(hr);
        }

        public string SubmitBESJob(string submitJobRequestUrl, string apiKey, string jobConfig)
        {
            Util.AuthorizationToken = apiKey;
            HttpResult hr = Util.HttpPost(submitJobRequestUrl, jobConfig).Result;
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);
            
            string jobId = hr.Payload.Replace("\"", "");
            return jobId;
        }

        public void StartBESJob(string submitJobRequestUrl, string apiKey, string jobId)
        {
            Util.AuthorizationToken = apiKey;
            string startJobApiLocation = submitJobRequestUrl.Replace("jobs?api-version=2.0", "jobs/" + jobId + "/start?api-version=2.0");
            HttpResult hr = Util.HttpPost(startJobApiLocation, string.Empty).Result;
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);

        }

        public string GetBESJobStatus(string submitJobRequestUrl, string apiKey, string jobId, out string results)
        {
            Util.AuthorizationToken = apiKey;
            string getJobStatusApiLocation = submitJobRequestUrl.Replace("jobs?api-version=2.0", "jobs/" + jobId + "?api-version=2.0");            
            HttpResult hr = Util.HttpGet(getJobStatusApiLocation).Result;
            if (!hr.IsSuccess)
                throw new Exception(hr.Payload);
            dynamic parsed = jss.Deserialize<object>(hr.Payload);
            string jobStatus = parsed["StatusCode"];
            results = hr.Payload;            
            return jobStatus;
        }
        #endregion

        #region Annotation
        public Annotation[] GetAnnotation(WorkspaceSetting setting, string experimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/experiments/{experimentId}/annotations";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            Annotation[] annotations = jss.Deserialize<Annotation[]>(hr.Payload);
            return annotations;
        }
        #endregion

        #region ProjectContainer
        public ProjectContainer[] GetProjectContainer(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/projectcontainers";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            ProjectContainer[] projectContainers = jss.Deserialize<ProjectContainer[]>(hr.Payload);
            return projectContainers;
        }
        #endregion

        #region Notebook
        public Notebook[] GetNotebook(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/notebooks";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            Notebook[] notebooks = jss.Deserialize<Notebook[]>(hr.Payload);
            return notebooks;
        }

        public string[] GetNotebookAttachmentName(WorkspaceSetting setting, string familyId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/notebooks/family/{familyId}/attachments";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            string[] attachments = jss.Deserialize<string[]>(hr.Payload);
            return attachments;
        }

        public NotebookSession GetNotebookSession(WorkspaceSetting setting, string familyId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/notebooks/family/{familyId}/session";
            HttpResult hr = Util.HttpPost(query, string.Empty).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            NotebookSession session = jss.Deserialize<NotebookSession>(hr.Payload);
            return session;
        }
        #endregion

        #region Gateway
        public Gateway[] GetGateway(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + $"workspaces/{setting.WorkspaceId}/gateways";
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            Gateway[] gateways = jss.Deserialize<Gateway[]>(hr.Payload);
            return gateways;
        }
        #endregion
    }
}
