using LiteNetLib.Utils;

public class ClickPacket { public int Index { get; set; } }

public class GameStatePacket
{
    public int Index { get; set; } // Chess Grid Index
    public string Mark { get; set; } // X or O
    public string Status { get; set; } // Playing, XWins, OWins, Draw
}