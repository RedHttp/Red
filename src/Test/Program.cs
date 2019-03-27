using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        static async Task<Response.Type> Auth(Request req, Response res)
        {
            return Response.Type.Final;
        }
        static async Task<Response.Type> Auth(Request req, Response res, WebSocketDialog wsd)
        {
            return Response.Type.Final;
        }
        static async Task Main(string[] args)
        {
            
            var array = new NDArray<int>(5, 5);
            var array2 = new int[5, 5];
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Console.WriteLine(i * 5 + j);
                    array[i, j] = i * 5 + j;
                    array2[i, j] = i * 5 + j;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (array[i, j] == array2[i, j])
                    {
                        throw new Exception();
                    }
                }
            }
            
            var server = new RedHttpServer(5001);
            server.RespondWithExceptionDetails = true;

            server.Get("/exception", (req, res) => throw new Exception("oh no!"));
            server.Get("/index", Auth, (req, res) => res.SendFile("./index.html"));
            server.Get("/webm", (req, res) => res.SendFile("./Big_Buck_Bunny_alt.webm"));
            
            server.WebSocket("/echo", Auth, async (req, res, wsd) =>
            {
                await wsd.SendText("Welcome to the echo test server");
                wsd.OnTextReceived += (sender, eventArgs) => { wsd.SendText("you sent: " + eventArgs.Text); };

                return wsd.Final();
            });
            await server.RunAsync();
        }
    }

    class NDArray<T>
    {
        private readonly int[] _dimensionSizes;
        private readonly T[] _array;

        private int Resolve(int[] coordinates)
        {
            int dim = 0;
            for (int i = 0; i < _dimensionSizes.Length; i++)
            {
                var dimensionSize = _dimensionSizes[i];
                var coordinate = coordinates[i];
                dim += 
            }
            var result =  coordinates.Aggregate(0, (acc, coordinate) => acc += (coordinate * _dimensionSizes[dim]) + _dimensionSizes[dim++]);
            Console.WriteLine(result);
            return result;
        }
        
        public NDArray(params int[] dimensionSizes)
        {
            _dimensionSizes = dimensionSizes;
            _array = new T[Resolve(dimensionSizes)];
        }

        public ArraySegment<T> GetSegment(NDPoint start, int count)
        {
            return new ArraySegment<T>(_array, Resolve(start), count);
        }
        public ArraySegment<T> GetSegment(NDPoint start, NDPoint end)
        {
            var offset = Resolve(start);
            return GetSegment(start, Resolve(end) - offset);
        }

        public T this[params int[] coordinates]
        {
            get
            {
                if (coordinates.Length != _dimensionSizes.Length)
                    throw new ArgumentOutOfRangeException(nameof(coordinates), "Incorrect dimensions");
                return _array[Resolve(coordinates)];
            }
            set
            {
                if (coordinates.Length != _dimensionSizes.Length)
                    throw new ArgumentOutOfRangeException(nameof(coordinates), "Incorrect dimensions");
                _array[Resolve(coordinates)] = value;
            }
        }
        public T this[NDPoint point]
        {
            get => this[(int[]) point];
            set => this[(int[]) point] = value;
        }

        public struct NDPoint
        {
            private readonly int[] _coordinates;

            public NDPoint(params int[] coordinates)
            {
                _coordinates = coordinates;
            }
            
            public static implicit operator int[](NDPoint point)
            {
                return point._coordinates;
            } 
        }
    }
}