using System.Net.Http;

namespace NDocRaptor {
    public sealed class DocRaptorResponse {
        public int NumberOfPages { get; internal set; }
        public HttpResponseMessage Response { get; private set; }

        public bool Success {
            get {
                return Response.IsSuccessStatusCode;
            }
        }

        public string ReasonPhrase {
            get {
                return Response.ReasonPhrase;
            }
        }

        internal DocRaptorResponse(HttpResponseMessage response) {
            Response = response;
        }
    }
}