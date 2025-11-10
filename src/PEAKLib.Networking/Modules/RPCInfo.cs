namespace PEAKLib.Networking.Modules
{
    /// <summary>
    /// Information provided to RPC handlers when they declare an RPCInfo final parameter.
    /// </summary>
    public readonly struct RPCInfo
    {
        /// <summary>
        /// </summary>
        public readonly ulong SteamId64;
        /// <summary>
        /// </summary>
        public readonly string SteamIdString;
        /// <summary>
        /// </summary>
        public readonly bool IsLocalLoopback;

        /// <summary>
        /// </summary>
        public RPCInfo(ulong steamId64, string steamIdString, bool isLocalLoopback = false)
        {
            SteamId64 = steamId64;
            SteamIdString = steamIdString;
            IsLocalLoopback = isLocalLoopback;
        }
    }
}
