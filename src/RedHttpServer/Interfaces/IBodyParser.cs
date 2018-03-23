using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <summary>
    ///     Interface for classes used for parsing and deserializing body
    /// </summary>
    public interface IBodyParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Parse<T>(Request request);
    }
}