using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorageBrowser
{
    public static class CollectionExtensions
    {
        public static Task ForEachAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> taskSelector,
                CancellationToken token, int initialRequestCount = 10, int maxRequestCount = 20)
        {
            var atATime = new SemaphoreSlim(initialRequestCount, maxRequestCount);
            return Task.WhenAll(source.Select(item => ProcessAsync(item, taskSelector, atATime, token)));
        }

        public static Task ForEachAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> taskSelector,
            int initialRequestCount = 10, int maxRequestCount = 20)
        {
            return ForEachAsync(source, taskSelector, CancellationToken.None, initialRequestCount, maxRequestCount);
        }

        private static async Task ProcessAsync<TSource>(TSource item, Func<TSource, Task> taskSelector,
            SemaphoreSlim atATime, CancellationToken token)
        {
            await atATime.WaitAsync(token);
            try
            {
                await taskSelector(item);
            }
            finally
            {
                atATime.Release();
            }
        }

        public static async Task<IEnumerable<TOut>> ForEachAsync<TSource, TOut>(this IEnumerable<TSource> source,
            Func<TSource, Task<TOut>> taskSelector, CancellationToken token, int initialRequestCount = 10,
            int maxRequestCount = 20)
        {
            var atATime = new SemaphoreSlim(initialRequestCount, maxRequestCount);
            return await Task.WhenAll(source.Select(item => ProcessAsync(item, taskSelector, atATime, token)));
        }

        public static Task<IEnumerable<TOut>> ForEachAsync<TSource, TOut>(this IEnumerable<TSource> source,
            Func<TSource, Task<TOut>> taskSelector, int initialRequestCount = 10, int maxRequestCount = 20)
        {
            return ForEachAsync(source, taskSelector, CancellationToken.None, initialRequestCount, maxRequestCount);
        }

        private static async Task<TOut> ProcessAsync<TSource, TOut>(TSource item, Func<TSource, Task<TOut>> taskSelector,
            SemaphoreSlim atATime, CancellationToken token)
        {
            await atATime.WaitAsync(token);
            try
            {
                return await taskSelector(item);
            }
            finally
            {
                atATime.Release();
            }
        }

        internal class DisposableTaskCompletionSource<TOut> : IDisposable
        {
            public TaskCompletionSource<TOut> Source { get; set; }

            public DisposableTaskCompletionSource(CancellationToken token)
            {
                var tcs = new TaskCompletionSource<TOut>();
                token.Register(() => tcs.TrySetCanceled(), false);
                Source = tcs;
            }

            public void Dispose()
            {
                Source.TrySetResult(default(TOut));
            }
        }
    }
}