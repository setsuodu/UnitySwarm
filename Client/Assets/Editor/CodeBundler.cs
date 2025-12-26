using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class CodeBundler : EditorWindow
{
    [MenuItem("Tools/Export Selected Scripts for AI")]
    public static void ExportSelectedCode()
    {
        StringBuilder sb = new StringBuilder();

        // 1. 这里填入你想要导出的【脚本文件名】
        string[] scriptNames = { "ResourceManager", "UIManager", "UI_Login" };

        foreach (var name in scriptNames)
        {
            // 关键修正：添加 "t:Script" 过滤，只找 C# 脚本
            string[] guids = AssetDatabase.FindAssets($"{name} t:Script");

            bool found = false;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // 确保文件名完全匹配（防止 UI_Login 匹配到 UI_LoginEditor）
                if (Path.GetFileNameWithoutExtension(path) == name && path.EndsWith(".cs"))
                {
                    string content = File.ReadAllText(path);
                    sb.AppendLine($"// File: {Path.GetFileName(path)}");
                    sb.AppendLine(content);
                    sb.AppendLine("\n// " + new string('-', 20) + "\n");
                    found = true;
                    break;
                }
            }

            if (!found) Debug.LogWarning($"未找到脚本: {name}.cs");
        }

        if (sb.Length > 0)
        {
            GUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log("✅ 脚本已成功拼接并复制到剪贴板！");
        }
    }
}