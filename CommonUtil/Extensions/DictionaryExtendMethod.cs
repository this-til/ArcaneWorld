using System.Collections.Generic;

namespace CommonUtil.Extensions;

public static class DictionaryExtendMethod {

    public static bool IsEmpty<K, V>(this IDictionary<K, V> dictionary) => dictionary.Count == 0;
        
}