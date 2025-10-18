using System.Collections.Concurrent;
using System.IO.Compression;
using System.Threading.Tasks;
using ArcaneWorld.Script.Constants;
using ArcaneWorld.Util;
using CakeToolset.Attribute;
using CommonUtil;
using CommonUtil.Container;
using CommonUtil.Extensions;
using Fractural.Tasks;
using Godot;
using Environment = System.Environment;

namespace ArcaneWorld.Planet;

[Tool]
[Log]
[ClassName]
public partial class Planet : Node3D {

    [Export]
    public int tileDivisions { get; set; } = 50;

    [Export]
    public int chunkDivisions { get; set; } = 5;

    public IReadOnlyList<Point> tilePointList = null!;

    public IReadOnlyList<Face> tileFaceList = null!;

    public IReadOnlyList<Point> chunkPointList = null!;

    public IReadOnlyList<Face> chunkFaceList = null!;

    public IReadOnlyDictionary<Vector3, Point> tilePositionMap = null!;

    public IReadOnlyDictionary<SphereAxialCoords, Point> tileCoordsMap = null!;

    public IReadOnlyDictionary<Vector3, Point> chunkPositionMap = null!;

    public IReadOnlyDictionary<SphereAxialCoords, Point> chunkCoordsMap = null!;

    private VpTree<Vector3> chunkPointVpTree = null!;

    public IReadOnlyList<Chunk> chunkList = null!;

    public IReadOnlyDictionary<Point, Chunk> pointChunkMap = null!;

    public IReadOnlyDictionary<Vector3, Chunk> posChunkMap = null!;

    public IReadOnlyList<Tile> tileList = null!;

    private VpTree<Vector3> tilePointVpTree = null!;

    public IReadOnlyDictionary<Point, Tile> pointTileMap = null!;

    public IReadOnlyDictionary<Vector3, Tile> posTileMap = null!;

    public Point? getPointByCoords(PlanerDomain domain, SphereAxialCoords coords) {
        return domain switch {
            PlanerDomain.Tile => tileCoordsMap[coords],
            PlanerDomain.Chunk => chunkCoordsMap[coords],
            _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
        };
    }

    public Point? getPointByPosition(PlanerDomain domain, Vector3 position) {
        return domain switch {
            PlanerDomain.Tile => tilePositionMap[position],
            PlanerDomain.Chunk => chunkPositionMap[position],
            _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
        };
    }

    /// <summary>
    /// 通过点找寻最近的区块
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Chunk? searchNearestChunk(Vector3 pos) {
        chunkPointVpTree.Search(pos.Normalized(), 1, out Vector3[] results, out _);
        return posChunkMap.GetValueOrDefault(results[0]);
    }

    public Tile? searchNearestTile(Vector3 pos) {
        tilePointVpTree.Search(pos.Normalized(), 1, out Vector3[] results, out _);
        return posTileMap.GetValueOrDefault(results[0]);
    }

    public override void _Ready() {
        base._Ready();
        _ = generatedPlane();
    }

    private async Task generatedPlane() {
        (chunkPointList, chunkFaceList) = await generatedPointsAndFaces(PlanerDomain.Chunk, chunkDivisions);
        (tilePointList, tileFaceList) = await generatedPointsAndFaces(PlanerDomain.Tile, tileDivisions);

        tilePositionMap = tilePointList.ToDictionary(c => c.position, c => c);
        tileCoordsMap = tilePointList.ToDictionary(c => c.coords, c => c);
        chunkPositionMap = chunkPointList.ToDictionary(c => c.position, c => c);
        chunkCoordsMap = chunkPointList.ToDictionary(c => c.coords, c => c);

        await initChunks();
        await initTiles();

    }

