using Godot;

namespace ArcaneWorld.Planet;

/// Copyright (C) 2025 Zhu Xiaohe(aka ZeromaXHe)
/// Author: Zhu XH
/// Date: 2025-03-04 20:25
public readonly record struct SphereAxialCoords {

    public readonly AxialCoords coords;

    public readonly SphereAxialType type;

    public readonly int typeIdx;

    /// <summary>
    /// 正十二面体的细分次数
    /// </summary>
    public readonly int divisions;

    public int width => divisions * 5;

    public SphereAxialCoords(int q, int r, int divisions) {
        this.divisions = divisions;
        coords = new AxialCoords(q, r);
        if (!validateAxial()) {
            type = SphereAxialType.invalid;
            typeIdx = -1;
        }
        else if (r == -divisions || r == 2 * divisions) {
            type = SphereAxialType.poleVertices;
            typeIdx = r == -divisions
                ? 0
                : 1;
        }
        else if (r < 0) {
            if (-q % divisions == 0) {
                type = SphereAxialType.edgesSpecial;
                typeIdx = -q / divisions * 6;
            }
            else {
                type = -q % divisions == divisions + r - 1
                    ? SphereAxialType.facesSpecial
                    : SphereAxialType.faces;
                typeIdx = -q / divisions * 4;
            }
        }
        else if (r == 0) {
            if (-q % divisions == 0) {
                type = SphereAxialType.midVertices;
                typeIdx = -q / divisions * 2;
            }
            else {
                type = -q % divisions == divisions - 1
                    ? SphereAxialType.edgesSpecial
                    : SphereAxialType.edges;
                typeIdx = -q / divisions * 6 + 1;
            }
        }
        else if (r < divisions) {
            if (-q % divisions == 0) {
                type = SphereAxialType.edges;
                typeIdx = -q / divisions * 6 + 3;
            }
            else if (-q % divisions == r) {
                type = SphereAxialType.edges;
                typeIdx = -q / divisions * 6 + 2;
            }
            else {
                type = SphereAxialType.faces;
                typeIdx = -q / divisions * 4 + (-q % divisions > r
                    ? 1
                    : 2);
            }
        }
        else if (r == divisions) {
            if (-q % divisions == 0) {
                type = SphereAxialType.midVertices;
                typeIdx = -q / divisions * 2 + 1;
            }
            else {
                type = -q % divisions == divisions - 1
                    ? SphereAxialType.edgesSpecial
                    : SphereAxialType.edges;
                typeIdx = -q / divisions * 6 + 4;
            }
        }
        else {
            if (-q % divisions == r - divisions) {
                type = SphereAxialType.edgesSpecial;
                typeIdx = -q / divisions * 6 + 5;
            }
            else {
                type = -q % divisions == divisions - 1
                    ? SphereAxialType.facesSpecial
                    : SphereAxialType.faces;
                typeIdx = -q / divisions * 4 + 3;
            }
        }
    }

    public bool SpecialNeighbor => type is SphereAxialType.edgesSpecial or SphereAxialType.facesSpecial;

    // 正二十面体索引，0 ~ 19
    public int index => type switch {
        SphereAxialType.poleVertices => typeIdx == 0
            ? 0
            : 3,
        SphereAxialType.midVertices => typeIdx / 2 * 4 + 1 + typeIdx % 2,
        SphereAxialType.edges or SphereAxialType.edgesSpecial => typeIdx / 6 * 4 + (typeIdx % 6 + 1) / 2,
        SphereAxialType.faces or SphereAxialType.facesSpecial => typeIdx / 4 * 4 + typeIdx % 4,
        _ => -1
    };

    // 获取列索引，从右到左 0 ~ 4
    public int Column => type is SphereAxialType.poleVertices && typeIdx == 1
        ? 0
        : type is not SphereAxialType.invalid
            ? -coords.q / divisions
            : -1;

    // 获取行索引，从上到下 0 ~ 3
    public int Row => type switch {
        SphereAxialType.poleVertices => typeIdx == 0
            ? 0
            : 3,
        SphereAxialType.midVertices => 1 + typeIdx % 2,
        SphereAxialType.edges or SphereAxialType.edgesSpecial => (typeIdx % 6 + 1) / 2,
        SphereAxialType.faces or SphereAxialType.facesSpecial => typeIdx % 4,
        _ => -1
    };

    // 在北边的 5 个面上
    public bool isNorth5 => Row == 0;

    // 在南边的 5 个面上
    public bool isSouth5 => Row == 3;

    // 属于极地十面
    public bool isPole10 => isNorth5 || isSouth5;

    // 属于赤道十面
    public bool isEquator10 => !isPole10;

    public bool isEquatorWest => Row == 1;

    public bool isEquatorEast => Row == 2;

    public bool isValid => type != SphereAxialType.invalid;

    public bool validateAxial() {
        // q 在 (-Width, 0] 之间
        if (coords.q > 0 || coords.q <= -width) {
            return false;
        }
        // r 在 (-ColWidth, 2 * ColWidth) 之间)
        if (coords.r < -divisions || coords.r > 2 * divisions) {
            return false;
        }
        // 北极点
        if (coords.r == -divisions) {
            return coords.q == 0;
        }
        // 南极点
        if (coords.r == 2 * divisions) {
            return coords.q == -divisions;
        }
        if (coords.r < 0) {
            return -coords.q % divisions < divisions + coords.r;
        }
        if (coords.r > divisions) {
            return -coords.q % divisions > divisions - coords.r;
        }
        return true;
    }

    // 距离左边最近的边的 Q 差值
    // （当 Column 4 向左跨越回 Column 0 时，保持返回与普通情况一致性，即：将 Column 0 视作 6 的位置计算）
    private int leftEdgeDiffQ() {
        return Row switch {
            0 => coords.q + coords.r + divisions + Column * divisions,
            1 => coords.q + divisions + Column * divisions,
            2 => coords.q + coords.r + Column * divisions,
            3 => coords.q + divisions + Column * divisions,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // 距离右边最近的边的 Q 差值（向右不存在特殊情况）
    private int rightEdgeDiffQ() {
        return Row switch {
            0 => -Column * divisions - coords.q,
            1 => -Column * divisions - coords.q - coords.r,
            2 => -Column * divisions - coords.q,
            3 => -Column * divisions - coords.q - coords.r + divisions,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // 左右边最近的边的 Q 差值（特殊情况的处理规则同左边情况）
    private int leftRightEdgeDiffQ() {
        return Row switch {
            0 => coords.r + divisions,
            1 => divisions - coords.r,
            2 => coords.r,
            3 => 2 * divisions - coords.r,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// 获取原始正二十面体的三角形的三个顶点
    /// </summary>
    /// <returns>按照非平行于 XZ 平面的边的单独点第一个，然后两个平行边上的点按顺时针顺序排列，返回三个点的数组</returns>
    private IEnumerable<SphereAxialCoords>? triangleVertices() {
        var nextColumn = (Column + 1) % 5;
        return Row switch {
            0 => [
                new SphereAxialCoords(0, -divisions, divisions),
                new SphereAxialCoords(-Column * divisions, 0, divisions),
                new SphereAxialCoords(-nextColumn * divisions, 0, divisions)
            ],
            1 => [
                new SphereAxialCoords(-nextColumn * divisions, divisions, divisions),
                new SphereAxialCoords(-nextColumn * divisions, 0, divisions),
                new SphereAxialCoords(-Column * divisions, 0, divisions)
            ],
            2 => [
                new SphereAxialCoords(-Column * divisions, 0, divisions),
                new SphereAxialCoords(-Column * divisions, divisions, divisions),
                new SphereAxialCoords(-nextColumn * divisions, divisions, divisions)
            ],
            3 => [
                new SphereAxialCoords(-divisions, 2 * divisions, divisions),
                new SphereAxialCoords(-nextColumn * divisions, divisions, divisions),
                new SphereAxialCoords(-Column * divisions, divisions, divisions)
            ],
            _ => null
        };
    }

    // 转经纬度
    public LongitudeLatitudeCoords toLongitudeAndLatitude() {
        switch (type) {
            case SphereAxialType.poleVertices:
                return new LongitudeLatitudeCoords(
                    0f,
                    90f * (typeIdx == 0
                        ? 1
                        : -1)
                );
            case SphereAxialType.midVertices:
                var longitude = typeIdx / 2f * 72f - typeIdx % 2 * 36f;
                var latitude = typeIdx % 2 == 0
                    ? 29.141262794f
                    : -29.141262794f;
                return new LongitudeLatitudeCoords(longitude, latitude);
            case SphereAxialType.edges:
            case SphereAxialType.edgesSpecial:
            case SphereAxialType.faces:
            case SphereAxialType.facesSpecial:
                var tri = triangleVertices()!.ToArray();
                var triCoords = tri.Select(sa => sa.toLongitudeAndLatitude()).ToArray();
                var horizontalCoords1 = triCoords[0]
                    .Slerp(
                        triCoords[1],
                        (float)Mathf.Abs(coords.r - tri[0].coords.r) / divisions
                    );
                var horizontalCoords2 = triCoords[0]
                    .Slerp(
                        triCoords[2],
                        (float)Mathf.Abs(coords.r - tri[0].coords.r) / divisions
                    );
                return horizontalCoords1.Slerp(
                    horizontalCoords2,
                    (float)(Row % 2 == 1
                        ? leftEdgeDiffQ()
                        : rightEdgeDiffQ()) / leftRightEdgeDiffQ()
                );
            default:
                throw new ArgumentException($"暂不支持的类型：{type}");
        }
    }

    // TODO：现在只能先分情况全写一遍了…… 有点蠢，后续优化
    public int distanceTo(SphereAxialCoords sa) {
        if (Column == sa.Column) // 同一列可以直接按平面求距离
        {
            return coords.DistanceTo(sa.coords);
        }
        if (isEquator10 && sa.isEquator10) // 两者都在赤道十面内
        {
            var left = index > sa.index
                ? this
                : sa;
            var right = index < sa.index
                ? this
                : sa;
            return Mathf.Min(
                left.coords.DistanceTo(right.coords),
                right.coords.DistanceTo(left.coords + new AxialCoords(width, 0))
            );
        }

        // 有其中一个是极点的话，则直接求 R 的差值即可
        if (type == SphereAxialType.poleVertices) {
            return typeIdx == 1
                ? 2 * divisions - sa.coords.r
                : sa.coords.r + divisions;
        }
        if (sa.type == SphereAxialType.poleVertices) {
            return sa.typeIdx == 1
                ? 2 * divisions - coords.r
                : coords.r + divisions;
        }
        return distanceOnePole(sa);
    }

    private int distanceOnePole(SphereAxialCoords sa) {
        if (isNorth5) {
            // 北极五面
            switch (Mathf.PosMod(sa.index - index, 20)) {
                case 6:
                case 7:
                    // sa 在逆斜列上的情况，直接按平面求距离
                    return index < sa.index
                        ? sa.coords.DistanceTo(coords)
                        : sa.coords.DistanceTo(coords + new AxialCoords(width, 0));
                case 4:
                case 5:
                case 10:
                case 11:
                    // sa 在左边逆斜列的情况
                    var rotLeft = coords.RotateCounterClockwiseAround(new AxialCoords(-(Column + 1) * divisions, 0));
                    return index < sa.index
                        ? sa.coords.DistanceTo(rotLeft)
                        : sa.coords.DistanceTo(rotLeft + new AxialCoords(width, 0));
                case 8:
                case 9:
                    // sa 在左边隔一列的逆斜列的情况
                    var rotLeft2 = coords
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column + 1) * divisions, 0))
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column + 2) * divisions, 0));
                    return index < sa.index
                        ? sa.coords.DistanceTo(rotLeft2)
                        : sa.coords.DistanceTo(rotLeft2 + new AxialCoords(width, 0));
                case 14:
                case 15:
                    // 14，15 是边界情况，可能看作左边隔一列的逆斜列近，也可能看作右边隔一列的斜列近
                    var rot2Left = coords
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column + 1) * divisions, 0))
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column + 2) * divisions, 0));
                    var rot2Right = coords
                        .RotateClockwiseAround(new AxialCoords(-Column * divisions, 0))
                        .RotateClockwiseAround(new AxialCoords(-(Column - 1) * divisions, 0));
                    return Mathf.Min(
                        index < sa.index
                            ? sa.coords.DistanceTo(rot2Left)
                            : sa.coords.DistanceTo(rot2Left + new AxialCoords(width, 0)),
                        index < sa.index
                            ? rot2Right.DistanceTo(sa.coords + new AxialCoords(width, 0))
                            : rot2Right.DistanceTo(sa.coords)
                    );
                case 12:
                case 13:
                    // sa 在右边隔一列的斜列的情况
                    var rotRight2 = coords
                        .RotateClockwiseAround(new AxialCoords(-Column * divisions, 0))
                        .RotateClockwiseAround(new AxialCoords(-(Column - 1) * divisions, 0));
                    return index < sa.index
                        ? rotRight2.DistanceTo(sa.coords + new AxialCoords(width, 0))
                        : rotRight2.DistanceTo(sa.coords);
                case 16:
                case 17:
                case 18:
                case 19:
                    // sa 在右边斜列上的情况
                    var rotRight = coords
                        .RotateClockwiseAround(new AxialCoords(-Column * divisions, 0));
                    return index < sa.index
                        ? rotRight.DistanceTo(sa.coords + new AxialCoords(width, 0))
                        : rotRight.DistanceTo(sa.coords);
            }
        }
        else if (isSouth5) {
            // 南极五面
            switch (Mathf.PosMod(sa.index - index, 20)) {
                case 1:
                case 2:
                case 3:
                case 4:
                    // sa 在左边斜列上的情况
                    var rotLeft = coords.RotateClockwiseAround(new AxialCoords(-(Column + 1) * divisions, divisions));
                    return index < sa.index
                        ? sa.coords.DistanceTo(rotLeft)
                        : sa.coords.DistanceTo(rotLeft + new AxialCoords(width, 0));
                case 7:
                case 8:
                    // sa 在左边隔一列的斜列上的情况
                    var rotLeft2 = coords
                        .RotateClockwiseAround(new AxialCoords(-(Column + 1) * divisions, divisions))
                        .RotateClockwiseAround(new AxialCoords(-(Column + 2) * divisions, divisions));
                    return index < sa.index
                        ? sa.coords.DistanceTo(rotLeft2)
                        : sa.coords.DistanceTo(rotLeft2 + new AxialCoords(width, 0));
                case 5:
                case 6:
                    // 5，6 是边界情况，可能看作左边隔一列的逆斜列近，也可能看作右边隔一列的斜列近
                    var rot2Left = coords
                        .RotateClockwiseAround(new AxialCoords(-(Column + 1) * divisions, divisions))
                        .RotateClockwiseAround(new AxialCoords(-(Column + 2) * divisions, divisions));
                    var rot2Right = coords
                        .RotateCounterClockwiseAround(new AxialCoords(-Column * divisions, divisions))
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column - 1) * divisions, divisions));
                    var leftDist = index < sa.index
                        ? sa.coords.DistanceTo(rot2Left)
                        : sa.coords.DistanceTo(rot2Left + new AxialCoords(width, 0));
                    var rightDist = index < sa.index
                        ? rot2Right.DistanceTo(sa.coords + new AxialCoords(width, 0))
                        : rot2Right.DistanceTo(sa.coords);
                    return Mathf.Min(leftDist, rightDist);
                case 11:
                case 12:
                    // sa 在右边隔一列的逆斜列上的情况
                    var rotRight2 = coords
                        .RotateCounterClockwiseAround(new AxialCoords(-Column * divisions, divisions))
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column - 1) * divisions, divisions));
                    return index < sa.index
                        ? rotRight2.DistanceTo(sa.coords + new AxialCoords(width, 0))
                        : rotRight2.DistanceTo(sa.coords);
                case 9:
                case 10:
                case 15:
                case 16:
                    // sa 在右边逆斜列上的情况
                    var rotRight = coords
                        .RotateCounterClockwiseAround(new AxialCoords(-Column * divisions, divisions));
                    return index < sa.index
                        ? rotRight.DistanceTo(sa.coords + new AxialCoords(width, 0))
                        : rotRight.DistanceTo(sa.coords);
                case 13:
                case 14:
                    // sa 在逆斜列上的情况，直接按平面求距离
                    return index < sa.index
                        ? coords.DistanceTo(sa.coords + new AxialCoords(width, 0))
                        : coords.DistanceTo(sa.coords);
            }
        }
        else {
            return sa.distanceOnePole(this);
        }

        throw new NotImplementedException(); // 按道理不应该走到这里
    }

}

