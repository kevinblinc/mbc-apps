namespace SageTokenManager;

public interface ITokenStore
{
    Task<TokenState?> LoadAsync();
    Task SaveAsync(TokenState token);
}
