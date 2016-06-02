using System;
using System.IO;
using Newtonsoft.Json;

namespace AzureMachineLearning
{
    public sealed class UserInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }
    }

    public sealed class WorkspaceUser
    {
        public string Status { get; set; }
        public UserInfo User { get; set; }
    }

    public sealed class DataSource
    {
        public DatasetEndPoint VisualizeEndPoint { get; set; }
        public DatasetEndPoint SchemaEndPoint { get; set; }
        public string SchemaStatus { get; set; }
        public string Id { get; set; }
        public string DataTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FamilyId { get; set; }
        public string SourceOrigin { get; set; }
        public int Size { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Owner { get; set; }   
        public string ExperimentId { get; set; }     
        public string ClientVersion { get; set; }
        public string PromotedFrom { get; set; }
        public string UploadedFromFileName { get; set; }
        public int ServiceVersion { get; set; }
        public bool isLatest { get; set; }
        public string Category { get; set; }
        public DatasetEndPoint DownloadLocation { get; set; }
        public bool IsDeprecated { get; set; }
        public string Culture { get; set; }
        public int Batch { get; set; }

        public sealed class DatasetEndPoint
        {
            public string BaseUri { get; set; }
            public int size { get; set; }
            public string Name { get; set; }
            public string EndpointType { get; set; }
            public string CredentialContainer { get; set; }
            public string AccessCredential { get; set; }
            public string Location { get; set; }
            public string FileType { get; set; }
            public bool isAuxiliary { get; set; }
        }
    }

    public sealed class WebService
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreationTime { get; set; }
        public string WorkspaceId { get; set; }
        public string DefaultEndpointName { get; set; }
        public int EndpointCount { get; set; }
    }

    public sealed class Endpoint
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreationTime { get; set; }
        public string WebServiceId { get; set; }
        public string WorkspaceId { get; set; }
        public string HelpLocation { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string ApiLocation { get; set; }
        public string Version { get; set; }
        public bool PreventUpdate { get; set; }
        public bool SampleDataEnabled { get; set; }
        public string ExperimentLocation { get; set; }
        public int MaxConcurrentCalls { get; set; }
        public string DiagnosticsTraceLevel { get; set; }
        public string ThrottleLevel { get; set; }
        public WseResource[] Resources { get; set; }

        public sealed class WseResource
        {
            public string Name { get; set; }
            public string Kind { get; set; }
            public WserLocation Location { get; set; }

            public sealed class WserLocation
            {
                public string BaseLocation { get; set; }
                public string RelativeLocation { get; set; }
                public string SasBlobToken { get; set; }
            }            
        }
    }

    public sealed class AuthorizationToken
    {
        public string PrimaryToken { get; set; }
        public string SecondaryToken { get; set; }
    }

    internal sealed class WorkspaceEx
    {
        public string WorkspaceId { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public string HDInsightClusterConnectionString { get; set; }
        public string HDInsightStorageConnectionString { get; set; }
        public bool UseDefaultHDInsightSettings { get; set; }
        public AuthorizationToken AuthorizationToken { get; set; }
        public string MigrationStatus { get; set; }
        public string OwnerEmail { get; set; }
        public string UserStorage { get; set; }
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionState { get; set; }
        public string Region { get; set; }
        public string WorkspaceStatus { get; set; }
        public string Type { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public sealed class Workspace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SubscriptionId { get; set; }
        public string Region { get; set; }
        public string Description { get; set; }
        public string OwnerId { get; set; }
        public string StorageAccountName { get; set; }
        public string WorkspaceState { get; set; }
        public string EditorLink { get; set; } 
        public AuthorizationToken AuthorizationToken { get; set; }
    }

    public sealed class AddWorkspaceRequest
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string OwnerId { get; set; }
        public bool ImmediateActivation { get; set; }
        public string Source { get; set; }
    }

    public sealed class ExperimentSave
    {
        public string Description { get; set; }
        public string Summary { get; set; }
        public bool IsDraft { get; set; }
        public string ParentExprimentId { get; set; }
        public bool DisableNodeUpdate { get; set; } = false;
        public string Category { get; set; } = "user";
        public string ExperimentGraph { get; set; }
        public string WebService { get; set; }
    }

    public sealed class TrainedModel
    {
        public sealed class TmLanguage
        {
            public string Language { get; set; }
            public string Version { get; set; }
        }

        public TmLanguage Language { get; set; }

        public string Id { get; set; }
        public string DataTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FamilyId { get; set; }
        public string ResourceUploadId { get; set; }
        public string SourceOrigin { get; set; }
        public int Size { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Owner { get; set; }
        public string ExperimentId { get; set; }
        public string ClientVersion { get; set; }
        public string PromotedFrom { get; set; }
        public string UploadedFromFilename { get; set; }
        public int ServiceVersion { get; set; }
        public bool IsLatest { get; set; }
        public string Category { get; set; }
        public string DownloadLocation { get; set; }
        public bool IsDeprecated { get; set; }
        public string Culture { get; set; }
        public int Batch { get; set; }
        public long CreatedDateTicks { get; set; }
    }

    public sealed class AddEndpointRequest
    {
        public string WebServiceId { get; set; }
        public string EndpointName { get; set; }
        public string Description { get; set; }
        public string ThrottleLevel { get; set; }
        public int? MaxConcurrentCalls { get; set; }
    }

    public sealed class DeployStatus
    {
        public string ActivityId { get; set; }
        public string WebServiceGroupId { get; set; }
        public string EndpointId { get; set; }
        public string Status { get; set; }
    }

    public sealed class AmlException : Exception
    {
        public AmlException(string s) : base(s) { }

        public AmlException(AmlResult r) : base("Error: [" + r.Status + " (" + r.Reason + ")]: " + r.Payload.GetAwaiter().GetResult())
        {

        }
    }
}