    private async Task<(List<Point> planetPointList, List<Face> planetFaceList)> generatedPointsAndFaces(PlanerDomain domain, int divisions) {

        await GDTask.SwitchToThreadPool();

        ulong time = Time.GetTicksMsec();

        ConcurrentBag<Point> pointsCollect = new ConcurrentBag<Point>();
        ConcurrentBag<Face> planetsCollect = new ConcurrentBag<Face>();

        Vector3 pn = IcosahedronConstants.Vertices[0]; // 北极点
        Vector3 ps = IcosahedronConstants.Vertices[6]; // 南极点

        pointsCollect.Add(
            new Point {
                position = pn,
                coords = new SphereAxialCoords(
                    0,
                    -divisions,
                    divisions
                ),
                domain = domain,
            }
        );
        pointsCollect.Add(
            new Point {
                position = ps,
                coords = new SphereAxialCoords(
                    -divisions,
                    2 * divisions,
                    divisions
                ),
                domain = domain,
            }
        );

        Vector3[][] edges = generatedEdgeVectors(divisions, pn, ps);

        await Task.WhenAll(
            Enumerable.Range(0, 5)
                .SelectMany(
                    col => new List<Task>() {
                        generatedNorthTriangle(
                            domain,
                            edges,
                            col,
                            divisions,
                            pointsCollect,
                            planetsCollect
                        ),
                        generatedEquatorTwoTriangles(
                            domain,
                            edges,
                            col,
                            divisions,
                            pointsCollect,
                            planetsCollect
                        ),
                        generatedSouthTriangle(
                            domain,
                            edges,
                            col,
                            divisions,
                            pointsCollect,
                            planetsCollect
                        ),
                    }
                )
                .ToList()
        );

        // 先排序后再分配 id，确保 id 是有序的
        List<Point> planetPoints = pointsCollect
            .OrderBy(p => p.coords.Coords.r) // 先按 r 排序（从北到南）
            .ThenBy(p => p.coords.Coords.q) // 再按 q 排序（从右到左）
            .Peek((p, i) => p.id = i)
            .ToList();

        Dictionary<Vector3, Point> dictionary = planetPoints.ToDictionary(p => p.position, p => p);
        
        List<Face> planetFaces = planetsCollect
            .Peek((p, i) => p.id = i)
            .ToList();

        planetPoints
            .Peek(p => p._faces = new List<Face>())
            .End();

        planetFaces
            .Peek(
                f => f.TriVertices
                    .Select(v => dictionary[v])
                    .NotNull()
                    .Peek(p => p._faces.Add(f))
                    .End()
            )
            .End();

        log.Info($"--- generatedPointsAndFaces for {domain} cost: {Time.GetTicksMsec() - time} ms");

        return (planetPoints, planetFaces);
    }

    private Vector3[][] generatedEdgeVectors(int divisions, Vector3 pn, Vector3 ps) {
        IReadOnlyList<Vector3> points = IcosahedronConstants.Vertices;
        IReadOnlyList<int> indices = IcosahedronConstants.Indices;
        Vector3[][] edges = new Vector3[30][]; // 30 条边
        // 初始化所有的边上的点位置
        for (int col = 0; col < 5; col++) // 5 个竖列四面组
        {
            // p1 到 p4 映射到平面上是竖列四面组中间的四个点，中间 Z 字型边按顺序连接：p2，p1，p3，p4
            Vector3 p1 = points[indices[col * 12 + 1]];
            Vector3 p2 = points[indices[col * 12 + 2]];
            Vector3 p3 = points[indices[col * 12 + 3]];
            Vector3 p4 = points[indices[col * 12 + 7]];
            // 每个竖列四面组有六个属于它的边（右边两个三趾鸡爪形的从上到下的边，列左边界的三条边不属于它）
            edges[col * 6] = Math3dUtil.Subdivide(pn, p1, divisions); // 从左上到右下
            edges[col * 6 + 1] = Math3dUtil.Subdivide(p1, p2, divisions); // 从右往左
            edges[col * 6 + 2] = Math3dUtil.Subdivide(p1, p3, divisions); // 从右上到左下
            edges[col * 6 + 3] = Math3dUtil.Subdivide(p1, p4, divisions); // 从左上到右下
            edges[col * 6 + 4] = Math3dUtil.Subdivide(p4, p3, divisions); // 从右往左
            edges[col * 6 + 5] = Math3dUtil.Subdivide(p4, ps, divisions); // 从右上到左下
        }

        return edges;
    }

    // 构造北部的第一个面
    private Task generatedNorthTriangle
    (
        PlanerDomain domain,
        Vector3[][] edges,
        int col,
        int divisions,
        ConcurrentBag<Point> pointsCollect,
        ConcurrentBag<Face> planetsCollect
    ) => Task.Run(
        () => {
            int nextCol = (col + 1) % 5;
            Vector3[] northEast = edges[col * 6]; // 北极出来的靠东的边界
            Vector3[] northWest = edges[nextCol * 6]; // 北极出来的靠西的边界
            Vector3[] tropicOfCancer = edges[col * 6 + 1]; // 北回归线的边（E -> W）
            Vector3[] preLine = new[] { northEast[0] }; // 初始为北极点
            for (int i = 1; i <= divisions; i++) {
                Vector3[] nowLine = i == divisions
                    ? tropicOfCancer
                    : Math3dUtil.Subdivide(northEast[i], northWest[i], i);
                pointsCollect.Add(
                    new Point {
                        position = nowLine[0],
                        coords = new SphereAxialCoords(
                            -divisions * col,
                            i == divisions
                                ? 0
                                : i - divisions,
                            divisions
                        ),
                        domain = domain,
                    }
                );
                for (int j = 0; j < i; j++) {
                    if (j > 0) {
                        planetsCollect.Add(
                            new Face {
                                domain = domain,
                                TriVertices = [nowLine[j], preLine[j], preLine[j - 1]]
                            }
                        );
                        pointsCollect.Add(
                            new Point {
                                position = nowLine[j],
                                coords = new SphereAxialCoords(
                                    -divisions * col - j,
                                    i == divisions
                                        ? 0
                                        : i - divisions,
                                    divisions
                                ),
                                domain = domain,
                            }
                        );
                    }

                    planetsCollect.Add(
                        new Face {
                            domain = domain,
                            TriVertices = [preLine[j], nowLine[j], nowLine[j + 1]]
                        }
                    );
                }

                preLine = nowLine;
            }
        }
    );

