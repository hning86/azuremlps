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
        [Parameter(Mandatory = false)]
        public string ExperimentId { get; set; }
        protected override void ProcessRecord()
        {
            List<UserAsset> trainedModelsInWorkspace = new List<UserAsset>(Sdk.GetTrainedModels(GetWorkspaceSetting()));
            if (string.IsNullOrEmpty(ExperimentId))
                WriteObject(trainedModelsInWorkspace, true);
            else // find all transforms in the specified experiment
            {
                List<UserAsset> trainedModelsInExperiment = new List<UserAsset>();
                string rawJson;
                Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                dynamic graph = jss.Deserialize<object>(rawJson);
                foreach (var node in graph["Graph"]["ModuleNodes"])
                    foreach (var inputPort in node["InputPortsInternal"])
                        if (!string.IsNullOrEmpty(inputPort["TrainedModelId"]))
                        {
                            var trainedModel = trainedModelsInWorkspace.SingleOrDefault(tm => tm.Id == inputPort["TrainedModelId"]);
                            if (trainedModel != null && !trainedModelsInExperiment.Contains(trainedModel))
                                trainedModelsInExperiment.Add(trainedModel);
                        }
                WriteObject(trainedModelsInExperiment, true);
            }
        }
    }

    [Cmdlet("Get", "AmlTransform")]
    public class GetTransform : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = false)]
        public string ExperimentId { get; set; }
        protected override void ProcessRecord()
        {
            List<UserAsset> transformsInWorkspace = new List<UserAsset>(Sdk.GetTransforms(GetWorkspaceSetting()));
            if (string.IsNullOrEmpty(ExperimentId))
                WriteObject(transformsInWorkspace, true);
            else // find all transforms in the specified experiment
            {
                List<UserAsset> transformsInExperiment = new List<UserAsset>();
                string rawJson;
                Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                dynamic graph = jss.Deserialize<object>(rawJson);
                foreach (var node in graph["Graph"]["ModuleNodes"])
                    foreach (var inputPort in node["InputPortsInternal"])
                        if (!string.IsNullOrEmpty(inputPort["TransformModuleId"]))
                        {
                            var transform = transformsInWorkspace.Single(tran => tran.Id == inputPort["TransformModuleId"]);
                            if (transform != null && !transformsInExperiment.Contains(transform))
                                transformsInExperiment.Add(transform);
                        }
                WriteObject(transformsInExperiment, true);
            }
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
            WriteObject(string.Format("Transform \"{0}\"' has been successfully promoted.", TransformName));
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
            WriteObject(string.Format("Trained Model \"{0}\"' has been successfully promoted.", TrainedModelName));
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
            WriteObject(string.Format("Dataset \"{0}\"' has been successfully promoted.", DatasetName));
        }
    }

    [Cmdlet("Replace", "AmlExperimentUserAsset")]
    public class ReplaceExperimentUserAsset : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        [Parameter(Mandatory = true)]
        public string OldAssetModuleId { get; set; }
        [Parameter(Mandatory = true)]
        public string NewAssetModuleId { get; set; }
        [Parameter(Mandatory = true)]
        [ValidateSet("TrainedModel", "Transform", "Dataset")]
        public string AssetType { get; set; }

        protected override void ProcessRecord()
        {
            if (OldAssetModuleId == NewAssetModuleId)
            {
                WriteWarning("Source and target are the same asset. No replacement is needed.");
                return;
            }
            string rawJson;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic graph = jss.Deserialize<object>(rawJson);
            Dictionary<string, string> assetTypeNames = new Dictionary<string, string>();
            assetTypeNames.Add("TrainedModel", "TrainedModelId");
            assetTypeNames.Add("Transform", "TransformModuleId");
            assetTypeNames.Add("Dataset", "DataSourceId");

            int count = 0;
            foreach (var node in graph["Graph"]["ModuleNodes"])
                foreach (var inputPort in node["InputPortsInternal"])
                    if (inputPort[assetTypeNames[AssetType]] == OldAssetModuleId)
                    {
                        inputPort[assetTypeNames[AssetType]] = NewAssetModuleId;
                        count++;
                    }

            string clientData = graph["Graph"]["SerializedClientData"];
            graph["Graph"]["SerializedClientData"] = clientData.Replace(OldAssetModuleId, NewAssetModuleId);
            rawJson = jss.Serialize(graph);
            Sdk.SaveExperiment(GetWorkspaceSetting(), exp, rawJson);
            WriteObject(string.Format("{0} has been replaced.", AssetType));
        }
    }
}
