namespace Red
{
    /// <summary>
    ///     The type of handling that has been performed.
    /// </summary>
    public enum HandlerType
    {
        /// <summary>
        ///     This handler has sent a final response.
        ///     Do not invoke the rest of the handler-chain
        /// </summary>
        Final,

        /// <summary>
        ///     This handler processed data.
        ///     Continue invoking the handler-chain
        /// </summary>
        Continue,

        /// <summary>
        ///     An error occured when handling the request.
        ///     Stop invoking the rest of the handler chain
        /// </summary>
        Error
    }
}