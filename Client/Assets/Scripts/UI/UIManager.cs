using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Hierarchy")]
    private Transform _uiRoot;
    private Dictionary<string, Transform> _layers = new Dictionary<string, Transform>();

    // 缓存已实例化的面板
    private Dictionary<Type, BasePanel> _panelDict = new Dictionary<Type, BasePanel>();
    // 缓存正在加载中的任务，防止重复触发加载
    private Dictionary<Type, Task> _loadingTasks = new Dictionary<Type, Task>();
    // UI 堆栈
    private Stack<BasePanel> _panelStack = new Stack<BasePanel>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitRoot();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化 UI 根节点与层级
    /// </summary>
    private void InitRoot()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject root = new GameObject("UI_Root");
            canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            _uiRoot = root.transform;
        }
        else
        {
            _uiRoot = canvas.transform;
        }

        // 检查 EventSystem
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // 创建基础层级（可选：控制不同面板的遮挡顺序）
        string[] layerNames = { "Normal", "PopUp", "Top" };
        foreach (var name in layerNames)
        {
            RectTransform layer = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            layer.SetParent(_uiRoot, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = layer.offsetMax = Vector2.zero;
            _layers.Add(name, layer);
        }
    }

    /// <summary>
    /// 核心方法：获取或加载面板（带任务缓存保护）
    /// </summary>
    public async Task<T> GetOrLoadPanel<T>() where T : BasePanel
    {
        Type type = typeof(T);

        // 1. 如果已经存在实例，直接返回
        if (_panelDict.TryGetValue(type, out var panel))
        {
            return (T)panel;
        }

        // 2. 如果该面板正在加载中，返回现有的加载 Task，避免重复实例化
        if (_loadingTasks.TryGetValue(type, out var loadingTask))
        {
            return await (Task<T>)loadingTask;
        }

        // 3. 开启加载流程并存入任务缓存
        Task<T> loadProcess = RealLoad<T>();
        _loadingTasks[type] = loadProcess;

        try
        {
            return await loadProcess;
        }
        finally
        {
            // 加载完成后，无论成功失败都移除任务缓存
            _loadingTasks.Remove(type);
        }
    }

    private async Task<T> RealLoad<T>() where T : BasePanel
    {
        Type type = typeof(T);
        string path = $"UI/{type.Name}";

        // 调用另一个单例 ResourceManager 执行具体的加载与实例化
        T panel = await ResourceManager.Instance.InstantiateAsync<T>(path, _layers["Normal"]);

        if (panel == null)
        {
            Debug.LogError($"[UIManager] 无法加载面板: {path}");
            return null;
        }

        // 重置 UI 变换
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _panelDict.Add(type, panel);
        return panel;
    }

    /// <summary>
    /// 打开/压入面板
    /// </summary>
    public async Task<T> PushPanel<T>() where T : BasePanel
    {
        // 暂停当前顶层面板
        if (_panelStack.Count > 0)
        {
            _panelStack.Peek().OnPause();
        }

        // 获取或加载新面板
        T panel = await GetOrLoadPanel<T>();

        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            _panelStack.Push(panel);
            panel.OnEnter();
        }

        return panel;
    }

    /// <summary>
    /// 弹出/关闭顶层面板
    /// </summary>
    /// <param name="isDestroy">是否彻底从内存卸载</param>
    public void PopPanel(bool isDestroy = false)
    {
        if (_panelStack.Count <= 0) return;

        BasePanel topPanel = _panelStack.Pop();
        topPanel.OnExit();

        if (isDestroy)
        {
            _panelDict.Remove(topPanel.GetType());
            ResourceManager.Instance.Release(topPanel.gameObject);
        }
        else
        {
            topPanel.gameObject.SetActive(false);
        }

        // 恢复上一个面板
        if (_panelStack.Count > 0)
        {
            _panelStack.Peek().OnResume();
        }
    }
}