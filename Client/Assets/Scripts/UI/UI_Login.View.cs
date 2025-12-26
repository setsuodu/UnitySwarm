using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// 必须加 partial
public partial class UI_Login : BasePanel
{
    [Header("UI Components")]
    [SerializeField] private InputField inputAccount;
    [SerializeField] private InputField inputPassword;
    [SerializeField] private Button btnLogin;
    [SerializeField] private Text txtStatus;

    protected override void Awake()
    {
        base.Awake(); // 这一步会完成自动绑定

        // 现在可以直接使用 btn_Login，不需要 Find
        btnLogin.onClick.AddListener(() => {
            Debug.Log($"用户 {inputAccount.text} 尝试登录");
        });
    }

    // BasePanel 的重写
    public override void OnEnter()
    {
        base.OnEnter();
        InitView();
        // 绑定 UI 事件
        UIEvent.OnLoginSuccess += OnLoginSuccessCallback;
    }

    public override void OnExit()
    {
        base.OnExit();
        // 必须取消绑定！
        UIEvent.OnLoginSuccess -= OnLoginSuccessCallback;
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
        txtStatus.text = $"登录成功: {username}";
        // 可以在这里跳转到下一个面板
        await UIManager.Instance.PushPanel<UI_Game>();
    }
}