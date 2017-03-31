using AzureML.Contract;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Threading;

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
            PackingServiceActivity activity = Sdk.UnpackExperimentFromGallery(GetWorkspaceSetting(), PackageUri, GalleryUri, EntityId);
            while (activity.Status != "Complete")
            {
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                pr.StatusDescription = "Status: " + activity.Status;
                WriteProgress(pr);
                activity = Sdk.GetActivityStatus(GetWorkspaceSetting(), activity.ActivityId, false);
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
        [Parameter(Mandatory = true, ParameterSetName = "Copy Within the Current Workspace")]
        [Parameter(Mandatory = true, ParameterSetName = "Copy across Workspaces")]
        public string ExperimentId { get; set; }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "Copy across Workspaces")]
        [ValidateSet("South Central US", "West Europe", "Southeast Asia", "Japan East", "West Central US")]
        public string DestinationLocation { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Copy across Workspaces")]
        public string DestinationWorkspaceId { get; set; }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "Copy across Workspaces")]
        public string DestinationWorkspaceAuthorizationToken { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "Copy Within the Current Workspace")]
        public string NewExperimentName { get; set; }
        protected override void ProcessRecord()
        {
            WorkspaceSetting sourceWSSetting = GetWorkspaceSetting();

            if (string.IsNullOrEmpty(DestinationWorkspaceId) || sourceWSSetting.WorkspaceId.ToLower() == DestinationWorkspaceId.ToLower())
            {
                // copying in the same workspace
                ProgressRecord pr = new ProgressRecord(1, "Copy Experiment", "Experiment Name:");
                pr.CurrentOperation = "Getting experiment...";
                pr.PercentComplete = 1;
                WriteProgress(pr);

                string rawJson = string.Empty;
                Experiment exp = Sdk.GetExperimentById(sourceWSSetting, ExperimentId, out rawJson);

                pr.StatusDescription = "Experiment Name: " + exp.Description;
                pr.CurrentOperation = "Copying...";
                pr.PercentComplete = 2;
                WriteProgress(pr);
                Sdk.SaveExperimentAs(sourceWSSetting, exp, rawJson, NewExperimentName);
                pr.PercentComplete = 100;
                WriteProgress(pr);
                WriteObject("A copy of experiment \"" + exp.Description + "\"is created in the current workspace.");
            }
            else
            {
                if (!string.IsNullOrEmpty(NewExperimentName))
                    WriteWarning("New name is ignored when copying Experiment across Workspaces.");

                // if no destination location is set, use the one the current workspace is in.
                if (string.IsNullOrEmpty(DestinationLocation))
                    DestinationLocation = sourceWSSetting.Location;

                var destWSSetting = GetWorkspaceSetting(DestinationLocation, DestinationWorkspaceId, DestinationWorkspaceAuthorizationToken);

                var sourceWS = Sdk.GetWorkspaceFromAmlRP(sourceWSSetting);
                var destWS = Sdk.GetWorkspaceFromAmlRP(destWSSetting);

                // copying across workspaces
                ProgressRecord pr = new ProgressRecord(1, "Copy Experiment", "Experiment Name:");
                pr.CurrentOperation = "Getting experiment...";
                pr.PercentComplete = 1;
                WriteProgress(pr);

                string rawJson = string.Empty;
                Experiment exp = Sdk.GetExperimentById(sourceWSSetting, ExperimentId, out rawJson);

                pr.StatusDescription = "Experiment Name: " + exp.Description;
                pr.CurrentOperation = "Packing experiment from source workspace to storage...";
                pr.PercentComplete = 2;
                WriteProgress(pr);
                PackingServiceActivity activity = Sdk.PackExperiment(sourceWSSetting, ExperimentId);

                pr.CurrentOperation = "Packing experiment from source workspace to storage...";
                pr.PercentComplete = 3;
                WriteProgress(pr);
                activity = Sdk.GetActivityStatus(sourceWSSetting, activity.ActivityId, true);
                while (activity.Status != "Complete")
                {
                    if (pr.PercentComplete < 100)
                        pr.PercentComplete++;
                    else
                        pr.PercentComplete = 1;
                    WriteProgress(pr);
                    activity = Sdk.GetActivityStatus(sourceWSSetting, activity.ActivityId, true);
                }

                pr.CurrentOperation = "Unpacking experiment from storage to destination workspace...";
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                WriteProgress(pr);

                activity = Sdk.UnpackExperiment(destWSSetting, activity.Location, Location);
                while (activity.Status != "Complete")
                {
                    if (pr.PercentComplete < 100)
                        pr.PercentComplete++;
                    else
                        pr.PercentComplete = 1;
                    WriteProgress(pr);
                    activity = Sdk.GetActivityStatus(destWSSetting, activity.ActivityId, false);
                }
                pr.PercentComplete = 100;
                WriteProgress(pr);
                WriteObject(string.Format("Experiment \"{0}\" has been successfully copied from workspace \"{1}\" to \"{2}\".", exp.Description, sourceWS.FriendlyName, destWS.FriendlyName));
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

    [Cmdlet("Get", "AmlExperimentNode")]
    public class GetExperimentNode : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        [Parameter(Mandatory = true)]
        public string Comment { get; set; }
        protected override void ProcessRecord()
        {
            string rawJson = string.Empty;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic graph = jss.Deserialize<object>(rawJson);
            List<GraphNode> nodes = new List<GraphNode>();
            foreach (var node in graph["Graph"]["ModuleNodes"])
            {
                GraphNode gn = new GraphNode
                {
                    Id = node["Id"],
                    ModuleId = node["ModuleId"],
                    Comment = node["Comment"]
                };
                if (gn.Comment.ToLower().Trim() == Comment.ToLower().Trim()) nodes.Add(gn);
            }
            WriteObject(nodes, true);
        }
    }

    [Cmdlet("Download", "AmlExperimentNodeOutput")]
    public class DownloadExperimentNodeOutput : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        [Parameter(Mandatory = true)]
        public string NodeId { get; set; }        
        [Parameter(Mandatory = true)]
        public string OutputPortName { get; set; }
        [Parameter(Mandatory = true)]
        public string DownloadFileName { get; set; }
        [Parameter(Mandatory = false)]
        [ValidateSet("Payload", "Visualization")]
        public string OutputType { get; set; }
        protected override void ProcessRecord()
        {            
            string rawJson = string.Empty;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            dynamic graph = jss.Deserialize<object>(rawJson);
            List<GraphNode> nodes = new List<GraphNode>();
            bool foundNode = false;
            bool foundPort = false;
            foreach (var node in graph["NodeStatuses"])
                if (string.Compare(node["NodeId"], NodeId, true) == 0)
                {
                    foundNode = true;
                    if (string.IsNullOrEmpty(OutputType) || OutputType == "Payload")
                    {
                        foreach (var port in node["OutputEndpoints"])
                            if (string.Compare(port["Name"], OutputPortName, true) == 0)
                            {
                                foundPort = true;
                                if (File.Exists(DownloadFileName))
                                    throw new Exception(DownloadFileName + " aleady exists.");

                                ProgressRecord pr = new ProgressRecord(1, "Download file", string.Format("Download file \"{0}\" from Azure ML Studio", DownloadFileName));
                                pr.PercentComplete = 1;
                                pr.CurrentOperation = "Downloading...";
                                WriteProgress(pr);

                                string sasUrl = port["BaseUri"] + port["Location"] + port["AccessCredential"];
                                Task task = Sdk.DownloadFileAsync(sasUrl, DownloadFileName);
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

                                WriteObject(DownloadFileName + " is downloaded successfully.");
                                return;
                            }
                    }
                    else if (OutputType == "Visualization")
                    {                        
                        foreach (var port in node["MetadataOutputEndpoints"])
                        {
                            if (string.Compare(port.Key, OutputPortName, true) == 0)
                            {
                                foundPort = true;
                                foreach (var subNode in port.Value)
                                {
                                    if (subNode["Key"] == "visualization")
                                    {
                                        var subNodeValues = subNode["Value"];
                                        string sasUrl = subNodeValues["BaseUri"] + subNodeValues["Location"] + subNodeValues["AccessCredential"];
                                        Task task = Sdk.DownloadFileAsync(sasUrl, DownloadFileName);

                                        if (File.Exists(DownloadFileName))
                                            throw new Exception(DownloadFileName + " aleady exists.");

                                        ProgressRecord pr = new ProgressRecord(1, "Download file", string.Format("Download file \"{0}\" from Azure ML Studio", DownloadFileName));
                                        pr.PercentComplete = 1;
                                        pr.CurrentOperation = "Downloading...";
                                        WriteProgress(pr);

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

                                        WriteObject(DownloadFileName + " is downloaded successfully.");
                                        return;
                                    }
                                }                                
                            }                            
                        }
                    }
                }
            if (!foundNode)
                throw new Exception("Node not found! Please make sure the node exists, and you have run the experiment at least once.");
            if (!foundPort)
                throw new Exception("Port not found! Please make sure you supplied the correct port name.");
        }
    }

    [Cmdlet("Layout", "AmlExperiment")]
    public class LayoutExperiment : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }

        protected override void ProcessRecord()
        {
            string rawJson = string.Empty;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            rawJson = Sdk.AutoLayoutGraph(rawJson);
            MemoryStream ms = new MemoryStream(UnicodeEncoding.Unicode.GetBytes(rawJson));
            ser = new DataContractJsonSerializer(typeof(Experiment));
            exp = (Experiment)ser.ReadObject(ms);
            Sdk.SaveExperiment(GetWorkspaceSetting(), exp, rawJson);
            WriteObject("Graph auto-layout finished.");
        }
    }

    [Cmdlet("Export", "AmlWebServiceDefinitionFromExperiment")]
    public class ExportAmlWebServiceDefinitionFromExperiment : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        [Parameter(Mandatory = true)]
        public string OutputFile { get; set; }
        protected override void ProcessRecord()
        {
            string rawJson = string.Empty;
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), ExperimentId, out rawJson);
            if (exp.Status.StatusCode != "Finished")
              throw new Exception("Experiment must be in Finished state. The current state is: " + exp.Status.StatusCode);
            else
            {
                string output = Sdk.ExportAmlWebServiceDefinitionFromExperiment(GetWorkspaceSetting(), ExperimentId);
                File.WriteAllText(OutputFile, output);
                WriteObject(string.Format("Experiment graph exported to file \"{0}\"", OutputFile));
            }
        }
    }
}