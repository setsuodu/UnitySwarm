using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    // 缓存加载的原始资源句柄 (非实例化)
    private Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();
    // 缓存实例化的 GameObject 对应的句柄
    private Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new Dictionary<GameObject, AsyncOperationHandle<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 确保场景切换时不被销毁
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 防止重复创建（比如从其他场景跳回初始场景时）
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 异步加载原始资源 (Texture, AudioClip, TextAsset等)
    /// </summary>
    public async Task<T> LoadAssetAsync<T>(string path) where T : Object
    {
        if (_assetHandles.TryGetValue(path, out var handle))
            return handle.Result as T;

#if UNITY_EDITOR
        // 编辑器下模拟异步，保持逻辑一致性
        await Task.Yield();
        string fullPath = $"Assets/Bundles/{path}{GetExtension(typeof(T))}";
        T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
        return asset;
#else
        var h = Addressables.LoadAssetAsync<T>(path);
        _assetHandles[path] = h;
        return await h.Task;
#endif
    }

    /// <summary>
    /// 异步实例化 Prefab
    /// </summary>
    public async Task<T> InstantiateAsync<T>(string path, Transform parent = null) where T : Component
    {
        GameObject instance = null;
        System.Type type = typeof(T);

#if UNITY_EDITOR
        // 1. 编辑器模式加载
        await System.Threading.Tasks.Task.Yield();
        string fullPath = $"Assets/Bundles/{path}.prefab";
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);

        if (prefab == null)
        {
            Debug.LogError($"[ResourceManager] 找不到预制体: {fullPath}");
            return null;
        }
        instance = UnityEngine.Object.Instantiate(prefab, parent);
#else
    // 2. 真机模式加载 (Addressables)
    var h = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(path, parent);
    instance = await h.Task;
    _instanceHandles[instance] = h;
#endif

        // --- 以下是通用逻辑 (Shared Logic) ---

        if (instance != null)
        {
            // 统一修改名称，去掉 (Clone)
            instance.name = type.Name;

            // 统一动态挂载脚本，触发 BasePanel 的 Awake 和 UIBinder
            return instance.AddComponent<T>();
        }

        return null;
    }

    /// <summary>
    /// 自动识别并释放/销毁
    /// </summary>
    public void Release(Object target)
    {
        if (target == null) return;

        if (target is GameObject go)
        {
#if !UNITY_EDITOR
            if (_instanceHandles.TryGetValue(go, out var h))
            {
                Addressables.ReleaseInstance(h);
                _instanceHandles.Remove(go);
                return;
            }
#endif
            Destroy(go);
        }
    }

    private string GetExtension(System.Type t)
    {
        if (t == typeof(Texture2D)) return ".png";
        if (t == typeof(AudioClip)) return ".mp3";
        if (t == typeof(Material)) return ".mat";
        return ".asset";
    }
}