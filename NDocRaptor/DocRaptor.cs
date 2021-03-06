﻿using System;
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
        
        /// <summary>
        /// Represents a simple wrapper around the DocRaptor web API
        /// </summary>
        /// <param name="apiKey">Your API key (see DocRaptor dashboard)</param>
        /// <param name="useSsl">Whether to use ssl (slower)</param>
        /// <param name="testMode">Whether to enable test-mode</param>
        /// <param name="tag">Tag to use for all requests</param>
        public DocRaptor(string apiKey, bool useSsl = false, bool testMode = false, string tag = null) {
            ApiKey = apiKey;
            TestMode = testMode;
            Tag = tag;
            Client = new HttpClient();
            DocRaptorUrl = new Uri((useSsl ? "https" : "http") + "://docraptor.com/docs");
        }

        /// <summary>
        /// Send a HTTP request to DocRaptor
        /// </summary>
        /// <param name="url">The URL of the page to convert</param>
        /// <param name="content">The content to convert</param>
        /// <param name="name">The document name (visible in DocRaptor dashboard)</param>
        /// <param name="tag">The tag to use</param>
        /// <param name="documentType">The document type</param>
        /// <param name="strict">Whether to use strict HTML validation</param>
        /// <param name="javascript">Whether to enable javascript</param>
        /// <param name="test">Whether to enable test-mode</param>
        /// <param name="help">Whether to mark document for support</param>
        /// <param name="parameters">Extra parameters to pass trough (i.e. "doc[prince_options][version]" = "8.1")</param>
        /// <returns>A DocRaptorResponse object containing response information.</returns>
        public async Task<DocRaptorResponse> CreateDocumentAsync(
            Uri url = null,
            string content = null,
            string name = "Untitled",
            string tag = null,
            DocumentType documentType = DocumentType.Pdf,
            bool strict = false,
            bool javascript = false,
            bool test = false,
            bool help = false,
            int pipeline = 5,
            Dictionary<string, string> parameters = null) {

            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }

            var form = new Dictionary<string, string> {
                { "doc[document_type]", documentType.ToString().ToLower() },
                { "doc[name]", name },
                { "doc[test]", test.ToString().ToLower() },
                { "doc[tag]", tag },
                { "doc[strict]", strict ? "html" : "none" },
                { "doc[pipeline]", pipeline.ToString() },
                { "doc[ignore_console_messages]", "true" }
            };

            if (url != null) {
                form.Add("doc[document_url]", url.ToString());
            }
            else {
                form.Add("doc[document_content]", content);
            }

            if (help) {
                form.Add("doc[help]", "true");
            }

            if (documentType == DocumentType.Pdf) {
                form.Add("doc[javascript]", javascript.ToString().ToLower());
            }

            if (parameters != null) {
                foreach (var kv in parameters) {
                    form.Add(kv.Key, kv.Value);
                }
            }

            var requestUrl = DocRaptorUrl.ToString() + "?user_credentials=" + ApiKey;
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Content = new FormUrlEncodedContent(form);

            var responseMessage = await Client.SendAsync(request).ConfigureAwait(false);

            return CreateResponse(responseMessage);
        }

        /// <summary>
        /// Send a PDF generation request using HTML content
        /// </summary>
        /// <param name="url">The URL of the page to convert to PDF</param>
        /// <param name="name">The document name (visible in DocRaptor dashboard)</param>
        /// <param name="strict">Whether to use strict HTML validation</param>
        /// <param name="javascript">Whether to enable javascript</param>
        /// <param name="help">Whether to mark document for support</param>
        /// <returns>A DocRaptorResponse object containing response information.</returns>
        public async Task<DocRaptorResponse> CreatePdfDocumentAsync(
            Uri url,
            string name = "Untitled",
            bool strict = false,
            bool javascript = false,
            bool help = false,
            int pipeline = 5) {

            if (url == null) {
                throw new ArgumentNullException("url");
            }

            return await CreateDocumentAsync(url, null, name, Tag, DocumentType.Pdf, strict, javascript, TestMode, help, pipeline).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a PDF generation request using HTML content
        /// </summary>
        /// <param name="content">The HTML content</param>
        /// <param name="name">The document name (visible in DocRaptor dashboard)</param>
        /// <param name="princeVersion">Prince version (7.0, 8.0 or 9.0)</param>
        /// <param name="strict">Whether to use strict HTML validation</param>
        /// <param name="javascript">Whether to enable javascript</param>
        /// <param name="help">Whether to mark document for support</param>
        /// <returns>A DocRaptorResponse object containing response information.</returns>
        public async Task<DocRaptorResponse> CreatePdfDocumentAsync(
            string content,
            string name = "Untitled",
            bool strict = false,
            bool javascript = false,
            bool help = false,
            int pipeline = 5) {

            if (content == null) {
                throw new ArgumentNullException("content");
            }

            return await CreateDocumentAsync(null, content, name, Tag, DocumentType.Pdf, strict, javascript, TestMode, help, pipeline).ConfigureAwait(false);
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

