using CakeToolset.Attribute;
using CakeToolset.Global;
using Fractural.Tasks;
using Fractural.Tasks.Internal;
using Godot;

[Log]
[Tool]
public partial class S_GDTaskPlayerLoopAutoload : Node, Fractural.Tasks.GDTaskPlayerLoopAutoload , ISerializationListener {

    public void LocalAddAction(PlayerLoopTiming timing, IPlayerLoopItem action) {
        PlayerLoopRunner runner = runners[(int)timing];
        if (runner == null) {
            GDTaskPlayerLoopAutoload.ThrowInvalidLoopTiming(timing);
        }
        runner!.AddAction(action);
    }

    // NOTE: Continuation means a asynchronous task invoked by another task after the other task finishes.
    public void LocalAddContinuation(PlayerLoopTiming timing, Action continuation) {
        ContinuationQueue q = yielders[(int)timing];
        if (q == null) {
            GDTaskPlayerLoopAutoload.ThrowInvalidLoopTiming(timing);
        }
        q!.Enqueue(continuation);
    }

    public double DeltaTime => GetProcessDeltaTime();

    public double PhysicsDeltaTime => GetPhysicsProcessDeltaTime();

    public int mainThreadId { get; private set; }

    private bool isInitialized = false;

    private ContinuationQueue[] yielders = null!;

    private PlayerLoopRunner[] runners = null!;

    private ProcessListener processListener = null!;

    public override void _Ready() {
        base._Ready();
        Initialize();
    }

    private void Initialize() {
        GDTaskPlayerLoopAutoloadHold.Global = this;

        ProcessMode = ProcessModeEnum.Pausable;
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
        yielders = new[] {
            new ContinuationQueue(PlayerLoopTiming.Process),
            new ContinuationQueue(PlayerLoopTiming.PhysicsProcess),
            new ContinuationQueue(PlayerLoopTiming.PauseProcess),
            new ContinuationQueue(PlayerLoopTiming.PausePhysicsProcess),
        };
        runners = new[] {
            new PlayerLoopRunner(PlayerLoopTiming.Process),
            new PlayerLoopRunner(PlayerLoopTiming.PhysicsProcess),
            new PlayerLoopRunner(PlayerLoopTiming.PauseProcess),
            new PlayerLoopRunner(PlayerLoopTiming.PausePhysicsProcess),
        };
        processListener = new ProcessListener();
        AddChild(processListener);
        processListener.ProcessMode = ProcessModeEnum.Always;
        processListener.OnProcess += PauseProcess;
        processListener.OnPhysicsProcess += PausePhysicsProcess;

        isInitialized = true;
    }

    public override void _Process(double delta) {
        // 程序集重新加载期间组件可能未初始化
        if (!isInitialized) {
            return;
        }

        yielders[(int)PlayerLoopTiming.Process].Run();
        runners[(int)PlayerLoopTiming.Process].Run();
    }

    public override void _PhysicsProcess(double delta) {
        // 程序集重新加载期间组件可能未初始化
        if (!isInitialized) {
            return;
        }

        yielders[(int)PlayerLoopTiming.PhysicsProcess].Run();
        runners[(int)PlayerLoopTiming.PhysicsProcess].Run();
    }

    private void PauseProcess(double delta) {
        // 程序集重新加载期间组件可能未初始化
        if (!isInitialized) {
            return;
        }

        yielders[(int)PlayerLoopTiming.PauseProcess].Run();
        runners[(int)PlayerLoopTiming.PauseProcess].Run();
    }

    private void PausePhysicsProcess(double delta) {
        // 程序集重新加载期间组件可能未初始化
        if (!isInitialized) {
            return;
        }

        yielders[(int)PlayerLoopTiming.PausePhysicsProcess].Run();
        runners[(int)PlayerLoopTiming.PausePhysicsProcess].Run();
    }

    public void OnBeforeSerialize() {
        log.Info("GDTaskPlayerLoopAutoload OnBeforeSerialize...");

        // 标记为未初始化状态
        isInitialized = false;

        // 程序集重新加载前清理所有运行时状态
        // 静态引用会在程序集重新加载时自动重置，这里主动清理确保状态一致
        GDTaskPlayerLoopAutoloadHold.Global = null;

        if (yielders != null) {
            foreach (ContinuationQueue yielder in yielders) {
                yielder.Clear();
            }
            yielders = null!;
        }
        if (runners != null) {
            foreach (PlayerLoopRunner runner in runners) {
                runner.Clear();
            }
            runners = null!;
        }

        // 清理子节点和事件订阅
        if (processListener != null) {
            processListener.OnProcess -= PauseProcess;
            processListener.OnPhysicsProcess -= PausePhysicsProcess;
            processListener.Free();
            processListener = null!;
        }

    }

    public void OnAfterDeserialize() {
        log.Info("GDTaskPlayerLoopAutoload OnAfterDeserialize...");
        Initialize();
    }

}
