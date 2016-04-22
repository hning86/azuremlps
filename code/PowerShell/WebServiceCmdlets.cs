using AzureML.Contract;
using System.Management.Automation;
using System;



namespace AzureML.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlWebService")]
    public class GetWebServices : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = false)]
        public string WebServiceId { get; set; }        

        protected override void ProcessRecord()
        {            
            if (string.IsNullOrEmpty(WebServiceId))
            {
                WebService[] wss = Sdk.GetWebServicesInWorkspace(GetWorkspaceSetting());
                WriteObject(wss, true);
            }
            else
            {
                WebService ws = Sdk.GetWebServicesById(GetWorkspaceSetting(), WebServiceId);
                WriteObject(ws);
            }
        }
    }

    [Cmdlet(VerbsCommon.New, "AmlWebService")]
    public class NewWebService : AzureMLPsCmdlet
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
            Experiment exp = Sdk.GetExperimentById(GetWorkspaceSetting(), PredictiveExperimentId, out rawJson);

            pr.StatusDescription += exp.Description;
            pr.CurrentOperation = "Deploying web service";
            pr.PercentComplete = 2;
            WriteProgress(pr);
            WebServiceCreationStatus status = Sdk.DeployWebServiceFromPredictiveExperiment(GetWorkspaceSetting(), PredictiveExperimentId);

            while (status.Status != "Completed")
            {
                if (pr.PercentComplete == 100)
                    pr.PercentComplete = 1;
                pr.PercentComplete++;
                WriteProgress(pr);
                status = Sdk.GetWebServiceCreationStatus(GetWorkspaceSetting(), status.ActivityId);
                if (status.Status == "Failed")
                    throw new Exception("Failed to create web service. Activity Id: " + status.ActivityId);
            }
            pr.PercentComplete = 100;
            WriteProgress(pr);

            WriteObject(Sdk.GetWebServicesById(GetWorkspaceSetting(), status.WebServiceGroupId));
        }
    }

    [Cmdlet(VerbsCommon.Remove, "AmlWebService")]
    public class RemoveWebService : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        protected override void ProcessRecord()
        {            
            WebService ws = Sdk.GetWebServicesById(GetWorkspaceSetting(), WebServiceId);
            Sdk.RemoveWebServiceById(GetWorkspaceSetting(), WebServiceId);
            WriteObject("Web service \"" + ws.Name + "\" was removed.");
        }
    }
}
