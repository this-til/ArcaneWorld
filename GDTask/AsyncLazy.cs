using System;
using System.Threading;

namespace Fractural.Tasks
{
    public partial class AsyncLazy
    {
        static Action<object> continuation = SetCompletionSource;

        Func<GDTask> taskFactory;
        GDTaskCompletionSource completionSource;
        GDTask.Awaiter awaiter;

        object syncLock;
        bool initialized;

        public AsyncLazy(Func<GDTask> taskFactory)
        {
            this.taskFactory = taskFactory;
            this.completionSource = new GDTaskCompletionSource();
            this.syncLock = new object();
            this.initialized = false;
        }

        internal AsyncLazy(GDTask task)
        {
            this.taskFactory = null;
            this.completionSource = new GDTaskCompletionSource();
            this.syncLock = null;
            this.initialized = true;

            GDTask.Awaiter awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                SetCompletionSource(awaiter);
            }
            else
            {
                this.awaiter = awaiter;
                awaiter.SourceOnCompleted(continuation, this);
            }
        }

        public GDTask Task
        {
            get
            {
                EnsureInitialized();
                return completionSource.Task;
            }
        }


        public GDTask.Awaiter GetAwaiter() => Task.GetAwaiter();

        void EnsureInitialized()
        {
            if (Volatile.Read(ref initialized))
            {
                return;
            }

            EnsureInitializedCore();
        }

        void EnsureInitializedCore()
        {
            lock (syncLock)
            {
                if (!Volatile.Read(ref initialized))
                {
                    Func<GDTask> f = Interlocked.Exchange(ref taskFactory, null);
                    if (f != null)
                    {
                        GDTask task = f();
                        GDTask.Awaiter awaiter = task.GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            SetCompletionSource(awaiter);
                        }
                        else
                        {
                            this.awaiter = awaiter;
                            awaiter.SourceOnCompleted(continuation, this);
                        }

                        Volatile.Write(ref initialized, true);
                    }
                }
            }
        }

        void SetCompletionSource(in GDTask.Awaiter awaiter)
        {
            try
            {
                awaiter.GetResult();
                completionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
            }
        }

        static void SetCompletionSource(object state)
        {
            AsyncLazy self = (AsyncLazy)state;
            try
            {
                self.awaiter.GetResult();
                self.completionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                self.completionSource.TrySetException(ex);
            }
            finally
            {
                self.awaiter = default;
            }
        }
    }

    public partial class AsyncLazy<T>
    {
        static Action<object> continuation = SetCompletionSource;

        Func<GDTask<T>> taskFactory;
        GDTaskCompletionSource<T> completionSource;
        GDTask<T>.Awaiter awaiter;

        object syncLock;
        bool initialized;

        public AsyncLazy(Func<GDTask<T>> taskFactory)
        {
            this.taskFactory = taskFactory;
            this.completionSource = new GDTaskCompletionSource<T>();
            this.syncLock = new object();
            this.initialized = false;
        }

        internal AsyncLazy(GDTask<T> task)
        {
            this.taskFactory = null;
            this.completionSource = new GDTaskCompletionSource<T>();
            this.syncLock = null;
            this.initialized = true;

            GDTask<T>.Awaiter awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                SetCompletionSource(awaiter);
            }
            else
            {
                this.awaiter = awaiter;
                awaiter.SourceOnCompleted(continuation, this);
            }
        }

        public GDTask<T> Task
        {
            get
            {
                EnsureInitialized();
                return completionSource.Task;
            }
        }


        public GDTask<T>.Awaiter GetAwaiter() => Task.GetAwaiter();

        void EnsureInitialized()
        {
            if (Volatile.Read(ref initialized))
            {
                return;
            }

            EnsureInitializedCore();
        }

        void EnsureInitializedCore()
        {
            lock (syncLock)
            {
                if (!Volatile.Read(ref initialized))
                {
                    Func<GDTask<T>> f = Interlocked.Exchange(ref taskFactory, null);
                    if (f != null)
                    {
                        GDTask<T> task = f();
                        GDTask<T>.Awaiter awaiter = task.GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            SetCompletionSource(awaiter);
                        }
                        else
                        {
                            this.awaiter = awaiter;
                            awaiter.SourceOnCompleted(continuation, this);
                        }

                        Volatile.Write(ref initialized, true);
                    }
                }
            }
        }

        void SetCompletionSource(in GDTask<T>.Awaiter awaiter)
        {
            try
            {
                T result = awaiter.GetResult();
                completionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
            }
        }

        static void SetCompletionSource(object state)
        {
            AsyncLazy<T> self = (AsyncLazy<T>)state;
            try
            {
                T result = self.awaiter.GetResult();
                self.completionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                self.completionSource.TrySetException(ex);
            }
            finally
            {
                self.awaiter = default;
            }
        }
    }
}
