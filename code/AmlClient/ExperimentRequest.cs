using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureMachineLearning
{
    public abstract class ExperimentRequest
    {
        /// <value>description of the experiment (name)</value>
        public string Description { get; set; }
        /// <value>summary of the experiment</value>
        public string Summary { get; set; }
        /// <value>false to run, true to just save</value>
        public bool IsDraft { get; set; }
        /// <value>null for a new or id existing experiment otherwise </value>
        public string ParentExperimentId { get; set; }
        /// <value></value>
        public bool DisableNodesUpdate { get; set; }
        /// <value>"user"</value>
        public string Category { get; set; }
        /// <value></value>
        public string ExperimentGraph { get; set; }
        /// <value>web service info for a scoring graph</value>
        public string WebService { get; set; }

        /// <value>Etag provided by previous Get</value>
        [JsonIgnore]
        public string Etag { get; protected set; }
    }

    /// <summary>
    /// To save a draft of a new experiment, use
    /// IsDraft = true and
    /// ParentExperimentId = null.
    /// 
    /// The experiment id and etag will be returned.
    /// </summary>
    public sealed class NewExperimentRequest : ExperimentRequest
    {
        public NewExperimentRequest(Experiment e)
        {
            IsDraft = true;
            ParentExperimentId = null;

            // fill in field values
            Description = e.Description;
            Summary = e.Summary;
            Category = e.Category;
            ExperimentGraph = e.Graph;
        }
    }

    /// <summary>
    /// To save a draft of an existing experiment,
    /// use IsDraft = true and 
    /// ParentExperimentId = {experiment id}
    /// 
    /// The If-Match header of this request must contain
    /// the etag of the current version of the experiment.
    /// </summary>
    public sealed class UpdateExperimentRequest : ExperimentRequest
    {
        public UpdateExperimentRequest(Experiment e)
        {
            IsDraft = true;
            ParentExperimentId = e.ExperimentId;

            Description = e.Description;
            Summary = e.Summary;
            DisableNodesUpdate = e.DisableNodesUpdate;
            Category = e.Category;
            ExperimentGraph = e.Graph;
            WebService = e.WebService;
            Etag = e.Etag;
        }
    }

    public sealed class RunExperimentRequest : ExperimentRequest
    {
        public RunExperimentRequest(Experiment e)
        {
            IsDraft = false;
            ParentExperimentId = e.ExperimentId;
            Etag = e.Etag;

            // TODO: don't know if we need the other fields
        }
    }
}
