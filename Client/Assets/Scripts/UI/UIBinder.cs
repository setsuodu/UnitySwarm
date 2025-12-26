using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIBinder
{
    /// <summary>
    /// 自动绑定规则：查找物体名以组件缩写开头的物体
    /// 例如：Button变量名为 "btn_Login"，则匹配名为 "btn_Login" 的物体
    /// </summary>
    public static void Bind(MonoBehaviour target)
    {
        Type type = target.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            // --- 核心修改：增加数组判断 ---
            if (field.FieldType.IsArray)
            {
                BindArray(target, field);
                continue;
            }

            // 原有的单体绑定逻辑
            if (typeof(UIBehaviour).IsAssignableFrom(field.FieldType) || field.FieldType == typeof(GameObject))
            {
                Transform found = FindDeepChild(target.transform, field.Name);
                if (found != null)
                {
                    object value = field.FieldType == typeof(GameObject) ?
                                   found.gameObject : found.GetComponent(field.FieldType);
                    field.SetValue(target, value);
                }
            }
        }
    }

    private static void BindArray(MonoBehaviour target, FieldInfo field)
    {
        // 获取数组元素的类型 (如 Button[] 则元素类型为 Button)
        Type elementType = field.FieldType.GetElementType();
        bool isGameObject = elementType == typeof(GameObject);

        // 存储找到的组件/物体
        var list = new System.Collections.Generic.List<object>();
        int index = 0;

        // 按照 变量名_数字 的规则循环查找，直到找不到为止
        while (true)
        {
            string searchName = $"{field.Name}_{index}";
            Transform found = FindDeepChild(target.transform, searchName);

            if (found == null) break; // 找不到序号连贯的物体则停止

            if (isGameObject)
                list.Add(found.gameObject);
            else
                list.Add(found.GetComponent(elementType));

            index++;
        }

        if (list.Count > 0)
        {
            // 将 List 转换为对应的数组类型并赋值
            Array array = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++) array.SetValue(list[i], i);
            field.SetValue(target, array);
        }
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}