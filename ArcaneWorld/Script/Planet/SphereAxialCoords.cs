﻿using Godot;

namespace ArcaneWorld.Planet;

/// Copyright (C) 2025 Zhu Xiaohe(aka ZeromaXHe)
/// Author: Zhu XH
/// Date: 2025-03-04 20:25
public readonly record struct SphereAxialCoords {

    public readonly AxialCoords Coords;

    public readonly SphereAxialType Type;

    public readonly int TypeIdx;

    /// <summary>
    /// 正十二面体的细分次数
    /// </summary>
    public readonly int divisions;

    public int Width => divisions * 5;

    public SphereAxialCoords(int q, int r, int divisions) {
        this.divisions = divisions;
        Coords = new AxialCoords(q, r);
        if (!ValidateAxial()) {
            Type = SphereAxialType.Invalid;
            TypeIdx = -1;
        }
        else if (r == -divisions || r == 2 * divisions) {
            Type = SphereAxialType.PoleVertices;
            TypeIdx = r == -divisions
                ? 0
                : 1;
        }
        else if (r < 0) {
            if (-q % divisions == 0) {
                Type = SphereAxialType.EdgesSpecial;
                TypeIdx = -q / divisions * 6;
            }
            else {
                Type = -q % divisions == divisions + r - 1
                    ? SphereAxialType.FacesSpecial
                    : SphereAxialType.Faces;
                TypeIdx = -q / divisions * 4;
            }
        }
        else if (r == 0) {
            if (-q % divisions == 0) {
                Type = SphereAxialType.MidVertices;
                TypeIdx = -q / divisions * 2;
            }
            else {
                Type = -q % divisions == divisions - 1
                    ? SphereAxialType.EdgesSpecial
                    : SphereAxialType.Edges;
                TypeIdx = -q / divisions * 6 + 1;
            }
        }
        else if (r < divisions) {
            if (-q % divisions == 0) {
                Type = SphereAxialType.Edges;
                TypeIdx = -q / divisions * 6 + 3;
            }
            else if (-q % divisions == r) {
                Type = SphereAxialType.Edges;
                TypeIdx = -q / divisions * 6 + 2;
            }
            else {
                Type = SphereAxialType.Faces;
                TypeIdx = -q / divisions * 4 + (-q % divisions > r
                    ? 1
                    : 2);
            }
        }
        else if (r == divisions) {
            if (-q % divisions == 0) {
                Type = SphereAxialType.MidVertices;
                TypeIdx = -q / divisions * 2 + 1;
            }
            else {
                Type = -q % divisions == divisions - 1
                    ? SphereAxialType.EdgesSpecial
                    : SphereAxialType.Edges;
                TypeIdx = -q / divisions * 6 + 4;
            }
        }
        else {
            if (-q % divisions == r - divisions) {
                Type = SphereAxialType.EdgesSpecial;
                TypeIdx = -q / divisions * 6 + 5;
            }
            else {
                Type = -q % divisions == divisions - 1
                    ? SphereAxialType.FacesSpecial
                    : SphereAxialType.Faces;
                TypeIdx = -q / divisions * 4 + 3;
            }
        }
    }

    public bool SpecialNeighbor => Type is SphereAxialType.EdgesSpecial or SphereAxialType.FacesSpecial;

    // 正二十面体索引，0 ~ 19
    public int Index => Type switch {
        SphereAxialType.PoleVertices => TypeIdx == 0
            ? 0
            : 3,
        SphereAxialType.MidVertices => TypeIdx / 2 * 4 + 1 + TypeIdx % 2,
        SphereAxialType.Edges or SphereAxialType.EdgesSpecial => TypeIdx / 6 * 4 + (TypeIdx % 6 + 1) / 2,
        SphereAxialType.Faces or SphereAxialType.FacesSpecial => TypeIdx / 4 * 4 + TypeIdx % 4,
        _ => -1
    };

    // 获取列索引，从右到左 0 ~ 4
    public int Column => Type is SphereAxialType.PoleVertices && TypeIdx == 1
        ? 0
        : Type is not SphereAxialType.Invalid
            ? -Coords.q / divisions
            : -1;

    // 获取行索引，从上到下 0 ~ 3
    public int Row => Type switch {
        SphereAxialType.PoleVertices => TypeIdx == 0
            ? 0
            : 3,
        SphereAxialType.MidVertices => 1 + TypeIdx % 2,
        SphereAxialType.Edges or SphereAxialType.EdgesSpecial => (TypeIdx % 6 + 1) / 2,
        SphereAxialType.Faces or SphereAxialType.FacesSpecial => TypeIdx % 4,
        _ => -1
    };

    // 在北边的 5 个面上
    public bool IsNorth5 => Row == 0;

    // 在南边的 5 个面上
    public bool IsSouth5 => Row == 3;

    // 属于极地十面
    public bool IsPole10 => IsNorth5 || IsSouth5;

    // 属于赤道十面
    public bool IsEquator10 => !IsPole10;

    public bool IsEquatorWest => Row == 1;

    public bool IsEquatorEast => Row == 2;

    public bool IsValid() => Type != SphereAxialType.Invalid;

    public bool ValidateAxial() {
        // q 在 (-Width, 0] 之间
        if (Coords.q > 0 || Coords.q <= -Width) {
            return false;
        }
        // r 在 (-ColWidth, 2 * ColWidth) 之间)
        if (Coords.r < -divisions || Coords.r > 2 * divisions) {
            return false;
        }
        // 北极点
        if (Coords.r == -divisions) {
            return Coords.q == 0;
        }
        // 南极点
        if (Coords.r == 2 * divisions) {
            return Coords.q == -divisions;
        }
        if (Coords.r < 0) {
            return -Coords.q % divisions < divisions + Coords.r;
        }
        if (Coords.r > divisions) {
            return -Coords.q % divisions > divisions - Coords.r;
        }
        return true;
    }

    // 距离左边最近的边的 Q 差值
    // （当 Column 4 向左跨越回 Column 0 时，保持返回与普通情况一致性，即：将 Column 0 视作 6 的位置计算）
    private int LeftEdgeDiffQ() {
        return Row switch {
            0 => Coords.q + Coords.r + divisions + Column * divisions,
            1 => Coords.q + divisions + Column * divisions,
            2 => Coords.q + Coords.r + Column * divisions,
            3 => Coords.q + divisions + Column * divisions,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // 距离右边最近的边的 Q 差值（向右不存在特殊情况）
    private int RightEdgeDiffQ() {
        return Row switch {
            0 => -Column * divisions - Coords.q,
            1 => -Column * divisions - Coords.q - Coords.r,
            2 => -Column * divisions - Coords.q,
            3 => -Column * divisions - Coords.q - Coords.r + divisions,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // 左右边最近的边的 Q 差值（特殊情况的处理规则同左边情况）
    private int LeftRightEdgeDiffQ() {
        return Row switch {
            0 => Coords.r + divisions,
            1 => divisions - Coords.r,
            2 => Coords.r,
            3 => 2 * divisions - Coords.r,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// 获取原始正二十面体的三角形的三个顶点
    /// </summary>
    /// <returns>按照非平行于 XZ 平面的边的单独点第一个，然后两个平行边上的点按顺时针顺序排列，返回三个点的数组</returns>
    private IEnumerable<SphereAxialCoords>? TriangleVertices() {
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
    public LongitudeLatitudeCoords ToLongitudeAndLatitude() {
        switch (Type) {
            case SphereAxialType.PoleVertices:
                return new LongitudeLatitudeCoords(
                    0f,
                    90f * (TypeIdx == 0
                        ? 1
                        : -1)
                );
            case SphereAxialType.MidVertices:
                var longitude = TypeIdx / 2 * 72f - TypeIdx % 2 * 36f;
                var latitude = TypeIdx % 2 == 0
                    ? 29.141262794f
                    : -29.141262794f;
                return new LongitudeLatitudeCoords(longitude, latitude);
            case SphereAxialType.Edges:
            case SphereAxialType.EdgesSpecial:
            case SphereAxialType.Faces:
            case SphereAxialType.FacesSpecial:
                var tri = TriangleVertices()!.ToArray();
                var triCoords = tri.Select(sa => sa.ToLongitudeAndLatitude()).ToArray();
                var horizontalCoords1 = triCoords[0]
                    .Slerp(
                        triCoords[1],
                        (float)Mathf.Abs(Coords.r - tri[0].Coords.r) / divisions
                    );
                var horizontalCoords2 = triCoords[0]
                    .Slerp(
                        triCoords[2],
                        (float)Mathf.Abs(Coords.r - tri[0].Coords.r) / divisions
                    );
                return horizontalCoords1.Slerp(
                    horizontalCoords2,
                    (float)(Row % 2 == 1
                        ? LeftEdgeDiffQ()
                        : RightEdgeDiffQ()) / LeftRightEdgeDiffQ()
                );
            default:
                throw new ArgumentException($"暂不支持的类型：{Type}");
        }
    }

    // TODO：现在只能先分情况全写一遍了…… 有点蠢，后续优化
    public int DistanceTo(SphereAxialCoords sa) {
        if (Column == sa.Column) // 同一列可以直接按平面求距离
        {
            return Coords.DistanceTo(sa.Coords);
        }
        if (IsEquator10 && sa.IsEquator10) // 两者都在赤道十面内
        {
            var left = Index > sa.Index
                ? this
                : sa;
            var right = Index < sa.Index
                ? this
                : sa;
            return Mathf.Min(
                left.Coords.DistanceTo(right.Coords),
                right.Coords.DistanceTo(left.Coords + new AxialCoords(Width, 0))
            );
        }

        // 有其中一个是极点的话，则直接求 R 的差值即可
        if (Type == SphereAxialType.PoleVertices) {
            return TypeIdx == 1
                ? 2 * divisions - sa.Coords.r
                : sa.Coords.r + divisions;
        }
        if (sa.Type == SphereAxialType.PoleVertices) {
            return sa.TypeIdx == 1
                ? 2 * divisions - Coords.r
                : Coords.r + divisions;
        }
        return DistanceOnePole(sa);
    }

    private int DistanceOnePole(SphereAxialCoords sa) {
        if (IsNorth5) {
            // 北极五面
            switch (Mathf.PosMod(sa.Index - Index, 20)) {
                case 6:
                case 7:
                    // sa 在逆斜列上的情况，直接按平面求距离
                    return Index < sa.Index
                        ? sa.Coords.DistanceTo(Coords)
                        : sa.Coords.DistanceTo(Coords + new AxialCoords(Width, 0));
                case 4:
                case 5:
                case 10:
                case 11:
                    // sa 在左边逆斜列的情况
                    var rotLeft = Coords.RotateCounterClockwiseAround(new AxialCoords(-(Column + 1) * divisions, 0));
                    return Index < sa.Index
                        ? sa.Coords.DistanceTo(rotLeft)
                        : sa.Coords.DistanceTo(rotLeft + new AxialCoords(Width, 0));
                case 8:
                case 9:
                    // sa 在左边隔一列的逆斜列的情况
                    var rotLeft2 = Coords
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column + 1) * divisions, 0))
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column + 2) * divisions, 0));
                    return Index < sa.Index
                        ? sa.Coords.DistanceTo(rotLeft2)
                        : sa.Coords.DistanceTo(rotLeft2 + new AxialCoords(Width, 0));
                case 14:
                case 15:
                    // 14，15 是边界情况，可能看作左边隔一列的逆斜列近，也可能看作右边隔一列的斜列近
                    var rot2Left = Coords
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column + 1) * divisions, 0))
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column + 2) * divisions, 0));
                    var rot2Right = Coords
                        .RotateClockwiseAround(new AxialCoords(-Column * divisions, 0))
                        .RotateClockwiseAround(new AxialCoords(-(Column - 1) * divisions, 0));
                    return Mathf.Min(
                        Index < sa.Index
                            ? sa.Coords.DistanceTo(rot2Left)
                            : sa.Coords.DistanceTo(rot2Left + new AxialCoords(Width, 0)),
                        Index < sa.Index
                            ? rot2Right.DistanceTo(sa.Coords + new AxialCoords(Width, 0))
                            : rot2Right.DistanceTo(sa.Coords)
                    );
                case 12:
                case 13:
                    // sa 在右边隔一列的斜列的情况
                    var rotRight2 = Coords
                        .RotateClockwiseAround(new AxialCoords(-Column * divisions, 0))
                        .RotateClockwiseAround(new AxialCoords(-(Column - 1) * divisions, 0));
                    return Index < sa.Index
                        ? rotRight2.DistanceTo(sa.Coords + new AxialCoords(Width, 0))
                        : rotRight2.DistanceTo(sa.Coords);
                case 16:
                case 17:
                case 18:
                case 19:
                    // sa 在右边斜列上的情况
                    var rotRight = Coords
                        .RotateClockwiseAround(new AxialCoords(-Column * divisions, 0));
                    return Index < sa.Index
                        ? rotRight.DistanceTo(sa.Coords + new AxialCoords(Width, 0))
                        : rotRight.DistanceTo(sa.Coords);
            }
        }
        else if (IsSouth5) {
            // 南极五面
            switch (Mathf.PosMod(sa.Index - Index, 20)) {
                case 1:
                case 2:
                case 3:
                case 4:
                    // sa 在左边斜列上的情况
                    var rotLeft = Coords.RotateClockwiseAround(new AxialCoords(-(Column + 1) * divisions, divisions));
                    return Index < sa.Index
                        ? sa.Coords.DistanceTo(rotLeft)
                        : sa.Coords.DistanceTo(rotLeft + new AxialCoords(Width, 0));
                case 7:
                case 8:
                    // sa 在左边隔一列的斜列上的情况
                    var rotLeft2 = Coords
                        .RotateClockwiseAround(new AxialCoords(-(Column + 1) * divisions, divisions))
                        .RotateClockwiseAround(new AxialCoords(-(Column + 2) * divisions, divisions));
                    return Index < sa.Index
                        ? sa.Coords.DistanceTo(rotLeft2)
                        : sa.Coords.DistanceTo(rotLeft2 + new AxialCoords(Width, 0));
                case 5:
                case 6:
                    // 5，6 是边界情况，可能看作左边隔一列的逆斜列近，也可能看作右边隔一列的斜列近
                    var rot2Left = Coords
                        .RotateClockwiseAround(new AxialCoords(-(Column + 1) * divisions, divisions))
                        .RotateClockwiseAround(new AxialCoords(-(Column + 2) * divisions, divisions));
                    var rot2Right = Coords
                        .RotateCounterClockwiseAround(new AxialCoords(-Column * divisions, divisions))
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column - 1) * divisions, divisions));
                    var leftDist = Index < sa.Index
                        ? sa.Coords.DistanceTo(rot2Left)
                        : sa.Coords.DistanceTo(rot2Left + new AxialCoords(Width, 0));
                    var rightDist = Index < sa.Index
                        ? rot2Right.DistanceTo(sa.Coords + new AxialCoords(Width, 0))
                        : rot2Right.DistanceTo(sa.Coords);
                    return Mathf.Min(leftDist, rightDist);
                case 11:
                case 12:
                    // sa 在右边隔一列的逆斜列上的情况
                    var rotRight2 = Coords
                        .RotateCounterClockwiseAround(new AxialCoords(-Column * divisions, divisions))
                        .RotateCounterClockwiseAround(new AxialCoords(-(Column - 1) * divisions, divisions));
                    return Index < sa.Index
                        ? rotRight2.DistanceTo(sa.Coords + new AxialCoords(Width, 0))
                        : rotRight2.DistanceTo(sa.Coords);
                case 9:
                case 10:
                case 15:
                case 16:
                    // sa 在右边逆斜列上的情况
                    var rotRight = Coords
                        .RotateCounterClockwiseAround(new AxialCoords(-Column * divisions, divisions));
                    return Index < sa.Index
                        ? rotRight.DistanceTo(sa.Coords + new AxialCoords(Width, 0))
                        : rotRight.DistanceTo(sa.Coords);
                case 13:
                case 14:
                    // sa 在逆斜列上的情况，直接按平面求距离
                    return Index < sa.Index
                        ? Coords.DistanceTo(sa.Coords + new AxialCoords(Width, 0))
                        : Coords.DistanceTo(sa.Coords);
            }
        }
        else {
            return sa.DistanceOnePole(this);
        }

        throw new NotImplementedException(); // 按道理不应该走到这里
    }

}

