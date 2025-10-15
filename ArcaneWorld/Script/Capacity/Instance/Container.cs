using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using ArcaneWorld.Attribute;
using ArcaneWorld.Capacity;
using ArcaneWorld.Util;
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
public partial class Container<T> : IContainer<T>, IAutoSerialize where T : class {

    /// <summary>
    /// 传递的锁，该锁应该存于组件附加的类型中
    /// 当然你也可以创建一个新的
    /// </summary>
    [Required]
    public ILock transferLock { get; init; } = null!;

    [SaveField]
    private EqDictionary<T, long> itemCounts = new EqDictionary<T, long>();

    private long _totalCount;

    /// <summary>
    /// 容器版本号，每次内容变更时递增
    /// </summary>
    private long _version;

    private long _maxTypeCount = -1;

    public long maxTypeCount {
        get {
            using (transferLock.lockForRead()) {
                return _maxTypeCount;
            }
        }
        set {
            using (transferLock.lockForWrite()) {
                _maxTypeCount = value;
            }
        }
    }

    private long _maxTotalCount = -1;

    public long maxTotalCount {
        get {
            using (transferLock.lockForRead()) {
                return _maxTotalCount;
            }
        }
        set {
            using (transferLock.lockForWrite()) {
                _maxTotalCount = value;
            }
        }
    }

    public long itemTypeCount {
        get {
            using (transferLock.lockForRead()) {
                return itemCounts.Count;
            }
        }
    }

    [SaveField]
    public long totalCount {
        get {
            using (transferLock.lockForRead()) {
                return _totalCount;
            }
        }
        private set {
            using (transferLock.lockForWrite()) {
                _totalCount = value;
            }
        }
    }

    /// <summary>
    /// 容器版本号，用于追踪内容变更
    /// </summary>
    [SaveField]
    public long version {
        get {
            using (transferLock.lockForRead()) {
                return _version;
            }
        }
        private set {
            using (transferLock.lockForWrite()) {
                _version = value;
            }
        }
    }

    public IReadOnlyCollection<T> typeSnapshot {
        get {
            using (transferLock.lockForRead()) {
                return itemCounts.Keys.ToList().AsReadOnly();
            }
        }
    }

    public IReadOnlyCollection<KeyValuePair<T, long>> snapshot {
        get {
            using (transferLock.lockForRead()) {
                return itemCounts.ToList().AsReadOnly();
            }
        }
    }

    private bool canAddUnsafe(T item, long count) {
        // 检查总数限制
        if (_maxTotalCount >= 0 && _totalCount + count > _maxTotalCount) {
            return false;
        }

        // 检查类型数限制
        if (
            _maxTypeCount >= 0
            && !itemCounts.ContainsKey(item)
            && itemCounts.Count >= _maxTypeCount
        ) {
            return false;
        }

        return true;
    }

    private long getMaxAddableCountUnsafe(T item) {
        long maxByTotal = _maxTotalCount >= 0
            ? _maxTotalCount - _totalCount
            : long.MaxValue;

        if (_maxTypeCount >= 0 && !itemCounts.ContainsKey(item) &&
            itemCounts.Count >= _maxTypeCount) {
            return 0;
        }

        return Math.Max(0, maxByTotal);
    }

    public long getCount(T item) {
        using (transferLock.lockForRead()) {
            return itemCounts.GetValueOrDefault(item, 0);
        }
    }

    public bool contains(T item) {
        using (transferLock.lockForRead()) {
            return itemCounts.ContainsKey(item) && itemCounts[item] > 0;
        }
    }

    public long insert(T item, long count, bool simulation) {
        if (count <= 0) {
            return 0;
        }

        if (simulation) {
            // 模拟操作，不修改状态
            using (transferLock.lockForRead()) {
                return canAddUnsafe(item, count)
                    ? count
                    : getMaxAddableCountUnsafe(item);
            }
        }

        using (transferLock.lockForWrite()) {
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
    }

    public long extract(T item, long count, bool simulation) {
        if (count <= 0) {
            return 0;
        }

        if (simulation) {
            // 模拟操作，不修改状态
            using (transferLock.lockForRead()) {
                return Math.Min(count, itemCounts.GetValueOrDefault(item, 0));
            }
        }

        using (transferLock.lockForWrite()) {
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
    }

    public void clear() {
        using (transferLock.lockForWrite()) {
            if (itemCounts.Count > 0 || _totalCount > 0) {
                itemCounts.Clear();
                _totalCount = 0;
                // 递增版本号
                _version++;
            }
        }
    }

    public StructReadLockContext lockForRead() {
        return transferLock.lockForRead();
    }

    public StructWriteLockContext lockForWrite() {
        return transferLock.lockForWrite();
    }

    protected partial void onBeforeSerialize(JsonSerializer jsonSerializer) {
    }

    protected partial void onAfterSerialize(JObject jObject, JsonSerializer jsonSerializer) {
    }

    protected partial void onBeforeDeserialize(JObject data, JsonSerializer jsonSerializer) {
    }

    protected partial void onAfterDeserialize(JObject data, JsonSerializer jsonSerializer) {
    }

}
