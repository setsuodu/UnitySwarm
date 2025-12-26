using UnityEngine;
using UnityEngine.UI;

public partial class UI_Game
{
    void InitView()
    {
        Debug.Log("[UI] UI_Game.InitView");

        // 绑定棋盘点击
        for (int i = 0; i < btnChess.Length; i++)
        {
            int index = i;
            btnChess[i].onClick.RemoveAllListeners(); // 防止重复绑定
            btnChess[i].onClick.AddListener(() => HandleGridClick(index));
        }
    }

    void HandleGridClick(int index)
    {
        Debug.Log($"[UI] 点击了棋位: {index}");
        // 通过客户端发送数据
        TTTGameClient.Instance.RequestMove(index);
    }

    // 供 TTTGameClient 调用的更新方法
    public void RefreshBoard(int index, string mark)
    {
        if (index >= 0 && index < btnChess.Length)
        {
            btnChess[index].GetComponentInChildren<Text>().text = mark;
        }
    }
}