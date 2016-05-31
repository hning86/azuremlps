using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task<string> GetPayload()
        {
            var p = await Payload;
            return p;
        }

        public async Task<T> GetPayload<T>() where T : class
        {
            var p = await Payload;

            if (typeof(T) == typeof(string))
                return p as T;

            var t = JsonConvert.DeserializeObject<T>(p);
            return t;
        }
    }
}
