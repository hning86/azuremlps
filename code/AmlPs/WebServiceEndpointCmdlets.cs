using System;
using System.IO;
using System.Management.Automation;

namespace AzureMachineLearning.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlWebServiceEndpoint")]
    public class GetWebServiceEndpoint : AmlCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        [Parameter(Mandatory = false)]
        public string EndpointName { get; set; }
        public GetWebServiceEndpoint() { }

        protected override void ProcessRecord()
        {         
            if (string.IsNullOrEmpty(EndpointName))
            {                
                Endpoint[] weps = WorkspaceEx.GetWebServiceEndpoints(GetWorkspaceSetting(), WebServiceId);
                WriteObject(weps, true);
            }
            else
            {
                Endpoint wep = WorkspaceEx.GetWebServiceEndpointByName(GetWorkspaceSetting(), WebServiceId, EndpointName);
                WriteObject(wep);
            }
        }
    }

    [Cmdlet(VerbsCommon.Remove, "AmlWebServiceEndpoint")]
    public class RemoveWebServiceEndpoint : AmlCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        [Parameter(Mandatory = true)]
        public string EndpointName { get; set; }
        protected override void ProcessRecord()
        {            
            string rawOutcome = string.Empty;
            WorkspaceEx.RemoveWebServiceEndpoint(GetWorkspaceSetting(), WebServiceId, EndpointName);
            WriteObject(string.Format("Web service endpoint \"{0}\" was successfully removed.", EndpointName));
        }
    }

    [Cmdlet(VerbsCommon.Add, "AmlWebServiceEndpoint")]
    public class AddWebServiceEndpoint : AmlCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        [Parameter(Mandatory = true)]
        public string EndpointName { get; set; }
        [Parameter(Mandatory = false)]
        public string Description { get; set; }
        [Parameter(Mandatory = false)]
        [ValidateSet("High", "Low")]
        public string ThrottleLevel { get; set; }
        [Parameter(Mandatory = false)]
        public int? MaxConcurrentCalls { get; set; }
        public AddWebServiceEndpoint()
        {
            // set default values
            ThrottleLevel = "Low";             
        }

        protected override void ProcessRecord()
        {         
            if (ThrottleLevel.ToLower() == "low" && MaxConcurrentCalls != null) //if Throttle Level is set to Low, you can't set the max concurrent call number.
            {
                WriteWarning("When ThrottleLevel is set to Low, MaxConcurrentCalls is automatically set to the default value of 4.");
                MaxConcurrentCalls = null;
            }
            AddEndpointRequest req = new AddEndpointRequest
            {
                WebServiceId = WebServiceId,
                EndpointName = EndpointName,
                Description = Description,
                ThrottleLevel = ThrottleLevel,
                MaxConcurrentCalls = MaxConcurrentCalls
            };            
            WorkspaceEx.AddWebServiceEndpoint(GetWorkspaceSetting(), req);
            WriteObject(string.Format("Web service endpoint \"{0}\" was successfully added.", EndpointName));
        }
    }

    [Cmdlet("Refresh", "AmlWebServiceEndpoint")]
    public class RefreshWebServiceEndpoint : AmlCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        [Parameter(Mandatory = true)]
        public string EndpointName { get; set; }
        [Parameter(Mandatory = false)]        
        public SwitchParameter OverwriteResources { get; set; }
        protected override void ProcessRecord()
        {            
            Endpoint wse = WorkspaceEx.GetWebServiceEndpointByName(GetWorkspaceSetting(), WebServiceId, EndpointName);
            WorkspaceEx.RefreshWebServiceEndPoint(GetWorkspaceSetting(), WebServiceId, EndpointName, OverwriteResources.ToBool());
            WriteObject(string.Format("Endpoint \"{0}\" is refreshed.", wse.Name));
        }        
    }

    [Cmdlet("Patch", "AmlWebServiceEndpoint")]
    public class PatchWebServiceEndpoint: AmlCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        [Parameter(Mandatory = true)]
        public string EndpointName { get; set; }
        //[Parameter(Mandatory = false)]
        //public string ThrottleLevel { get; set; }
        //[Parameter(Mandatory = false)]
        //public int? MaxConcurrentCalls { get; set; }
        //[Parameter(Mandatory = false)]
        //public string Description { get; set; }
        //[Parameter(Mandatory = true)]
        //[ValidateSet("None", "Error", "All")]
        //public string DiagnosticsTraceLevel { get; set; }
        [Parameter(Mandatory = true)]
        public string ResourceName { get; set; }
        [Parameter(Mandatory = true)]
        public string BaseLocation { get; set; }
        [Parameter(Mandatory = true)]
        public string RelativeLocation { get; set; }
        [Parameter(Mandatory = true)]
        public string SasBlobToken { get; set; }

        protected override void ProcessRecord()
        {            
            ProgressRecord pr = new ProgressRecord(1, "Patch Web Service Endpoint", "Web Service");
            pr.PercentComplete = 1;
            pr.CurrentOperation = "Getting web service...";
            WriteProgress(pr);
            WebService ws = WorkspaceEx.GetWebServicesById(GetWorkspaceSetting(), WebServiceId);

            pr.PercentComplete = 10;
            pr.StatusDescription = "Web Service \"" + ws.Name + "\"";
            pr.CurrentOperation = "Getting web service endpoint...";
            WriteProgress(pr);

            Endpoint wep = WorkspaceEx.GetWebServiceEndpointByName(GetWorkspaceSetting(), WebServiceId, EndpointName);
            pr.PercentComplete = 20;
            pr.StatusDescription = "Web Service \"" + ws.Name + "\", Endpoint \"" + wep.Name + "\"";
            pr.CurrentOperation = "Patching web service endpoint with new trained model...";
            WriteProgress(pr);

            dynamic patchReq = new
            {
                Resources = new[] {
                    new {
                        Name = ResourceName,
                        Location = new {
                            BaseLocation = BaseLocation,
                            RelativeLocation = RelativeLocation,
                            SasBlobToken = SasBlobToken
                        }
                    }
                }
            };
            WorkspaceEx.PatchWebServiceEndpoint(GetWorkspaceSetting(), WebServiceId, EndpointName, patchReq);
            WriteObject(string.Format("Endpoint \"{0}\" resource \"{1}\" successfully patched.", wep.Name, wep.Resources[0].Name));
        }
    }
}
