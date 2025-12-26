using LiteNetLib.Utils;

public class ClickPacket { public int Index { get; set; } }

public class GameStatePacket
{
    public int Index { get; set; }
    public string Mark { get; set; }
    public string Status { get; set; } // Playing, XWins, OWins, Draw
}