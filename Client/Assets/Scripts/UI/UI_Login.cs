using LiteNetLib;
using UnityEngine;

// 放逻辑（HandleLogin、事件处理）
// 负责纯业务逻辑（比如点击按钮后具体怎么连接服务器、发包等）
public partial class UI_Login // 同样必须加 partial
{
    void InitView()
    {
        Debug.Log("[UI] UI_Login.InitView");
        inputIP.text = NetUtils.GetLocalIp(LocalAddrType.IPv4);

        // 绑定按钮点击
        btnConnect.onClick.RemoveAllListeners();
        btnConnect.onClick.AddListener(HandleConnect);
    }

    async void HandleConnect()
    {
        string ip = inputIP.text; // 假设你加了个输入框输入IP
        //Debug.Log($"ip is null ? {string.IsNullOrEmpty(ip)}");

        // 1. 先连接服务器
        TTTGameClient.Instance.ConnectToServer(ip);

        // 2. 等待连接成功（可以用事件或Task完成源）
        await TTTGameClient.Instance.WaitForConnected(); // 你可以自己加这个方法
        
        Debug.Log("<color=green>连接服务器成功，可以进入游戏界面</color>");
        UIManager.Instance.PopPanel(); // 移除自己

        // 3. 连接成功后，再发登录包或直接进入游戏
        await UIManager.Instance.PushPanel<UI_Game>();
    }
}