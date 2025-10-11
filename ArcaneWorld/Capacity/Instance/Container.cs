using System.Collections.Concurrent;
using ArcaneWorld.Capacity;
using CommonUtil.Container;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RegisterSystem;

namespace ArcaneWorld.Capacity.Instance;

/// <summary>
/// 高性能线程安全的物品容器
/// 物品无堆叠数量，单类物品无差异，支持类型和总数的双限制
/// </summary>
public abstract partial class Container<T> : Node, IContainer<T>, IDisposable where T : class {

    private EqDictionary<T, long> itemCounts = new EqDictionary<T, long>();

    private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    private long _totalCount = 0;

    /// <summary>
    /// 容器版本号，每次内容变更时递增
    /// </summary>
    private long _version = 0;
    
    /// <summary>
    /// 缓存的 Godot Dictionary 快照
    /// </summary>
    private Dictionary? _cachedGodotSnapshot = null;
    
    /// <summary>
    /// 缓存快照对应的版本号
    /// </summary>
    private long _cachedVersion = -1;

    private bool _disposed = false;

    private long _maxTypeCount = -1;

    [Export]
    public long maxTypeCount {
        get {
            rwLock.EnterReadLock();
            try {
                return _maxTypeCount;
            }
            finally {
                rwLock.ExitReadLock();
            }
        }
        set {
            rwLock.EnterReadLock();
            try {
                _maxTypeCount = value;
            }
            finally {
                rwLock.ExitReadLock();
            }
        }
    }

    private long _maxTotalCount = -1;

    [Export]
    public long maxTotalCount {
        get {
            rwLock.EnterReadLock();
            try {
                return _maxTotalCount;
            }
            finally {
                rwLock.ExitReadLock();
            }
        }
        set {
            rwLock.EnterReadLock();
            try {
                _maxTotalCount = value;
            }
            finally {
                rwLock.ExitReadLock();
            }
        }
    }

    [Export]
    public long itemTypeCount {
        get {
            rwLock.EnterReadLock();
            try {
                return itemCounts.Count;
            }
            finally {
                rwLock.ExitReadLock();
            }
        }
        // ReSharper disable once ValueParameterNotUsed
        private set {
            // readOnly
        }
    }

    [Export]
    public long totalCount {
        get {
            rwLock.EnterReadLock();
            try {
                return _totalCount;
            }
            finally {
                rwLock.ExitReadLock();
            }
        }
        // ReSharper disable once ValueParameterNotUsed
        private set {
            // readOnly
        }
    }

    /// <summary>
    /// 容器版本号，用于追踪内容变更
    /// </summary>
    [Export]
    public long version {
        get {
            rwLock.EnterReadLock();
            try {
                return _version;
            }
            finally {
                rwLock.ExitReadLock();
            }
        }
        // ReSharper disable once ValueParameterNotUsed
        private set {
            // readOnly
        }
    }

    /// <summary>
    /// 转换对象为 Godot 可识别的字符串标识
    /// 子类需要重写此方法来提供特定的转换逻辑
    /// </summary>
    protected abstract string convertToGodotKey(T item);

    /// <summary>
    /// 从 Godot 字符串标识转换回对象
    /// 子类需要重写此方法来提供特定的转换逻辑
    /// </summary>
    protected abstract T convertFromGodotKey(string key);
    
    [Export]
    public Dictionary godotSnapshot {
        get {
            rwLock.EnterReadLock();
            try {
                // 检查缓存是否有效
                if (_cachedGodotSnapshot != null && _cachedVersion == _version) {
                    return _cachedGodotSnapshot;
                }
            }
            finally {
                rwLock.ExitReadLock();
            }

            // 缓存无效，需要重新构建
            rwLock.EnterWriteLock();
            try {
                // 双重检查，可能其他线程已经更新了缓存
                if (_cachedGodotSnapshot != null && _cachedVersion == _version) {
                    return _cachedGodotSnapshot;
                }

                // 重新构建缓存
                Dictionary itemsDict = new Dictionary();
                foreach (KeyValuePair<T, long> kvp in itemCounts) {
                    string key = convertToGodotKey(kvp.Key);
                    itemsDict[key] = kvp.Value;
                }

                // 更新缓存
                _cachedGodotSnapshot = itemsDict;
                _cachedVersion = _version;

                return itemsDict;
            }
            finally {
                rwLock.ExitWriteLock();
            }
        }
        set {
            rwLock.EnterWriteLock();
            try {
                // 清空当前内容
                itemCounts.Clear();
                _totalCount = 0;

                // 递增版本号表示内容已变更
                _version++;
                
                // 清除缓存
                _cachedGodotSnapshot = null;
                _cachedVersion = -1;
                
                // 从 Godot Dictionary 恢复数据
                if (value == null) {
                    return;
                }

                foreach (Variant key in value.Keys) {
                    string keyStr = key.AsString();
                    long count = value[key].AsInt64();

                    if (count <= 0) {
                        continue;
                    }
                    
                    T item = convertFromGodotKey(keyStr);
                    if (item == null) {
                        continue;
                    }
                    
                    itemCounts[item] = count;
                    _totalCount += count;
                }
                
            }
            finally {
                rwLock.ExitWriteLock();
            }
        }
    }

    public IReadOnlyCollection<T> typeSnapshot {
        get {
            rwLock.EnterReadLock();
            try {
                return itemCounts.Keys.ToList().AsReadOnly();
            }
            finally {
                rwLock.ExitReadLock();
            }
        }
    }

