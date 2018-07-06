using System;
using System.Net;
using System.Threading.Tasks;
using Red;

namespace Test
{
    class MySess
    {
        public string Name { get; set; }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var server = new RedHttpServer(5000);
            
            Console.WriteLine("Hello World!");
        }
    }
}