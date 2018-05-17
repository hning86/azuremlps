using System;
using System.Collections.Generic;
using System.IO;

namespace AzureML.Contract
{
    public class Gateway
    {
        public string Scope { get; set; }
        public string GatewayName { get; set; }
        public string CreateTime { get; set; }
        public string Description { get; set; }
        public string ExpiryTime { get; set; }
        public string RegistrationKey { get; set; }
        public string LastConnectTime { get; set; }
        public string Location { get; set; }
        public string RegisterTime { get; set; }
        public string Status { get; set; }
        public string Version { get; set; }
        public string VersionStatus { get; set; }
        public string ProvisioningState { get; set; }
        public string Host { get; set; }
    }

    public class Invitation
    {
        public string Email { get; set; }
        public bool Failed { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class Notebook
    {
        public string FamilyId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Language { get; set; }
        public string Modified { get; set; }
        public string Created { get; set; }
        public string Tags { get; set; }
    }

    public class NotebookSession
    {
        public string AuthenticationUri { get; set; }
        public string SessionUri { get; set; }
        public string OriginUri { get; set; }
    }

    public class ProjectContainer
    {
        public string ContainerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Creator { get; set; }
        public Dictionary<string, List<string>> Contents { get; set; }
    }

    public class Annotation
    {
        public string Id { get; set; }
        public string ParentAnnotationId { get; set; }
        public string ExperimentId { get; set; }
        public string Content { get; set; }
        public bool IsCollapsed { get; set; }
        public string NodeId { get; set; }
        public string Edge { get; set; }
    }

    public class WorkspaceUser
    {
        public string Status { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }
        public WorkspaceUser(WorkspaceUserInternal iuser)
        {
            Status = iuser.Status;
            Email = iuser.User.Email;
            Name = iuser.User.Name;
            Role = iuser.User.Role;
            UserId = iuser.User.UserId;
        }
    }

    public class WorkspaceUserInternal
    {
        public string Status { get; set; }
        public UserDetailInternal User { get; set; }
        public class UserDetailInternal
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
            public string UserId { get; set; }
        }
    }
    public class WorkspaceSetting
    {
        public string WorkspaceId { get; set; }
        public string AuthorizationToken { get; set; }
        public string Location { get; set; }
    }
    public class Dataset : UserAsset
    {
        public EndPoint VisualizeEndPoint { get; set; }
        public EndPoint SchemaEndPoint { get; set; }
        public EndPoint DownloadLocation { get; set; }
        public string SchemaStatus { get; set; }
        public string UploadedFromFileName { get; set; }

        public class EndPoint
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
    public class WebService
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreationTime { get; set; }
        public string WorkspaceId { get; set; }
        public string DefaultEndpointName { get; set; }
        public int EndpointCount { get; set; }
    }
    public class WebServiceEndPoint
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
        public Resource[] Resources { get; set; }
        public class Resource
        {
            public string Name { get; set; }
            public string Kind { get; set; }
            public EpLocation Location { get; set; }
            public class EpLocation
            {
                public string BaseLocation { get; set; }
                public string RelativeLocation { get; set; }
                public string SasBlobToken { get; set; }
            }

        }
    }

