using System.Collections.Concurrent;

namespace dy.net.job
{
    /// <summary>
    /// Serializes scheduled sync jobs that share the same Douyin account.
    /// </summary>
    internal static class DouyinSyncRequestGate
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new();

        public static Task EnterAsync(string cookieId, CancellationToken cancellationToken)
        {
            var key = string.IsNullOrWhiteSpace(cookieId) ? "__default__" : cookieId;
            return Gates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1)).WaitAsync(cancellationToken);
        }

        public static void Exit(string cookieId)
        {
            var key = string.IsNullOrWhiteSpace(cookieId) ? "__default__" : cookieId;
            if (Gates.TryGetValue(key, out var gate))
            {
                gate.Release();
            }
        }
    }
}
