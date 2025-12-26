using System.Threading.Tasks;
using UnityEngine;

public class TTTGameClient : MonoBehaviour
{
    public static TTTGameClient Instance { get; private set; }

    private SimpleClient _net;
    private TaskCompletionSource<bool> _connectTcs;

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

    //async void Start()
    //{
    //    await UIManager.Instance.PushPanel<UI_Login>();
    //}

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

    public void SendMove(int index) => _net.SendToServer(new ClickPacket { Index = index });

    // 收到落子回调
    void OnReceiveState(GameStatePacket packet)
    {
        // 收到消息后，通知 UI 层更新视图
        Debug.Log($"[C] index={packet.Index}, mark={packet.Mark}, status={packet.Status}");

        UI_Game ui_game = UIManager.Instance.GetOrLoadPanel<UI_Game>().Result;
        ui_game.RefreshBoard(packet.Index, packet.Mark);
    }
}