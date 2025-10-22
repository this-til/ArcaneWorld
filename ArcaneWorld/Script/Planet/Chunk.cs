using ArcaneWorld.Nodes;
using ArcaneWorld.Util;
using Fractural.Tasks;
using Godot;

namespace ArcaneWorld.Planet;

public partial class Chunk : Node3D {

    public required Planet planet { get; init; }

    public int id { get; internal set; }

    public required Point point { get; init; }

    // 已确保顺序为顺时针方向
    public IReadOnlyList<Face> faces => point.faces;

    public List<Chunk> _neighbors = null!;

    public IReadOnlyList<Chunk> neighbors => _neighbors;

    internal List<Tile> _tiles = null!;

    public IReadOnlyList<Tile> tiles => _tiles;

    public bool insight { get; set; }

    public ChunkLod lod { get; set; } = ChunkLod.JustHex;

    internal void initNeighbors() {
        _neighbors = point.neighbor.Select(p => planet.pointChunkMap[p]).ToList();
    }

    // 陆地网格
    public MeshInstance3D terrain { get; private set; } = null!;

    public MeshInstance3D water { get; private set; } = null!;

    /// <summary>
    /// 生成网格数据（纯计算，线程安全）
    /// 基于chunk内所有tiles生成完整的网格
    /// </summary>
    public async Task generateMesh() {
        if (terrain is null) {
            await GDTask.SwitchToMainThread();
            terrain = new MeshInstance3D() { Name = "Terrain" };
            AddChild(terrain);
        }

        if (water is null) {
            await GDTask.SwitchToMainThread();
            water = new MeshInstance3D() { Name = "Water" };
            AddChild(water);
        }

        if (_tiles == null || _tiles.Count < 3) {
            return;
        }

        await GDTask.SwitchToMainThread();

        GenerateChunkGridTool generateChunkGridTool = new GenerateChunkGridTool() {
            terrain = new SurfaceTool(),
            water = new SurfaceTool()
        };

        generateChunkGridTool.terrain.Begin(Mesh.PrimitiveType.Triangles);
        generateChunkGridTool.water.Begin(Mesh.PrimitiveType.Triangles);

        await GDTask.SwitchToThreadPool();

        foreach (Tile tile in tiles) {
            tile.generateGrid(lod, generateChunkGridTool);
        }

        await GDTask.SwitchToMainThread();

        generateChunkGridTool.terrain.GenerateNormals();
        generateChunkGridTool.water.GenerateNormals();

        terrain.Mesh = generateChunkGridTool.terrain.Commit();
        water.Mesh = generateChunkGridTool.water.Commit();

        generateChunkGridTool.terrain.Clear();
        generateChunkGridTool.water.Clear();

    }



    /// <summary>
    /// 仅绘制六边形（无扰动，点平均周围地块高度）
    /// </summary>
    /*private void triangulateJustHex(Tile tile) {

        float radius = planet.radius;

        Vector3 ids = Vector3.One * tile.id;
        float height = tile.height;
        float waterHeight = tile.waterHeight;
        Tile preNeighbor = tile.getNeighbor(0, -1);
        Tile neighbor = tile.getNeighbor(0);
        Tile nextNeighbor = tile.getNeighbor(0, 1);
        Vector3 v0 = Vector3.Zero;
        Vector3 vw0 = Vector3.Zero;

        for (int i = 0; i < tile.faces.Count; i++) {

            float neighborHeight = neighbor.height;
            float neighborWaterHeight = neighbor.waterHeight;
            float preHeight = preNeighbor.height;
            float preWaterHeight = preNeighbor.waterHeight;
            float nextHeight = nextNeighbor.height;
            float nextWaterHeight = nextNeighbor.waterHeight;

            float avgHeight1 = (preHeight + neighborHeight + height) / 3f;
            float avgHeight2 = (neighborHeight + nextHeight + height) / 3f;
            float avgWaterHeight1 = (preWaterHeight + neighborWaterHeight + waterHeight) / 3f;
            float avgWaterHeight2 = (neighborWaterHeight + nextWaterHeight + waterHeight) / 3f;

            Vector3 v1 = tile.getFirstCorner(i, radius + avgHeight1);

            if (i == 0) {
                v0 = v1;
            }

            Vector3 v2 = tile.getFirstCorner(i, radius + avgHeight2, offset: 1);
            Vector3 vw1 = tile.getFirstCorner(i, radius + avgWaterHeight1);

            if (i == 0) {
                vw0 = vw1;
            }

            Vector3 vw2 = tile.getFirstCorner(i, radius + avgWaterHeight2, offset: 1);

            if (i > 0 && i < tile.faces.Count - 1) {

                // 绘制地面
                terrain.AddTriangleUnperturbed(
                    [v0, v1, v2],
                    MeshConstant.QuadArr(MeshConstant.Weights1, MeshConstant.Weights2),
                    tis: ids
                );

                // 绘制水面
                if (tile.isUnderWater) {
                    water.AddTriangleUnperturbed(
                        [vw0, vw1, vw2],
                        MeshConstant.TriArr(MeshConstant.Weights1),
                        tis: ids
                    );
                }
            }

            preNeighbor = neighbor;
            neighbor = nextNeighbor;
            nextNeighbor = tile.getNeighbor(i, 2);
        }

    }*/

}

public class GenerateChunkGridTool {

    public required SurfaceTool terrain { get; init; }

    public required SurfaceTool water { get; init; }

}

public enum ChunkLod {

    JustHex, // 每个地块只有六个平均高度点组成的六边形（非平面）

    PlaneHex, // 高度立面，无特征，无河流的六边形

    SimpleHex, // 最简单的 Solid + 斜面六边形 

    TerracesHex, // 增加台阶

    Full, // 增加边细分

}
