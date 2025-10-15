#if TOOLS
using Godot;
using System.Linq;

namespace ArcaneWorld.Addons.ResourceTreeGenerator;

[Tool]
public partial class ResourceTreeGenerator : EditorPlugin {

    public const string generateResourceTree = "生成资源树";

    public override void _EnterTree() {
        AddToolMenuItem(generateResourceTree, new Callable(this, nameof(GenerateResourceTree)));
    }

    public override void _ExitTree() {
        RemoveToolMenuItem(generateResourceTree);
    }

    public void GenerateResourceTree() {
        string projectPath = ProjectSettings.GlobalizePath("res://");
        GD.Print($"项目根目录: {projectPath}");

        // 扫描资源文件
        var resources = ScanResources(projectPath);

        // 生成资源树代码
        string generatedCode = GenerateResourceTreeCode(resources);

        // 保存到文件
        string outputPath = System.IO.Path.Combine(projectPath, "Script", "Generated", "ResourceTree.cs");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));
        System.IO.File.WriteAllText(outputPath, generatedCode);

        GD.Print($"资源树已生成: {outputPath}");
        GD.Print($"共扫描到 {resources.Count} 个资源文件");
    }

    private List<ResourceInfo> ScanResources(string projectPath) {
        var resources = new List<ResourceInfo>();
        var supportedExtensions = new HashSet<string> {
            ".tscn", ".cs", ".gd", ".csproj", ".godot", ".cfg",
            ".svg", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tga",
            ".ogg", ".wav", ".mp3", ".import", ".json", ".xml",
            ".txt", ".md", ".yml", ".yaml", ".toml", ".ini"
        };

        ScanDirectory(projectPath, "", resources, supportedExtensions);
        return resources;
    }

    private void ScanDirectory(string fullPath, string relativePath, List<ResourceInfo> resources, HashSet<string> supportedExtensions) {
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
                if (supportedExtensions.Contains(file.Extension.ToLower())) {
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
                ScanDirectory(subDir.FullName, newRelativePath, resources, supportedExtensions);
            }
        }
        catch (System.Exception ex) {
            GD.PrintErr($"扫描目录时出错 {fullPath}: {ex.Message}");
        }
    }

    private string GenerateResourceTreeCode(List<ResourceInfo> resources) {
        var code = new System.Text.StringBuilder();

        code.AppendLine("namespace ArcaneWorld.Generated;");
        code.AppendLine();
        code.AppendLine("public static class ResourceTree {");
        code.AppendLine();

        // 按目录层级生成嵌套结构
        GenerateNestedStructure(code, resources, "", 1);

        code.AppendLine("}");

        return code.ToString();
    }

    private void GenerateNestedStructure(System.Text.StringBuilder code, List<ResourceInfo> resources, string currentPath, int indentLevel) {
        string indent = new string(' ', indentLevel * 4);
        
        // 获取当前路径下的直接子目录和文件
        var currentLevelResources = resources.Where(r => r.DirectoryPath == currentPath).ToList();
        var subDirectories = resources
            .Where(r => r.DirectoryPath.StartsWith(currentPath) && r.DirectoryPath != currentPath)
            .Select(r => {
                string relativePath = r.DirectoryPath.Substring(currentPath.Length).TrimStart('/');
                return relativePath.Split('/')[0];
            })
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
            string subPath = string.IsNullOrEmpty(currentPath) ? subDir : $"{currentPath}/{subDir}";
            
            code.AppendLine();
            code.AppendLine($"{indent}public static class {className} {{");
            GenerateNestedStructure(code, resources, subPath, indentLevel + 1);
            code.AppendLine($"{indent}}}");
        }
    }

    private string ToClassName(string path) {
        if (string.IsNullOrEmpty(path))
            return "Root";

        // 将路径转换为类名
        var parts = path.Split('/', '\\');
        var className = string.Join(
            "",
            parts.Select(
                part =>
                    System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(part.ToLower())
                        .Replace("-", "")
                        .Replace("_", "")
            )
        );

        // 确保类名以字母开头
        if (char.IsDigit(className[0])) {
            className = "C" + className;
        }

        return className;
    }

    private string ToFieldName(string fileName) {
        // 只保留字母、数字和下划线
        var result = new System.Text.StringBuilder();
        foreach (char c in fileName) {
            if (char.IsLetterOrDigit(c)) {
                result.Append(c);
            } else {
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
    
    private bool IsCSharpKeyword(string name) {
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
        return keywords.Contains(name.ToLower());
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
