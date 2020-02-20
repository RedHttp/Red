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

        public class TestPayload
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }
        
        [Test]
        public async Task JsonSerialization()
        {
            var obj = new TestPayload
            {
                Name = "test",
                Number = 42
            };
            _server.Get("/json", (req, res) => res.SendJson(obj));
            _server.Start();

            var (status, content) = await _httpClient.GetContent(BaseUrl + "/json");

            Assert.AreEqual(status, HttpStatusCode.OK);
            Assert.AreEqual(content, "{\"Name\":\"test\",\"Number\":42}");
            
            await _server.StopAsync();
        }
        [Test]
        public async Task XmlSerialization()
        {
            var obj = new TestPayload
            {
                Name = "test",
                Number = 42
            };
            _server.Get("/xml", (req, res) => res.SendXml(obj));
            _server.Start();

            var (status, content) = await _httpClient.GetContent(BaseUrl + "/xml");

            Assert.AreEqual(status, HttpStatusCode.OK);
            Assert.AreEqual(content, "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestPayload xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Name>test</Name><Number>42</Number></TestPayload>");
            
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