using System;
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

            Assert.AreEqual(HttpStatusCode.OK, status0);
            Assert.AreEqual(HttpStatusCode.OK, status1);
            Assert.AreEqual(HttpStatusCode.OK, status2);
            
            Assert.AreEqual("1", content0);
            Assert.AreEqual("1", content1);
            Assert.AreEqual("2", content2);
            
            await _server.StopAsync();
        }
        [Test]
        public async Task WebsocketRouteOrdering()
        {
            _server.WebSocket("/websocket", (req, res, wsd) => res.SendString("1"));
            _server.Get("/*",  (req, res) => res.SendString("2"));
            await _server.StartAsync();

            var (status0, _) = await _httpClient.GetContent(BaseUrl + "/websocket");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/askjldald");

            Assert.AreEqual(HttpStatusCode.UpgradeRequired, status0);
            Assert.AreEqual(HttpStatusCode.OK, status1);
            
            Assert.AreEqual("2", content1);
            
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

            Assert.AreEqual(HttpStatusCode.OK, status0);
            Assert.AreEqual(HttpStatusCode.OK, status1);
            Assert.AreEqual(HttpStatusCode.OK, status2);
            Assert.AreEqual(HttpStatusCode.OK, status3);
            
            Assert.AreEqual("1", content0);
            Assert.AreEqual("2", content1);
            Assert.AreEqual("3", content2);
            Assert.AreEqual("1", content3);
            
            await _server.StopAsync();
        }

        [Test]
        public async Task ReverseWildcardOrderingRoutingTest()
        {
            // The ordering of catch-all wildcard matters as shown by this test
            _server.Get("/*", (req, res) => res.SendString("1"));
            _server.Get("/hello/*", (req, res) => res.SendString("2"));
            _server.Get("/hello/world",  (req, res) => res.SendString("3"));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl + "/blah");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/hello/world");
            var (status2, content2) = await _httpClient.GetContent(BaseUrl + "/hello");
            var (status3, content3) = await _httpClient.GetContent(BaseUrl + "/blah/blah");

            Assert.AreEqual(HttpStatusCode.OK, status0);
            Assert.AreEqual(HttpStatusCode.OK, status1);
            Assert.AreEqual(HttpStatusCode.OK, status2);
            Assert.AreEqual(HttpStatusCode.OK, status3);
            
            Assert.AreEqual(content0, "1");
            Assert.AreEqual(content1, "1");
            Assert.AreEqual(content2, "1");
            Assert.AreEqual(content3, "1");
            
            await _server.StopAsync();
        }
        [Test]
        public async Task WildcardOrderingRoutingTest()
        {
            // The ordering of catch-all wildcard matters as shown by this test
            _server.Get("/hello/world",  (req, res) => res.SendString("3"));
            _server.Get("/hello/*", (req, res) => res.SendString("2"));
            _server.Get("/*", (req, res) => res.SendString("1"));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl + "/blah");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/hello/world");
            var (status2, content2) = await _httpClient.GetContent(BaseUrl + "/hello");
            var (status3, content3) = await _httpClient.GetContent(BaseUrl + "/blah/blah");

            Assert.AreEqual(HttpStatusCode.OK, status0);
            Assert.AreEqual(HttpStatusCode.OK, status1);
            Assert.AreEqual(HttpStatusCode.OK, status2);
            Assert.AreEqual(HttpStatusCode.OK, status3);
            
            Assert.AreEqual(content0, "1");
            Assert.AreEqual(content1, "3");
            Assert.AreEqual(content2, "2");
            Assert.AreEqual(content3, "1");
            
            await _server.StopAsync();
        }
        
        [Test]
        public async Task ParametersRoutingTest()
        {
            _server.Get("/test", (req, res) => res.SendString("test1"));
            _server.Get("/:kind/test", (req, res) => res.SendString(req.Context.Params["kind"] + "2"));
            _server.Get("/:kind",  (req, res) => res.SendString(req.Context.Params["kind"] + "3"));
            _server.Start();

            var (status0, content0) = await _httpClient.GetContent(BaseUrl + "/test");
            var (status1, content1) = await _httpClient.GetContent(BaseUrl + "/banana/test");
            var (status2, content2) = await _httpClient.GetContent(BaseUrl + "/apple");
            var (status3, content3) = await _httpClient.GetContent(BaseUrl + "/orange");
            var (status4, content4) = await _httpClient.GetContent(BaseUrl + "/peach/test");

            Assert.AreEqual(HttpStatusCode.OK, status0);
            Assert.AreEqual(HttpStatusCode.OK, status1);
            Assert.AreEqual(HttpStatusCode.OK, status2);
            Assert.AreEqual(HttpStatusCode.OK, status3);
            Assert.AreEqual(HttpStatusCode.OK, status4);
            
            Assert.AreEqual("test1", content0);
            Assert.AreEqual("banana2", content1);
            Assert.AreEqual("apple3", content2);
            Assert.AreEqual("orange3", content3);
            Assert.AreEqual("peach2", content4);
            
            await _server.StopAsync();
        }
        
        
        [Test]
        public async Task NotFoundRoutingTest()
        {
            _server.Get("/test", (req, res) => res.SendString("test1"));
            _server.Get("/:kind/test", (req, res) => res.SendString(req.Context.Params["kind"] + "2"));
            _server.Start();

            var (status0, _) = await _httpClient.GetContent(BaseUrl + "/test/blah");
            var (status1, _) = await _httpClient.GetContent(BaseUrl + "/blah/blah");
            var (status2, _) = await _httpClient.GetContent(BaseUrl + "/");
            var (status3, _) = await _httpClient.GetContent(BaseUrl);
            var (status4, _) = await _httpClient.GetContent(BaseUrl + "/test1");
            var (status5, content5) = await _httpClient.GetContent(BaseUrl + "/test");

            Assert.AreEqual(HttpStatusCode.NotFound, status0);
            Assert.AreEqual(HttpStatusCode.NotFound, status1);
            Assert.AreEqual(HttpStatusCode.NotFound, status2);
            Assert.AreEqual(HttpStatusCode.NotFound, status3);
            Assert.AreEqual(HttpStatusCode.NotFound, status4);
            
            Assert.AreEqual(HttpStatusCode.OK, status5);
            Assert.AreEqual("test1", content5);
            
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