    // 赤道两个面（第二、三面）的构造
    private Task generatedEquatorTwoTriangles
    (
        PlanerDomain domain,
        Vector3[][] edges,
        int col,
        int divisions,
        ConcurrentBag<Point> pointsCollect,
        ConcurrentBag<Face> planetsCollect
    ) => Task.Run(
        () => {
            int nextCol = (col + 1) % 5;
            Vector3[] equatorWest = edges[nextCol * 6 + 3]; // 向东南方斜跨赤道的靠西的边界
            Vector3[] equatorMid = edges[col * 6 + 2]; // 向西南方斜跨赤道的中间的边
            Vector3[] equatorEast = edges[col * 6 + 3]; // 向东南方斜跨赤道的靠东的边界
            Vector3[] tropicOfCapricorn = edges[col * 6 + 4]; // 南回归线的边（E -> W）
            Vector3[] preLineWest = edges[col * 6 + 1]; // 北回归线的边（E -> W）
            Vector3[] preLineEast = new[] { equatorEast[0] };
            for (int i = 1; i <= divisions; i++) {
                Vector3[] nowLineEast = i == divisions
                    ? tropicOfCapricorn
                    : Math3dUtil.Subdivide(equatorEast[i], equatorMid[i], i);
                Vector3[] nowLineWest = Math3dUtil.Subdivide(equatorMid[i], equatorWest[i], divisions - i);
                // 构造东边面（第三面）
                pointsCollect.Add(
                    new Point {
                        position = nowLineEast[0],
                        coords = new SphereAxialCoords(-divisions * col, i, divisions),
                        domain = domain,
                    }
                );
                for (int j = 0; j < i; j++) {
                    if (j > 0) {
                        planetsCollect.Add(
                            new Face {
                                domain = domain,
                                TriVertices = [nowLineEast[j], preLineEast[j], preLineEast[j - 1]]
                            }
                        );
                        /*if (i == divisions) {
                            pointsCollect.Add(
                                new Point {
                                    position = nowLineEast[j],
                                    coords = new SphereAxialCoords(-divisions * col - j, i, divisions),
                                    domain = domain,
                                }
                            );
                        }
                        else {
                            pointsCollect.Add(
                                new Point {
                                    position = nowLineEast[j],
                                    coords = new SphereAxialCoords(-divisions * col - j, i, divisions),
                                    domain = domain,
                                }
                            );
                        }*/
                        
                        pointsCollect.Add(
                            new Point {
                                position = nowLineEast[j],
                                coords = new SphereAxialCoords(-divisions * col - j, i, divisions),
                                domain = domain,
                            }
                        );
                    }

                    planetsCollect.Add(
                        new Face {
                            domain = domain,
                            TriVertices = [preLineEast[j], nowLineEast[j], nowLineEast[j + 1]]
                        }
                    );
                }

                // 构造西边面（第二面）
                if (i < divisions) {
                    pointsCollect.Add(
                        new Point {
                            position = nowLineWest[0],
                            coords = new SphereAxialCoords(-divisions * col - i, i, divisions),
                            domain = domain,
                        }
                    );
                }
                for (int j = 0; j <= divisions - i; j++) {
                    if (j > 0) {
                        planetsCollect.Add(
                            new Face {
                                domain = domain,
                                TriVertices = [preLineWest[j], nowLineWest[j - 1], nowLineWest[j]]
                            }
                        );
                        if (j < divisions - i) {
                            pointsCollect.Add(
                                new Point {
                                    position = nowLineWest[j],
                                    coords = new SphereAxialCoords(-divisions * col - i - j, i, divisions),
                                    domain = domain,
                                }
                            );
                        }
                    }

                    planetsCollect.Add(
                        new Face {
                            domain = domain,
                            TriVertices = [nowLineWest[j], preLineWest[j + 1], preLineWest[j]]
                        }
                    );
                }

                preLineEast = nowLineEast;
                preLineWest = nowLineWest;
            }
        }
    );