public enum SphereAxialType {

    // 正二十面体南北极顶点。
    // 索引：0 北极点，1 南极点
    PoleVertices,

    // 正二十面体中间的十个顶点。
    // 索引：0、1 第一组竖向四面的中间右侧从北到南两点，2、3 第二组，以此类推，8、9 第五组（最后一组）
    MidVertices,

    // 正二十面体 边上的普通点（可以简单用六边形坐标搜索邻居）
    // 索引：0 ~ 5 第一组竖向四面的从北到南六边（左侧三边不算），6 ~ 11 第二组，以此类推，24 ~ 29 第五组（最后一组）
    Edges,

    // 正二十面体 边上的特殊点
    // 索引：0 ~ 5 第一组竖向四面的从北到南六边（左侧三边不算），6 ~ 11 第二组，以此类推，24 ~ 29 第五组（最后一组）
    // 索引 % 6 == 0 || 5（第一边和最后一边）时，相邻的面索引是当前面索引 - 4
    // 其它情况说明在南北回归线西边与 MidVertices 相邻
    EdgesSpecial,

    // 正二十面体 面上的普通点（可以简单用六边形坐标搜索邻居）
    // 索引：0 ~ 3 第一组竖向四面从北到南，4 ~ 7 第二组，以此类推，16 ~ 19 第五组（最后一组）
    Faces,

    // 正二十面体 面上的特殊点（需要用特殊规则搜索邻居
    // 索引：0 ~ 3 第一组竖向四面从北到南，4 ~ 7 第二组，以此类推，16 ~ 19 第五组（最后一组）
    // 相邻的面索引 + 4 即可
    FacesSpecial,

    // 无效坐标
    Invalid,

}
