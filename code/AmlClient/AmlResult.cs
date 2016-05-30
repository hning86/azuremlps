using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;

namespace AzureMachineLearning
{
    public sealed class AmlResult
    {
        public bool IsSuccess { get; set; }
        public int Status { get; set; }
        public string Reason { get; set; }
        public Task<string> Payload { get; set; }
        public Func<Task<Stream>> GetStream { get; set; }

        public AmlResult(HttpResponseMessage m)
        {
            Status = (int)m.StatusCode;
            Payload = m.Content.ReadAsStringAsync();
            GetStream = () => m.Content.ReadAsStreamAsync();
            IsSuccess = m.IsSuccessStatusCode;
            Reason = m.ReasonPhrase;
        }

        public void ThrowIfFailed()
        {
            throw new AmlException(this);
        }
    }
}
