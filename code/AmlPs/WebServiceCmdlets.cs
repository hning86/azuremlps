using System.Management.Automation;



namespace AzureMachineLearning.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlWebService")]
    public class GetWebServices : AmlCmdlet
    {
        [Parameter(Mandatory = false)]
        public string WebServiceId { get; set; }        

        protected override void ProcessRecord()
        {            
            if (string.IsNullOrEmpty(WebServiceId))
            {
                WebService[] wss = Workspace.GetWebServicesInWorkspace(GetWorkspaceSetting());
                WriteObject(wss, true);
            }
            else
            {
                WebService ws = Workspace.GetWebServicesById(GetWorkspaceSetting(), WebServiceId);
                WriteObject(ws);
            }
        }
    }

    [Cmdlet(VerbsCommon.New, "AmlWebService")]
    public class NewWebService : AmlCmdlet
    {
        [Parameter(Mandatory = true)]
        public string PredictiveExperimentId { get; set; }
        protected override void ProcessRecord()
        {            
            ProgressRecord pr = new ProgressRecord(1, "Deploy Web Service", "Predictive Experiment Name: ");

            pr.CurrentOperation = "Getting Predictive Experiment...";
            pr.PercentComplete = 1;
            WriteProgress(pr);
            string rawJson = string.Empty;
            Experiment exp = Workspace.GetExperimentById(GetWorkspaceSetting(), PredictiveExperimentId, out rawJson);

            pr.StatusDescription += exp.Description;
            pr.CurrentOperation = "Deploying web service";
            pr.PercentComplete = 2;
            WriteProgress(pr);
            DeployStatus status = Workspace.DeployWebServiceFromPredictiveExperiment(GetWorkspaceSetting(), PredictiveExperimentId);

            while (status.Status != "Completed")
            {
                if (pr.PercentComplete == 100)
                    pr.PercentComplete = 1;
                pr.PercentComplete++;
                WriteProgress(pr);
                status = Workspace.GetWebServiceCreationStatus(GetWorkspaceSetting(), status.ActivityId);
            }
            pr.PercentComplete = 100;
            WriteProgress(pr);

            WriteObject(Workspace.GetWebServicesById(GetWorkspaceSetting(), status.WebServiceGroupId));
        }
    }

    [Cmdlet(VerbsCommon.Remove, "AmlWebService")]
    public class RemoveWebService : AmlCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        protected override void ProcessRecord()
        {            
            WebService ws = Workspace.GetWebServicesById(GetWorkspaceSetting(), WebServiceId);
            Workspace.RemoveWebServiceById(GetWorkspaceSetting(), WebServiceId);
            WriteObject("Web service \"" + ws.Name + "\" was removed.");
        }
    }
}
