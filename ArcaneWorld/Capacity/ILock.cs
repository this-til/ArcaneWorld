namespace ArcaneWorld.Capacity;

public interface ILock {

    /// 获取读锁，用于批量读取操作
    /// </summary>
    /// <returns>读锁上下文，需要在 using 语句中使用</returns>
    public IDisposable lockForRead();

    /// <summary>
    /// 获取写锁，用于批量写入操作
    /// </summary>
    /// <returns>写锁上下文，需要在 using 语句中使用</returns>
    public IDisposable lockForWrite();

}
