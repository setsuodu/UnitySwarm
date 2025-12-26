using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
    /// 注意：path 必须是 Addressables Groups 窗口中设置的 Address
    /// </summary>
    public async Task<T> LoadAssetAsync<T>(string path) where T : Object
    {
        if (_assetHandles.TryGetValue(path, out var handle))
            return handle.Result as T;

        // 统一接口：无论是编辑器还是 Client.exe，都通过 Addressables 加载
        var h = Addressables.LoadAssetAsync<T>(path);
        _assetHandles[path] = h;
        return await h.Task;
    }

    /// <summary>
    /// 异步实例化 Prefab
    /// </summary>
    public async Task<T> InstantiateAsync<T>(string path, Transform parent = null) where T : Component
    {
        GameObject instance = null;
        System.Type type = typeof(T);

        // Client.exe 运行代码时走这里
        // 必须确保 path 与 Addressables 窗口里的 Address 字符串一致
        var h = Addressables.InstantiateAsync(path, parent);
        instance = await h.Task;

        if (instance != null)
        {
            _instanceHandles[instance] = h;
            instance.name = type.Name;
            // 动态挂载代码逻辑
            T component = instance.AddComponent<T>();
            return component;
        }

        Debug.LogError("Addressables 加载失败，路径或地址错误: " + path);
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
            // 如果是 AA 管理的实例，使用 ReleaseInstance 销毁
            if (_instanceHandles.TryGetValue(go, out var h))
            {
                Addressables.ReleaseInstance(h);
                _instanceHandles.Remove(go);
                return;
            }
            // 否则执行普通销毁
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