using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader Include 包装类: atmosphere_common.gdshaderinc
/// 路径: res://addons/zylann.atmosphere/shaders/include/atmosphere_common.gdshaderinc
/// </summary>
public class AtmosphereCommonIncShader {

    private readonly ShaderMaterial _material;

    public AtmosphereCommonIncShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: u_density
    /// </summary>
    public float uDensity {
        get => _material.GetShaderParameter("u_density").As<float>();
        set => _material.SetShaderParameter("u_density", value);
    }

}
