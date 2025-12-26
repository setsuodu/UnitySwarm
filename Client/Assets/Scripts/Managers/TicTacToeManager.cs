using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TicTacToeManager : MonoBehaviour
{
    public Button[] buttons; // 9 个按钮数组
    public bool isPlayerXTurn = true;
    public List<int> playerXMoves = new List<int>();
    public List<int> playerOMoves = new List<int>();
    public bool gameOver = false;

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    void OnButtonClick(int index)
    {
        if (gameOver || buttons[index].GetComponentInChildren<Text>().text != "") return;

        string mark = isPlayerXTurn ? "X" : "O";
        buttons[index].GetComponentInChildren<Text>().text = mark;

        if (isPlayerXTurn)
            playerXMoves.Add(index);
        else
            playerOMoves.Add(index);

        CheckWin();
        isPlayerXTurn = !isPlayerXTurn;
    }

    void CheckWin()
    {
        // 检查胜利条件：行、列、对角线
        int[][] winConditions = new int[][] {
            new int[] {0,1,2}, new int[] {3,4,5}, new int[] {6,7,8}, // 行
            new int[] {0,3,6}, new int[] {1,4,7}, new int[] {2,5,8}, // 列
            new int[] {0,4,8}, new int[] {2,4,6} // 对角线
        };

        foreach (var condition in winConditions)
        {
            if (CheckLine(condition, playerXMoves))
            {
                Debug.Log("Player X Wins!");
                gameOver = true;
                return;
            }
            if (CheckLine(condition, playerOMoves))
            {
                Debug.Log("Player O Wins!");
                gameOver = true;
                return;
            }
        }

        if (playerXMoves.Count + playerOMoves.Count == 9)
        {
            Debug.Log("Draw!");
            gameOver = true;
        }
    }

    bool CheckLine(int[] line, List<int> moves)
    {
        return moves.Contains(line[0]) && moves.Contains(line[1]) && moves.Contains(line[2]);
    }
}