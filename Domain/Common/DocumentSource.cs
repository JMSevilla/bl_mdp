namespace WTW.MdpService.Domain.Common;

public enum DocumentSource
{
    None = 0,
    /// <summary>
    /// Use this enum when member uploads document directly.
    /// </summary>
    Incoming,
    /// <summary>
    /// Use this enum when member uploads document indirectly(i.e. retrieved doc from gbg) or document genarated by Assure.
    /// </summary>
    Outgoing
}