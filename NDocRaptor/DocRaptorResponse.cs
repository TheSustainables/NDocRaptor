using System.IO;
using System.Net.Http;

namespace NDocRaptor {
    public sealed class DocRaptorResponse {
        /// <summary>
        /// The X-DocRaptor-Num-Pages header, when using PDF.
        /// </summary>
        /// <returns></returns>
        public int NumberOfPages { get; internal set; }

        /// <summary>
        /// The HTTP response
        /// </summary>
        public HttpResponseMessage Response { get; private set; }

        /// <summary>
        /// Whether the request returned 200 - OK
        /// </summary>
        public bool Success {
            get {
                return Response.IsSuccessStatusCode;
            }
        }

        /// <summary>
        /// The HTTP status code phrase
        /// </summary>
        public string ReasonPhrase {
            get {
                return Response.ReasonPhrase;
            }
        }

        /// <summary>
        /// Save to content to a file
        /// </summary>
        /// <param name="path"></param>
        public void SaveAs(string path) {
            using (var fileStream = new FileStream(path, FileMode.Create)) {
                Response.Content.CopyToAsync(fileStream).Wait();
            }
        }

        internal DocRaptorResponse(HttpResponseMessage response) {
            Response = response;
        }
    }
}