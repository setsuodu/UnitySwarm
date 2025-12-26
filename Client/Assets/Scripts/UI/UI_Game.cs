using UnityEngine;
using UnityEngine.UI;

public partial class UI_Game
{
    private TTTGameClient _client;

    private void InitView()
    {
        // 获取网络客户端引用（假设挂在同一个或全局物体上）
        _client = FindObjectOfType<TTTGameClient>();

        // 1. 绑定棋盘点击
        for (int i = 0; i < btnChess.Length; i++)
        {
            int index = i;
            btnChess[i].onClick.RemoveAllListeners(); // 防止重复绑定
            btnChess[i].onClick.AddListener(() => HandleGridClick(index));
        }

        // 2. 绑定连接按钮
        connectButton.onClick.RemoveAllListeners();
        connectButton.onClick.AddListener(HandleConnect);
    }

    private void HandleGridClick(int index)
    {
        Debug.Log($"点击了棋位: {index}");
        // 通过客户端发送数据
        _client.SendMove(index);
    }

    private void HandleConnect()
    {
        string ip = string.IsNullOrEmpty(ipInputField.text) ? "127.0.0.1" : ipInputField.text;
        _client.ConnectToServer(ip);

        connectButton.interactable = false; // UI 反馈
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