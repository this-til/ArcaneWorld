using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader Include 包装类: atmosphere_funcs_v1.gdshaderinc
/// 路径: res://addons/zylann.atmosphere/shaders/include/atmosphere_funcs_v1.gdshaderinc
/// </summary>
public class AtmosphereFuncsV1IncShader {

    private readonly ShaderMaterial _material;

    public AtmosphereFuncsV1IncShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: u_day_color0
    /// </summary>
    public Color uDayColor0 {
        get => _material.GetShaderParameter("u_day_color0").As<Color>();
        set => _material.SetShaderParameter("u_day_color0", value);
    }

    /// <summary>
    /// Shader 参数: u_day_color1
    /// </summary>
    public Color uDayColor1 {
        get => _material.GetShaderParameter("u_day_color1").As<Color>();
        set => _material.SetShaderParameter("u_day_color1", value);
    }

    /// <summary>
    /// Shader 参数: u_night_color0
    /// </summary>
    public Color uNightColor0 {
        get => _material.GetShaderParameter("u_night_color0").As<Color>();
        set => _material.SetShaderParameter("u_night_color0", value);
    }

    /// <summary>
    /// Shader 参数: u_night_color1
    /// </summary>
    public Color uNightColor1 {
        get => _material.GetShaderParameter("u_night_color1").As<Color>();
        set => _material.SetShaderParameter("u_night_color1", value);
    }

    /// <summary>
    /// Shader 参数: u_day_night_transition_scale
    /// </summary>
    public float uDayNightTransitionScale {
        get => _material.GetShaderParameter("u_day_night_transition_scale").As<float>();
        set => _material.SetShaderParameter("u_day_night_transition_scale", value);
    }

}
