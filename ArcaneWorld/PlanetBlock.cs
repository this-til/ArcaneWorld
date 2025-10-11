using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Array = Godot.Collections.Array;

namespace ArcaneWorld;

[ClassName]
public partial class PlanetBlock : Node3D {

    public Planet planet { get; set; } = null!;

    [ExportGroup("Planet Block Base")]
    [Export]
    public PlanetDirection planetDirection { get; set; }

    [Export]
    public Vector3 position { get; set; }

    [Export]
    public Vector3 left { get; set; }

    [Export]
    public Vector3 forward { get; set; }

    [Export]
    public float size { get; set; }

    [Export]
    public int resolution { get; set; }

    [ExportGroup("LOD Settings")]
    [Export]
    public int currentDepth { get; set; }

    [Export]
    public int maxDepth { get; set; }

    public MeshInstance3D meshInstance { get; protected set; } = null!;

    public StaticBody3D staticBody { get; protected set; } = null!;

    public CollisionShape3D collisionShape { get; protected set; } = null!;

    public PlanetBlock? parent { get; set; } = null!;

    public PlanetBlock[]? children { get; set; } = null!;

    public bool isLeaf => children == null || children.Length == 0;

    protected bool _visible;

    public bool visible {
        get => _visible;
        set {
            _visible = value;
            if (meshInstance != null) {
                meshInstance.Visible = visible;
            }
            if (staticBody != null) {
                staticBody.SetDeferred("freeze", !visible);
            }
        }
    }

    public float lodSize => size * planet.lodThreshold;

    public override void _Ready() {
        setupComponents();
    }

    private void setupComponents() {
        meshInstance = new MeshInstance3D();
        AddChild(meshInstance);

        staticBody = new StaticBody3D();
        AddChild(staticBody);

        collisionShape = new CollisionShape3D();
        staticBody.AddChild(collisionShape);
    }

    public async Task initialize
    (
        Planet planet,
        PlanetDirection direction,
        Vector3 position,
        Vector3 left,
        Vector3 forward,
        float size,
        int resolution,
        int currentDepth,
        int maxDepth
    ) {
        GD.Print($"初始化 PlanetBlock: {direction}, 深度: {currentDepth}/{maxDepth}");
        
        this.planet = planet;
        this.planetDirection = direction;
        this.left = left;
        this.forward = forward;
        this.size = size;
        this.resolution = resolution;
        this.currentDepth = currentDepth;
        this.maxDepth = maxDepth;

        // 调整位置到左下角
        this.position = position;
        this.position -= left * (size * 0.5f);
        this.position -= forward * (size * 0.5f);

        // 确保组件已创建
        if (meshInstance == null) {
            setupComponents();
        }

        // 从最细节开始生成
        await generateHierarchy();
        
        // 顶层块默认可见，子块由LOD系统控制
        visible = (currentDepth == 0);
        
        GD.Print($"PlanetBlock {direction} 初始化完成，可见: {visible}");
    }

    private async Task generateHierarchy() {
        if (currentDepth >= maxDepth) {
            // 达到最大深度，生成叶子节点的原始网格
            await generateMesh();
            return;
        }

        // 创建4个子区块
        await subdivide();

        // 基于子节点的网格生成父节点的合并网格
        await generateMeshFromChildren();
    }

    private async Task subdivide() {
        await GDTask.SwitchToMainThread();

        children = new PlanetBlock[4];
        float halfScale = size * 0.5f;
        float quarterScale = size * 0.25f;

        // position现在是左下角，需要计算中心位置
        Vector3 centerPos = position;
        centerPos += left * halfScale;
        centerPos += forward * halfScale;

        Vector3 stepLeft = left * quarterScale;
        Vector3 stepForward = forward * quarterScale;

        List<Task> tasks = new List<Task>();

        // 创建4个子区块
        for (int i = 0; i < 4; i++) {
            PlanetBlock childBlock = new PlanetBlock() {
                Name = $"Block_{planetDirection}_{currentDepth}_{i}"
            };
            AddChild(childBlock);

            children[i] = childBlock;
            childBlock.parent = this;

            Vector3 childPos = centerPos;
            switch (i) {
                case 0:
                    childPos += -stepLeft + stepForward; // 左上
                    break;
                case 1:
                    childPos += stepLeft + stepForward; // 右上
                    break;
                case 2:
                    childPos += -stepLeft - stepForward; // 左下
                    break;
                case 3:
                    childPos += stepLeft - stepForward; // 右下
                    break;
            }

            tasks.Add(
                childBlock.initialize(
                    planet,
                    planetDirection,
                    childPos,
                    left,
                    forward,
                    halfScale,
                    resolution,
                    currentDepth + 1,
                    maxDepth
                )
            );
        }

        await Task.WhenAll(tasks);
    }

