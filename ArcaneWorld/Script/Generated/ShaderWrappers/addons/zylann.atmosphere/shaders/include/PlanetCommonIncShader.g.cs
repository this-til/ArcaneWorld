using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader Include 包装类: planet_common.gdshaderinc
/// 路径: res://addons/zylann.atmosphere/shaders/include/planet_common.gdshaderinc
/// </summary>
public class PlanetCommonIncShader {

    private readonly ShaderMaterial _material;

    public PlanetCommonIncShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: u_planet_radius
    /// </summary>
    public float uPlanetRadius {
        get => _material.GetShaderParameter("u_planet_radius").As<float>();
        set => _material.SetShaderParameter("u_planet_radius", value);
    }

    /// <summary>
    /// Shader 参数: u_atmosphere_height
    /// </summary>
    public float uAtmosphereHeight {
        get => _material.GetShaderParameter("u_atmosphere_height").As<float>();
        set => _material.SetShaderParameter("u_atmosphere_height", value);
    }

    /// <summary>
    /// Shader 参数: u_sun_position
    /// </summary>
    public Vector3 uSunPosition {
        get => _material.GetShaderParameter("u_sun_position").As<Vector3>();
        set => _material.SetShaderParameter("u_sun_position", value);
    }

}
