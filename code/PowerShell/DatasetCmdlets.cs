using AzureML.Contract;
using System.IO;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AzureML.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlDataset")]
    public class GetDatasetCmdlet : AzureMLPsCmdlet
    {
        protected override void ProcessRecord()
        {            
            Dataset[] datasets = Sdk.GetDataset(GetWorkspaceSetting());
            WriteObject(datasets, true);
        }
    }

    [Cmdlet("Upload", "AmlDataset")]
    public class UploadDatasetCmdlet: AzureMLPsCmdlet
    {
        [Parameter(Mandatory =false)]
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
           Task<string> uploadTask = Sdk.UploadDatasetAsnyc(GetWorkspaceSetting(), FileFormat, UploadFileName);
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
    public class DownloadDatasetCmdlet : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "Dataset Id")]
        public string DatasetId { get; set; }
        [Parameter(Mandatory = true)]
        public string DownloadFileName { get; set; }

        protected override void ProcessRecord()
        {
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
    public class RemoveDatasetCmdlet : AzureMLPsCmdlet
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