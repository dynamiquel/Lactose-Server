using System.Collections;

namespace LactoseWebApp;

public static class ContainerExtensions
{
    public static TU GetOrAdd<T, TU>(this IDictionary<T, TU> dictionary, T key) where TU : new()
    {
        if (!dictionary.ContainsKey(key))
            dictionary[key] = new TU();

        return dictionary[key];
    }

    public static bool IsEmpty<T>(this ICollection<T>? collection)
    {
        return collection is null || collection.Count == 0;
    }

    public static void Append<T>(this ISet<T> set, IEnumerable<T> items)
    {
        if (set is HashSet<T> hashSet)
            hashSet.EnsureCapacity(set.Count + items.Count());
        
        foreach (var item in items)
            set.Add(item);
    }
}