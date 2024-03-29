﻿using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

namespace RateLimiterTest.HttpHandlers
{
    public class ClientSideRateLimitedHandler
        : DelegatingHandler, IAsyncDisposable
    {
        private readonly RateLimiter _rateLimiter;

        public ClientSideRateLimitedHandler(RateLimiter limiter) => _rateLimiter = limiter;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using RateLimitLease lease = await _rateLimiter.AcquireAsync(
                permitCount: 1, cancellationToken);

            if (lease.IsAcquired)
            {
                await Task.Delay(5000);
                return await base.SendAsync(request, cancellationToken);
            }

            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            if (lease.TryGetMetadata(
                    MetadataName.RetryAfter, out TimeSpan retryAfter))
            {
                response.Headers.Add(
                    "Retry-After",
                    ((int)retryAfter.TotalSeconds).ToString(
                        NumberFormatInfo.InvariantInfo));
            }

            return response;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _rateLimiter.DisposeAsync().ConfigureAwait(false);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _rateLimiter.Dispose();
            }
        }
    }
}
