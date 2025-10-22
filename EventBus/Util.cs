using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EventBus;

public interface IPriority {

    int priority { get; }

}


public static class ListExtend {

    public static void addByPriority<T>(this List<T> list, T t) where T : IPriority {

        bool needInsert = true;
        for (int index = 0; index < list.Count; index++) {
            T _t = list[index];
            if (_t.priority >= t.priority) {
                continue;
            }
            list.Insert(index, t);
            needInsert = false;
            break;
        }
        if (needInsert) {
            list.Add(t);
        }

    }

    public static void removeByPriority<T>(this List<T> list, T t) where T : IPriority {
        for (int index = 0; index < list.Count; index++) {
            T _t = list[index];
            if (!t.Equals(_t)) {
                continue;
            }
            list.RemoveAt(index);
            index--;
        }
    }

}

public static class AsyncUtil {

    private static Dictionary<Type, bool> canAwaitMap = new Dictionary<Type, bool>();

    public static bool canAwait(this Type type) {
        if (!canAwaitMap.TryGetValue(type, out var result)) {
            result = _canAwait(type);
            canAwaitMap.Add(type, result);
        }
        return result;
    }

    public static bool _canAwait(Type type) {

        MethodInfo? getAwaiter = type.GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
        if (getAwaiter == null) {
            return false;
        }

        Type awaiterType = getAwaiter.ReturnType;
        if (!typeof(INotifyCompletion).IsAssignableFrom(awaiterType)) {
            return false;
        }

        PropertyInfo? isCompleted = awaiterType.GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Instance);
        if (isCompleted?.PropertyType != typeof(bool)) {
            return false;
        }

        MethodInfo? getResult = awaiterType.GetMethod("GetResult", BindingFlags.Public | BindingFlags.Instance);
        return getResult != null;
    }

}