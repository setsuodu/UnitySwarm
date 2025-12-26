using System.Threading.Tasks;
using UnityEngine;

public class TTTGameClient : MonoBehaviour
{
    public static TTTGameClient Instance { get; private set; }

    private SimpleClient _net;
    private TaskCompletionSource<bool> _connectTcs;

    // 假设在登录或匹配成功后，服务端分配了玩家的标记
    public string MyMark { get; set; } = "X";
    private bool _isMyTurn = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        _net = GetComponent<SimpleClient>();
        _net.OnConnectedToServer += OnConnected;
        _net.Processor.SubscribeReusable<GameStatePacket>(OnReceiveState);
    }

    void OnConnected()
    {
        _connectTcs?.TrySetResult(true);
        //Debug.Log("连接服务器成功");
    }

    public void ConnectToServer(string ip)
    {
        _connectTcs = new TaskCompletionSource<bool>();
        _net.Connect(ip, 9050);
        Debug.Log($"ConnectToServer: {ip}:9050");
    }

    public Task WaitForConnected() => _connectTcs?.Task ?? Task.CompletedTask;

    private void SendMove(int index) => _net.SendToServer(new ClickPacket { Index = index });

    // 收到落子回调
    void OnReceiveState(GameStatePacket packet)
    {
        // 1. 根据服务器下发的回合信息，判断下一手是不是我 
        _isMyTurn = (packet.IsXTurn && MyMark == "X") || (!packet.IsXTurn && MyMark == "O");

        // 2. 如果游戏已经结束，强制禁止操作
        if (packet.Status != "Playing") _isMyTurn = false;

        // 3. 更新 UI 显示 
        UI_Game ui_game = UIManager.Instance.GetOrLoadPanel<UI_Game>().Result;
        ui_game.RefreshBoard(packet.Index, packet.Mark);

        // 可以在 UI 上提示“轮到你了”或“等待对方”
        Debug.Log($"[C] 当前回合: {(packet.IsXTurn ? "X" : "O")} | 我的标记: {MyMark}");
    }

    // UI 按钮调用此方法
    public void RequestMove(int index)
    {
        if (_isMyTurn)
        {
            SendMove(index);
        }
        else
        {
            Debug.Log("<color=red>还没轮到你！</color>");
        }
    }
}