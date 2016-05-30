
          public sealed class ModuleParameter
          {
          public string ValueType {get;set;}
          public string LinkedGlobalParameter {get;set;}
          public string Name {get;set;}
          public string Value {get;set;}
          }

          public sealed class ModuleInput
          {
          public string Name {get;set;}
          public string DataSourceId {get;set;}
          public string TrainedModelId {get;set;}
          public string TransformModuleId {get;set;}
          public string Language {get;set;}
          public string Version {get;set;}
          public string PortIndex {get;set;}
          public string NodeId {get;set;}
          }

          public sealed class ModuleOutput
          {
          public string Name {get;set;}
          public string NodeId {get;set;}
          public string OutputType {get;set;}
          }


      public sealed class ModuleInfo
      {
      public string Id {get;set;}
      public string ModuleId {get;set;}
      public string Comment {get;set;}
      public bool CommentCollapsed {get;set;}
      public string Language {get;set;}
      public string Version {get;set;}
      public bool UsePreviousResults {get;set;}
      public ModuleParameter[] ModuleParameters {get;set;}
      public bool IsPartOfPartialRun {get;set}
      public ModuleInput[] InputPortsInternal {get;set;}
      public ModuleOutput[] OutputPortsInternal {get;set;}
      }


public sealed class GraphEdge
{
public string SourceOutputPortId {get;set;}
public string DestinationInputPortId {get;set;}
public string DestinationInputPortIndex {get;set;}
}


public sealed class GraphInfo
{
public ModuleInfo[] ModuleNodes {get;set;}
public string SerializedClientData {get;set;}
public string PublishInfo {get;set;}
public string Parameters {get;set;}
public GraphEdge[] EdgesInternal {get;set;}
}

public abstract class Pair<T>
{
public T Value {get;set}
}

public sealed class NamePair<V> : Pair<V>
{
public string Name {get;set;}
}

public sealed class KeyPair<V> : Pair<V>
  {
  public string Key {get;set;}
  }

public sealed class StatusInfo<K>
{
public string StatusCode {get;set;}
public string StatusDetail {get;set;}
public DateTime CreationTime {get;set;}
public DateTime StartTime {get;set;}
public DateTime EndTime {get;set;}
public K Metadata {get;set;}
}

public sealed class Endpoint
{
public string BaseUri {get;set;}
public int Size {get;set;}
public string Name {get;set;}
public string EndpointType {get;set;}
public string CredentialContainer {get;set;}
public string AccessCredential {get;set;}
public string Location {get;set;}
public string FileType {get;set;}
public bool IsAuxiliary {get;set;}
}

public sealed class NodeStatus
  {
  public string NodeId {get;set;}
  public StatusInfo Status {get;set;}
  public Endpoint StandardErrorEndpoint {get;set;}
  public Endpoint StandardOutEndpoint {get;set;}
  public Endpoint[] OutputEndpoints {get;set;}
  public Dictionary<string,KeyPair<Endpoint>> MetadataOutputEndpoints {get;set;}
  }




public sealed class ExperimentInfo
{
public string ExperimentId {get;set;}
public string RunId {get;set;}
public string ParentExperimentId {get;set;}
public string OriginalExperimentDocumentationLink {get;set;}
public string Summary {get;set;}
public string Description {get;set;}
public GraphInfo Graph {get;set;}
public StatusInfo Status {get;set;}
public string Etag {get;set;}
public NodeStatus[] NodeStatuses {get;set;}

public string Creator {get;set;}
public bool IsLeaf {get;set;}
public string DisableNodesUpdate {get;set;}
public WebServiceInfo WebService {get;set;}

public sealed class PortInfo
      {
      public string Id {get;set;}
      public string PortId {get;set;}
      public string Name {get;set;}
      }

public sealed class WebServiceInfo
{
public bool IsWebServiceExperiment {get;set;}
public PortInfo[] Inputs {get;set;}
public PortInfo[] Outputs {get;set}
public ParameterInfo[] Parameters {get;set;}

public sealed class ParameterInfo
      {
      public string Name {get;set;}
      public string Value {get;set;}
      public Details ParameterDefinition {get;set;}

        public sealed class Details
        {
          public bool HasRules {get;set;}
          public string Name {get;set;}
          public bool IsOptional {get;set;}
          public string ParameterType {get;set;}
          public string ParameterRules {get;set;}
          public string MarkupType {get;set;}
          public bool HasDefaultValue {get;set;}
          public string DefaultValue {get;set;}
          public string ScriptName {get;set;}
          public string ModeValuesInfo {get;set;}
          public string CredentialDescriptor {get;set;}
          public string Description {get;set;}
          public string FriendlyName {get;set;}
        }
      }

public string WebServiceGroupId {get;set;}
public string ModelPackageId {get;set;}
public string RunId {get;set;}
public string SerializedClientData {get;set;}
public string ArmWebServiceId {get;set;}
}

public string Category {get;set;}
public string[] Tags {get;set;}
public bool IsPartialRun {get;set;}
}
