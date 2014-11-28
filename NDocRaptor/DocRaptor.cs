using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NDocRaptor {
    public class DocRaptor {
        private readonly HttpClient Client;
        private readonly Uri DocRaptorUrl;
        private readonly string ApiKey;
        private readonly string Tag;
        private readonly bool TestMode;

        public DocRaptor(string apiKey, bool useSsl = false, bool testMode = false, string tag = null) {
            ApiKey = apiKey;
            TestMode = testMode;
            Tag = tag;
            Client = new HttpClient();
            DocRaptorUrl = new Uri(useSsl ? "https" : "http" + "://docraptor.com/docs");
        }

        public async Task<DocRaptorResponse> CreateDocumentAsync(
            Uri url = null,
            string content = null,
            string name = "Untitled",
            string tag = null,
            DocumentType documentType = DocumentType.Pdf,
            string princeVersion = "9.0",
            bool strict = false,
            bool javascript = false,
            bool test = false,
            bool help = false) {

            var form = new Dictionary<string, string> {
                { "doc[document_type]", documentType.ToString().ToLower() },
                { "doc[test]", test.ToString().ToLower() },
                { "doc[tag]", tag },
                { "doc[strict]", strict ? "html" : "none" }
            };

            if (url != null) {
                form.Add("doc[document_url]", url.ToString());
            }
            else {
                form.Add("doc[document_content]", content);
            }

            if (!string.IsNullOrEmpty(name)) {
                form.Add("doc[name]", name);
            }

            if (help) {
                form.Add("doc[help]", "true");
            }

            if (documentType == DocumentType.Pdf) {
                form.Add("doc[prince_options][version]", princeVersion);
                form.Add("doc[javascript]", javascript.ToString().ToLower());
            }

            var requestUrl = DocRaptorUrl.ToString() + "?user_credentials=" + ApiKey;
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Content = new FormUrlEncodedContent(form);

            var responseMessage = await Client.SendAsync(request);

            return CreateResponse(responseMessage);
        }

        public async Task<DocRaptorResponse> CreatePdfDocumentAsync(
            Uri url,
            string name = "Untitled",
            string princeVersion = "9.0",
            bool strict = false,
            bool javascript = false,
            bool help = false) {

            if (url == null) {
                throw new ArgumentNullException("url");
            }

            return await CreateDocumentAsync(url, null, name, Tag, DocumentType.Pdf, princeVersion, strict, javascript, TestMode, help);
        }

        public async Task<DocRaptorResponse> CreatePdfDocumentAsync(
            string content,
            string name = "Untitled",
            string princeVersion = "9.0",
            bool strict = false,
            bool javascript = false,
            bool help = false) {

            if (content == null) {
                throw new ArgumentNullException("content");
            }

            return await CreateDocumentAsync(null, content, name, Tag, DocumentType.Pdf, princeVersion, strict, javascript, TestMode, help);
        }

        private static DocRaptorResponse CreateResponse(HttpResponseMessage responseMessage) {
            var numPagesHeader = 0;
            if (responseMessage.Headers.Contains("X-DocRaptor-Num-Pages")) {
                numPagesHeader = int.Parse(responseMessage.Headers.GetValues("X-DocRaptor-Num-Pages").First());
            }

            var response = new DocRaptorResponse(responseMessage) {
                NumberOfPages = numPagesHeader
            };

            return response;
        }

        public enum DocumentType {
            Pdf,
            Xls,
            Xlsx
        }
    }
}

