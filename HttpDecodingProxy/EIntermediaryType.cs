namespace IziHardGames.Proxy.Http
{
    /// <summary>
    /// https://httpwg.org/specs/rfc9110.html#intermediaries
    /// </summary>
    public enum EIntermediaryType
    {
        None,
        Proxy,
        Gateway,
        /// <summary>
        /// A tunnel acts as a blind relay between two connections without changing the messages
        /// </summary>
        Tunnel,
        /// <summary>
        /// Man In The Middle
        /// </summary>
        MITM,
        /// <summary>
        /// For example, an interception proxy [RFC3040] (also commonly known as a transparent proxy [RFC1919]) 
        /// differs from an HTTP proxy because it is not chosen by the client. 
        /// Instead, an interception proxy filters or redirects outgoing TCP port 80 packets (and occasionally other common port traffic). 
        /// Interception proxies are commonly found on public network access points, as a means of enforcing account subscription 
        /// prior to allowing use of non-local Internet services, and within corporate firewalls to enforce network usage policies.
        /// </summary>
        Interceptor,
    }
}