    // 构造南部的最后一面（列的第四面）
    private Task generatedSouthTriangle
    (
        PlanerDomain domain,
        Vector3[][] edges,
        int col,
        int divisions,
        ConcurrentBag<Point> pointsCollect,
        ConcurrentBag<Face> planetsCollect
    ) => Task.Run(
        () => {
            int nextCol = (col + 1) % 5;
            Vector3[] southWest = edges[nextCol * 6 + 5]; // 向南方连接南极的靠西的边界
            Vector3[] southEast = edges[col * 6 + 5]; // 向南方连接南极的靠东的边界
            Vector3[] preLine = edges[col * 6 + 4]; // 南回归线的边（E -> W）
            for (int i = 1; i <= divisions; i++) {
                Vector3[] nowLine = Math3dUtil.Subdivide(southEast[i], southWest[i], divisions - i);
                if (i < divisions) {
                    pointsCollect.Add(
                        new Point {
                            position = nowLine[0],
                            coords = new SphereAxialCoords(-divisions * col - i, divisions + i, divisions),
                            domain = domain,
                        }
                    );
                }
                for (int j = 0; j <= divisions - i; j++) {
                    if (j > 0) {
                        planetsCollect.Add(
                            new Face {
                                domain = domain,
                                TriVertices = [preLine[j], nowLine[j - 1], nowLine[j]]
                            }
                        );
                        if (j < divisions - i) {
                            pointsCollect.Add(
                                new Point {
                                    position = nowLine[j],
                                    coords = new SphereAxialCoords(-divisions * col - i - j, divisions + i, divisions),
                                    domain = domain,
                                }
                            );
                        }
                    }

                    planetsCollect.Add(
                        new Face {
                            domain = domain,
                            TriVertices = [nowLine[j], preLine[j + 1], preLine[j]]
                        }
                    );
                }

                preLine = nowLine;
            }
        }
    );

    private async Task initChunks() {
        ulong time = Time.GetTicksMsec();

        await GDTask.SwitchToThreadPool();

        await TaskUtil.ParallelProcessBatch(chunkPointList, p => p.orderedFaces());

        await TaskUtil.ParallelProcessBatch(chunkPointList, p => p.orderedNeighborPoint(this));

        chunkList = chunkPointList
            .Select(
                p => new Chunk() {
                    planet = this,
                    point = p,
                    planetPosition = default, // TODO
                }
            )
            .ToList();

        pointChunkMap = chunkList.ToDictionary(c => c.point, c => c);
        posChunkMap = chunkList.ToDictionary(c => c.point.position, c => c);

        chunkList
            .Peek((c, i) => c.id = i)
            .Peek(c => c.initNeighbors())
            .End();

        chunkPointVpTree = new VpTree<Vector3>();
        chunkPointVpTree.Create(
            chunkPointList
                .Select(p => p.position)
                .ToArray(),
            (p0, p1) => p0.DistanceTo(p1)
        );

        GD.Print($"initChunks chunkDivisions {chunkDivisions}, cost: {Time.GetTicksMsec() - time} ms");
    }

    private async Task initTiles() {
        var time = Time.GetTicksMsec();

        await TaskUtil.ParallelProcessBatch(tilePointList, p => p.orderedFaces());

        await TaskUtil.ParallelProcessBatch(tilePointList, p => p.orderedNeighborPoint(this));

        tileList = tilePointList
            .Select(
                p => new Tile() {
                    planet = this,
                    point = p,
                    chunk = searchNearestChunk(p.position)!
                }
            )
            .ToList();

        pointTileMap = tileList.ToDictionary(c => c.point, c => c);
        posTileMap = tileList.ToDictionary(c => c.point.position, c => c);

        tileList
            .Peek((c, i) => c.id = i)
            .Peek(c => c.initNeighbors())
            .End();

        tilePointVpTree = new VpTree<Vector3>();
        tilePointVpTree.Create(
            tilePointList
                .Select(p => p.position)
                .ToArray(),
            (p0, p1) => p0.DistanceTo(p1)
        );

        Dictionary<Chunk, List<Tile>> dictionary = chunkList.ToDictionary(c => c, c => new List<Tile>());

        tileList
            .Peek(t => dictionary[t.chunk].Add(t))
            .End();

        dictionary
            .Peek(kv => kv.Key._tiles = kv.Value)
            .End();

        GD.Print($"initTiles tileDivisions {tileDivisions}, cost: {Time.GetTicksMsec() - time} ms");
    }

}
