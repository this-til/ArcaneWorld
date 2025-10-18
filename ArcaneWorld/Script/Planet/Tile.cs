namespace ArcaneWorld.Planet;

public class Tile {

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

}
