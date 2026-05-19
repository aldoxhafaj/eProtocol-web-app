namespace eProtocol.Application.Abstractions;

public interface IUserContext
{
    Guid UserId { get; }
    string Role { get; }
}
