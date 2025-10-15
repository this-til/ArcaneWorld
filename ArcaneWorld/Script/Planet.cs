using Fractural.Tasks;
using Godot;
using System;
using System.Threading.Tasks;
using Godot.Collections;

namespace ArcaneWorld;

[ClassName]
public partial class Planet : Node3D {

    [ExportGroup("Planet Base")]
    [Export]
    public int size { get; set; } = 1024;

    [Export]
    public int resolution { get; set; } = 32;

    [Export]
    public int split { get; set; } = 4;

    [ExportGroup("LOD Setting")]
    [Export]
    public float lodThreshold { get; set; } = 0.5f;

    // LOD更新间隔（秒）
    [Export]
    public float lodUpdateInterval { get; set; } = 0.1f;

    [ExportGroup("Other")]
    [Export]
    public bool isGenerated { get; set; }

    [Export]
    public PlanetBlock[] faces { get; set; } = new PlanetBlock[6];

    protected float lastLODUpdate;

    public override void _Ready() {
        // 延迟调用星球生成，避免在节点初始化期间添加子节点
        CallDeferred(nameof(startPlanetGeneration));
    }

    private void startPlanetGeneration() {
        // 自动开始生成星球
        GD.Print("开始生成星球...");
        _ = createTree();
    }

    public override void _Process(double delta) {
        if (!isGenerated) {
            return;
        }

        float currentTime = (float)Time.GetTicksMsec() / 1000.0f;
        if (currentTime - lastLODUpdate > lodUpdateInterval) {
            updateLOD();
            lastLODUpdate = currentTime;
        }
    }

    public async Task createTree() {
        await GDTask.SwitchToMainThread();
        GD.Print($"创建星球树，大小: {size}, 分辨率: {resolution}, 分割层数: {split}");

        var hs = size / 2f;

        PlanetBlock[] all = await Task.WhenAll(
            createFace(PlanetDirection.front, new Vector3(0, 0, -hs), new Vector3(-1, 0, 0), new Vector3(0, -1, 0)),
            createFace(PlanetDirection.back, new Vector3(0, 0, hs), new Vector3(1, 0, 0), new Vector3(0, -1, 0)),
            createFace(PlanetDirection.left, new Vector3(-hs, 0, 0), new Vector3(0, 0, 1), new Vector3(0, -1, 0)),
            createFace(PlanetDirection.right, new Vector3(hs, 0, 0), new Vector3(0, 0, -1), new Vector3(0, -1, 0)),
            createFace(PlanetDirection.top, new Vector3(0, hs, 0), new Vector3(0, 0, 1), new Vector3(-1, 0, 0)),
            createFace(PlanetDirection.bottom, new Vector3(0, -hs, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0))
        );
        for (int i = 0; i < all.Length; ++i) {
            faces[i] = all[i];
        }

        await GDTask.SwitchToMainThread();
        GD.Print($"星球生成完成，共 {all.Length} 个面");
        isGenerated = true;
    }

    private async Task<PlanetBlock> createFace(PlanetDirection direction, Vector3 position, Vector3 left, Vector3 forward) {
        await GDTask.SwitchToMainThread();
        GD.Print($"创建面: {direction}");

        PlanetBlock planetBlock = new PlanetBlock() {
            Name = $"Face_{direction}",
            planet = this
        };

        AddChild(planetBlock);

        // 初始化面的参数
        await planetBlock.initialize(
            this,
            direction,
            position,
            left,
            forward,
            size,
            resolution,
            0,
            split
        );

        GD.Print($"面 {direction} 创建完成");
        return planetBlock;
    }

    /// <summary>
    /// 更新所有面的LOD（带视锥和背面剔除）
    /// </summary>
    public void updateLOD() {
        Camera3D? camera = findMainCamera();
        if (camera == null) {
            GD.PrintErr("找不到主摄像机，LOD更新失败");
            return;
        }

        foreach (PlanetBlock planetBlock in faces) {
            planetBlock?.updateLOD(camera);
        }
    }

    /// <summary>
    /// 查找主摄像机
    /// </summary>
    private Camera3D? findMainCamera() {
        // 首先尝试通过组找到标记为"MainCamera"的相机
        Array<Node>? cameras = GetTree().GetNodesInGroup("MainCamera");
        if (cameras.Count > 0 && cameras[0] is Camera3D mainCamera) {
            return mainCamera;
        }

        // 如果没有找到，查找当前视口的相机
        Viewport? viewport = GetViewport();
        if (viewport?.GetCamera3D() != null) {
            return viewport.GetCamera3D();
        }

        // 最后尝试查找场景中第一个Camera3D
        return GetTree().GetFirstNodeInGroup("Camera3D") as Camera3D;
    }

    /// <summary>
    /// 重新生成星球
    /// </summary>
    public void regeneratePlanet() {
        isGenerated = false;
        foreach (PlanetBlock planetBlock in faces) {
            planetBlock?.QueueFree();
        }
        faces = new PlanetBlock[6];
        _ = createTree();
    }

}

public static class PlanetDirectionExtensions {

    public static Vector3 asPos(this PlanetDirection planetDirection, float planetScale) {
        var hs = planetScale / 2;
        switch (planetDirection) {
            case PlanetDirection.front:
                return new Vector3(0, 0, -hs);
            case PlanetDirection.back:
                return new Vector3(0, 0, hs);
            case PlanetDirection.left:
                return new Vector3(-hs, 0, 0);
            case PlanetDirection.right:
                return new Vector3(hs, 0, 0);
            case PlanetDirection.top:
                return new Vector3(0, hs, 0);
            case PlanetDirection.bottom:
                return new Vector3(0, -hs, 0);
            default:
                return Vector3.Zero;
        }
    }

    public static Vector3 asLeft(this PlanetDirection planetDirection) {
        switch (planetDirection) {
            case PlanetDirection.front:
                return new Vector3(-1, 0, 0);
            case PlanetDirection.back:
                return new Vector3(1, 0, 0);
            case PlanetDirection.left:
                return new Vector3(0, 0, 1);
            case PlanetDirection.right:
                return new Vector3(0, 0, -1);
            case PlanetDirection.top:
                return new Vector3(0, 0, 1);
            case PlanetDirection.bottom:
                return new Vector3(0, 0, -1);
            default:
                return Vector3.Zero;
        }
    }

    public static Vector3 asForward(this PlanetDirection planetDirection) {
        switch (planetDirection) {
            case PlanetDirection.front:
                return new Vector3(0, -1, 0);
            case PlanetDirection.back:
                return new Vector3(0, -1, 0);
            case PlanetDirection.left:
                return new Vector3(0, -1, 0);
            case PlanetDirection.right:
                return new Vector3(0, -1, 0);
            case PlanetDirection.top:
                return new Vector3(-1, 0, 0);
            case PlanetDirection.bottom:
                return new Vector3(-1, 0, 0);
            default:
                return Vector3.Zero;
        }
    }

}

public enum PlanetDirection {

    front,

    back,

    left,

    right,

    top,

    bottom,

}
