using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LactoseWebApp;

public static class ContainerExtensions
{
    public static TU GetOrAdd<T, TU>(this IDictionary<T, TU> dictionary, T key) where TU : new()
    {
        if (!dictionary.ContainsKey(key))
            dictionary[key] = new TU();

        return dictionary[key];
    }
    
    public static bool IsEmpty<T>([NotNullWhen(false)] this ICollection<T>? collection)
    {
        return collection is null || collection.Count == 0;
    }

    public static void AddRange<T>(this ISet<T> set, IEnumerable<T> items)
    {
        if (set is HashSet<T> hashSet)
            hashSet.EnsureCapacity(set.Count + items.Count());
        
        foreach (var item in items)
            set.Add(item);
    }

    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    {
        ((List<T>)list).AddRange(items);
    }
    
    public static int Remove<T>(this ICollection<T> collection, Predicate<T> matchingCondition)
    {
        int removedCount = 0;
        
        if (collection is IList<T> list)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                T element = list[i];
                bool shouldRemove = matchingCondition(element);
                if (shouldRemove)
                {
                    list.RemoveAt(i);
                    removedCount++;
                }
            }
        }
        else
        {
            for (var i = collection.Count - 1; i >= 0; i--)
            {
                T element = collection.ElementAt(i);
                bool shouldRemove = matchingCondition(element);
                if (shouldRemove)
                {
                    collection.Remove(element);
                    removedCount++;
                }
            }
        }

        return removedCount;
    }
    
    public static string ToCommaSeparatedString(this IEnumerable<object> enumerable)
    {
        var list = enumerable.ToImmutableList();

        if (list.Count <= 0)
            return string.Empty;

        if (list[0].ToString() is null)
            throw new ArgumentNullException(
                nameof(enumerable), "The provided objects in the enumerable cannot return null with ToString");
        
        // Approximate reserved capacity based on the length of the first element.
        var sb = new StringBuilder(list[0].ToString()!.Length * list.Count);

        for (var i = 0; i < list.Count - 1; i++)
            sb.Append($"{list[i].ToString()}, ");

        sb.Append(list[^1]);

        return sb.ToString();
    }
}