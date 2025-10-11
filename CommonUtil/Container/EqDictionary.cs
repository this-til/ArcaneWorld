using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CommonUtil.Container;

public class EqDictionary<K, V> : Dictionary<K, V> {

    public EqDictionary() {
    }

    public EqDictionary(IDictionary<K, V> dictionary) : base(dictionary) {
    }

    public EqDictionary(IDictionary<K, V> dictionary, IEqualityComparer<K> comparer) : base(dictionary, comparer) {
    }

    public EqDictionary(IEqualityComparer<K> comparer) : base(comparer) {
    }

    public EqDictionary(int capacity) : base(capacity) {
    }

    public EqDictionary(int capacity, IEqualityComparer<K> comparer) : base(capacity, comparer) {
    }

    protected EqDictionary(SerializationInfo info, StreamingContext context) : base(info, context) {
    }

    public EqDictionary(IEnumerable<KeyValuePair<K, V>> collection) : base(collection) {
    }

    public EqDictionary(IEnumerable<KeyValuePair<K, V>> collection, IEqualityComparer<K> comparer) : base(collection, comparer) {
    }

    public override bool Equals(object? obj) {
        if (this == obj) {
            return true;
        }

        if (obj is not IDictionary<K, V> dictionary) {
            return false;
        }

        if (Count != dictionary.Count) {
            return false;
        }

        foreach (K key in Keys) {
            if (!dictionary.TryGetValue(key, out V value)) {
                return false;
            }

            if (!Equals(this[key], value)) {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode() {
        HashCode hashCode = new HashCode();
        foreach (KeyValuePair<K, V> keyValuePair in this) {
            hashCode.Add(keyValuePair.Key);
            hashCode.Add(keyValuePair.Value);
        }

        return hashCode.ToHashCode();
    }

}
