using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <summary>
    ///     Interface for classes used for parsing and deserializing body
    /// </summary>
    public interface IBodyParser
    {
        /// <summary>
        /// Parse the request body stream into a string
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<string> Parse(Request request);
        
        
        /// <summary>
        /// Parse the request body stream into an object of a given type
        /// </summary>
        /// <param name="request"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Parse<T>(Request request);
    }
}