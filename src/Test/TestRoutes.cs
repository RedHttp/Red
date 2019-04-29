using System.Threading.Tasks;
using Red;
using Red.Interfaces;

namespace Test
{
    public class TestRoutes
    {
        public static void Register(IRouter router)
        {
            router.Get("/1", (req, res) => res.SendString("Test1"));
            router.Get("/2", GetTest2);
        }

        private static Task<HandlerType> GetTest2(Request arg1, Response arg2)
        {
            return arg2.SendString("Test2");
        }
    }
}