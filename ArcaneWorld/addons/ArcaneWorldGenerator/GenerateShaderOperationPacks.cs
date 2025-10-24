using CakeToolset.Attribute;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArcaneWorld.addons.ArcaneWorldGenerator;

[Log]
public partial class GenerateShaderOperationPacks {

    public static void generateShaderOperationPacks() {
        string projectPath = ProjectSettings.GlobalizePath("res://");
        log.Info($"项目根目录: {projectPath}");

        // 扫描所有 .gdshader 和 .gdshaderinc 文件
        var shaderFiles = ScanShaderFiles(projectPath);
        log.Info($"共扫描到 {shaderFiles.Count} 个 shader 文件");

        // 清理所有生成文件
        CleanAllGeneratedFiles(projectPath);

        // 生成包装类代码
        foreach (var shaderFile in shaderFiles) {
            try {
                string generatedCode = GenerateShaderWrapperCode(shaderFile);
                
                // 保存到文件
                string outputPath = GetOutputPath(projectPath, shaderFile);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                File.WriteAllText(outputPath, generatedCode);
                
                log.Info($"已生成 Shader 包装类: {outputPath}");
            }
            catch (Exception ex) {
                log.Error($"生成 Shader 包装类失败 {shaderFile.FileName}: {ex.Message}");
            }
        }
    }

    private static List<ShaderFileInfo> ScanShaderFiles(string projectPath) {
        var shaderFiles = new List<ShaderFileInfo>();
        ScanDirectory(projectPath, "", shaderFiles);
        return shaderFiles;
    }

    private static void ScanDirectory(string fullPath, string relativePath, List<ShaderFileInfo> shaderFiles) {
        try {
            var directory = new DirectoryInfo(fullPath);
            if (!directory.Exists) {
                return;
            }

            // 跳过不需要的目录
            if (directory.Name.StartsWith(".") ||
                directory.Name == "bin" ||
                directory.Name == "obj" ||
                directory.Name == "node_modules") {
                return;
            }

            // 扫描 .gdshader 文件
            foreach (var file in directory.GetFiles("*.gdshader")) {
                string resourcePath = string.IsNullOrEmpty(relativePath)
                    ? file.Name
                    : $"{relativePath}/{file.Name}";
                    
                shaderFiles.Add(new ShaderFileInfo {
                    FileName = file.Name,
                    ResourcePath = resourcePath,
                    FullPath = file.FullName,
                    DirectoryPath = relativePath,
                    FileType = ShaderFileType.Shader
                });
            }

            // 扫描 .gdshaderinc 文件
            foreach (var file in directory.GetFiles("*.gdshaderinc")) {
                string resourcePath = string.IsNullOrEmpty(relativePath)
                    ? file.Name
                    : $"{relativePath}/{file.Name}";
                    
                shaderFiles.Add(new ShaderFileInfo {
                    FileName = file.Name,
                    ResourcePath = resourcePath,
                    FullPath = file.FullName,
                    DirectoryPath = relativePath,
                    FileType = ShaderFileType.Include
                });
            }

            foreach (var subDir in directory.GetDirectories()) {
                string newRelativePath = string.IsNullOrEmpty(relativePath)
                    ? subDir.Name
                    : $"{relativePath}/{subDir.Name}";
                ScanDirectory(subDir.FullName, newRelativePath, shaderFiles);
            }
        }
        catch (Exception ex) {
            log.Error($"扫描目录时出错 {fullPath}: {ex.Message}");
        }
    }

    private static string GenerateShaderWrapperCode(ShaderFileInfo shaderFile) {
        // 读取 shader 文件内容
        string shaderContent = File.ReadAllText(shaderFile.FullPath);
        
        // 解析 uniform 变量
        var uniforms = ParseUniforms(shaderContent);
        
        // 生成类名
        string className = GetClassName(shaderFile.FileName);
        
        if (shaderFile.FileType == ShaderFileType.Include) {
            return GenerateIncludeWrapperCode(shaderFile, uniforms, className);
        } else {
            return GenerateShaderWrapperCodeInternal(shaderFile, uniforms, className);
        }
    }

