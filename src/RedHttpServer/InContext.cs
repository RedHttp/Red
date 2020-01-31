namespace Red
{
    /// <summary>
    /// Common base for Request and Response
    /// </summary>
    public abstract class InContext
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        protected InContext(Context context)
        {
            Context = context;
        }
        

        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <typeparam name="TData">the type key</typeparam>
        /// <returns>Object of specified type, registered to request. Otherwise default</returns>
        public TData? GetData<TData>() where TData : class => Context.GetData<TData>();
        /// <summary>
        ///     Function that middleware can use to attach data to the request, so the next handlers has access to the data
        /// </summary>
        /// <typeparam name="TData">the type of the data object (implicitly)</typeparam>
        /// <param name="data">the data object</param>
        public void SetData<TData>(TData data) where TData : class => Context.SetData(data);

        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <param name="key">the data key</param>
        public string? GetData(string key) => Context.GetData(key);
        /// <summary>
        ///     Function that middleware can use to attach data to the request, so the next handlers has access to the data
        /// </summary>
        public void SetData(string key, string value) => Context.SetData(key, value);
        /// <summary>
        ///     The Red.Context this instance is in
        /// </summary>
        public readonly Context Context;
        
    }
}