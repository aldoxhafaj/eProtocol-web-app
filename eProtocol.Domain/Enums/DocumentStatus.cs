namespace eProtocol.Domain.Enums;

public enum DocumentStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Archived = 4,
    Overdue = 5,
    Draft = Pending,
    Registered = Pending,
    UnderReview = InProgress
}
