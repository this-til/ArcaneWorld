using Godot;

namespace ArcaneWorld.Planet;

public class Chunk {

    public int id { get; internal set; }

    public required Planet planet { get; init; }

    public required Point point { get; init; }

    //这里存储是实际地块中心位置（带有星球半径）
    public required Vector3 planetPosition { get; init; }

    // 已确保顺序为顺时针方向
    public IReadOnlyList<Face> faces => point.faces;

    public List<Chunk> _neighbors = null!;

    // 已确保顺序和 faces 对应，每个邻居共边的顶点是 HexFaceIds[i] 和 HexFaceIds[(i + 1) % HexFaceIds.Count]
    public IReadOnlyList<Chunk> neighbors => _neighbors;

    internal List<Tile> _tiles;

    public IReadOnlyList<Tile> tiles => _tiles;

    public bool Insight { get; set; }

    public ChunkLod Lod { get; set; }

    internal void initNeighbors() {
        _neighbors = point.neighbor.Select(p => planet.pointChunkMap[p]).ToList();
    }

}

public enum ChunkLod {

    JustHex, // 每个地块只有六个平均高度点组成的六边形（非平面）

    PlaneHex, // 高度立面，无特征，无河流的六边形

    SimpleHex, // 最简单的 Solid + 斜面六边形 

    TerracesHex, // 增加台阶

    Full, // 增加边细分

}
