using AzureML.Contract;
using AzureML.PowerShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AzureMLPS.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlModule")]
    public class GetModule : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = false)]
        // switch, when set, to show custom modules only
        public SwitchParameter Custom { get; set; }
        protected override void BeginProcessing()
        {
            Module[] modules = Sdk.GetModules(GetWorkspaceSetting());
            if (Custom.IsPresent)
                modules = modules.Where(m => m.ReleaseState == "Custom").ToArray();
            WriteObject(modules, true);
        }
    }

    [Cmdlet(VerbsCommon.New, "AmlCustomModule")]
    public class NewCustomModule : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string CustomModuleZipFileName { get; set; }
        protected override void BeginProcessing()
        {
            ProgressRecord pr = new ProgressRecord(1, "Create Custom Module", string.Format("Upload custom module ZIP file \"{0}\" into Azure ML Studio", CustomModuleZipFileName));
            pr.PercentComplete = 1;
            pr.CurrentOperation = "Uploading custom module ZIP file...";
            WriteProgress(pr);
            string uploadedResourceMetadata = Sdk.UploadResource(GetWorkspaceSetting(), "Zip");
            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic m = jss.Deserialize<object>(uploadedResourceMetadata);
            string uploadId = m["Id"];
            Task<string> task = Sdk.UploadResourceInChunksAsnyc(GetWorkspaceSetting(), 1, 0, uploadId, CustomModuleZipFileName, "Zip");
            while (!task.IsCompleted)
            {
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                Thread.Sleep(500);
                WriteProgress(pr);
            }
            string uploadMetadata = task.Result;
            string activityId = Sdk.BeginParseCustomModuleJob(GetWorkspaceSetting(), uploadMetadata);

            pr.CurrentOperation = "Creating custom module...";
            WriteProgress(pr);
            dynamic statusObj = jss.Deserialize<object>(Sdk.GetCustomModuleBuildJobStatus(GetWorkspaceSetting(), activityId));
            string jobStatus = statusObj[0];
            while (jobStatus == "Pending")
            {
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                statusObj = jss.Deserialize<object>(Sdk.GetCustomModuleBuildJobStatus(GetWorkspaceSetting(), activityId));
                jobStatus = statusObj[0].ToString();
                Thread.Sleep(500);
                WriteProgress(pr);
            }

            pr.PercentComplete = 100;
            WriteProgress(pr);

            if (jobStatus == "Finished")
            {
                string moduleId = statusObj[1];
                WriteObject(moduleId);
            }
            else
                throw new System.Exception("Custom module upload failed: " + statusObj[1]);
        }
    }
}
