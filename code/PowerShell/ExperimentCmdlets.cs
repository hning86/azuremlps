using AzureML.Contract;
using System.IO;
using System.Management.Automation;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AzureML.PowerShell
{    
    [Cmdlet(VerbsCommon.Remove, "AmlExperiment")]
    public class RemoveExperiment : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        
        protected override void ProcessRecord()
        {            
            Sdk.RemoveExperimentById(GetWorkspaceSetting(), ExperimentId);
            WriteObject("Experiment removed.");
        }
    }
   
    // Note this Commandlet users an unsupported API that might break in the future!
    [Cmdlet(VerbsCommon.Copy, "AmlExperimentFromGallery")]
    public class CopyExperimentFromGallery : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string PackageUri;
        [Parameter(Mandatory = true)]
        public string GalleryUri;
        [Parameter(Mandatory = true)]
        public string EntityId;

        protected override void ProcessRecord()
        {
            WriteWarning("Note this Commandlet uses an unsupported API that might break in the future!");
            ProgressRecord pr = new ProgressRecord(1, "Copy from Gallery", "Gallery Experiment");
            pr.PercentComplete = 1;
            pr.CurrentOperation = "Unpacking experiment from Gallery to workspace...";
            WriteProgress(pr);
            PackingServiceActivity activity = Sdk.UnpackExperimentFromGallery_UnsupportedAPI(GetWorkspaceSetting(), PackageUri, GalleryUri, EntityId);
            while (activity.Status != "Complete")
            {
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;                
                pr.StatusDescription = "Status: " + activity.Status;
                WriteProgress(pr);
                activity = Sdk.GetActivityStatus(GetWorkspaceSetting(), WorkspaceId, AuthorizationToken, activity.ActivityId, false);
            }
            pr.StatusDescription = "Status: " + activity.Status;
            pr.PercentComplete = 100;
            WriteProgress(pr);
            WriteObject("Experiment copied from Gallery.");
        }
    }

    [Cmdlet(VerbsCommon.Copy, "AmlExperiment")]
    public class CopyExperiment : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        [Parameter(Position = 0, Mandatory = false)]
        public string DestinationWorkspaceId { get; set; }
        [Parameter(Position = 1, Mandatory = false)]
        public string DestinationWorkspaceAuthorizationToken { get; set; }
        [Parameter(Mandatory = false)]
        public string NewExperimentName { get; set; }
        protected  override void ProcessRecord()
        {
            WorkspaceSetting ws = GetWorkspaceSetting();
            if (string.IsNullOrEmpty(DestinationWorkspaceId) || ws.WorkspaceId.ToLower() == DestinationWorkspaceId.ToLower())
            {
                // copying in the same workspace
                ProgressRecord pr = new ProgressRecord(1, "Copy Experiment", "Experiment Name:");
                pr.CurrentOperation = "Getting experiment...";
                pr.PercentComplete = 1;
                WriteProgress(pr);

                string rawJson = string.Empty;
                Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);

                pr.StatusDescription = "Experiment Name: " + exp.Description;
                pr.CurrentOperation = "Copying...";
                pr.PercentComplete = 2;
                WriteProgress(pr);
                Sdk.SaveExperimentAs(GetWorkspaceSetting(), exp, rawJson, NewExperimentName);
                pr.PercentComplete = 100;
                WriteProgress(pr);
                WriteObject("Experiment \"" + exp.Description + "\" copied from within the current workspace.");
            }
            else
            {
                if (!string.IsNullOrEmpty(NewExperimentName))
                    WriteWarning("New name is ignored when copying Experiment across Workspaces.");
                // copying across workspaces
                ProgressRecord pr = new ProgressRecord(1, "Copy Experiment", "Experiment Name:");
                pr.CurrentOperation = "Getting experiment...";
                pr.PercentComplete = 1;
                WriteProgress(pr);

                string rawJson = string.Empty;
                Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);

                pr.StatusDescription = "Experiment Name: " + exp.Description;
                pr.CurrentOperation = "Packing experiment from source workspace to storage...";
                pr.PercentComplete = 2;
                WriteProgress(pr);
                PackingServiceActivity activity = Sdk.PackExperiment(GetWorkspaceSetting(), ExperimentId);

                pr.CurrentOperation = "Packing experiment from source workspace to storage...";
                pr.PercentComplete = 3;
                WriteProgress(pr);
                activity = Sdk.GetActivityStatus(GetWorkspaceSetting(), activity.ActivityId, true);
                while (activity.Status != "Complete")
                {
                    if (pr.PercentComplete < 100)
                        pr.PercentComplete++;
                    else
                        pr.PercentComplete = 1;
                    WriteProgress(pr);
                    activity = Sdk.GetActivityStatus(GetWorkspaceSetting(), activity.ActivityId, true);
                }

                pr.CurrentOperation = "Unpacking experiment from storage to destination workspace...";
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                WriteProgress(pr);
                activity = Sdk.UnpackExperiment(GetWorkspaceSetting(), DestinationWorkspaceId, DestinationWorkspaceAuthorizationToken, activity.Location);
                while (activity.Status != "Complete")
                {
                    if (pr.PercentComplete < 100)
                        pr.PercentComplete++;
                    else
                        pr.PercentComplete = 1;
                    WriteProgress(pr);
                    activity = Sdk.GetActivityStatus(GetWorkspaceSetting(), DestinationWorkspaceId, DestinationWorkspaceAuthorizationToken, activity.ActivityId, false);
                }
                pr.PercentComplete = 100;
                WriteProgress(pr);
                WriteObject("Experiment \"" + exp.Description + "\" copied over from another workspace.");
            }         
        }
    }

    [Cmdlet("Export", "AmlExperimentGraph")]
    public class ExportExperimentGraph : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        [Parameter(Mandatory = true)]
        public string OutputFile { get; set; }
        protected override void ProcessRecord()
        {            
            string rawJson = string.Empty;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            File.WriteAllText(OutputFile, rawJson);
            WriteObject(string.Format("Experiment graph exported to file \"{0}\"", OutputFile));
        }
    }

    [Cmdlet("Import", "AmlExperimentGraph")]
    public class ImportExperimentGraph : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string InputFile { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Overwrite { get; set; }
        [Parameter(Mandatory = false)]
        public string NewName { get; set; }
        protected override void ProcessRecord()
        {
            if (Overwrite.IsPresent && !string.IsNullOrEmpty(NewName))
                WriteWarning("Since you specified Overwrite, the new name supplied will be igored.");
            string rawJson = File.ReadAllText(InputFile);
            MemoryStream ms = new MemoryStream(UnicodeEncoding.Unicode.GetBytes(rawJson));
            ser = new DataContractJsonSerializer(typeof(Experiment));
            Experiment exp = (Experiment)ser.ReadObject(ms);
            if (Overwrite)
                Sdk.SaveExperiment(GetWorkspaceSetting(), exp, rawJson);
            else
                Sdk.SaveExperimentAs(GetWorkspaceSetting(), exp, rawJson, string.IsNullOrEmpty(NewName) ? exp.Description : NewName);

            WriteObject(string.Format("File \"{0}\" imported as an Experiment graph.", InputFile));
        }
    }

    [Cmdlet("Start", "AmlExperiment")]
    public class StartExperiment : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        protected override void ProcessRecord()
        {
            string rawJson = string.Empty;
            ProgressRecord progress = new ProgressRecord(1, "Start Experiment", "Experiment Name:");
            progress.CurrentOperation = "Getting experiment graph...";

            progress.PercentComplete = 1;
            WriteProgress(progress);
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            progress.StatusDescription = "Experiment Name: " + exp.Description;

            progress.CurrentOperation = "Saving experiment...";
            progress.PercentComplete = 2;
            WriteProgress(progress);
            Sdk.SaveExperiment(GetWorkspaceSetting(), exp, rawJson);

            progress.CurrentOperation = "Submitting experiment to run...";
            progress.PercentComplete = 3;
            WriteProgress(progress);
            Sdk.RunExperiment(GetWorkspaceSetting(), exp, rawJson);

            progress.CurrentOperation = "Getting experiment status...";
            progress.PercentComplete = 4;
            WriteProgress(progress);
            exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            int percentage = 5;
            while (exp.Status.StatusCode != "Finished" && exp.Status.StatusCode != "Failed")
            {
                exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
                progress.CurrentOperation = "Experiment Status: " + exp.Status.StatusCode;
                percentage++;
                // reset the percentage count if it reaches 100 and execution is still in progress.
                if (percentage > 100) percentage = 1;
                progress.PercentComplete = percentage;
                WriteProgress(progress);
            }

            progress.PercentComplete = 100;
            WriteProgress(progress);

            WriteObject(string.Format("Experiment \"{0}\" status: ", exp.Description) + exp.Status.StatusCode);
        }
    }

    [Cmdlet(VerbsCommon.Get, "AmlExperiment")]
    public class GetExperiment : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = false)]
        public string ExperimentId { get; set; }
        public GetExperiment() { }

        protected override void ProcessRecord()
        {           
            if (string.IsNullOrEmpty(ExperimentId))
            {
                // get all experiments in the workspace
                Experiment[] exps = Sdk.GetExperiments(GetWorkspaceSetting());
                WriteObject(exps, true);
            }
            else
            {
                // get a specific experiment
                string rawJson = string.Empty;
                string errorMsg = string.Empty;
                Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
                WriteObject(exp);
            }
        }        
    }    
}