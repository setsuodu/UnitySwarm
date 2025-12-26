using UnityEngine;
using UnityEngine.UI;

// 放 UI 组件声明 + Awake/OnEnter 等生命周期
// 负责视图层（UI 组件声明、BasePanel 生命周期、事件订阅/取消订阅）
public partial class UI_Login : BasePanel // 必须加 partial
{
    [Header("UI Components")]
    [SerializeField] private InputField inputIP;
    [SerializeField] private Button btnConnect;

    protected override void Awake() => base.Awake(); // 这一步会完成自动绑定

    // 全局事件（UIEvent）放 OnEnter/OnExit
    public override void OnEnter()
    {
        base.OnEnter(); // 这一行绝对不能漏！
        InitView(); // 把按钮事件绑定移到逻辑文件去
        UIEvent.OnLoginSuccess += OnLoginSuccessCallback; // 绑定事件
    }

    public override void OnExit()
    {
        UIEvent.OnLoginSuccess -= OnLoginSuccessCallback; // 必须取消绑定！
        base.OnExit();
    }
}