using eProtocol.Domain.Common;

namespace eProtocol.Domain.Entities;

public class ProtocolSequence : BaseEntity
{
    public int Year { get; set; }
    public int StartNumber { get; set; } = 1;
    public int EndNumber { get; set; } = int.MaxValue;
    public int LastNumber { get; set; }
}