    public class AuthorizationToken
    {
        public string PrimaryToken { get; set; }
        public string SecondaryToken { get; set; }
    }
    public class Workspace
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
        public string UserStorageKey { get; set; }
        public string UserStorageArmId { get; set; }
        public string UserStorageBlobEndpoint { get; set; }
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionState { get; set; }
        public string Region { get; set; }
        public string WorkspaceStatus { get; set; }
        public string Type { get; set; }
        public string CreatedTime { get; set; }
        public string OwnerPrincipalId { get; set; }
        public string Tags { get; set; }
        public string KeyVaultKeyIdentifier { get; set; }
        public string MsIdentityUrl { get; set; }
        public string MsIdentityPrincipalOid { get; set; }
        public string MsIdentityClientId { get; set; }
        public string MsIdentityCertificate { get; set; }
        public string TenantId { get; set; }
    }

    public class WorkspaceRdfe
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


    public class Experiment
    {
        public string ExperimentId { get; set; }
        public string RunId { get; set; }
        public string ParentExperimentId { get; set; }
        public string OriginalExperimentDocumentationLink { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public ExpStatus Status { get; set; }
        public string Etag { get; set; }
        public string Creator { get; set; }
        public bool IsLeaf { get; set; }
        public string DisableNodesUpdate { get; set; }
        public string Category { get; set; }
        public string[] Tags { get; set; }
        public class ExpStatus
        {
            public string StatusCode { get; set; }
            public string StatusDetail { get; set; }
            public string CreationTime { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string Metadata { get; set; }
        }
    }

    public class AddWebServiceEndpointRequest
    {
        public string WebServiceId { get; set; }
        public string EndpointName { get; set; }
        public string Description { get; set; }
        public string ThrottleLevel { get; set; }
        public int? MaxConcurrentCalls { get; set; }
        public bool PreventUpdate { get; set; }
    }

    public class PackingServiceActivity
    {
        public string Location { get; set; }
        public int ItemsComplete { get; set; }
        public int ItemsPending { get; set; }
        public string Status { get; set; }
        public string ActivityId { get; set; }
    }

    public class HttpResult
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string Payload { get; set; }
        public Stream PayloadStream { get; set; }
    }

    public class WebServiceCreationStatus
    {
        public string ActivityId { get; set; }
        public string WebServiceGroupId { get; set; }
        public string EndpointId { get; set; }
        public string Status { get; set; }

    }

    public class AmlRestApiException : Exception
    {
        public AmlRestApiException(HttpResult hr) : base("Error: [" + hr.StatusCode + " (" + hr.ReasonPhrase + ")]: " + hr.Payload) { }
    }

    public class StudioGraphNode
    {
        public string Id { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string UserData { get; set; }
    }

    public class StudioGraphEdge
    {
        public string Id { get; set; }
        public StudioGraphNode SourceNode { get; set; }
        public StudioGraphNode DestinationNode { get; set; }
        public string UserData { get; set; }
    }

    public class StudioGraph
    {
        public StudioGraph()
        {
            Nodes = new List<StudioGraphNode>();
            Edges = new List<StudioGraphEdge>();
        }

        public string Id { get; set; }
        public List<StudioGraphNode> Nodes { get; set; }
        public List<StudioGraphEdge> Edges { get; set; }
        public string UserData { get; set; }
    }

    public class GraphNode
    {
        public string Id { get; set; }
        public string ModuleId { get; set; }
        public string Comment { get; set; }
    }
    public class Module
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string FamilyId { get; set; }
        public bool IsDeterministic { get; set; }
        public bool IsBlocking { get; set; }
        public string ModuleType { get; set; }
        public string ReleaseState { get; set; }
        public ModuleLanguageMetadata ModuleLanguage { get; set; }
        public string ResourceUploadId { get; set; }
        public int Size { get; set; }
        public string CreatedDate { get; set; }
        public string Owner { get; set; }
        public bool IsLatest { get; set; }
        public bool IsDeprecated { get; set; }
        public string SourceOrigin { get; set; }
        public string ClientVersion { get; set; }
        public string ServiceVersion { get; set; }
        public int Batch { get; set; }
        public string EscalationEmail { get; set; }
        public string InformationUrl { get; set; }
        public class ModuleLanguageMetadata
        {
            public string Language { get; set; }
            public string Version { get; set; }
        }
    }

    public class UserAssetBase
    {
        public string Name { get; set; }
        public string FamilyId { get; set; }
        public string Id { get; set; }
        public bool IsLatest { get; set; }
        public string DataTypeId { get; set; }


    }
    public class UserAsset : UserAssetBase
    {
        public int Batch { get; set; }
        public string ExperimentId { get; set; }
        public string Owner { get; set; }
        public string PromotedFrom { get; set; }
        public string ResourceUploadId { get; set; }
        public string SourceOrigin { get; set; }
        public TrainedModelLanguageMetadata Language { get; set; }
        public class TrainedModelLanguageMetadata
        {
            public string Language { get; set; }
            public string Version { get; set; }
        }
        public int Size { get; set; }
        public string CreatedDate { get; set; }
        public string ClientVersion { get; set; }
        public int ServiceVersion { get; set; }
        public string Category { get; set; }
        public bool IsDeprecated { get; set; }
        public string Description { get; set; }
    }

    public enum UserAssetType
    {
        TrainedModel,
        Transform,
        Dataset
    }
}