    private static string GenerateShaderWrapperCodeInternal(ShaderFileInfo shaderFile, List<UniformInfo> uniforms, string className) {
        var code = new StringBuilder();
        code.AppendLine("using Godot;");
        code.AppendLine("using System;");
        code.AppendLine();
        code.AppendLine("namespace ArcaneWorld.Generated.ShaderWrappers;");
        code.AppendLine();
        code.AppendLine("/// <summary>");
        code.AppendLine($"/// Shader 包装类: {shaderFile.FileName}");
        code.AppendLine($"/// 路径: res://{shaderFile.ResourcePath}");
        code.AppendLine("/// </summary>");
        code.AppendLine($"public class {className} {{");
        code.AppendLine();
        
        // 生成字段和构造函数
        code.AppendLine("    private readonly ShaderMaterial _material;");
        code.AppendLine();
        code.AppendLine($"    public {className}(ShaderMaterial material) {{");
        code.AppendLine("        _material = material;");
        code.AppendLine("    }");
        code.AppendLine();
        
        // 生成属性，直接操作 ShaderMaterial
        foreach (var uniform in uniforms) {
            string propertyName = ToCamelCase(uniform.Name);
            string comment = uniform.IsGlobal ? $"Global Shader 参数: {uniform.Name}" : $"Shader 参数: {uniform.Name}";
            code.AppendLine($"    /// <summary>");
            code.AppendLine($"    /// {comment}");
            code.AppendLine($"    /// </summary>");
            code.AppendLine($"    public {uniform.CSharpType} {propertyName} {{");
            
            if (uniform.IsGlobal) {
                code.AppendLine($"        get => RenderingServer.GlobalShaderParameterGet(\"{uniform.Name}\").As<{uniform.CSharpType}>();");
                code.AppendLine($"        set => RenderingServer.GlobalShaderParameterSet(\"{uniform.Name}\", value);");
            } else {
                code.AppendLine($"        get => _material.GetShaderParameter(\"{uniform.Name}\").As<{uniform.CSharpType}>();");
                code.AppendLine($"        set => _material.SetShaderParameter(\"{uniform.Name}\", value);");
            }
            code.AppendLine($"    }}");
            code.AppendLine();
        }
        
        code.AppendLine("}");
        
        return code.ToString();
    }

    private static string GenerateIncludeWrapperCode(ShaderFileInfo shaderFile, List<UniformInfo> uniforms, string className) {
        var code = new StringBuilder();
        code.AppendLine("using Godot;");
        code.AppendLine("using System;");
        code.AppendLine();
        code.AppendLine("namespace ArcaneWorld.Generated.ShaderWrappers;");
        code.AppendLine();
        code.AppendLine("/// <summary>");
        code.AppendLine($"/// Shader Include 包装类: {shaderFile.FileName}");
        code.AppendLine($"/// 路径: res://{shaderFile.ResourcePath}");
        code.AppendLine("/// </summary>");
        code.AppendLine($"public class {className} {{");
        code.AppendLine();
        
        // 生成字段和构造函数
        code.AppendLine("    private readonly ShaderMaterial _material;");
        code.AppendLine();
        code.AppendLine($"    public {className}(ShaderMaterial material) {{");
        code.AppendLine("        _material = material;");
        code.AppendLine("    }");
        code.AppendLine();
        
        // 生成属性，直接操作 ShaderMaterial
        foreach (var uniform in uniforms) {
            string propertyName = ToCamelCase(uniform.Name);
            string comment = uniform.IsGlobal ? $"Global Shader 参数: {uniform.Name}" : $"Shader 参数: {uniform.Name}";
            code.AppendLine($"    /// <summary>");
            code.AppendLine($"    /// {comment}");
            code.AppendLine($"    /// </summary>");
            code.AppendLine($"    public {uniform.CSharpType} {propertyName} {{");
            
            if (uniform.IsGlobal) {
                code.AppendLine($"        get => RenderingServer.GlobalShaderParameterGet(\"{uniform.Name}\").As<{uniform.CSharpType}>();");
                code.AppendLine($"        set => RenderingServer.GlobalShaderParameterSet(\"{uniform.Name}\", value);");
            } else {
                code.AppendLine($"        get => _material.GetShaderParameter(\"{uniform.Name}\").As<{uniform.CSharpType}>();");
                code.AppendLine($"        set => _material.SetShaderParameter(\"{uniform.Name}\", value);");
            }
            code.AppendLine($"    }}");
            code.AppendLine();
        }
        
        code.AppendLine("}");
        
        return code.ToString();
    }