    public IReadOnlyCollection<KeyValuePair<T, long>> snapshot {
        get {
            rwLock.EnterReadLock();
            try {
                return itemCounts.ToList().AsReadOnly();
            }
            finally {
                rwLock.ExitReadLock();
            }

        }
    }

    private bool canAddUnsafe(T item, long count) {
        // 检查总数限制
        if (maxTotalCount >= 0 && _totalCount + count > maxTotalCount) {
            return false;
        }

        // 检查类型数限制
        if (
            maxTypeCount >= 0
            && !itemCounts.ContainsKey(item)
            && itemCounts.Count >= maxTypeCount
        ) {
            return false;
        }

        return true;
    }

    private long getMaxAddableCountUnsafe(T item) {
        long maxByTotal = maxTotalCount >= 0
            ? maxTotalCount - _totalCount
            : long.MaxValue;

        if (maxTypeCount >= 0 && !itemCounts.ContainsKey(item) &&
            itemCounts.Count >= maxTypeCount) {
            return 0;
        }

        return Math.Max(0, maxByTotal);
    }

    public long getCount(T item) {
        rwLock.EnterReadLock();
        try {
            return itemCounts.GetValueOrDefault(item, 0);
        }
        finally {
            rwLock.ExitReadLock();
        }
    }

    public bool contains(T item) {
        rwLock.EnterReadLock();
        try {
            return itemCounts.ContainsKey(item) && itemCounts[item] > 0;
        }
        finally {
            rwLock.ExitReadLock();
        }
    }

    public long insert(T item, long count, bool simulation) {
        if (count <= 0)
            return 0;

        if (simulation) {
            // 模拟操作，不修改状态
            rwLock.EnterReadLock();
            try {
                return canAddUnsafe(item, count)
                    ? count
                    : getMaxAddableCountUnsafe(item);
            }
            finally {
                rwLock.ExitReadLock();
            }
        }

        rwLock.EnterWriteLock();
        try {
            long maxAddable = getMaxAddableCountUnsafe(item);
            long actualAdd = Math.Min(count, maxAddable);

            if (actualAdd > 0) {
                if (itemCounts.TryGetValue(item, out long currentCount)) {
                    itemCounts[item] = currentCount + actualAdd;
                }
                else {
                    itemCounts[item] = actualAdd;
                }
                _totalCount += actualAdd;
                // 递增版本号
                _version++;
            }

            return actualAdd;
        }
        finally {
            rwLock.ExitWriteLock();
        }
    }

    public long extract(T item, long count, bool simulation) {
        if (count <= 0)
            return 0;

        if (simulation) {
            // 模拟操作，不修改状态
            rwLock.EnterReadLock();
            try {
                return Math.Min(count, itemCounts.GetValueOrDefault(item, 0));
            }
            finally {
                rwLock.ExitReadLock();
            }
        }

        rwLock.EnterWriteLock();
        try {
            if (!itemCounts.TryGetValue(item, out long currentCount)) {
                return 0;
            }

            long actualExtract = Math.Min(count, currentCount);
            long newCount = currentCount - actualExtract;

            if (actualExtract > 0) {
                if (newCount == 0) {
                    itemCounts.Remove(item);
                }
                else {
                    itemCounts[item] = newCount;
                }

                _totalCount -= actualExtract;
                // 递增版本号
                _version++;
            }

            return actualExtract;
        }
        finally {
            rwLock.ExitWriteLock();
        }
    }

    public void clear() {
        rwLock.EnterWriteLock();
        try {
            if (itemCounts.Count > 0 || _totalCount > 0) {
                itemCounts.Clear();
                _totalCount = 0;
                // 递增版本号
                _version++;
                // 清除缓存
                _cachedGodotSnapshot = null;
                _cachedVersion = -1;
            }
        }
        finally {
            rwLock.ExitWriteLock();
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (!_disposed) {
            rwLock?.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// 获取读锁，用于批量读取操作
    /// </summary>
    /// <returns>读锁上下文，需要在 using 语句中使用</returns>
    public IDisposable lockForRead() {
        return new ReadLockContext(rwLock);
    }

    /// <summary>
    /// 获取写锁，用于批量写入操作
    /// </summary>
    /// <returns>写锁上下文，需要在 using 语句中使用</returns>
    public IDisposable lockForWrite() {
        return new WriteLockContext(rwLock);
    }

    /// <summary>
    /// 读锁上下文
    /// </summary>
    private class ReadLockContext : IDisposable {

        private readonly ReaderWriterLockSlim rwLock;

        private bool disposed = false;

        public ReadLockContext(ReaderWriterLockSlim rwLock) {
            this.rwLock = rwLock;
            rwLock.EnterReadLock();
        }

        public void Dispose() {
            if (!disposed) {
                rwLock.ExitReadLock();
                disposed = true;
            }
        }

    }

    /// <summary>
    /// 写锁上下文
    /// </summary>
    private class WriteLockContext : IDisposable {

        private readonly ReaderWriterLockSlim rwLock;

        private bool disposed = false;

        public WriteLockContext(ReaderWriterLockSlim rwLock) {
            this.rwLock = rwLock;
            rwLock.EnterWriteLock();
        }

        public void Dispose() {
            if (!disposed) {
                rwLock.ExitWriteLock();
                disposed = true;
            }
        }

    }

}
