using ArcaneWorld.Util;
using ArcaneWorld.Util.Extensions;
using CommonUtil.Extensions;
using Godot;

namespace ArcaneWorld.Planet;

public partial class Tile {

    public required Planet planet { get; init; }

    public int id { get; internal set; }

    public required Point point { get; init; }

    public required Chunk chunk { get; init; }

    public IReadOnlyList<Face> faces => point.faces;

    public List<Tile> _neighbors = null!;

    // 已确保顺序和 faces 对应，每个邻居共边的顶点是 HexFaceIds[i] 和 HexFaceIds[(i + 1) % HexFaceIds.Count]
    public IReadOnlyList<Tile> neighbors => _neighbors;

    internal void initNeighbors() {
        // TODO
        _neighbors = point.neighbor.Select(p => planet.pointTileMap[p]).ToList();
    }

    public bool isPentagon => faces.Count == 5;

    public bool isUnderWater => waterHeight > height;

    public int neighborCount => neighbors.Count;

    public Tile getNeighbor(int index, int offset = 0) => _neighbors[(index + offset).Wrap(_neighbors.Count)];

    /// <summary>
    /// 按照 tile 高度查询 idx (顺时针第一个)角落的位置，相对于 Tile 中心进行插值 size 的缩放。
    /// </summary>
    public Vector3 getFirstCorner(int index, float radius = 1f, float size = 1f, int offset = 0) {
        return Math3dUtil.ProjectToSphere(
            point.unitCentroid.Lerp(
                faces[(index + offset).Wrap(faces.Count)].center,
                size
            ),
            radius
        );
    }

    public float height { get; set; }

    public float waterHeight { get; set; }

    public virtual void generateGrid(ChunkLod lod, GenerateChunkGridTool generateChunkGridTool) {
        if (lod == ChunkLod.JustHex) {
            triangulateJustHex(generateChunkGridTool);
            return;
        }

        if (lod == ChunkLod.PlaneHex) {
            triangulatePlaneHex(generateChunkGridTool);
            return;
        }

    }

    /// <summary>
    /// 仅绘制六边形（无扰动，点平均周围地块高度）
    /// </summary>
    public virtual void triangulateJustHex(GenerateChunkGridTool generateChunkGridTool) {
        float radius = planet.radius;

        Vector3 ids = Vector3.One * id;
        Tile preNeighbor = getNeighbor(0, -1);
        Tile neighbor = getNeighbor(0);
        Tile nextNeighbor = getNeighbor(0, 1);
        Vector3 v0 = Vector3.Zero;
        Vector3 vw0 = Vector3.Zero;

        for (int i = 0; i < faces.Count; i++) {

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

            Vector3 v1 = getFirstCorner(i, radius + avgHeight1);

            if (i == 0) {
                v0 = v1;
            }

            Vector3 v2 = getFirstCorner(i, radius + avgHeight2, offset: 1);
            Vector3 vw1 = getFirstCorner(i, radius + avgWaterHeight1);

            if (i == 0) {
                vw0 = vw1;
            }

            Vector3 vw2 = getFirstCorner(i, radius + avgWaterHeight2, offset: 1);

            if (i > 0 && i < faces.Count - 1) {

                generateChunkGridTool.terrain.AddTriangleFan(
                    [v0, v1, v2],
                    colors: MeshConstant.QuadArr(MeshConstant.Weights1, MeshConstant.Weights2)
                );

                
                // 绘制地面
                /*generateChunkGridTool.terrain.AddTriangleUnperturbed(
                    [v0, v1, v2],
                    MeshConstant.QuadArr(MeshConstant.Weights1, MeshConstant.Weights2),
                    tis: ids
                );
                */

                // 绘制水面
                if (isUnderWater) {
                    generateChunkGridTool.water.AddTriangleFan(
                        [vw0, vw1, vw2],
                        colors: MeshConstant.TriArr(MeshConstant.Weights1)
                    );

                    /*generateChunkGridTool.water.AddTriangleUnperturbed(
                        [vw0, vw1, vw2],
                        MeshConstant.TriArr(MeshConstant.Weights1),
                        tis: ids
                    );*/
                }
            }

            preNeighbor = neighbor;
            neighbor = nextNeighbor;
            nextNeighbor = getNeighbor(i, 2);
        }
    }

    /// <summary>
    /// 绘制平面六边形（有高度立面、处理接缝、但无特征、无河流）
    /// </summary>
    public virtual void triangulatePlaneHex(GenerateChunkGridTool generateChunkGridTool) {

    }

}
