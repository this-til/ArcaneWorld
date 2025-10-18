using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CommonUtil.Extensions;

public static class LinqExtensions {

    public static IEnumerable<T> Peek<T>(this IEnumerable<T> source, Action<T> action) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (action == null) {
            throw new ArgumentNullException(nameof(action));
        }

        foreach (T item in source) {
            action(item);
            yield return item;
        }
    }

    public static IEnumerable<T> Peek<T>(this IEnumerable<T> source, Action<T, int> action) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (action == null) {
            throw new ArgumentNullException(nameof(action));
        }

        int i = 0;
        foreach (T item in source) {
            action(item, i);
            i++;
            yield return item;
        }
    }

    public static IEnumerable<T> TryPeek<T>(this IEnumerable<T> source, Action<T> action, Action<T, Exception> ex, bool discardExceptionElements = true) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (action == null) {
            throw new ArgumentNullException(nameof(action));
        }
        if (ex == null) {
            throw new ArgumentNullException(nameof(ex));
        }

        foreach (T item in source) {
            try {
                action(item);
            }
            catch (Exception exception) {
                ex(item, exception);
                if (discardExceptionElements) {
                    continue;
                }
            }
            yield return item;
        }
    }

    public static IEnumerable<O> TrySelect<I, O>(this IEnumerable<I> source, Func<I, O> selector, Func<I, Exception, O> ex) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }
        if (ex == null) {
            throw new ArgumentNullException(nameof(ex));
        }

        foreach (I item in source) {
            O result;
            try {
                result = selector(item);
            }
            catch (Exception exception) {
                result = ex(item, exception);
            }
            yield return result;
        }
    }

    public static IEnumerable<O> TrySelect<I, O>(this IEnumerable<I> source, Func<I, O> selector, Action<I, Exception> ex) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }
        if (ex == null) {
            throw new ArgumentNullException(nameof(ex));
        }

        foreach (I item in source) {
            O result;
            try {
                result = selector(item);
            }
            catch (Exception exception) {
                ex(item, exception);
                continue; // 跳过该项，不产生输出
            }
            yield return result;
        }
    }

    public static IEnumerable<O> TrySelectMany<I, O>(this IEnumerable<I> source, Func<I, IEnumerable<O>> selector, Func<I, Exception, IEnumerable<O>> ex) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }
        if (ex == null) {
            throw new ArgumentNullException(nameof(ex));
        }

        foreach (I item in source) {
            IEnumerable<O> result;
            try {
                result = selector(item);
            }
            catch (Exception exception) {
                result = ex(item, exception);
            }

            foreach (O subItem in result) {
                yield return subItem;
            }
        }
    }

    public static IEnumerable<O> TrySelectMany<I, O>(this IEnumerable<I> source, Func<I, IEnumerable<O>> selector, Action<I, Exception> ex) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }
        if (ex == null) {
            throw new ArgumentNullException(nameof(ex));
        }

        foreach (I item in source) {
            IEnumerable<O> result;
            try {
                result = selector(item);
            }
            catch (Exception exception) {
                ex(item, exception);
                continue; // 跳过该项，不产生输出
            }

            foreach (O subItem in result) {
                yield return subItem;
            }
        }
    }

    public static IEnumerable<T> TryWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate, Action<T, Exception>? onException = null) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        foreach (T item in source) {
            bool shouldInclude;
            try {
                shouldInclude = predicate(item);
            }
            catch (Exception exception) {
                // 如果谓词抛出异常，跳过该项
                shouldInclude = false;
                onException?.Invoke(item, exception);
            }

            if (shouldInclude) {
                yield return item;
            }
        }
    }

    public static IEnumerable<T> Diversion<T>(this IEnumerable<T> source, Func<T, bool> predicate, Func<IEnumerable<T>, IEnumerable<T>> away) {

        List<T> outList = new List<T>();

        List<T> awayList = new List<T>();

        foreach (T x1 in source) {
            if (predicate(x1)) {
                outList.Add(x1);
            }
            else {
                awayList.Add(x1);
            }
        }

        outList.AddRange(away(awayList));

        return outList;
    }

    public static void End<T>(this IEnumerable<T> source) {
        foreach (T x1 in source) {

        }
    }

    public static IDictionary<K, IList<V>> ClassifiedCollection<T, K, V>
    (
        this IEnumerable<T> source,
        Func<T, K> keySelector,
        Func<T, V> valueSelector,
        Func<K, IList<V>>? createContainer = null,
        IEnumerable<K>? defKeys = null
    ) {

        createContainer ??= k => new List<V>();

        Dictionary<K, IList<V>> dictionary = new Dictionary<K, IList<V>>();

        if (defKeys is not null) {
            foreach (K k in defKeys) {
                if (dictionary.TryGetValue(k, out IList<V>? lv)) {
                    continue;
                }

                lv = createContainer(k);
                dictionary.Add(k, lv);
            }
        }

        foreach (T t in source) {
            K k = keySelector(t);
            if (k == null) {
                continue;
            }

            V v = valueSelector(t);
            if (!dictionary.TryGetValue(k, out IList<V>? lv)) {
                lv = createContainer(k);
                dictionary.Add(k, lv);
            }

            lv.Add(v);
        }

        return dictionary;
    }

    public static IReadOnlyDictionary<K, V> ToReadOnlyDictionary<K, V>(this IDictionary<K, V> dictionary) where K : notnull => ImmutableDictionary.CreateRange(dictionary);

    /*public static IEnumerable<V> DistinctBy<V, E>(this IEnumerable<V> source, Func<V, E> converter) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (converter == null) {
            throw new ArgumentNullException(nameof(converter));
        }

        HashSet<E> seenKeys = new HashSet<E>();

        foreach (V item in source) {
            E key = converter(item);
            if (seenKeys.Add(key)) {
                yield return item;
            }
        }
    }*/

    public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate, Action<T> filtered) {
        return source.Where(
            t => {
                if (predicate(t)) {
                    return true;
                }
                filtered(t);
                return false;
            }
        );
    }

    /*public static IEnumerable<T> Exception<T>(this IEnumerable<T> source, Func<Exception, T> exception) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        if (exception == null) {
            throw new ArgumentNullException(nameof(exception));
        }

        using IEnumerator<T> enumerator = source.GetEnumerator();

        while (true) {
            T current;
            bool hasNext = false;

            try {
                hasNext = enumerator.MoveNext();
            }
            catch (Exception ex) {
                current = exception(ex);
            }

            if (!hasNext) {
                yield break;
            }

            current = enumerator.Current;

            yield return current;

        }
    }

    public static IEnumerable<T?> Exception<T>(this IEnumerable<T> source, Action<Exception> exception) =>
        source
            .Exception(
                e => {
                    exception(e);
                    return default!;
                }
            );*/

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source) =>
        source
            .Where(t => t is not null)
            .Cast<T>();

    /*public static T FirstOrDefault<T>(this IEnumerable<T?> enumerable, T def) {
        foreach (T? x1 in enumerable) {
            if (x1 is null) {
                continue;
            }
            return x1;
        }
        return def;
    }*/

    public static IReadOnlyList<V> AsReadOnly<V>(this IEnumerable<V> enumerable) {
        if (enumerable is IReadOnlyList<V> readOnlyList) {
            return readOnlyList;
        }
        return enumerable.ToList().AsReadOnly();
    }

    public static List<T?> ControlQuantity<T>(this IEnumerable<T> enumerable, int quantity) {
        List<T?> list = new List<T?>();
        foreach (T x1 in enumerable) {
            list.Add(x1);
            if (list.Count >= quantity) {
                break;
            }
        }

        while (list.Count < quantity) {
            list.Add(default);
        }

        return list;
    }

}
