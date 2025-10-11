using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.Container;

public class EqList<T> : List<T> {

    public EqList() {
    }

    public EqList(IEnumerable<T> collection) : base(collection) {
    }

    public EqList(int capacity) : base(capacity) {
    }

    public override bool Equals(object? obj) {
        if (obj == this) {
            return true;
        }

        if (obj is not IList<T> list) {
            return false;
        }

        if (Count != list.Count) {
            return false;
        }

        for (int i = 0; i < Count; i++) {
            if (!Equals(this[i], list[i])) {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode() {
        HashCode hashCode = new HashCode();
        foreach (T t in this) {
            hashCode.Add(t);
        }

        return hashCode.ToHashCode();
    }

    public override string ToString() {
        return string.Join(',', this.Select(t => t?.ToString() ?? "null"));
    }

}