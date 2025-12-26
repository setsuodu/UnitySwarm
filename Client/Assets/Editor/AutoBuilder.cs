using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using UnityEngine;

public class AutoBuilder
{
    [MenuItem("Framework/Build/One-Key Build & Sync COS")]
    public static void BuildAndSync()
    {
        // 1. 自动化配置 Group (调用你之前的 AddressableExporter 逻辑)
        AddressableExporter.FullAutoConfig();

        // 2. 强制设置 Profile 变量，确保 BuildPath 指向 ServerData
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var profileSettings = settings.profileSettings;
        string profileId = profileSettings.GetProfileId("Default");
        profileSettings.SetValue(profileId, "RemoteBuildPath", "ServerData/[BuildTarget]");

        // 3. 执行打包
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();

        // 4. 同步腾讯云 (调用命令行工具 coscli)
        SyncToTencentCOS();

        Debug.Log("<color=green>【框架】一键打包并上传腾讯云完成！</color>");
    }

    private static void SyncToTencentCOS()
    {
        string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
        string sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "ServerData", platform);

        // 注意：这里需要你预先下载 coscli 并在腾讯云配好存储桶
        // 参数说明：cp [本地目录] [存储桶路径] -r (递归)
        string cosCommand = $"/usr/local/bin/coscli cp {sourcePath} cos://your-bucket-name/hotfix/{platform} -r";

        // 执行命令行 (Windows 下需改用 cmd.exe)
        System.Diagnostics.Process.Start("/bin/bash", $"-c \"{cosCommand}\"");
    }
}