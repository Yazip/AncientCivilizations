using Unity.VisualScripting;
using UnityEngine;

public struct EdgeVertices
{

    Vector3 v1, v2, v3, v4, v5;
    
    public Vector3 V1
    {
        get
        {
            return v1;
        }
    }

    public Vector3 V2
    {
        get
        {
            return v2;
        }
    }

    public Vector3 V3
    {
        get
        {
            return v3;
        }
    }

    public Vector3 V4
    {
        get
        {
            return v4;
        }
    }

    public Vector3 V5
    {
        get
        {
            return v5;
        }
    }

    public EdgeVertices(Vector3 corner1, Vector3 corner2)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, 0.25f);
        v3 = Vector3.Lerp(corner1, corner2, 0.5f);
        v4 = Vector3.Lerp(corner1, corner2, 0.75f);
        v5 = corner2;
    }

    // Метод для интерполяции уступов между всеми четырьмя парами вершин двух рёбер
    public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
    {
        EdgeVertices result;
        result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step);
        result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step);
        result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step);
        result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);
        result.v5 = HexMetrics.TerraceLerp(a.v5, b.v5, step);
        return result;
    }
}
