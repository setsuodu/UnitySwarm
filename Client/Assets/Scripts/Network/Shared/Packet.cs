using LiteNetLib.Utils;

public class ClickPacket { public int Index { get; set; } }

public class GameStatePacket
{
    public int Index { get; set; }  // 落子索引
    public string Mark { get; set; } // X 或 O
    public string Status { get; set; } // Playing, XWins, OWins, Draw
    public bool IsXTurn { get; set; } // [新增] 当前是否为 X 的回合
}