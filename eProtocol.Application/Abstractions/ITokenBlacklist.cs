namespace eProtocol.Application.Abstractions;

public interface ITokenBlacklist
{
    void Blacklist(string token);
    bool IsBlacklisted(string token);
}
