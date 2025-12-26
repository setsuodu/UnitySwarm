using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(SimpleClient))]
public class TTTGameClient : MonoBehaviour
{
    private SimpleClient _net;
    //private UI_Game _uiGame;

    async void Start()
    {
        _net = GetComponent<SimpleClient>();

        // 订阅服务器发来的状态包
        _net.Processor.SubscribeReusable<GameStatePacket>(OnReceiveState);

        // 通过 UIManager 加载并推入游戏界面
        await UIManager.Instance.PushPanel<UI_Login>();
    }

    public void ConnectToServer(string ip)
    {
        _net.Connect(ip, 9050);
    }

    public void SendMove(int index)
    {
        _net.SendToServer(new ClickPacket { Index = index });
    }

    private void OnReceiveState(GameStatePacket packet)
    {
        // 收到消息后，通知 UI 层更新视图
        //if (_uiGame != null)
        //{
        //    _uiGame.RefreshBoard(packet.Index, packet.Mark);
        //}
    }
}