    private static List<UniformInfo> ParseUniforms(string shaderContent) {
        var uniforms = new List<UniformInfo>();
        
        // 匹配 uniform 声明的正则表达式
        // [global] uniform <type> <name> : <hints> = <default>;
        var regex = new Regex(@"(global\s+)?uniform\s+(\w+(?:<[^>]+>)?)\s+(\w+)\s*(?::\s*([^=;]+))?\s*(?:=\s*([^;]+))?\s*;");
        
        var matches = regex.Matches(shaderContent);
        foreach (Match match in matches) {
            bool isGlobal = match.Groups[1].Success && match.Groups[1].Value.Trim() == "global";
            string glslType = match.Groups[2].Value.Trim();
            string name = match.Groups[3].Value.Trim();
            string hints = match.Groups[4].Success ? match.Groups[4].Value.Trim() : "";
            string defaultValue = match.Groups[5].Success ? match.Groups[5].Value.Trim() : "";
            
            var uniformInfo = new UniformInfo {
                Name = name,
                GlslType = glslType,
                CSharpType = MapGlslTypeToCSharp(glslType, hints),
                DefaultValue = MapDefaultValue(glslType, defaultValue, hints),
                Hints = hints,
                IsGlobal = isGlobal
            };
            
            uniforms.Add(uniformInfo);
        }
        
        return uniforms;
    }

    private static string MapGlslTypeToCSharp(string glslType, string hints) {
        // 根据 hints 确定特殊类型
        if (hints.Contains("source_color")) {
            return "Color";
        }
        
        return glslType switch {
            "float" => "float",
            "int" => "int",
            "bool" => "bool",
            "vec2" => "Vector2",
            "vec3" => "Vector3",
            "vec4" when hints.Contains("source_color") => "Color",
            "vec4" => "Vector4",
            "ivec2" => "Vector2I",
            "ivec3" => "Vector3I",
            "ivec4" => "Vector4I",
            "mat2" => "Vector4", // 2x2 矩阵
            "mat3" => "Basis",
            "mat4" => "Transform3D",
            "sampler2D" => "Texture2D",
            "sampler2DArray" => "Texture2DArray",
            "sampler3D" => "Texture3D",
            "samplerCube" => "Cubemap",
            _ => "Variant"
        };
    }

    private static string MapDefaultValue(string glslType, string defaultValue, string hints) {
        if (string.IsNullOrEmpty(defaultValue)) {
            return "";
        }
        
        // 根据类型转换默认值
        string csharpType = MapGlslTypeToCSharp(glslType, hints);
        
        switch (csharpType) {
            case "float":
                return $" = {defaultValue}f";
            case "int":
                return $" = {defaultValue}";
            case "bool":
                return $" = {(defaultValue == "true" ? "true" : "false")}";
            case "Color":
                return ParseColorDefaultValue(defaultValue);
            case "Vector2":
                return ParseVec2DefaultValue(defaultValue);
            case "Vector3":
                return ParseVec3DefaultValue(defaultValue);
            case "Vector4":
                return ParseVec4DefaultValue(defaultValue);
            case "Vector2I":
                return ParseIVec2DefaultValue(defaultValue);
            default:
                return "";
        }
    }

    private static string ParseColorDefaultValue(string value) {
        // vec4(1.0, 0.0, 0.0, 1.0) -> new Color(1.0f, 0.0f, 0.0f, 1.0f)
        var match = Regex.Match(value, @"vec4\s*\(\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*\)");
        if (match.Success) {
            return $" = new Color({match.Groups[1].Value}f, {match.Groups[2].Value}f, {match.Groups[3].Value}f, {match.Groups[4].Value}f)";
        }
        return "";
    }

    private static string ParseVec2DefaultValue(string value) {
        var match = Regex.Match(value, @"vec2\s*\(\s*([0-9.]+)\s*,\s*([0-9.]+)\s*\)");
        if (match.Success) {
            return $" = new Vector2({match.Groups[1].Value}f, {match.Groups[2].Value}f)";
        }
        return "";
    }

