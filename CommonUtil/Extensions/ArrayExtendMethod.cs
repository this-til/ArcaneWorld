using System;
using System.Collections;
using System.Linq;

namespace CommonUtil.Extensions;

public static class ArrayExtendMethod {

    /// <summary>
    /// 判断数组是不是空
    /// </summary>
    public static bool IsEmpty<T>(this T[]? t) => t == null || t.Length == 0;

    /// <summary>
    /// 判断数组索引是否在数组范围内
    /// </summary>
    public static bool InArray<T>(this T[]? list, int at) => list is not null && at >= 0 && at < list.Length;

    public static int DepthHashCode<T>(this T[]? array) {
        if (array is null) {
            return 0;
        }

        int hash = 17;
        unchecked {
            foreach (object? t in array) {
                hash = hash * 23 + t?.GetHashCode() ?? 0;
            }
        }

        return hash;
    }

}