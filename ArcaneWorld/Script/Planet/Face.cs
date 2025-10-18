using Godot;

namespace ArcaneWorld.Planet;

public class Face {

    public int id { get; internal set; }

    public required PlanerDomain domain { get; init; }

    // 三角形重心 median point
    public Vector3 Center { get; private set; }

    // 第一个顶点是非水平边的顶点，后续水平边的两点按照顺时针方向排列
    public required Vector3[] TriVertices {
        get => field;
        init {
            field = value;
            Center = (value[0] + value[1] + value[2]) / 3f;
        }
    }

    public bool IsAdjacentTo(Face face) => TriVertices.Intersect(face.TriVertices).Count() == 2;

    public int getPointIdx(Point point) {
        if (TriVertices.All(facePointId => facePointId != point.position)) {
            throw new ArgumentException("Given point must be one of the points on the face!");
        }

        for (var i = 0; i < 3; i++) {
            if (TriVertices[i] == point.position) {
                return i;
            }
        }

        return -1;
    }

    // 顺时针第一个顶点
    public Point getLeftOtherPoints(Point point, Planet planet) {
        var idx = getPointIdx(point);
        return planet.getPointByPosition(domain, TriVertices[(idx + 1) % 3])!;
    }

    // 顺时针第二个顶点
    public Point getRightOtherPoints(Point point, Planet planet) {
        var idx = getPointIdx(point);
        return planet.getPointByPosition(domain, TriVertices[(idx + 2) % 3])!;
    }

}