    private static string ParseVec3DefaultValue(string value) {
        var match = Regex.Match(value, @"vec3\s*\(\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*\)");
        if (match.Success) {
            return $" = new Vector3({match.Groups[1].Value}f, {match.Groups[2].Value}f, {match.Groups[3].Value}f)";
        }
        return "";
    }

    private static string ParseVec4DefaultValue(string value) {
        var match = Regex.Match(value, @"vec4\s*\(\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*\)");
        if (match.Success) {
            return $" = new Vector4({match.Groups[1].Value}f, {match.Groups[2].Value}f, {match.Groups[3].Value}f, {match.Groups[4].Value}f)";
        }
        return "";
    }

    private static string ParseIVec2DefaultValue(string value) {
        var match = Regex.Match(value, @"ivec2\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)");
        if (match.Success) {
            return $" = new Vector2I({match.Groups[1].Value}, {match.Groups[2].Value})";
        }
        return "";
    }

    private static string GetOutputPath(string projectPath, ShaderFileInfo shaderFile) {
        // 生成输出路径，保持目录结构
        string relativePath = shaderFile.DirectoryPath;
        string className = GetClassName(shaderFile.FileName);
        
        string outputDir = Path.Combine(projectPath, "Script", "Generated", "ShaderWrappers");
        if (!string.IsNullOrEmpty(relativePath)) {
            outputDir = Path.Combine(outputDir, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }
        
        return Path.Combine(outputDir, $"{className}.g.cs");
    }

    private static string GetClassName(string fileName) {
        // 移除扩展名并转换为类名
        string baseName;
        if (fileName.EndsWith(".gdshaderinc")) {
            baseName = fileName.Replace(".gdshaderinc", "");
            return ToPascalCase(baseName) + "IncShader";
        } else if (fileName.EndsWith(".gdshader")) {
            baseName = fileName.Replace(".gdshader", "");
            return ToPascalCase(baseName) + "Shader";
        } else {
            baseName = fileName;
            return ToPascalCase(baseName) + "Shader";
        }
    }

    private static string ToPascalCase(string name) {
        if (string.IsNullOrEmpty(name)) {
            return name;
        }

        // 处理 snake_case 转换为 PascalCase
        var parts = name.Split('_');
        var result = new StringBuilder();
        
        foreach (var part in parts) {
            if (part.Length > 0) {
                result.Append(char.ToUpper(part[0]));
                if (part.Length > 1) {
                    result.Append(part.Substring(1));
                }
            }
        }
        
        return result.ToString();
    }

    private static string ToCamelCase(string name) {
        if (string.IsNullOrEmpty(name)) {
            return name;
        }

        // 处理 snake_case 转换为 camelCase
        var parts = name.Split('_');
        var result = new StringBuilder();
        
        for (int i = 0; i < parts.Length; i++) {
            var part = parts[i];
            if (part.Length > 0) {
                if (i == 0) {
                    // 第一个单词首字母小写
                    result.Append(char.ToLower(part[0]));
                } else {
                    // 后续单词首字母大写
                    result.Append(char.ToUpper(part[0]));
                }
                if (part.Length > 1) {
                    result.Append(part.Substring(1));
                }
            }
        }
        
        return result.ToString();
    }

    private class ShaderFileInfo {
        public string FileName { get; set; } = "";
        public string ResourcePath { get; set; } = "";
        public string FullPath { get; set; } = "";
        public string DirectoryPath { get; set; } = "";
        public ShaderFileType FileType { get; set; }
    }

    private class UniformInfo {
        public string Name { get; set; } = "";
        public string GlslType { get; set; } = "";
        public string CSharpType { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public string Hints { get; set; } = "";
        public bool IsGlobal { get; set; } = false;
    }

    private static void CleanAllGeneratedFiles(string projectPath) {
        string generatedDir = Path.Combine(projectPath, "Script", "Generated", "ShaderWrappers");
        if (Directory.Exists(generatedDir)) {
            try {
                Directory.Delete(generatedDir, true);
                log.Info($"已清理所有生成文件: {generatedDir}");
            }
            catch (Exception ex) {
                log.Error($"清理生成文件目录失败 {generatedDir}: {ex.Message}");
            }
        }
    }

    private enum ShaderFileType {
        Shader,    // .gdshader 文件
        Include    // .gdshaderinc 文件
    }
}

