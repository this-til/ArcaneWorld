using System;
using System.Collections.Generic;

namespace CommonUtil.Extensions;

public static class ListExtendMethod {

    private static readonly Random random = new Random();

    public static bool IsEmpty<T>(this IList<T>? list) => list is null || list.Count <= 0;

    public static bool InList<T>(this IList<T>? list, int at) => list is not null && at >= 0 && at < list.Count;

    public static T? RandomElement<T>(this IList<T> list, Random? _random = null) {
        _random ??= random;
        if (list.IsEmpty()) {
            return default;
        }
        return list[_random.Next(0, list.Count)];
    }

}

public static class IReadOnlyExtendMethod {

    private static readonly Random random = new Random();

    public static T? RandomReadOnlyElement<T>(this IReadOnlyList<T> list, Random? _random = null) {
        _random ??= random;
        if (list.Count == 0) {
            return default;
        }
        return list[_random.Next(0, list.Count)];
    }

}
