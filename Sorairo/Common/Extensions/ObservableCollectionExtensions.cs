#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Collections.ObjectModel;

#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ObservableCollectionExtensions
{
    public static int FindIndex<T>(this ObservableCollection<T> collection, Predicate<T> predicate)
    {
        var index = 0;
        foreach (var item in collection)
        {
            if (predicate(item))
            {
                return index;
            }
            ++index;
        }
        return -1;
    }

    public static T? Find<T>(this ObservableCollection<T> collection, Predicate<T> predicate)
    {
        foreach (var item in collection)
        {
            if (predicate(item))
            {
                return item;
            }
        }
        return default;
    }
}
