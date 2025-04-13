namespace SageTokenManager;

public interface ISageTokenProvider
{
    Task<string> GetAccessTokenAsync();
}
