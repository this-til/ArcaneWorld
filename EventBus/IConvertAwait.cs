using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EventBus;

public interface IConvertAwait {

    /// <summary>
    /// 判断一个type可以被转换成可等待的对象
    /// 判断成功后将缓存接口，以后所有命中的类型都将直接调用convert
    /// </summary>
    bool canConvert(Type type);

    IGetAwaiter? convert(object obj);

}

public interface IGetAwaiter {

    IAwaiter GetAwaiter();

}

public interface IAwaiter : INotifyCompletion {

    bool IsCompleted { get; }

    void GetResult();

}

public class TaskConvertAwaiter : IConvertAwait {

    public static TaskConvertAwaiter instance { get; } = new TaskConvertAwaiter();

    public bool canConvert(Type type) => typeof(Task).IsAssignableFrom(type);

    public IGetAwaiter? convert(object obj) {
        return obj is not Task task
            ? null
            : new TaskGetAwaiter(task);
    }

    class TaskGetAwaiter : IGetAwaiter {

        readonly Task task;

        public TaskGetAwaiter(Task task) {
            this.task = task;
        }

        public IAwaiter GetAwaiter() {
            return new TaskAwaiter(task.GetAwaiter());
        }

    }

    readonly struct TaskAwaiter : IAwaiter {

        readonly System.Runtime.CompilerServices.TaskAwaiter awaiter;

        public TaskAwaiter(System.Runtime.CompilerServices.TaskAwaiter awaiter) {
            this.awaiter = awaiter;
        }

        public void OnCompleted(Action continuation) => awaiter.OnCompleted(continuation);

        public bool IsCompleted => awaiter.IsCompleted;

        public void GetResult() => awaiter.GetResult();

    }

}

public class ValueTaskConvertAwaiter : IConvertAwait {

    public static ValueTaskConvertAwaiter instance { get; } = new ValueTaskConvertAwaiter();

    public bool canConvert(Type type) => typeof(ValueTask).IsAssignableFrom(type);

    public IGetAwaiter? convert(object obj) {
        return obj is not ValueTask valueTask
            ? null
            : new ValueTaskGetAwaiter(valueTask);
    }

    class ValueTaskGetAwaiter : IGetAwaiter {

        private readonly ValueTask valueTask;

        public ValueTaskGetAwaiter(ValueTask valueTask) {
            this.valueTask = valueTask;
        }

        public IAwaiter GetAwaiter() {
            return new ValueTaskAwaiter(valueTask.GetAwaiter());
        }

    }

    readonly struct ValueTaskAwaiter : IAwaiter {

        private readonly System.Runtime.CompilerServices.ValueTaskAwaiter valueTask;

        public ValueTaskAwaiter(System.Runtime.CompilerServices.ValueTaskAwaiter valueTask) {
            this.valueTask = valueTask;
        }

        public bool IsCompleted => valueTask.IsCompleted;

        public void GetResult() {
            valueTask.GetResult();
        }

        public void OnCompleted(Action continuation) {
            valueTask.OnCompleted(continuation);
        }

    }

}

/*public class GenericValueTaskConvertAwaiter : IConvertAwait {

public static GenericValueTaskConvertAwaiter instance { get; } = new GenericValueTaskConvertAwaiter();

protected readonly ConcurrentDictionary<Type, Func<object, IGetAwaiter?>> convertCache = new ConcurrentDictionary<Type, Func<object, IGetAwaiter?>>();

public bool canConvert(Type type) {
    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);
}

public IGetAwaiter? convert(object obj) {
    if (obj == null) {
        return null;
    }

    return convertCache.GetOrAdd
        (
            obj.GetType(),
            type => {
                Type resultType = type.GetGenericArguments()[0];

                MethodInfo? method = typeof(GenericValueTaskConvertAwaiter)
                    .GetMethod(nameof(createConverter), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(resultType);

                return (Func<object, IGetAwaiter?>)method?.CreateDelegate(typeof(Func<object, IGetAwaiter?>))!;

            }
        )
        ?.Invoke(obj);
}

private static IGetAwaiter createConverter<T>(object obj) => new GenericValueTaskGetAwaiter<T>((ValueTask<T>)obj);

class GenericValueTaskGetAwaiter<TResult> : IGetAwaiter {

    private readonly ValueTask<TResult> awaiter;

    public GenericValueTaskGetAwaiter(ValueTask<TResult> awaiter) {
        this.awaiter = awaiter;
    }

    public IAwaiter GetAwaiter() {
        return new GenericValueTaskAwaiter<TResult>(awaiter.GetAwaiter());
    }

}

readonly struct GenericValueTaskAwaiter<TResult> : IAwaiter {

    private readonly ValueTaskAwaiter<TResult> awaiter;

    public GenericValueTaskAwaiter(ValueTaskAwaiter<TResult> awaiter) {
        this.awaiter = awaiter;
    }

    public bool IsCompleted => awaiter.IsCompleted;

    public void GetResult() {
        awaiter.GetResult();
    }

    public void OnCompleted(Action continuation) {
        awaiter.OnCompleted(continuation);
    }

}

}*/