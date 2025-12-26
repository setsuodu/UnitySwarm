using UnityEngine;
using LiteNetLib;
using System.Linq;

[RequireComponent(typeof(SimpleServer))]
public class TTTGameServer : MonoBehaviour
{
    private SimpleServer _net;
    private string[] board = new string[9];
    private bool isPlayerXTurn = true;
    private bool gameOver = false;

    // 对应 的胜利条件
    private readonly int[][] winConditions = new int[][] {
        new int[] {0,1,2}, new int[] {3,4,5}, new int[] {6,7,8}, // 行
        new int[] {0,3,6}, new int[] {1,4,7}, new int[] {2,5,8}, // 列
        new int[] {0,4,8}, new int[] {2,4,6} // 对角线
    };

    void Start()
    {
        _net = GetComponent<SimpleServer>();
        _net.StartServer(9050);

        // 订阅点击消息
        _net.Processor.SubscribeReusable<ClickPacket, NetPeer>(HandleClick);
        ResetBoard();
    }

    private void ResetBoard()
    {
        for (int i = 0; i < 9; i++) board[i] = "";
        isPlayerXTurn = true;
        gameOver = false;
    }

    private void HandleClick(ClickPacket packet, NetPeer peer)
    {
        // 核心逻辑判断：如果游戏结束或该位置已有子，则忽略请求 
        if (gameOver || board[packet.Index] != "") return;

        // 1. 落子
        string mark = isPlayerXTurn ? "X" : "O";
        board[packet.Index] = mark;

        // 2. 胜负判定 (参考 TicTacToeManager.CheckWin 逻辑 )
        string status = CheckWinStatus();

        if (status != "Playing")
        {
            gameOver = true;
        }
        else
        {
            // 3. 只有在继续游戏时才切换回合 
            isPlayerXTurn = !isPlayerXTurn;
        }

        // 4. 将最新状态同步给所有玩家
        _net.SendToAll(new GameStatePacket
        {
            Index = packet.Index,
            Mark = mark,
            Status = status,
            IsXTurn = isPlayerXTurn // 新增：告诉客户端现在该谁下
        });
    }

    private string CheckWinStatus()
    {
        foreach (var condition in winConditions)
        {
            // 的逻辑变体：检查棋盘数组
            if (board[condition[0]] != "" &&
                board[condition[0]] == board[condition[1]] &&
                board[condition[0]] == board[condition[2]])
            {
                return board[condition[0]] == "X" ? "XWins" : "OWins";
            }
        }

        if (board.All(s => s != "")) return "Draw";
        return "Playing";
    }
}