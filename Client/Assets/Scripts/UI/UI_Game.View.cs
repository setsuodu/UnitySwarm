using UnityEngine;
using UnityEngine.UI;

// 继承自 BasePanel，负责 UI 组件的引用和生命周期重写
public partial class UI_Game : BasePanel
{
    [Header("Board Components")]
    [SerializeField] private Button[] btnChess;

    [Header("Connect Components")]
    [SerializeField] private InputField ipInputField;
    [SerializeField] private Button connectButton;

    protected override void Awake() => base.Awake(); // 这一步会完成自动绑定

    // 重写 BasePanel 的进入逻辑
    public override void OnEnter()
    {
        base.OnEnter();
        InitView();
    }

    public override void OnExit()
    {
        base.OnExit();
        // 可以在这里做一些清理工作
    }
}