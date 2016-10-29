using AzureML.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AzureML.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlDataset")]
    public class GetDataset : AzureMLPsCmdlet
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
                WriteObject(Sdk.GetDataset(GetWorkspaceSetting()), true);
                return;
            }
            if (string.IsNullOrEmpty(ExperimentId))
            {
                WriteWarning("ExperimentId must be specified.");
                return;
            }
            List<Dataset> datasetInWorkspace = new List<Dataset>(Sdk.GetDataset(GetWorkspaceSetting()));
            Dictionary<string, UserAssetBase> datasetInExperiment = new Dictionary<string, UserAssetBase>();
            string rawJson;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic graph = jss.Deserialize<object>(rawJson);
            foreach (var node in graph["Graph"]["ModuleNodes"])
                foreach (var inputPort in node["InputPortsInternal"])
                {
                    string id = inputPort["DataSourceId"];
                    if (!string.IsNullOrEmpty(id))
                    {
                        string familyId = id.Split('.')[1];
                        UserAssetBase dataset = datasetInWorkspace.SingleOrDefault(tm => tm.Id == id || tm.FamilyId == familyId);
                        if (dataset != null && !datasetInExperiment.ContainsKey(id))
                        {
                            bool isLatest = (datasetInWorkspace.SingleOrDefault(t => t.Id == id) != null);
                            datasetInExperiment.Add(id, new UserAssetBase
                            {
                                Id = id,
                                FamilyId = familyId,
                                DataTypeId = dataset.DataTypeId,
                                IsLatest = isLatest,
                                Name = dataset.Name
                            });
                        }
                    }
                }
            WriteObject(datasetInExperiment.Values, true);
        }
    }

    [Cmdlet("Upload", "AmlDataset")]
    public class UploadDataset : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = false)]
        [ValidateSet("GenericCSV", "GenericCSVNoHeader", "GenericTSV", "GenericTSVNoHeader", "ARFF", "Zip", "RData", "PlainText")]
        public string FileFormat { get; set; }

        [Parameter(Mandatory = false)]
        public string DatasetName { get; set; }
        [Parameter(Mandatory = false)]
        public string Description { get; set; }

        [Parameter(Mandatory = true)]
        public string UploadFileName { get; set; }
        protected override void ProcessRecord()
        {
            ProgressRecord pr = new ProgressRecord(1, "Upload file", string.Format("Upload file \"{0}\" into Azure ML Studio", Path.GetFileName(UploadFileName)));
            pr.PercentComplete = 1;
            pr.CurrentOperation = "Uploading...";
            WriteProgress(pr);

            // step 1. upload file
            Task<string> uploadTask = Sdk.UploadResourceAsnyc(GetWorkspaceSetting(), FileFormat, UploadFileName);
            while (!uploadTask.IsCompleted)
            {
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                Thread.Sleep(500);
                WriteProgress(pr);
            }

            // step 2. generate schema
            pr.PercentComplete = 2;
            pr.StatusDescription = "Generating schema for dataset \"" + DatasetName + "\"";
            pr.CurrentOperation = "Generating schema...";
            WriteProgress(pr);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic parsed = jss.Deserialize<object>(uploadTask.Result);
            string dtId = parsed["DataTypeId"];
            string uploadId = parsed["Id"];
            string dataSourceId = Sdk.StartDatasetSchemaGen(GetWorkspaceSetting(), dtId, uploadId, DatasetName, Description, UploadFileName);

            // step 3. get status for schema generation
            string schemaJobStatus = "NotStarted";
            while (true)
            {
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                pr.CurrentOperation = "Schema generation status: " + schemaJobStatus;
                WriteProgress(pr);

                schemaJobStatus = Sdk.GetDatasetSchemaGenStatus(GetWorkspaceSetting(), dataSourceId);
                if (schemaJobStatus == "NotSupported" || schemaJobStatus == "Complete" || schemaJobStatus == "Failed")
                    break;
            }
            pr.PercentComplete = 100;
            WriteProgress(pr);

            WriteObject("Dataset upload status: " + schemaJobStatus);
        }
    }

    [Cmdlet("Download", "AmlDataset")]
    public class DownloadDataset : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "Dataset Id")]
        public string DatasetId { get; set; }
        [Parameter(Mandatory = true)]
        public string DownloadFileName { get; set; }

        protected override void ProcessRecord()
        {
            if (File.Exists(DownloadFileName))
                throw new Exception(DownloadFileName + " aleady exists.");

            ProgressRecord pr = new ProgressRecord(1, "Download file", string.Format("Download dataset \"{0}\" from Azure ML Studio", DownloadFileName));
            pr.PercentComplete = 1;
            pr.CurrentOperation = "Downloading...";
            WriteProgress(pr);

            Task task = Sdk.DownloadDatasetAsync(GetWorkspaceSetting(), DatasetId, DownloadFileName);
            while (!task.IsCompleted)
            {
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                Thread.Sleep(500);
                WriteProgress(pr);
            }
            pr.PercentComplete = 100;
            WriteProgress(pr);

            WriteObject("Dataset downloaded successfully as file \"" + DownloadFileName + "\".");
        }
    }

    [Cmdlet(VerbsCommon.Remove, "AmlDataset")]
    public class RemoveDataset : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string DatasetFamilyId { get; set; }
        protected override void BeginProcessing()
        {            
            Sdk.DeleteDataset(GetWorkspaceSetting(), DatasetFamilyId);
            WriteObject("Dataset removed.");
        }       
    }

   
}