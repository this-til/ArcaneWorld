#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ArcaneWorld.addons.ArcaneWorldGenerator;

[Tool]
public partial class ArcaneWorldGenerator : EditorPlugin {

    public const string generateResourceTreeKey = "生成资源树";

    public const string generateShaderOperationPacksKey = "生成着色器操作包装";

    public const string deleteMetaFilesKey = "删除所有.meta文件";

    public override void _EnterTree() {
        AddToolMenuItem(generateResourceTreeKey, new Callable(this, nameof(generateResourceTree)));
        AddToolMenuItem(generateShaderOperationPacksKey, new Callable(this, nameof(generateShaderOperationPacks)));
        AddToolMenuItem(deleteMetaFilesKey, new Callable(this, nameof(deleteAllMetaFiles)));
    }

    public override void _ExitTree() {
        RemoveToolMenuItem(generateResourceTreeKey);
        RemoveToolMenuItem(generateShaderOperationPacksKey);
        RemoveToolMenuItem(deleteMetaFilesKey);
    }

    private void generateResourceTree() {
        GenerateResourceTree.generateResourceTree();
    }

    private void generateShaderOperationPacks() {
        GenerateShaderOperationPacks.generateShaderOperationPacks();
    }

    private void deleteAllMetaFiles() {
        string projectPath = ProjectSettings.GlobalizePath("res://");
        int deletedCount = DeleteMetaFilesRecursively(projectPath);
        
        GD.Print($"删除操作完成！共删除了 {deletedCount} 个 .meta 文件");
        
        // 刷新文件系统
        EditorInterface.Singleton.GetResourceFilesystem().ScanSources();
    }

    private int DeleteMetaFilesRecursively(string directory) {
        int deletedCount = 0;
        
        try {
            // 获取当前目录下的所有 .meta 文件
            var metaFiles = Directory.GetFiles(directory, "*.meta", SearchOption.TopDirectoryOnly);
            
            foreach (string metaFile in metaFiles) {
                try {
                    File.Delete(metaFile);
                    deletedCount++;
                    GD.Print($"已删除: {metaFile}");
                } catch (Exception ex) {
                    GD.PrintErr($"删除文件失败: {metaFile}, 错误: {ex.Message}");
                }
            }
            
            // 递归处理子目录
            var subDirectories = Directory.GetDirectories(directory);
            foreach (string subDirectory in subDirectories) {
                deletedCount += DeleteMetaFilesRecursively(subDirectory);
            }
        } catch (Exception ex) {
            GD.PrintErr($"处理目录失败: {directory}, 错误: {ex.Message}");
        }
        
        return deletedCount;
    }

}

#endif
