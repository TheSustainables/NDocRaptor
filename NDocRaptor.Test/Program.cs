using System;
using System.Diagnostics;
using System.IO;

namespace NDocRaptor {
    class Program {
        static void Main(string[] args) {
            var raptor = new DocRaptor("<your api key>", testMode: true);

            Action<DocRaptorResponse> test = response => {
                Console.WriteLine("Success: {0}", response.Success);
                Console.WriteLine("Error-phrase: {0}", response.ReasonPhrase);

                if (response.Success) {
                    var tempFile = Path.GetTempFileName() + ".pdf";
                    var bytes = response.Response.Content.ReadAsByteArrayAsync().Result;
                    File.WriteAllBytes(tempFile, bytes);
                    Process.Start(tempFile);
                }
            };

            var html = "<html><head><title></title></head><body><h1>Yeah</h1></body></html>";
            test(raptor.CreatePdfDocumentAsync(html).Result);
            test(raptor.CreatePdfDocumentAsync(new Uri("https://html.spec.whatwg.org/multipage/")).Result);

            Console.ReadLine();
        }
    }
}