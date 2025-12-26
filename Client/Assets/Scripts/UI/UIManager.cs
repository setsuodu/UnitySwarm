using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 逻辑根节点：DontDestroyOnLoad，只存脚本
    // 渲染根节点：随场景销毁，挂载 Canvas
    private RectTransform _uiRoot;
    private Dictionary<string, RectTransform> _layers = new Dictionary<string, RectTransform>();

    private Dictionary<Type, BasePanel> _panelDict = new Dictionary<Type, BasePanel>();
    private Dictionary<Type, Task> _loadingTasks = new Dictionary<Type, Task>();
    private Stack<BasePanel> _panelStack = new Stack<BasePanel>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 仅逻辑管理器不销毁
            EnsureUIRoot();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 核心修复：确保渲染层级正确嵌套
    /// </summary>
    public void EnsureUIRoot()
    {
        // 如果引用还在且物体没被销毁（切场景），就跳过
        if (_uiRoot != null) return;

        // 1. 创建 UI 实体根节点
        GameObject rootGo = new GameObject("UI_Root");

        // 【关键修复点】：必须先手动加 RectTransform，防止后面加 Canvas 时引用丢失
        _uiRoot = rootGo.AddComponent<RectTransform>();

        // 2. 挂载根 Canvas 组件
        Canvas rootCanvas = rootGo.AddComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // 3. 挂载适配器并设置参考分辨率
        CanvasScaler scaler = rootGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 4. 挂载射线拦截器
        rootGo.AddComponent<GraphicRaycaster>();

        // 5. 此时 _uiRoot 已经是 RectTransform 且稳定了，开始创建子层级
        _layers.Clear();
        CreateLayer("Normal", 0);
        CreateLayer("PopUp", 100);
        CreateLayer("Top", 200);

        // 6. 自动补全 EventSystem
        if (GameObject.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        Debug.Log("<color=green>UIManager: UI_Root 实体层级初始化成功！</color>");
    }

    private void CreateLayer(string name, int order)
    {
        // 创建层级物体，顺便加上必要组件
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        RectTransform rt = go.GetComponent<RectTransform>();

        // 这里的 _uiRoot 绝对不会是 null 了
        rt.SetParent(_uiRoot, false);

        // 锚点全屏拉伸
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // 设置子 Canvas 排序，实现层级覆盖
        Canvas c = go.GetComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = order;

        _layers.Add(name, rt);
    }

    #region 面板管理逻辑

    public async Task<T> GetOrLoadPanel<T>() where T : BasePanel
    {
        Type type = typeof(T);

        // 如果在当前场景已实例化且物体还在
        if (_panelDict.TryGetValue(type, out var panel) && panel != null)
            return (T)panel;

        if (_loadingTasks.TryGetValue(type, out var loadingTask))
            return await (Task<T>)loadingTask;

        Task<T> loadProcess = RealLoad<T>();
        _loadingTasks[type] = loadProcess;

        try { return await loadProcess; }
        finally { _loadingTasks.Remove(type); }
    }

    private async Task<T> RealLoad<T>() where T : BasePanel
    {
        EnsureUIRoot(); // 实例化前最后一次确认根节点

        Type type = typeof(T);
        string path = $"UI/{type.Name}";

        // 默认将面板生成在 Normal 层级下
        T panel = await ResourceManager.Instance.InstantiateAsync<T>(path, _layers["Normal"]);
        if (panel == null) return null;

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _panelDict[type] = panel;
        return panel;
    }

    public async Task<T> PushPanel<T>() where T : BasePanel
    {
        EnsureUIRoot();

        if (_panelStack.Count > 0) _panelStack.Peek().OnPause();

        T panel = await GetOrLoadPanel<T>();
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            _panelStack.Push(panel);
            panel.OnEnter();
        }
        return panel;
    }

    public void PopPanel(bool isDestroy = false)
    {
        if (_panelStack.Count <= 0) return;

        BasePanel topPanel = _panelStack.Pop();
        topPanel.OnExit();

        HandleRelease(topPanel, isDestroy);

        if (_panelStack.Count > 0) _panelStack.Peek().OnResume();
    }

    public void ClosePanel(BasePanel panel, bool isDestroy = false)
    {
        if (panel == null) return;

        if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
        {
            PopPanel(isDestroy);
            return;
        }

        if (_panelStack.Contains(panel))
        {
            List<BasePanel> list = new List<BasePanel>(_panelStack);
            list.Remove(panel);
            panel.OnExit();
            HandleRelease(panel, isDestroy);
            list.Reverse();
            _panelStack = new Stack<BasePanel>(list);
        }
    }

    private void HandleRelease(BasePanel panel, bool isDestroy)
    {
        if (isDestroy)
        {
            _panelDict.Remove(panel.GetType());
            ResourceManager.Instance.Release(panel.gameObject);
        }
        else
        {
            panel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 切场景时由外部调用，清空引用防止报错
    /// </summary>
    public void ClearOnLevelLoaded()
    {
        _panelStack.Clear();
        _panelDict.Clear();
        _uiRoot = null; // 标记根节点已毁，下次 Push 自动重建
    }

    #endregion
}