using ArcaneWorld.Register;

namespace ArcaneWorld.Capacity;

public interface IHandler<in T> : ILock where T : class {

    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="item">物品类型</param>
    /// <param name="count">请求添加的物品数量</param>
    /// <param name="simulation">如果为true表示执行模拟操作，不会改编容器状态</param>
    /// <returns>添加了的物品数量</returns>
    long insert(T item, long count, bool simulation);

    /// <summary>
    /// 提取物品
    /// </summary>
    /// <param name="item">物品类型</param>
    /// <param name="count">请求提取的数量</param>
    /// <param name="simulation">如果为true表示执行模拟操作，不会改编容器状态</param>
    /// <returns>提取到的数量</returns>
    long extract(T item, long count, bool simulation);

}

public interface IItemHandler : IHandler<Item> {

}

public interface IFluidHandler : IHandler<Fluid> {

}

public interface IEnergyHandler : IHandler<OriginalVis> {

}
