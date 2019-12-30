using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedHttpServer.Tests
{
    public static class TestUtils
    {
        public static async Task<(HttpStatusCode, string)> GetContent(this HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            return (response.StatusCode, await response.Content.ReadAsStringAsync());
        } 
    }
}