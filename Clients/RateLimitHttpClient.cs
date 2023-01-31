namespace RateLimiterTest.Clients
{
    public interface IRateLimitHttpClient
    {
        ValueTask<int> GetAsync(string url, CancellationToken cancellationToken);
    }

    public class RateLimitHttpClient : IRateLimitHttpClient
    {
        private readonly HttpClient _httpClient;
        public RateLimitHttpClient(HttpClient client)
        {
            _httpClient = client;
        }

        public async ValueTask<int> GetAsync(string url, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            return (int)response.StatusCode;
        }
    }
}
