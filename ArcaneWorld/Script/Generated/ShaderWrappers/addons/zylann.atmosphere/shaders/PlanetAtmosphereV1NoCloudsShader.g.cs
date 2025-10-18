using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: planet_atmosphere_v1_no_clouds.gdshader
/// 路径: res://addons/zylann.atmosphere/shaders/planet_atmosphere_v1_no_clouds.gdshader
/// </summary>
public class PlanetAtmosphereV1NoCloudsShader {

    private readonly ShaderMaterial _material;

    public PlanetAtmosphereV1NoCloudsShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: u_depth_texture
    /// </summary>
    public Texture2D uDepthTexture {
        get => _material.GetShaderParameter("u_depth_texture").As<Texture2D>();
        set => _material.SetShaderParameter("u_depth_texture", value);
    }

}
