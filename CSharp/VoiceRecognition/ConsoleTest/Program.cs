using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var auth = new Authentication("");
            var token = auth.GetAccessToken();
        }

        public class Authentication
        {
            public static readonly string FetchTokenUri = "https://northeurope.api.cognitive.microsoft.com/sts/v1.0";
            private string subscriptionKey;
            private string token;

            public Authentication(string subscriptionKey)
            {
                this.subscriptionKey = subscriptionKey;
                this.token = FetchTokenAsync(FetchTokenUri, subscriptionKey).Result;
            }

            public string GetAccessToken()
            {
                return this.token;
            }

            private async Task<string> FetchTokenAsync(string fetchUri, string subscriptionKey)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    UriBuilder uriBuilder = new UriBuilder(fetchUri);
                    uriBuilder.Path += "/issueToken";

                    var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
                    Console.WriteLine("Token Uri: {0}", uriBuilder.Uri.AbsoluteUri);
                    return await result.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
