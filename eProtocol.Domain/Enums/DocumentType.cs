namespace eProtocol.Domain.Enums;

public enum DocumentType
{
    Incoming = 1,
    Outgoing = 2,
    Internal = 3,
    IncomingExternal = Incoming,
    OutgoingExternal = Outgoing
}
