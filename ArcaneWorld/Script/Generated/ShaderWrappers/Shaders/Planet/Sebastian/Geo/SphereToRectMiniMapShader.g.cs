using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: SphereToRectMiniMap.gdshader
/// 路径: res://Shaders/Planet/Sebastian/Geo/SphereToRectMiniMap.gdshader
/// </summary>
public class SphereToRectMiniMapShader {

    private readonly ShaderMaterial _material;

    public SphereToRectMiniMapShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: lon
    /// </summary>
    public float lon {
        get => _material.GetShaderParameter("lon").As<float>();
        set => _material.SetShaderParameter("lon", value);
    }

    /// <summary>
    /// Shader 参数: lat
    /// </summary>
    public float lat {
        get => _material.GetShaderParameter("lat").As<float>();
        set => _material.SetShaderParameter("lat", value);
    }

    /// <summary>
    /// Shader 参数: pos_normal
    /// </summary>
    public Vector3 posNormal {
        get => _material.GetShaderParameter("pos_normal").As<Vector3>();
        set => _material.SetShaderParameter("pos_normal", value);
    }

    /// <summary>
    /// Shader 参数: angle_to_north
    /// </summary>
    public float angleToNorth {
        get => _material.GetShaderParameter("angle_to_north").As<float>();
        set => _material.SetShaderParameter("angle_to_north", value);
    }

}
