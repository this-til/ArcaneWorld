namespace ArcaneWorld.Util;

/// <summary>
/// 写锁上下文
/// </summary>
public class WriteLockContext : IDisposable {

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

public readonly ref struct StructWriteLockContext : IDisposable {

    private readonly ReaderWriterLockSlim rwLock;

    public StructWriteLockContext(ReaderWriterLockSlim rwLock) {
        this.rwLock = rwLock;
        rwLock.EnterWriteLock();
    }

    public void Dispose() {
        rwLock.ExitWriteLock();
    }

}
