using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleAsync
{
    public class Decompile
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task<HttpResponseMessage> GetResultFromTheInterWebs() =>
            await _client.GetAsync(new Uri("https://www.google.com/"));
    }
}
