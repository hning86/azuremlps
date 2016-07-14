using AzureML.Contract;
using AzureML.PowerShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AzureMLPS.PowerShell
{
    [Cmdlet("Get", "AmlTrainedModel")]
    public class GetTrainedModel : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateSet("Experiment", "Workspace")]
        public string Scope { get; set; }

        [Parameter(Mandatory = false)]
        public string ExperimentId { get; set; }
        protected override void ProcessRecord()
        {
            if (Scope.ToLower() == "workspace")
            {
                WriteObject(Sdk.GetTrainedModels(GetWorkspaceSetting()), true);
                return;
            }
            if (string.IsNullOrEmpty(ExperimentId))
            {
                WriteWarning("ExperimentId must be specified.");
                return;
            }
            List<UserAsset> trainedModelsInWorkspace = new List<UserAsset>(Sdk.GetTrainedModels(GetWorkspaceSetting()));
            Dictionary<string, UserAssetBase> trainedModelsInExperiment = new Dictionary<string, UserAssetBase>();
            string rawJson;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic graph = jss.Deserialize<object>(rawJson);
            foreach (var node in graph["Graph"]["ModuleNodes"])
                foreach (var inputPort in node["InputPortsInternal"])
                {
                    string id = inputPort["TrainedModelId"];
                    if (!string.IsNullOrEmpty(id))
                    {
                        string familyId = id.Split('.')[1];
                        UserAssetBase trainedModel = trainedModelsInWorkspace.SingleOrDefault(tm => tm.Id == id || tm.FamilyId == familyId);
                        if (trainedModel != null && !trainedModelsInExperiment.ContainsKey(id))
                        {
                            bool isLatest = (trainedModelsInWorkspace.SingleOrDefault(t => t.Id == id) != null);
                            trainedModelsInExperiment.Add(id, new UserAssetBase
                            {
                                Id = id,
                                FamilyId = familyId,
                                DataTypeId = trainedModel.DataTypeId,
                                IsLatest = isLatest,
                                Name = trainedModel.Name
                            });
                        }
                    }
                }
            WriteObject(trainedModelsInExperiment.Values, true);
        }
    }

    [Cmdlet("Get", "AmlTransform")]
    public class GetTransform : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateSet("Experiment", "Workspace")]
        public string Scope { get; set; }

        [Parameter(Mandatory = false)]
        public string ExperimentId { get; set; }
        protected override void ProcessRecord()
        {
            if (Scope.ToLower() == "workspace")
            {
                WriteObject(Sdk.GetTransforms(GetWorkspaceSetting()), true);
                return;
            }
            if (string.IsNullOrEmpty(ExperimentId))
            {
                WriteWarning("ExperimentId must be specified.");
                return;
            }
            List<UserAsset> transformsInWorkspace = new List<UserAsset>(Sdk.GetTransforms(GetWorkspaceSetting()));
            Dictionary<string, UserAssetBase> transformsInExperiment = new Dictionary<string, UserAssetBase>();
            string rawJson;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic graph = jss.Deserialize<object>(rawJson);
            foreach (var node in graph["Graph"]["ModuleNodes"])
                foreach (var inputPort in node["InputPortsInternal"])
                {
                    string id = inputPort["TransformModuleId"];
                    if (!string.IsNullOrEmpty(id))
                    {
                        string familyId = id.Split('.')[1];
                        UserAssetBase transform = transformsInWorkspace.SingleOrDefault(tm => tm.Id == id || tm.FamilyId == familyId);
                        if (transform != null && !transformsInExperiment.ContainsKey(id))
                        {
                            bool isLatest = (transformsInWorkspace.SingleOrDefault(t => t.Id == id) != null);
                            transformsInExperiment.Add(id, new UserAssetBase
                            {
                                Id = id,
                                FamilyId = familyId,
                                DataTypeId = transform.DataTypeId,
                                IsLatest = isLatest,
                                Name = transform.Name
                            });
                        }
                    }
                }
            WriteObject(transformsInExperiment.Values, true);
        }
    }

    [Cmdlet("Promote", "AmlTransform")]
    public class PromoteTransform : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }

        [Parameter(Mandatory = true)]
        public string TransformModuleNodeId { get; set; }
        [Parameter(Mandatory = true)]
        public string NodeOutputPortName { get; set; }
        [Parameter(Mandatory = true)]
        public string TransformName { get; set; }
        [Parameter(Mandatory = true)]
        public string TransformDescription { get; set; }
        [Parameter(Mandatory = false)]
        public SwitchParameter Overwrite { get; set; }

        protected override void ProcessRecord()
        {
            string rawJson;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);

            if (exp.Status.StatusCode != "Finished")
            {
                WriteWarning("Experiment is not a finished state. The transform may have not been produced, or it may be a cached version from a previous run.");
            }

            string familyId = null;
            if (Overwrite.IsPresent) // overwrite an existing transform of the same name, if it exists
            {
                UserAsset[] transforms = Sdk.GetTransforms(GetWorkspaceSetting());
                UserAsset transformToOverwrite = transforms.SingleOrDefault(aa => aa.Name.ToLower().Trim() == TransformName.ToLower().Trim());
                if (transformToOverwrite != null)
                    familyId = transformToOverwrite.FamilyId;
            }

            Sdk.PromoteUserAsset(GetWorkspaceSetting(), ExperimentId, TransformModuleNodeId, NodeOutputPortName, TransformName, TransformDescription, UserAssetType.Transform, familyId);
            WriteObject(string.Format("Transform \"{0}\" has been successfully promoted.", TransformName));
        }
    }

    [Cmdlet("Promote", "AmlTrainedModel")]
    public class PromoteTrainedModel : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }

        [Parameter(Mandatory = true)]
        public string TrainModuleNodeId { get; set; }
        [Parameter(Mandatory = true)]
        public string NodeOutputPortName { get; set; }
        [Parameter(Mandatory = true)]
        public string TrainedModelName { get; set; }
        [Parameter(Mandatory = true)]
        public string TrainedModelDescription { get; set; }
        [Parameter(Mandatory = false)]
        public SwitchParameter Overwrite { get; set; }

        protected override void ProcessRecord()
        {
            string rawJson;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);

            if (exp.Status.StatusCode != "Finished")
            {
                WriteWarning("Experiment is not a finished state. The trained model may have not been produced, or it may be a cached version from a previous run.");
            }

            string familyId = null;
            if (Overwrite.IsPresent) // overwrite an existing trained model of the same name, if it exists
            {
                UserAsset[] trainedModel = Sdk.GetTrainedModels(GetWorkspaceSetting());
                UserAsset trainedModelToOverwrite = trainedModel.SingleOrDefault(aa => aa.Name.ToLower().Trim() == TrainedModelName.ToLower().Trim());
                if (trainedModelToOverwrite != null)
                    familyId = trainedModelToOverwrite.FamilyId;
            }

            Sdk.PromoteUserAsset(GetWorkspaceSetting(), ExperimentId, TrainModuleNodeId, NodeOutputPortName, TrainedModelName, TrainedModelDescription, UserAssetType.TrainedModel, familyId);
            WriteObject(string.Format("Trained Model \"{0}\" has been successfully promoted.", TrainedModelName));
        }
    }

    [Cmdlet("Promote", "AmlDataset")]
    public class PromoteDataset : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }

        [Parameter(Mandatory = true)]
        public string ModuleNodeId { get; set; }
        [Parameter(Mandatory = true)]
        public string NodeOutputPortName { get; set; }
        [Parameter(Mandatory = true)]
        public string DatasetName { get; set; }
        [Parameter(Mandatory = true)]
        public string DatasetDescription { get; set; }
        [Parameter(Mandatory = false)]
        public SwitchParameter Overwrite { get; set; }

        protected override void ProcessRecord()
        {
            string rawJson;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);

            if (exp.Status.StatusCode != "Finished")
            {
                WriteWarning("Experiment is not a finished state. The dataset may have not been produced, or it may be a cached version from a previous run.");
            }

            string familyId = null;
            if (Overwrite.IsPresent) // overwrite an existing trained model of the same name, if it exists
            {
                Dataset[] dataset = Sdk.GetDataset(GetWorkspaceSetting());
                Dataset datasetToOverwrite = dataset.SingleOrDefault(aa => aa.Name.ToLower().Trim() == DatasetName.ToLower().Trim());
                if (datasetToOverwrite != null)
                    familyId = datasetToOverwrite.FamilyId;
            }

            Sdk.PromoteUserAsset(GetWorkspaceSetting(), ExperimentId, ModuleNodeId, NodeOutputPortName, DatasetName, DatasetDescription, UserAssetType.Dataset, familyId);
            WriteObject(string.Format("Dataset \"{0}\" has been successfully promoted.", DatasetName));
        }
    }

    [Cmdlet("Update", "AmlExperimentModule")]
    public class UpdateAmlExperimentModule: AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "All modules")]
        [Parameter(Mandatory = true, ParameterSetName = "Modules with a specific name")]
        public string ExperimentId { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "All modules")]
        public SwitchParameter All { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "Modules with a specific name")]
        public string ModuleName { get; set; }
        protected override void ProcessRecord()
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();

            var modules = Sdk.GetModules(GetWorkspaceSetting());
            
            string rawJson = "";
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            dynamic graph = jss.Deserialize<object>(rawJson);
            List<string> updatedModules = new List<string>();
            foreach (dynamic node in graph["Graph"]["ModuleNodes"])
            {
                string moduleId = node["ModuleId"];
                string familyId = moduleId.Split('.')[1];
                if (!modules.Any(m => m.FamilyId.Equals(familyId, StringComparison.InvariantCultureIgnoreCase)))
                    throw new Exception(string.Format("The module with family id \"{0}\" in the experiment cannot be found in the workspace!", moduleId));
                var module = modules.SingleOrDefault(m => m.FamilyId.Equals(familyId, StringComparison.InvariantCultureIgnoreCase));
                if (!module.Id.Equals(moduleId, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(ModuleName) || module.Name.Equals(ModuleName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //needs update
                        WriteObject(string.Format("Module \"{0}\" with family id of \"{1}\" is updated from batch \"{2}\" to \"{3}\".", module.Name, module.FamilyId, moduleId.Split('.')[2], module.Id.Split('.')[2]));
                        node["ModuleId"] = module.Id;
                        updatedModules.Add(module.Name);
                    }
                }
            }

            if (updatedModules.Count > 0)
            {
                rawJson = jss.Serialize(graph);
                Sdk.SaveExperiment(GetWorkspaceSetting(), exp, rawJson);                
            }
            else
                WriteObject("All modules are already up-to-date, so no update is needed.");
        }

    }

    [Cmdlet("Update", "AmlExperimentUserAsset")]
    public class UpdateAmlExperimentUserAsset : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "AllAssets")]
        public SwitchParameter All { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "AllAssets")]
        [Parameter(Mandatory = true, ParameterSetName = "IndividualAsset")]
        public string ExperimentId { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "IndividualAsset")]
        public string AssetName { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "IndividualAsset")]
        public UserAssetType AssetType { get; set; }

        protected override void ProcessRecord()
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Dictionary<string, string> updatedAssetIds = new Dictionary<string, string>();
            string rawJson = "";
            UserAsset[] assetsInWorkspace = new UserAsset[] { };

            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            dynamic graph = jss.Deserialize<object>(rawJson);

            Dictionary<UserAssetType, string> assetNodeNames = new Dictionary<UserAssetType, string>();
            assetNodeNames.Add(UserAssetType.TrainedModel, "TrainedModelId");
            assetNodeNames.Add(UserAssetType.Transform, "TransformModuleId");
            assetNodeNames.Add(UserAssetType.Dataset, "DataSourceId");

            Dictionary<UserAssetType, string> assetTypeNames = new Dictionary<UserAssetType, string>();
            assetTypeNames.Add(UserAssetType.TrainedModel, "ILearnerDotNet");
            assetTypeNames.Add(UserAssetType.Transform, "ITransformDotNet");
            assetTypeNames.Add(UserAssetType.Dataset, "Dataset");
            List<UserAssetType> foundTypes = new List<UserAssetType>();

            foreach (dynamic node in graph["Graph"]["ModuleNodes"])
                foreach (dynamic inputPort in node["InputPortsInternal"])
                    foreach (UserAssetType assetType in assetNodeNames.Keys)
                    {
                        string assetId = inputPort[assetNodeNames[assetType]];
                        if (!string.IsNullOrEmpty(assetId) && !foundTypes.Contains(assetType))
                            foundTypes.Add(assetType);
                    }

            if (foundTypes.Count > 0)
            {
                if (All.IsPresent || (foundTypes.Contains(UserAssetType.Dataset) && AssetType == UserAssetType.Dataset))
                    assetsInWorkspace = assetsInWorkspace.Union(Sdk.GetDataset(GetWorkspaceSetting())).ToArray();
                if (All.IsPresent || (foundTypes.Contains(UserAssetType.TrainedModel) && AssetType == UserAssetType.TrainedModel))
                    assetsInWorkspace = assetsInWorkspace.Union(Sdk.GetTrainedModels(GetWorkspaceSetting())).ToArray();
                if (All.IsPresent || (foundTypes.Contains(UserAssetType.Transform) && AssetType == UserAssetType.Transform))
                    assetsInWorkspace = assetsInWorkspace.Union(Sdk.GetTransforms(GetWorkspaceSetting())).ToArray();

                UserAsset foundAsset = null;
                if (!string.IsNullOrEmpty(AssetName))
                {
                    foundAsset = assetsInWorkspace.SingleOrDefault(a => a.Name.ToLower() == AssetName.ToLower() && a.DataTypeId == assetTypeNames[AssetType]);
                    if (foundAsset == null)
                        throw new Exception(string.Format("{0} \"{1}\" is not found in the current workspace.", assetTypeNames[AssetType], AssetName));
                }

                //Dictionary<string, string> updatedAssets = new Dictionary<string, string>();
                bool foundAssetWithAssetNameInExperiment = false;
                foreach (dynamic node in graph["Graph"]["ModuleNodes"])
                    foreach (dynamic inputPort in node["InputPortsInternal"])
                        foreach (UserAssetType assetType in assetNodeNames.Keys)
                        {
                            string experimentAssetId = inputPort[assetNodeNames[assetType]];
                            if (!string.IsNullOrEmpty(experimentAssetId))
                            {
                                string familyId = experimentAssetId.Split('.')[1];
                                if (All.IsPresent || foundAsset.FamilyId == familyId)
                                {
                                    foundAssetWithAssetNameInExperiment = true;
                                    string assetName = assetsInWorkspace.SingleOrDefault(a => a.FamilyId == familyId).Name;
                                    UserAsset workspaceAsset = assetsInWorkspace.SingleOrDefault(a => a.FamilyId == familyId);
                                    if (workspaceAsset == null)
                                        throw new Exception(string.Format("Can't find {0} of family id \"{1}\" in the workspace.", familyId));
                                    if (workspaceAsset.Id != experimentAssetId)
                                    {
                                        if (!updatedAssetIds.ContainsKey(experimentAssetId))
                                        {
                                            inputPort[assetNodeNames[AssetType]] = workspaceAsset.Id;
                                            WriteObject(string.Format("{0} \"{1}\" has been updated from \"{2}\" to \"{3}\"", AssetType, assetName, experimentAssetId, workspaceAsset.Id));
                                            updatedAssetIds.Add(experimentAssetId, workspaceAsset.Id);
                                        }
                                    }
                                }
                            }
                        }
                if (!foundAssetWithAssetNameInExperiment)
                    throw new Exception(string.Format("Can't find {0} named \"{1}\" in the experiment.", AssetType, AssetName));

                if (updatedAssetIds.Count == 0)
                    WriteObject(string.Format("{0} already up-to-date.", All.IsPresent ? "All assets are" : AssetType + " \"" + AssetName + "\" is"));
                else
                {
                    string clientData = graph["Graph"]["SerializedClientData"];
                    foreach (var assetId in updatedAssetIds.Keys)
                        graph["Graph"]["SerializedClientData"] = clientData.Replace(assetId, updatedAssetIds[assetId]);
                    rawJson = jss.Serialize(graph);
                    Sdk.SaveExperiment(GetWorkspaceSetting(), exp, rawJson);
                }
            }
            else
                WriteObject("No updatable asset is found.");
        }
    }

    [Cmdlet("Replace", "AmlExperimentUserAsset")]
    public class ReplaceExperimentUserAsset : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        [Parameter(Mandatory = true)]
        public UserAssetType AssetType { get; set; }
        [Parameter(Mandatory = true)]
        public UserAssetBase ExistingAsset { get; set; }
        [Parameter(Mandatory = true)]
        public UserAssetBase NewAsset { get; set; }

        protected override void ProcessRecord()
        {
            if (ExistingAsset.Id == NewAsset.Id)
            {
                WriteWarning("Source and target are the same asset. No replacement is needed.");
                return;
            }
            string rawJson;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic graph = jss.Deserialize<object>(rawJson);
            Dictionary<UserAssetType, string> assetNodeNames = new Dictionary<UserAssetType, string>();
            assetNodeNames.Add(UserAssetType.TrainedModel, "TrainedModelId");
            assetNodeNames.Add(UserAssetType.Transform, "TransformModuleId");
            assetNodeNames.Add(UserAssetType.Dataset, "DataSourceId");

            int count = 0;
            foreach (var node in graph["Graph"]["ModuleNodes"])
                foreach (var inputPort in node["InputPortsInternal"])
                    if (inputPort[assetNodeNames[AssetType]] == ExistingAsset.Id)
                    {
                        inputPort[assetNodeNames[AssetType]] = NewAsset.Id;
                        count++;
                    }

            string clientData = graph["Graph"]["SerializedClientData"];
            graph["Graph"]["SerializedClientData"] = clientData.Replace(ExistingAsset.Id, NewAsset.Id);
            rawJson = jss.Serialize(graph);
            Sdk.SaveExperiment(GetWorkspaceSetting(), exp, rawJson);
            if (count > 0)
                WriteObject(string.Format("{0} \"{1}\" in the experiment \"{2}\" has been replaced with \"{3}\".", AssetType, ExistingAsset.Name, exp.Description, NewAsset.Name));
            else
                WriteWarning(string.Format("{0} \"{1}\" with id \"{2}\" cannot be found in the experiment \"{3}\".", AssetType, ExistingAsset.Name, ExistingAsset.Id, exp.Description));
        }
    }
}