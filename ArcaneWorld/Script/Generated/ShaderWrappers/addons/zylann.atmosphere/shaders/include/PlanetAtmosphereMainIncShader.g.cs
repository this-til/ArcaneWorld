using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader Include 包装类: planet_atmosphere_main.gdshaderinc
/// 路径: res://addons/zylann.atmosphere/shaders/include/planet_atmosphere_main.gdshaderinc
/// </summary>
public class PlanetAtmosphereMainIncShader {

    private readonly ShaderMaterial _material;

    public PlanetAtmosphereMainIncShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: u_clip_mode
    /// </summary>
    public bool uClipMode {
        get => _material.GetShaderParameter("u_clip_mode").As<bool>();
        set => _material.SetShaderParameter("u_clip_mode", value);
    }

    /// <summary>
    /// Shader 参数: u_sphere_depth_factor
    /// </summary>
    public float uSphereDepthFactor {
        get => _material.GetShaderParameter("u_sphere_depth_factor").As<float>();
        set => _material.SetShaderParameter("u_sphere_depth_factor", value);
    }

    /// <summary>
    /// Shader 参数: u_depth_texture
    /// </summary>
    public Texture2D uDepthTexture {
        get => _material.GetShaderParameter("u_depth_texture").As<Texture2D>();
        set => _material.SetShaderParameter("u_depth_texture", value);
    }

    /// <summary>
    /// Shader 参数: u_blue_noise_texture
    /// </summary>
    public Texture2D uBlueNoiseTexture {
        get => _material.GetShaderParameter("u_blue_noise_texture").As<Texture2D>();
        set => _material.SetShaderParameter("u_blue_noise_texture", value);
    }

}