public enum SphereAxialType {

    // 正二十面体南北极顶点。
    // 索引：0 北极点，1 南极点
    poleVertices,

    // 正二十面体中间的十个顶点。
    // 索引：0、1 第一组竖向四面的中间右侧从北到南两点，2、3 第二组，以此类推，8、9 第五组（最后一组）
    midVertices,

    // 正二十面体 边上的普通点（可以简单用六边形坐标搜索邻居）
    // 索引：0 ~ 5 第一组竖向四面的从北到南六边（左侧三边不算），6 ~ 11 第二组，以此类推，24 ~ 29 第五组（最后一组）
    edges,

    // 正二十面体 边上的特殊点
    // 索引：0 ~ 5 第一组竖向四面的从北到南六边（左侧三边不算），6 ~ 11 第二组，以此类推，24 ~ 29 第五组（最后一组）
    // 索引 % 6 == 0 || 5（第一边和最后一边）时，相邻的面索引是当前面索引 - 4
    // 其它情况说明在南北回归线西边与 MidVertices 相邻
    edgesSpecial,

    // 正二十面体 面上的普通点（可以简单用六边形坐标搜索邻居）
    // 索引：0 ~ 3 第一组竖向四面从北到南，4 ~ 7 第二组，以此类推，16 ~ 19 第五组（最后一组）
    faces,

    // 正二十面体 面上的特殊点（需要用特殊规则搜索邻居
    // 索引：0 ~ 3 第一组竖向四面从北到南，4 ~ 7 第二组，以此类推，16 ~ 19 第五组（最后一组）
    // 相邻的面索引 + 4 即可
    facesSpecial,

    // 无效坐标
    invalid,

}
