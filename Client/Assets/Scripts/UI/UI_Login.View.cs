using System.Threading.Tasks;
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

    // BasePanel 的重写
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

    // 事件系统触发的中间层
    private void OnLoginSuccessCallback(string username)
    {
        // 因为 UIEvent 定义是 Action，这里直接调用异步方法并不等待它结束
        // 但由于是在主线程执行 UI 操作，这在 Unity 中是安全的
        _ = HandleLoginSuccess(username);
    }

    private async Task HandleLoginSuccess(string username)
    {
        //txtStatus.text = $"登录成功: {username}";
        // 可以在这里跳转到下一个面板
        await UIManager.Instance.PushPanel<UI_Game>();
    }
}