using AzureML.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Web.Script.Serialization;

namespace AzureML.PowerShell
{
    public class InvokeWebServiceEndpointCmdlet : AzureMLPsCmdletBase
    {
        private string _apiKey = string.Empty;
        [Parameter(Mandatory = true)]
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                _apiKey = value;
                
            }
        }
    }

    [Cmdlet("Invoke", "AmlWebServiceRRSEndpoint")]
    public class InvokeWebServiceRRSEndpoint : InvokeWebServiceEndpointCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ApiLocation { get; set; }
        [Parameter(Mandatory = false)]
        public string InputJsonText { get; set; }
        [Parameter(Mandatory = false)]
        public string InputJsonFile { get; set; }

        [Parameter(Mandatory = false)]
        public string OutputJsonFile { get; set; }

        protected override void ProcessRecord()
        {
            string input = InputJsonText;
            if (InputJsonFile != null && InputJsonFile != string.Empty)
                input = File.ReadAllText(InputJsonFile);

            string output = Sdk.InvokeRRS(ApiLocation + "/execute?api-version=2.0&details=true", ApiKey, input);

            if (OutputJsonFile != null && OutputJsonFile != string.Empty)
                File.WriteAllText(OutputJsonFile, output);
            else
                WriteObject(output);
        }
    }

    [Cmdlet("Invoke", "AmlWebServiceBESEndpoint")]
    public class InvokeWebServiceBESEndpoint : InvokeWebServiceEndpointCmdlet
    {
        [Parameter(Mandatory = true)]
        public string SubmitJobRequestUrl { get; set; }
        [Parameter(Mandatory = false)]
        public string JobConfigFile { get; set; }
        [Parameter(Mandatory = false)]
        public string JobConfigString { get; set; }

        protected override void ProcessRecord()
        {
            if (JobConfigString == null || JobConfigString == string.Empty)
                JobConfigString = File.ReadAllText(JobConfigFile);
            ProgressRecord pr = new ProgressRecord(1, "Batch Execution Service", "Run Azure ML BES Job");

            // Submit the job
            pr.CurrentOperation = "Submitting the job...";
            pr.PercentComplete = 1;
            WriteProgress(pr);
            string jobId = Sdk.SubmitBESJob(SubmitJobRequestUrl, ApiKey, JobConfigString);
            pr.CurrentOperation = "Starting the job...";
            pr.PercentComplete = 2;
            WriteProgress(pr);
            pr.StatusDescription += ": " + jobId;
            Sdk.StartBESJob(SubmitJobRequestUrl, ApiKey, jobId);

            // Query job status
            pr.CurrentOperation = "Getting job status...";
            pr.PercentComplete = 3;
            WriteProgress(pr);

            string jobStatus = "Job Status: NotStarted";
            string outputMsg = string.Empty;
            while (true)
            {
                jobStatus = Sdk.GetBESJobStatus(SubmitJobRequestUrl, ApiKey, jobId, out outputMsg);
                pr.CurrentOperation = "Job Status: " + jobStatus;
                WriteProgress(pr);

                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                if (jobStatus == "Failed" || jobStatus == "Canceled" || jobStatus == "Finished")
                    break;
            }            
            WriteObject(outputMsg);
        }
    }
}