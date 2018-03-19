using System;
using System.Threading.Tasks;

namespace Lykke.Service.CexIoAdapter.Services.Tools
{
    public static class TaskExtensions
    {
        public static async Task<TResult> Select<TSource, TResult>(
            this Task<TSource> source, Func<TSource, TResult> mapFunc)
        {
            var v = await source;
            return mapFunc(v);
        }

        public static async Task<TResult> Select<TResult>(
            this Task source, Func<TResult> mapFunc)
        {
            await source;
            return mapFunc();
        }
    }
}
