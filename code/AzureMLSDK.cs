using AzureML.Contract;
using AzureML.PowerShell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace AzureML
{
    public class ManagementSDK
    {
        private DataContractJsonSerializer ser;
        private string _studioApiBaseURL = @"https://{0}studioapi.azureml.net/api/";
        private string _webServiceApiBaseUrl = @"https://{0}management.azureml.net/";
        private string _azMgmtApiBaseUrl = @"https://management.core.windows.net/{0}/cloudservices/amlsdk/resources/machinelearning/~/workspaces/";
        private string httpResponsePayload = string.Empty;

        public string StudioApi = "https://studioapi.azureml.net/api/";
        public string WebServiceApi = "https://management.azureml.net/";
        protected ManagementUtil Util { get; private set; }
        private string _sdkName = "dotnetsdk_0.2";
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
                    break;
                case "west europe":
                    key = "europewest.";
                    break;
                case "southeast asia":
                    key = "asiasoutheast.";
                    break;
                default:
                    throw new Exception("Unsupported location: " + location);
            }
            StudioApi = string.Format(_studioApiBaseURL, key);
            WebServiceApi = string.Format(_webServiceApiBaseUrl, key);
        }

        private string GetExperimentGraphFromJson(string rawJson)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic parsed = jss.Deserialize<object>(rawJson);
            string graph = jss.Serialize(parsed["Graph"]);
            return graph;
        }
        private string GetExperimentWebServiceFromJson(string rawJson)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
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
            throw new ArgumentException(string.Format(
              "A Certificate with Thumbprint '{0}' could not be located.",
              thumbprint));
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
            JavaScriptSerializer jss = new JavaScriptSerializer();
            WorkspaceRdfe[] workspaces = jss.Deserialize<WorkspaceRdfe[]>(result);
            return workspaces;
        }

        public async Task<string> CreateWorkspace(string managementCertThumbprint, string azureSubscriptionId, string workspaceName, string location, string storageAccountName, string storageAccountKey, string ownerEmail, string source)
        {        
            // initial workspace is a made-up but valid guid.
            string reqUrl = string.Format(_azMgmtApiBaseUrl + "/e582920d010646acbb0ec3183dc2243a", azureSubscriptionId);

            HttpWebRequest httpReq = GetRdfeHttpRequest(managementCertThumbprint, reqUrl, "PUT");            

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string payload = jss.Serialize(new
                {
                    Name = workspaceName,
                    Location = location,
                    StorageAccountName = storageAccountName,
                    StorageAccountKey = storageAccountKey,
                    OwnerId = ownerEmail,
                    ImmediateActivation = true,
                    Source = source
                });
            httpReq.ContentLength = payload.Length;
            Stream stream = httpReq.GetRequestStream();

            //byte[] buffer = System.Text.Encoding.GetEncoding(1252).GetBytes(payload);
            byte[] buffer = ASCIIEncoding.ASCII.GetBytes(payload);
            stream.Write(buffer, 0, buffer.Length);
            
            WebResponse resp = await httpReq.GetResponseAsync();

            long len = resp.ContentLength;
            buffer = new byte[len];
            resp.GetResponseStream().Read(buffer, 0, (int)len);
            string result = ASCIIEncoding.ASCII.GetString(buffer);            
            dynamic d = jss.Deserialize<object>(result);
            return d["Id"];
        }

        public WorkspaceRdfe GetCreateWorkspaceStatus(string managementCertThumbprint, string azureSubscriptionId, string workspaceId)
        {
            string reqUrl = string.Format(_azMgmtApiBaseUrl + "/{1}", azureSubscriptionId, workspaceId);
            HttpWebRequest httpReq = GetRdfeHttpRequest(managementCertThumbprint, reqUrl, "GET");
            
            WebResponse resp = httpReq.GetResponse();
            long len = resp.ContentLength;
            byte[] buffer = new byte[len];
            resp.GetResponseStream().Read(buffer, 0, (int)len);
            string result = ASCIIEncoding.ASCII.GetString(buffer);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            WorkspaceRdfe ws = jss.Deserialize<WorkspaceRdfe>(result);
            return ws;
        }
             

        public void RemoveWorkspace(string managementCertThumbprint, string azureSubscriptionId, string workspaceId)
        {
            string reqUrl = string.Format("https://management.core.windows.net/{0}/cloudservices/{1}/resources/machinelearning/~/workspaces/{2}", azureSubscriptionId, "powershell", workspaceId);
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
            string queryUrl = StudioApi + string.Format("workspaces/{0}", setting.WorkspaceId);
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                MemoryStream ms = new MemoryStream(UnicodeEncoding.Unicode.GetBytes(hr.Payload));
                ser = new DataContractJsonSerializer(typeof(Workspace));
                Workspace ws = (Workspace)ser.ReadObject(ms);
                return ws;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public void AddWorkspaceUsers(WorkspaceSetting setting, string emails, string role)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/invitations", setting.WorkspaceId);
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
        #endregion

        #region Dataset
        public Dataset[] GetDataset(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + string.Format("workspaces/{0}/datasources", setting.WorkspaceId);
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Dataset[] datasets = jss.Deserialize<Dataset[]>(hr.Payload);
            return datasets;
        }

        public void DeleteDataset(WorkspaceSetting setting, string datasetFamilyId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string url = StudioApi + string.Format("workspaces/{0}/datasources/family/{1}", setting.WorkspaceId, datasetFamilyId);
            HttpResult hr = Util.HttpDelete(url).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public async Task DownloadDatasetAsync(WorkspaceSetting setting, string datasetId, string filename)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string url = StudioApi + string.Format("workspaces/{0}/datasources/{1}", setting.WorkspaceId, datasetId);
            HttpResult hr = Util.HttpGet(url).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Dataset ds = jss.Deserialize<Dataset>(hr.Payload);
            string downloadUrl = ds.DownloadLocation.BaseUri + ds.DownloadLocation.Location + ds.DownloadLocation.AccessCredential;
            hr = await Util.HttpGet(downloadUrl, false);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            FileStream fs = File.Create(filename);
            hr.PayloadStream.Seek(0, SeekOrigin.Begin);
            hr.PayloadStream.CopyTo(fs);
            fs.Close();
        }

        public async Task<string> UploadDatasetAsnyc(WorkspaceSetting setting, string FileFormat, string UploadFileName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = StudioApi + string.Format("resourceuploads/workspaces/{0}/?userStorage=true&dataTypeId={1}", setting.WorkspaceId, FileFormat);
            HttpResult hr = await Util.HttpPostFile(query, UploadFileName);
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            return hr.Payload;
        }
        public string StartDatasetSchemaGen(WorkspaceSetting setting, string dataTypeId, string uploadFileId, string datasetName, string description, string uploadFileName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            JavaScriptSerializer jss = new JavaScriptSerializer();
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
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string query = StudioApi + string.Format("workspaces/{0}/datasources/{1}", setting.WorkspaceId, dataSourceId);
            HttpResult hr = Util.HttpGet(query).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
            dynamic parsed = jss.Deserialize<object>(hr.Payload);
            string schemaJobStatus = parsed["SchemaStatus"];
            return schemaJobStatus;
        }
        #endregion

        #region Experiment
        public Experiment[] GetExperiments(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments", setting.WorkspaceId);
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                MemoryStream ms = new MemoryStream(UnicodeEncoding.Unicode.GetBytes(hr.Payload));
                ser = new DataContractJsonSerializer(typeof(Experiment[]));
                Experiment[] exps = (Experiment[])ser.ReadObject(ms);
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
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}", setting.WorkspaceId, experimentId);
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                rawJson = hr.Payload;
                MemoryStream ms = new MemoryStream(UnicodeEncoding.Unicode.GetBytes(hr.Payload));
                ser = new DataContractJsonSerializer(typeof(Experiment));
                Experiment exp = (Experiment)ser.ReadObject(ms);
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
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}", setting.WorkspaceId, createNewCopy ? string.Empty : exp.ExperimentId);
            HttpResult hr = Util.HttpPost(queryUrl, body).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }
        public void RemoveExperimentById(WorkspaceSetting setting, string ExperimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}?deleteAncestors=true", setting.WorkspaceId, ExperimentId);
            HttpResult hr = Util.HttpDelete(queryUrl).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public PackingServiceActivity PackExperiment(WorkspaceSetting setting, string experimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/packages?api-version=2.0&experimentid={1}/&clearCredentials=true&includeAuthorId=false", setting.WorkspaceId, experimentId);
            HttpResult hr = Util.HttpPost(queryUrl, string.Empty).Result;
            if (hr.IsSuccess)
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                PackingServiceActivity activity = jss.Deserialize<PackingServiceActivity>(hr.Payload);
                return activity;
            }
            throw new AmlRestApiException(hr);
        }
        public PackingServiceActivity GetActivityStatus(WorkspaceSetting setting, string activityId, bool isPacking)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            return GetActivityStatus(setting, setting.WorkspaceId, setting.AuthorizationToken, activityId, isPacking);
        }

        public PackingServiceActivity GetActivityStatus(WorkspaceSetting setting, string wsId, string authCode, string activityId, bool isPacking)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/packages?{1}ActivityId={2}", wsId, (isPacking ? "package" : "unpack"), activityId);
            HttpResult hr = Util.HttpGet(authCode, queryUrl, true).Result;

            if (hr.IsSuccess)
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                PackingServiceActivity activity = jss.Deserialize<PackingServiceActivity>(hr.Payload);
                return activity;
            }
            else
                throw new AmlRestApiException(hr);
        }


        public PackingServiceActivity UnpackExperiment(WorkspaceSetting setting, string destWorkspaceId, string destWorkspaceAuthCode, string packedLocation)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/packages?api-version=2.0&packageUri={1}", destWorkspaceId, HttpUtility.UrlEncode(packedLocation));
            HttpResult hr = Util.HttpPut(destWorkspaceAuthCode, queryUrl, string.Empty).Result;
            if (hr.IsSuccess)
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                PackingServiceActivity activity = jss.Deserialize<PackingServiceActivity>(hr.Payload);
                return activity;
            }
            throw new AmlRestApiException(hr);
        }

        
        // Note this API is NOT officially supported. It might break in the future and we won't support it if/when it happens.
        public PackingServiceActivity UnpackExperimentFromGallery_UnsupportedAPI(WorkspaceSetting setting, string packageUri, string galleryUrl, string entityId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/packages?api-version=2.0&packageUri={1}&communityUri={2}&entityId={3}", setting.WorkspaceId, HttpUtility.UrlEncode(packageUri), HttpUtility.UrlEncode(galleryUrl), entityId);
            HttpResult hr = Util.HttpPut(setting.AuthorizationToken, queryUrl, string.Empty).Result;
            if (hr.IsSuccess)
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                PackingServiceActivity activity = jss.Deserialize<PackingServiceActivity>(hr.Payload);
                return activity;
            }
            throw new AmlRestApiException(hr);
        }
        #endregion

        #region Web Service

        public WebService[] GetWebServicesInWorkspace(WorkspaceSetting setting)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices", setting.WorkspaceId);
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                MemoryStream ms = new MemoryStream(UnicodeEncoding.Unicode.GetBytes(hr.Payload));
                ser = new DataContractJsonSerializer(typeof(WebService[]));
                WebService[] wss = (WebService[])ser.ReadObject(ms);
                return wss;
            }
            else
                throw new AmlRestApiException(hr);
        }
        public WebService GetWebServicesById(WorkspaceSetting setting, string webServiceId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}", setting.WorkspaceId, webServiceId);
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                MemoryStream ms = new MemoryStream(UnicodeEncoding.Unicode.GetBytes(hr.Payload));
                ser = new DataContractJsonSerializer(typeof(WebService));
                WebService ws = (WebService)ser.ReadObject(ms);
                return ws;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public WebServiceCreationStatus DeployWebServiceFromPredicativeExperiment(WorkspaceSetting setting, string predicativeExperimentId)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}/webservice", setting.WorkspaceId, predicativeExperimentId);
            HttpResult hr = Util.HttpPost(queryUrl, string.Empty).Result;
            if (hr.IsSuccess)
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
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
            string queryUrl = StudioApi + string.Format("workspaces/{0}/experiments/{1}/webservice", setting.WorkspaceId, activityId);
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
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
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}", setting.WorkspaceId, webServiceId);
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
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints", setting.WorkspaceId, webServiceId);
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                ser = new DataContractJsonSerializer(typeof(WebServiceEndPoint[]));
                MemoryStream ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(hr.Payload));
                WebServiceEndPoint[] weps = (WebServiceEndPoint[])ser.ReadObject(ms);
                return weps;
            }
            else
                throw new AmlRestApiException(hr);
        }
        public WebServiceEndPoint GetWebServiceEndpointByName(WorkspaceSetting setting, string webServiceId, string epName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, webServiceId, epName);
            HttpResult hr = Util.HttpGet(queryUrl).Result;
            if (hr.IsSuccess)
            {
                ser = new DataContractJsonSerializer(typeof(WebServiceEndPoint));
                MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(hr.Payload));
                WebServiceEndPoint ep = (WebServiceEndPoint)ser.ReadObject(ms);
                return ep;
            }
            else
                throw new AmlRestApiException(hr);
        }

        public void AddWebServiceEndpoint(WorkspaceSetting setting, AddWebServiceEndpointRequest req)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, req.WebServiceId, req.EndpointName);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string body = jss.Serialize(req);
            HttpResult hr = Util.HttpPut(queryUrl, body).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }
        public void RefreshWebServiceEndPoint(WorkspaceSetting setting, string webServiceId, string endpointName, bool overwriteResources)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string query = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}/refresh", setting.WorkspaceId, webServiceId, endpointName);
            string body = "{\"OverwriteResources\": \"" + overwriteResources.ToString() + "\"}";
            HttpResult hr = Util.HttpPost(query, body).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public void PatchWebServiceEndpoint(WorkspaceSetting setting, string webServiceId, string endpointName, dynamic patchReq)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string body = jss.Serialize(patchReq);
            string url = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, webServiceId, endpointName);
            HttpResult hr = Util.HttpPatch(url, body).Result;
            if (!hr.IsSuccess)
                throw new AmlRestApiException(hr);
        }

        public void RemoveWebServiceEndpoint(WorkspaceSetting setting, string webServiceId, string endpointName)
        {
            ValidateWorkspaceSetting(setting);
            Util.AuthorizationToken = setting.AuthorizationToken;
            string queryUrl = WebServiceApi + string.Format("workspaces/{0}/webservices/{1}/endpoints/{2}", setting.WorkspaceId, webServiceId, endpointName);
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
            JavaScriptSerializer jss = new JavaScriptSerializer();
            HttpResult hr = Util.HttpGet(getJobStatusApiLocation).Result;
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
