using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class AddressableExporter
{
    // 配置资源存放路径与对应的 Group 名称
    private static Dictionary<string, string> PathToGroupMap = new Dictionary<string, string>()
    {
        { "Assets/Bundles/UI", "UI_Group" },
        { "Assets/Bundles/Config", "Config_Group" },
        { "Assets/Bundles/Common", "Common_Group" },
        { "Assets/Bundles/Hotfix", "Hotfix_Group" }
    };

    [MenuItem("Framework/Addressables/Full Auto Config")]
    public static void FullAutoConfig()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Settings 没找到，请先在 Window->Addressables->Groups 点击 Create 按钮");
            return;
        }

        // 1. 处理热更新代码 (拷贝并改名)
        PreProcessHotfixFiles();

        // 2. 遍历配置的路径进行自动分组
        foreach (var kvp in PathToGroupMap)
        {
            ProcessFolderToGroup(settings, kvp.Key, kvp.Value);
        }

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=green>【框架】Addressables 自动化配置完成！</color>");
    }

    private static void ProcessFolderToGroup(AddressableAssetSettings settings, string path, string groupName)
    {
        if (!Directory.Exists(path)) return;

        // 获取或创建 Group
        AddressableAssetGroup group = GetOrCreateGroup(settings, groupName);

        // 扫描该文件夹下所有资源（排除 .meta）
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (file.EndsWith(".meta") || file.EndsWith(".cs")) continue;

            string assetPath = file.Replace("\\", "/");
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            // 创建项
            var entry = settings.CreateOrMoveEntry(guid, group);

            // 设置 Address 为文件名 (不带路径和后缀)，方便 UIManager 加载
            entry.address = Path.GetFileNameWithoutExtension(assetPath);

            // 给特定 Group 打上 Label (可选)
            if (groupName == "UI_Group") entry.SetLabel("UI", true);
        }
    }

    private static void PreProcessHotfixFiles()
    {
        // 假设原始 DLL 在 Assets/Scripts/Hotfix~/ 目录下 (加~防止Unity编译)
        string sourcePath = "Assets/Scripts/Hotfix~/Hotfix.dll";
        string targetPath = "Assets/Bundles/Hotfix/Hotfix.dll.bytes";

        if (File.Exists(sourcePath))
        {
            string dir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.Copy(sourcePath, targetPath, true);
            Debug.Log($"已将热更代码拷贝至: {targetPath}");
        }
    }

    private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
    {
        AddressableAssetGroup group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);

            // 框架逻辑：针对热更组，默认开启 Remote 路径（这里可以根据需求写死或手动调）
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (groupName.Contains("Hotfix") || groupName.Contains("UI"))
            {
                // 注意：这里需要你先在 AA Setting 里配好 RemoteBuildPath 和 RemoteLoadPath 的变量名
                // schema.BuildPath.SetVariableByName(settings, "RemoteBuildPath");
                // schema.LoadPath.SetVariableByName(settings, "RemoteLoadPath");
            }
        }
        return group;
    }
}