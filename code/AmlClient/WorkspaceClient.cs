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
    public sealed class WorkspaceClient
    {
        public const string Version = "0.2.4";

        public string ApiName { get; set; } = "dotnetsdk";

        public WorkspaceStorageClient StorageClient { get; private set; }

        public Workspace Storage { get; private set; }

        public AmlClient Aml { get; private set; }

        public LocationApi Location { get; private set; }

        public Uri WorkspaceApi { get; private set; }

        public Uri UploadResourceApi { get; private set; }

        public Uri UploadBlobApi { get; private set; }

        public Uri ExperimentsApi { get; private set; }

        public Uri DataSourcesApi { get; private set; }

        public Uri TrainedModelsApi { get; private set; }

        public Uri PackagesApi { get; private set; }

        public WorkspaceClient(WorkspaceStorageClient storageClient, Workspace storage)
        {
            this.StorageClient = storageClient;
            this.Storage = storage;
            this.Aml = new AmlClient(this.Storage.AuthorizationToken);

            var loc = Location = new LocationApi(Storage.Region);

            this.WorkspaceApi = new Uri(loc.Studio + "workspaces/" + this.Storage.Id + "/");
            this.UploadResourceApi = new Uri(loc.Studio + "resourceuploads/workspaces/" + this.Storage.Id + "/");
            this.UploadBlobApi = new Uri(loc.Studio + "blobuploads/workspaces/" + Storage.Id + "/");

            this.ExperimentsApi = new Uri(WorkspaceApi + "/experiments/");
            this.DataSourcesApi = new Uri(WorkspaceApi + "/datasources/");
            this.TrainedModelsApi = new Uri(WorkspaceApi + "/trainedmodels/");
            this.PackagesApi = new Uri(this.WorkspaceApi + "/packages?api-version=2.0");
        }

        #region Workspace

        internal async Task<WorkspaceEx> GetWorkspaceEx()
        {
            var r = await Aml.Get(this.WorkspaceApi);
            r.ThrowIfFailed();
            return await r.GetPayload<WorkspaceEx>();
        }

        public async Task<string> AddWorkspaceUsers(string emails, string role)
        {
            var content = new
            {
                Role = role,
                Emails = emails
            };

            var uri = new Uri(this.WorkspaceApi.AbsoluteUri + "/invitations");
            var r = await Aml.Post(uri, content);
            r.ThrowIfFailed();
            return await r.GetPayload();
        }

        public async Task<WorkspaceUser[]> GetWorkspaceUsers()
        {
            var uri = new Uri(this.WorkspaceApi.AbsoluteUri + "/users");
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<WorkspaceUser[]>();
        }

        #endregion

        #region DataSource

        public async Task<DataSource[]> GetDataSources()
        {
            var uri = new Uri(this.WorkspaceApi.AbsoluteUri + "/datasources");
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<DataSource[]>();
        }

        public async Task<string> DeleteDataSource(string id)
        {
            var uri = new Uri(this.WorkspaceApi + "/datasources/family/" + id);
            var r = await Aml.Delete(uri);
            r.ThrowIfFailed();

            return await r.Payload;
        }

        static async Task<AmlResult> GetData(DataSource ds)
        {
            var uri = new Uri(ds.DownloadLocation.BaseUri + ds.DownloadLocation.Location + ds.DownloadLocation.AccessCredential);
            var client = new HttpClient();
            var response = await client.GetAsync(uri);
            return new AmlResult(response);
        }

        public async Task<DataSource> GetDataSource(string id, string filename)
        {
            var sourceUri = new Uri(DataSourcesApi + id);
            var r = await Aml.Get(sourceUri);
            r.ThrowIfFailed();

            var ds = await r.GetPayload<DataSource>();
            var dr = await GetData(ds);
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
            var uri = new Uri(UploadResourceApi + "?userStorage=true&dataTypeId=" + type);

            using (var s = File.OpenRead(fileName))
            {
                var r = await Aml.Post(uri, s);
                r.ThrowIfFailed();
                var ur = await r.Payload;
                var response = JsonConvert.DeserializeObject<ResourceUploadResponse>(ur);
                return response;
            }
        }

        public async Task<string> UploadResourceChunk(int numOfBlocks, int blockId, string uploadId, string fileName, DataSourceType type)
        {
            var uri = new Uri(this.UploadBlobApi
                + "/?numberOfBlocks=" + numOfBlocks
                + "&blockId=" + blockId
                + "&uploadId=" + uploadId
                + "&dataTypeId=" + Enum.GetName(typeof(DataSourceType), type)
                );

            using (var s = File.OpenRead(fileName))
            {
                var r = await Aml.Post(uri, s);
                r.ThrowIfFailed();
                return await r.Payload;
            }
        }

        public async Task<string> StartDatasetSchemaGen(DataSourceType type, string uploadId, string datasetName, string description, string uploadFileName)
        {
            var schemaJob = new
            {
                DataSource = new
                {
                    Name = datasetName,
                    DataTypeId = Enum.GetName(typeof(DataSourceType), type),
                    Description = description,
                    FamilyId = string.Empty,
                    Owner = "User",
                    SourceOrigin = "FromResourceUpload"
                },

                UploadId = uploadId,
                UploadedFromFileName = Path.GetFileName(uploadFileName),
                ClientPoll = true
            };

            var r = await Aml.Post(DataSourcesApi, schemaJob);
            r.ThrowIfFailed();

            var dataSourceId = (await r.Payload).Replace("\"", "");
            return dataSourceId;
        }

        #endregion

        #region Custom Module

        public async Task<string> BeginParseCustomModuleJob(string moduleUploadMetadata)
        {
            var uri = new Uri(WorkspaceApi + "/modules/custom");
            var r = await Aml.Post(uri, moduleUploadMetadata);
            r.ThrowIfFailed();

            string activityId = (await r.Payload).Replace("\"", "");
            return activityId;            
        }

        public async Task<string> GetCustomModuleBuildJobStatus(string id)
        {
            var uri = new Uri(WorkspaceApi + "/modules/custom?activityGroupId=" + id);
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();
            return await r.GetPayload();
        }

        #endregion

        #region Trained Model

        public async Task<TrainedModel[]> GetTrainedModels()
        {
            var r = await Aml.Get(TrainedModelsApi);
            r.ThrowIfFailed();

            return await r.GetPayload<TrainedModel[]>();
        }

        public async Task<TrainedModel> GetTrainedModel(string id)
        {
            var uri = new Uri(TrainedModelsApi + id);
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();

            return await r.GetPayload<TrainedModel>();
        }

        #endregion

        #region Experiment

        public async Task<Experiment[]> GetExperiments()
        {
            var r = await Aml.Get(this.ExperimentsApi);
            r.ThrowIfFailed();

            var exps = await r.GetPayload<Experiment[]>();

            // only display user's own experiments.
            exps = exps.Where(e => e.Category == "user" || string.IsNullOrEmpty(e.Category)).ToArray();
            return exps;
        }

        public async Task<Experiment> GetExperiment(string id)
        {
            var uri = new Uri(ExperimentsApi + id);
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();

            return await r.GetPayload<Experiment>();
        }

        public async Task<string> RunExperiment(string id)
        {
            var e = await GetExperiment(id);
            var req = new RunExperimentRequest(e);
            var uri = new Uri(ExperimentsApi + e.ExperimentId);
            var r = await Aml.Post(uri, req);
            return await r.GetPayload();
        }

        public async Task<string> NewExperiment(Experiment e)
        {
            var req = new NewExperimentRequest(e);
            var r = await Aml.Post(ExperimentsApi, req);
            return await r.Payload;
        }

        public async Task<string> SaveAsExperiment(string id, string name)
        {
            var e = await GetExperiment(id);
            if (e == null)
                throw new AmlException("experiment id "+id+" not found");
            e.Description = name;
            return await NewExperiment(e);
        }

        public async Task<string> UpdateExperiment(Experiment e)
        {
            if (e.ExperimentId == null || e.Etag == null)
                throw new AmlException("cannot update experiment");
            var uri = new Uri(ExperimentsApi + e.ExperimentId);
            var u = new UpdateExperimentRequest(e);
            var r = await Aml.Post(uri, u);
            return await r.GetPayload();
        }

        public async Task<string> RemoveExperiment(string id, bool removeHistory)
        {
            var e = await GetExperiment(id);
            var uri = new Uri(ExperimentsApi + e.ExperimentId + "?deleteAncestors=" + removeHistory);
            var r = await Aml.Delete(uri, e);
            return await r.GetPayload();
        }

        public async Task<DeployStatus> DeployExperiment(string id)
        {
            var uri = new Uri(ExperimentsApi + id + "/webservice");
            var r = await Aml.Post(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<DeployStatus>();
        }

        public async Task<DeployStatus> GetDeploy(string id)
        {
            var uri = new Uri(ExperimentsApi + id + "/webservice");
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<DeployStatus>();
        }

        public async Task<Packing> Pack(string id)
        {
            var uri = new Uri(PackagesApi
                + "&experimentid=" + id
                + "&clearCredentials=true"
                + "&includeAuthorId=false");
            var r = await Aml.Post(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<Packing>();
        }

        public async Task<T> GetActivity<T>(T a) where T : PackState
        {
            var uri = new Uri(PackagesApi + "&" + a.IdField);
            var r = await Aml.Get(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<T>();
        }

        public async Task<Unpacking> Unpack(Uri package)
        {
            var uri = new Uri(PackagesApi
                + "&packageUri=" + HttpUtility.UrlEncode(package.AbsoluteUri));
            var r = await Aml.Put(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<Unpacking>();
        }

        public async Task<Unpacking> UnpackFromGallery(Uri package, Uri gallery, string entityId)
        {
            var uri = new Uri(PackagesApi
                + "&packageUri=" + HttpUtility.UrlEncode(package.AbsoluteUri)
                + "&communityUri=" + HttpUtility.UrlEncode(gallery.AbsoluteUri)
                + "&entityId=" + entityId);
            var r = await Aml.Put(uri);
            r.ThrowIfFailed();
            return await r.GetPayload<Unpacking>();
        }

        #endregion
    }
}
