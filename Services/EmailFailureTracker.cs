using System.Collections.Concurrent;

namespace AutoMarket.Services
{
    /// <summary>
    /// Tracks email sending failures for circuit breaker pattern.
    /// </summary>
    public class EmailFailureTracker
    {
        private readonly ConcurrentDictionary<string, FailureInfo> _failures = new();
        private readonly int _maxFailures;
        private readonly TimeSpan _failureWindow;
        private readonly TimeSpan _circuitBreakerTimeout;

        public EmailFailureTracker(int maxFailures = 5, TimeSpan? failureWindow = null, TimeSpan? circuitBreakerTimeout = null)
        {
            _maxFailures = maxFailures;
            _failureWindow = failureWindow ?? TimeSpan.FromMinutes(5);
            _circuitBreakerTimeout = circuitBreakerTimeout ?? TimeSpan.FromMinutes(1);
        }

        public void RecordFailure()
        {
            var now = DateTime.UtcNow;
            var key = "global"; // Track global failures

            _failures.AddOrUpdate(key,
                new FailureInfo { Count = 1, FirstFailure = now, LastFailure = now, CircuitOpenUntil = null },
                (k, existing) =>
                {
                    // Reset if outside failure window
                    if (now - existing.FirstFailure > _failureWindow)
                    {
                        return new FailureInfo { Count = 1, FirstFailure = now, LastFailure = now, CircuitOpenUntil = null };
                    }

                    var newCount = existing.Count + 1;
                    var circuitOpenUntil = newCount >= _maxFailures 
                        ? now.Add(_circuitBreakerTimeout) 
                        : existing.CircuitOpenUntil;

                    return new FailureInfo
                    {
                        Count = newCount,
                        FirstFailure = existing.FirstFailure,
                        LastFailure = now,
                        CircuitOpenUntil = circuitOpenUntil
                    };
                });
        }

        public void RecordSuccess()
        {
            _failures.TryRemove("global", out _);
        }

        public bool IsCircuitOpen()
        {
            if (!_failures.TryGetValue("global", out var info))
                return false;

            if (info.CircuitOpenUntil == null)
                return false;

            if (DateTime.UtcNow < info.CircuitOpenUntil.Value)
                return true;

            // Circuit breaker timeout expired, reset
            _failures.TryRemove("global", out _);
            return false;
        }

        public int GetFailureCount()
        {
            return _failures.TryGetValue("global", out var info) ? info.Count : 0;
        }

        private class FailureInfo
        {
            public int Count { get; set; }
            public DateTime FirstFailure { get; set; }
            public DateTime LastFailure { get; set; }
            public DateTime? CircuitOpenUntil { get; set; }
        }
    }
}

