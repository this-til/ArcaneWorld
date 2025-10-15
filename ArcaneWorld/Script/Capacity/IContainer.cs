namespace ArcaneWorld.Capacity;

public interface IContainer<T> : IHandler<T> where T : class {

    /// <summary>
    /// 最大物品类型数量限制，-1 表示无限制
    /// </summary>
    public long maxTypeCount { get; }

    /// <summary>
    /// 最大物品总数量限制，-1 表示无限制
    /// </summary>
    public long maxTotalCount { get; }

    /// <summary>
    /// 当前的物品数量
    /// </summary>
    public long totalCount { get; }

    /// <summary>
    /// 当前的物品类型数量
    /// </summary>
    public long itemTypeCount { get; }

    /// <summary>
    /// 返回当前具有物品的类型
    /// 该方法总会克隆数据
    /// </summary>
    public IReadOnlyCollection<T> typeSnapshot { get; }

    /// <summary>
    /// 类型以及数量
    /// 该方法总会克隆数据
    /// </summary>
    public IReadOnlyCollection<KeyValuePair<T, long>> snapshot { get; }

    /// <summary>
    /// 获取指定物品的数量
    /// </summary>
    public long getCount(T item);

    /// <summary>
    /// 判定是否包含指定物品
    /// </summary>
    public bool contains(T item);

}
