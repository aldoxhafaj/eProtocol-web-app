using eProtocol.Domain.Common;

namespace eProtocol.Domain.Entities;

public class ProtocolSequence : BaseEntity
{
    public int Year { get; set; }
    public int LastNumber { get; set; }
}
