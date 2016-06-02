using AzureML.Contract;
using System;
using System.IO;
using System.Management.Automation;

namespace AzureML.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlWebServiceEndpoint")]
    public class GetWebServiceEndpoint : AzureMLPsCmdlet
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
                WebServiceEndPoint[] weps = Sdk.GetWebServiceEndpoints(GetWorkspaceSetting(), WebServiceId);
                WriteObject(weps, true);
            }
            else
            {
                WebServiceEndPoint wep = Sdk.GetWebServiceEndpointByName(GetWorkspaceSetting(), WebServiceId, EndpointName);
                WriteObject(wep);
            }
        }
    }

    [Cmdlet(VerbsCommon.Remove, "AmlWebServiceEndpoint")]
    public class RemoveWebServiceEndpoint : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        [Parameter(Mandatory = true)]
        public string EndpointName { get; set; }
        protected override void ProcessRecord()
        {            
            string rawOutcome = string.Empty;
            Sdk.RemoveWebServiceEndpoint(GetWorkspaceSetting(), WebServiceId, EndpointName);
            WriteObject(string.Format("Web service endpoint \"{0}\" was successfully removed.", EndpointName));
        }
    }

    [Cmdlet(VerbsCommon.Add, "AmlWebServiceEndpoint")]
    public class AddWebServiceEndpoint : AzureMLPsCmdlet
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
        [Parameter(Mandatory = false)]
        public SwitchParameter PreventUpdate { get; set; }
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
            AddWebServiceEndpointRequest req = new AddWebServiceEndpointRequest
            {
                WebServiceId = WebServiceId,
                EndpointName = EndpointName,
                Description = Description,
                ThrottleLevel = ThrottleLevel,
                MaxConcurrentCalls = MaxConcurrentCalls,
                PreventUpdate = PreventUpdate.IsPresent
            };                        
                
            Sdk.AddWebServiceEndpoint(GetWorkspaceSetting(), req);
            WriteObject(string.Format("Web service endpoint \"{0}\" was successfully added.", EndpointName));
        }
    }

    [Cmdlet("Refresh", "AmlWebServiceEndpoint")]
    public class RefreshWebServiceEndpoint : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WebServiceId { get; set; }
        [Parameter(Mandatory = true)]
        public string EndpointName { get; set; }
        [Parameter(Mandatory = false)]        
        public SwitchParameter OverwriteResources { get; set; }
        protected override void ProcessRecord()
        {            
            WebServiceEndPoint wse = Sdk.GetWebServiceEndpointByName(GetWorkspaceSetting(), WebServiceId, EndpointName);
            bool updated = Sdk.RefreshWebServiceEndPoint(GetWorkspaceSetting(), WebServiceId, EndpointName, OverwriteResources.ToBool());
            if (updated)
                WriteObject(string.Format("Endpoint \"{0}\" " + (OverwriteResources.IsPresent ? "and the Trained Model(s) are" : "is") + " refreshed.", wse.Name));
            else
                WriteObject(string.Format("No change detected, so endpoint \"{0}\" is NOT refreshed.", wse.Name));
        }        
    }

    [Cmdlet("Patch", "AmlWebServiceEndpoint")]
    public class PatchWebServiceEndpoint: AzureMLPsCmdlet
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
            WebService ws = Sdk.GetWebServicesById(GetWorkspaceSetting(), WebServiceId);

            pr.PercentComplete = 10;
            pr.StatusDescription = "Web Service \"" + ws.Name + "\"";
            pr.CurrentOperation = "Getting web service endpoint...";
            WriteProgress(pr);

            WebServiceEndPoint wep = Sdk.GetWebServiceEndpointByName(GetWorkspaceSetting(), WebServiceId, EndpointName);
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
            Sdk.PatchWebServiceEndpoint(GetWorkspaceSetting(), WebServiceId, EndpointName, patchReq);
            WriteObject(string.Format("Endpoint \"{0}\" resource \"{1}\" successfully patched.", wep.Name, wep.Resources[0].Name));
        }
    }
}
