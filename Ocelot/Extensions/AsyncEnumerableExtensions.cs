using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pooshit.Ocelot.Extensions;

public static class AsyncEnumerableExtensions {

    public static async IAsyncEnumerable<T> Buffer<T>(this IAsyncEnumerable<T> enumerable) {
        List<T> buffer = [];
        await foreach (T item in enumerable)
            buffer.Add(item);

        foreach (T item in buffer)
            yield return item;
    }

    public static async Task<T[]> ToArray<T>(this IAsyncEnumerable<T> enumerable) {
        List<T> buffer = [];
        await foreach (T item in enumerable)
            buffer.Add(item);

        return buffer.ToArray();
    }
}