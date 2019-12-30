using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RedHttpServer.Tests
{
    public class BodyParserTests
    {
        private Red.RedHttpServer _server;
        private HttpClient _httpClient;

        private const int TestPort = 7592;
        private const string BaseUrl = "http://localhost:7592";

        [SetUp]
        public void Setup()
        {
            _server = new Red.RedHttpServer(TestPort);
            _httpClient = new HttpClient();
        }

        class TestPayload
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }
        
        [Test]
        public async Task BasicSerializationDeserialization()
        {
            var obj = new TestPayload
            {
                Name = "test",
                Number = 42
            };
            
            _server.Get("/json", (req, res) => res.SendJson(obj));
            _server.Get("/xml", (req, res) => res.SendXml(obj));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl + "/json");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/xml");

            Assert.AreEqual(status0, HttpStatusCode.OK);
            Assert.AreEqual(status1, HttpStatusCode.OK);
            
            
            Assert.AreEqual(content0, "1");
            Assert.AreEqual(content1, "1");
            
            await _server.StopAsync();
        }
        

        [TearDown]
        public void Teardown()
        {
            _server.StopAsync().Wait();
            _httpClient.Dispose();
        }
    }
}