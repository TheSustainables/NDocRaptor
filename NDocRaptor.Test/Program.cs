using System;
using System.Diagnostics;
using System.IO;

namespace NDocRaptor {
    class Program {
        static void Main(string[] args) {
            var raptor = new DocRaptor("<your api key>", testMode: true);

            Action<DocRaptorResponse> test = result => {
                Console.WriteLine("Success: {0}", result.Success);
                Console.WriteLine("Error-phrase: {0}", result.ReasonPhrase);

                if (result.Success) {
                    var path = Path.GetTempFileName() + ".pdf";
                    result.SaveAs(path);
                    Process.Start(path);
                }
            };

            var html = "<html><head><title></title></head><body><h1>Yeah</h1></body></html>";
            test(raptor.CreatePdfDocumentAsync(html).Result);
            test(raptor.CreatePdfDocumentAsync(new Uri("https://html.spec.whatwg.org/multipage/")).Result);

            Console.ReadLine();
        }
    }
}