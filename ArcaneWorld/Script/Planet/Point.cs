using ArcaneWorld.Util;
using Godot;

namespace ArcaneWorld.Planet;

public class Point {

    public int id { get; internal set; }

    public required PlanerDomain domain { get; init; }

    public required Vector3 position { get; init; }

    public required SphereAxialCoords coords { get; init; }

    internal List<Face> _faces = null!;

    /// <summary>
    /// 维护与该点相关的所有三角面 ID
    /// </summary>
    public IReadOnlyList<Face> faces => _faces;

    private List<Point> _neighbors = null!;

    // 已确保顺序和 faces 对应，每个邻居共边的顶点是 HexFaceIds[i] 和 HexFaceIds[(i + 1) % HexFaceIds.Count]
    public IReadOnlyList<Point> neighbor => _neighbors;

    private Vector3? _unitCentroid;

    public Vector3 unitCentroid => _unitCentroid ??= faces
        .Select(f => f.center.Normalized())
        .Aggregate((v1, v2) => v1 + v2) / faces.Count;

    internal void orderedFaces() {
        List<Face> faces = _faces;
        if (faces.Count == 0) {
            return;
        }

        // 将第一个面设置为最接近北方顺时针方向第一个的面
        Face first = faces[0];
        float minAngle = Mathf.Tau;

        foreach (Face face in faces) {
            float angle = position.DirectionTo(face.center).AngleTo(Vector3.Up);
            if (angle < minAngle) {
                minAngle = angle;
                first = face;
            }
        }

        // 第二个面必须保证和第一个面形成顺时针方向，从而保证所有都是顺时针
        Face second = faces.First(
            face =>
                face.id != first.id
                && face.isAdjacentTo(first)
                && Math3dUtil.IsRightVSeq(
                    Vector3.Zero,
                    position,
                    first.center,
                    face.center
                )
        );

        List<Face> orderedList = new List<Face> { first, second };
        Face currentFace = orderedList[1];

        while (orderedList.Count < faces.Count) {
            List<int> existingIds = orderedList.Select(face => face.id).ToList();
            Face neighbour = faces.First(
                face =>
                    !existingIds.Contains(face.id) && face.isAdjacentTo(currentFace)
            );
            currentFace = neighbour;
            orderedList.Add(currentFace);
        }

        _faces = orderedList;
    }

    internal void orderedNeighborPoint(Planet planet) {
        _neighbors = faces
            .Select(face => face.getRightOtherPoints(this, planet))
            .ToList();
    }

}
