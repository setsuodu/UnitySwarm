using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

public class SimpleClient : MonoBehaviour, INetEventListener
{
    private NetManager _netManager;
    private NetPeer _serverPeer;
    public NetPacketProcessor Processor { get; private set; } = new NetPacketProcessor();
    public Action OnConnectedToServer;

    public void Connect(string ip, int port)
    {
        _netManager = new NetManager(this) { AutoRecycle = true };
        _netManager.Start();
        _netManager.Connect(ip, port, "ExampleGame");
    }

    void Update() => _netManager?.PollEvents();

    public void SendToServer<T>(T packet, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : class, new()
    {
        if (_serverPeer == null) return;
        NetDataWriter writer = new NetDataWriter();
        Processor.Write(writer, packet);
        _serverPeer.Send(writer, method);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod delivery) => Processor.ReadAllPackets(reader);
    public void OnPeerConnected(NetPeer peer) { _serverPeer = peer; OnConnectedToServer?.Invoke(); }

    // 其他接口留空...
    public void OnPeerDisconnected(NetPeer p, DisconnectInfo d) { _serverPeer = null; }
    public void OnNetworkError(System.Net.IPEndPoint e, System.Net.Sockets.SocketError s) { }
    public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint e, NetPacketReader r, UnconnectedMessageType t) { }
    public void OnNetworkLatencyUpdate(NetPeer p, int l) { }
    public void OnConnectionRequest(ConnectionRequest r) { }
}