    private async Task generateMesh() {
        GD.Print($"开始生成网格: {Name}, 分辨率: {resolution}");
        await GDTask.SwitchToThreadPool();

        // 在后台线程生成网格数据
        int vertCount = (resolution + 1) * (resolution + 1);
        int triCount = resolution * resolution * 6;

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[triCount];

        // 生成顶点和UV
        generateVertices(vertices, uvs);

        // 生成三角形
        generateTriangles(triangles);

        // 回到主线程创建网格
        await GDTask.SwitchToMainThread();

        ArrayMesh mesh = new ArrayMesh();
        Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.TexUV] = uvs;
        arrays[(int)Mesh.ArrayType.Index] = triangles;

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        mesh.SurfaceSetName(0, $"Mesh_{Name}");

        meshInstance.Mesh = mesh;

        // 创建碰撞形状
        ConcavePolygonShape3D? shape = mesh.CreateTrimeshShape();
        collisionShape.Shape = shape;

        // 设置材质
        if (meshInstance.MaterialOverride == null) {
            StandardMaterial3D material = new StandardMaterial3D();
            material.AlbedoColor = Colors.Gray;
            meshInstance.MaterialOverride = material;
        }
        
        GD.Print($"网格生成完成: {Name}, 顶点数: {vertCount}, 三角形数: {triCount/3}");
    }

    private void generateVertices(Vector3[] vertices, Vector2[] uvs) {
        float uvFactor = 1.0f / resolution;
        int verticesPerRow = resolution + 1;
        float planetRadius = planet.size * 0.5f;

        for (int z = 0; z <= resolution; z++) {
            for (int x = 0; x <= resolution; x++) {
                int index = z * verticesPerRow + x;

                float px = (float)x / resolution;
                float pz = (float)z / resolution;
                Vector3 vx = left * (px * size);
                Vector3 vz = forward * (pz * size);

                uvs[index] = new Vector2(x * uvFactor, z * uvFactor);
                Vector3 worldPos = position + vx + vz;

                // 添加噪声
                float noiseValue = generateNoise(worldPos, 2, 1.7f, 0.1f, size / 16);
                vertices[index] = worldPos.Normalized() * (planetRadius + noiseValue);
            }
        }
    }

    private void generateTriangles(int[] triangles) {
        int triIndex = 0;
        int verticesPerRow = resolution + 1;

        for (int z = 0; z < resolution; z++) {
            for (int x = 0; x < resolution; x++) {
                int vi = z * verticesPerRow + x;

                // 第一个三角形
                triangles[triIndex] = vi;
                triangles[triIndex + 1] = vi + verticesPerRow;
                triangles[triIndex + 2] = vi + 1;

                // 第二个三角形
                triangles[triIndex + 3] = vi + 1;
                triangles[triIndex + 4] = vi + verticesPerRow;
                triangles[triIndex + 5] = vi + verticesPerRow + 1;

                triIndex += 6;
            }
        }
    }

    private float generateNoise(Vector3 point, int octaves, float lacunarity, float gain, float warp) {
        float sum = 0.0f, freq = 1.0f, amp = 1.0f;

        // 使用Godot的噪声系统
        FastNoiseLite noise = new FastNoiseLite();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;

        // 基础噪声层
        for (int o = 0; o < octaves; o++) {
            sum += amp * noise.GetNoise3D(point.X * freq * 0.03f, point.Y * freq * 0.03f, point.Z * freq * 0.03f);
            freq *= lacunarity;
            amp *= gain;
        }

        // 山脉噪声层
        float ridgeNoise = 1.0f - Mathf.Abs(noise.GetNoise3D(point.X * freq * 0.003f, point.Y * freq * 0.003f, point.Z * freq * 0.003f));
        ridgeNoise = Mathf.Pow(ridgeNoise, 2.0f);

        sum *= ridgeNoise * warp * octaves;

        return sum;
    }

    private async Task generateMeshFromChildren() {
        if (children == null || children.Length != 4) {
            GD.PrintErr("无法从子节点生成网格：子节点数量不正确");
            return;
        }

        await GDTask.SwitchToMainThread();

        // 获取子节点的顶点数据
        Vector3[][] childVertices = new Vector3[4][];
        for (int i = 0; i < 4; i++) {
            if (children[i].meshInstance?.Mesh is ArrayMesh arrayMesh) {
                Array? childArrays = arrayMesh.SurfaceGetArrays(0);
                childVertices[i] = childArrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            }
        }

        await GDTask.SwitchToThreadPool();

        // 在后台线程生成合并网格
        int vertCount = (resolution + 1) * (resolution + 1);
        int triCount = resolution * resolution * 6;

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[triCount];

        // 从子节点生成顶点
        generateVerticesFromChildren(vertices, uvs, childVertices);

        // 生成三角形
        generateTriangles(triangles);

        // 回到主线程创建网格
        await GDTask.SwitchToMainThread();

        ArrayMesh mesh = new ArrayMesh();
        Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.TexUV] = uvs;
        arrays[(int)Mesh.ArrayType.Index] = triangles;

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        mesh.SurfaceSetName(0, $"MergedMesh_{Name}");

        meshInstance.Mesh = mesh;

        // 创建碰撞形状
        ConcavePolygonShape3D? shape = mesh.CreateTrimeshShape();
        collisionShape.Shape = shape;

        // 设置材质
        if (meshInstance.MaterialOverride == null) {
            StandardMaterial3D material = new StandardMaterial3D();
            material.AlbedoColor = Colors.Gray;
            meshInstance.MaterialOverride = material;
        }
    }

    private void generateVerticesFromChildren(Vector3[] vertices, Vector2[] uvs, Vector3[][] childVertices) {
        float uvFactor = 1.0f / resolution;
        int verticesPerRow = resolution + 1;

        for (int z = 0; z <= resolution; z++) {
            for (int x = 0; x <= resolution; x++) {
                int index = z * verticesPerRow + x;
                uvs[index] = new Vector2(x * uvFactor, z * uvFactor);
                vertices[index] = sampleVertexFromChildren(x, z, childVertices);
            }
        }
    }

    private Vector3 sampleVertexFromChildren(int x, int z, Vector3[][] childVertices) {
        int halfRes = resolution / 2;

        // 确定采样的子节点和在子节点中的坐标
        Vector3[] vertices;
        int childX, childZ;

        if (x <= halfRes && z <= halfRes) {
            // 左下 - 子节点[2]
            vertices = childVertices[2];
            childX = x * 2;
            childZ = z * 2;
        }
        else if (x > halfRes && z <= halfRes) {
            // 右下 - 子节点[3]
            vertices = childVertices[3];
            childX = (x - halfRes) * 2;
            childZ = z * 2;
        }
        else if (x <= halfRes && z > halfRes) {
            // 左上 - 子节点[0]
            vertices = childVertices[0];
            childX = x * 2;
            childZ = (z - halfRes) * 2;
        }
        else {
            // 右上 - 子节点[1]
            vertices = childVertices[1];
            childX = (x - halfRes) * 2;
            childZ = (z - halfRes) * 2;
        }

        // 确保坐标在有效范围内
        childX = Mathf.Clamp(childX, 0, resolution);
        childZ = Mathf.Clamp(childZ, 0, resolution);

        int vertIndex = childZ * (resolution + 1) + childX;

        if (vertices != null && vertIndex >= 0 && vertIndex < vertices.Length) {
            return vertices[vertIndex];
        }

        // 回退：如果采样失败，使用基本计算
        float px = (float)x / resolution;
        float pz = (float)z / resolution;
        Vector3 vx = left * (px * size);
        Vector3 vz = forward * (pz * size);
        Vector3 pos = position + vx + vz;
        return pos.Normalized() * size;
    }

    public void eliminateLevel() {
        visible = false;
        if (children == null) {
            return;
        }
        foreach (PlanetBlock child in children) {
            child.eliminateLevel();
            child.visible = false;
        }
    }

    public void updateLOD(Camera3D camera) {
        if (meshInstance is null) {
            return;
        }
        Aabb bounds = meshInstance.GetAabb();

        Vector3 closestPoint = getClosestPointOnAabb(bounds, camera.GlobalPosition);
        float dist = camera.GlobalPosition.DistanceTo(closestPoint);

        // 计算视角因子
        float viewAngleFactor = calculateViewAngleFactor(camera, closestPoint);

        // 根据视角调整有效LOD距离
        float adjustedLodSize = lodSize * viewAngleFactor;

        if (dist > adjustedLodSize || isLeaf) {
            // 距离太远、视角不佳或者是叶子节点，显示当前块
            eliminateLevel();
            visible = true;
            return;
        }

        // 距离较近且视角良好，显示子块（更高细节）
        visible = false;

        if (children != null) {
            foreach (PlanetBlock child in children) {
                child.updateLOD(camera);
            }
        }
    }

    private float calculateViewAngleFactor(Camera3D camera, Vector3 targetPoint) {
        Vector3 cameraPos = camera.GlobalPosition;
        Vector3 cameraForward = -camera.GlobalTransform.Basis.Z;

        // 计算从摄像机到目标点的方向
        Vector3 directionToTarget = (targetPoint - cameraPos).Normalized();

        // 计算视角的cos值（点积）
        float cosAngle = cameraForward.Dot(directionToTarget);

        // 使用FOV来调整视角敏感度
        float fovFactor = camera.Fov / 60.0f; // 标准化到60度FOV

        // 早退：如果完全在视野外（背后），直接返回低因子
        if (cosAngle < -0.2f) {
            return 0;
        }

        // 计算基础因子
        float baseFactor = Mathf.Lerp(0, 1, (cosAngle + 1.0f) * 0.5f);

        // 根据FOV调整
        return baseFactor * fovFactor;
    }

    /// <summary>
    /// 计算AABB上到指定点的最近点
    /// </summary>
    private Vector3 getClosestPointOnAabb(Aabb aabb, Vector3 point) {
        Vector3 min = aabb.Position;
        Vector3 max = aabb.Position + aabb.Size;

        Vector3 closestPoint = new Vector3(
            Mathf.Clamp(point.X, min.X, max.X),
            Mathf.Clamp(point.Y, min.Y, max.Y),
            Mathf.Clamp(point.Z, min.Z, max.Z)
        );

        return closestPoint;
    }

}
