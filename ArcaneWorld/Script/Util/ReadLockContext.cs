namespace ArcaneWorld.Util;

/// <summary>
/// 读锁上下文
/// </summary>
public class ReadLockContext : IDisposable {

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

public readonly ref struct StructReadLockContext : IDisposable {

    private readonly ReaderWriterLockSlim rwLock;

    public StructReadLockContext(ReaderWriterLockSlim rwLock) {
        this.rwLock = rwLock;
        rwLock.EnterReadLock();
    }

    public void Dispose() {
        rwLock.ExitReadLock();
    }

}
