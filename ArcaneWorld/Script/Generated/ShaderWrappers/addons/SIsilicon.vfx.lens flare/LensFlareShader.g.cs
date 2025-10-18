using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: lens_flare.gdshader
/// 路径: res://addons/SIsilicon.vfx.lens flare/lens_flare.gdshader
/// </summary>
public class LensFlareShader {

    private readonly ShaderMaterial _material;

    public LensFlareShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: lod
    /// </summary>
    public float lod {
        get => _material.GetShaderParameter("lod").As<float>();
        set => _material.SetShaderParameter("lod", value);
    }

    /// <summary>
    /// Shader 参数: stretch_to_aspect
    /// </summary>
    public float stretchToAspect {
        get => _material.GetShaderParameter("stretch_to_aspect").As<float>();
        set => _material.SetShaderParameter("stretch_to_aspect", value);
    }

    /// <summary>
    /// Shader 参数: ghosts
    /// </summary>
    public int ghosts {
        get => _material.GetShaderParameter("ghosts").As<int>();
        set => _material.SetShaderParameter("ghosts", value);
    }

    /// <summary>
    /// Shader 参数: ghost_dispersal
    /// </summary>
    public float ghostDispersal {
        get => _material.GetShaderParameter("ghost_dispersal").As<float>();
        set => _material.SetShaderParameter("ghost_dispersal", value);
    }

    /// <summary>
    /// Shader 参数: halo_width
    /// </summary>
    public float haloWidth {
        get => _material.GetShaderParameter("halo_width").As<float>();
        set => _material.SetShaderParameter("halo_width", value);
    }

    /// <summary>
    /// Shader 参数: distort
    /// </summary>
    public float distort {
        get => _material.GetShaderParameter("distort").As<float>();
        set => _material.SetShaderParameter("distort", value);
    }

    /// <summary>
    /// Shader 参数: bloom_scale
    /// </summary>
    public float bloomScale {
        get => _material.GetShaderParameter("bloom_scale").As<float>();
        set => _material.SetShaderParameter("bloom_scale", value);
    }

    /// <summary>
    /// Shader 参数: bloom_bias
    /// </summary>
    public float bloomBias {
        get => _material.GetShaderParameter("bloom_bias").As<float>();
        set => _material.SetShaderParameter("bloom_bias", value);
    }

    /// <summary>
    /// Shader 参数: streak_strength
    /// </summary>
    public float streakStrength {
        get => _material.GetShaderParameter("streak_strength").As<float>();
        set => _material.SetShaderParameter("streak_strength", value);
    }

    /// <summary>
    /// Shader 参数: distortion_quality
    /// </summary>
    public int distortionQuality {
        get => _material.GetShaderParameter("distortion_quality").As<int>();
        set => _material.SetShaderParameter("distortion_quality", value);
    }

    /// <summary>
    /// Shader 参数: lens_color
    /// </summary>
    public Texture2D lensColor {
        get => _material.GetShaderParameter("lens_color").As<Texture2D>();
        set => _material.SetShaderParameter("lens_color", value);
    }

    /// <summary>
    /// Shader 参数: lens_dirt
    /// </summary>
    public Texture2D lensDirt {
        get => _material.GetShaderParameter("lens_dirt").As<Texture2D>();
        set => _material.SetShaderParameter("lens_dirt", value);
    }

    /// <summary>
    /// Shader 参数: starburst
    /// </summary>
    public Texture2D starburst {
        get => _material.GetShaderParameter("starburst").As<Texture2D>();
        set => _material.SetShaderParameter("starburst", value);
    }

    /// <summary>
    /// Shader 参数: SCREEN_TEXTURE
    /// </summary>
    public Texture2D sCREENTEXTURE {
        get => _material.GetShaderParameter("SCREEN_TEXTURE").As<Texture2D>();
        set => _material.SetShaderParameter("SCREEN_TEXTURE", value);
    }

}
