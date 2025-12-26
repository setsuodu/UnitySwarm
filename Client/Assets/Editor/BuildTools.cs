// 在 Assets/Editor/BuildTools.cs
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

public class BuildTools
{
    [MenuItem("Framework/Build/Build Addressables Only")]
    public static void BuildAddressables()
    {
        // 1. 先执行你写的全自动配置脚本，确保 Group 和 Address 都是对的
        AddressableExporter.FullAutoConfig();

        // 2. 执行 AA 官方的构建命令
        AddressableAssetSettings.BuildPlayerContent();

        UnityEngine.Debug.Log("<color=green>【框架】AA 资源打包完成！</color>");
    }
}