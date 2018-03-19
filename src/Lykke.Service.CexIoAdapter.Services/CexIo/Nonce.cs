using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.CexIoAdapter.Services.Tools;

namespace Lykke.Service.CexIoAdapter.Services.CexIo
{
    public sealed class Nonce : IDisposable
    {
        private static readonly ConcurrentDictionary<string, Nonce> Pool = new ConcurrentDictionary<string, Nonce>();

        public static Task<T> With<T>(
            string userId,
            Func<long, Task<T>> code)
        {
            var nonce = Pool.GetOrAdd(userId, _ => new Nonce());
            return nonce.With(code);
        }

        private readonly SemaphoreSlim _nonceLock = new SemaphoreSlim(1);

        private long _current;

        private async Task<T> With<T>(
            Func<long, Task<T>> code)
        {
            await _nonceLock.WaitAsync();

            try
            {
                var epoch = DateTime.UtcNow.Epoch();

                if (epoch > _current)
                {
                    _current = epoch;
                }
                else
                {
                    _current++;
                }

                return await code(_current);
            }
            finally
            {
                _nonceLock.Release();
            }
        }

        public void Dispose()
        {
            _nonceLock?.Dispose();
        }
    }
}
