using System;
using System.Collections;
using System.Collections.Generic;
using CommonUtil.Extensions;

namespace CommonUtil.Container;

public class Value<V> {

    protected Func<V>? valueFactory;

    protected V? _value;

    protected bool isLoad;

    public V value {
        get {
            if (isLoad) {
                return _value!;
            }

            lock (this) {
                if (isLoad) {
                    return _value!;
                }

                if (valueFactory is not null) {
                    _value = valueFactory();
                    if (_value is not null) {
                        valueFactory = null !;
                        isLoad = true;
                    }
                }

                return _value!;
            }
        }
    }

    public Value(V? value) {
        isLoad = true;
        _value = value;
    }

    public Value(Func<V> valueFactory) => this.valueFactory = valueFactory;
    public static implicit operator V(Value<V> value) => value.value;
    public static implicit operator Value<V>(Func<V> valueFactory) => new Value<V>(valueFactory);
    public static implicit operator Value<V>(V value) => new Value<V>(value);

}

public interface IReadOnlyValueList<V> : IReadOnlyList<V> {

    IReadOnlyList<Value<V>> allStorage { get; }

}

public class ValueList<V> : IReadOnlyValueList<V> {

    protected List<Value<V>> _allStorage = new List<Value<V>>();

    protected List<Value<V>> lazyStorage = new List<Value<V>>();

    protected List<V> storage = new List<V>();

    public IReadOnlyList<Value<V>> allStorage => _allStorage;

    protected void map() {
        if (lazyStorage.IsEmpty()) {
            return;
        }

        for (int i = lazyStorage.Count - 1; i >= 0; i--) {
            V res = lazyStorage[i].value;
            if (res is null) {
                continue;
            }

            storage.Add(res);
            lazyStorage.RemoveAt(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerator<V> GetEnumerator() {
        throw new NotImplementedException();
    }

    public int Count {
        get {
            map();
            return storage.Count;
        }
    }

    public bool contains(V item) {
        map();
        return storage.Contains(item);
    }

    public void Add(Value<V> item) {
        lazyStorage.Add(item);
        _allStorage.Add(item);
    }

    public void AddRange(IEnumerable<Value<V>> items) {
        foreach (Value<V> item in items) {
            Add(item);
        }
    }

    public static implicit operator ValueList<V>(Value<V> value) => [value];

    public V this[int index] {
        get {
            map();
            if (index < 0 || index >= storage.Count) {
                return default!;
            }
            return storage[index];
        }
    }

}
