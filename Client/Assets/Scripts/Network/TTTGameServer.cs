using UnityEngine;
using LiteNetLib;

[RequireComponent(typeof(SimpleServer))]
public class TTTGameServer : MonoBehaviour
{
	private SimpleServer _net;
	private string[] board = new string[9];
	private bool isPlayerXTurn = true;

	void Start()
	{
		_net = GetComponent<SimpleServer>();
		_net.StartServer(9050);
		Debug.Log("listen on 9050");

		// 订阅网络层的消息，并转回逻辑处理
		_net.Processor.SubscribeReusable<ClickPacket, NetPeer>(HandleClick);
		for (int i = 0; i < 9; i++) board[i] = "";
	}

	private void HandleClick(ClickPacket packet, NetPeer peer)
	{
		if (board[packet.Index] != "") return;

		string mark = isPlayerXTurn ? "X" : "O";
		board[packet.Index] = mark;

		// 游戏逻辑判断 (此处省略你之前的 CheckWin 细节)
		string status = "Playing";

		// 调用网络层进行广播
		_net.SendToAll(new GameStatePacket
		{
			Index = packet.Index,
			Mark = mark,
			Status = status
		});

		isPlayerXTurn = !isPlayerXTurn;
	}
}