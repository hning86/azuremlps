using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMachineLearning
{
    public class BatchExecution
    {
        public WorkspaceClient Workspace { get; private set; }


        //public BatchExecution(WorkspaceClient workspace)
        //{
        //    this.Workspace = workspace;
        //}

        //public string SubmitBESJob(string submitJobRequestUrl, string apiKey, string jobConfig)
        //{
        //    Util.AuthorizationToken = apiKey;
        //    AmlResult hr = Util.HttpPost(submitJobRequestUrl, jobConfig).Result;
        //    if (!hr.IsSuccess)
        //        throw new Exception(hr.Payload);

        //    string jobId = hr.Payload.Replace("\"", "");
        //    return jobId;
        //}

        //public void StartBESJob(string submitJobRequestUrl, string apiKey, string jobId)
        //{
        //    Util.AuthorizationToken = apiKey;
        //    string startJobApiLocation = submitJobRequestUrl.Replace("jobs?api-version=2.0", "jobs/" + jobId + "/start?api-version=2.0");
        //    AmlResult hr = Util.HttpPost(startJobApiLocation, string.Empty).Result;
        //    if (!hr.IsSuccess)
        //        throw new Exception(hr.Payload);

        //}

        //public string GetBESJobStatus(string submitJobRequestUrl, string apiKey, string jobId, out string results)
        //{
        //    Util.AuthorizationToken = apiKey;
        //    string getJobStatusApiLocation = submitJobRequestUrl.Replace("jobs?api-version=2.0", "jobs/" + jobId + "?api-version=2.0");
        //    JavaScriptSerializer jss = new JavaScriptSerializer();
        //    AmlResult hr = Util.HttpGet(getJobStatusApiLocation).Result;
        //    if (!hr.IsSuccess)
        //        throw new Exception(hr.Payload);
        //    dynamic parsed = jss.Deserialize<object>(hr.Payload);
        //    string jobStatus = parsed["StatusCode"];
        //    results = hr.Payload;
        //    return jobStatus;
        //}
    }
}
