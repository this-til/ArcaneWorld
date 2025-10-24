using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader Include 包装类: HexCellData.gdshaderinc
/// 路径: res://Shaders/Planet/HexCellData.gdshaderinc
/// </summary>
public class HexCellDataIncShader {

    private readonly ShaderMaterial _material;

    public HexCellDataIncShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Global Shader 参数: hex_tile_data
    /// </summary>
    public Color hexTileData {
        get => RenderingServer.GlobalShaderParameterGet("hex_tile_data").As<Color>();
        set => RenderingServer.GlobalShaderParameterSet("hex_tile_data", value);
    }

    /// <summary>
    /// Global Shader 参数: hex_tile_civ_data
    /// </summary>
    public Color hexTileCivData {
        get => RenderingServer.GlobalShaderParameterGet("hex_tile_civ_data").As<Color>();
        set => RenderingServer.GlobalShaderParameterSet("hex_tile_civ_data", value);
    }

    /// <summary>
    /// Global Shader 参数: hex_tile_data_texel_size
    /// </summary>
    public Vector4 hexTileDataTexelSize {
        get => RenderingServer.GlobalShaderParameterGet("hex_tile_data_texel_size").As<Vector4>();
        set => RenderingServer.GlobalShaderParameterSet("hex_tile_data_texel_size", value);
    }

    /// <summary>
    /// Global Shader 参数: hex_map_edit_mode
    /// </summary>
    public bool hexMapEditMode {
        get => RenderingServer.GlobalShaderParameterGet("hex_map_edit_mode").As<bool>();
        set => RenderingServer.GlobalShaderParameterSet("hex_map_edit_mode", value);
    }

}
