using CommonUtil.Container;
using RegisterSystem;
using FlexibleRequired;
using System.Collections;

namespace CakeToolset;

public class R_AdditionalDataKey : RegisterManage<AdditionalDataKey> {

}

public abstract class AdditionalDataKey : RegisterBasics {

    public abstract Type valueType { get; }

}

public partial class AdditionalDataKey<T> : AdditionalDataKey {

    public override Type valueType => typeof(T);

    [Required]
    public Func<T> defValueFactory { protected get; init; } = null!;

    public T defValue => defValueFactory();

}

public class AdditionalDataMap : ICloneable, IEnumerable<KeyValuePair<AdditionalDataKey, object>>, IDisposable {

    internal EqDictionary<AdditionalDataKey, object> map = new EqDictionary<AdditionalDataKey, object>();
    internal volatile bool needsCopy = false;
    private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

    public V get<V>(AdditionalDataKey<V> additionalDataKey) {
        rwLock.EnterReadLock();
        try {
            if (map.TryGetValue(additionalDataKey, out object? v)) {
                return (V)v;
            }
        }
        finally {
            rwLock.ExitReadLock();
        }

        // 需要修改时升级到写锁
        rwLock.EnterWriteLock();
        try {
            // 双重检查，可能其他线程已经添加了这个值
            if (map.TryGetValue(additionalDataKey, out object? v)) {
                return (V)v;
            }

            // 确保有独立的副本
            ensureUniqueCopyUnsafe();
            
            V defValue = additionalDataKey.defValue!;
            map.Add(additionalDataKey, defValue);
            return defValue;
        }
        finally {
            rwLock.ExitWriteLock();
        }
    }

    public AdditionalDataMap set<V>(AdditionalDataKey<V> additionalDataKey, V v) {
        rwLock.EnterWriteLock();
        try {
            // 确保有独立的副本
            ensureUniqueCopyUnsafe();
            
            map[additionalDataKey] = v!;
            return this;
        }
        finally {
            rwLock.ExitWriteLock();
        }
    }

    private void ensureUniqueCopyUnsafe() {
        if (needsCopy) {
            // 执行深拷贝
            EqDictionary<AdditionalDataKey, object> newMap = new EqDictionary<AdditionalDataKey, object>();
            foreach (KeyValuePair<AdditionalDataKey, object> keyValuePair in map) {
                newMap[keyValuePair.Key] = keyValuePair.Value is ICloneable cloneable
                    ? cloneable.Clone()
                    : keyValuePair.Value;
            }
            map = newMap;
            needsCopy = false;
        }
    }

    public AdditionalDataMap copy() {
        rwLock.EnterReadLock();
        try {
            AdditionalDataMap additionalDataMap = new AdditionalDataMap();
            // 使用 Copy-on-Write: 只传递引用，标记需要复制
            additionalDataMap.map = map;
            additionalDataMap.needsCopy = true;
            // 当前实例也需要在下次修改时复制
            needsCopy = true;
            return additionalDataMap;
        }
        finally {
            rwLock.ExitReadLock();
        }
    }

    /// <summary>
    /// 转换为只读的 AdditionalDataMap
    /// 使用 Copy-on-Write 策略，不会立即复制数据
    /// </summary>
    public ReadOnlyAdditionalDataMap toReadOnly() {
        rwLock.EnterReadLock();
        try {
            // 标记当前实例在下次修改时需要复制
            needsCopy = true;
            return new ReadOnlyAdditionalDataMap(map);
        }
        finally {
            rwLock.ExitReadLock();
        }
    }

    /// <summary>
    /// 获取所有键值对的枚举器
    /// 注意：枚举期间会持有读锁，应尽快完成枚举操作
    /// </summary>
    public IEnumerator<KeyValuePair<AdditionalDataKey, object>> GetEnumerator() {
        rwLock.EnterReadLock();
        try {
            // 创建快照以避免在枚举过程中持有锁
            var snapshot = map.ToList();
            return snapshot.GetEnumerator();
        }
        finally {
            rwLock.ExitReadLock();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public object Clone() => copy();

    public override bool Equals(object? obj) {
        if (this == obj) {
            return true;
        }

        if (obj is not AdditionalDataMap additionalDataMap) {
            return false;
        }

        rwLock.EnterReadLock();
        try {
            return map.Equals(additionalDataMap.map);
        }
        finally {
            rwLock.ExitReadLock();
        }
    }

    public override int GetHashCode() {
        rwLock.EnterReadLock();
        try {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return map.GetHashCode();
        }
        finally {
            rwLock.ExitReadLock();
        }
    }

    public void Dispose() {
        rwLock?.Dispose();
        GC.SuppressFinalize(this);
    }

}

/// <summary>
/// 只读的 AdditionalDataMap，提供高效的读取访问和与可修改版本的转换
/// 线程安全：支持多线程并发读取
/// </summary>
public class ReadOnlyAdditionalDataMap : IEnumerable<KeyValuePair<AdditionalDataKey, object>> {
    
    private readonly EqDictionary<AdditionalDataKey, object> map;
    
    // 内部构造函数，只能通过 AdditionalDataMap 转换创建
    internal ReadOnlyAdditionalDataMap(EqDictionary<AdditionalDataKey, object> map) {
        this.map = map;
    }
    
    /// <summary>
    /// 获取指定键的值，如果不存在则返回默认值（不会修改底层数据）
    /// 线程安全：多个线程可以同时调用此方法
    /// </summary>
    public V get<V>(AdditionalDataKey<V> additionalDataKey) {
        // ReadOnlyAdditionalDataMap 不修改数据，但需要防止在读取时底层字典被修改
        // 由于我们共享引用，读取操作本身是安全的（EqDictionary 的读取是线程安全的）
        if (map.TryGetValue(additionalDataKey, out object? v)) {
            return (V)v;
        }
        // 只读版本不能修改底层数据，直接返回默认值
        return additionalDataKey.defValue;
    }
    
    /// <summary>
    /// 检查是否包含指定的键
    /// </summary>
    public bool containsKey(AdditionalDataKey key) => map.ContainsKey(key);
    
    /// <summary>
    /// 获取键值对数量
    /// </summary>
    public int count => map.Count;
    
    /// <summary>
    /// 转换为可修改的 AdditionalDataMap
    /// 使用 Copy-on-Write 策略，不会立即复制数据
    /// </summary>
    public AdditionalDataMap toMutable() {
        AdditionalDataMap mutable = new AdditionalDataMap();
        mutable.map = map;
        mutable.needsCopy = true;
        return mutable;
    }
    
    /// <summary>
    /// 获取所有键值对的枚举器（只读）
    /// 注意：如果在枚举期间底层数据被修改，可能会抛出异常
    /// </summary>
    public IEnumerator<KeyValuePair<AdditionalDataKey, object>> GetEnumerator() {
        // 为了避免枚举期间的并发修改异常，创建快照
        return map.ToList().GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public override bool Equals(object? obj) {
        if (this == obj) {
            return true;
        }
        
        if (obj is ReadOnlyAdditionalDataMap readOnlyMap) {
            return map.Equals(readOnlyMap.map);
        }
        
        if (obj is AdditionalDataMap mutableMap) {
            return map.Equals(mutableMap.map);
        }
        
        return false;
    }
    
    public override int GetHashCode() => map.GetHashCode();
}

