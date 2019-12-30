using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RedHttpServer.Tests
{
    public class RoutingIntegrationTests
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

        [Test]
        public async Task BasicRouting()
        {
            _server.Get("/", (req, res) => res.SendString("1"));
            _server.Get("/hello",  (req, res) => res.SendString("2"));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl);
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/");
            var (status2, content2) = await _httpClient.GetContent(BaseUrl + "/hello");

            Assert.AreEqual(status0, HttpStatusCode.OK);
            Assert.AreEqual(status1, HttpStatusCode.OK);
            Assert.AreEqual(status2, HttpStatusCode.OK);
            
            Assert.AreEqual(content0, "1");
            Assert.AreEqual(content1, "1");
            Assert.AreEqual(content2, "2");
            
            await _server.StopAsync();
        }
        
        [Test]
        public async Task WildcardRoutingTest()
        {
            _server.Get("/hello",  (req, res) => res.SendString("3"));
            _server.Get("/hello/*", (req, res) => res.SendString("2"));
            _server.Get("/*", (req, res) => res.SendString("1"));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl + "/blah");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/hello/blah");
            var (status2, content2) = await _httpClient.GetContent(BaseUrl + "/hello");
            var (status3, content3) = await _httpClient.GetContent(BaseUrl + "/blah/blah");

            Assert.AreEqual(status0, HttpStatusCode.OK);
            Assert.AreEqual(status1, HttpStatusCode.OK);
            Assert.AreEqual(status2, HttpStatusCode.OK);
            Assert.AreEqual(status3, HttpStatusCode.OK);
            
            Assert.AreEqual(content0, "1");
            Assert.AreEqual(content1, "2");
            Assert.AreEqual(content2, "3");
            Assert.AreEqual(content3, "1");
            
            await _server.StopAsync();
        }

        [Test]
        public async Task WildcardOrderingRoutingTest()
        {
            // The ordering of catch-all wildcard matters as shown by this test
            _server.Get("/*", (req, res) => res.SendString("1"));
            _server.Get("/hello/*", (req, res) => res.SendString("2"));
            _server.Get("/hello",  (req, res) => res.SendString("3"));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl + "/blah");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/hello/blah");
            var (status2, content2) = await _httpClient.GetContent(BaseUrl + "/hello");
            var (status3, content3) = await _httpClient.GetContent(BaseUrl + "/blah/blah");

            Assert.AreEqual(status0, HttpStatusCode.OK);
            Assert.AreEqual(status1, HttpStatusCode.OK);
            Assert.AreEqual(status2, HttpStatusCode.OK);
            Assert.AreEqual(status3, HttpStatusCode.OK);
            
            Assert.AreEqual(content0, "1");
            Assert.AreEqual(content1, "1");
            Assert.AreEqual(content2, "1");
            Assert.AreEqual(content3, "1");
            
            await _server.StopAsync();
        }
        
        [Test]
        public async Task ParametersRoutingTest()
        {
            _server.Get("/test", (req, res) => res.SendString("test1"));
            _server.Get("/:kind/test", (req, res) => res.SendString(req.Context.ExtractUrlParameter("kind") + "2"));
            _server.Get("/:kind",  (req, res) => res.SendString(req.Context.ExtractUrlParameter("kind") + "3"));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl + "/test");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/banana/test");
            var (status2, content2) = await _httpClient.GetContent(BaseUrl + "/apple");
            var (status3, content3) = await _httpClient.GetContent(BaseUrl + "/orange");
            var (status4, content4) = await _httpClient.GetContent(BaseUrl + "/peach/test");

            Assert.AreEqual(status0, HttpStatusCode.OK);
            Assert.AreEqual(status1, HttpStatusCode.OK);
            Assert.AreEqual(status2, HttpStatusCode.OK);
            Assert.AreEqual(status3, HttpStatusCode.OK);
            Assert.AreEqual(status4, HttpStatusCode.OK);
            
            Assert.AreEqual(content0, "test1");
            Assert.AreEqual(content1, "banana2");
            Assert.AreEqual(content2, "apple3");
            Assert.AreEqual(content3, "orange3");
            Assert.AreEqual(content4, "peach2");
            
            await _server.StopAsync();
        }
        
        
        [Test]
        public async Task NotFoundRoutingTest()
        {
            _server.Get("/test", (req, res) => res.SendString("test1"));
            _server.Get("/:kind/test", (req, res) => res.SendString(req.Context.ExtractUrlParameter("kind") + "2"));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl + "/test/blah");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/blah/blah");
            var (status2, content2) = await _httpClient.GetContent(BaseUrl + "/");
            var (status3, content3) = await _httpClient.GetContent(BaseUrl);
            var (status4, content4) = await _httpClient.GetContent(BaseUrl + "/test1");

            Assert.AreEqual(status0, HttpStatusCode.NotFound);
            Assert.AreEqual(status1, HttpStatusCode.NotFound);
            Assert.AreEqual(status2, HttpStatusCode.NotFound);
            Assert.AreEqual(status3, HttpStatusCode.NotFound);
            Assert.AreEqual(status4, HttpStatusCode.NotFound);
            
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