#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


[Tool]
public partial class ArcaneWorldGenerator : EditorPlugin {

    public const string generateResourceTreeKey = "生成资源树";

    public const string generateShaderOperationPacksKey = "生成着色器操作包装";

    public override void _EnterTree() {
        AddToolMenuItem(generateResourceTreeKey, new Callable(this, nameof(generateResourceTree)));
        AddToolMenuItem(generateShaderOperationPacksKey, new Callable(this, nameof(generateShaderOperationPacks)));
    }

    public override void _ExitTree() {
        RemoveToolMenuItem(generateResourceTreeKey);
        RemoveToolMenuItem(generateShaderOperationPacksKey);
    }

    private void generateShaderOperationPacks() {

    }

    public void generateResourceTree() {
        string projectPath = ProjectSettings.GlobalizePath("res://");
        GD.Print($"项目根目录: {projectPath}");

        // 扫描资源文件
        var resources = ScanResources(projectPath);

        // 生成资源树代码
        string generatedCode = GenerateResourceTreeCode(resources);

        // 保存到文件
        string outputPath = Path.Combine(projectPath, "Script", "Generated", "R.g.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, generatedCode);

        GD.Print($"资源树已生成: {outputPath}");
        GD.Print($"共扫描到 {resources.Count} 个资源文件");
    }

    private static List<ResourceInfo> ScanResources(string projectPath) {
        var resources = new List<ResourceInfo>();
        var excludedExtensions = new HashSet<string> {
            ".uid", ".import", ".csproj", ".sln", 
            ".dll", ".pdb", ".exe", ".so", ".dylib",
            ".cache", ".tmp", ".log", ".bak"
        };

        ScanDirectory(projectPath, "", resources, excludedExtensions);
        return resources;
    }

    private static void ScanDirectory(string fullPath, string relativePath, List<ResourceInfo> resources, HashSet<string> excludedExtensions) {
        try {
            var directory = new System.IO.DirectoryInfo(fullPath);
            if (!directory.Exists)
                return;

            // 跳过一些不需要的目录
            if (directory.Name.StartsWith(".") ||
                directory.Name == "bin" ||
                directory.Name == "obj" ||
                directory.Name == "node_modules") {
                return;
            }

            foreach (var file in directory.GetFiles()) {
                // 排除不需要的扩展名和没有扩展名的文件
                if (!string.IsNullOrEmpty(file.Extension) && !excludedExtensions.Contains(file.Extension.ToLower())) {
                    string resourcePath = string.IsNullOrEmpty(relativePath)
                        ? file.Name
                        : $"{relativePath}/{file.Name}";
                    resources.Add(
                        new ResourceInfo {
                            FileName = file.Name,
                            ResourcePath = resourcePath,
                            FullPath = file.FullName,
                            Extension = file.Extension,
                            DirectoryPath = relativePath
                        }
                    );
                }
            }

            foreach (var subDir in directory.GetDirectories()) {
                string newRelativePath = string.IsNullOrEmpty(relativePath)
                    ? subDir.Name
                    : $"{relativePath}/{subDir.Name}";
                ScanDirectory(subDir.FullName, newRelativePath, resources, excludedExtensions);
            }
        }
        catch (System.Exception ex) {
            GD.PrintErr($"扫描目录时出错 {fullPath}: {ex.Message}");
        }
    }

    private static string GenerateResourceTreeCode(List<ResourceInfo> resources) {
        var code = new System.Text.StringBuilder();

        code.AppendLine("namespace ArcaneWorld.Generated;");
        code.AppendLine();
        code.AppendLine("public static class R {");
        code.AppendLine();

        // 按目录层级生成嵌套结构
        GenerateNestedStructure(code, resources, "", 1);

        code.AppendLine("}");

        return code.ToString();
    }

    private static void GenerateNestedStructure(System.Text.StringBuilder code, List<ResourceInfo> resources, string currentPath, int indentLevel) {
        string indent = new string(' ', indentLevel * 4);

        // 获取当前路径下的直接子目录和文件
        var currentLevelResources = resources.Where(r => r.DirectoryPath == currentPath).ToList();
        var subDirectories = resources
            .Where(r => r.DirectoryPath.StartsWith(currentPath) && r.DirectoryPath != currentPath)
            .Select(
                r => {
                    string relativePath = r.DirectoryPath.Substring(currentPath.Length).TrimStart('/');
                    return relativePath.Split('/')[0];
                }
            )
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        // 生成当前目录下的文件
        foreach (var resource in currentLevelResources.OrderBy(r => r.FileName)) {
            string fieldName = ToFieldName(resource.FileName);
            string resourcePath = "res://" + resource.ResourcePath;
            code.AppendLine($"{indent}public const string {fieldName} = \"{resourcePath}\";");
        }

        // 生成子目录的嵌套类
        foreach (var subDir in subDirectories) {
            string className = ToClassName(subDir);
            string subPath = string.IsNullOrEmpty(currentPath)
                ? subDir
                : $"{currentPath}/{subDir}";

            code.AppendLine();
            code.AppendLine($"{indent}public static class {className} {{");
            GenerateNestedStructure(code, resources, subPath, indentLevel + 1);
            code.AppendLine($"{indent}}}");
        }
    }

    private static string ToClassName(string path) {
        if (string.IsNullOrEmpty(path))
            return "Root";

        // 复用 ToFieldName 的逻辑
        string className = ToFieldName(path);

        // 移除开头的 @ 或 _ 前缀
        className = className.TrimStart('@', '_');

        // 确保首字母大写
        if (!string.IsNullOrEmpty(className) && char.IsLower(className[0])) {
            className = char.ToUpper(className[0]) + className.Substring(1);
        }

        // 确保类名不为空且以字母开头
        if (string.IsNullOrEmpty(className) || char.IsDigit(className[0])) {
            className = "C" + className;
        }

        // 检查C#关键字冲突
        if (IsCSharpKeyword(className)) {
            className = "@" + className;
        }

        return className;
    }

    private static string ToFieldName(string fileName) {
        // 只保留字母、数字和下划线
        var result = new System.Text.StringBuilder();
        foreach (char c in fileName) {
            if (char.IsLetterOrDigit(c)) {
                result.Append(c);
            }
            else {
                result.Append('_');
            }
        }

        string fieldName = result.ToString();

        // 确保字段名以字母或下划线开头
        if (string.IsNullOrEmpty(fieldName) || char.IsDigit(fieldName[0])) {
            fieldName = "_" + fieldName;
        }

        // 检查C#关键字冲突
        if (IsCSharpKeyword(fieldName)) {
            fieldName = "@" + fieldName;
        }

        return fieldName;
    }

    private static bool IsCSharpKeyword(string name) {
        var keywords = new HashSet<string> {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while", "add", "alias", "ascending", "async", "await", "by", "descending",
            "dynamic", "equals", "from", "get", "global", "group", "into", "join", "let", "nameof",
            "on", "orderby", "partial", "remove", "select", "set", "value", "var", "when", "where", "yield"
        };
        return keywords.Contains(name);
    }

    private class ResourceInfo {

        public string FileName { get; set; } = "";

        public string ResourcePath { get; set; } = "";

        public string FullPath { get; set; } = "";

        public string Extension { get; set; } = "";

        public string DirectoryPath { get; set; } = "";

    }

}

#endif
