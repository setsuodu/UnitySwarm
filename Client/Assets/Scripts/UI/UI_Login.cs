using UnityEngine;

public partial class UI_Login // 同样必须加 partial
{
    private void InitView()
    {
        // 绑定按钮点击
        btnLogin.onClick.AddListener(HandleLogin);
    }

    private void HandleLogin()
    {
        string account = inputAccount.text;
        string pwd = inputPassword.text;

        // 模拟发送 Packet（全栈开发这里直接调你的网络模块）
        Debug.Log($"发送登录请求: {account}");

        // 假设这里收到服务器回包
        bool isSuccess = (account == "admin");

        if (isSuccess)
        {
            // 通过 UIEvent 广播，不直接调 UIManager，解耦！
            UIEvent.OnLoginSuccess?.Invoke(account);
        }
        else
        {
            UIEvent.OnShowToast?.Invoke("账号或密码错误");
        }
    }
}